using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 字符串辅助工具类
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 判断字符串是否为空或空白
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 安全地截取字符串
        /// </summary>
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (startIndex < 0) startIndex = 0;
            if (startIndex >= value.Length) return string.Empty;
            if (length < 0) length = 0;
            if (startIndex + length > value.Length)
                length = value.Length - startIndex;
            return value.Substring(startIndex, length);
        }

        /// <summary>
        /// 移除字符串中的所有空白字符
        /// </summary>
        public static string RemoveWhitespace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        /// <summary>
        /// 将字符串按行分割
        /// </summary>
        public static string[] SplitLines(this string value)
        {
            if (string.IsNullOrEmpty(value)) return new string[0];
            return value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// 转为 Base64
        /// </summary>
        public static string ToBase64(this string value)
        {
            if (value == null) return null;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 从 Base64 解码
        /// </summary>
        public static string FromBase64(this string value)
        {
            if (value == null) return null;
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        /// <summary>
        /// 判断字符串是否包含中文
        /// </summary>
        public static bool ContainsChinese(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Regex.IsMatch(value, @"[\u4e00-\u9fa5]");
        }
    }
}
