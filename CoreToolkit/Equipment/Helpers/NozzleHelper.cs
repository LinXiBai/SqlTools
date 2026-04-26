using System;
using CoreToolkit.Common.Helpers;
using CoreToolkit.Equipment.Models;

namespace CoreToolkit.Equipment.Helpers
{
    /// <summary>
    /// 吸嘴相关辅助计算
    /// </summary>
    public static class NozzleHelper
    {
        /// <summary>
        /// 根据元件尺寸推荐吸嘴外径（经验公式，可调整）
        /// </summary>
        public static double RecommendOuterDiameter(double componentWidth, double componentLength)
        {
            double maxSide = Math.Max(componentWidth, componentLength);
            return maxSide * 0.8;
        }

        /// <summary>
        /// 估算真空值（kPa），根据元件重量和吸嘴面积
        /// </summary>
        public static int EstimateVacuumLevel(double componentWeightGram, double nozzleAreaMm2)
        {
            if (nozzleAreaMm2 <= 0) return 0;
            double pressure = (componentWeightGram * 0.0098) / (nozzleAreaMm2 * 1e-6); // Pa
            int kpa = (int)(pressure / 1000.0 * 2); // 安全系数 2
            return MathHelper.Clamp(kpa, 10, 80);
        }
    }
}
