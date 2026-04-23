using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using CoreToolkit.Data;
using CoreToolkit.Data.Models;
using CoreToolkit.Desktop.MVVM;
using Newtonsoft.Json;

namespace MotionTest.WPF.ViewModels
{
    /// <summary>
    /// 统计周期类型
    /// </summary>
    public enum StatPeriodType
    {
        Day,
        Week,
        Month
    }

    /// <summary>
    /// 安全事件统计行视图模型（用于 DataGrid 绑定）
    /// </summary>
    public class SafetyStatRowViewModel : ObservableObject
    {
        private string _period;
        private DateTime _periodStart;
        private DateTime _periodEnd;
        private int _collisionCount;
        private int _interlockCount;
        private int _eStopCount;
        private int _infoCount;
        private int _totalCount;

        public string Period
        {
            get => _period;
            set => SetProperty(ref _period, value);
        }

        /// <summary>
        /// 周期开始日期（用于穿透查询）
        /// </summary>
        public DateTime PeriodStart
        {
            get => _periodStart;
            set => SetProperty(ref _periodStart, value);
        }

        /// <summary>
        /// 周期结束日期（用于穿透查询）
        /// </summary>
        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set => SetProperty(ref _periodEnd, value);
        }

        public int CollisionCount
        {
            get => _collisionCount;
            set => SetProperty(ref _collisionCount, value);
        }

        public int InterlockCount
        {
            get => _interlockCount;
            set => SetProperty(ref _interlockCount, value);
        }

        public int EStopCount
        {
            get => _eStopCount;
            set => SetProperty(ref _eStopCount, value);
        }

