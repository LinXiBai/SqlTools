using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MotionTest.WPF.ViewModels;

namespace MotionTest.WPF.Views
{
    /// <summary>
    /// SafetyStatisticsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SafetyStatisticsWindow : Window
    {
        public SafetyStatisticsWindow(SafetyStatisticsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        /// <summary>
        /// DataGrid 双击穿透到详细日志
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is SafetyStatRowViewModel row)
            {
                try
                {
                    var historyWindow = new SafetyEventHistoryWindow(
                        ((SafetyStatisticsViewModel)DataContext).LogRepo,
                        row.PeriodStart,
                        row.PeriodEnd);
                    historyWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开明细窗口失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// 统计周期类型转换器
    /// </summary>
    public class StatPeriodTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatPeriodType period)
            {
                return period switch
                {
                    StatPeriodType.Day => "按天统计",
                    StatPeriodType.Week => "按周统计",
                    StatPeriodType.Month => "按月统计",
                    _ => period.ToString()
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
