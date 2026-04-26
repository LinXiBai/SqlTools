using System;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 日期时间辅助工具类
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        public static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取当前时间戳（秒）
        /// </summary>
        public static long GetTimestampSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 将时间戳（毫秒）转换为本地时间
        /// </summary>
        public static DateTime FromTimestamp(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
        }

        /// <summary>
        /// 获取不带毫秒的当前时间（适合数据库插入）
        /// </summary>
        public static DateTime NowWithoutMilliseconds()
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        /// <summary>
        /// 获取当月开始时间
        /// </summary>
        public static DateTime StartOfMonth(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return new DateTime(d.Year, d.Month, 1, 0, 0, 0);
        }

        /// <summary>
        /// 获取当月结束时间
        /// </summary>
        public static DateTime EndOfMonth(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month), 23, 59, 59);
        }
    }
}
