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
        private string _formDepartment = "软件";
        private string _formOperator = "";
        private string _formApplicant = "";
        private string _formProjectNumber = "";
        private string _formDeviceNumber = "";
        private string _formDeviceType = "";
        private string _formMachineCode = "";

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
        public DateTime FormRecordTime { get => _formRecordTime; set => SetProperty(ref _formRecordTime, value); }
        public string FormDepartment { get => _formDepartment; set => SetProperty(ref _formDepartment, value); }
        public string FormOperatorName { get => _formOperator; set => SetProperty(ref _formOperator, value); }
        public string FormApplicant { get => _formApplicant; set => SetProperty(ref _formApplicant, value); }
        public string FormProjectNumber { get => _formProjectNumber; set => SetProperty(ref _formProjectNumber, value); }
        public string FormDeviceNumber { get => _formDeviceNumber; set => SetProperty(ref _formDeviceNumber, value); }
        public string FormDeviceType { get => _formDeviceType; set => SetProperty(ref _formDeviceType, value); }
        public string FormMachineCode { get => _formMachineCode; set => SetProperty(ref _formMachineCode, value); }

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

        #endregion

        #region 方法

        private void LoadOptions()
        {
            try
            {
                DepartmentOptions = new ObservableCollection<string>(new[] { "" }.Concat(_repository.GetDistinctDepartments()));
                OperatorOptions = new ObservableCollection<string>(new[] { "" }.Concat(_repository.GetDistinctOperators()));
                DeviceTypeOptions = new ObservableCollection<string>(new[] { "" }.Concat(_repository.GetDistinctDeviceTypes()));
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
                if (_repository.MachineCodeExists(FormMachineCode))
                {
                    var result = MessageBox.Show("该机器码已存在，是否继续保存？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes) return;
                }

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
                ExecuteSearch();
                ExecuteRefreshStats();
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteClearForm()
        {
            FormRecordTime = DateTime.Now;
            FormDepartment = DepartmentOptions.FirstOrDefault(o => !string.IsNullOrEmpty(o)) ?? "";
            FormOperatorName = "";
            FormApplicant = "";
            FormProjectNumber = "";
            FormDeviceNumber = "";
            FormDeviceType = "";
            FormMachineCode = "";
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
