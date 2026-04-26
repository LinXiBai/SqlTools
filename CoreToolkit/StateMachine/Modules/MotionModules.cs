using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Motion.Core;
using CoreToolkit.StateMachine.Core;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;
using CoreToolkit.StateMachine.Monitors;

namespace CoreToolkit.StateMachine.Modules
{
    /// <summary>
    /// 轴移动模块
    /// 执行单轴或多轴移动并支持到位检测
    /// </summary>
    public class AxisMoveModule : FlowModuleBase, ITrajectoryRecordable
    {
        private readonly IMotionCard _motionCard;
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
        
        // 到位检测配置
        /// <summary>
        /// 到位检测配置
        /// </summary>
        public InPositionConfig InPositionConfig { get; set; }
        
        /// <summary>
        /// 是否启用到位检测
        /// </summary>
        public bool EnableInPositionCheck { get; set; } = true;
        
        // 轨迹记录
        /// <summary>
        /// 轨迹记录
        /// </summary>
        public TrajectoryRecord TrajectoryRecord => _recorder?.Record;
        
        /// <summary>
        /// 是否启用轨迹记录
        /// </summary>
        public bool EnableTrajectoryRecording { get; set; } = true;
        
        /// <summary>
        /// 轨迹采样间隔（毫秒）
        /// </summary>
        public int TrajectorySampleIntervalMs { get; set; } = 10;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionCard">运动控制卡实例</param>
        /// <param name="name">模块名称</param>
        public AxisMoveModule(IMotionCard motionCard, string name = null) : base(name ?? "AxisMove")
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _recorder = new TrajectoryRecorder(motionCard);
            TimeoutMs = 60000; // 默认60秒超时
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (AxisIndices == null || AxisIndices.Length == 0)
                throw new InvalidOperationException("未设置轴索引");
            if (TargetPositions == null || TargetPositions.Length != AxisIndices.Length)
                throw new InvalidOperationException("目标位置数组长度与轴数量不匹配");

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

                    if (IsAbsolute)
                    {
                        _motionCard.SetVelocityProfile(axis, acc, dec);
                        _motionCard.MoveAbsolute(axis, pos, vel);
                    }
                    else
                    {
                        _motionCard.SetVelocityProfile(axis, acc, dec);
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
                                var status = _motionCard.GetAxisStatus(axis);
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

    /// <summary>
    /// 轴组移动模块
    /// 执行轴组插补运动
    /// </summary>
    public class AxisGroupMoveModule : FlowModuleBase, ITrajectoryRecordable
    {
        private readonly IAxisGroup _axisGroup;
        private readonly TrajectoryRecorder _recorder;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.AxisGroupMove;
        
        /// <summary>
        /// 轴组实例
        /// </summary>
        public IAxisGroup AxisGroup => _axisGroup;
        
        /// <summary>
        /// 目标位置数组
        /// </summary>
        public double[] TargetPositions { get; set; }
        
        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }
        
        /// <summary>
        /// 加速度
        /// </summary>
        public double Acceleration { get; set; }
        
        /// <summary>
        /// 减速度
        /// </summary>
        public double Deceleration { get; set; }
        
        /// <summary>
        /// 运动模式
        /// </summary>
        public MotionMode MotionMode { get; set; } = MotionMode.LinearInterpolation;
        
        /// <summary>
        /// 圆弧插补圆心
        /// </summary>
        public double[] CenterPoint { get; set; }
        
        /// <summary>
        /// 圆弧插补方向（1=顺时针, -1=逆时针）
        /// </summary>
        public int CircularDirection { get; set; } = 1;
        
        // PTP/PVT专用参数
        /// <summary>
        /// 速度百分比
        /// </summary>
        public double SpeedPercent { get; set; } = 100;
        
        /// <summary>
        /// PVT数据
        /// </summary>
        public PVTPoint[][] PVTData { get; set; }
        
        // 轨迹记录
        /// <summary>
        /// 轨迹记录
        /// </summary>
        public TrajectoryRecord TrajectoryRecord => _recorder?.Record;
        
        /// <summary>
        /// 是否启用轨迹记录
        /// </summary>
        public bool EnableTrajectoryRecording { get; set; } = true;
        
        /// <summary>
        /// 轨迹采样间隔（毫秒）
        /// </summary>
        public int TrajectorySampleIntervalMs { get; set; } = 10;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="axisGroup">轴组实例</param>
        /// <param name="name">模块名称</param>
        public AxisGroupMoveModule(IAxisGroup axisGroup, string name = null) : base(name ?? "AxisGroupMove")
        {
            _axisGroup = axisGroup ?? throw new ArgumentNullException(nameof(axisGroup));
            _recorder = new TrajectoryRecorder(axisGroup);
            TimeoutMs = 60000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (TargetPositions == null || TargetPositions.Length != _axisGroup.AxisCount)
                throw new InvalidOperationException("目标位置数组长度与轴组轴数量不匹配");

            // 启动轨迹记录
            if (EnableTrajectoryRecording)
            {
                _recorder.Start(_axisGroup.AxisIds, TrajectorySampleIntervalMs, Name);
            }

            try
            {
                // 根据运动模式执行相应运动
                switch (MotionMode)
                {
                    case MotionMode.LinearInterpolation:
                        _axisGroup.MoveLinearAbs(TargetPositions, Speed, Acceleration, Deceleration);
                        break;
                    case MotionMode.CircularInterpolation:
                        if (CenterPoint == null || CenterPoint.Length < 2)
                            throw new InvalidOperationException("圆弧插补需要设置圆心坐标");
                        _axisGroup.MoveCircularAbs(CenterPoint, TargetPositions, CircularDirection, Speed);
                        break;
                    case MotionMode.PTP:
                        _axisGroup.MovePTP(TargetPositions, SpeedPercent);
                        break;
                    case MotionMode.PVT:
                        if (PVTData == null)
                            throw new InvalidOperationException("PVT模式需要设置PVT数据");
                        _axisGroup.MovePVTSync(PVTData);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的运动模式: {MotionMode}");
                }

                // 等待完成
                SetWaitingState();
                var completed = await Task.Run(() =>
                    _axisGroup.WaitForComplete(TimeoutMs), cancellationToken);
                SetRunningState();

                if (!completed)
                {
                    Statistics.ErrorMessage = "轴组运动超时";
                    return false;
                }

                // 保存结果
                var finalStatus = _axisGroup.GetStatus();
                context.SetResult($"{ModuleId}_Positions", finalStatus.Positions);
                context.SetResult($"{ModuleId}_Velocities", finalStatus.Velocities);

                return true;
            }
            finally
            {
                _recorder?.Stop();
            }
        }

        /// <summary>
        /// 取消执行
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
            _axisGroup?.Stop(true);
            _recorder?.Stop();
        }
    }

