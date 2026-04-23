using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 安全监控器
    /// 后台线程实时监控设备安全状态
    /// </summary>
    public class SafetyMonitor : IDisposable
    {
        private readonly IMotionCard _motionCard;
        private readonly ICollisionDetector _collisionDetector;
        private readonly InterlockEngine _interlockEngine;
        private Task _monitorTask;
        private CancellationTokenSource _cts;
        private bool _disposed;
        private readonly Dictionary<string, int[]> _volumeAxisMap = new Dictionary<string, int[]>();

        /// <summary>
        /// 监控周期（毫秒）
        /// </summary>
        public int IntervalMs { get; set; } = 100;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _monitorTask != null && !_monitorTask.IsCompleted;

        /// <summary>
        /// 碰撞事件
        /// </summary>
        public event EventHandler<CollisionResult> CollisionDetected;

        /// <summary>
        /// 互锁触发事件
        /// </summary>
        public event EventHandler<InterlockEvaluationResult> InterlockTriggered;

        /// <summary>
        /// 安全状态变更事件
        /// </summary>
        public event EventHandler<bool> SafetyStatusChanged;

        /// <summary>
        /// 当前是否安全
        /// </summary>
        public bool IsCurrentlySafe { get; private set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SafetyMonitor(IMotionCard motionCard,
            ICollisionDetector collisionDetector,
            InterlockEngine interlockEngine = null)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _collisionDetector = collisionDetector ?? throw new ArgumentNullException(nameof(collisionDetector));
            _interlockEngine = interlockEngine;
        }

        /// <summary>
        /// 注册动态体积与轴的映射关系
        /// </summary>
        public void RegisterVolumeAxisMapping(string volumeId, params int[] axisIndices)
        {
            _volumeAxisMap[volumeId] = axisIndices;
        }

        /// <summary>
        /// 启动监控
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoop(_cts.Token), _cts.Token);
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            try
            {
                _monitorTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch { }
            _monitorTask = null;
        }

        /// <summary>
        /// 监控循环
        /// </summary>
        private async Task MonitorLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(IntervalMs, token);

                    // 1. 更新动态体积位置
                    UpdateDynamicVolumes();

                    // 2. 碰撞检测
                    var collisionResult = _collisionDetector.CheckCollision();
                    if (!collisionResult.IsSafe)
                    {
                        HandleCollision(collisionResult);
                    }

                    // 3. 互锁评估
                    if (_interlockEngine != null)
                    {
                        var interlockResult = _interlockEngine.EvaluateAll();
                        if (!interlockResult.IsSafe)
                        {
                            HandleInterlock(interlockResult);
                        }
                    }

                    // 4. 更新总体安全状态
                    bool nowSafe = collisionResult.IsSafe &&
                                   (_interlockEngine == null || _interlockEngine.EvaluateAll().IsSafe);

                    if (nowSafe != IsCurrentlySafe)
                    {
                        IsCurrentlySafe = nowSafe;
                        SafetyStatusChanged?.Invoke(this, nowSafe);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 监控异常不应中断，记录后继续
                    System.Diagnostics.Debug.WriteLine($"[SafetyMonitor] 异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 更新所有动态体积的当前位置
        /// </summary>
        private void UpdateDynamicVolumes()
        {
            foreach (var kv in _volumeAxisMap)
            {
                var axes = kv.Value;
                if (axes == null || axes.Length == 0) continue;

                double x = 0, y = 0, z = 0;
                x = _motionCard.GetPosition(axes[0]);
                if (axes.Length > 1) y = _motionCard.GetPosition(axes[1]);
                if (axes.Length > 2) z = _motionCard.GetPosition(axes[2]);

                _collisionDetector.UpdateAxisPosition(kv.Key, x, y, z);
            }
        }

        /// <summary>
        /// 处理碰撞事件
        /// </summary>
        private void HandleCollision(CollisionResult result)
        {
            // 触发事件供上层处理
            CollisionDetected?.Invoke(this, result);

            // 根据策略决定是否急停（这里保守策略：立即急停）
            try
            {
                _motionCard.StopAll(true);
            }
            catch { }
        }

        /// <summary>
        /// 处理互锁事件
        /// </summary>
        private void HandleInterlock(InterlockEvaluationResult result)
        {
            InterlockTriggered?.Invoke(this, result);

            switch (result.RecommendedAction)
            {
                case InterlockAction.EmergencyStop:
                    try { _motionCard.StopAll(true); } catch { }
                    break;
                case InterlockAction.DecelerateStop:
                    try { _motionCard.StopAll(false); } catch { }
                    break;
                case InterlockAction.BlockMotion:
                    // 仅阻止新运动，已在SafeMotionController中处理
                    break;
                case InterlockAction.AlarmOnly:
                    // 仅报警，不停机
                    break;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _cts?.Dispose();
                _disposed = true;
            }
        }
    }
}
