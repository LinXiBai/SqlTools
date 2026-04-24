using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CoreToolkit.Data;
using CoreToolkit.Data.Models;
using LicenseManager.WPF.ViewModels;

namespace LicenseManager.WPF
{
    /// <summary>
    /// 授权码生成管理系统主窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LicenseManagerViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // 使用 DatabaseConfig 注册并创建上下文，避免硬编码
                DatabaseConfig.Register("LicenseDb", "license.db");
                var db = DatabaseFactory.CreateContext("LicenseDb");
                db.InitDatabase();
                var repository = new LicenseRecordRepository(db);
                _viewModel = new LicenseManagerViewModel(repository, Dispatcher);
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库初始化失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 查看详情按钮点击
        /// </summary>
        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LicenseRecordViewModel vm && vm.FullRecord != null)
            {
                var detailWindow = new DetailWindow(vm.FullRecord);
                detailWindow.ShowDialog();
            }
        }

        /// <summary>
        /// 删除按钮点击
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LicenseRecordViewModel vm)
            {
                _viewModel.DeleteCommand.Execute(vm);
            }
        }
    }

    /// <summary>
    /// 布尔值反转 Visibility 转换器
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is int i ? i > 0 : value is bool b && b;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
