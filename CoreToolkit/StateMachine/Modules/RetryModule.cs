using System;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.StateMachine.Core;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Modules
{
    /// <summary>
    /// 重试模块
    /// 包装任意模块，在失败时自动重试，支持退避延迟
    /// </summary>
    public class RetryModule : FlowModuleBase
    {
        private readonly IFlowModule _innerModule;
        private int _maxRetries = 3;
        private int _retryDelayMs = 500;
        private bool _useExponentialBackoff = false;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;

        /// <summary>
        /// 最大重试次数（默认3次）
        /// </summary>
        public int MaxRetries
        {
            get => _maxRetries;
            set => _maxRetries = Math.Max(0, value);
        }

        /// <summary>
        /// 重试间隔（毫秒，默认500ms）
        /// </summary>
        public int RetryDelayMs
        {
            get => _retryDelayMs;
            set => _retryDelayMs = Math.Max(0, value);
        }

        /// <summary>
        /// 是否使用指数退避（每次延迟翻倍）
        /// </summary>
        public bool UseExponentialBackoff
        {
            get => _useExponentialBackoff;
            set => _useExponentialBackoff = value;
        }

        /// <summary>
        /// 实际重试次数
        /// </summary>
        public int ActualRetryCount { get; private set; }

        /// <summary>
        /// 内部模块
        /// </summary>
        public IFlowModule InnerModule => _innerModule;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="innerModule">被包装的模块</param>
        /// <param name="name">模块名称</param>
        public RetryModule(IFlowModule innerModule, string name = null) : base(name ?? $"Retry({innerModule?.Name})")
        {
            _innerModule = innerModule ?? throw new ArgumentNullException(nameof(innerModule));
            _innerModule.Parent = this;
        }

        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            ActualRetryCount = 0;
            int currentDelay = RetryDelayMs;

            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (attempt > 0)
                {
                    ActualRetryCount = attempt;
                    ReportProgress((double)attempt / (MaxRetries + 1), $"第{attempt}次重试", 0, $"等待 {currentDelay}ms 后重试...");
                    await Task.Delay(currentDelay, cancellationToken);

                    if (UseExponentialBackoff)
                        currentDelay *= 2;
                }

                ReportProgress((double)attempt / (MaxRetries + 1), $"第{attempt + 1}次执行", 0);

                // 重置内部模块状态
                _innerModule.Reset();

                bool success = await _innerModule.ExecuteAsync(context);

                if (success)
                {
                    ReportProgress(1.0, "执行成功", 1.0);
                    return true;
                }

                // 记录最后一次失败的异常信息
                if (_innerModule.Statistics.Exception != null)
                {
                    Statistics.Exception = _innerModule.Statistics.Exception;
                }
                Statistics.ErrorMessage = $"第 {attempt + 1} 次执行失败: {_innerModule.Statistics.ErrorMessage}";
            }

            Statistics.ErrorMessage = $"重试耗尽(共{MaxRetries}次): {Statistics.ErrorMessage}";
            return false;
        }

        public override void Cancel()
        {
            base.Cancel();
            _innerModule?.Cancel();
        }

        public override void Reset()
        {
            base.Reset();
            ActualRetryCount = 0;
            _innerModule?.Reset();
        }
    }
}
