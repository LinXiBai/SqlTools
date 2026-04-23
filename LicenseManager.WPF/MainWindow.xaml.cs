using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CoreToolkit.Common;
using CoreToolkit.Data;
using Newtonsoft.Json;

namespace LicenseManager.WPF
{
    public partial class MainWindow : Window
    {
        private SqliteDbContext _dbContext;
        private LicenseRecordRepository _repository;
        private List<LicenseRecordViewModel> _recordList;

        private readonly Dictionary<string, string> _deviceTypePrefixes = new Dictionary<string, string>
        {
            { "高速贴片机", "GT" },
            { "猎奇耦合测试机", "LQ" },
            { "飞泰耦合测试机", "FT" },
            { "芯片测试机", "XP" },
            { "个人电脑", "PC" },
            { "共晶/固晶", "GJ" }
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeText();
            InitializeDragDrop();
            
            if (InitializeDatabase())
            {
                LoadRecords();
                LogMessage("授权码管理系统已启动");
            }
        }

        private void InitializeDragDrop()
        {
            // 机器码拖放设置
            MachineCodeText.AllowDrop = true;
            MachineCodeText.PreviewDragOver += TextBox_PreviewDragOver;
            MachineCodeText.Drop += MachineCodeText_Drop;
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private bool InitializeDatabase()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.db");
                _dbContext = new SqliteDbContext(dbPath);
                _dbContext.InitDatabase();
                _repository = new LicenseRecordRepository(_dbContext);
                LogMessage("数据库初始化完成: " + dbPath);
                return true;
            }
            catch (Exception ex)
            {
                string errorMsg = $"数据库初始化失败: {ex.Message}";
                if (ex.InnerException != null)
                    errorMsg += $"\n内部错误: {ex.InnerException.Message}";
                LogMessage(errorMsg);
                MessageBox.Show(errorMsg + "\n\n请确保:\n1. x64 和 x86 文件夹存在且包含 SQLite.Interop.dll\n2. 已安装 Visual C++ 运行库", 
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void InitializeTimeText()
        {
            RecordTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        #region 事件处理

        private void DeviceTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string deviceType = ((DeviceTypeCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "高速贴片机";
            
            if (ProjectNumberText != null && string.IsNullOrWhiteSpace(ProjectNumberText.Text))
            {
                if (_deviceTypePrefixes.TryGetValue(deviceType, out string prefix))
                {
                    string yearMonth = DateTime.Now.ToString("yyyyMM");
                    ProjectNumberText.Text = $"{prefix}-{yearMonth}-";
                    ProjectNumberText.Focus();
                    ProjectNumberText.CaretIndex = ProjectNumberText.Text.Length;
                }
            }
        }

        private void GenerateMachineCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var machineCodeInfo = LicenseSerializer.GenerateMachineCode();
                string json = LicenseSerializer.SerializeMachineCode(machineCodeInfo);
                MachineCodeText.Text = json;
                LogMessage("机器码已生成");
            }
            catch (Exception ex)
            {
                LogMessage($"生成机器码失败: {ex.Message}");
                MessageBox.Show($"生成机器码失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportMachineCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON文件|*.json|文本文件|*.txt|所有文件|*.*",
                Title = "导入机器码文件"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string content = File.ReadAllText(dialog.FileName);
                    var machineCode = LicenseSerializer.DeserializeMachineCode(content);
                    if (machineCode != null)
                    {
                        MachineCodeText.Text = content;
                        LogMessage($"机器码已导入: {dialog.FileName}");
                    }
                    else
                        MessageBox.Show("无效的机器码文件格式", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    LogMessage($"导入机器码失败: {ex.Message}");
                    MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewMachineCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MachineCodeText.Text))
            {
                MessageBox.Show("机器码文件为空", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 读取机器码内容
                string machineCodeContent = MachineCodeText.Text;
                if (File.Exists(MachineCodeText.Text))
                {
                    machineCodeContent = File.ReadAllText(MachineCodeText.Text);
                    LogMessage($"已读取机器码文件: {Path.GetFileName(MachineCodeText.Text)}");
                }

                var machineCode = LicenseSerializer.DeserializeMachineCode(machineCodeContent);
                if (machineCode != null)
                {
                    string info = $"机器码详情:\n\n" +
                                  $"计算机名: {machineCode.MachineName}\n" +
                                  $"CPU ID: {machineCode.CpuId}\n" +
                                  $"MAC地址: {machineCode.MacAddress}\n" +
                                  $"硬盘ID: {machineCode.DiskId}\n" +
                                  $"操作系统: {machineCode.OsVersion}\n" +
                                  $"生成时间: {machineCode.GeneratedAt:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"扩展信息: {machineCode.ExtraInfo}";
                    MessageBox.Show(info, "机器码详情", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("无法解析机器码", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                LogMessage($"查看机器码失败: {ex.Message}");
            }
        }



        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null)
            {
                MessageBox.Show("数据库未初始化，无法保存记录", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ProjectNumberText.Text))
                {
                    MessageBox.Show("请输入项目号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProjectNumberText.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(DeviceNumberText.Text))
                {
                    MessageBox.Show("请输入设备号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DeviceNumberText.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(MachineCodeText.Text))
                {
                    MessageBox.Show("请选择机器码文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 读取机器码内容
                string machineCodeContent = MachineCodeText.Text;
                if (File.Exists(MachineCodeText.Text))
                {
                    machineCodeContent = File.ReadAllText(MachineCodeText.Text);
                    LogMessage($"已读取机器码文件: {Path.GetFileName(MachineCodeText.Text)}");
                }

                if (_repository.MachineCodeExists(machineCodeContent))
                {
                    var result = MessageBox.Show("该机器码已存在授权记录，是否继续添加？", "确认", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) return;
                }

                // 自动使用当前时间
                DateTime recordTime = DateTime.Now;

                string deviceType = ((DeviceTypeCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "高速贴片机";
                string fullDeviceNumber = $"{deviceType}-{DeviceNumberText.Text}";
                
                // 项目号格式：项目号#设备号
                string projectNumberWithDevice = $"{ProjectNumberText.Text.Trim()}#{DeviceNumberText.Text.Trim()}";

                var record = new LicenseRecord
                {
                    RecordTime = recordTime,
                    Department = ((DepartmentCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "技术部",
                    Operator = ((OperatorCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "彭耀东",
                    Applicant = ApplicantText.Text.Trim(),
                    ProjectNumber = projectNumberWithDevice,
                    DeviceNumber = fullDeviceNumber,
                    DeviceType = deviceType,
                    MachineCode = machineCodeContent
                };

                long id = _repository.Insert(record);
                LogMessage($"授权记录已保存，ID: {id}");

                MessageBox.Show("授权记录保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadRecords();
            }
            catch (Exception ex)
            {
                LogMessage($"保存记录失败: {ex.Message}");
                MessageBox.Show($"保存记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null)
            {
                MessageBox.Show("数据库未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string keyword = SearchText.Text?.Trim();
                string deviceTypeFilter = ((SearchDeviceTypeCombo.SelectedItem as ComboBoxItem)?.Content as string) ?? "全部设备";

                IEnumerable<LicenseRecord> records;

                if (deviceTypeFilter == "全部设备")
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                        records = _repository.GetAll();
                    else
                    {
                        records = _repository.SearchRecords(projectNumber: keyword);
                        if (!records.Any())
                            records = _repository.SearchRecords(deviceNumber: keyword);
                    }
                }
                else
                {
                    records = _repository.GetAll().Where(r => r.DeviceNumber?.StartsWith(deviceTypeFilter) == true);
                    
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        records = records.Where(r => 
                            (r.ProjectNumber?.Contains(keyword) == true) || 
                            (r.DeviceNumber?.Contains(keyword) == true));
                    }
                }

                UpdateRecordGrid(records);
                LogMessage($"搜索完成，找到 {records.Count()} 条记录");
            }
            catch (Exception ex)
            {
                LogMessage($"搜索失败: {ex.Message}");
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadRecords();
        }

        private void LicenseRecordGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private long _editingRecordId = -1;

        private void LicenseRecordGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // 检查当前行是否允许编辑
            if (e.Row.Item is LicenseRecordViewModel viewModel)
            {
                if (!viewModel.IsEditing)
                {
                    e.Cancel = true; // 取消编辑
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void EditRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null) return;

            if (sender is Button btn && btn.Tag is long id)
            {
                // 如果正在编辑其他行，提示先保存
                if (_editingRecordId != -1 && _editingRecordId != id)
                {
                    MessageBox.Show("请先保存当前编辑的记录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 如果点击的是当前正在编辑的行，不做任何操作
                if (_editingRecordId == id)
                {
                    return;
                }

                _editingRecordId = id;
                
                // 启用DataGrid编辑
                LicenseRecordGrid.IsReadOnly = false;
                
                // 标记所有行：当前行为可编辑，其他行为只读
                foreach (var item in _recordList)
                {
                    item.IsEditing = (item.Id == id);
                }
                
                // 选中当前行
                var viewModel = _recordList.FirstOrDefault(r => r.Id == id);
                if (viewModel != null)
                {
                    LicenseRecordGrid.SelectedItem = viewModel;
                }
                
                LicenseRecordGrid.Items.Refresh();
                LogMessage($"开始编辑记录，ID: {id}，请双击单元格进行编辑");
            }
        }

        private void SaveEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null) return;

            if (sender is Button btn && btn.Tag is long id)
            {
                try
                {
                    // 结束编辑，确保数据更新到ViewModel
                    LicenseRecordGrid.CommitEdit();
                    
                    // 禁用DataGrid编辑
                    LicenseRecordGrid.IsReadOnly = true;

                    // 获取编辑后的数据
                    var viewModel = _recordList.FirstOrDefault(r => r.Id == id);
                    if (viewModel != null)
                    {
                        // 从数据库获取原始记录
                        var record = _repository.GetById(id);
                        if (record != null)
                        {
                            // 更新记录数据
                            record.RecordTime = viewModel.RecordTime;
                            record.Department = viewModel.Department;
                            record.Operator = viewModel.Operator;
                            record.Applicant = viewModel.Applicant;
                            record.ProjectNumber = viewModel.ProjectNumber;
                            record.DeviceNumber = viewModel.DeviceNumber;
                            record.DeviceType = viewModel.DeviceType;

                            _repository.Update(record);
                            LogMessage($"记录已更新，ID: {id}");
                            MessageBox.Show("记录保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        viewModel.IsEditing = false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"保存编辑失败: {ex.Message}");
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // 无论成功失败，都重置编辑状态
                    _editingRecordId = -1;
                    
                    // 禁用DataGrid编辑，重置所有行的IsEditing状态
                    LicenseRecordGrid.IsReadOnly = true;
                    foreach (var item in _recordList)
                    {
                        item.IsEditing = false;
                    }
                    LicenseRecordGrid.Items.Refresh();
                }
            }
        }



        private void ExportRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null) return;

            if (sender is Button btn && btn.Tag is long id)
            {
                var record = _repository.GetById(id);
                if (record == null) return;

                var dialog = new SaveFileDialog
                {
                    FileName = "机器码.license",
                    Filter = "License文件|*.license|所有文件|*.*",
                    Title = "导出机器码文件"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // 直接使用数据库中存储的原始机器码字符串
                        // 不再尝试反序列化和重新序列化，保持原始格式
                        string machineCodeJson = record.MachineCode;
                        
                        // 保存为 .license 文件
                        File.WriteAllText(dialog.FileName, machineCodeJson);
                        
                        LogMessage($"机器码已导出: {dialog.FileName}");
                        LogMessage($"导出内容: {(machineCodeJson.Length > 100 ? machineCodeJson.Substring(0, 100) + "..." : machineCodeJson)}");
                        LogMessage($"导出内容长度: {machineCodeJson.Length}");
                        MessageBox.Show("机器码导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"导出失败: {ex.Message}");
                        MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null) return;

            if (sender is Button btn && btn.Tag is long id)
            {
                var result = MessageBox.Show("确定要删除这条记录吗？", "确认删除", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.Delete(id);
                        LogMessage($"记录已删除，ID: {id}");
                        LoadRecords();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"删除失败: {ex.Message}");
                        MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private void ExportLogBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"LicenseLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Filter = "文本文件|*.txt|所有文件|*.*",
                Title = "导出日志"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, LogTextBox.Text);
                    LogMessage($"日志已导出: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出日志失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region 拖放事件处理

        private void MachineCodeText_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0)
                    {
                        string filePath = files[0];
                        if (File.Exists(filePath))
                        {
                            MachineCodeText.Text = filePath;
                            LogMessage($"机器码文件已选择: {Path.GetFileName(filePath)}");
                        }
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    string text = (string)e.Data.GetData(DataFormats.Text);
                    MachineCodeText.Text = text;
                    LogMessage("机器码已从粘贴导入");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"导入机器码失败: {ex.Message}");
                MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 新增按钮事件处理

        private void ClearMachineCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            MachineCodeText.Text = string.Empty;
        }

        private void OpenLicenseToolBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "授权码软件", "授权码生成.exe");
                
                if (!File.Exists(exePath))
                {
                    LogMessage($"未找到授权码生成程序: {exePath}");
                    MessageBox.Show($"未找到授权码生成程序\n\n路径: {exePath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LogMessage($"启动授权码生成程序: {exePath}");
                Process.Start(exePath);
            }
            catch (Exception ex)
            {
                LogMessage($"启动授权码生成程序失败: {ex.Message}");
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion

        #region 辅助方法

        private void LoadRecords()
        {
            if (_repository == null)
            {
                LogMessage("数据库未初始化，无法加载记录");
                return;
            }

            try
            {
                var records = _repository.GetRecentRecords(100);
                UpdateRecordGrid(records);
                LogMessage($"已加载 {records.Count()} 条记录");
            }
            catch (Exception ex)
            {
                LogMessage($"加载记录失败: {ex.Message}");
            }
        }

        private void UpdateRecordGrid(IEnumerable<LicenseRecord> records)
        {
            int count = 0;
            _recordList = records.Select(r => 
            {
                if (count == 0)
                {
                    LogMessage($"第一条记录 - 机器码长度: {r.MachineCode?.Length ?? 0}");
                    LogMessage($"机器码前100字符: {((r.MachineCode?.Length ?? 0) > 100 ? r.MachineCode.Substring(0, 100) + "..." : r.MachineCode)}");
                }
                count++;
                return new LicenseRecordViewModel
                {
                    Id = r.Id,
                    RecordTime = r.RecordTime,
                    Department = r.Department,
                    Operator = r.Operator,
                    Applicant = r.Applicant,
                    ProjectNumber = r.ProjectNumber,
                    DeviceNumber = r.DeviceNumber,
                    DeviceType = r.DeviceType,
                    MachineCodeShort = TruncateString(r.MachineCode, 20),
                    FullRecord = r
                };
            }).ToList();

            LicenseRecordGrid.ItemsSource = _recordList;
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str;
            return str.Substring(0, maxLength) + "...";
        }

        private void ClearForm()
        {
            RecordDatePicker.SelectedDate = DateTime.Now;
            RecordTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
            DepartmentCombo.SelectedIndex = 0;
            OperatorCombo.SelectedIndex = 0;
            DeviceTypeCombo.SelectedIndex = 0;
            ProjectNumberText.Clear();
            DeviceNumberText.Clear();
            ApplicantText.Clear();
            MachineCodeText.Clear();
        }

        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            LogTextBox.AppendText($"[{timestamp}] {message}\r\n");
            LogTextBox.ScrollToEnd();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosing(e);
        }

        #endregion
    }

    public class LicenseRecordViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public long Id { get; set; }
        
        private DateTime _recordTime;
        public DateTime RecordTime 
        { 
            get => _recordTime; 
            set { _recordTime = value; OnPropertyChanged(nameof(RecordTime)); }
        }
        
        private string _department;
        public string Department 
        { 
            get => _department; 
            set { _department = value; OnPropertyChanged(nameof(Department)); }
        }
        
        private string _operator;
        public string Operator 
        { 
            get => _operator; 
            set { _operator = value; OnPropertyChanged(nameof(Operator)); }
        }
        
        private string _applicant;
        public string Applicant 
        { 
            get => _applicant; 
            set { _applicant = value; OnPropertyChanged(nameof(Applicant)); }
        }
        
        private string _projectNumber;
        public string ProjectNumber 
        { 
            get => _projectNumber; 
            set { _projectNumber = value; OnPropertyChanged(nameof(ProjectNumber)); }
        }
        
        private string _deviceNumber;
        public string DeviceNumber 
        { 
            get => _deviceNumber; 
            set { _deviceNumber = value; OnPropertyChanged(nameof(DeviceNumber)); }
        }

        private string _deviceType;
        public string DeviceType 
        { 
            get => _deviceType; 
            set { _deviceType = value; OnPropertyChanged(nameof(DeviceType)); }
        }
        
        public string MachineCodeShort { get; set; }
        public LicenseRecord FullRecord { get; set; }
        
        private bool _isEditing;
        public bool IsEditing 
        { 
            get => _isEditing; 
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class BoolToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool b && b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is System.Windows.Visibility v && v == System.Windows.Visibility.Visible;
        }
    }

    public class InverseBoolToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool b && b ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is System.Windows.Visibility v && v != System.Windows.Visibility.Visible;
        }
    }
}
