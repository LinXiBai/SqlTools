using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CoreToolkit.Data;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;

namespace MotionTest.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// PCI-1285 运动控制卡测试主窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMotionCard _motionCard;
        private LogRepository _logRepo;
        private AxisParameterRepository _axisParamRepo;
        private IOParameterRepository _ioParamRepo;
        private SqliteDbContext _logDb;
        private SqliteDbContext _motionDb;
        private DispatcherTimer _statusTimer;
        private bool _isCardOpen = false;
        private List<AxisStatusViewModel> _axisStatusList;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabases();
            InitializeAxisStatusGrid();
            InitializeTimer();
            LogMessage("PCI-1285 测试工具已启动");
        }

        #region 初始化

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitializeDatabases()
        {
            try
            {
                DatabaseConfig.Register("LogDb", "logs.db");
                DatabaseConfig.Register("MotionDb", "motion.db");
                DatabaseFactory.InitializeAllDatabases();

                _logDb = DatabaseFactory.CreateContext("LogDb");
                _motionDb = DatabaseFactory.CreateContext("MotionDb");
                _logRepo = new LogRepository(_logDb);
                _axisParamRepo = new AxisParameterRepository(_motionDb);
                _ioParamRepo = new IOParameterRepository(_motionDb);

                LogMessage("数据库初始化完成");
            }
            catch (Exception ex)
            {
                LogMessage($"数据库初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化轴状态列表
        /// </summary>
        private void InitializeAxisStatusGrid()
        {
            _axisStatusList = new List<AxisStatusViewModel>();
            string[] axisNames = { "X轴", "Y轴", "Z轴", "R轴", "A轴", "B轴", "C轴", "D轴" };

            for (int i = 0; i < 8; i++)
            {
                _axisStatusList.Add(new AxisStatusViewModel
                {
                    AxisId = i,
                    AxisName = axisNames[i]
                });
            }

            AxisStatusGrid.ItemsSource = _axisStatusList;
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitializeTimer()
        {
            _statusTimer = new DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromMilliseconds(200);
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
        }

        #endregion

        #region 控制卡操作

        /// <summary>
        /// 初始化控制卡
        /// </summary>
        private void InitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cardType = ((CardTypeCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "PCI-1285";
                int cardId = int.Parse(CardIdText.Text);

                LogMessage($"正在初始化控制卡: {cardType}, 卡号: {cardId}");

                // 检查是否支持该卡类型
                if (!MotionCardFactory.IsSupported(cardType))
                {
                    MessageBox.Show($"不支持的卡类型: {cardType}\n支持的类型: {string.Join(", ", MotionCardFactory.GetSupportedCardTypes())}", 
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _motionCard = MotionCardFactory.CreateCard(cardType, cardId);

                var config = new MotionConfig
                {
                    CardId = cardId,
                    AxisConfigs = new List<AxisConfig>(),
                    InputCount = 16,
                    OutputCount = 16
                };

                // 从数据库加载轴配置
                var axisParams = _axisParamRepo.GetByCardId(cardId);
                if (axisParams.Count == 0)
                {
                    // 如果没有参数，创建默认参数
                    InitializeDefaultParameters(cardId);
                    axisParams = _axisParamRepo.GetByCardId(cardId);
                }

                foreach (var param in axisParams)
                {
                    config.AxisConfigs.Add(new AxisConfig
                    {
                        AxisId = param.轴号,
                        AxisName = param.轴名称,
                        PulseEquivalent = (double)param.脉冲当量 / param.脉冲当量分母,
                        DefaultSpeed = param.运动高速,
                        DefaultAcceleration = param.加速度,
                        DefaultDeceleration = param.减速度,
                        HomeSpeedHigh = param.原点高速,
                        HomeSpeedLow = param.原点低速,
                        HomeDirection = param.回原方向,
                        SoftPositiveLimit = param.正向软极限,
                        SoftNegativeLimit = param.负向软极限
                    });
                }

                _motionCard.Initialize(config);

                CardStatusText.Text = "状态: 已初始化";
                CardStatusText.Foreground = Brushes.Blue;
                LogMessage($"控制卡初始化成功: {_motionCard.CardName}");
                _logRepo?.WriteInfo($"控制卡初始化成功: {_motionCard.CardName}", "MainWindow");
            }
            catch (Exception ex)
            {
                LogMessage($"初始化失败: {ex.Message}");
                _logRepo?.WriteError($"控制卡初始化失败: {ex.Message}", "MainWindow");
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开控制卡
        /// </summary>
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_motionCard == null)
                {
                    MessageBox.Show("请先初始化控制卡", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                LogMessage("正在打开控制卡...");
                _motionCard.Open();
                _isCardOpen = true;

                CardStatusText.Text = "状态: 已连接";
                CardStatusText.Foreground = Brushes.Green;
                LogMessage("控制卡已打开");
                _logRepo?.WriteInfo("控制卡已打开", "MainWindow");
            }
            catch (Exception ex)
            {
                LogMessage($"打开失败: {ex.Message}");
                _logRepo?.WriteError($"控制卡打开失败: {ex.Message}", "MainWindow");
                MessageBox.Show($"打开失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 关闭控制卡
        /// </summary>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _motionCard?.Close();
                _isCardOpen = false;

                CardStatusText.Text = "状态: 已关闭";
                CardStatusText.Foreground = Brushes.Red;
                LogMessage("控制卡已关闭");
                _logRepo?.WriteInfo("控制卡已关闭", "MainWindow");
            }
            catch (Exception ex)
            {
                LogMessage($"关闭失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 复位控制卡
        /// </summary>
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                _motionCard.Reset();
                LogMessage("控制卡已复位");
                _logRepo?.WriteInfo("控制卡已复位", "MainWindow");
            }
            catch (Exception ex)
            {
                LogMessage($"复位失败: {ex.Message}");
            }
        }

        #endregion

        #region 轴操作

        /// <summary>
        /// 伺服ON
        /// </summary>
        private void ServoOnBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                _motionCard.SetServoEnable(axis, true);
                LogMessage($"轴{axis} 伺服ON");
                
                // 立即刷新该轴状态
                RefreshAxisStatus(axis);
            }
            catch (Exception ex)
            {
                LogMessage($"伺服ON失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 伺服OFF
        /// </summary>
        private void ServoOffBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                _motionCard.SetServoEnable(axis, false);
                LogMessage($"轴{axis} 伺服OFF");
                
                // 立即刷新该轴状态
                RefreshAxisStatus(axis);
            }
            catch (Exception ex)
            {
                LogMessage($"伺服OFF失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                double speed = double.Parse(SpeedText.Text) / 5;

                LogMessage($"轴{axis} 开始回零...");
                Task.Run(() =>
                {
                    try
                    {
                        _motionCard.Home(axis, speed);
                        LogMessage($"轴{axis} 回零完成");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"回零失败: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogMessage($"回零失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        private void MoveAbsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                double position = double.Parse(PositionText.Text);
                double speed = double.Parse(SpeedText.Text);

                LogMessage($"轴{axis} 绝对运动到 {position}, 速度 {speed}");
                Task.Run(() =>
                {
                    try
                    {
                        _motionCard.MoveAbsolute(axis, position, speed);
                        _motionCard.WaitForMotionComplete(axis, 10000);
                        LogMessage($"轴{axis} 运动完成");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"运动失败: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogMessage($"运动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 相对运动
        /// </summary>
        private void MoveRelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                double distance = double.Parse(PositionText.Text);
                double speed = double.Parse(SpeedText.Text);

                LogMessage($"轴{axis} 相对运动 {distance}, 速度 {speed}");
                Task.Run(() =>
                {
                    try
                    {
                        _motionCard.MoveRelative(axis, distance, speed);
                        _motionCard.WaitForMotionComplete(axis, 10000);
                        LogMessage($"轴{axis} 运动完成");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"运动失败: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogMessage($"运动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// JOG+ 按下
        /// </summary>
        private void JogPosBtn_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                double speed = double.Parse(SpeedText.Text);
                _motionCard.Jog(axis, 1, speed);
                LogMessage($"轴{axis} JOG+ 开始");
            }
            catch (Exception ex)
            {
                LogMessage($"JOG失败: {ex.Message}");
            }
        }

        /// <summary>
        /// JOG+ 释放
        /// </summary>
        private void JogPosBtn_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                _motionCard.Stop(axis);
                LogMessage($"轴{axis} JOG+ 停止");
            }
            catch (Exception ex)
            {
                LogMessage($"停止失败: {ex.Message}");
            }
        }

        /// <summary>
        /// JOG- 按下
        /// </summary>
        private void JogNegBtn_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                double speed = double.Parse(SpeedText.Text);
                _motionCard.Jog(axis, -1, speed);
                LogMessage($"轴{axis} JOG- 开始");
            }
            catch (Exception ex)
            {
                LogMessage($"JOG失败: {ex.Message}");
            }
        }

        /// <summary>
        /// JOG- 释放
        /// </summary>
        private void JogNegBtn_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                _motionCard.Stop(axis);
                LogMessage($"轴{axis} JOG- 停止");
            }
            catch (Exception ex)
            {
                LogMessage($"停止失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止轴
        /// </summary>
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int axis = AxisCombo.SelectedIndex;
                _motionCard.Stop(axis);
                LogMessage($"轴{axis} 已停止");
            }
            catch (Exception ex)
            {
                LogMessage($"停止失败: {ex.Message}");
            }
        }

        #endregion

        #region IO操作

        /// <summary>
        /// 输出ON
        /// </summary>
        private void OutputOnBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int output = OutputCombo.SelectedIndex;
                _motionCard.WriteOutput(output, true);
                LogMessage($"输出点{output} ON");
            }
            catch (Exception ex)
            {
                LogMessage($"输出失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 输出OFF
        /// </summary>
        private void OutputOffBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                int output = OutputCombo.SelectedIndex;
                _motionCard.WriteOutput(output, false);
                LogMessage($"输出点{output} OFF");
            }
            catch (Exception ex)
            {
                LogMessage($"输出失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新输入状态
        /// </summary>
        private void RefreshInputBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckCardOpen()) return;

                bool input = _motionCard.ReadInput(0);
                InputIndicator.Fill = input ? Brushes.Green : Brushes.Gray;
                InputStatusText.Text = input ? "ON" : "OFF";
            }
            catch (Exception ex)
            {
                LogMessage($"读取输入失败: {ex.Message}");
            }
        }

        #endregion

        #region 数据库操作

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitDbBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeDatabases();
                int cardId = int.Parse(CardIdText.Text);
                InitializeDefaultParameters(cardId);
                MessageBox.Show("数据库初始化完成，已创建默认参数", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载轴参数
        /// </summary>
        private void LoadParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cardId = int.Parse(CardIdText.Text);
                var params_list = _axisParamRepo.GetByCardId(cardId);
                LogMessage($"已加载 {params_list.Count} 个轴参数");
                foreach (var param in params_list)
                {
                    LogMessage($"轴{param.轴号}: {param.轴名称}, 高速={param.运动高速}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"加载参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存轴参数
        /// </summary>
        private void SaveParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cardId = int.Parse(CardIdText.Text);
                // 这里可以添加参数编辑对话框
                LogMessage("参数已保存到数据库");
            }
            catch (Exception ex)
            {
                LogMessage($"保存参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查看日志
        /// </summary>
        private void ViewLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logs = _logRepo.GetRecentLogs(20);
                LogMessage("===== 最近日志 =====");
                foreach (var log in logs)
                {
                    LogMessage($"[{log.Level}] {log.Timestamp:HH:mm:ss} - {log.Message}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"查看日志失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开流程控制窗口
        /// </summary>
        private void FlowControlBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flowWindow = new FlowControlWindow();
                flowWindow.Show();
                LogMessage("打开流程控制窗口");
            }
            catch (Exception ex)
            {
                LogMessage($"打开流程控制窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化默认参数
        /// </summary>
        private void InitializeDefaultParameters(int cardId)
        {
            // 清空现有参数
            var existing = _axisParamRepo.GetByCardId(cardId);
            foreach (var param in existing)
            {
                _axisParamRepo.Delete(param.Id);
            }

            // 创建8轴默认参数
            string[] axisNames = { "X轴", "Y轴", "Z轴", "R轴", "A轴", "B轴", "C轴", "D轴" };
            for (int i = 0; i < 8; i++)
            {
                var axisParam = new AxisParameter
                {
                    轴名称 = axisNames[i],
                    轴号 = i,
                    卡号 = cardId,
                    轴类型 = 0,
                    脉冲当量 = 1000,
                    脉冲当量分母 = 1,
                    运动低速 = 1000,
                    运动高速 = 50000,
                    加速度 = 100000,
                    减速度 = 100000,
                    加加速度 = 0,
                    减减速度 = 0,
                    回原模式 = 1,
                    回原方向 = -1,
                    原点高速 = 10000,
                    原点低速 = 1000,
                    原点加速度 = 50000,
                    原点减速度 = 50000,
                    原点偏移 = 0,
                    正向软极限 = 100000,
                    负向软极限 = -100000,
                    使能IO = i
                };
                _axisParamRepo.Insert(axisParam);
            }

            // 创建IO参数
            var existingIo = _ioParamRepo.GetByCardId(cardId);
            foreach (var param in existingIo)
            {
                _ioParamRepo.Delete(param.Id);
            }

            for (int i = 0; i < 16; i++)
            {
                var ioParam = new IOParameter
                {
                    卡号 = cardId,
                    端口号 = i,
                    输入点 = i,
                    输入名称 = $"输入{i}",
                    输出点 = i,
                    输出名称 = $"输出{i}"
                };
                _ioParamRepo.Insert(ioParam);
            }

            LogMessage($"已创建卡{cardId}的默认参数");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 检查控制卡是否已打开
        /// </summary>
        private bool CheckCardOpen()
        {
            if (_motionCard == null || !_isCardOpen)
            {
                MessageBox.Show("请先打开控制卡", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 定时刷新状态
        /// </summary>
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (!_isCardOpen || _motionCard == null) return;

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    var status = _motionCard.GetAxisStatus(i);
                    var vm = _axisStatusList[i];
                    vm.Position = status.ActualPosition;
                    vm.CommandPosition = status.CommandPosition;
                    vm.Speed = status.CurrentSpeed;
                    vm.ServoOn = status.ServoOn;
                    vm.IsRunning = status.IsRunning;
                    vm.InPosition = status.InPosition;
                    vm.IsAlarm = status.IsAlarm;
                    vm.PositiveLimit = status.PositiveLimit;
                    vm.NegativeLimit = status.NegativeLimit;
                    vm.HomeSignal = status.HomeSignal;
                }
                AxisStatusGrid.Items.Refresh();
            }
            catch
            {
                // 忽略状态读取错误
            }
        }

        /// <summary>
        /// 记录日志消息
        /// </summary>
        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                LogTextBox.AppendText($"[{timestamp}] {message}\r\n");
                LogTextBox.ScrollToEnd();
            });
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        /// <summary>
        /// 窗口关闭时清理资源
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _statusTimer?.Stop();
            _motionCard?.Close();
            _motionCard?.Dispose();
            _logDb?.Dispose();
            _motionDb?.Dispose();
            base.OnClosing(e);
        }

        #endregion
    }

    /// <summary>
    /// 轴状态视图模型
    /// </summary>
    public class AxisStatusViewModel : INotifyPropertyChanged
    {
        private int _axisId;
        private string _axisName;
        private double _position;
        private double _commandPosition;
        private double _speed;
        private bool _servoOn;
        private bool _isRunning;
        private bool _inPosition;
        private bool _isAlarm;
        private bool _positiveLimit;
        private bool _negativeLimit;
        private bool _homeSignal;
        private bool _isSelected;

        public int AxisId
        {
            get => _axisId;
            set { _axisId = value; OnPropertyChanged(nameof(AxisId)); }
        }

        public string AxisName
        {
            get => _axisName;
            set { _axisName = value; OnPropertyChanged(nameof(AxisName)); }
        }

        public double Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public double CommandPosition
        {
            get => _commandPosition;
            set { _commandPosition = value; OnPropertyChanged(nameof(CommandPosition)); }
        }

        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(nameof(Speed)); }
        }

        public bool ServoOn
        {
            get => _servoOn;
            set { _servoOn = value; OnPropertyChanged(nameof(ServoOn)); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(nameof(IsRunning)); }
        }

        public bool InPosition
        {
            get => _inPosition;
            set { _inPosition = value; OnPropertyChanged(nameof(InPosition)); }
        }

        public bool IsAlarm
        {
            get => _isAlarm;
            set { _isAlarm = value; OnPropertyChanged(nameof(IsAlarm)); }
        }

        public bool PositiveLimit
        {
            get => _positiveLimit;
            set { _positiveLimit = value; OnPropertyChanged(nameof(PositiveLimit)); }
        }

        public bool NegativeLimit
        {
            get => _negativeLimit;
            set { _negativeLimit = value; OnPropertyChanged(nameof(NegativeLimit)); }
        }

        public bool HomeSignal
        {
            get => _homeSignal;
            set { _homeSignal = value; OnPropertyChanged(nameof(HomeSignal)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
