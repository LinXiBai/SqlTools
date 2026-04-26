using System;
using System.Windows;
using CoreToolkit.Data;
using CoreToolkit.Motion.Core;
using MotionTest.WPF.ViewModels;

namespace MotionTest.WPF
{
    /// <summary>
    /// 安全防护监控窗口（MVVM）
    /// 所有业务逻辑已迁移到 SafetyMonitorViewModel
    /// 此类仅负责窗口生命周期管理和 ViewModel 的创建与清理
    /// </summary>
    public partial class SafetyMonitorWindow : Window
    {
        private SafetyMonitorViewModel _viewModel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionCard">运动控制卡实例</param>
        /// <param name="logRepo">日志仓储（可选，用于安全事件持久化）</param>
        public SafetyMonitorWindow(IMotionCard motionCard, LogRepository logRepo = null)
        {
            InitializeComponent();

            // 创建 ViewModel 并设置为 DataContext
            _viewModel = new SafetyMonitorViewModel(motionCard, logRepo, Dispatcher);
            DataContext = _viewModel;
        }

        /// <summary>
        /// 窗口关闭时清理资源
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel?.Cleanup();
            base.OnClosing(e);
        }
    }
}
