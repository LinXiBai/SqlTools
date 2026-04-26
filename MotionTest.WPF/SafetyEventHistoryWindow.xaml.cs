using System;
using System.Windows;
using CoreToolkit.Data;
using MotionTest.WPF.ViewModels;

namespace MotionTest.WPF
{
    /// <summary>
    /// 安全事件历史查询窗口（MVVM）
    /// </summary>
    public partial class SafetyEventHistoryWindow : Window
    {
        private readonly SafetyEventHistoryViewModel _viewModel;

        public SafetyEventHistoryWindow(LogRepository logRepo)
        {
            InitializeComponent();
            _viewModel = new SafetyEventHistoryViewModel(logRepo);
            DataContext = _viewModel;
        }

        /// <summary>
        /// 带初始日期范围的历史查询窗口
        /// </summary>
        public SafetyEventHistoryWindow(LogRepository logRepo, DateTime startDate, DateTime endDate)
            : this(logRepo)
        {
            _viewModel.StartDate = startDate;
            _viewModel.EndDate = endDate;
        }
    }
}
