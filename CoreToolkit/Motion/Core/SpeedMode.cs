namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴速度模式
    /// </summary>
    public enum SpeedMode
    {
        /// <summary>
        /// 高速模式 - 使用数据库中存储的100%参数
        /// </summary>
        High = 0,

        /// <summary>
        /// 中速模式 - 使用高速参数的50%
        /// </summary>
        Medium = 1,

        /// <summary>
        /// 慢速模式 - 使用高速参数的10%
        /// </summary>
        Slow = 2
    }

    /// <summary>
    /// 速度模式扩展方法
    /// </summary>
    public static class SpeedModeExtensions
    {
        /// <summary>
        /// 获取速度模式的缩放比例
        /// </summary>
        public static double GetScaleFactor(this SpeedMode mode)
        {
            switch (mode)
            {
                case SpeedMode.High:
                    return 1.0;     // 100%
                case SpeedMode.Medium:
                    return 0.5;     // 50%
                case SpeedMode.Slow:
                    return 0.1;     // 10%
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// 获取速度模式的显示名称
        /// </summary>
        public static string GetDisplayName(this SpeedMode mode)
        {
            switch (mode)
            {
                case SpeedMode.High:
                    return "高速";
                case SpeedMode.Medium:
                    return "中速";
                case SpeedMode.Slow:
                    return "慢速";
                default:
                    return "未知";
            }
        }
    }
}
