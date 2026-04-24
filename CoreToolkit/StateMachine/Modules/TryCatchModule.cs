using System;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.StateMachine.Core;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Modules
{
    /// <summary>
    /// Try-Catch 模块
    /// 尝试执行主模块，失败时执行备用模块，防止单个模块失败导致整个流程终止
    /// </summary>
    public class TryCatchModule : FlowModuleBase
    {
        private readonly IFlowModule _tryModule;
        private IFlowModule _catchModule;
        private bool _alwaysRunFinally = false;
        private IFlowModule _finallyModule;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;

        /// <summary>
        /// Try 模块
        /// </summary>
        public IFlowModule TryModule => _tryModule;

        /// <summary>
        /// Catch 模块（失败时执行，可选）
        /// </summary>
        public IFlowModule CatchModule => _catchModule;

        /// <summary>
        /// Finally 模块（无论成功失败都执行，可选）
        /// </summary>
        public IFlowModule FinallyModule => _finallyModule;

        /// <summary>
        /// Try 模块是否执行成功
        /// </summary>
        public bool TrySucceeded { get; private set; }

        /// <summary>
        /// 是否执行了 Catch 分支
        /// </summary>
        public bool CatchExecuted { get; private set; }

        /// <summary>
        /// 捕获的异常
        /// </summary>
        public Exception CaughtException { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tryModule">主模块</param>
        /// <param name="name">模块名称</param>
        public TryCatchModule(IFlowModule tryModule, string name = null) : base(name ?? $"TryCatch({tryModule?.Name})")
        {
            _tryModule = tryModule ?? throw new ArgumentNullException(nameof(tryModule));
            _tryModule.Parent = this;
        }

        /// <summary>
        /// 设置 Catch 分支（主模块失败时执行）
        /// </summary>
        public void SetCatchModule(IFlowModule catchModule)
        {
            _catchModule = catchModule;
            _catchModule.Parent = this;
        }

        /// <summary>
        /// 设置 Finally 分支（无论成败都执行）
        /// </summary>
        public void SetFinallyModule(IFlowModule finallyModule)
        {
            _finallyModule = finallyModule;
            _finallyModule.Parent = this;
        }

        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            TrySucceeded = false;
            CatchExecuted = false;
            CaughtException = null;

            // === Try ===
            ReportProgress(0.0, "Try", 0);
            _tryModule.Reset();
            bool trySuccess = await _tryModule.ExecuteAsync(context);

            if (trySuccess)
            {
                TrySucceeded = true;
                ReportProgress(0.5, "Try 成功", 1.0);
            }
            else
            {
                TrySucceeded = false;
                CaughtException = _tryModule.Statistics.Exception;
                Statistics.ErrorMessage = _tryModule.Statistics.ErrorMessage;
                Statistics.Exception = _tryModule.Statistics.Exception;
                ReportProgress(0.3, "Try 失败", 1.0, _tryModule.Statistics.ErrorMessage);

                // === Catch ===
                if (_catchModule != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReportProgress(0.4, "Catch", 0);
                    _catchModule.Reset();
                    CatchExecuted = true;
                    bool catchSuccess = await _catchModule.ExecuteAsync(context);

                    if (catchSuccess)
                    {
                        ReportProgress(0.7, "Catch 成功", 1.0);
                        // Catch 成功视为整体成功（可配置）
                        trySuccess = true;
                    }
                    else
                    {
                        ReportProgress(0.7, "Catch 失败", 1.0, _catchModule.Statistics.ErrorMessage);
                        Statistics.ErrorMessage = $"Catch 也失败了: {_catchModule.Statistics.ErrorMessage}";
                        Statistics.Exception = _catchModule.Statistics.Exception;
                    }
                }
            }

            // === Finally ===
            if (_finallyModule != null)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReportProgress(0.8, "Finally", 0);
                    _finallyModule.Reset();
                    await _finallyModule.ExecuteAsync(context);
                    ReportProgress(1.0, "Finally 完成", 1.0);
                }
                catch (Exception finallyEx)
                {
                    // Finally 异常不应掩盖主结果，但需记录
                    System.Diagnostics.Debug.WriteLine($"[TryCatchModule] Finally 异常: {finallyEx}");
                }
            }
            else
            {
                ReportProgress(1.0, trySuccess ? "完成" : "失败", 1.0);
            }

            return trySuccess;
        }

        public override void Cancel()
        {
            base.Cancel();
            _tryModule?.Cancel();
            _catchModule?.Cancel();
            _finallyModule?.Cancel();
        }

        public override void Reset()
        {
            base.Reset();
            TrySucceeded = false;
            CatchExecuted = false;
            CaughtException = null;
            _tryModule?.Reset();
            _catchModule?.Reset();
            _finallyModule?.Reset();
        }
    }
}
