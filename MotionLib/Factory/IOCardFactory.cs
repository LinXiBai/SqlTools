using System;
using System.Collections.Generic;
using MotionLib.Interfaces;
using MotionLib.Providers.Advantech;

namespace MotionLib.Factory
{
    /// <summary>
    /// IO扩展卡工厂类
    /// 用于创建和管理IO扩展卡实例
    /// </summary>
    public class IOCardFactory
    {
        private static readonly Dictionary<string, Func<int, int, IIOCard>> _cardCreators =
            new Dictionary<string, Func<int, int, IIOCard>>(StringComparer.OrdinalIgnoreCase)
            {
                { "PCI1750", (inputCount, outputCount) => new AdvantechIOCard("PCI-1750", inputCount, outputCount) },
                { "PCI-1750", (inputCount, outputCount) => new AdvantechIOCard("PCI-1750", inputCount, outputCount) },
                { "PCI1756", (inputCount, outputCount) => new AdvantechIOCard("PCI-1756", inputCount, outputCount) },
                { "PCI-1756", (inputCount, outputCount) => new AdvantechIOCard("PCI-1756", inputCount, outputCount) },
                { "PCI1730", (inputCount, outputCount) => new AdvantechIOCard("PCI-1730", inputCount, outputCount) },
                { "PCI-1730", (inputCount, outputCount) => new AdvantechIOCard("PCI-1730", inputCount, outputCount) }
            };

        /// <summary>
        /// 注册IO卡创建器
        /// </summary>
        public static void RegisterCard(string cardType, Func<int, int, IIOCard> creator)
        {
            _cardCreators[cardType] = creator;
        }

        /// <summary>
        /// 创建IO扩展卡实例
        /// </summary>
        /// <param name="cardType">卡类型</param>
        /// <param name="inputCount">输入点数量</param>
        /// <param name="outputCount">输出点数量</param>
        /// <returns>IO卡实例</returns>
        public static IIOCard CreateCard(string cardType, int inputCount = 32, int outputCount = 32)
        {
            if (!_cardCreators.TryGetValue(cardType, out var creator))
            {
                throw new NotSupportedException($"不支持的IO卡类型: {cardType}");
            }

            var card = creator(inputCount, outputCount);
            return card;
        }

        /// <summary>
        /// 创建IO扩展卡实例（使用默认配置）
        /// </summary>
        public static IIOCard CreateCard(string cardType)
        {
            // 根据卡类型使用默认IO配置
            var (inputCount, outputCount) = GetDefaultIOConfig(cardType);
            return CreateCard(cardType, inputCount, outputCount);
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

        /// <summary>
        /// 获取默认IO配置
        /// </summary>
        private static (int inputCount, int outputCount) GetDefaultIOConfig(string cardType)
        {
            switch (cardType.ToUpper())
            {
                case "PCI1750":
                case "PCI-1750":
                    return (32, 32);  // 32入32出
                case "PCI1756":
                case "PCI-1756":
                    return (32, 32);  // 32入32出
                case "PCI1730":
                case "PCI-1730":
                    return (16, 16);  // 16入16出
                default:
                    return (32, 32);  // 默认32入32出
            }
        }
    }
}
