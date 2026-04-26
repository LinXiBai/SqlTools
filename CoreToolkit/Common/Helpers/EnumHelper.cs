using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 枚举辅助工具类
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举值的 Description 特性描述
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            if (value == null) return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// 将枚举转换为键值对列表（值, 描述）
        /// </summary>
        public static List<KeyValuePair<int, string>> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new KeyValuePair<int, string>(Convert.ToInt32(e), e.GetDescription()))
                .ToList();
        }

        /// <summary>
        /// 根据 Description 获取枚举值
        /// </summary>
        public static T GetValueByDescription<T>(string description) where T : Enum
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null && attribute.Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }
            return default;
        }

        /// <summary>
        /// 尝试将字符串解析为枚举值
        /// </summary>
        public static bool TryParse<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, true, out result);
        }
    }
}
