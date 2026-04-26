using System;

namespace CoreToolkit.Data.Models
{
    /// <summary>
    /// 安全事件级别统计
    /// </summary>
    public class SafetyEventStatistic
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 事件数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 安全事件按天统计（已聚合碰撞/互锁/急停）
    /// </summary>
    public class SafetyEventDailyStat
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 碰撞次数（Error级别）
        /// </summary>
        public int CollisionCount { get; set; }

        /// <summary>
        /// 互锁次数（Warning级别）
        /// </summary>
        public int InterlockCount { get; set; }

        /// <summary>
        /// 急停次数（Fatal级别）
        /// </summary>
        public int EStopCount { get; set; }

        /// <summary>
        /// 信息次数（Info级别）
        /// </summary>
        public int InfoCount { get; set; }

        /// <summary>
        /// 总事件数
        /// </summary>
        public int TotalCount => CollisionCount + InterlockCount + EStopCount + InfoCount;
    }

    /// <summary>
    /// 安全事件周期统计（周/月）
    /// </summary>
    public class SafetyEventPeriodStat
    {
        /// <summary>
        /// 周期标识（如 "2024-W03" 或 "2024-04"）
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// 周期开始日期
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// 周期结束日期
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// 碰撞次数
        /// </summary>
        public int CollisionCount { get; set; }

        /// <summary>
        /// 互锁次数
        /// </summary>
        public int InterlockCount { get; set; }

        /// <summary>
        /// 急停次数
        /// </summary>
        public int EStopCount { get; set; }

        /// <summary>
        /// 信息次数
        /// </summary>
        public int InfoCount { get; set; }

        /// <summary>
        /// 总事件数
        /// </summary>
        public int TotalCount => CollisionCount + InterlockCount + EStopCount + InfoCount;
    }

    /// <summary>
    /// 安全事件摘要统计
    /// </summary>
    public class SafetyEventSummary
    {
        /// <summary>
        /// 总事件数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 致命错误数（急停）
        /// </summary>
        public int FatalCount { get; set; }

        /// <summary>
        /// 错误数（碰撞）
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// 警告数（互锁）
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// 信息数（监控启停等）
        /// </summary>
        public int InfoCount { get; set; }
    }
}
