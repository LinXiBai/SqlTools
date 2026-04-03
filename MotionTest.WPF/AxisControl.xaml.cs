using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MotionLib.Core;

namespace MotionTest.WPF
{
    /// <summary>
    /// 轴控制用户控件
    /// </summary>
    public partial class AxisControl : UserControl
    {
        private int _axisId;

        /// <summary>
        /// 轴操作事件
        /// </summary>
        public event Action<int, string, object> AxisAction;

        public AxisControl(int axisId)
        {
            InitializeComponent();
            _axisId = axisId;
            txtAxisTitle.Text = $"轴 {axisId}";
        }

        /// <summary>
        /// 更新轴状态显示
        /// </summary>
        public void UpdateStatus(AxisStatus status)
        {
            // 更新状态指示灯
            borderServo.Background = status.ServoOn ? Brushes.Lime : Brushes.Gray;
            borderRunning.Background = status.IsRunning ? Brushes.Blue : Brushes.Gray;
            borderAlarm.Background = status.IsAlarm ? Brushes.Red : Brushes.Gray;
            borderHome.Background = status.Homed ? Brushes.Lime : Brushes.Gray;

            // 更新位置显示
            txtPosition.Text = $"位置: {status.ActualPosition:F3}";
        }

        private void btnServoOn_Click(object sender, RoutedEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "ServoOn", null);
        }

        private void btnServoOff_Click(object sender, RoutedEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "ServoOff", null);
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtHomeSpeed.Text, out double speed))
            {
                AxisAction?.Invoke(_axisId, "Home", speed);
            }
            else
            {
                MessageBox.Show("请输入有效的回零速度！");
            }
        }

        private void btnMoveAbs_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtTargetPos.Text, out double position) &&
                double.TryParse(txtSpeed.Text, out double speed))
            {
                AxisAction?.Invoke(_axisId, "MoveAbs", Tuple.Create(position, speed));
            }
            else
            {
                MessageBox.Show("请输入有效的位置和速度！");
            }
        }

        private void btnMoveRel_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtRelDistance.Text, out double distance) &&
                double.TryParse(txtSpeed.Text, out double speed))
            {
                AxisAction?.Invoke(_axisId, "MoveRel", Tuple.Create(distance, speed));
            }
            else
            {
                MessageBox.Show("请输入有效的距离和速度！");
            }
        }

        private void btnJogMinus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (double.TryParse(txtJogSpeed.Text, out double speed))
            {
                AxisAction?.Invoke(_axisId, "Jog-", speed);
            }
        }

        private void btnJogMinus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "Stop", null);
        }

        private void btnJogPlus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (double.TryParse(txtJogSpeed.Text, out double speed))
            {
                AxisAction?.Invoke(_axisId, "Jog+", speed);
            }
        }

        private void btnJogPlus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "Stop", null);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "Stop", null);
        }

        private void btnEmgStop_Click(object sender, RoutedEventArgs e)
        {
            AxisAction?.Invoke(_axisId, "EmgStop", null);
        }

        private void btnSetPos_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtSetPos.Text, out double position))
            {
                AxisAction?.Invoke(_axisId, "SetPosition", position);
            }
            else
            {
                MessageBox.Show("请输入有效的位置值！");
            }
        }
    }
}
