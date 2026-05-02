using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = CoreToolkit.StateMachine.Models.ExecutionContext;
using CoreToolkit.StateMachine.Models;
using CoreToolkit.Data;
using Newtonsoft.Json;

namespace CoreToolkit.StateMachine.Core
{
    /// <summary>
    /// 状态机管理器
    /// 管理多个状态机的生命周期和执行
    /// </summary>
    public class StateMachineManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, StateMachine> _machines 
            = new ConcurrentDictionary<string, StateMachine>();
        private readonly ConcurrentDictionary<string, FlowStatistics> _statistics 
            = new ConcurrentDictionary<string, FlowStatistics>();
        private readonly StateMachineRecordRepository _recordRepository;
        private readonly BlockingCollection<StateMachineRecord> _persistQueue;
        private readonly Task _persistTask;
        private int _persistInFlight;
        private bool _disposed = false;

        /// <summary>
        /// 构造状态机管理器（无持久化）
        /// </summary>
        public StateMachineManager() { }

        /// <summary>
        /// 构造状态机管理器（支持持久化）
        /// </summary>
        /// <param name="recordRepository">状态机记录仓储</param>
        public StateMachineManager(StateMachineRecordRepository recordRepository)
        {
            _recordRepository = recordRepository;

            _persistQueue = new BlockingCollection<StateMachineRecord>(boundedCapacity: 1024);
            _persistTask = Task.Run((Action)PersistLoop);
        }

        /// <summary>全局默认超时时间(毫秒)</summary>
        public int DefaultTimeoutMs { get; set; } = 60000;

        /// <summary>状态机集合</summary>
        public IReadOnlyDictionary<string, StateMachine> Machines => _machines;

        /// <summary>统计信息集合</summary>
        public IReadOnlyDictionary<string, FlowStatistics> Statistics => _statistics;

        /// <summary>
        /// 创建新的状态机
        /// </summary>
        public StateMachine CreateMachine(string name, string description = null)
        {
            var machine = new StateMachine(name, description);
            machine.OnStatusChanged += (s, e) =>
            {
                if (e.NewStatus == StateMachineStatus.Completed || 
                    e.NewStatus == StateMachineStatus.Error)
                {
                    // 保存统计信息
                    SaveStatistics(machine);
                }
            };

            _machines[name] = machine;
            return machine;
        }

        /// <summary>
        /// 获取或创建状态机
        /// </summary>
        public StateMachine GetOrCreateMachine(string name, string description = null)
        {
            if (_machines.TryGetValue(name, out var machine))
                return machine;
            return CreateMachine(name, description);
        }

        /// <summary>
        /// 获取状态机
        /// </summary>
        public StateMachine GetMachine(string name)
        {
            _machines.TryGetValue(name, out var machine);
            return machine;
        }

