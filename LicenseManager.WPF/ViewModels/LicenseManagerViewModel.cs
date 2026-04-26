using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CoreToolkit.Data;
using CoreToolkit.Data.Models;
using CoreToolkit.Desktop.MVVM;

namespace LicenseManager.WPF.ViewModels
{
    /// <summary>
    /// 许可证管理器主视图模型
    /// </summary>
    public class LicenseManagerViewModel : ObservableObject
    {
        private readonly LicenseRecordRepository _repository;
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _searchDebounceTimer;

        // 仪表盘
        private long _statTotalCount;
        private long _statProjectCount;
        private long _statDeviceCount;
        private long _statDepartmentCount;
        private long _statOperatorCount;

        // 搜索条件
        private string _searchKeyword = "";
        private string _filterDepartment = "";
        private string _filterOperator = "";
        private string _filterApplicant = "";
        private string _filterDeviceType = "";
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;
        private string _sortColumn = "RecordTime";
        private bool _sortDescending = true;

        // 分页
        private int _pageIndex = 0;
        private int _pageSize = 50;
        private long _totalCount;
        private int _totalPages;
        private bool _isLoading;

        // 表单
        private DateTime _formRecordTime = DateTime.Now;
        private string _formRecordTimeText = DateTime.Now.ToString("HH:mm:ss");
        private string _formDepartment = "软件";
        private string _formOperator = "彭耀东";
        private string _formApplicant = "";
        private string _formProjectNumber = "";
        private string _formDeviceNumber = "";
        private string _formDeviceType = "个人电脑";
        private string _formMachineCode = "";
        private string _formMachineCodeFilePath = "";

        // 编辑模式
        private bool _isEditing;
        private long _editingRecordId;

        // 日志
        private ObservableCollection<string> _logs = new ObservableCollection<string>();

        // 下拉选项
        private ObservableCollection<string> _departmentOptions = new ObservableCollection<string>();
        private ObservableCollection<string> _operatorOptions = new ObservableCollection<string>();
        private ObservableCollection<string> _deviceTypeOptions = new ObservableCollection<string>();

        public LicenseManagerViewModel(LicenseRecordRepository repository, Dispatcher dispatcher)
        {
            _repository = repository;
            _dispatcher = dispatcher;

            Records = new ObservableCollection<LicenseRecordViewModel>();

            SearchCommand = new RelayCommand(ExecuteSearch);
            ResetFilterCommand = new RelayCommand(ExecuteResetFilter);
            PreviousPageCommand = new RelayCommand(() => ChangePage(-1), () => PageIndex > 0);
            NextPageCommand = new RelayCommand(() => ChangePage(1), () => PageIndex + 1 < TotalPages);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            ClearFormCommand = new RelayCommand(ExecuteClearForm);
            DeleteCommand = new RelayCommand<LicenseRecordViewModel>(ExecuteDelete);
            BatchDeleteCommand = new RelayCommand(ExecuteBatchDelete, () => Records.Any(r => r.IsSelected));
            ExportCommand = new RelayCommand(ExecuteExport);
            RefreshStatsCommand = new RelayCommand(ExecuteRefreshStats);
            RefreshCommand = new RelayCommand(ExecuteSearch);
            ImportMachineCodeCommand = new RelayCommand(ExecuteImportMachineCode);
            ViewMachineCodeFileCommand = new RelayCommand(ExecuteViewMachineCodeFile);
            ClearMachineCodeCommand = new RelayCommand(ExecuteClearMachineCode);
            OpenAuthSoftwareCommand = new RelayCommand(ExecuteOpenAuthSoftware);
            ClearLogsCommand = new RelayCommand(() => Logs.Clear());
            ExportLogsCommand = new RelayCommand(ExecuteExportLogs);

            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                ExecuteSearch();
            };

            LoadOptions();
            ExecuteRefreshStats();
            ExecuteSearch();
            AddLog("系统初始化完成");
        }

        #region 属性

        public ObservableCollection<LicenseRecordViewModel> Records { get; }

