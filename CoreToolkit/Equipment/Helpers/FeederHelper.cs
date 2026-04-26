using System;
using CoreToolkit.Equipment.Models;

namespace CoreToolkit.Equipment.Helpers
{
    /// <summary>
    /// 供料器相关辅助计算
    /// </summary>
    public static class FeederHelper
    {
        /// <summary>
        /// 计算料带推进的脉冲数或距离
        /// </summary>
        public static double CalculateAdvanceDistance(FeederInfo feeder, int componentCount = 1)
        {
            if (feeder == null || feeder.Pitch <= 0)
                return 0;
            return feeder.Pitch * componentCount;
        }

        /// <summary>
        /// 检查供料器是否还有足够物料
        /// </summary>
        public static bool HasEnoughComponents(FeederInfo feeder, int requiredCount)
        {
            return feeder != null && feeder.RemainingComponents >= requiredCount;
        }
    }
}
