using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CoreToolkit.Data;
using CoreToolkit.Desktop.MVVM;
using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Helpers;
using CoreToolkit.Safety.Models;

namespace MotionTest.WPF.ViewModels
{
    /// <summary>
    /// 安全监控日志条目
    /// </summary>
    public class SafetyLogEntry : ObservableObject
    {
        private DateTime _timestamp;
        private string _message;
        private string _level;
        private Brush _messageBrush;

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public string TimeText => Timestamp.ToString("HH:mm:ss.fff");

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public Brush MessageBrush
        {
            get => _messageBrush;
            set => SetProperty(ref _messageBrush, value);
        }
    }

    /// <summary>
    /// 安全体积视图模型（包装 SafetyVolume 便于绑定）
    /// </summary>
    public class SafetyVolumeViewModel : ObservableObject
    {
        private SafetyVolume _volume;

        public SafetyVolume Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        public string Name => _volume?.Name;
        public string Type => _volume?.Type.ToString();
        public string SizeText => _volume?.BoundingBox != null
            ? $"{_volume.BoundingBox.SizeX:F1}×{_volume.BoundingBox.SizeY:F1}×{_volume.BoundingBox.SizeZ:F1}"
            : "--";
        public string MarginText => _volume?.SafetyMargin.ToString("F1");
        public bool IsActive => _volume?.IsActive ?? false;
        public string LinkedAxesText => _volume?.LinkedAxes != null
            ? string.Join(",", _volume.LinkedAxes)
            : "--";
    }

    /// <summary>
    /// 互锁规则视图模型
    /// </summary>
    public class InterlockRuleViewModel : ObservableObject
    {
        private InterlockRule _rule;

        public InterlockRule Rule
        {
            get => _rule;
            set => SetProperty(ref _rule, value);
        }

        public string Name => _rule?.Name;
        public string Action => _rule?.Action.ToString();
        public string ConditionExpression => _rule?.ConditionExpression ?? (_rule?.Condition != null ? "Lambda" : "None");
        public bool Enabled => _rule?.Enabled ?? false;
    }

    /// <summary>
    /// 安全监控窗口视图模型
    /// </summary>
    public class SafetyMonitorViewModel : ObservableObject
    {
        private readonly IMotionCard _motionCard;
        private readonly LogRepository _logRepo;
        private readonly Dispatcher _dispatcher;

        private SafeMotionController _safeController;
        private SafetyMonitor _monitor;
        private DualHeadAntiCollision _dualHead;
        private DispatcherTimer _uiRefreshTimer;

        // === 总体状态 ===
        private string _overallStatusText = "未初始化";
        private Brush _overallStatusBrush = Brushes.Gray;
        private bool _isSafetyInitialized;
        private bool _isMonitorRunning;

        // === 详细状态 ===
        private string _monitorRunningText = "否";
        private Brush _monitorRunningBrush = Brushes.Red;
        private string _collisionStatusText = "未知";
        private Brush _collisionStatusBrush = Brushes.Gray;
        private string _interlockStatusText = "未知";
        private Brush _interlockStatusBrush = Brushes.Gray;
        private int _volumeCount;
        private int _ruleCount;

        // === 软限位 ===
        private string _xLimitText = "未启用";
        private string _yLimitText = "未启用";
        private string _zLimitText = "未启用";

        // === 双头 ===
        private string _headAPosText = "--";
        private string _headBPosText = "--";
        private string _headSeparationText = "--";
        private Brush _headSeparationBrush = Brushes.Gray;
        private string _minSeparationText = "50.0";
        private string _preventCrossingText = "是";

        // === 日志 ===
        private string _lastEventTimeText = "";

        public SafetyMonitorViewModel(IMotionCard motionCard, LogRepository logRepo, Dispatcher dispatcher)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _logRepo = logRepo;
            _dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;

            Volumes = new ObservableCollection<SafetyVolumeViewModel>();
            Rules = new ObservableCollection<InterlockRuleViewModel>();
            Logs = new ObservableCollection<SafetyLogEntry>();

            InitializeCommands();
            InitializeTimer();

            AddLog("安全防护监控窗口已打开", "Info", Brushes.LightGreen);
        }

        #region 属性

        public string OverallStatusText
        {
            get => _overallStatusText;
            set => SetProperty(ref _overallStatusText, value);
        }

        public Brush OverallStatusBrush
        {
            get => _overallStatusBrush;
            set => SetProperty(ref _overallStatusBrush, value);
        }

