using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Core
{
    /// <summary>
    /// 流程模块基类
    /// 提供通用的执行框架和状态管理
    /// </summary>
    public abstract class FlowModuleBase : IFlowModule
    {
        private ModuleStatus _status = ModuleStatus.Pending;
        private readonly object _statusLock = new object();
        private CancellationTokenSource _cts;
        protected readonly Stopwatch _executionTimer = new Stopwatch();
        protected readonly Stopwatch _waitTimer = new Stopwatch();

        /// <summary>
        /// 模块ID
        /// </summary>
        public string ModuleId { get; protected set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
        
        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 模块类型
        /// </summary>
        public abstract ModuleType Type { get; }
        
        /// <summary>
        /// 模块状态
        /// </summary>
        public ModuleStatus Status
        {
            get
            {
                lock (_statusLock)
                {
                    return _status;
                }
            }
            protected set
            {
                ModuleStatus oldStatus;
                ModuleStatus newValue = value;

                lock (_statusLock)
                {
                    oldStatus = _status;
                    if (oldStatus == newValue) return;
                    _status = newValue;
                }

                // 更新统计信息
                UpdateStatistics(oldStatus, newValue);

                // 触发事件
                OnStatusChanged?.Invoke(this, new ModuleEventArgs
                {
                    ModuleId = ModuleId,
                    ModuleName = Name,
                    OldStatus = oldStatus,
                    NewStatus = newValue,
                    DurationMs = _executionTimer.ElapsedMilliseconds,
                    Timestamp = DateTime.Now
                });
            }
        }
        
        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;
        
        /// <summary>
        /// 是否并行执行
        /// </summary>
        public bool IsParallel { get; set; } = false;
        
        /// <summary>
        /// 父模块
        /// </summary>
        public IFlowModule Parent { get; set; }
        
        /// <summary>
        /// 模块统计信息
        /// </summary>
        public ModuleStatistics Statistics { get; protected set; } = new ModuleStatistics();

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event EventHandler<ModuleEventArgs> OnStatusChanged;
        
        /// <summary>
        /// 进度变更事件
        /// </summary>
        public event EventHandler<double> OnProgressChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">模块名称</param>
        protected FlowModuleBase(string name)
        {
            Name = name ?? GetType().Name;
            Statistics.ModuleId = ModuleId;
            Statistics.ModuleName = Name;
            Statistics.Type = Type;
        }

        /// <summary>
        /// 执行模块
        /// </summary>
        /// <param name="context">执行上下文</param>
        /// <returns>执行结果</returns>
        public virtual async Task<bool> ExecuteAsync(ExecutionContext context)
        {
            if (Status == ModuleStatus.Running || Status == ModuleStatus.Completed)
                return Status == ModuleStatus.Completed;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            Statistics.StartTime = DateTime.Now;
            Statistics.ModuleId = ModuleId;
            Statistics.ModuleName = Name;
            Statistics.Type = Type;

            bool timedOut = false;

            try
            {
                Status = ModuleStatus.Running;
                _executionTimer.Restart();
                _waitTimer.Stop();

                var executeTask = ExecuteInternalAsync(context, _cts.Token);

                using (var timeoutCts = new CancellationTokenSource(TimeoutMs))
                {
                    var timeoutTask = Task.Delay(TimeoutMs, timeoutCts.Token);
                    var completedTask = await Task.WhenAny(executeTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        timedOut = true;
                        _cts.Cancel();

                        try
                        {
                            await executeTask;
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        Status = ModuleStatus.Timeout;
                        Statistics.IsTimeout = true;
                        Statistics.IsSuccess = false;
                        Statistics.ErrorMessage = $"执行超时(设置:{TimeoutMs}ms, 实际:{_executionTimer.ElapsedMilliseconds}ms)";
                        OnTimeoutOccurred?.Invoke(this, new TimeoutEventArgs
                        {
                            ModuleId = ModuleId,
                            ModuleName = Name,
                            Timeout = TimeSpan.FromMilliseconds(TimeoutMs),
                            ActualDuration = _executionTimer.Elapsed,
                            Type = TimeoutType.ExecutionTimeout,
                            Timestamp = DateTime.Now
                        });
                        return false;
                    }
                    else
                    {
                        timeoutCts.Cancel();
                        var completed = await executeTask;

                        if (completed)
                        {
                            Status = ModuleStatus.Completed;
                            Statistics.IsSuccess = true;
                        }
                        else if (_cts.IsCancellationRequested && !timedOut)
                        {
                            Status = ModuleStatus.Cancelled;
                            Statistics.IsSuccess = false;
                        }
                        else
                        {
                            Status = ModuleStatus.Failed;
                            Statistics.IsSuccess = false;
                        }

                        return completed;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (timedOut)
                {
                    Status = ModuleStatus.Timeout;
                    Statistics.IsTimeout = true;
                }
                else
                {
                    Status = ModuleStatus.Cancelled;
                }
                Statistics.IsSuccess = false;
                return false;
            }
            catch (Exception ex)
            {
                Status = ModuleStatus.Failed;
                Statistics.IsSuccess = false;
                Statistics.ErrorMessage = ex.Message;
                OnStatusChanged?.Invoke(this, new ModuleEventArgs
                {
                    ModuleId = ModuleId,
                    ModuleName = Name,
                    OldStatus = ModuleStatus.Running,
                    NewStatus = ModuleStatus.Failed,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.Now
                });
                return false;
            }
            finally
            {
                _executionTimer.Stop();
                Statistics.EndTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 内部执行逻辑（子类实现）
        /// </summary>
        protected abstract Task<bool> ExecuteInternalAsync(ExecutionContext context, CancellationToken cancellationToken);

        /// <summary>
        /// 取消执行
        /// </summary>
        public virtual void Cancel()
        {
            _cts?.Cancel();
            if (Status == ModuleStatus.Running || Status == ModuleStatus.Waiting)
            {
                Status = ModuleStatus.Cancelled;
            }
        }

        /// <summary>
        /// 重置模块
        /// </summary>
        public virtual void Reset()
        {
            Cancel();
            _cts?.Dispose();
            _cts = null;
            Status = ModuleStatus.Pending;
            Statistics = new ModuleStatistics
            {
                ModuleId = ModuleId,
                ModuleName = Name,
                Type = Type
            };
            _executionTimer.Reset();
            _waitTimer.Reset();
        }

        /// <summary>
        /// 报告进度
        /// </summary>
        /// <param name="progress">进度值（0-1）</param>
        protected void ReportProgress(double progress)
        {
            OnProgressChanged?.Invoke(this, Math.Max(0, Math.Min(1, progress)));
        }

        /// <summary>
        /// 设置为等待状态
        /// </summary>
        protected void SetWaitingState()
        {
            if (Status != ModuleStatus.Running) return;
            Status = ModuleStatus.Waiting;
            _waitTimer.Start();
        }

        /// <summary>
        /// 设置为运行状态
        /// </summary>
        protected void SetRunningState()
        {
            if (Status != ModuleStatus.Waiting) return;
            Status = ModuleStatus.Running;
            _waitTimer.Stop();
            Statistics.WaitTime += _waitTimer.Elapsed;
            _waitTimer.Reset();
        }

        private void UpdateStatistics(ModuleStatus oldStatus, ModuleStatus newStatus)
        {
            Statistics.ModuleId = ModuleId;
            Statistics.ModuleName = Name;
            Statistics.Type = Type;

            if (newStatus == ModuleStatus.Waiting && oldStatus == ModuleStatus.Running)
            {
                _waitTimer.Start();
            }
            else if (newStatus == ModuleStatus.Running && oldStatus == ModuleStatus.Waiting)
            {
                _waitTimer.Stop();
                Statistics.WaitTime += _waitTimer.Elapsed;
            }
        }

        /// <summary>
        /// 超时事件 - 受保护的虚方法供子类调用
        /// </summary>
        /// <param name="e">超时事件参数</param>
        protected virtual void OnRaiseTimeoutOccurred(TimeoutEventArgs e)
        {
            OnTimeoutOccurred?.Invoke(this, e);
        }
        
        /// <summary>
        /// 超时事件
        /// </summary>
        public event EventHandler<TimeoutEventArgs> OnTimeoutOccurred;
    }
}
