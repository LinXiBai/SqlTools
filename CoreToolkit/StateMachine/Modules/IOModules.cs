using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Motion.Core;
using CoreToolkit.StateMachine.Core;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;
using CoreToolkit.StateMachine.Monitors;
using CoreToolkit.Motion.Interfaces;

namespace CoreToolkit.StateMachine.Modules
{
    /// <summary>
    /// IO输出模块
    /// </summary>
    public class IOOutputModule : FlowModuleBase
    {
        private readonly IIOCard _ioCard;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.IOOutput;
        
        /// <summary>
        /// IO索引
        /// </summary>
        public int IoIndex { get; set; }
        
        /// <summary>
        /// 输出状态
        /// </summary>
        public bool OutputState { get; set; }
        
        /// <summary>
        /// 设置后延迟时间（毫秒）
        /// </summary>
        public int DelayAfterSetMs { get; set; }
        
        /// <summary>
        /// 是否检查反馈
        /// </summary>
        public bool CheckFeedback { get; set; }
        
        /// <summary>
        /// 反馈IO索引
        /// </summary>
        public int FeedbackIoIndex { get; set; } = -1;
        
        /// <summary>
        /// 期望的反馈状态
        /// </summary>
        public bool ExpectedFeedbackState { get; set; }
        
        /// <summary>
        /// 反馈超时时间（毫秒）
        /// </summary>
        public int FeedbackTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ioCard">IO卡实例</param>
        /// <param name="name">模块名称</param>
        public IOOutputModule(IIOCard ioCard, string name = null) : base(name ?? "IOOutput")
        {
            _ioCard = ioCard ?? throw new ArgumentNullException(nameof(ioCard));
            TimeoutMs = 5000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            // 设置输出
            _ioCard.WriteOutput(IoIndex, OutputState);
            context.SetResult($"{ModuleId}_IO_{IoIndex}", OutputState);

            // 等待指定时间
            if (DelayAfterSetMs > 0)
            {
                await Task.Delay(DelayAfterSetMs, cancellationToken);
            }

            // 检查反馈
            if (CheckFeedback && FeedbackIoIndex >= 0)
            {
                SetWaitingState();
                var completed = await Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < FeedbackTimeoutMs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var actualState = _ioCard.ReadInput(FeedbackIoIndex);
                        if (actualState == ExpectedFeedbackState)
                            return true;

                        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                    }
                    return false;
                }, cancellationToken);

                SetRunningState();

