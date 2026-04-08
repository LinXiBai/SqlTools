using System;
using System.Collections.Generic;
using System.Linq;
using CoreToolkit.Algorithm.Helpers;
using CoreToolkit.Vision.Models;

namespace CoreToolkit.Vision.Helpers
{
    /// <summary>
    /// 标定辅助类：九点标定、旋转中心计算
    /// </summary>
    public static class CalibrationHelper
    {
        /// <summary>
        /// 使用最小二乘法进行九点标定，返回 CoordinateTransform
        /// </summary>
        /// <param name="points">标定数据点列表，至少需要3组数据</param>
        /// <returns>坐标变换对象</returns>
        /// <exception cref="ArgumentException">当标定数据点少于3组时抛出</exception>
        /// <exception cref="InvalidOperationException">当标定点共线或数据异常时抛出</exception>
        public static CoordinateTransform NinePointCalibration(IList<CalibrationData> points)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("至少需要 3 组标定数据。", nameof(points));

            int n = points.Count;
            double sumPx = 0, sumPy = 0, sumMx = 0, sumMy = 0;
            foreach (var p in points)
            {
                sumPx += p.PixelX; sumPy += p.PixelY;
                sumMx += p.MachineX; sumMy += p.MachineY;
            }

            double avgPx = sumPx / n, avgPy = sumPy / n;
            double avgMx = sumMx / n, avgMy = sumMy / n;

            double sxx = 0, sxy = 0, syy = 0;
            double sxm = 0, sym = 0, symx = 0, sxmy = 0;

            foreach (var p in points)
            {
                double dx = p.PixelX - avgPx;
                double dy = p.PixelY - avgPy;
                double dmx = p.MachineX - avgMx;
                double dmy = p.MachineY - avgMy;

                sxx += dx * dx; sxy += dx * dy; syy += dy * dy;
                sxm += dx * dmx; sym += dy * dmy;
                symx += dy * dmx; sxmy += dx * dmy;
            }

            double denominator = sxx * syy - sxy * sxy;
            if (Math.Abs(denominator) < 1e-10)
                throw new InvalidOperationException("标定点共线或数据异常，无法计算。");

            double a = (sxm * syy - sxmy * sxy) / denominator;
            double b = (sxmy * sxx - sxm * sxy) / denominator;
            double c = (symx * syy - sym * sxy) / denominator;
            double d = (sym * sxx - symx * sxy) / denominator;

            double offsetX = avgMx - a * avgPx - b * avgPy;
            double offsetY = avgMy - c * avgPx - d * avgPy;

            double scaleX = Math.Sqrt(a * a + c * c);
            double scaleY = Math.Sqrt(b * b + d * d);
            double angle = Math.Atan2(c, a);

            return new CoordinateTransform
            {
                ScaleX = scaleX,
                ScaleY = scaleY,
                AngleRad = angle,
                OffsetX = offsetX,
                OffsetY = offsetY
            };
        }

        /// <summary>
        /// 计算旋转中心（通过绕某点旋转两个角度，记录机械坐标变化）
        /// </summary>
        /// <param name="angle1Deg">角度1（度）</param>
        /// <param name="x1">角度1对应的机械X</param>
        /// <param name="y1">角度1对应的机械Y</param>
        /// <param name="angle2Deg">角度2（度）</param>
        /// <param name="x2">角度2对应的机械X</param>
        /// <param name="y2">角度2对应的机械Y</param>
        /// <returns>旋转中心坐标 (cx, cy)</returns>
        /// <exception cref="ArgumentException">当两个角度相同时抛出</exception>
        public static (double cx, double cy) CalculateRotationCenter(double angle1Deg, double x1, double y1, double angle2Deg, double x2, double y2)
        {
            if (Math.Abs(angle1Deg - angle2Deg) < 1e-6)
                throw new ArgumentException("两个角度不能相同。");

            double a1 = GeometryHelper.DegreesToRadians(angle1Deg);
            double a2 = GeometryHelper.DegreesToRadians(angle2Deg);

            double cos1 = Math.Cos(a1), sin1 = Math.Sin(a1);
            double cos2 = Math.Cos(a2), sin2 = Math.Sin(a2);

            // 解线性方程组求旋转中心
            double A = 1 - cos1, B = sin1, C = x1 - cos1 * x1 + sin1 * y1;
            double D = -sin1, E = 1 - cos1, F = y1 + sin1 * x1 - cos1 * y1;
            double G = 1 - cos2, H = sin2, I = x2 - cos2 * x2 + sin2 * y2;
            double J = -sin2, K = 1 - cos2, L = y2 + sin2 * x2 - cos2 * y2;

            // 最小二乘近似解
            double m11 = A * A + D * D + G * G + J * J;
            double m12 = A * B + D * E + G * H + J * K;
            double m21 = A * B + D * E + G * H + J * K;
            double m22 = B * B + E * E + H * H + K * K;
            double v1 = A * C + D * F + G * I + J * L;
            double v2 = B * C + E * F + H * I + K * L;

            double det = m11 * m22 - m12 * m21;
            if (Math.Abs(det) < 1e-10) return (0, 0);

            double cx = (v1 * m22 - m12 * v2) / det;
            double cy = (m11 * v2 - v1 * m21) / det;
            return (cx, cy);
        }
    }
}
