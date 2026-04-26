using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CoreToolkit.Motion.Core;
using CoreToolkit.StateMachine.Core;
using CoreToolkit.StateMachine.Models;
using CoreToolkit.StateMachine.Modules;
using CoreToolkit.Motion.Factory;
using CoreToolkit.Data;
using CoreToolkit.Motion.Providers.Advantech;
using System.Diagnostics;

namespace MotionTest.WPF
{
    /// <summary>
    /// 流程控制窗口
    /// 测试状态机和轨迹显示功能
    /// </summary>
    public partial class FlowControlWindow : Window
    {
        private IMotionCard _motionCard;
        private IAxisGroup _axisGroup;
        private StateMachineManager _stateMachineManager;
        private StateMachine _currentMachine;
        private DispatcherTimer _statusTimer;
        private readonly List<FlowStatistics> _history = new List<FlowStatistics>();
        private readonly object _logLock = new object();

        // 模块树数据源
        private List<ModuleTreeItem> _moduleTreeItems = new List<ModuleTreeItem>();

        public FlowControlWindow()
        {
            InitializeComponent();
            Loaded += FlowControlWindow_Loaded;
            Closed += FlowControlWindow_Closed;
        }

        private void FlowControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeMotionCard();
            InitializeStateMachine();
            InitializeTimers();
            SetupEventHandlers();
        }

        private void FlowControlWindow_Closed(object sender, EventArgs e)
        {
            _statusTimer?.Stop();
            _currentMachine?.Stop();
            _motionCard?.Close();
        }

        #region 初始化