    /// <summary>
    /// 延迟模块
    /// </summary>
    public class DelayModule : FlowModuleBase
    {
        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Delay;
        
        /// <summary>
        /// 延迟时间（毫秒）
        /// </summary>
        public int DelayMs { get; set; }
        
        /// <summary>
        /// 等待条件
        /// </summary>
        public Func<ExecutionContext, bool> Condition { get; set; }
        
        /// <summary>
        /// 条件检查间隔（毫秒）
        /// </summary>
        public int CheckIntervalMs { get; set; } = 10;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public DelayModule(string name = null) : base(name ?? "Delay")
        {
            TimeoutMs = int.MaxValue; // 延迟模块通常不设超时
        }

        /// <summary>
        /// 执行内部逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (Condition != null)
            {
                // 等待条件满足
                SetWaitingState();
                var sw = Stopwatch.StartNew();
                
                while (!Condition(context))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (sw.ElapsedMilliseconds > TimeoutMs)
                    {
                        Statistics.ErrorMessage = "等待条件超时";
                        OnRaiseTimeoutOccurred(new TimeoutEventArgs
                        {
                            ModuleId = ModuleId,
                            ModuleName = Name,
                            Timeout = TimeSpan.FromMilliseconds(TimeoutMs),
                            ActualDuration = sw.Elapsed,
                            Type = TimeoutType.ConditionTimeout,
                            Timestamp = DateTime.Now
                        });
                        return false;
                    }
                    
                    await Task.Delay(CheckIntervalMs, cancellationToken);
                }
                SetRunningState();
            }
            else if (DelayMs > 0)
            {
                // 固定延迟
                SetWaitingState();
                await Task.Delay(DelayMs, cancellationToken);
                SetRunningState();
            }

            return true;
        }
    }

    /// <summary>
    /// 自定义动作模块
    /// </summary>
    public class CustomActionModule : FlowModuleBase
    {
        private Func<ExecutionContext, CancellationToken, Task<bool>> _action;
        private Action<ExecutionContext> _syncAction;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public CustomActionModule(string name = null) : base(name ?? "Custom")
        {
        }

        /// <summary>
        /// 设置异步执行动作
        /// </summary>
        /// <param name="action">执行动作</param>
        public void SetAction(Func<ExecutionContext, CancellationToken, Task<bool>> action)
        {
            _action = action;
        }

        /// <summary>
        /// 设置同步执行动作
        /// </summary>
        /// <param name="action">执行动作</param>
        public void SetAction(Action<ExecutionContext> action)
        {
            _syncAction = action;
        }

        /// <summary>
        /// 执行内部逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                return await _action(context, cancellationToken);
            }
            else if (_syncAction != null)
            {
                await Task.Run(() => _syncAction(context), cancellationToken);
                return true;
            }
            else
            {
                throw new InvalidOperationException("未设置执行动作");
            }
        }
    }
}
