using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using CoreToolkit.Data;
using CoreToolkit.Data.Models;
using CoreToolkit.Desktop.MVVM;

namespace MotionTest.WPF.ViewModels
{
    /// <summary>
    /// 日报表数据项（用于柱状图和表格）
    /// </summary>
    public class DailyReportItem : ObservableObject
    {
        public DateTime Date { get; set; }
        public string DateLabel => Date.ToString("MM-dd");
        public int FatalCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int TotalCount => FatalCount + ErrorCount + WarningCount + InfoCount;

        // 柱状图高度比例（相对当天最大值）
        public double FatalBarHeight => MaxCount > 0 ? (double)FatalCount / MaxCount * 100 : 0;
        public double ErrorBarHeight => MaxCount > 0 ? (double)ErrorCount / MaxCount * 100 : 0;
        public double WarningBarHeight => MaxCount > 0 ? (double)WarningCount / MaxCount * 100 : 0;
        public double InfoBarHeight => MaxCount > 0 ? (double)InfoCount / MaxCount * 100 : 0;

        // 用于计算比例的参考值（由外部设置）
        public int MaxCount { get; set; } = 1;
    }

    /// <summary>
    /// 统计卡片数据
    /// </summary>
    public class StatCard : ObservableObject
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public string Icon { get; set; }
    }

    /// <summary>
    /// 安全事件统计报表视图模型
    /// </summary>
    public class SafetyReportViewModel : ObservableObject
    {
        private readonly LogRepository _logRepo;

        private DateTime _startDate = DateTime.Now.AddDays(-7);
        private DateTime _endDate = DateTime.Now.AddDays(1);
        private string _reportTitle = "安全事件统计报表";
        private int _totalEvents;
        private int _fatalEvents;
        private int _errorEvents;
        private int _warningEvents;
        private int _infoEvents;

        public SafetyReportViewModel(LogRepository logRepo)
        {
            _logRepo = logRepo;

            DailyItems = new ObservableCollection<DailyReportItem>();
            StatCards = new ObservableCollection<StatCard>();

            TodayCommand = new RelayCommand(() => SetRange(DateTime.Now, DateTime.Now.AddDays(1)));
            ThisWeekCommand = new RelayCommand(() =>
            {
                var today = DateTime.Now;
                var start = today.AddDays(-(int)today.DayOfWeek);
                SetRange(start, today.AddDays(1));
            });
            ThisMonthCommand = new RelayCommand(() =>
            {
                var today = DateTime.Now;
                SetRange(new DateTime(today.Year, today.Month, 1), today.AddDays(1));
            });
            Last7DaysCommand = new RelayCommand(() => SetRange(DateTime.Now.AddDays(-7), DateTime.Now.AddDays(1)));
            Last30DaysCommand = new RelayCommand(() => SetRange(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(1)));
            RefreshCommand = new RelayCommand(RefreshData);
            ExportCommand = new RelayCommand(ExportReport);

            RefreshData();
        }

        #region 属性

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    RefreshData();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    RefreshData();
            }
        }

        public string ReportTitle
        {
            get => _reportTitle;
            set => SetProperty(ref _reportTitle, value);
        }

        public int TotalEvents
        {
            get => _totalEvents;
            set => SetProperty(ref _totalEvents, value);
        }

        public int FatalEvents
        {
            get => _fatalEvents;
            set => SetProperty(ref _fatalEvents, value);
        }

        public int ErrorEvents
        {
            get => _errorEvents;
            set => SetProperty(ref _errorEvents, value);
        }

        public int WarningEvents
        {
            get => _warningEvents;
            set => SetProperty(ref _warningEvents, value);
        }

        public int InfoEvents
        {
            get => _infoEvents;
            set => SetProperty(ref _infoEvents, value);
        }

        public ObservableCollection<DailyReportItem> DailyItems { get; }
        public ObservableCollection<StatCard> StatCards { get; }

        #endregion

        #region 命令

        public ICommand TodayCommand { get; }
        public ICommand ThisWeekCommand { get; }
        public ICommand ThisMonthCommand { get; }
        public ICommand Last7DaysCommand { get; }
        public ICommand Last30DaysCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region 方法

        private void SetRange(DateTime start, DateTime end)
        {
            _startDate = start;
            _endDate = end;
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                if (_logRepo == null)
                {
                    ClearData();
                    return;
                }

                // 1. 查询摘要统计
                var summary = _logRepo.GetSafetyEventSummary(StartDate, EndDate);
                TotalEvents = summary.TotalCount;
                FatalEvents = summary.FatalCount;
                ErrorEvents = summary.ErrorCount;
                WarningEvents = summary.WarningCount;
                InfoEvents = summary.InfoCount;

                // 2. 更新统计卡片
                UpdateStatCards();

                // 3. 查询按天明细
                var dailyStats = _logRepo.GetSafetyEventDailyStats(StartDate, EndDate);
                BuildDailyItems(dailyStats);

                // 4. 更新标题
                ReportTitle = $"安全事件统计报表 ({StartDate:yyyy-MM-dd} ~ {EndDate:yyyy-MM-dd})";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyReport] 刷新失败: {ex.Message}");
            }
        }

        private void ClearData()
        {
            TotalEvents = FatalEvents = ErrorEvents = WarningEvents = InfoEvents = 0;
            DailyItems.Clear();
            StatCards.Clear();
        }

        private void UpdateStatCards()
        {
            StatCards.Clear();
            StatCards.Add(new StatCard
            {
                Title = "总事件数",
                Count = TotalEvents,
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                ForegroundBrush = Brushes.White,
                Icon = "📊"
            });
            StatCards.Add(new StatCard
            {
                Title = "急停 (Fatal)",
                Count = FatalEvents,
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                ForegroundBrush = Brushes.White,
                Icon = "🛑"
            });
            StatCards.Add(new StatCard
            {
                Title = "碰撞 (Error)",
                Count = ErrorEvents,
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                ForegroundBrush = Brushes.White,
                Icon = "💥"
            });
            StatCards.Add(new StatCard
            {
                Title = "互锁 (Warning)",
                Count = WarningEvents,
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                ForegroundBrush = Brushes.White,
                Icon = "⚠️"
            });
            StatCards.Add(new StatCard
            {
                Title = "信息 (Info)",
                Count = InfoEvents,
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                ForegroundBrush = Brushes.White,
                Icon = "ℹ️"
            });
        }

        private void BuildDailyItems(IEnumerable<SafetyEventDailyStat> dailyStats)
        {
            DailyItems.Clear();

            // 生成日期范围内的所有日期
            var dateRange = Enumerable.Range(0, (EndDate - StartDate).Days + 1)
                .Select(i => StartDate.AddDays(i).Date)
                .ToList();

            // 按日期建立字典（模型已聚合）
            var grouped = dailyStats
                .ToDictionary(s => s.Date.Date, s => s);

            var items = new List<DailyReportItem>();
            foreach (var date in dateRange)
            {
                var item = new DailyReportItem { Date = date };
                if (grouped.TryGetValue(date, out var stat))
                {
                    item.FatalCount = stat.EStopCount;
                    item.ErrorCount = stat.CollisionCount;
                    item.WarningCount = stat.InterlockCount;
                    item.InfoCount = stat.InfoCount;
                }
                items.Add(item);
            }

            // 计算最大值用于柱状图比例
            int maxCount = items.Any() ? items.Max(i => i.TotalCount) : 1;
            if (maxCount == 0) maxCount = 1;
            foreach (var item in items)
            {
                item.MaxCount = maxCount;
                DailyItems.Add(item);
            }
        }

        private void ExportReport()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV 文件 (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"SafetyReport_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var lines = new System.Collections.Generic.List<string>();
                    lines.Add("日期,急停(Fatal),碰撞(Error),互锁(Warning),信息(Info),合计");
                    foreach (var item in DailyItems)
                    {
                        lines.Add($"{item.Date:yyyy-MM-dd},{item.FatalCount},{item.ErrorCount},{item.WarningCount},{item.InfoCount},{item.TotalCount}");
                    }
                    lines.Add($"合计,{FatalEvents},{ErrorEvents},{WarningEvents},{InfoEvents},{TotalEvents}");
                    System.IO.File.WriteAllLines(dialog.FileName, lines);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafetyReport] 导出失败: {ex.Message}");
            }
        }

        #endregion
    }
}