        // 仪表盘
        public long StatTotalCount { get => _statTotalCount; set => SetProperty(ref _statTotalCount, value); }
        public long StatProjectCount { get => _statProjectCount; set => SetProperty(ref _statProjectCount, value); }
        public long StatDeviceCount { get => _statDeviceCount; set => SetProperty(ref _statDeviceCount, value); }
        public long StatDepartmentCount { get => _statDepartmentCount; set => SetProperty(ref _statDepartmentCount, value); }
        public long StatOperatorCount { get => _statOperatorCount; set => SetProperty(ref _statOperatorCount, value); }

        // 搜索条件
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            }
        }

        public string FilterDepartment { get => _filterDepartment; set { if (SetProperty(ref _filterDepartment, value)) ExecuteSearch(); } }
        public string FilterOperator { get => _filterOperator; set { if (SetProperty(ref _filterOperator, value)) ExecuteSearch(); } }
        public string FilterApplicant { get => _filterApplicant; set { if (SetProperty(ref _filterApplicant, value)) ExecuteSearch(); } }
        public string FilterDeviceType { get => _filterDeviceType; set { if (SetProperty(ref _filterDeviceType, value)) ExecuteSearch(); } }
        public DateTime? FilterStartDate { get => _filterStartDate; set { if (SetProperty(ref _filterStartDate, value)) ExecuteSearch(); } }
        public DateTime? FilterEndDate { get => _filterEndDate; set { if (SetProperty(ref _filterEndDate, value)) ExecuteSearch(); } }

        public string SortColumn { get => _sortColumn; set { if (SetProperty(ref _sortColumn, value)) ExecuteSearch(); } }
        public bool SortDescending { get => _sortDescending; set { if (SetProperty(ref _sortDescending, value)) ExecuteSearch(); } }

        // 分页
        public int PageIndex { get => _pageIndex; set => SetProperty(ref _pageIndex, value); }
        public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }
        public long TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }
        public bool IsLoading { get => _isLoading; set { if (SetProperty(ref _isLoading, value)) OnPropertyChanged(nameof(IsNotLoading)); } }
        public bool IsNotLoading => !IsLoading;
        public string PaginationText => $"第 {PageIndex + 1} / {Math.Max(1, TotalPages)} 页，共 {TotalCount} 条";

        // 表单
        public DateTime FormRecordTime
        {
            get => _formRecordTime;
            set
            {
                if (SetProperty(ref _formRecordTime, value))
                {
                    var text = value.ToString("HH:mm:ss");
                    if (_formRecordTimeText != text)
                        FormRecordTimeText = text;
                }
            }
        }

        public string FormRecordTimeText
        {
            get => _formRecordTimeText;
            set
            {
                if (SetProperty(ref _formRecordTimeText, value))
                {
                    if (TimeSpan.TryParse(value, out var ts))
                    {
                        var newTime = FormRecordTime.Date.Add(ts);
                        if (_formRecordTime != newTime)
                        {
                            _formRecordTime = newTime;
                            OnPropertyChanged(nameof(FormRecordTime));
                        }
                    }
                }
            }
        }

        public string FormDepartment { get => _formDepartment; set => SetProperty(ref _formDepartment, value); }
        public string FormOperatorName { get => _formOperator; set => SetProperty(ref _formOperator, value); }
        public string FormApplicant { get => _formApplicant; set => SetProperty(ref _formApplicant, value); }
        public string FormProjectNumber { get => _formProjectNumber; set => SetProperty(ref _formProjectNumber, value); }
        public string FormDeviceNumber { get => _formDeviceNumber; set => SetProperty(ref _formDeviceNumber, value); }
        public string FormDeviceType { get => _formDeviceType; set => SetProperty(ref _formDeviceType, value); }
        public string FormMachineCode { get => _formMachineCode; set => SetProperty(ref _formMachineCode, value); }
        public string FormMachineCodeFilePath { get => _formMachineCodeFilePath; set => SetProperty(ref _formMachineCodeFilePath, value); }

        // 编辑模式
        public bool IsEditing { get => _isEditing; set { if (SetProperty(ref _isEditing, value)) { OnPropertyChanged(nameof(FormTitle)); OnPropertyChanged(nameof(SaveButtonText)); } } }
        public long EditingRecordId { get => _editingRecordId; set => SetProperty(ref _editingRecordId, value); }
        public string FormTitle => IsEditing ? "编辑授权记录" : "添加授权记录";
        public string SaveButtonText => IsEditing ? "更新记录" : "保存记录";

        // 日志
        public ObservableCollection<string> Logs { get => _logs; set => SetProperty(ref _logs, value); }

        // 下拉选项
        public ObservableCollection<string> DepartmentOptions { get => _departmentOptions; set => SetProperty(ref _departmentOptions, value); }
        public ObservableCollection<string> OperatorOptions { get => _operatorOptions; set => SetProperty(ref _operatorOptions, value); }
        public ObservableCollection<string> DeviceTypeOptions { get => _deviceTypeOptions; set => SetProperty(ref _deviceTypeOptions, value); }

        // 是否有选中项（批量操作）
        public bool HasSelection => Records.Any(r => r.IsSelected);

        #endregion

        #region 命令

        public ICommand SearchCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BatchDeleteCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RefreshStatsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ImportMachineCodeCommand { get; }
        public ICommand ViewMachineCodeFileCommand { get; }
        public ICommand ClearMachineCodeCommand { get; }
        public ICommand OpenAuthSoftwareCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand ExportLogsCommand { get; }

        #endregion

        #region 方法

        private void LoadOptions()
        {
            try
            {
                DepartmentOptions = new ObservableCollection<string>(new[] { "" }.Concat(_repository.GetDistinctDepartments()));
                OperatorOptions = new ObservableCollection<string>(new[] { "", "彭耀东", "李柯", "欧阳健峰" });
                DeviceTypeOptions = new ObservableCollection<string>(new[] { "", "猎奇耦合测试机", "飞泰耦合测试机", "猎奇高速贴片机", "猎奇高速固晶机", "猎奇芯片测试机", "个人电脑" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseManager] 加载选项失败: {ex.Message}");
            }
        }

        private void ExecuteSearch()
        {
            try
            {
                IsLoading = true;

                var result = _repository.SearchPaged(
                    keyword: string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword,
                    department: string.IsNullOrEmpty(FilterDepartment) ? null : FilterDepartment,
                    operatorName: string.IsNullOrEmpty(FilterOperator) ? null : FilterOperator,
                    applicant: string.IsNullOrEmpty(FilterApplicant) ? null : FilterApplicant,
                    deviceType: string.IsNullOrEmpty(FilterDeviceType) ? null : FilterDeviceType,
                    startDate: FilterStartDate,
                    endDate: FilterEndDate,
                    sortColumn: SortColumn,
                    sortDescending: SortDescending,
                    pageIndex: PageIndex,
                    pageSize: PageSize);

                Records.Clear();
                foreach (var vm in result.Items.Select(LicenseRecordViewModel.FromEntity))
                {
                    vm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(LicenseRecordViewModel.IsSelected))
                        {
                            OnPropertyChanged(nameof(HasSelection));
                            (BatchDeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                        }
                    };
                    Records.Add(vm);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                OnPropertyChanged(nameof(PaginationText));

                (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseManager] 搜索失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteResetFilter()
        {
            SearchKeyword = "";
            FilterDepartment = "";
            FilterOperator = "";
            FilterApplicant = "";
            FilterDeviceType = "";
            FilterStartDate = null;
            FilterEndDate = null;
            PageIndex = 0;
            ExecuteSearch();
        }

        private void ChangePage(int delta)
        {
            var newPage = PageIndex + delta;
            if (newPage >= 0 && newPage < TotalPages)
            {
                PageIndex = newPage;
                ExecuteSearch();
            }
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(FormProjectNumber)
                && !string.IsNullOrWhiteSpace(FormDeviceNumber)
                && !string.IsNullOrWhiteSpace(FormMachineCode);
        }

        private void ExecuteSave()
        {
            try
            {
                if (!IsEditing && _repository.MachineCodeExists(FormMachineCode))
                {
                    var result = MessageBox.Show("该机器码已存在，是否继续保存？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes) return;
                }

                if (IsEditing)
                {
                    var record = _repository.GetById(EditingRecordId);
                    if (record == null)
                    {
                        MessageBox.Show("记录不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    record.RecordTime = FormRecordTime;
                    record.Department = FormDepartment;
                    record.Operator = FormOperatorName;
                    record.Applicant = FormApplicant;
                    record.ProjectNumber = FormProjectNumber;
                    record.DeviceNumber = FormDeviceNumber;
                    record.DeviceType = FormDeviceType;
                    record.MachineCode = FormMachineCode;
                    record.UpdatedAt = DateTime.Now;
                    _repository.Update(record);
                    AddLog($"更新记录 ID: {record.Id}, 项目号: {record.ProjectNumber}");
                    MessageBox.Show("更新成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var record = new LicenseRecord
                    {
                        RecordTime = FormRecordTime,
                        Department = FormDepartment,
                        Operator = FormOperatorName,
                        Applicant = FormApplicant,
                        ProjectNumber = FormProjectNumber,
                        DeviceNumber = FormDeviceNumber,
                        DeviceType = FormDeviceType,
                        MachineCode = FormMachineCode,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _repository.Insert(record);
                    AddLog($"保存记录 ID: {record.Id}, 项目号: {record.ProjectNumber}");
                    MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ExecuteClearForm();
                ExecuteSearch();
                ExecuteRefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{(IsEditing ? "更新" : "保存")}失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteClearForm()
        {
            IsEditing = false;
            EditingRecordId = 0;
            FormRecordTime = DateTime.Now;
            FormRecordTimeText = FormRecordTime.ToString("HH:mm:ss");
            FormDepartment = DepartmentOptions.FirstOrDefault(o => !string.IsNullOrEmpty(o)) ?? "";
            FormOperatorName = "彭耀东";
            FormApplicant = "";
            FormProjectNumber = "";
            FormDeviceNumber = "";
            FormDeviceType = "个人电脑";
            FormMachineCode = "";
            FormMachineCodeFilePath = "";
        }

        private void ExecuteDelete(LicenseRecordViewModel vm)
        {
            if (vm == null) return;
            try
            {
                var result = MessageBox.Show($"确定删除记录 [{vm.ProjectNumber} - {vm.DeviceNumber}] 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                _repository.Delete(vm.Id);
                Records.Remove(vm);
                ExecuteRefreshStats();
                AddLog($"删除记录 ID: {vm.Id}, 项目号: {vm.ProjectNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteBatchDelete()
        {
            var selected = Records.Where(r => r.IsSelected).ToList();
            if (selected.Count == 0) return;

            try
            {
                var result = MessageBox.Show($"确定删除选中的 {selected.Count} 条记录吗？", "确认批量删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                foreach (var vm in selected)
                {
                    _repository.Delete(vm.Id);
                    Records.Remove(vm);
                }

                ExecuteRefreshStats();
                AddLog($"批量删除 {selected.Count} 条记录");
                MessageBox.Show("批量删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExport()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV 文件 (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"LicenseRecords_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() != true) return;

                // 导出当前查询的全部结果
                var allResults = _repository.SearchPaged(
                    keyword: string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword,
                    department: string.IsNullOrEmpty(FilterDepartment) ? null : FilterDepartment,
                    operatorName: string.IsNullOrEmpty(FilterOperator) ? null : FilterOperator,
                    applicant: string.IsNullOrEmpty(FilterApplicant) ? null : FilterApplicant,
                    deviceType: string.IsNullOrEmpty(FilterDeviceType) ? null : FilterDeviceType,
                    startDate: FilterStartDate,
                    endDate: FilterEndDate,
                    sortColumn: SortColumn,
                    sortDescending: SortDescending,
                    pageIndex: 0,
                    pageSize: 10000);

                var lines = new List<string>();
                lines.Add("ID,记录时间,部门,记录人,申请人,项目号,设备号,设备类型,机器码");
                foreach (var r in allResults.Items)
                {
                    lines.Add($"{r.Id},{r.RecordTime:yyyy-MM-dd HH:mm},{EscapeCsv(r.Department)},{EscapeCsv(r.Operator)},{EscapeCsv(r.Applicant)},{EscapeCsv(r.ProjectNumber)},{EscapeCsv(r.DeviceNumber)},{EscapeCsv(r.DeviceType)},{EscapeCsv(r.MachineCode)}");
                }
                File.WriteAllLines(dialog.FileName, lines);
                AddLog($"导出记录到: {dialog.FileName}");
                MessageBox.Show("导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefreshStats()
        {
            try
            {
                var stats = _repository.GetStatistics();
                StatTotalCount = stats.TotalCount;
                StatProjectCount = stats.ProjectCount;
                StatDeviceCount = stats.DeviceCount;
                StatDepartmentCount = stats.DepartmentCount;
                StatOperatorCount = stats.OperatorCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseManager] 刷新统计失败: {ex.Message}");
            }
        }

        public void LoadForEdit(LicenseRecordViewModel vm)
        {
            if (vm?.FullRecord == null) return;
            IsEditing = true;
            EditingRecordId = vm.Id;
            FormRecordTime = vm.FullRecord.RecordTime;
            FormRecordTimeText = vm.FullRecord.RecordTime.ToString("HH:mm:ss");
            FormDepartment = vm.FullRecord.Department ?? "";
            FormOperatorName = vm.FullRecord.Operator ?? "";
            FormApplicant = vm.FullRecord.Applicant ?? "";
            FormProjectNumber = vm.FullRecord.ProjectNumber ?? "";
            FormDeviceNumber = vm.FullRecord.DeviceNumber ?? "";
            FormDeviceType = vm.FullRecord.DeviceType ?? "";
            FormMachineCode = vm.FullRecord.MachineCode ?? "";
            FormMachineCodeFilePath = "";
            AddLog($"加载记录 ID: {vm.Id} 到编辑区");
        }

        public void AddLog(string message)
        {
            _dispatcher.Invoke(() =>
            {
                Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss.fff}] {message}");
                if (Logs.Count > 500) Logs.RemoveAt(Logs.Count - 1);
            });
        }

        private void ExecuteImportMachineCode()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "授权文件|*.license;*.txt|所有文件|*.*",
                Title = "选择机器码文件"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    FormMachineCodeFilePath = dialog.FileName;
                    FormMachineCode = File.ReadAllText(dialog.FileName);
                    AddLog($"导入机器码文件: {System.IO.Path.GetFileName(dialog.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteViewMachineCodeFile()
        {
            if (string.IsNullOrEmpty(FormMachineCodeFilePath) || !File.Exists(FormMachineCodeFilePath))
            {
                MessageBox.Show("文件不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(FormMachineCodeFilePath);
                AddLog($"查看机器码文件: {System.IO.Path.GetFileName(FormMachineCodeFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteClearMachineCode()
        {
            FormMachineCodeFilePath = "";
            FormMachineCode = "";
        }

        private void ExecuteOpenAuthSoftware()
        {
            try
            {
                var exePath = @"E:\公司\项目\SqlDemo\LicenseManager.WPF\bin\Debug\net472\授权码\授权码生成.exe";
                if (File.Exists(exePath))
                {
                    System.Diagnostics.Process.Start(exePath);
                    AddLog($"打开授权软件: {exePath}");
                }
                else
                {
                    MessageBox.Show($"未找到授权软件: {exePath}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExportLogs()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "文本文件|*.txt",
                    FileName = $"Logs_{DateTime.Now:yyyyMMdd_HHmmss}"
                };
                if (dialog.ShowDialog() != true) return;
                File.WriteAllLines(dialog.FileName, Logs.Reverse());
                MessageBox.Show("导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        #endregion
    }
}