        private void InitializeMotionCard()
        {
            try
            {
                // 创建运动卡（使用模拟模式）
                _motionCard = MotionCardFactory.CreateCard("PCI-1285", 0);
                if (!_motionCard.IsOpen)
                {
                    _motionCard.Open();
                }

                // 创建XY轴组
                if (_motionCard is PCI1285 pci1285)
                {
                    _axisGroup = pci1285.CreateXYGroup(0, 1, "TestXY");
                    _axisGroup.Enable();
                }

                Log("运动卡初始化完成");
            }
            catch (Exception ex)
            {
                Log($"运动卡初始化失败: {ex.Message}");
                MessageBox.Show($"运动卡初始化失败: {ex.Message}\n将使用模拟模式", "警告", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InitializeStateMachine()
        {
            _stateMachineManager = new StateMachineManager();
            Log("状态机管理器初始化完成");
        }

        private void InitializeTimers()
        {
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
        }

        private void SetupEventHandlers()
        {
            sliderSpeed.ValueChanged += (s, e) =>
            {
                txtSpeedValue.Text = $"{(int)sliderSpeed.Value}%";
            };
        }

        #endregion

        #region 按钮事件

        private void btnCreateFlow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flowType = cmbFlowType.SelectedIndex;
                _currentMachine = CreateFlowMachine(flowType);

                // 更新UI
                UpdateModuleTree();
                UpdateStatusDisplay();
                Log($"创建流程: {cmbFlowType.Text}");
            }
            catch (Exception ex)
            {
                Log($"创建流程失败: {ex.Message}");
                MessageBox.Show($"创建流程失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMachine == null)
            {
                MessageBox.Show("请先创建流程", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 订阅事件
                _currentMachine.OnStatusChanged += Machine_OnStatusChanged;
                _currentMachine.OnModuleExecuting += Machine_OnModuleExecuting;
                _currentMachine.OnModuleCompleted += Machine_OnModuleCompleted;
                _currentMachine.OnTimeoutOccurred += Machine_OnTimeoutOccurred;

                // 重置并启动
                _currentMachine.Reset();
                var context = new CoreToolkit.StateMachine.Models.ExecutionContext();
                
                Log("启动流程...");
                var success = await _currentMachine.StartAsync(context);

                // 取消订阅
                _currentMachine.OnStatusChanged -= Machine_OnStatusChanged;
                _currentMachine.OnModuleExecuting -= Machine_OnModuleExecuting;
                _currentMachine.OnModuleCompleted -= Machine_OnModuleCompleted;
                _currentMachine.OnTimeoutOccurred -= Machine_OnTimeoutOccurred;

                // 显示结果
                if (success)
                {
                    Log("流程执行成功");
                    
                    // 获取轨迹记录
                    var trajectoryRecord = GetTrajectoryRecord();
                    if (trajectoryRecord != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            trajectoryChart.Record = trajectoryRecord;
                            UpdateStatistics(trajectoryRecord);
                        });
                    }
                }
                else
                {
                    Log($"流程执行失败: {_currentMachine.ErrorMessage}");
                }

                // 添加到历史记录
                AddToHistory(_currentMachine);
            }
            catch (Exception ex)
            {
                Log($"启动流程异常: {ex.Message}");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _currentMachine?.Stop();
            Log("停止流程");
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMachine?.Status == StateMachineStatus.Running)
            {
                _currentMachine.Pause();
                btnPause.Content = "恢复";
                Log("暂停流程");
            }
            else if (_currentMachine?.Status == StateMachineStatus.Paused)
            {
                _currentMachine.Resume();
                btnPause.Content = "暂停";
                Log("恢复流程");
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _currentMachine?.Reset();
            Log("重置流程");
        }

        private void lvHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvHistory.SelectedItem is FlowStatistics stats)
            {
                // 可以显示历史记录的详细信息
            }
        }

        #endregion

        #region 流程创建

        private StateMachine CreateFlowMachine(int flowType)
        {
            var machineName = $"Flow_{cmbFlowType.Text}_{DateTime.Now:HHmmss}";
            var machine = _stateMachineManager.CreateMachine(machineName, cmbFlowType.Text);
            var speedPercent = sliderSpeed.Value;
            var timeoutMs = int.Parse(txtTimeout.Text);

            switch (flowType)
            {
                case 0: // 单轴PTP运动
                    CreateSingleAxisPTPFlow(machine, speedPercent, timeoutMs);
                    break;
                case 1: // 双轴直线插补
                    CreateLinearInterpolationFlow(machine, speedPercent, timeoutMs);
                    break;
                case 2: // 双轴圆弧插补
                    CreateCircularInterpolationFlow(machine, speedPercent, timeoutMs);
                    break;
                case 3: // 多轴并行运动
                    CreateParallelMotionFlow(machine, speedPercent, timeoutMs);
                    break;
                case 4: // IO信号测试
                    CreateIOSignalFlow(machine, timeoutMs);
                    break;
                case 5: // 综合流程
                    CreateComplexFlow(machine, speedPercent, timeoutMs);
                    break;
            }

            return machine;
        }

        private void CreateSingleAxisPTPFlow(StateMachine machine, double speedPercent, int timeoutMs)
        {
            var sequential = new SequentialModule("单轴PTP流程");

            // 移动到起始位置
            var move1 = new AxisMoveModule(_motionCard, "移动到起始位置")
            {
                AxisIndices = new[] { 0 },
                TargetPositions = new[] { 0.0 },
                Velocities = new[] { 10000.0 * speedPercent / 100 },
                Accelerations = new[] { 100000.0 * speedPercent / 100 },
                Decelerations = new[] { 100000.0 * speedPercent / 100 },
                IsAbsolute = true,
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            // 延迟
            var delay1 = new DelayModule("到位稳定")
            {
                DelayMs = 500
            };

            // 移动到目标位置
            var move2 = new AxisMoveModule(_motionCard, "移动到目标位置")
            {
                AxisIndices = new[] { 0 },
                TargetPositions = new[] { 50000.0 },
                Velocities = new[] { 20000.0 * speedPercent / 100 },
                Accelerations = new[] { 100000.0 * speedPercent / 100 },
                Decelerations = new[] { 100000.0 * speedPercent / 100 },
                IsAbsolute = true,
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            // 到位检测
            var checkInPos = new InPositionModule(_motionCard, "到位检测")
            {
                Configs = new[]
                {
                    new InPositionConfig
                    {
                        AxisIndex = 0,
                        Tolerance = 100,
                        TimeoutMs = 5000,
                        StableCount = 3
                    }
                },
                TargetPositions = new[] { 50000.0 }
            };

            sequential.AddModule(move1);
            sequential.AddModule(delay1);
            sequential.AddModule(move2);
            sequential.AddModule(checkInPos);

            machine.SetRootModule(sequential);
        }

        private void CreateLinearInterpolationFlow(StateMachine machine, double speedPercent, int timeoutMs)
        {
            var sequential = new SequentialModule("直线插补流程");

            // 使用轴组进行直线插补
            var moveGroup = new AxisGroupMoveModule(_axisGroup, "直线插补运动")
            {
                TargetPositions = new[] { 30000.0, 20000.0 },
                Speed = 15000.0 * speedPercent / 100,
                Acceleration = 100000.0 * speedPercent / 100,
                Deceleration = 100000.0 * speedPercent / 100,
                MotionMode = MotionMode.LinearInterpolation,
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            sequential.AddModule(moveGroup);
            machine.SetRootModule(sequential);
        }

        private void CreateCircularInterpolationFlow(StateMachine machine, double speedPercent, int timeoutMs)
        {
            var sequential = new SequentialModule("圆弧插补流程");

            // 先移动到起始位置
            var moveStart = new AxisGroupMoveModule(_axisGroup, "移动到圆弧起点")
            {
                TargetPositions = new[] { 20000.0, 0.0 },
                Speed = 10000.0 * speedPercent / 100,
                Acceleration = 100000.0 * speedPercent / 100,
                Deceleration = 100000.0 * speedPercent / 100,
                MotionMode = MotionMode.LinearInterpolation,
                TimeoutMs = timeoutMs
            };

            // 圆弧插补
            var moveCircle = new AxisGroupMoveModule(_axisGroup, "圆弧插补运动")
            {
                TargetPositions = new[] { 0.0, 20000.0 },
                CenterPoint = new[] { 0.0, 0.0 },
                CircularDirection = 1, // 顺时针
                Speed = 8000.0 * speedPercent / 100,
                MotionMode = MotionMode.CircularInterpolation,
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            sequential.AddModule(moveStart);
            sequential.AddModule(moveCircle);
            machine.SetRootModule(sequential);
        }

        private void CreateParallelMotionFlow(StateMachine machine, double speedPercent, int timeoutMs)
        {
            var parallel = new ParallelModule("多轴并行运动");

            // 轴0运动
            var move0 = new AxisMoveModule(_motionCard, "轴0运动")
            {
                AxisIndices = new[] { 0 },
                TargetPositions = new[] { 40000.0 },
                Velocities = new[] { 15000.0 * speedPercent / 100 },
                Accelerations = new[] { 100000.0 * speedPercent / 100 },
                Decelerations = new[] { 100000.0 * speedPercent / 100 },
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            // 轴1运动
            var move1 = new AxisMoveModule(_motionCard, "轴1运动")
            {
                AxisIndices = new[] { 1 },
                TargetPositions = new[] { 30000.0 },
                Velocities = new[] { 12000.0 * speedPercent / 100 },
                Accelerations = new[] { 100000.0 * speedPercent / 100 },
                Decelerations = new[] { 100000.0 * speedPercent / 100 },
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            parallel.AddModule(move0);
            parallel.AddModule(move1);
            machine.SetRootModule(parallel);
        }

        private void CreateIOSignalFlow(StateMachine machine, int timeoutMs)
        {
            var sequential = new SequentialModule("IO信号测试流程");

            // 设置输出
            var setOutput = new CustomActionModule("设置DO0=ON")
            {
                TimeoutMs = timeoutMs
            };
            setOutput.SetAction(ctx =>
            {
                _motionCard.WriteOutput(0, true);
                Log("设置DO0 = ON");
            });

            // 等待延迟
            var delay = new DelayModule("等待500ms")
            {
                DelayMs = 500
            };

            // 清除输出
            var clearOutput = new CustomActionModule("设置DO0=OFF")
            {
                TimeoutMs = timeoutMs
            };
            clearOutput.SetAction(ctx =>
            {
                _motionCard.WriteOutput(0, false);
                Log("设置DO0 = OFF");
            });

            sequential.AddModule(setOutput);
            sequential.AddModule(delay);
            sequential.AddModule(clearOutput);
            machine.SetRootModule(sequential);
        }

        private void CreateComplexFlow(StateMachine machine, double speedPercent, int timeoutMs)
        {
            var sequential = new SequentialModule("综合测试流程");

            // 1. 初始化：所有轴归零
            var homingSeq = new ParallelModule("轴归零");
            for (int i = 0; i < 2; i++)
            {
                int axisIdx = i;
                var homeAction = new CustomActionModule($"轴{axisIdx}归零")
                {
                    TimeoutMs = timeoutMs
                };
                homeAction.SetAction(ctx =>
                {
                    _motionCard.Home(axisIdx, 10000); // 使用单一速度参数
                    Log($"轴{axisIdx}归零完成");
                });
                homingSeq.AddModule(homeAction);
            }

            // 2. 直线插补
            var linearMove = new AxisGroupMoveModule(_axisGroup, "直线插补到工作位置")
            {
                TargetPositions = new[] { 25000.0, 15000.0 },
                Speed = 12000.0 * speedPercent / 100,
                Acceleration = 100000.0 * speedPercent / 100,
                Deceleration = 100000.0 * speedPercent / 100,
                MotionMode = MotionMode.LinearInterpolation,
                TimeoutMs = timeoutMs,
                EnableTrajectoryRecording = true
            };

            // 3. 等待到位 + 设置IO（并行）
            var parallelOps = new ParallelModule("到位检测和IO操作");
            
            var inPosCheck = new InPositionModule(_motionCard, "到位检测")
            {
                Configs = new[]
                {
                    new InPositionConfig { AxisIndex = 0, TimeoutMs = 5000 },
                    new InPositionConfig { AxisIndex = 1, TimeoutMs = 5000 }
                }
            };

            var ioOperation = new SequentialModule("IO操作序列");
            ioOperation.AddModule(new DelayModule("等待200ms") { DelayMs = 200 });
            ioOperation.AddModule(new CustomActionModule("设置DO1=ON")
            {
                TimeoutMs = timeoutMs
            }.Also(m => m.SetAction(ctx => _motionCard.WriteOutput(1, true))));
            ioOperation.AddModule(new DelayModule("保持500ms") { DelayMs = 500 });
            ioOperation.AddModule(new CustomActionModule("设置DO1=OFF")
            {
                TimeoutMs = timeoutMs
            }.Also(m => m.SetAction(ctx => _motionCard.WriteOutput(1, false))));

            parallelOps.AddModule(inPosCheck);
            parallelOps.AddModule(ioOperation);

            // 组装流程
            sequential.AddModule(homingSeq);
            sequential.AddModule(linearMove);
            sequential.AddModule(parallelOps);

            machine.SetRootModule(sequential);
        }

        #endregion

        #region 事件处理

        private void Machine_OnStatusChanged(object sender, StateMachineEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateStatusDisplay();
                Log($"状态机状态: {e.OldStatus} -> {e.NewStatus}");
            });
        }

        private void Machine_OnModuleExecuting(object sender, ModuleEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateModuleStatus(e.ModuleId, ModuleStatus.Running);
                Log($"开始执行: {e.ModuleName}");
            });
        }

        private void Machine_OnModuleCompleted(object sender, ModuleEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateModuleStatus(e.ModuleId, e.NewStatus);
                Log($"执行完成: {e.ModuleName} - {e.NewStatus} (耗时: {e.DurationMs:F0}ms)");
            });
        }

        private void Machine_OnTimeoutOccurred(object sender, TimeoutEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Log($"超时: {e.ModuleName} - {e.Type} (设置:{e.Timeout.TotalMilliseconds:F0}ms, 实际:{e.ActualDuration.TotalMilliseconds:F0}ms)");
            });
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (_currentMachine?.Status == StateMachineStatus.Running)
            {
                txtExecutionTime.Text = $"{_currentMachine.ExecutionTime.TotalMilliseconds:F0} ms";
            }
        }

        #endregion

        #region UI更新

        private void UpdateStatusDisplay()
        {
            if (_currentMachine == null) return;

            var status = _currentMachine.Status;
            txtMachineStatus.Text = status.ToString();

            borderStatus.Background = status switch
            {
                StateMachineStatus.Running => Brushes.Green,
                StateMachineStatus.Paused => Brushes.Orange,
                StateMachineStatus.Completed => Brushes.Blue,
                StateMachineStatus.Error => Brushes.Red,
                StateMachineStatus.Stopping => Brushes.Yellow,
                _ => Brushes.Gray
            };
        }

        private void UpdateModuleTree()
        {
            _moduleTreeItems.Clear();
            
            if (_currentMachine?.Modules != null)
            {
                foreach (var module in _currentMachine.Modules)
                {
                    _moduleTreeItems.Add(CreateTreeItem(module));
                }
            }

            treeModules.ItemsSource = null;
            treeModules.ItemsSource = _moduleTreeItems;
        }

        private ModuleTreeItem CreateTreeItem(IFlowModule module)
        {
            var item = new ModuleTreeItem
            {
                Id = module.ModuleId,
                Name = module.Name,
                Type = module.Type,
                Status = module.Status
            };

            // 递归添加子模块
            if (module is SequentialModule seq)
            {
                foreach (var child in seq.Modules)
                {
                    item.Children.Add(CreateTreeItem(child));
                }
            }
            else if (module is ParallelModule par)
            {
                foreach (var child in par.Modules)
                {
                    item.Children.Add(CreateTreeItem(child));
                }
            }

            return item;
        }

        private void UpdateModuleStatus(string moduleId, ModuleStatus status)
        {
            var item = FindTreeItem(_moduleTreeItems, moduleId);
            if (item != null)
            {
                item.Status = status;
                RefreshTreeView();
            }
        }

        private ModuleTreeItem FindTreeItem(List<ModuleTreeItem> items, string id)
        {
            foreach (var item in items)
            {
                if (item.Id == id) return item;
                var found = FindTreeItem(item.Children, id);
                if (found != null) return found;
            }
            return null;
        }

        private void RefreshTreeView()
        {
            // 简单的刷新方式
            var selected = treeModules.SelectedItem;
            treeModules.ItemsSource = null;
            treeModules.ItemsSource = _moduleTreeItems;
            
            // 恢复选中状态
            if (selected != null)
            {
                // 查找并选中之前选中的项
            }
        }

        private void UpdateStatistics(TrajectoryRecord record)
        {
            if (record == null || record.Points.Count == 0) return;

            statDuration.Text = $"{record.Points.Last().ElapsedMs:F1} ms";
            statPointCount.Text = record.Points.Count.ToString();
            statAxisCount.Text = record.AxisCount.ToString();

            // 计算最大速度
            double maxVel = 0;
            foreach (var point in record.Points)
            {
                if (point.Velocities != null)
                {
                    foreach (var vel in point.Velocities)
                    {
                        maxVel = Math.Max(maxVel, Math.Abs(vel));
                    }
                }
            }
            statMaxVel.Text = $"{maxVel:F2}";

            // 计算行程距离
            double distance = 0;
            if (record.Points.Count > 1)
            {
                var first = record.Points.First();
                var last = record.Points.Last();
                if (first.Positions != null && last.Positions != null && first.Positions.Length > 0)
                {
                    for (int i = 0; i < first.Positions.Length && i < last.Positions.Length; i++)
                    {
                        distance += Math.Abs(last.Positions[i] - first.Positions[i]);
                    }
                }
            }
            statDistance.Text = $"{distance:F2}";

            // 到位精度（最后一点的偏差）
            statAccuracy.Text = "-";
        }

        private void AddToHistory(StateMachine machine)
        {
            var stats = new FlowStatistics
            {
                FlowName = machine.Name,
                StartTime = machine.StartTime,
                EndTime = DateTime.Now,
                IsSuccess = machine.Status == StateMachineStatus.Completed,
                ErrorMessage = machine.ErrorMessage
            };

            _history.Add(stats);
            lvHistory.ItemsSource = null;
            lvHistory.ItemsSource = _history.OrderByDescending(h => h.StartTime).ToList();
        }

        #endregion

        #region 辅助方法

        private TrajectoryRecord GetTrajectoryRecord()
        {
            // 从状态机中获取轨迹记录
            if (_currentMachine?.Modules != null)
            {
                foreach (var module in _currentMachine.Modules)
                {
                    if (module is ITrajectoryRecordable rec && rec.TrajectoryRecord != null)
                    {
                        return rec.TrajectoryRecord;
                    }

                    // 递归检查子模块
                    if (module is SequentialModule seq)
                    {
                        foreach (var child in seq.Modules)
                        {
                            if (child is ITrajectoryRecordable childRec && childRec.TrajectoryRecord != null)
                            {
                                return childRec.TrajectoryRecord;
                            }
                        }
                    }
                    else if (module is ParallelModule par)
                    {
                        foreach (var child in par.Modules)
                        {
                            if (child is ITrajectoryRecordable childRec && childRec.TrajectoryRecord != null)
                            {
                                return childRec.TrajectoryRecord;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void Log(string message)
        {
            lock (_logLock)
            {
                Dispatcher.Invoke(() =>
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
                    txtLog.ScrollToEnd();
                });
            }
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// 模块树节点
    /// </summary>
    public class ModuleTreeItem : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ModuleType Type { get; set; }
        
        private ModuleStatus _status;
        public ModuleStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColor)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontWeight)));
            }
        }

        public Brush StatusColor => Status switch
        {
            ModuleStatus.Running => Brushes.Green,
            ModuleStatus.Completed => Brushes.Blue,
            ModuleStatus.Failed => Brushes.Red,
            ModuleStatus.Timeout => Brushes.Orange,
            ModuleStatus.Waiting => Brushes.Yellow,
            ModuleStatus.Cancelled => Brushes.Gray,
            _ => Brushes.LightGray
        };

        public string StatusText => Status != ModuleStatus.Pending ? $"({Status})" : "";

        public System.Windows.FontWeight FontWeight => 
            Status == ModuleStatus.Running ? FontWeights.Bold : FontWeights.Normal;

        public List<ModuleTreeItem> Children { get; set; } = new List<ModuleTreeItem>();

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 布尔值转画刷转换器
    /// </summary>
    public class BoolToBrushConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        public static T Also<T>(this T obj, System.Action<T> action)
        {
            action(obj);
            return obj;
        }
    }

    #endregion
}
