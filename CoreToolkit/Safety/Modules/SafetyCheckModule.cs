using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Helpers;
using CoreToolkit.Safety.Models;
using CoreToolkit.StateMachine.Core;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.Safety.Modules
{
    /// <summary>
    /// 安全检查模块
    /// 在状态机流程中嵌入安全校验节点
    /// </summary>
    public class SafetyCheckModule : FlowModuleBase
    {
        private readonly ICollisionDetector _collisionDetector;
        private readonly InterlockEngine _interlockEngine;
        private readonly SafetyCheckMode _checkMode;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;

        /// <summary>
        /// 检查失败时是否触发急停（默认true）
        /// </summary>
        public bool TriggerEmergencyStopOnFailure { get; set; } = true;

        /// <summary>
        /// 检查失败时的错误消息
        /// </summary>
        public string FailureMessage { get; private set; }

        /// <summary>
        /// 构造函数（全量检测）
        /// </summary>
        public SafetyCheckModule(ICollisionDetector collisionDetector,
            InterlockEngine interlockEngine = null,
            string name = null) : base(name ?? "SafetyCheck")
        {
            _collisionDetector = collisionDetector;
            _interlockEngine = interlockEngine;
            _checkMode = SafetyCheckMode.FullCheck;
        }

        /// <summary>
        /// 构造函数（指定检测模式）
        /// </summary>
        public SafetyCheckModule(ICollisionDetector collisionDetector,
            InterlockEngine interlockEngine,
            SafetyCheckMode checkMode,
            string name = null) : base(name ?? "SafetyCheck")
        {
            _collisionDetector = collisionDetector;
            _interlockEngine = interlockEngine;
            _checkMode = checkMode;
        }

        /// <summary>
        /// 执行安全检查
        /// </summary>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            // 1. 碰撞检测
            if ((_checkMode == SafetyCheckMode.FullCheck || _checkMode == SafetyCheckMode.CollisionOnly)
                && _collisionDetector != null)
            {
                var collisionResult = _collisionDetector.CheckCollision();
                if (!collisionResult.IsSafe)
                {
                    FailureMessage = $"[碰撞检测] {collisionResult.Message}";
                    Statistics.ErrorMessage = FailureMessage;
                    context.SharedData["CollisionResult"] = collisionResult;

                    if (TriggerEmergencyStopOnFailure)
                    {
                        context.SharedData["TriggerEmergencyStop"] = true;
                    }

                    return false;
                }
            }

            // 2. 互锁检测
            if ((_checkMode == SafetyCheckMode.FullCheck || _checkMode == SafetyCheckMode.InterlockOnly)
                && _interlockEngine != null)
            {
                var interlockResult = _interlockEngine.EvaluateAll();
                if (!interlockResult.IsSafe)
                {
                    FailureMessage = $"[互锁检测] {interlockResult.BlockReason}";
                    Statistics.ErrorMessage = FailureMessage;
                    context.SharedData["InterlockResult"] = interlockResult;

                    if (TriggerEmergencyStopOnFailure &&
                        interlockResult.RecommendedAction == InterlockAction.EmergencyStop)
                    {
                        context.SharedData["TriggerEmergencyStop"] = true;
                    }

                    return false;
                }
            }

            // 安全检查通过
            return true;
        }
    }

    /// <summary>
    /// 安全检查模式
    /// </summary>
    public enum SafetyCheckMode
    {
        /// <summary>
        /// 完整检测（碰撞+互锁）
        /// </summary>
        FullCheck,

        /// <summary>
        /// 仅碰撞检测
        /// </summary>
        CollisionOnly,

        /// <summary>
        /// 仅互锁检测
        /// </summary>
        InterlockOnly
    }
}
