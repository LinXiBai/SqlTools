using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreToolkit.StateMachine.Models
{
    /// <summary>
    /// 状态机状态
    /// </summary>
    public enum StateMachineStatus
    {
        /// <summary>空闲/待机</summary>
        Idle,
        /// <summary>运行中</summary>
        Running,
        /// <summary>暂停</summary>
        Paused,
        /// <summary>完成</summary>
        Completed,
        /// <summary>错误/报警</summary>
        Error,
        /// <summary>停止中</summary>
        Stopping
    }

    /// <summary>
    /// 流程模块状态
    /// </summary>
    public enum ModuleStatus
    {
        /// <summary>等待执行</summary>
        Pending,
        /// <summary>执行中</summary>
        Running,
        /// <summary>等待条件</summary>
        Waiting,
        /// <summary>完成</summary>
        Completed,
        /// <summary>失败</summary>
        Failed,
        /// <summary>超时</summary>
        Timeout,
        /// <summary>已取消</summary>
        Cancelled
    }

    /// <summary>
    /// 流程模块类型
    /// </summary>
    public enum ModuleType
    {
        /// <summary>轴移动</summary>
        AxisMove,
        /// <summary>轴组移动</summary>
        AxisGroupMove,
        /// <summary>IO输出</summary>
        IOOutput,
        /// <summary>IO输入检测</summary>
        IOInput,
        /// <summary>到位检测</summary>
        InPositionCheck,
        /// <summary>等待/延迟</summary>
        Delay,
        /// <summary>自定义动作</summary>
        Custom,
        /// <summary>并行容器</summary>
        Parallel,
        /// <summary>串行容器</summary>
        Sequential
    }

    /// <summary>
    /// 状态机事件参数
    /// </summary>
    public class StateMachineEventArgs : EventArgs
    {
        /// <summary>状态机名称</summary>
        public string MachineName { get; set; }
        /// <summary>旧状态</summary>
        public StateMachineStatus OldStatus { get; set; }
        /// <summary>新状态</summary>
        public StateMachineStatus NewStatus { get; set; }
        /// <summary>事件时间</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>附加信息</summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 模块执行事件参数
    /// </summary>
    public class ModuleEventArgs : EventArgs
    {
        /// <summary>模块ID</summary>
        public string ModuleId { get; set; }
        /// <summary>模块名称</summary>
        public string ModuleName { get; set; }
        /// <summary>旧状态</summary>
        public ModuleStatus OldStatus { get; set; }
        /// <summary>新状态</summary>
        public ModuleStatus NewStatus { get; set; }
        /// <summary>执行耗时(毫秒)</summary>
        public double DurationMs { get; set; }
        /// <summary>事件时间</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }
        /// <summary>异常对象（完整异常信息，便于调试）</summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// 超时事件参数
    /// </summary>
    public class TimeoutEventArgs : EventArgs
    {
        /// <summary>模块ID</summary>
        public string ModuleId { get; set; }
        /// <summary>模块名称</summary>
        public string ModuleName { get; set; }
        /// <summary>设置的超时时间</summary>
        public TimeSpan Timeout { get; set; }
        /// <summary>实际执行时间</summary>
        public TimeSpan ActualDuration { get; set; }
        /// <summary>超时类型</summary>
        public TimeoutType Type { get; set; }
        /// <summary>事件时间</summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 超时类型
    /// </summary>
    public enum TimeoutType
    {
        /// <summary>执行超时</summary>
        ExecutionTimeout,
        /// <summary>到位信号超时</summary>
        InPositionTimeout,
        /// <summary>IO信号超时</summary>
        IOSignalTimeout,
        /// <summary>等待条件超时</summary>
        ConditionTimeout
    }

    /// <summary>
    /// 流程执行统计
    /// </summary>
    public class FlowStatistics
    {
        /// <summary>流程名称</summary>
        public string FlowName { get; set; }
        /// <summary>开始时间</summary>
        public DateTime StartTime { get; set; }
        /// <summary>结束时间</summary>
        public DateTime? EndTime { get; set; }
        /// <summary>总耗时</summary>
        public TimeSpan TotalDuration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
        /// <summary>模块统计列表</summary>
        public List<ModuleStatistics> ModuleStats { get; set; } = new List<ModuleStatistics>();
        /// <summary>是否成功完成</summary>
        public bool IsSuccess { get; set; }
        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }
        /// <summary>异常对象（完整异常信息）</summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// 模块执行统计
    /// </summary>
    public class ModuleStatistics
    {
        /// <summary>模块ID</summary>
        public string ModuleId { get; set; }
        /// <summary>模块名称</summary>
        public string ModuleName { get; set; }
        /// <summary>模块类型</summary>
        public ModuleType Type { get; set; }
        /// <summary>开始时间</summary>
        public DateTime StartTime { get; set; }
        /// <summary>结束时间</summary>
        public DateTime? EndTime { get; set; }
        /// <summary>执行耗时</summary>
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
        /// <summary>等待时间(从Pending到Running)</summary>
        public TimeSpan WaitTime { get; set; }
        /// <summary>是否超时</summary>
        public bool IsTimeout { get; set; }
        /// <summary>是否成功</summary>
        public bool IsSuccess { get; set; }
        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }
        /// <summary>异常对象（完整异常信息）</summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// 执行上下文
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>上下文ID</summary>
        public string ContextId { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>流程参数（线程安全）</summary>
        public ConcurrentDictionary<string, object> Parameters { get; set; } = new ConcurrentDictionary<string, object>();
        /// <summary>执行结果（线程安全）</summary>
        public ConcurrentDictionary<string, object> Results { get; set; } = new ConcurrentDictionary<string, object>();
        /// <summary>共享数据（线程安全）</summary>
        public ConcurrentDictionary<string, object> SharedData { get; set; } = new ConcurrentDictionary<string, object>();
        /// <summary>取消令牌</summary>
        public System.Threading.CancellationToken CancellationToken { get; set; }
        /// <summary>父上下文</summary>
        public ExecutionContext ParentContext { get; set; }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>参数值</returns>
        public T GetParameter<T>(string key, T defaultValue = default)
        {
            if (Parameters.TryGetValue(key, out var value) && value is T t)
                return t;
            return defaultValue;
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="key">结果键</param>
        /// <param name="value">结果值</param>
        public void SetResult<T>(string key, T value)
        {
            Results[key] = value;
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="key">结果键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果值</returns>
        public T GetResult<T>(string key, T defaultValue = default)
        {
            if (Results.TryGetValue(key, out var value) && value is T t)
                return t;
            return defaultValue;
        }
    }

    /// <summary>
    /// 到位检测配置
    /// </summary>
    public class InPositionConfig
    {
        /// <summary>轴索引</summary>
        public int AxisIndex { get; set; }
        /// <summary>到位容差(脉冲)</summary>
        public double Tolerance { get; set; } = 100;
        /// <summary>超时时间(毫秒)</summary>
        public int TimeoutMs { get; set; } = 10000;
        /// <summary>检测间隔(毫秒)</summary>
        public int CheckIntervalMs { get; set; } = 10;
        /// <summary>稳定次数(连续满足条件次数)</summary>
        public int StableCount { get; set; } = 3;
    }

    /// <summary>
    /// IO信号检测配置
    /// </summary>
    public class IOSignalConfig
    {
        /// <summary>IO编号</summary>
        public int IoIndex { get; set; }
        /// <summary>期望状态(true=on, false=off)</summary>
        public bool ExpectedState { get; set; } = true;
        /// <summary>IO类型(Input/Output)</summary>
        public bool IsInput { get; set; } = true;
        /// <summary>超时时间(毫秒)</summary>
        public int TimeoutMs { get; set; } = 5000;
        /// <summary>检测间隔(毫秒)</summary>
        public int CheckIntervalMs { get; set; } = 10;
    }

    /// <summary>
    /// 轨迹记录点
    /// </summary>
    public class TrajectoryPoint
    {
        /// <summary>时间戳</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>相对开始时间(毫秒)</summary>
        public double ElapsedMs { get; set; }
        /// <summary>各轴位置</summary>
        public double[] Positions { get; set; }
        /// <summary>各轴速度</summary>
        public double[] Velocities { get; set; }
        /// <summary>各轴状态</summary>
        public int[] Statuses { get; set; }
    }

    /// <summary>
    /// 轨迹记录
    /// </summary>
    public class TrajectoryRecord
    {
        /// <summary>记录ID</summary>
        public string RecordId { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>记录名称</summary>
        public string Name { get; set; }
        /// <summary>轴数量</summary>
        public int AxisCount { get; set; }
        /// <summary>轴名称</summary>
        public string[] AxisNames { get; set; }
        /// <summary>记录开始时间</summary>
        public DateTime StartTime { get; set; }
        /// <summary>轨迹点列表</summary>
        public List<TrajectoryPoint> Points { get; set; } = new List<TrajectoryPoint>();
        /// <summary>采样间隔(毫秒)</summary>
        public int SampleIntervalMs { get; set; } = 10;

        /// <summary>获取指定轴的位置数据</summary>
        public double[] GetAxisPositions(int axisIndex)
        {
            var result = new double[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                result[i] = Points[i].Positions?[axisIndex] ?? 0;
            }
            return result;
        }

        /// <summary>获取指定轴的速度数据</summary>
        public double[] GetAxisVelocities(int axisIndex)
        {
            var result = new double[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                result[i] = Points[i].Velocities?[axisIndex] ?? 0;
            }
            return result;
        }

        /// <summary>获取时间轴数据(毫秒)</summary>
        public double[] GetTimeAxis()
        {
            var result = new double[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                result[i] = Points[i].ElapsedMs;
            }
            return result;
        }
    }
}