        public bool IsSafetyInitialized
        {
            get => _isSafetyInitialized;
            set
            {
                if (SetProperty(ref _isSafetyInitialized, value))
                {
                    ((RelayCommand)InitializeSafetyCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StartMonitorCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddVolumeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddRuleCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CheckCollisionCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMonitorRunning
        {
            get => _isMonitorRunning;
            set
            {
                if (SetProperty(ref _isMonitorRunning, value))
                {
                    ((RelayCommand)StartMonitorCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopMonitorCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string MonitorRunningText
        {
            get => _monitorRunningText;
            set => SetProperty(ref _monitorRunningText, value);
        }

        public Brush MonitorRunningBrush
        {
            get => _monitorRunningBrush;
            set => SetProperty(ref _monitorRunningBrush, value);
        }

        public string CollisionStatusText
        {
            get => _collisionStatusText;
            set => SetProperty(ref _collisionStatusText, value);
        }

        public Brush CollisionStatusBrush
        {
            get => _collisionStatusBrush;
            set => SetProperty(ref _collisionStatusBrush, value);
        }

        public string InterlockStatusText
        {
            get => _interlockStatusText;
            set => SetProperty(ref _interlockStatusText, value);
        }

        public Brush InterlockStatusBrush
        {
            get => _interlockStatusBrush;
            set => SetProperty(ref _interlockStatusBrush, value);
        }

        public int VolumeCount
        {
            get => _volumeCount;
            set => SetProperty(ref _volumeCount, value);
        }

        public int RuleCount
        {
            get => _ruleCount;
            set => SetProperty(ref _ruleCount, value);
        }

        public string XLimitText
        {
            get => _xLimitText;
            set => SetProperty(ref _xLimitText, value);
        }

        public string YLimitText
        {
            get => _yLimitText;
            set => SetProperty(ref _yLimitText, value);
        }

        public string ZLimitText
        {
            get => _zLimitText;
            set => SetProperty(ref _zLimitText, value);
        }

        public string HeadAPosText
        {
            get => _headAPosText;
            set => SetProperty(ref _headAPosText, value);
        }

        public string HeadBPosText
        {
            get => _headBPosText;
            set => SetProperty(ref _headBPosText, value);
        }

        public string HeadSeparationText
        {
            get => _headSeparationText;
            set => SetProperty(ref _headSeparationText, value);
        }

        public Brush HeadSeparationBrush
        {
            get => _headSeparationBrush;
            set => SetProperty(ref _headSeparationBrush, value);
        }

        public string MinSeparationText
        {
            get => _minSeparationText;
            set => SetProperty(ref _minSeparationText, value);
        }

        public string PreventCrossingText
        {
            get => _preventCrossingText;
            set => SetProperty(ref _preventCrossingText, value);
        }

        public string LastEventTimeText
        {
            get => _lastEventTimeText;
            set => SetProperty(ref _lastEventTimeText, value);
        }

        public ObservableCollection<SafetyVolumeViewModel> Volumes { get; }
        public ObservableCollection<InterlockRuleViewModel> Rules { get; }
        public ObservableCollection<SafetyLogEntry> Logs { get; }

        #endregion

        #region 命令

        public ICommand InitializeSafetyCommand { get; private set; }
        public ICommand StartMonitorCommand { get; private set; }
        public ICommand StopMonitorCommand { get; private set; }
        public ICommand EmergencyStopCommand { get; private set; }
        public ICommand AddVolumeCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }
        public ICommand CheckCollisionCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }

        private void InitializeCommands()
        {
            InitializeSafetyCommand = new RelayCommand(ExecuteInitializeSafety, () => !IsSafetyInitialized);
            StartMonitorCommand = new RelayCommand(ExecuteStartMonitor, () => IsSafetyInitialized && !IsMonitorRunning);
            StopMonitorCommand = new RelayCommand(ExecuteStopMonitor, () => IsMonitorRunning);
            EmergencyStopCommand = new RelayCommand(ExecuteEmergencyStop);
            AddVolumeCommand = new RelayCommand(ExecuteAddVolume, () => IsSafetyInitialized);
            AddRuleCommand = new RelayCommand(ExecuteAddRule, () => IsSafetyInitialized);
            CheckCollisionCommand = new RelayCommand(ExecuteCheckCollision, () => IsSafetyInitialized);
            ClearLogCommand = new RelayCommand(ExecuteClearLog);
        }

        #endregion

        #region 命令执行

        private void ExecuteInitializeSafety()
        {
            try
            {
                if (_motionCard == null || !_motionCard.IsOpen)
                {
                    AddLog("控制卡未打开，无法初始化安全系统", "Warning", Brushes.Orange);
                    return;
                }

                AddLog("正在初始化安全系统...", "Info", Brushes.LightBlue);

                _safeController = SafetySetupHelper.CreateSafeMotionSystem(_motionCard);

                // 注册示例体积
                _safeController.CollisionDetector.RegisterVolume(new SafetyVolume
                {
                    Id = "贴装头",
                    Name = "贴装头",
                    Type = VolumeType.Dynamic,
                    BoundingBox = new BoundingBox
                    {
                        MinX = -10, MaxX = 10,
                        MinY = -10, MaxY = 10,
                        MinZ = -30, MaxZ = 5
                    },
                    SafetyMargin = 2.0,
                    LinkedAxes = new[] { 0, 1, 2 },
                    OffsetX = 0, OffsetY = 0, OffsetZ = 0
                });

                _safeController.CollisionDetector.RegisterVolume(new SafetyVolume
                {
                    Id = "基板区域",
                    Name = "基板区域",
                    Type = VolumeType.Static,
                    BoundingBox = new BoundingBox
                    {
                        MinX = 100, MaxX = 500,
                        MinY = 50, MaxY = 450,
                        MinZ = -5, MaxZ = 10
                    },
                    SafetyMargin = 5.0
                });

                // 互锁规则
                SafetySetupHelper.AddServoEnableCheck(
                    _safeController.InterlockEngine,
                    () => _motionCard.GetServoEnable(0));

                // 软限位
                _safeController.SoftLimitGuard.SetLimit(0, 600, 0);
                _safeController.SoftLimitGuard.SetLimit(1, 500, 0);
                _safeController.SoftLimitGuard.SetLimit(2, 50, 0);

                // 双头防碰撞
                _dualHead = SafetySetupHelper.SetupDualHeadProtection(
                    _motionCard, headAAxis: 0, headBAxis: 1,
                    minSeparation: 50.0, preventCrossing: true);

                IsSafetyInitialized = true;
                RefreshVolumes();
                RefreshRules();
                RefreshSoftLimits();
                RefreshDualHeadConfig();

                UpdateOverallStatus(true, "已初始化", Brushes.Blue);
                AddLog("✅ 安全系统初始化完成", "Info", Brushes.LightGreen);
                AddLog($"   体积: 2 个, 互锁规则: {_safeController.InterlockEngine.GetAllRules().Count} 个", "Info", Brushes.Gray);

                // 持久化
                _logRepo?.WriteInfo("安全系统初始化完成", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 初始化失败: {ex.Message}", "Error", Brushes.Red);
                _logRepo?.WriteError($"安全系统初始化失败: {ex.Message}", "SafetyMonitor");
            }
        }

        private void ExecuteStartMonitor()
        {
            try
            {
                if (_monitor != null && _monitor.IsRunning) return;

                _monitor = SafetySetupHelper.CreateAndStartMonitor(
                    _motionCard,
                    _safeController.CollisionDetector,
                    _safeController.InterlockEngine,
                    intervalMs: 100);

                _monitor.RegisterVolumeAxisMapping("贴装头", 0, 1, 2);
                _monitor.CollisionDetected += OnCollisionDetected;
                _monitor.InterlockTriggered += OnInterlockTriggered;
                _monitor.SafetyStatusChanged += OnSafetyStatusChanged;

                IsMonitorRunning = true;
                UpdateOverallStatus(true, "监控中", Brushes.Green);
                AddLog("▶️ 后台安全监控已启动（周期 100ms）", "Info", Brushes.LightGreen);
                _logRepo?.WriteInfo("后台安全监控已启动", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 启动监控失败: {ex.Message}", "Error", Brushes.Red);
                _logRepo?.WriteError($"启动安全监控失败: {ex.Message}", "SafetyMonitor");
            }
        }

        private void ExecuteStopMonitor()
        {
            try
            {
                _monitor?.Stop();
                _monitor = null;
                IsMonitorRunning = false;
                UpdateOverallStatus(true, "已停止", Brushes.Orange);
                AddLog("⏹️ 后台安全监控已停止", "Info", Brushes.Orange);
                _logRepo?.WriteWarning("后台安全监控已停止", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"停止监控失败: {ex.Message}", "Error", Brushes.Red);
            }
        }

        private void ExecuteEmergencyStop()
        {
            try
            {
                _safeController?.EmergencyStop();
                _motionCard?.StopAll(true);
                UpdateOverallStatus(false, "急停", Brushes.Red);
                AddLog("🛑 急停按钮被按下！所有轴已停止", "Fatal", Brushes.Red);
                _logRepo?.WriteFatal("急停按钮被按下，所有轴已停止", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"急停执行异常: {ex.Message}", "Error", Brushes.Red);
                _logRepo?.WriteError($"急停执行异常: {ex.Message}", "SafetyMonitor");
            }
        }

        private void ExecuteAddVolume()
        {
            try
            {
                var volume = new SafetyVolume
                {
                    Name = $"禁区_{DateTime.Now:HHmmss}",
                    Type = VolumeType.Temporary,
                    BoundingBox = new BoundingBox
                    {
                        MinX = 200, MaxX = 300,
                        MinY = 100, MaxY = 200,
                        MinZ = 0, MaxZ = 50
                    },
                    SafetyMargin = 3.0
                };

                _safeController.CollisionDetector.RegisterVolume(volume);
                RefreshVolumes();
                AddLog($"➕ 添加临时禁区: {volume.Name}", "Info", Brushes.LightBlue);
                _logRepo?.WriteInfo($"添加安全体积: {volume.Name}", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"添加体积失败: {ex.Message}", "Error", Brushes.Red);
            }
        }

        private void ExecuteAddRule()
        {
            try
            {
                var rule = new InterlockRule
                {
                    Name = $"规则_{DateTime.Now:HHmmss}",
                    Condition = () => _motionCard.GetPosition(2) > 40,
                    Action = InterlockAction.AlarmOnly,
                    Message = "Z轴位置偏高报警"
                };

                _safeController.InterlockEngine.AddRule(rule);
                RefreshRules();
                AddLog($"➕ 添加互锁规则: {rule.Name} [AlarmOnly]", "Info", Brushes.LightBlue);
                _logRepo?.WriteInfo($"添加互锁规则: {rule.Name}", "SafetyMonitor");
            }
            catch (Exception ex)
            {
                AddLog($"添加规则失败: {ex.Message}", "Error", Brushes.Red);
            }
        }

        private void ExecuteCheckCollision()
        {
            try
            {
                if (_motionCard != null)
                {
                    _safeController.UpdateDynamicVolume("贴装头",
                        _motionCard.GetPosition(0),
                        _motionCard.GetPosition(1),
                        _motionCard.GetPosition(2));
                }

                var result = _safeController.CheckCollision();
                if (result.IsSafe)
                {
                    CollisionStatusText = "安全";
                    CollisionStatusBrush = Brushes.Green;
                    AddLog("✅ 手动检测：无碰撞风险", "Info", Brushes.LightGreen);
                }
                else
                {
                    CollisionStatusText = "碰撞!";
                    CollisionStatusBrush = Brushes.Red;
                    AddLog($"🚨 手动检测发现碰撞: {result.Message}", "Error", Brushes.Red);
                    _logRepo?.WriteError($"碰撞检测: {result.Message}", "SafetyMonitor");
                }
            }
            catch (Exception ex)
            {
                AddLog($"检测失败: {ex.Message}", "Error", Brushes.Red);
            }
        }

        private void ExecuteClearLog()
        {
            Logs.Clear();
        }

        #endregion

        #region 监控事件

        private void OnCollisionDetected(object sender, CollisionResult e)
        {
            InvokeOnUi(() =>
            {
                AddLog($"🚨 [碰撞检测] {e.Message}", "Error", Brushes.Red);
                if (e.AllCollisions != null)
                {
                    foreach (var c in e.AllCollisions)
                    {
                        AddLog($"   ↳ {c.A} ↔ {c.B}", "Error", Brushes.Red);
                    }
                }
                CollisionStatusText = "碰撞!";
                CollisionStatusBrush = Brushes.Red;
                UpdateOverallStatus(false, "异常", Brushes.Red);
                _logRepo?.WriteError($"碰撞检测: {e.Message}", "SafetyMonitor");
            });
        }

        private void OnInterlockTriggered(object sender, InterlockEvaluationResult e)
        {
            InvokeOnUi(() =>
            {
                AddLog($"⚠️ [互锁触发] {e.BlockReason}", "Warning", Brushes.Orange);
                AddLog($"   ↳ 建议动作: {e.RecommendedAction}", "Warning", Brushes.Orange);
                InterlockStatusText = "触发";
                InterlockStatusBrush = Brushes.Orange;
                _logRepo?.WriteWarning($"互锁触发: {e.BlockReason}", "SafetyMonitor");
            });
        }

        private void OnSafetyStatusChanged(object sender, bool isSafe)
        {
            InvokeOnUi(() =>
            {
                if (isSafe)
                {
                    UpdateOverallStatus(true, "监控中", Brushes.Green);
                    CollisionStatusText = "安全";
                    CollisionStatusBrush = Brushes.Green;
                    InterlockStatusText = "正常";
                    InterlockStatusBrush = Brushes.Green;
                }
                else
                {
                    UpdateOverallStatus(false, "异常", Brushes.Red);
                }
            });
        }

        #endregion

        #region UI 刷新定时器

        private void InitializeTimer()
        {
            _uiRefreshTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher);
            _uiRefreshTimer.Interval = TimeSpan.FromMilliseconds(200);
            _uiRefreshTimer.Tick += OnUiRefreshTick;
            _uiRefreshTimer.Start();
        }

        private void OnUiRefreshTick(object sender, EventArgs e)
        {
            if (!IsSafetyInitialized) return;
            RefreshDualHeadStatus();
        }

        #endregion

        #region 数据刷新

        private void RefreshVolumes()
        {
            if (_safeController?.CollisionDetector == null) return;
            Volumes.Clear();
            foreach (var v in _safeController.CollisionDetector.GetAllVolumes())
            {
                Volumes.Add(new SafetyVolumeViewModel { Volume = v });
            }
            VolumeCount = Volumes.Count;
        }

        private void RefreshRules()
        {
            if (_safeController?.InterlockEngine == null) return;
            Rules.Clear();
            foreach (var r in _safeController.InterlockEngine.GetAllRules())
            {
                Rules.Add(new InterlockRuleViewModel { Rule = r });
            }
            RuleCount = Rules.Count;
        }

        private void RefreshSoftLimits()
        {
            if (_safeController?.SoftLimitGuard == null) return;
            XLimitText = FormatLimit(_safeController.SoftLimitGuard.GetLimit(0));
            YLimitText = FormatLimit(_safeController.SoftLimitGuard.GetLimit(1));
            ZLimitText = FormatLimit(_safeController.SoftLimitGuard.GetLimit(2));
        }

        private void RefreshDualHeadConfig()
        {
            if (_dualHead == null) return;
            MinSeparationText = _dualHead.MinSeparation.ToString("F1");
            PreventCrossingText = _dualHead.PreventCrossing ? "是" : "否";
        }

        private void RefreshDualHeadStatus()
        {
            if (_dualHead == null || _motionCard == null) return;
            try
            {
                HeadAPosText = _motionCard.GetPosition(0).ToString("F2");
                HeadBPosText = _motionCard.GetPosition(1).ToString("F2");
                HeadSeparationText = _dualHead.CurrentSeparation.ToString("F2");
                HeadSeparationBrush = _dualHead.CurrentSeparation >= _dualHead.MinSeparation
                    ? Brushes.Green : Brushes.Red;
            }
            catch { }
        }

        private string FormatLimit(SoftLimitConfig limit)
        {
            if (limit == null || !limit.Enabled) return "未启用";
            return $"[{limit.NegativeLimit:F0}, {limit.PositiveLimit:F0}]";
        }

        #endregion

        #region 辅助方法

        private void UpdateOverallStatus(bool isSafe, string text, Brush brush)
        {
            OverallStatusText = text;
            OverallStatusBrush = brush;
        }

        private void AddLog(string message, string level, Brush brush)
        {
            InvokeOnUi(() =>
            {
                Logs.Add(new SafetyLogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Level = level,
                    MessageBrush = brush
                });
                LastEventTimeText = $"最后更新: {DateTime.Now:HH:mm:ss.fff}";
            });
        }

        private void InvokeOnUi(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }

        /// <summary>
        /// 清理资源（窗口关闭时调用）
        /// </summary>
        public void Cleanup()
        {
            _uiRefreshTimer?.Stop();
            _monitor?.Dispose();
            _monitor = null;
        }

        #endregion
    }
}
