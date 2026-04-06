using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Core
{
    /// <summary>
    /// 流程模块接口
    /// 定义流程执行的基本契约
    /// </summary>
    public interface IFlowModule
    {
        /// <summary>模块唯一ID</summary>
        string ModuleId { get; }
        /// <summary>模块名称</summary>
        string Name { get; set; }
        /// <summary>模块类型</summary>
        ModuleType Type { get; }
        /// <summary>当前状态</summary>
        ModuleStatus Status { get; }
        /// <summary>执行超时时间</summary>
        int TimeoutMs { get; set; }
        /// <summary>是否并行执行</summary>
        bool IsParallel { get; set; }
        /// <summary>父模块</summary>
        IFlowModule Parent { get; set; }
        /// <summary>执行统计</summary>
        ModuleStatistics Statistics { get; }

        /// <summary>状态变更事件</summary>
        event System.EventHandler<ModuleEventArgs> OnStatusChanged;
        /// <summary>执行进度事件</summary>
        event System.EventHandler<double> OnProgressChanged;

        /// <summary>
        /// 执行模块
        /// </summary>
        Task<bool> ExecuteAsync(ExecutionContext context);

        /// <summary>
        /// 取消执行
        /// </summary>
        void Cancel();

        /// <summary>
        /// 重置模块状态
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 可暂停模块接口
    /// </summary>
    public interface IPausableModule
    {
        /// <summary>暂停执行</summary>
        void Pause();
        /// <summary>恢复执行</summary>
        void Resume();
        /// <summary>是否已暂停</summary>
        bool IsPaused { get; }
    }

    /// <summary>
    /// 条件模块接口
    /// </summary>
    public interface IConditionModule
    {
        /// <summary>设置条件检查函数</summary>
        void SetCondition(System.Func<ExecutionContext, bool> condition);
        /// <summary>等待条件满足</summary>
        Task<bool> WaitForConditionAsync(ExecutionContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 轨迹记录模块接口
    /// </summary>
    public interface ITrajectoryRecordable
    {
        /// <summary>轨迹记录</summary>
        TrajectoryRecord TrajectoryRecord { get; }
        /// <summary>是否记录轨迹</summary>
        bool EnableTrajectoryRecording { get; set; }
        /// <summary>轨迹采样间隔(毫秒)</summary>
        int TrajectorySampleIntervalMs { get; set; }
    }
}