        public int InfoCount
        {
            get => _infoCount;
            set => SetProperty(ref _infoCount, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public Brush CollisionBrush => CollisionCount > 0 ? Brushes.Red : Brushes.Gray;
        public Brush InterlockBrush => InterlockCount > 0 ? Brushes.Orange : Brushes.Gray;
        public Brush EStopBrush => EStopCount > 0 ? Brushes.DarkRed : Brushes.Gray;
    }

    /// <summary>
    /// 安全事件统计报表视图模型
    /// </summary>
    public class SafetyStatisticsViewModel : ObservableObject
    {
        private readonly LogRepository _logRepo;

        /// <summary>
        /// 日志仓库（供外部穿透查询使用）
        /// </summary>
        public LogRepository LogRepo => _logRepo;

        private DateTime _startDate = DateTime.Now.AddDays(-30);
        private DateTime _endDate = DateTime.Now.AddDays(1);
        private StatPeriodType _periodType = StatPeriodType.Day;
        private int _totalEvents;
        private int _totalCollisions;
        private int _totalInterlocks;
        private int _totalEStops;
        private int _rowCount;
        private bool _isLoading;
        private bool _isEmpty;

        // 洞察指标
        private double _avgCollisions;
        private double _avgInterlocks;
        private double _avgEStops;
        private string _peakCollisionPeriod;
        private int _peakCollisionCount;
        private string _peakInterlockPeriod;
        private int _peakInterlockCount;
        private string _peakEStopPeriod;
        private int _peakEStopCount;

        public SafetyStatisticsViewModel(LogRepository logRepo)
        {
            _logRepo = logRepo;
            Stats = new ObservableCollection<SafetyStatRowViewModel>();

            RefreshCommand = new RelayCommand(async () => await ExecuteRefreshAsync(), () => !IsLoading);
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv, () => Stats.Count > 0 && !IsLoading);
            ExportJsonCommand = new RelayCommand(ExecuteExportJson, () => Stats.Count > 0 && !IsLoading);

            // 快捷日期选择命令
            TodayCommand = new RelayCommand(() => SetQuickRange(0, 0));
            Last7DaysCommand = new RelayCommand(() => SetQuickRange(-7, 0));
            ThisMonthCommand = new RelayCommand(() => SetQuickRange(
                -DateTime.Now.Day + 1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - DateTime.Now.Day));
            ThisYearCommand = new RelayCommand(() => SetQuickRange(
                -DateTime.Now.DayOfYear + 1, DateTime.IsLeapYear(DateTime.Now.Year) ? 366 - DateTime.Now.DayOfYear : 365 - DateTime.Now.DayOfYear));

            // 初始加载
            _ = ExecuteRefreshAsync();
        }

        #region 属性

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    _ = ExecuteRefreshAsync();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    _ = ExecuteRefreshAsync();
            }
        }

        public StatPeriodType PeriodType
        {
            get => _periodType;
            set
            {
                if (SetProperty(ref _periodType, value))
                    _ = ExecuteRefreshAsync();
            }
        }

        public int TotalEvents
        {
            get => _totalEvents;
            set => SetProperty(ref _totalEvents, value);
        }

        public int TotalCollisions
        {
            get => _totalCollisions;
            set => SetProperty(ref _totalCollisions, value);
        }

        public int TotalInterlocks
        {
            get => _totalInterlocks;
            set => SetProperty(ref _totalInterlocks, value);
        }

        public int TotalEStops
        {
            get => _totalEStops;
            set => SetProperty(ref _totalEStops, value);
        }

        public int RowCount
        {
            get => _rowCount;
            set => SetProperty(ref _rowCount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ExportCsvCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ExportJsonCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        // 洞察指标
        public double AvgCollisions
        {
            get => _avgCollisions;
            set => SetProperty(ref _avgCollisions, value);
        }

        public double AvgInterlocks
        {
            get => _avgInterlocks;
            set => SetProperty(ref _avgInterlocks, value);
        }

        public double AvgEStops
        {
            get => _avgEStops;
            set => SetProperty(ref _avgEStops, value);
        }

        public string PeakCollisionPeriod
        {
            get => _peakCollisionPeriod;
            set => SetProperty(ref _peakCollisionPeriod, value);
        }

        public int PeakCollisionCount
        {
            get => _peakCollisionCount;
            set => SetProperty(ref _peakCollisionCount, value);
        }

        public string PeakInterlockPeriod
        {
            get => _peakInterlockPeriod;
            set => SetProperty(ref _peakInterlockPeriod, value);
        }

        public int PeakInterlockCount
        {
            get => _peakInterlockCount;
            set => SetProperty(ref _peakInterlockCount, value);
        }

        public string PeakEStopPeriod
        {
            get => _peakEStopPeriod;
            set => SetProperty(ref _peakEStopPeriod, value);
        }

        public int PeakEStopCount
        {
            get => _peakEStopCount;
            set => SetProperty(ref _peakEStopCount, value);
        }

        public ObservableCollection<SafetyStatRowViewModel> Stats { get; }

        public IEnumerable<StatPeriodType> PeriodTypes => Enum.GetValues(typeof(StatPeriodType)).Cast<StatPeriodType>();

        public string PeriodTypeDisplay => PeriodType switch
        {
            StatPeriodType.Day => "按天统计",
            StatPeriodType.Week => "按周统计",
            StatPeriodType.Month => "按月统计",
            _ => "统计"
        };

        #endregion

        #region 命令

        public ICommand RefreshCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ExportJsonCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand Last7DaysCommand { get; }
        public ICommand ThisMonthCommand { get; }
        public ICommand ThisYearCommand { get; }

        #endregion

        #region 命令执行

        private void SetQuickRange(int startOffsetDays, int endOffsetDays)
        {
            var baseDate = DateTime.Now.Date;
            StartDate = baseDate.AddDays(startOffsetDays);
            EndDate = baseDate.AddDays(endOffsetDays).AddDays(1).AddSeconds(-1); // 包含整天
            _ = ExecuteRefreshAsync();
        }

        private async Task ExecuteRefreshAsync()
        {
            try
            {
                IsLoading = true;
                Stats.Clear();

                if (_logRepo == null)
                {
                    ResetSummary();
                    IsEmpty = true;
                    return;
                }

                await Task.Run(() =>
                {
                    switch (PeriodType)
                    {
                        case StatPeriodType.Day:
                            LoadDailyStats();
                            break;
                        case StatPeriodType.Week:
                            LoadWeeklyStats();
                            break;
                        case StatPeriodType.Month:
                            LoadMonthlyStats();
                            break;
                    }
                });

                // 汇总
                TotalCollisions = Stats.Sum(s => s.CollisionCount);
                TotalInterlocks = Stats.Sum(s => s.InterlockCount);
                TotalEStops = Stats.Sum(s => s.EStopCount);
                TotalEvents = TotalCollisions + TotalInterlocks + TotalEStops + Stats.Sum(s => s.InfoCount);
                RowCount = Stats.Count;
                IsEmpty = Stats.Count == 0;

                // 洞察计算
                CalculateInsights();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyStatistics] 刷新失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadDailyStats()
        {
            var daily = _logRepo.GetSafetyEventDailyStats(StartDate, EndDate);
            foreach (var d in daily)
            {
                Stats.Add(new SafetyStatRowViewModel
                {
                    Period = d.Date.ToString("yyyy-MM-dd"),
                    PeriodStart = d.Date,
                    PeriodEnd = d.Date.AddDays(1).AddSeconds(-1),
                    CollisionCount = d.CollisionCount,
                    InterlockCount = d.InterlockCount,
                    EStopCount = d.EStopCount,
                    InfoCount = d.InfoCount,
                    TotalCount = d.TotalCount
                });
            }
        }

        private void LoadWeeklyStats()
        {
            var weekly = _logRepo.GetSafetyEventWeeklyStats(StartDate, EndDate);
            foreach (var w in weekly)
            {
                Stats.Add(new SafetyStatRowViewModel
                {
                    Period = w.Period,
                    PeriodStart = w.PeriodStart,
                    PeriodEnd = w.PeriodEnd,
                    CollisionCount = w.CollisionCount,
                    InterlockCount = w.InterlockCount,
                    EStopCount = w.EStopCount,
                    InfoCount = w.InfoCount,
                    TotalCount = w.TotalCount
                });
            }
        }

        private void LoadMonthlyStats()
        {
            var monthly = _logRepo.GetSafetyEventMonthlyStats(StartDate, EndDate);
            foreach (var m in monthly)
            {
                Stats.Add(new SafetyStatRowViewModel
                {
                    Period = m.Period,
                    PeriodStart = m.PeriodStart,
                    PeriodEnd = m.PeriodEnd,
                    CollisionCount = m.CollisionCount,
                    InterlockCount = m.InterlockCount,
                    EStopCount = m.EStopCount,
                    InfoCount = m.InfoCount,
                    TotalCount = m.TotalCount
                });
            }
        }

        private void CalculateInsights()
        {
            int count = Stats.Count;
            if (count == 0)
            {
                AvgCollisions = AvgInterlocks = AvgEStops = 0;
                PeakCollisionPeriod = PeakInterlockPeriod = PeakEStopPeriod = "-";
                PeakCollisionCount = PeakInterlockCount = PeakEStopCount = 0;
                return;
            }

            AvgCollisions = Math.Round((double)TotalCollisions / count, 2);
            AvgInterlocks = Math.Round((double)TotalInterlocks / count, 2);
            AvgEStops = Math.Round((double)TotalEStops / count, 2);

            var peakCollision = Stats.OrderByDescending(s => s.CollisionCount).First();
            PeakCollisionPeriod = peakCollision.Period;
            PeakCollisionCount = peakCollision.CollisionCount;

            var peakInterlock = Stats.OrderByDescending(s => s.InterlockCount).First();
            PeakInterlockPeriod = peakInterlock.Period;
            PeakInterlockCount = peakInterlock.InterlockCount;

            var peakEStop = Stats.OrderByDescending(s => s.EStopCount).First();
            PeakEStopPeriod = peakEStop.Period;
            PeakEStopCount = peakEStop.EStopCount;
        }

        private void ResetSummary()
        {
            TotalEvents = 0;
            TotalCollisions = 0;
            TotalInterlocks = 0;
            TotalEStops = 0;
            RowCount = 0;
            AvgCollisions = AvgInterlocks = AvgEStops = 0;
            PeakCollisionPeriod = PeakInterlockPeriod = PeakEStopPeriod = "-";
            PeakCollisionCount = PeakInterlockCount = PeakEStopCount = 0;
        }

        private void ExecuteExportCsv()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV 文件 (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"SafetyStats_{PeriodType}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var lines = new List<string>();
                    lines.Add("Period,CollisionCount,InterlockCount,EStopCount,InfoCount,TotalCount");
                    foreach (var s in Stats)
                    {
                        lines.Add($"{s.Period},{s.CollisionCount},{s.InterlockCount},{s.EStopCount},{s.InfoCount},{s.TotalCount}");
                    }
                    lines.Add($"Total,{TotalCollisions},{TotalInterlocks},{TotalEStops},{Stats.Sum(s => s.InfoCount)},{TotalEvents}");
                    File.WriteAllLines(dialog.FileName, lines);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyStatistics] CSV导出失败: {ex.Message}");
            }
        }

        private void ExecuteExportJson()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON 文件 (*.json)|*.json",
                    DefaultExt = "json",
                    FileName = $"SafetyStats_{PeriodType}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var data = new
                    {
                        PeriodType = PeriodType.ToString(),
                        StartDate,
                        EndDate,
                        Summary = new
                        {
                            TotalEvents,
                            TotalCollisions,
                            TotalInterlocks,
                            TotalEStops
                        },
                        Records = Stats.Select(s => new
                        {
                            s.Period,
                            s.CollisionCount,
                            s.InterlockCount,
                            s.EStopCount,
                            s.InfoCount,
                            s.TotalCount
                        }).ToList()
                    };
                    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyStatistics] JSON导出失败: {ex.Message}");
            }
        }

        #endregion
    }
}
