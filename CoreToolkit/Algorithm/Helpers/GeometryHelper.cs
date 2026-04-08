using System;
using CoreToolkit.Algorithm.Models;

namespace CoreToolkit.Algorithm.Helpers
{
    /// <summary>
    /// 几何计算辅助类
    /// </summary>
    public static class GeometryHelper
    {
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degrees">角度值</param>
        /// <returns>弧度值</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="radians">弧度值</param>
        /// <returns>角度值</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// 两点间距离
        /// </summary>
        /// <param name="x1">第一个点的 X 坐标</param>
        /// <param name="y1">第一个点的 Y 坐标</param>
        /// <param name="x2">第二个点的 X 坐标</param>
        /// <param name="y2">第二个点的 Y 坐标</param>
        /// <returns>两点之间的距离</returns>
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1, dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 点绕中心旋转后的新坐标
        /// </summary>
        /// <param name="px">要旋转的点的 X 坐标</param>
        /// <param name="py">要旋转的点的 Y 坐标</param>
        /// <param name="cx">旋转中心的 X 坐标</param>
        /// <param name="cy">旋转中心的 Y 坐标</param>
        /// <param name="angleRad">旋转角度（弧度）</param>
        /// <returns>旋转后的新坐标</returns>
        public static (double x, double y) RotatePoint(double px, double py, double cx, double cy, double angleRad)
        {
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            double dx = px - cx;
            double dy = py - cy;
            double rx = dx * cos - dy * sin + cx;
            double ry = dx * sin + dy * cos + cy;
            return (rx, ry);
        }

        /// <summary>
        /// 求两直线交点（已知两点和角度）
        /// </summary>
        /// <param name="x1">第一条直线上一点的 X 坐标</param>
        /// <param name="y1">第一条直线上一点的 Y 坐标</param>
        /// <param name="angle1Rad">第一条直线的角度（弧度）</param>
        /// <param name="x2">第二条直线上一点的 X 坐标</param>
        /// <param name="y2">第二条直线上一点的 Y 坐标</param>
        /// <param name="angle2Rad">第二条直线的角度（弧度）</param>
        /// <returns>两直线的交点，如果平行则返回 null</returns>
        public static Point2D? Intersection(double x1, double y1, double angle1Rad, double x2, double y2, double angle2Rad)
        {
            // 方向向量
            double dx1 = Math.Cos(angle1Rad), dy1 = Math.Sin(angle1Rad);
            double dx2 = Math.Cos(angle2Rad), dy2 = Math.Sin(angle2Rad);
            
            // 计算叉积判断平行
            double cross = dx1 * dy2 - dy1 * dx2;
            if (Math.Abs(cross) < 1e-10) return null; // 平行
            
            // 参数方程求解
            double dx = x2 - x1;
            double dy = y2 - y1;
            double t = (dx * dy2 - dy * dx2) / cross;
            
            double x = x1 + t * dx1;
            double y = y1 + t * dy1;
            return new Point2D(x, y);
        }

        /// <summary>
        /// 三点求圆心
        /// </summary>
        /// <param name="x1">第一个点的 X 坐标</param>
        /// <param name="y1">第一个点的 Y 坐标</param>
        /// <param name="x2">第二个点的 X 坐标</param>
        /// <param name="y2">第二个点的 Y 坐标</param>
        /// <param name="x3">第三个点的 X 坐标</param>
        /// <param name="y3">第三个点的 Y 坐标</param>
        /// <returns>圆心坐标，如果三点共线则返回 null</returns>
        public static Point2D? CircleCenter(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a = x2 - x1, b = y2 - y1, c = x3 - x1, d = y3 - y1;
            double e = a * (x1 + x2) + b * (y1 + y2);
            double f = c * (x1 + x3) + d * (y1 + y3);
            double g = 2 * (a * (y3 - y2) - b * (x3 - x2));
            if (Math.Abs(g) < 1e-10) return null; // 共线
            double cx = (d * e - b * f) / g;
            double cy = (a * f - c * e) / g;
            return new Point2D(cx, cy);
        }
    }
}
