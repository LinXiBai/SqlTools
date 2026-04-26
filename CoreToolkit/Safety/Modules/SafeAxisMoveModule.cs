using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Helpers;
using CoreToolkit.Safety.Models;
using CoreToolkit.StateMachine.Core;
using CoreToolkit.StateMachine.Monitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.Safety.Modules
{
    /// <summary>
    /// 带安全防护的轴移动模块
    /// 在标准AxisMoveModule基础上增加了运动前安全检查
    /// </summary>
    public class SafeAxisMoveModule : FlowModuleBase
    {
        private readonly IMotionCard _motionCard;
        private readonly SafeMotionController _safeController;
        private readonly TrajectoryRecorder _recorder;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.AxisMove;

        /// <summary>
        /// 轴索引数组
        /// </summary>
        public int[] AxisIndices { get; set; }

        /// <summary>
        /// 目标位置数组
        /// </summary>
        public double[] TargetPositions { get; set; }

        /// <summary>
        /// 速度数组
        /// </summary>
        public double[] Velocities { get; set; }

        /// <summary>
        /// 加速度数组
        /// </summary>
        public double[] Accelerations { get; set; }

        /// <summary>
        /// 减速度数组
        /// </summary>
        public double[] Decelerations { get; set; }

        /// <summary>
        /// 是否为绝对位置
        /// </summary>
        public bool IsAbsolute { get; set; } = true;

        /// <summary>
        /// 是否启用到位检测
        /// </summary>
        public bool EnableInPositionCheck { get; set; } = true;

        /// <summary>
        /// 到位检测配置
        /// </summary>
        public InPositionConfig InPositionConfig { get; set; }

        /// <summary>
        /// 是否启用轨迹记录
        /// </summary>
        public bool EnableTrajectoryRecording { get; set; } = true;

        /// <summary>
        /// 轨迹采样间隔（毫秒）
        /// </summary>
        public int TrajectorySampleIntervalMs { get; set; } = 10;

        /// <summary>
        /// 安全控制器（外部传入时使用）
        /// </summary>
        public SafeMotionController SafeController => _safeController;

        /// <summary>
        /// 是否启用安全检查
        /// </summary>
        public bool EnableSafetyCheck { get; set; } = true;

        /// <summary>
        /// 构造函数（使用外部SafeMotionController）
        /// </summary>
        public SafeAxisMoveModule(SafeMotionController safeController, string name = null) : base(name ?? "SafeAxisMove")
        {
            _safeController = safeController ?? throw new ArgumentNullException(nameof(safeController));
            _motionCard = safeController.MotionCard;
            _recorder = new TrajectoryRecorder(_motionCard);
            TimeoutMs = 60000;
        }

        /// <summary>
        /// 构造函数（独立创建SafeMotionController）
        /// </summary>
        public SafeAxisMoveModule(IMotionCard motionCard,
            ICollisionDetector collisionDetector = null,
            SoftLimitGuard softLimitGuard = null,
            InterlockEngine interlockEngine = null,
            string name = null) : base(name ?? "SafeAxisMove")
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _safeController = new SafeMotionController(motionCard, collisionDetector, softLimitGuard, interlockEngine);
            _recorder = new TrajectoryRecorder(motionCard);
            TimeoutMs = 60000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (AxisIndices == null || AxisIndices.Length == 0)
                throw new InvalidOperationException("未设置轴索引");
            if (TargetPositions == null || TargetPositions.Length != AxisIndices.Length)
                throw new InvalidOperationException("目标位置数组长度与轴数量不匹配");

            // === 运动前安全检查 ===
            if (EnableSafetyCheck)
            {
                var safetyResult = _safeController.PreMoveCheck(AxisIndices, TargetPositions);
                if (!safetyResult.IsAllowed)
                {
                    Statistics.ErrorMessage = safetyResult.BlockReason;
                    return false;
                }
            }

            // 启动轨迹记录
            if (EnableTrajectoryRecording)
            {
                _recorder.Start(AxisIndices, TrajectorySampleIntervalMs, Name);
            }

            try
            {
                // 执行移动
                for (int i = 0; i < AxisIndices.Length; i++)
                {
                    var axis = AxisIndices[i];
                    var pos = TargetPositions[i];
                    var vel = Velocities?[i] ?? 10000;
                    var acc = Accelerations?[i] ?? 100000;
                    var dec = Decelerations?[i] ?? 100000;

                    _motionCard.SetVelocityProfile(axis, acc, dec);

                    if (IsAbsolute)
                    {
                        _motionCard.MoveAbsolute(axis, pos, vel);
                    }
                    else
                    {
                        _motionCard.MoveRelative(axis, pos, vel);
                    }
                }

                // 等待移动完成并检测到位
                if (EnableInPositionCheck)
                {
                    SetWaitingState();

                    var inPositionDetector = new InPositionDetector(_motionCard);
                    var configs = new List<InPositionConfig>();

                    for (int i = 0; i < AxisIndices.Length; i++)
                    {
                        var config = InPositionConfig ?? new InPositionConfig
                        {
                            AxisIndex = AxisIndices[i],
                            TimeoutMs = TimeoutMs
                        };
                        config.AxisIndex = AxisIndices[i];
                        configs.Add(config);
                    }

                    var completed = await inPositionDetector.WaitForInPositionAsync(
                        configs.ToArray(), cancellationToken);

                    SetRunningState();

                    if (!completed)
                    {
                        Statistics.ErrorMessage = "到位检测超时或失败";
                        OnRaiseTimeoutOccurred(new TimeoutEventArgs
                        {
                            ModuleId = ModuleId,
                            ModuleName = Name,
                            Timeout = TimeSpan.FromMilliseconds(TimeoutMs),
                            ActualDuration = TimeSpan.FromMilliseconds(_executionTimer.ElapsedMilliseconds),
                            Type = TimeoutType.InPositionTimeout,
                            Timestamp = DateTime.Now
                        });
                        return false;
                    }
                }
                else
                {
                    // 简单等待运动完成
                    var completed = await Task.Run(async () =>
                    {
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < TimeoutMs)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            bool allDone = true;
                            foreach (var axis in AxisIndices)
                            {
                                if (!_motionCard.IsInPosition(axis))
                                {
                                    allDone = false;
                                    break;
                                }
                            }

                            if (allDone) return true;
                            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                        }
                        return false;
                    }, cancellationToken);

                    if (!completed)
                    {
                        Statistics.ErrorMessage = "运动完成等待超时";
                        return false;
                    }
                }

                // 保存结果
                context.SetResult($"{ModuleId}_Positions", GetCurrentPositions());

                return true;
            }
            finally
            {
                _recorder?.Stop();
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            // 急停所有轴
            if (_motionCard != null && AxisIndices != null)
            {
                foreach (var axis in AxisIndices)
                {
                    try { _motionCard.Stop(axis, true); } catch { }
                }
            }
            _recorder?.Stop();
        }

        private double[] GetCurrentPositions()
        {
            var positions = new double[AxisIndices.Length];
            for (int i = 0; i < AxisIndices.Length; i++)
            {
                positions[i] = _motionCard.GetPosition(AxisIndices[i]);
            }
            return positions;
        }
    }
}
