using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using CoreToolkit.Data;
using CoreToolkit.Desktop.MVVM;

namespace MotionTest.WPF.ViewModels
{
    /// <summary>
    /// 安全事件历史日志条目视图模型
    /// </summary>
    public class SafetyHistoryLogViewModel : ObservableObject
    {
        private long _id;
        private string _level;
        private string _message;
        private DateTime _timestamp;
        private string _loggerName;
        private string _additionalInfo;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public string LoggerName
        {
            get => _loggerName;
            set => SetProperty(ref _loggerName, value);
        }

        public string AdditionalInfo
        {
            get => _additionalInfo;
            set => SetProperty(ref _additionalInfo, value);
        }

        public string TimeText => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        public Brush LevelBrush
        {
            get
            {
                switch (Level?.ToLower())
                {
                    case "fatal": return Brushes.DarkRed;
                    case "error": return Brushes.Red;
                    case "warning": return Brushes.Orange;
                    case "info": return Brushes.DodgerBlue;
                    case "debug": return Brushes.Gray;
                    default: return Brushes.Black;
                }
            }
        }
    }

    /// <summary>
    /// 安全事件历史查询视图模型
    /// </summary>
    public class SafetyEventHistoryViewModel : ObservableObject
    {
        private readonly LogRepository _logRepo;

        private DateTime _startDate = DateTime.Now.AddDays(-7);
        private DateTime _endDate = DateTime.Now.AddDays(1);
        private string _searchKeyword = "";
        private int _resultCount;
        private bool _includeInfo = true;
        private bool _includeWarning = true;
        private bool _includeError = true;
        private bool _includeFatal = true;

        public SafetyEventHistoryViewModel(LogRepository logRepo)
        {
            _logRepo = logRepo;

            Logs = new ObservableCollection<SafetyHistoryLogViewModel>();

            SearchCommand = new RelayCommand(ExecuteSearch);
            RefreshCommand = new RelayCommand(ExecuteSearch);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            ExportCommand = new RelayCommand(ExecuteExport);

            // 初始加载
            ExecuteSearch();
        }

        #region 属性

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    ExecuteSearch();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    ExecuteSearch();
            }
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                    ExecuteSearch();
            }
        }

        public int ResultCount
        {
            get => _resultCount;
            set => SetProperty(ref _resultCount, value);
        }

        public bool IncludeInfo
        {
            get => _includeInfo;
            set
            {
                if (SetProperty(ref _includeInfo, value))
                    ExecuteSearch();
            }
        }

        public bool IncludeWarning
        {
            get => _includeWarning;
            set
            {
                if (SetProperty(ref _includeWarning, value))
                    ExecuteSearch();
            }
        }

        public bool IncludeError
        {
            get => _includeError;
            set
            {
                if (SetProperty(ref _includeError, value))
                    ExecuteSearch();
            }
        }

        public bool IncludeFatal
        {
            get => _includeFatal;
            set
            {
                if (SetProperty(ref _includeFatal, value))
                    ExecuteSearch();
            }
        }

        public ObservableCollection<SafetyHistoryLogViewModel> Logs { get; }

        #endregion

        #region 命令

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region 命令执行

        private void ExecuteSearch()
        {
            try
            {
                Logs.Clear();

                if (_logRepo == null)
                {
                    ResultCount = 0;
                    return;
                }

                var allLogs = _logRepo.GetLogsByTimeRange(StartDate, EndDate)
                    .Where(l => string.IsNullOrEmpty(SearchKeyword) ||
                                (l.Message != null && l.Message.Contains(SearchKeyword)) ||
                                (l.LoggerName != null && l.LoggerName.Contains(SearchKeyword)))
                    .Where(l => IsLevelIncluded(l.Level))
                    .OrderByDescending(l => l.Timestamp)
                    .Take(500); // 最多显示500条，避免UI卡顿

                foreach (var log in allLogs)
                {
                    Logs.Add(new SafetyHistoryLogViewModel
                    {
                        Id = log.Id,
                        Level = log.Level,
                        Message = log.Message,
                        Timestamp = log.Timestamp,
                        LoggerName = log.LoggerName,
                        AdditionalInfo = log.AdditionalInfo
                    });
                }

                ResultCount = Logs.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyEventHistory] 查询失败: {ex.Message}");
            }
        }

        private bool IsLevelIncluded(string level)
        {
            return level?.ToLower() switch
            {
                "info" => IncludeInfo,
                "warning" => IncludeWarning,
                "error" => IncludeError,
                "fatal" => IncludeFatal,
                _ => true
            };
        }

        private void ExecuteClearFilters()
        {
            StartDate = DateTime.Now.AddDays(-7);
            EndDate = DateTime.Now.AddDays(1);
            SearchKeyword = "";
            IncludeInfo = true;
            IncludeWarning = true;
            IncludeError = true;
            IncludeFatal = true;
            ExecuteSearch();
        }

        private void ExecuteExport()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV 文件 (*.csv)|*.csv|JSON 文件 (*.json)|*.json",
                    DefaultExt = "csv",
                    FileName = $"SafetyEvents_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    if (dialog.FileName.EndsWith(".csv"))
                    {
                        ExportToCsv(dialog.FileName);
                    }
                    else
                    {
                        ExportToJson(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyEventHistory] 导出失败: {ex.Message}");
            }
        }

        private void ExportToCsv(string filePath)
        {
            var lines = new System.Collections.Generic.List<string>();
            lines.Add("Id,Timestamp,Level,LoggerName,Message,AdditionalInfo");

            foreach (var log in Logs)
            {
                lines.Add($"{log.Id},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Level},{EscapeCsv(log.LoggerName)},{EscapeCsv(log.Message)},{EscapeCsv(log.AdditionalInfo)}");
            }

            System.IO.File.WriteAllLines(filePath, lines);
        }

        private void ExportToJson(string filePath)
        {
            var data = Logs.Select(l => new
            {
                l.Id,
                l.Timestamp,
                l.Level,
                l.LoggerName,
                l.Message,
                l.AdditionalInfo
            }).ToList();

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(filePath, json);
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        #endregion
    }
}
