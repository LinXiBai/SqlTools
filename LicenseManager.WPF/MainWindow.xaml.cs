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
                // 使用持久化路径，防止编译清理输出目录时丢失数据
                var dbPath = App.GetLicenseDbPath();

                // 首次迁移：如果旧位置（exe 同级目录）有数据而新位置没有，自动复制
                var legacyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.db");
                if (!System.IO.File.Exists(dbPath) && System.IO.File.Exists(legacyPath))
                {
                    try { System.IO.File.Copy(legacyPath, dbPath, overwrite: false); }
                    catch { /* 忽略迁移失败 */ }
                }

                // 使用 DatabaseConfig 注册并创建上下文
                DatabaseConfig.Register("LicenseDb", dbPath);
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
        /// 编辑按钮点击
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LicenseRecordViewModel vm)
            {
                _viewModel.LoadForEdit(vm);
            }
        }

        /// <summary>
        /// 导出单行按钮点击
        /// </summary>
        private void ExportRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LicenseRecordViewModel vm && vm.FullRecord != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"License_{vm.ProjectNumber}_{DateTime.Now:yyyyMMdd}",
                    Filter = "JSON文件|*.json|文本文件|*.txt|所有文件|*.*",
                    Title = "导出授权记录"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var exportData = new
                        {
                            vm.FullRecord.Id,
                            vm.FullRecord.RecordTime,
                            vm.FullRecord.Department,
                            vm.FullRecord.Operator,
                            vm.FullRecord.Applicant,
                            vm.FullRecord.ProjectNumber,
                            vm.FullRecord.DeviceNumber,
                            vm.FullRecord.DeviceType,
                            vm.FullRecord.CreatedAt,
                            vm.FullRecord.UpdatedAt,
                            MachineCode = vm.FullRecord.MachineCode
                        };

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(exportData, Newtonsoft.Json.Formatting.Indented);
                        System.IO.File.WriteAllText(dialog.FileName, json);
                        _viewModel.AddLog($"导出单条记录 ID:{vm.Id} 到 {System.IO.Path.GetFileName(dialog.FileName)}");
                        MessageBox.Show("导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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

        /// <summary>
        /// 机器码框拖放预览
        /// </summary>
        private void MachineCodeBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        /// <summary>
        /// 机器码框拖放
        /// </summary>
        private void MachineCodeBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    try
                    {
                        var path = files[0];
                        _viewModel.FormMachineCodeFilePath = path;
                        _viewModel.FormMachineCode = System.IO.File.ReadAllText(path);
                        _viewModel.AddLog($"拖放导入机器码文件: {System.IO.Path.GetFileName(path)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