        /// <summary>
        /// 移除状态机
        /// </summary>
        public bool RemoveMachine(string name)
        {
            if (_machines.TryRemove(name, out var machine))
            {
                machine.Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public async Task<bool> StartMachineAsync(string name, ExecutionContext context = null)
        {
            if (!_machines.TryGetValue(name, out var machine))
                throw new InvalidOperationException($"状态机 '{name}' 不存在");

            return await machine.StartAsync(context);
        }

        /// <summary>
        /// 停止状态机
        /// </summary>
        public void StopMachine(string name)
        {
            if (_machines.TryGetValue(name, out var machine))
            {
                machine.Stop();
            }
        }

        /// <summary>
        /// 暂停状态机
        /// </summary>
        public void PauseMachine(string name)
        {
            if (_machines.TryGetValue(name, out var machine))
            {
                machine.Pause();
            }
        }

        /// <summary>
        /// 恢复状态机
        /// </summary>
        public void ResumeMachine(string name)
        {
            if (_machines.TryGetValue(name, out var machine))
            {
                machine.Resume();
            }
        }

        /// <summary>
        /// 停止所有状态机
        /// </summary>
        public void StopAllMachines()
        {
            foreach (var machine in _machines.Values)
            {
                machine.Stop();
            }
        }

        /// <summary>
        /// 获取所有运行中的状态机
        /// </summary>
        public IEnumerable<StateMachine> GetRunningMachines()
        {
            return _machines.Values.Where(m => m.Status == StateMachineStatus.Running);
        }

        /// <summary>
        /// 获取状态机的统计信息
        /// </summary>
        public FlowStatistics GetStatistics(string machineName)
        {
            _statistics.TryGetValue(machineName, out var stats);
            return stats;
        }

        /// <summary>
        /// 清除所有统计信息
        /// </summary>
        public void ClearStatistics()
        {
            _statistics.Clear();
        }

        /// <summary>
        /// 从指定记录恢复状态机并执行
        /// </summary>
        /// <param name="machineName">状态机名称</param>
        /// <param name="recordId">记录ID</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> RestoreMachineAsync(string machineName, long recordId)
        {
            if (_recordRepository == null)
                throw new InvalidOperationException("状态机管理器未配置持久化仓储，无法执行恢复操作");

            var record = await _recordRepository.GetByIdAsync(recordId);
            if (record == null)
                throw new InvalidOperationException($"找不到指定的状态机记录 (ID: {recordId})");

            if (record.Status != StateMachineStatus.Error.ToString() &&
                record.Status != StateMachineStatus.Paused.ToString())
            {
                throw new InvalidOperationException($"记录状态为 '{record.Status}'，仅支持从 Error 或 Paused 状态恢复");
            }

            var machine = GetOrCreateMachine(machineName, record.Description);

            var context = new ExecutionContext
            {
                ContextId = record.ContextId ?? Guid.NewGuid().ToString("N")
            };

            if (!string.IsNullOrEmpty(record.ResumeDataJson))
            {
                context.DeserializeResumeData(record.ResumeDataJson);
            }

            return await StartMachineAsync(machineName, context);
        }

        /// <summary>
        /// 从指定状态机最近一条 Error 记录恢复并执行
        /// </summary>
        /// <param name="machineName">状态机名称</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> RestoreMachineAsync(string machineName)
        {
            if (_recordRepository == null)
                throw new InvalidOperationException("状态机管理器未配置持久化仓储，无法执行恢复操作");

            var record = await _recordRepository.GetLatestRecordAsync(machineName, StateMachineStatus.Error.ToString());
            if (record == null)
                throw new InvalidOperationException($"找不到状态机 '{machineName}' 的可恢复记录");

            return await RestoreMachineAsync(machineName, record.Id);
        }

        /// <summary>
        /// 获取全局统计摘要
        /// </summary>
        public GlobalStatistics GetGlobalStatistics()
        {
            var allStats = _statistics.Values.ToList();
            if (allStats.Count == 0)
                return new GlobalStatistics();

            return new GlobalStatistics
            {
                TotalFlowCount = allStats.Count,
                SuccessCount = allStats.Count(s => s.IsSuccess),
                FailedCount = allStats.Count(s => !s.IsSuccess),
                TotalExecutionTime = TimeSpan.FromMilliseconds(allStats.Sum(s => s.TotalDuration.TotalMilliseconds)),
                AverageExecutionTime = TimeSpan.FromMilliseconds(allStats.Average(s => s.TotalDuration.TotalMilliseconds)),
                MaxExecutionTime = allStats.Max(s => s.TotalDuration),
                MinExecutionTime = allStats.Min(s => s.TotalDuration)
            };
        }

        /// <summary>
        /// 等待后台持久化队列与进行中的写入完成，便于测试或退出前从数据库读到最新记录。
        /// </summary>
        public void WaitForPersistenceFlush(TimeSpan? timeout = null)
        {
            if (_persistQueue == null)
                return;

            var max = timeout ?? TimeSpan.FromSeconds(30);
            var sw = Stopwatch.StartNew();
            var stableIdleTicks = 0;
            while (sw.Elapsed < max)
            {
                if (_persistQueue.Count == 0 && Volatile.Read(ref _persistInFlight) == 0)
                {
                    stableIdleTicks++;
                    if (stableIdleTicks >= 20)
                        return;
                }
                else
                {
                    stableIdleTicks = 0;
                }

                Thread.Sleep(5);
            }
        }

        private void SaveStatistics(StateMachine machine)
        {
            var stats = new FlowStatistics
            {
                FlowName = machine.Name,
                StartTime = machine.StartTime,
                EndTime = DateTime.Now,
                IsSuccess = machine.Status == StateMachineStatus.Completed,
                ErrorMessage = machine.ErrorMessage,
                ModuleStats = machine.Modules.Select(m => m.Statistics).ToList()
            };

            _statistics[machine.Name] = stats;

            // 持久化到数据库
            if (_recordRepository != null)
            {
                try
                {
                    var record = new StateMachineRecord
                    {
                        MachineName = machine.Name,
                        Description = machine.Description,
                        Status = machine.Status.ToString(),
                        StartTime = machine.StartTime,
                        EndTime = DateTime.Now,
                        DurationMs = machine.ExecutionTime.TotalMilliseconds,
                        IsSuccess = machine.Status == StateMachineStatus.Completed,
                        ErrorMessage = machine.ErrorMessage,
                        Exception = machine.Exception?.ToString(),
                        ModuleStatsJson = JsonConvert.SerializeObject(stats.ModuleStats, Formatting.None),
                        ResumeDataJson = machine.GetExecutionContext()?.SerializeResumeData(),
                        ContextId = machine.GetExecutionContext()?.ContextId,
                        ModuleCount = machine.Modules.Count,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // 通过后台队列持久化，避免阻塞状态机线程
                    TryEnqueueRecord(record);
                }
                catch (Exception ex)
                {
                    // 持久化失败不应影响主流程，记录到调试输出
                    System.Diagnostics.Debug.WriteLine($"[StateMachineManager] 持久化状态机记录失败: {ex.Message}");
                }
            }
        }

        private void TryEnqueueRecord(StateMachineRecord record)
        {
            if (record == null) return;
            if (_persistQueue == null || _persistQueue.IsAddingCompleted)
                return;

            // 队列满时不阻塞调用方：异步降级写入，避免丢记录
            if (!_persistQueue.TryAdd(record))
            {
                System.Diagnostics.Debug.WriteLine("[StateMachineManager] 持久化队列已满，已降级为异步写入。");
                Task.Run(() =>
                {
                    Interlocked.Increment(ref _persistInFlight);
                    try
                    {
                        try
                        {
                            _recordRepository.Insert(record);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[StateMachineManager] 降级持久化失败: {ex.Message}");
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _persistInFlight);
                    }
                });
            }
        }

        private void PersistLoop()
        {
            if (_recordRepository == null || _persistQueue == null) return;

            try
            {
                foreach (var record in _persistQueue.GetConsumingEnumerable())
                {
                    Interlocked.Increment(ref _persistInFlight);
                    try
                    {
                        try
                        {
                            _recordRepository.Insert(record);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[StateMachineManager] 后台持久化失败: {ex.Message}");
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _persistInFlight);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StateMachineManager] 持久化线程异常: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopAllMachines();
            _machines.Clear();
            _statistics.Clear();

            if (_persistQueue != null)
            {
                try
                {
                    _persistQueue.CompleteAdding();
                }
                catch { }
            }
            try
            {
                _persistTask?.Wait(TimeSpan.FromSeconds(10));
            }
            catch { }
            try
            {
                _persistQueue?.Dispose();
            }
            catch { }

            _disposed = true;
        }
    }

    /// <summary>
    /// 状态机
    /// 表示一个独立的设备流程
    /// </summary>
    public class StateMachine : IDisposable
    {
        private StateMachineStatus _status = StateMachineStatus.Idle;
        private CancellationTokenSource _cts;
        private bool _disposed = false;
        private readonly List<IFlowModule> _modules = new List<IFlowModule>();
        private IFlowModule _rootModule;
        private ExecutionContext _executionContext;
        private readonly Stopwatch _executionTimer = new Stopwatch();

        /// <summary>
        /// 状态机名称
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// 状态机描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 状态机状态
        /// </summary>
        public StateMachineStatus Status
        {
            get => _status;
            private set
            {
                var oldStatus = _status;
                if (oldStatus == value) return;
                
                _status = value;
                OnStatusChanged?.Invoke(this, new StateMachineEventArgs
                {
                    MachineName = Name,
                    OldStatus = oldStatus,
                    NewStatus = value,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; private set; }
        
        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime => _executionTimer.Elapsed;
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        
        /// <summary>
        /// 异常对象（完整异常信息）
        /// </summary>
        public Exception Exception { get; private set; }
        
        /// <summary>
        /// 模块列表
        /// </summary>
        public IReadOnlyList<IFlowModule> Modules => _modules;

        /// <summary>状态变更事件</summary>
        public event EventHandler<StateMachineEventArgs> OnStatusChanged;
        /// <summary>模块执行事件</summary>
        public event EventHandler<ModuleEventArgs> OnModuleExecuting;
        /// <summary>模块完成事件</summary>
        public event EventHandler<ModuleEventArgs> OnModuleCompleted;
        /// <summary>超时事件</summary>
        public event EventHandler<TimeoutEventArgs> OnTimeoutOccurred;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <param name="description">状态机描述</param>
        public StateMachine(string name, string description = null)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 设置根模块（通常是SequentialModule或ParallelModule）
        /// </summary>
        public void SetRootModule(IFlowModule module)
        {
            _rootModule = module;
            CollectModules(module);
        }

        /// <summary>
        /// 添加模块（用于简单线性流程）
        /// </summary>
        public void AddModule(IFlowModule module)
        {
            _modules.Add(module);
            
            // 如果没有根模块，创建默认的SequentialModule
            if (_rootModule == null)
            {
                _rootModule = new Modules.SequentialModule($"{Name}_Root");
            }
            
            if (_rootModule is Modules.SequentialModule seq)
            {
                seq.AddModule(module);
            }

            // 订阅模块事件
            SubscribeModuleEvents(module);
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public async Task<bool> StartAsync(ExecutionContext context = null)
        {
            if (Status == StateMachineStatus.Running)
                throw new InvalidOperationException("状态机已在运行中");

            if (_rootModule == null)
                throw new InvalidOperationException("未设置根模块");

            _cts = new CancellationTokenSource();
            _executionContext = context ?? new ExecutionContext();
            _executionContext.CancellationToken = _cts.Token;

            try
            {
                Status = StateMachineStatus.Running;
                StartTime = DateTime.Now;
                _executionTimer.Restart();
                ErrorMessage = null;
                Exception = null;

                // 重置所有模块
                ResetAllModules();

                // 执行根模块
                var success = await _rootModule.ExecuteAsync(_executionContext);

                if (success)
                {
                    Status = StateMachineStatus.Completed;
                }
                else if (_cts.IsCancellationRequested)
                {
                    Status = StateMachineStatus.Idle;
                }
                else
                {
                    ErrorMessage = _rootModule.Statistics.ErrorMessage;
                    Exception = _rootModule.Statistics.Exception;
                    Status = StateMachineStatus.Error;
                }

                return success;
            }
            catch (OperationCanceledException)
            {
                Status = StateMachineStatus.Idle;
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Exception = ex;
                Status = StateMachineStatus.Error;
                return false;
            }
            finally
            {
                _executionTimer.Stop();
            }
        }

        /// <summary>
        /// 异步停止状态机并等待清理完成
        /// </summary>
        public async Task StopAsync(TimeSpan timeout = default)
        {
            if (Status != StateMachineStatus.Running && Status != StateMachineStatus.Paused)
                return;

            _cts?.Cancel();
            _rootModule?.Cancel();
            Status = StateMachineStatus.Stopping;

            // 给模块一些时间完成清理
            var delay = timeout == default ? TimeSpan.FromSeconds(3) : timeout;
            await Task.Delay(delay);

            Status = StateMachineStatus.Idle;
        }

        /// <summary>
        /// 停止状态机
        /// </summary>
        public void Stop()
        {
            if (Status != StateMachineStatus.Running && Status != StateMachineStatus.Paused)
                return;

            _cts?.Cancel();
            _rootModule?.Cancel();
            Status = StateMachineStatus.Stopping;
        }

        /// <summary>
        /// 暂停状态机
        /// </summary>
        public void Pause()
        {
            if (Status != StateMachineStatus.Running)
                return;

            // 暂停所有可暂停的模块
            foreach (var module in _modules.OfType<IPausableModule>())
            {
                module.Pause();
            }
            Status = StateMachineStatus.Paused;
        }

        /// <summary>
        /// 恢复状态机
        /// </summary>
        public void Resume()
        {
            if (Status != StateMachineStatus.Paused)
                return;

            // 恢复所有可暂停的模块
            foreach (var module in _modules.OfType<IPausableModule>())
            {
                module.Resume();
            }
            Status = StateMachineStatus.Running;
        }

        /// <summary>
        /// 重置状态机
        /// </summary>
        public void Reset()
        {
            Stop();
            Status = StateMachineStatus.Idle;
            ErrorMessage = null;
            ResetAllModules();
        }

        /// <summary>
        /// 获取当前执行上下文
        /// </summary>
        public ExecutionContext GetExecutionContext()
        {
            return _executionContext;
        }

        private void CollectModules(IFlowModule module)
        {
            _modules.Add(module);
            SubscribeModuleEvents(module);

            // 递归收集子模块
            if (module is Modules.SequentialModule seq)
            {
                foreach (var child in seq.Modules)
                {
                    CollectModules(child);
                }
            }
            else if (module is Modules.ParallelModule par)
            {
                foreach (var child in par.Modules)
                {
                    CollectModules(child);
                }
            }
        }

        private void SubscribeModuleEvents(IFlowModule module)
        {
            module.OnStatusChanged += (s, e) =>
            {
                if (e.NewStatus == ModuleStatus.Running)
                {
                    OnModuleExecuting?.Invoke(this, e);
                }
                else if (e.NewStatus == ModuleStatus.Completed || 
                         e.NewStatus == ModuleStatus.Failed ||
                         e.NewStatus == ModuleStatus.Timeout)
                {
                    OnModuleCompleted?.Invoke(this, e);

                    if (e.NewStatus == ModuleStatus.Timeout)
                    {
                        OnTimeoutOccurred?.Invoke(this, new TimeoutEventArgs
                        {
                            ModuleId = e.ModuleId,
                            ModuleName = e.ModuleName,
                            Timeout = TimeSpan.FromMilliseconds(module.TimeoutMs),
                            ActualDuration = TimeSpan.FromMilliseconds(e.DurationMs),
                            Type = TimeoutType.ExecutionTimeout,
                            Timestamp = DateTime.Now
                        });
                    }
                }
            };
        }

        private void ResetAllModules()
        {
            foreach (var module in _modules)
            {
                module.Reset();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Stop();
            _cts?.Dispose();
            _cts = null;
            _disposed = true;
        }
    }

    /// <summary>
    /// 全局统计信息
    /// </summary>
    public class GlobalStatistics
    {
        /// <summary>
        /// 总流程次数
        /// </summary>
        public int TotalFlowCount { get; set; }
        
        /// <summary>
        /// 成功次数
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailedCount { get; set; }
        
        /// <summary>
        /// 总执行时间
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }
        
        /// <summary>
        /// 平均执行时间
        /// </summary>
        public TimeSpan AverageExecutionTime { get; set; }
        
        /// <summary>
        /// 最大执行时间
        /// </summary>
        public TimeSpan MaxExecutionTime { get; set; }
        
        /// <summary>
        /// 最小执行时间
        /// </summary>
        public TimeSpan MinExecutionTime { get; set; }
        
        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate => TotalFlowCount > 0 ? (double)SuccessCount / TotalFlowCount * 100 : 0;
    }
}
