using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.StateMachine.Core;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Modules
{
    /// <summary>
    /// 串行容器模块
    /// 按顺序执行子模块
    /// </summary>
    public class SequentialModule : FlowModuleBase
    {
        private readonly List<IFlowModule> _modules = new List<IFlowModule>();
        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Sequential;
        
        /// <summary>
        /// 子模块列表
        /// </summary>
        public IReadOnlyList<IFlowModule> Modules => _modules;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public SequentialModule(string name = null) : base(name ?? "Sequential") { }

        /// <summary>
        /// 添加子模块
        /// </summary>
        /// <param name="module">子模块</param>
        public void AddModule(IFlowModule module)
        {
            module.Parent = this;
            _modules.Add(module);
        }

        /// <summary>
        /// 批量添加子模块
        /// </summary>
        /// <param name="modules">子模块集合</param>
        public void AddRange(IEnumerable<IFlowModule> modules)
        {
            foreach (var module in modules)
            {
                AddModule(module);
            }
        }

        /// <summary>
        /// 插入子模块
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="module">子模块</param>
        public void InsertModule(int index, IFlowModule module)
        {
            module.Parent = this;
            _modules.Insert(index, module);
        }

        /// <summary>
        /// 移除子模块
        /// </summary>
        /// <param name="module">子模块</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveModule(IFlowModule module)
        {
            return _modules.Remove(module);
        }

        /// <summary>
        /// 清空子模块
        /// </summary>
        public void ClearModules()
        {
            _modules.Clear();
        }

        /// <summary>
        /// 执行模块逻辑（支持断点续传）
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            // 断点续传：从上次保存的索引继续
            string resumeKey = $"Sequential_{ModuleId}_Index";
            int startIndex = context.GetResult<int>(resumeKey, 0);

            for (int i = startIndex; i < _modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var module = _modules[i];
                ReportProgress((double)i / _modules.Count, $"步骤 {i + 1}/{_modules.Count}: {module.Name}");

                var remainingTimeout = TimeoutMs - (int)this._executionTimer.ElapsedMilliseconds;
                if (remainingTimeout <= 0)
                {
                    // 保存当前索引以便续传
                    context.SetResult(resumeKey, i);
                    throw new TimeoutException("容器执行超时");
                }
                var originalTimeout = module.TimeoutMs;
                try
                {
                    module.TimeoutMs = Math.Min(originalTimeout, remainingTimeout);
                    var success = await module.ExecuteAsync(context);
                    if (!success)
                    {
                        Statistics.ErrorMessage = $"子模块 '{module.Name}' 执行失败: {module.Statistics.ErrorMessage}";
                        if (module.Statistics.Exception != null)
                            Statistics.Exception = module.Statistics.Exception;
                        return false;
                    }
                    // 成功执行后保存进度
                    context.SetResult(resumeKey, i + 1);
                }
                finally
                {
                    module.TimeoutMs = originalTimeout;
                }
            }

            // 完成后清除断点
            context.Results.TryRemove(resumeKey, out _);
            ReportProgress(1.0, "完成", 1.0);
            return true;
        }

        public override void Cancel()
        {
            base.Cancel();
            foreach (var module in _modules)
            {
                module.Cancel();
            }
        }

        public override void Reset()
        {
            base.Reset();
            foreach (var module in _modules)
            {
                module.Reset();
            }
        }
    }

    /// <summary>
    /// 并行容器模块
    /// 同时执行所有子模块
    /// </summary>
    public class ParallelModule : FlowModuleBase
    {
        private readonly List<IFlowModule> _modules = new List<IFlowModule>();
        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Parallel;
        
        /// <summary>
        /// 子模块列表
        /// </summary>
        public IReadOnlyList<IFlowModule> Modules => _modules;
        
        /// <summary>
        /// 并行执行选项
        /// </summary>
        public ParallelOptions ParallelOptions { get; set; } = new ParallelOptions();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public ParallelModule(string name = null) : base(name ?? "Parallel") { }

        /// <summary>
        /// 添加子模块
        /// </summary>
        /// <param name="module">子模块</param>
        public void AddModule(IFlowModule module)
        {
            module.Parent = this;
            module.IsParallel = true;
            _modules.Add(module);
        }

        /// <summary>
        /// 批量添加子模块
        /// </summary>
        /// <param name="modules">子模块集合</param>
        public void AddRange(IEnumerable<IFlowModule> modules)
        {
            foreach (var module in modules)
            {
                AddModule(module);
            }
        }

        /// <summary>
        /// 移除子模块
        /// </summary>
        /// <param name="module">子模块</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveModule(IFlowModule module)
        {
            return _modules.Remove(module);
        }

        /// <summary>
        /// 清空子模块
        /// </summary>
        public void ClearModules()
        {
            _modules.Clear();
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            var moduleList = _modules.ToList();
            if (moduleList.Count == 0)
            {
                ReportProgress(1.0);
                return true;
            }

            // 创建进度报告（使用局部变量捕获以支持取消订阅）
            var progressTracker = new Dictionary<string, double>();
            var progressHandlers = new Dictionary<string, EventHandler<double>>();

            foreach (var module in moduleList)
            {
                progressTracker[module.ModuleId] = 0;
                EventHandler<double> handler = (s, p) =>
                {
                    progressTracker[module.ModuleId] = p;
                    ReportProgress(progressTracker.Values.Average());
                };
                progressHandlers[module.ModuleId] = handler;
                module.OnProgressChanged += handler;
            }

            try
            {
                // 使用 CancellationTokenSource 以便一个失败时取消其他
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    // 并行执行所有模块
                    var executeTasks = moduleList.Select(async module =>
                    {
                        var originalTimeout = module.TimeoutMs;
                        try
                        {
                            module.TimeoutMs = this.TimeoutMs;
                            return await module.ExecuteAsync(context);
                        }
                        catch (OperationCanceledException) when (linkedCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                        {
                            module.Statistics.ErrorMessage = "被兄弟模块失败取消";
                            return false;
                        }
                        catch (Exception ex)
                        {
                            module.Statistics.ErrorMessage = ex.Message;
                            return false;
                        }
                        finally
                        {
                            module.TimeoutMs = originalTimeout;
                        }
                    }).ToList();

                    // 等待任意任务完成，一旦有失败立即取消其他
                    while (executeTasks.Count > 0)
                    {
                        var completedTask = await Task.WhenAny(executeTasks);
                        executeTasks.Remove(completedTask);

                        var result = await completedTask;
                        if (!result)
                        {
                            // 有模块失败，取消剩余任务
                            linkedCts.Cancel();

                            // 等待剩余任务完成取消
                            if (executeTasks.Count > 0)
                            {
                                await Task.WhenAll(executeTasks).ConfigureAwait(false);
                            }

                            var failedModules = moduleList.Where(m => m.Statistics.IsSuccess == false).ToList();
                            Statistics.ErrorMessage = $"以下模块执行失败: {string.Join(", ", failedModules.Select(m => m.Name))}";
                            return false;
                        }
                    }

                    ReportProgress(1.0);
                    return true;
                }
            }
            finally
            {
                // 取消订阅进度事件，防止内存泄漏
                foreach (var module in moduleList)
                {
                    if (progressHandlers.TryGetValue(module.ModuleId, out var handler))
                    {
                        module.OnProgressChanged -= handler;
                    }
                }
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            foreach (var module in _modules)
            {
                module.Cancel();
            }
        }

        public override void Reset()
        {
            base.Reset();
            foreach (var module in _modules)
            {
                module.Reset();
            }
        }
    }

    /// <summary>
    /// 条件分支模块
    /// 根据条件选择执行不同的分支
    /// </summary>
    public class ConditionalModule : FlowModuleBase
    {
        private readonly List<(Func<ExecutionContext, bool> Condition, IFlowModule Module)> _branches
            = new List<(Func<ExecutionContext, bool>, IFlowModule)>();
        private IFlowModule _elseModule;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public ConditionalModule(string name = null) : base(name ?? "Conditional") { }

        /// <summary>
        /// 添加条件分支
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <param name="module">分支模块</param>
        public void AddBranch(Func<ExecutionContext, bool> condition, IFlowModule module)
        {
            module.Parent = this;
            _branches.Add((condition, module));
        }

        /// <summary>
        /// 设置默认分支
        /// </summary>
        /// <param name="module">默认分支模块</param>
        public void SetElseModule(IFlowModule module)
        {
            module.Parent = this;
            _elseModule = module;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            // 按顺序检查条件，执行第一个满足条件的分支
            foreach (var (condition, module) in _branches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                    {
                        if (condition(context))
                        {
                            var originalTimeout = module.TimeoutMs;
                            try
                            {
                                module.TimeoutMs = this.TimeoutMs;
                                return await module.ExecuteAsync(context);
                            }
                            finally
                            {
                                module.TimeoutMs = originalTimeout;
                            }
                        }
                    }
                catch (Exception ex)
                {
                    Statistics.ErrorMessage = $"条件检查异常: {ex.Message}";
                    return false;
                }
            }

            // 执行else分支
            if (_elseModule != null)
            {
                _elseModule.TimeoutMs = this.TimeoutMs;
                return await _elseModule.ExecuteAsync(context);
            }

            // 没有匹配的分支，视为成功
            return true;
        }

        /// <summary>
        /// 取消执行
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
            foreach (var (_, module) in _branches)
            {
                module.Cancel();
            }
            _elseModule?.Cancel();
        }

        /// <summary>
        /// 重置模块
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            foreach (var (_, module) in _branches)
            {
                module.Reset();
            }
            _elseModule?.Reset();
        }
    }

    /// <summary>
    /// 循环模块
    /// 重复执行子模块直到条件不满足
    /// </summary>
    public class LoopModule : FlowModuleBase
    {
        private IFlowModule _bodyModule;
        private Func<ExecutionContext, int, bool> _loopCondition;
        private int _maxIterations = int.MaxValue;

        /// <summary>
        /// 模块类型
        /// </summary>
        public override ModuleType Type => ModuleType.Custom;
        
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int IterationCount { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        public LoopModule(string name = null) : base(name ?? "Loop") { }

        /// <summary>
        /// 设置循环体
        /// </summary>
        /// <param name="module">循环体模块</param>
        public void SetBody(IFlowModule module)
        {
            module.Parent = this;
            _bodyModule = module;
        }

        /// <summary>
        /// 设置循环条件
        /// </summary>
        /// <param name="condition">条件函数，参数为执行上下文和当前迭代次数</param>
        public void SetCondition(Func<ExecutionContext, int, bool> condition)
        {
            _loopCondition = condition;
        }

        /// <summary>
        /// 设置最大迭代次数
        /// </summary>
        /// <param name="max">最大迭代次数</param>
        public void SetMaxIterations(int max)
        {
            _maxIterations = max;
        }

        /// <summary>
        /// 执行模块逻辑
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>执行结果</returns>
        protected override async Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken)
        {
            if (_bodyModule == null)
                throw new InvalidOperationException("未设置循环体模块");

            IterationCount = 0;

            while (IterationCount < _maxIterations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 检查循环条件
                if (_loopCondition != null && !_loopCondition(context, IterationCount))
                    break;

                // 检查超时
                if (_executionTimer.ElapsedMilliseconds > TimeoutMs)
                {
                    throw new TimeoutException($"循环执行超时(迭代次数:{IterationCount})");
                }

                // 执行循环体
                var originalBodyTimeout = _bodyModule.TimeoutMs;
                try
                {
                    _bodyModule.TimeoutMs = TimeoutMs - (int)_executionTimer.ElapsedMilliseconds;
                    var success = await _bodyModule.ExecuteAsync(context);

                    if (!success)
                    {
                        Statistics.ErrorMessage = $"第 {IterationCount + 1} 次迭代失败: {_bodyModule.Statistics.ErrorMessage}";
                        return false;
                    }
                }
                finally
                {
                    _bodyModule.TimeoutMs = originalBodyTimeout;
                }

                // 重置循环体状态以便下次执行
                _bodyModule.Reset();
                IterationCount++;

                // 进度计算：有条件时预估最大迭代数，无条件时使用设定的最大值
                int estimatedMax = _loopCondition != null ? Math.Max(_maxIterations, 100) : _maxIterations;
                ReportProgress((double)IterationCount / estimatedMax);
            }

            ReportProgress(1.0);
            return true;
        }

        /// <summary>
        /// 取消执行
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
            _bodyModule?.Cancel();
        }

        /// <summary>
        /// 重置模块
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _bodyModule?.Reset();
            IterationCount = 0;
        }
    }
}
