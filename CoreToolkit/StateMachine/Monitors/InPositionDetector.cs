using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Motion.Core;
using CoreToolkit.StateMachine.Models;
using CoreToolkit.Motion.Interfaces;

namespace CoreToolkit.StateMachine.Monitors
{
    /// <summary>
    /// 到位检测器
    /// 提供精确的轴到位检测功能
    /// </summary>
    public class InPositionDetector
    {
        private readonly IMotionCard _motionCard;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionCard">运动控制卡实例</param>
        public InPositionDetector(IMotionCard motionCard)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
        }

        /// <summary>
        /// 等待单个轴到位
        /// </summary>
        public async Task<bool> WaitForInPositionAsync(
            InPositionConfig config, 
            CancellationToken cancellationToken)
        {
            return await WaitForInPositionAsync(new[] { config }, cancellationToken);
        }

        /// <summary>
        /// 等待多个轴同时到位
        /// </summary>
        public async Task<bool> WaitForInPositionAsync(
            InPositionConfig[] configs, 
            CancellationToken cancellationToken)
        {
            if (configs == null || configs.Length == 0)
                return true;

            var axisStates = new Dictionary<int, AxisInPositionState>();
            var minTimeout = int.MaxValue;

            foreach (var config in configs)
            {
                axisStates[config.AxisIndex] = new AxisInPositionState
                {
                    Config = config,
                    StableCounter = 0
                };
                minTimeout = Math.Min(minTimeout, config.TimeoutMs);
            }

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < minTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool allInPosition = true;

                foreach (var state in axisStates.Values)
                {
                    var config = state.Config;
                    var status = _motionCard.GetAxisStatus(config.AxisIndex);
                    var cmdPos = _motionCard.GetPosition(config.AxisIndex);

                    // 检查运动是否完成
                    bool motionDone = _motionCard.IsInPosition(config.AxisIndex);
                    
                    // 检查伺服报警
                    if (status.IsAlarm)
                    {
                        throw new MotionException($"轴 {config.AxisIndex} 伺服报警");
                    }

                    // 检查到位条件
                    bool inPosition = CheckInPosition(state, cmdPos, motionDone);

                    if (!inPosition)
                    {
                        allInPosition = false;
                        state.StableCounter = 0;
                    }
                }

                if (allInPosition && axisStates.Count > 0)
                {
                    return true;
                }

                await Task.Delay(configs[0].CheckIntervalMs);
            }

