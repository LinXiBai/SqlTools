using System.Collections.Generic;
using CoreToolkit.Algorithm.Models;

namespace CoreToolkit.Algorithm.Helpers
{
    /// <summary>
    /// 补偿算法辅助类（线性插值补偿、区域补偿）
    /// </summary>
    public static class CompensationHelper
    {
        /// <summary>
        /// 双线性插值补偿
        /// </summary>
        /// <param name="x">输入 X 坐标</param>
        /// <param name="y">输入 Y 坐标</param>
        /// <param name="gridPoints">网格点坐标列表</param>
        /// <param name="errors">对应网格点的误差值</param>
        /// <returns>补偿后的坐标偏移量</returns>
        public static (double cx, double cy) BilinearCompensate(
            double x, double y,
            IList<Point2D> gridPoints,
            IList<Point2D> errors)
        {
            // 简化实现：找最近的四个网格点，做加权平均
            double totalWeight = 0;
            double sumEx = 0, sumEy = 0;

            for (int i = 0; i < gridPoints.Count; i++)
            {
                double dist = gridPoints[i].DistanceTo(new Point2D(x, y));
                if (dist < 1e-6)
                    return (errors[i].X, errors[i].Y);

                double weight = 1.0 / (dist * dist);
                sumEx += errors[i].X * weight;
                sumEy += errors[i].Y * weight;
                totalWeight += weight;
            }

            if (totalWeight == 0) return (0, 0);
            return (sumEx / totalWeight, sumEy / totalWeight);
        }

        /// <summary>
        /// 一维线性补偿（常用于贴片机单轴补偿表）
        /// </summary>
        /// <param name="input">输入值</param>
        /// <param name="positions">位置点列表</param>
        /// <param name="errors">对应位置点的误差值</param>
        /// <returns>补偿值</returns>
        public static double LinearCompensate(double input, IList<double> positions, IList<double> errors)
        {
            if (positions.Count == 0 || positions.Count != errors.Count) return 0;
            if (input <= positions[0]) return errors[0];
            if (input >= positions[positions.Count - 1]) return errors[errors.Count - 1];

            for (int i = 0; i < positions.Count - 1; i++)
            {
                if (input >= positions[i] && input <= positions[i + 1])
                {
                    double t = (input - positions[i]) / (positions[i + 1] - positions[i]);
                    return errors[i] + t * (errors[i + 1] - errors[i]);
                }
            }
            return 0;
        }
    }
}
