using System;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 数学计算辅助工具类
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        /// <summary>
        /// 浮点数相等比较（考虑精度误差）
        /// </summary>
        public static bool ApproxEqual(double a, double b, double epsilon = 1e-6)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        public static double Lerp(double start, double end, double factor)
        {
            return start + (end - start) * Clamp(factor, 0.0, 1.0);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// 将角度规范化到 [0, 360)
        /// </summary>
        public static double NormalizeAngle(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0) angle += 360.0;
            return angle;
        }
    }
}