            return false;
        }

        /// <summary>
        /// 检查单轴到位状态
        /// </summary>
        private bool CheckInPosition(AxisInPositionState state, double currentPosition, bool motionDone)
        {
            var config = state.Config;

            // 基本到位条件: 运动完成且位置稳定
            if (!motionDone)
                return false;

            if (state.LastPosition.HasValue)
            {
                var positionDelta = Math.Abs(currentPosition - state.LastPosition.Value);

                if (positionDelta <= config.Tolerance)
                {
                    state.StableCounter++;

                    if (state.StableCounter >= config.StableCount)
                    {
                        return true;
                    }
                }
                else
                {
                    state.StableCounter = 0;
                }
            }
            else
            {
                state.StableCounter++;
                if (state.StableCounter >= config.StableCount)
                {
                    return true;
                }
            }

            state.LastPosition = currentPosition;
            return false;
        }

        /// <summary>
        /// 带位置跟踪的到位检测
        /// 适用于需要跟踪目标位置的场合
        /// </summary>
        public async Task<bool> WaitForTargetPositionAsync(
            int axisIndex, 
            double targetPosition,
            double tolerance,
            int timeoutMs,
            CancellationToken cancellationToken,
            int checkIntervalMs = 10,
            int stableCount = 3)
        {
            var config = new InPositionConfig
            {
                AxisIndex = axisIndex,
                Tolerance = tolerance,
                TimeoutMs = timeoutMs,
                CheckIntervalMs = checkIntervalMs,
                StableCount = stableCount
            };

            return await WaitForInPositionAsync(config, cancellationToken);
        }

        /// <summary>
        /// 轴到位状态跟踪
        /// </summary>
        private class AxisInPositionState
        {
            public InPositionConfig Config { get; set; }
            public double? LastPosition { get; set; }
            public int StableCounter { get; set; }
        }
    }

    /// <summary>
    /// IO信号检测器
    /// </summary>
    public class IOSignalDetector
    {
        private readonly IIOCard _ioCard;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ioCard">IO卡实例</param>
        public IOSignalDetector(IIOCard ioCard)
        {
            _ioCard = ioCard ?? throw new ArgumentNullException(nameof(ioCard));
        }

        /// <summary>
        /// 等待IO信号
        /// </summary>
        /// <param name="config">信号配置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        public async Task<bool> WaitForSignalAsync(
            IOSignalConfig config,
            CancellationToken cancellationToken)
        {
            return await WaitForSignalsAsync(new[] { config }, true, cancellationToken);
        }

        /// <summary>
        /// 等待多个IO信号
        /// </summary>
        /// <param name="configs">信号配置</param>
        /// <param name="waitForAll">true=等待所有信号, false=等待任意信号</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        public async Task<bool> WaitForSignalsAsync(
            IOSignalConfig[] configs,
            bool waitForAll,
            CancellationToken cancellationToken)
        {
            if (configs == null || configs.Length == 0)
                return true;

            var sw = Stopwatch.StartNew();
            var minTimeout = int.MaxValue;
            var stableCounters = new Dictionary<int, int>();
            var lastStates = new Dictionary<int, bool>();

            foreach (var config in configs)
            {
                minTimeout = Math.Min(minTimeout, config.TimeoutMs);
                stableCounters[config.IoIndex] = 0;
                lastStates[config.IoIndex] = !config.ExpectedState;
            }

            while (sw.ElapsedMilliseconds < minTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (waitForAll)
                {
                    // 等待所有信号
                    bool allMatched = true;
                    foreach (var config in configs)
                    {
                        var currentState = config.IsInput 
                            ? _ioCard.ReadInput(config.IoIndex) 
                            : _ioCard.ReadOutput(config.IoIndex);

                        if (currentState != config.ExpectedState)
                        {
                            allMatched = false;
                            stableCounters[config.IoIndex] = 0;
                        }
                        else if (currentState != lastStates[config.IoIndex])
                        {
                            stableCounters[config.IoIndex] = 0;
                        }
                        else
                        {
                            stableCounters[config.IoIndex]++;
                        }

                        lastStates[config.IoIndex] = currentState;
                    }

                    if (allMatched)
                        return true;
                }
                else
                {
                    // 等待任意信号
                    foreach (var config in configs)
                    {
                        var currentState = config.IsInput 
                            ? _ioCard.ReadInput(config.IoIndex) 
                            : _ioCard.ReadOutput(config.IoIndex);

                        if (currentState == config.ExpectedState)
                        {
                            if (currentState == lastStates[config.IoIndex])
                            {
                                stableCounters[config.IoIndex]++;
                                if (stableCounters[config.IoIndex] >= 3) // 默认稳定3次
                                    return true;
                            }
                            else
                            {
                                stableCounters[config.IoIndex] = 1;
                            }
                        }
                        else
                        {
                            stableCounters[config.IoIndex] = 0;
                        }

                        lastStates[config.IoIndex] = currentState;
                    }
                }

                await Task.Delay(configs[0].CheckIntervalMs);
            }

            return false;
        }

        /// <summary>
        /// 等待输入信号变化(上升沿或下降沿)
        /// </summary>
        public async Task<bool> WaitForEdgeAsync(
            int ioIndex,
            bool risingEdge, // true=上升沿, false=下降沿
            int timeoutMs,
            CancellationToken cancellationToken,
            int checkIntervalMs = 5)
        {
            var sw = Stopwatch.StartNew();
            bool lastState = _ioCard.ReadInput(ioIndex);

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool currentState = _ioCard.ReadInput(ioIndex);

                // 检测边沿
                if (risingEdge && !lastState && currentState)
                    return true;
                if (!risingEdge && lastState && !currentState)
                    return true;

                lastState = currentState;
                await Task.Delay(checkIntervalMs);
            }

            return false;
        }

        /// <summary>
        /// 等待IO信号脉冲(高电平或低电平持续指定时间)
        /// </summary>
        public async Task<bool> WaitForPulseAsync(
            int ioIndex,
            bool highLevel, // true=高电平脉冲, false=低电平脉冲
            int pulseWidthMs,
            int timeoutMs,
            CancellationToken cancellationToken,
            int checkIntervalMs = 5)
        {
            var sw = Stopwatch.StartNew();
            var pulseSw = new Stopwatch();
            bool inPulse = false;

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool currentState = _ioCard.ReadInput(ioIndex);

                if (currentState == highLevel)
                {
                    if (!inPulse)
                    {
                        pulseSw.Restart();
                        inPulse = true;
                    }
                    else if (pulseSw.ElapsedMilliseconds >= pulseWidthMs)
                    {
                        return true;
                    }
                }
                else
                {
                    inPulse = false;
                    pulseSw.Reset();
                }

                await Task.Delay(checkIntervalMs);
            }

            return false;
        }
    }
}
