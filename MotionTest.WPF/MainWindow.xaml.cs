using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MotionLib.Core;
using MotionLib.Interfaces;
using MotionLib.Providers.Advantech;

namespace MotionTest.WPF
{
    public partial class MainWindow : Window
    {
        private IMotionCard _motionCard;
        private DispatcherTimer _statusTimer;
        private List<AxisControl> _axisControls = new List<AxisControl>();
        private List<Button> _inputButtons = new List<Button>();
        private List<Button> _outputButtons = new List<Button>();
        private int _axisCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeIOControls();
            InitializeTimer();
        }

        private void InitializeIOControls()
        {
            // 创建16个输入指示灯
            for (int i = 0; i < 16; i++)
            {
                var btn = new Button
                {
                    Content = $"DI{i}",
                    Width = 50,
                    Height = 30,
                    Margin = new Thickness(2),
                    Background = Brushes.Gray,
                    IsEnabled = false
                };
                ugInputs.Children.Add(btn);
                _inputButtons.Add(btn);
            }

            // 创建16个输出按钮
            for (int i = 0; i < 16; i++)
            {
                int index = i;
                var btn = new Button
                {
                    Content = $"DO{i}",
                    Width = 50,
                    Height = 30,
                    Margin = new Thickness(2),
                    Background = Brushes.Gray
                };
                btn.Click += (s, e) => ToggleOutput(index);
                ugOutputs.Children.Add(btn);
                _outputButtons.Add(btn);
            }
        }

        private void InitializeAxisControls()
        {
            // 清除现有的轴控制
            spAxes.Children.Clear();
            _axisControls.Clear();

            // 根据实际轴数创建轴控制界面
            for (int i = 0; i < _axisCount; i++)
            {
                var axisControl = new AxisControl(i);
                axisControl.AxisAction += OnAxisAction;
                spAxes.Children.Add(axisControl);
                _axisControls.Add(axisControl);
            }

            Log($"已创建 {_axisCount} 个轴的控制界面");
        }

        private void InitializeTimer()
        {
            _statusTimer = new DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromMilliseconds(100);
            _statusTimer.Tick += StatusTimer_Tick;
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (_motionCard == null || !_motionCard.IsOpen) return;

            try
            {
                // 更新轴状态
                for (int i = 0; i < _axisCount; i++)
                {
                    var status = _motionCard.GetAxisStatus(i);
                    _axisControls[i].UpdateStatus(status);
                }

                // 更新输入状态
                for (int i = 0; i < 16; i++)
                {
                    bool value = _motionCard.ReadInput(i);
                    _inputButtons[i].Background = value ? Brushes.Lime : Brushes.Gray;
                }
            }
            catch (Exception ex)
            {
                Log($"状态更新错误: {ex.Message}");
            }
        }

