using System;

namespace CoreToolkit.Algorithm.Models
{
    /// <summary>
    /// 二维点
    /// </summary>
    public struct Point2D
    {
        /// <summary>
        /// 获取或设置 X 坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 获取或设置 Y 坐标
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 初始化二维点
        /// </summary>
        /// <param name="x">X 坐标</param>
        /// <param name="y">Y 坐标</param>
        public Point2D(double x, double y)
        {
            X = x; Y = y;
        }

        /// <summary>
        /// 计算到另一个点的距离
        /// </summary>
        /// <param name="other">另一个点</param>
        /// <returns>两点之间的距离</returns>
        public double DistanceTo(Point2D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 转换为字符串表示
        /// </summary>
        /// <returns>格式化的字符串表示</returns>
        public override string ToString()
        {
            return $"({X:F4}, {Y:F4})";
        }
    }
}
