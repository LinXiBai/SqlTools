using System;
using CoreToolkit.Vision.Models;

namespace CoreToolkit.Vision.Helpers
{
    /// <summary>
    /// 像素坐标与机械坐标转换工具
    /// </summary>
    public class CoordinateTransform
    {
        private double _angleRad = 0.0;
        private double _cosAngle = 1.0;
        private double _sinAngle = 0.0;

        public double ScaleX { get; set; } = 1.0;   // 像素/微米 或 像素/毫米
        public double ScaleY { get; set; } = 1.0;

        /// <summary>
        /// 像素坐标系与机械坐标系的旋转角（弧度）
        /// </summary>
        public double AngleRad
        {
            get => _angleRad;
            set
            {
                _angleRad = value;
                _cosAngle = Math.Cos(value);
                _sinAngle = Math.Sin(value);
            }
        }

        public double OffsetX { get; set; } = 0.0;  // 平移偏移
        public double OffsetY { get; set; } = 0.0;

        /// <summary>
        /// 像素坐标 -> 机械坐标
        /// </summary>
        public (double mx, double my) PixelToMachine(double px, double py)
        {
            double rx = px * ScaleX;
            double ry = py * ScaleY;
            double mx = rx * _cosAngle - ry * _sinAngle + OffsetX;
            double my = rx * _sinAngle + ry * _cosAngle + OffsetY;
            return (mx, my);
        }

        /// <summary>
        /// 机械坐标 -> 像素坐标
        /// </summary>
        public (double px, double py) MachineToPixel(double mx, double my)
        {
            double dx = mx - OffsetX;
            double dy = my - OffsetY;
            // cos(-x) = cos(x), sin(-x) = -sin(x)
            double rx = dx * _cosAngle + dy * _sinAngle;
            double ry = -dx * _sinAngle + dy * _cosAngle;
            return (rx / ScaleX, ry / ScaleY);
        }
    }
}
