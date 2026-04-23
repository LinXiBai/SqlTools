using System.Windows;
using CoreToolkit.Data;
using MotionTest.WPF.ViewModels;

namespace MotionTest.WPF
{
    /// <summary>
    /// 安全事件统计报表窗口（MVVM）
    /// </summary>
    public partial class SafetyReportWindow : Window
    {
        public SafetyReportWindow(LogRepository logRepo)
        {
            InitializeComponent();
            DataContext = new SafetyReportViewModel(logRepo);
        }
    }
}