                if (!completed)
                {
                    Statistics.ErrorMessage = $"IO反馈检测超时: IO{FeedbackIoIndex} 期望状态 {ExpectedFeedbackState}";
                    OnRaiseTimeoutOccurred(new TimeoutEventArgs
                    {
                        ModuleId = ModuleId,
                        ModuleName = Name,
                        Timeout = TimeSpan.FromMilliseconds(FeedbackTimeoutMs),
                        ActualDuration = TimeSpan.FromMilliseconds(FeedbackTimeoutMs),
                        Type = TimeoutType.IOSignalTimeout,
                        Timestamp = DateTime.Now
                    });
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// IO输入检测模块
    /// </summary>
    public class IOInputModule : FlowModuleBase
    {
        private readonly IIOCard _ioCard;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.IOInput;
        
        /// <summary>
        /// IO索引
        /// </summary>
        public int IoIndex { get; set; }
        
        /// <summary>
        /// 期望状态
        /// </summary>
        public bool ExpectedState { get; set; } = true;
        
        /// <summary>
        /// 检查间隔（毫秒）
        /// </summary>
        public int CheckIntervalMs { get; set; } = 10;
        
        /// <summary>
        /// 稳定时间（毫秒）
        /// </summary>
        public int StableTimeMs { get; set; } = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ioCard">IO卡实例</param>
        /// <param name="name">模块名称</param>
        public IOInputModule(IIOCard ioCard, string name = null) : base(name ?? "IOInput")
        {
            _ioCard = ioCard ?? throw new ArgumentNullException(nameof(ioCard));
            TimeoutMs = 10000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            SetWaitingState();
            
            var completed = await Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var stableSw = Stopwatch.StartNew();
                bool lastState = !_ioCard.ReadInput(IoIndex);

                while (sw.ElapsedMilliseconds < TimeoutMs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var currentState = _ioCard.ReadInput(IoIndex);
                    
                    if (currentState == ExpectedState)
                    {
                        if (StableTimeMs <= 0)
                            return true;

                        if (currentState != lastState)
                        {
                            stableSw.Restart();
                            lastState = currentState;
                        }
                        else if (stableSw.ElapsedMilliseconds >= StableTimeMs)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        stableSw.Restart();
                        lastState = currentState;
                    }

                    await Task.Delay(CheckIntervalMs, cancellationToken).ConfigureAwait(false);
                }
                return false;
            }, cancellationToken);

            SetRunningState();

            if (!completed)
            {
                var actualState = _ioCard.ReadInput(IoIndex);
                Statistics.ErrorMessage = $"IO输入检测超时: IO{IoIndex} 期望 {ExpectedState}, 实际 {actualState}";
                OnRaiseTimeoutOccurred(new TimeoutEventArgs
                {
                    ModuleId = ModuleId,
                    ModuleName = Name,
                    Timeout = TimeSpan.FromMilliseconds(TimeoutMs),
                    ActualDuration = TimeSpan.FromMilliseconds(TimeoutMs),
                    Type = TimeoutType.IOSignalTimeout,
                    Timestamp = DateTime.Now
                });
                return false;
            }

            context.SetResult($"{ModuleId}_IO_{IoIndex}", ExpectedState);
            return true;
        }
    }

    /// <summary>
    /// 到位检测模块
    /// 独立用于检测轴到位状态
    /// </summary>
    public class InPositionModule : FlowModuleBase
    {
        private readonly IMotionCard _motionCard;
        private readonly InPositionDetector _detector;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.InPositionCheck;
        
        /// <summary>
        /// 到位检测配置数组
        /// </summary>
        public InPositionConfig[] Configs { get; set; }
        
        /// <summary>
        /// 目标位置数组
        /// </summary>
        public double[] TargetPositions { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionCard">运动控制卡实例</param>
        /// <param name="name">模块名称</param>
        public InPositionModule(IMotionCard motionCard, string name = null) : base(name ?? "InPosition")
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _detector = new InPositionDetector(motionCard);
            TimeoutMs = 30000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (Configs == null || Configs.Length == 0)
            {
                // 使用默认配置
                if (TargetPositions == null || TargetPositions.Length == 0)
                    throw new InvalidOperationException("未设置到位检测配置或目标位置");

                Configs = new InPositionConfig[TargetPositions.Length];
                for (int i = 0; i < TargetPositions.Length; i++)
                {
                    Configs[i] = new InPositionConfig
                    {
                        AxisIndex = i,
                        TimeoutMs = TimeoutMs
                    };
                }
            }

            SetWaitingState();
            var completed = await _detector.WaitForInPositionAsync(Configs, cancellationToken);
            SetRunningState();

            if (!completed)
            {
                // 构建详细的错误信息
                var details = new List<string>();
                foreach (var config in Configs)
                {
                    var pos = _motionCard.GetPosition(config.AxisIndex);
                    var target = TargetPositions?[config.AxisIndex] ?? double.NaN;
                    var error = Math.Abs(pos - target);
                    details.Add($"轴{config.AxisIndex}: 目标={target:F2}, 实际={pos:F2}, 偏差={error:F2}");
                }

                Statistics.ErrorMessage = $"到位检测超时:\n{string.Join("\n", details)}";
                OnRaiseTimeoutOccurred(new TimeoutEventArgs
                {
                    ModuleId = ModuleId,
                    ModuleName = Name,
                    Timeout = TimeSpan.FromMilliseconds(TimeoutMs),
                    ActualDuration = TimeSpan.FromMilliseconds(this._executionTimer.ElapsedMilliseconds),
                    Type = TimeoutType.InPositionTimeout,
                    Timestamp = DateTime.Now
                });
                return false;
            }

            // 记录最终位置
            var finalPositions = new double[Configs.Length];
            for (int i = 0; i < Configs.Length; i++)
            {
                finalPositions[i] = _motionCard.GetPosition(Configs[i].AxisIndex);
            }
            context.SetResult($"{ModuleId}_Positions", finalPositions);

            return true;
        }
    }

    /// <summary>
    /// 复合IO操作模块
    /// 支持多路IO同时设置和检测
    /// </summary>
    public class CompositeIOModule : FlowModuleBase
    {
        private readonly IIOCard _ioCard;

        public override ModuleType Type => ModuleType.IOOutput;

        public class IOOperation
        {
            public int IoIndex { get; set; }
            public bool IsInput { get; set; }
            /// <summary>
            /// 设置值
            /// </summary>
            public bool SetValue { get; set; }
            
            /// <summary>
            /// 期望值
            /// </summary>
            public bool ExpectedValue { get; set; }
            
            /// <summary>
            /// 操作前延迟（毫秒）
            /// </summary>
            public int DelayBeforeMs { get; set; }
            
            /// <summary>
            /// 操作后延迟（毫秒）
            /// </summary>
            public int DelayAfterMs { get; set; }
        }

        /// <summary>
        /// IO操作列表
        /// </summary>
        public List<IOOperation> Operations { get; set; } = new List<IOOperation>();
        
        /// <summary>
        /// 是否并行执行
        /// </summary>
        public bool ParallelExecution { get; set; } = false;
        
        /// <summary>
        /// 是否等待所有输入信号
        /// </summary>
        public bool WaitForAllInputs { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ioCard">IO卡实例</param>
        /// <param name="name">模块名称</param>
        public CompositeIOModule(IIOCard ioCard, string name = null) : base(name ?? "CompositeIO")
        {
            _ioCard = ioCard ?? throw new ArgumentNullException(nameof(ioCard));
            TimeoutMs = 10000;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (Operations.Count == 0)
                return true;

            // 分离输入和输出操作
            var outputOps = new List<IOOperation>();
            var inputOps = new List<IOOperation>();

            foreach (var op in Operations)
            {
                if (op.IsInput)
                    inputOps.Add(op);
                else
                    outputOps.Add(op);
            }

            // 执行输出操作
            if (ParallelExecution)
            {
                // 并行执行
                var outputTasks = outputOps.Select(async op =>
                {
                    if (op.DelayBeforeMs > 0)
                        await Task.Delay(op.DelayBeforeMs, cancellationToken);
                    
                    _ioCard.WriteOutput(op.IoIndex, op.SetValue);
                    context.SetResult($"{ModuleId}_IO_{op.IoIndex}", op.SetValue);
                    
                    if (op.DelayAfterMs > 0)
                        await Task.Delay(op.DelayAfterMs, cancellationToken);
                });
                await Task.WhenAll(outputTasks);
            }
            else
            {
                // 串行执行
                foreach (var op in outputOps)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (op.DelayBeforeMs > 0)
                        await Task.Delay(op.DelayBeforeMs, cancellationToken);
                    
                    _ioCard.WriteOutput(op.IoIndex, op.SetValue);
                    context.SetResult($"{ModuleId}_IO_{op.IoIndex}", op.SetValue);
                    
                    if (op.DelayAfterMs > 0)
                        await Task.Delay(op.DelayAfterMs, cancellationToken);
                }
            }

            // 检测输入
            if (inputOps.Count > 0)
            {
                SetWaitingState();
                
                if (WaitForAllInputs)
                {
                    // 等待所有输入满足
                    var completed = await Task.Run(async () =>
                    {
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < TimeoutMs)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            bool allSatisfied = true;
                            foreach (var op in inputOps)
                            {
                                if (_ioCard.ReadInput(op.IoIndex) != op.ExpectedValue)
                                {
                                    allSatisfied = false;
                                    break;
                                }
                            }
                            
                            if (allSatisfied)
                                return true;
                            
                            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                        }
                        return false;
                    }, cancellationToken);

                    SetRunningState();

                    if (!completed)
                    {
                        var failedInputs = new List<string>();
                        foreach (var op in inputOps)
                        {
                            var actual = _ioCard.ReadInput(op.IoIndex);
                            if (actual != op.ExpectedValue)
                            {
                                failedInputs.Add($"IO{op.IoIndex}: 期望 {op.ExpectedValue}, 实际 {actual}");
                            }
                        }
                        Statistics.ErrorMessage = $"IO输入检测失败: {string.Join(", ", failedInputs)}";
                        return false;
                    }
                }
                else
                {
                    // 等待任意输入满足
                    var completed = await Task.Run(async () =>
                    {
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < TimeoutMs)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            foreach (var op in inputOps)
                            {
                                if (_ioCard.ReadInput(op.IoIndex) == op.ExpectedValue)
                                    return true;
                            }
                            
                            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                        }
                        return false;
                    }, cancellationToken);

                    SetRunningState();

                    if (!completed)
                    {
                        Statistics.ErrorMessage = "IO输入检测超时(任意模式)";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
