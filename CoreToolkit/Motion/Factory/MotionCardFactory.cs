using System;
using System.Collections.Generic;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Providers.Advantech;

namespace CoreToolkit.Motion.Factory
{
    /// <summary>
    /// 运动控制卡工厂类
    /// 用于创建和管理运动控制卡实例
    /// </summary>
    public class MotionCardFactory
    {
        private static readonly Dictionary<string, Func<int, IMotionCard>> _cardCreators =
            new Dictionary<string, Func<int, IMotionCard>>(StringComparer.OrdinalIgnoreCase)
            {
                { "PCI1203", (cardId) => new PCI1203() },
                { "PCI-1203", (cardId) => new PCI1203() },
                { "PCI1245", (cardId) => new PCI1203() },
                { "PCI-1245", (cardId) => new PCI1203() },
                { "PCI1285", (cardId) => new PCI1285() },
                { "PCI-1285", (cardId) => new PCI1285() }
            };

        /// <summary>
        /// 注册控制卡创建器
        /// </summary>
        public static void RegisterCard(string cardType, Func<int, IMotionCard> creator)
        {
            _cardCreators[cardType] = creator;
        }

        /// <summary>
        /// 创建运动控制卡实例
        /// </summary>
        public static IMotionCard CreateCard(string cardType, int cardId = 0)
        {
            if (!_cardCreators.TryGetValue(cardType, out var creator))
            {
                throw new NotSupportedException($"不支持的卡类型: {cardType}");
            }

            var card = creator(cardId);
            return card;
        }

        /// <summary>
        /// 获取支持的卡类型列表
        /// </summary>
        public static IEnumerable<string> GetSupportedCardTypes()
        {
            return _cardCreators.Keys;
        }

        /// <summary>
        /// 检查是否支持指定的卡类型
        /// </summary>
        public static bool IsSupported(string cardType)
        {
            return _cardCreators.ContainsKey(cardType);
        }
    }
}
