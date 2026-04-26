using CoreToolkit.Safety.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 软限位守卫
    /// 管理软件层面的行程限制
    /// </summary>
    public class SoftLimitGuard
    {
        private readonly Dictionary<int, SoftLimitConfig> _limits = new Dictionary<int, SoftLimitConfig>();
        private readonly object _lock = new object();

        /// <summary>
        /// 设置轴软限位
        /// </summary>
        public void SetLimit(int axisIndex, double positiveLimit, double negativeLimit, bool enabled = true)
        {
            lock (_lock)
            {
                _limits[axisIndex] = new SoftLimitConfig
                {
                    AxisIndex = axisIndex,
                    PositiveLimit = positiveLimit,
                    NegativeLimit = negativeLimit,
                    Enabled = enabled
                };
            }
        }

        /// <summary>
        /// 配置软限位
        /// </summary>
        public void SetLimit(SoftLimitConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            lock (_lock)
            {
                _limits[config.AxisIndex] = config;
            }
        }

        /// <summary>
        /// 批量配置软限位
        /// </summary>
        public void SetLimits(IEnumerable<SoftLimitConfig> configs)
        {
            if (configs == null) return;
            lock (_lock)
            {
                foreach (var cfg in configs)
                {
                    if (cfg != null)
                        _limits[cfg.AxisIndex] = cfg;
                }
            }
        }

        /// <summary>
        /// 获取轴软限位配置
        /// </summary>
        public SoftLimitConfig GetLimit(int axisIndex)
        {
            lock (_lock)
            {
                return _limits.TryGetValue(axisIndex, out var limit) ? limit : null;
            }
        }

        /// <summary>
        /// 检查目标位置是否在软限位范围内
        /// </summary>
        public MoveSafetyResult CheckPosition(int axisIndex, double targetPosition)
        {
            lock (_lock)
            {
                if (!_limits.TryGetValue(axisIndex, out var limit) || !limit.Enabled)
                    return MoveSafetyResult.Allowed();

                if (!limit.IsInRange(targetPosition))
                {
                    string reason;
                    if (targetPosition > limit.PositiveLimit)
                        reason = $"轴{axisIndex}目标位置 {targetPosition:F2} 超出正向软限位 {limit.PositiveLimit:F2}";
                    else
                        reason = $"轴{axisIndex}目标位置 {targetPosition:F2} 超出负向软限位 {limit.NegativeLimit:F2}";

                    return MoveSafetyResult.Blocked(reason, limit);
                }

                return MoveSafetyResult.Allowed();
            }
        }

        /// <summary>
        /// 批量检查多个轴的目标位置
        /// </summary>
        public MoveSafetyResult CheckPositions(int[] axisIndices, double[] targetPositions)
        {
            if (axisIndices == null || targetPositions == null)
                return MoveSafetyResult.Allowed();

            if (axisIndices.Length != targetPositions.Length)
                return MoveSafetyResult.Blocked("轴索引与目标位置数量不匹配");

            for (int i = 0; i < axisIndices.Length; i++)
            {
                var result = CheckPosition(axisIndices[i], targetPositions[i]);
                if (!result.IsAllowed)
                    return result;
            }

            return MoveSafetyResult.Allowed();
        }

        /// <summary>
        /// 获取当前所有限位配置
        /// </summary>
        public List<SoftLimitConfig> GetAllLimits()
        {
            lock (_lock)
            {
                return _limits.Values.ToList();
            }
        }

        /// <summary>
        /// 禁用指定轴软限位
        /// </summary>
        public void DisableLimit(int axisIndex)
        {
            lock (_lock)
            {
                if (_limits.TryGetValue(axisIndex, out var limit))
                {
                    limit.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 启用指定轴软限位
        /// </summary>
        public void EnableLimit(int axisIndex)
        {
            lock (_lock)
            {
                if (_limits.TryGetValue(axisIndex, out var limit))
                {
                    limit.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 清除所有限位
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _limits.Clear();
            }
        }
    }
}