        private void OnAxisAction(int axis, string action, object parameter)
        {
            if (_motionCard == null || !_motionCard.IsOpen)
            {
                MessageBox.Show("请先打开控制卡！");
                return;
            }

            try
            {
                switch (action)
                {
                    case "ServoOn":
                        _motionCard.SetServoEnable(axis, true);
                        Log($"轴{axis} 伺服使能");
                        break;
                    case "ServoOff":
                        _motionCard.SetServoEnable(axis, false);
                        Log($"轴{axis} 伺服关闭");
                        break;
                    case "Home":
                        double homeSpeed = Convert.ToDouble(parameter);
                        _motionCard.Home(axis, homeSpeed);
                        Log($"轴{axis} 开始回零，速度: {homeSpeed}");
                        break;
                    case "MoveAbs":
                        var absParams = (Tuple<double, double>)parameter;
                        _motionCard.MoveAbsolute(axis, absParams.Item1, absParams.Item2);
                        Log($"轴{axis} 绝对运动: 位置={absParams.Item1}, 速度={absParams.Item2}");
                        break;
                    case "MoveRel":
                        var relParams = (Tuple<double, double>)parameter;
                        _motionCard.MoveRelative(axis, relParams.Item1, relParams.Item2);
                        Log($"轴{axis} 相对运动: 距离={relParams.Item1}, 速度={relParams.Item2}");
                        break;
                    case "Jog+":
                        double jogSpeedPos = Convert.ToDouble(parameter);
                        _motionCard.Jog(axis, 1, jogSpeedPos);
                        Log($"轴{axis} JOG+ 速度: {jogSpeedPos}");
                        break;
                    case "Jog-":
                        double jogSpeedNeg = Convert.ToDouble(parameter);
                        _motionCard.Jog(axis, -1, jogSpeedNeg);
                        Log($"轴{axis} JOG- 速度: {jogSpeedNeg}");
                        break;
                    case "Stop":
                        _motionCard.Stop(axis, false);
                        Log($"轴{axis} 停止");
                        break;
                    case "EmgStop":
                        _motionCard.Stop(axis, true);
                        Log($"轴{axis} 急停");
                        break;
                    case "SetPosition":
                        double pos = Convert.ToDouble(parameter);
                        _motionCard.SetPosition(axis, pos);
                        Log($"轴{axis} 设置位置: {pos}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"轴{axis} 操作失败: {ex.Message}");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
        }

        private void ToggleOutput(int index)
        {
            if (_motionCard == null || !_motionCard.IsOpen) return;

            try
            {
                bool currentValue = _motionCard.ReadOutput(index);
                _motionCard.WriteOutput(index, !currentValue);
                _outputButtons[index].Background = !currentValue ? Brushes.Red : Brushes.Gray;
                Log($"输出{index} 设置为 {!currentValue}");
            }
            catch (Exception ex)
            {
                Log($"输出操作失败: {ex.Message}");
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cardId = int.Parse(((ComboBoxItem)cmbCardId.SelectedItem).Content.ToString());
                int cardTypeIndex = cmbCardType.SelectedIndex;

                // 根据选择的卡类型创建对应的实例
                switch (cardTypeIndex)
                {
                    case 0: // PCI-1285
                        _motionCard = new PCI1285();
                        break;
                    case 1: // PCI-1203 (16轴)
                        _motionCard = new PCI1203(16);
                        break;
                    case 2: // PCI-1203 (32轴)
                        _motionCard = new PCI1203(32);
                        break;
                    default:
                        _motionCard = new PCI1285();
                        break;
                }

                var config = new MotionConfig { CardId = cardId };
                _motionCard.Initialize(config);

                // 获取实际轴数并创建界面
                _axisCount = _motionCard.AxisCount;
                InitializeAxisControls();

                // 更新标题和状态
                txtTitle.Text = $"{_motionCard.Vendor} {_motionCard.Model} 运动控制卡测试";
                txtStatus.Text = $"状态: 已初始化 ({_axisCount}轴)";
                txtStatus.Foreground = Brushes.Blue;
                btnInit.IsEnabled = false;
                cmbCardType.IsEnabled = false;
                cmbCardId.IsEnabled = false;
                btnOpen.IsEnabled = true;
                Log($"控制卡初始化成功，卡号: {cardId}，型号: {_motionCard.Model}，轴数: {_axisCount}");
            }
            catch (Exception ex)
            {
                Log($"初始化失败: {ex.Message}");
                MessageBox.Show($"初始化失败: {ex.Message}");
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _motionCard.Open();
                txtStatus.Text = $"状态: 已打开 ({_axisCount}轴)";
                txtStatus.Foreground = Brushes.Green;
                btnOpen.IsEnabled = false;
                btnClose.IsEnabled = true;
                btnReset.IsEnabled = true;
                _statusTimer.Start();
                Log("控制卡已打开");
            }
            catch (Exception ex)
            {
                Log($"打开失败: {ex.Message}");
                MessageBox.Show($"打开失败: {ex.Message}");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _statusTimer.Stop();
                _motionCard.Close();
                txtStatus.Text = "状态: 已关闭";
                txtStatus.Foreground = Brushes.Red;
                btnOpen.IsEnabled = true;
                btnClose.IsEnabled = false;
                btnReset.IsEnabled = false;
                Log("控制卡已关闭");
            }
            catch (Exception ex)
            {
                Log($"关闭失败: {ex.Message}");
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _motionCard.Reset();
                Log("控制卡已复位");
            }
            catch (Exception ex)
            {
                Log($"复位失败: {ex.Message}");
                MessageBox.Show($"复位失败: {ex.Message}");
            }
        }

        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var databaseManager = new DatabaseManager();
                databaseManager.ShowDialog();
            }
            catch (Exception ex)
            {
                Log($"打开数据库管理失败: {ex.Message}");
                MessageBox.Show($"打开数据库管理失败: {ex.Message}");
            }
        }

        private void btnRefreshInputs_Click(object sender, RoutedEventArgs e)
        {
            if (_motionCard == null || !_motionCard.IsOpen) return;

            try
            {
                for (int i = 0; i < 16; i++)
                {
                    bool value = _motionCard.ReadInput(i);
                    _inputButtons[i].Background = value ? Brushes.Lime : Brushes.Gray;
                }
                Log("输入状态已刷新");
            }
            catch (Exception ex)
            {
                Log($"刷新输入失败: {ex.Message}");
            }
        }

        private void btnSetAllOutputs_Click(object sender, RoutedEventArgs e)
        {
            if (_motionCard == null || !_motionCard.IsOpen) return;

            try
            {
                for (int i = 0; i < 16; i++)
                {
                    _motionCard.WriteOutput(i, true);
                    _outputButtons[i].Background = Brushes.Red;
                }
                Log("所有输出已置位");
            }
            catch (Exception ex)
            {
                Log($"设置输出失败: {ex.Message}");
            }
        }

        private void btnResetAllOutputs_Click(object sender, RoutedEventArgs e)
        {
            if (_motionCard == null || !_motionCard.IsOpen) return;

            try
            {
                for (int i = 0; i < 16; i++)
                {
                    _motionCard.WriteOutput(i, false);
                    _outputButtons[i].Background = Brushes.Gray;
                }
                Log("所有输出已复位");
            }
            catch (Exception ex)
            {
                Log($"设置输出失败: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}\r\n");
                txtLog.ScrollToEnd();
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _statusTimer?.Stop();
            if (_motionCard != null)
            {
                try
                {
                    _motionCard.Dispose();
                }
                catch { }
            }
            base.OnClosing(e);
        }
    }
}
