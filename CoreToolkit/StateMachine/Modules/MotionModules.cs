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

        public override ModuleType Type => ModuleType.AxisMove;
        public int[] AxisIndices { get; set; }
        public double[] TargetPositions { get; set; }
        public double[] Velocities { get; set; }
        public double[] Accelerations { get; set; }
        public double[] Decelerations { get; set; }
        public bool IsAbsolute { get; set; } = true;
        
        // 到位检测配置
        public InPositionConfig InPositionConfig { get; set; }
        public bool EnableInPositionCheck { get; set; } = true;
        
        // 轨迹记录
        public TrajectoryRecord TrajectoryRecord => _recorder?.Record;
        public bool EnableTrajectoryRecording { get; set; } = true;
        public int TrajectorySampleIntervalMs { get; set; } = 10;

        public AxisMoveModule(IMotionCard motionCard, string name = null) : base(name ?? "AxisMove")
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _recorder = new TrajectoryRecorder(motionCard);
            TimeoutMs = 60000; // 默认60秒超时
        }

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

        public override ModuleType Type => ModuleType.AxisGroupMove;
        public IAxisGroup AxisGroup => _axisGroup;
        public double[] TargetPositions { get; set; }
        public double Speed { get; set; }
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public MotionMode MotionMode { get; set; } = MotionMode.LinearInterpolation;
        public double[] CenterPoint { get; set; } // 圆弧插补圆心
        public int CircularDirection { get; set; } = 1; // 1=CW, -1=CCW
        
        // PTP/PVT专用参数
        public double SpeedPercent { get; set; } = 100;
        public PVTPoint[][] PVTData { get; set; }
        
        // 轨迹记录
        public TrajectoryRecord TrajectoryRecord => _recorder?.Record;
        public bool EnableTrajectoryRecording { get; set; } = true;
        public int TrajectorySampleIntervalMs { get; set; } = 10;

        public AxisGroupMoveModule(IAxisGroup axisGroup, string name = null) : base(name ?? "AxisGroupMove")
        {
            _axisGroup = axisGroup ?? throw new ArgumentNullException(nameof(axisGroup));
            _recorder = new TrajectoryRecorder(axisGroup);
            TimeoutMs = 60000;
        }

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

        public override void Cancel()
        {
            base.Cancel();
            _axisGroup?.Stop(true);
            _recorder?.Stop();
        }
    }

    /// <summary>
    /// 延迟/等待模块
    /// </summary>
    public class DelayModule : FlowModuleBase
    {
        public override ModuleType Type => ModuleType.Delay;
        public int DelayMs { get; set; }
        public Func<ExecutionContext, bool> Condition { get; set; }
        public int CheckIntervalMs { get; set; } = 10;

        public DelayModule(string name = null) : base(name ?? "Delay")
        {
            TimeoutMs = int.MaxValue; // 延迟模块通常不设超时
        }

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

        public override ModuleType Type => ModuleType.Custom;

        public CustomActionModule(string name = null) : base(name ?? "Custom")
        {
        }

        public void SetAction(Func<ExecutionContext, CancellationToken, Task<bool>> action)
        {
            _action = action;
        }

        public void SetAction(Action<ExecutionContext> action)
        {
            _syncAction = action;
        }

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
