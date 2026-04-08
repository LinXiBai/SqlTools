using System;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 参数守卫工具类，用于简化参数校验
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// 确保参数不为 null
        /// </summary>
        /// <param name="argument">要检查的参数</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentNullException">当参数为 null 时抛出</exception>
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        /// <summary>
        /// 确保字符串参数不为 null 或空
        /// </summary>
        /// <param name="argument">要检查的字符串参数</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentException">当字符串为 null 或空时抛出</exception>
        public static void NotNullOrEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException($"{argumentName} cannot be null or empty.", argumentName);
        }

        /// <summary>
        /// 确保值类型参数不为默认值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="argument">要检查的参数</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentException">当参数为默认值时抛出</exception>
        public static void NotDefault<T>(T argument, string argumentName) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException($"{argumentName} cannot be default value.", argumentName);
        }

        /// <summary>
        /// 确保参数大于指定值
        /// </summary>
        /// <typeparam name="T">可比较类型</typeparam>
        /// <param name="argument">要检查的参数</param>
        /// <param name="minValue">最小值</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数小于等于最小值时抛出</exception>
        public static void GreaterThan<T>(T argument, T minValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(minValue) <= 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be greater than {minValue}.");
        }

        /// <summary>
        /// 确保参数大于等于指定值
        /// </summary>
        /// <typeparam name="T">可比较类型</typeparam>
        /// <param name="argument">要检查的参数</param>
        /// <param name="minValue">最小值</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数小于最小值时抛出</exception>
        public static void GreaterThanOrEqual<T>(T argument, T minValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(minValue) < 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be greater than or equal to {minValue}.");
        }

        /// <summary>
        /// 确保参数小于指定值
        /// </summary>
        /// <typeparam name="T">可比较类型</typeparam>
        /// <param name="argument">要检查的参数</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数大于等于最大值时抛出</exception>
        public static void LessThan<T>(T argument, T maxValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(maxValue) >= 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be less than {maxValue}.");
        }

        /// <summary>
        /// 确保参数小于等于指定值
        /// </summary>
        /// <typeparam name="T">可比较类型</typeparam>
        /// <param name="argument">要检查的参数</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="argumentName">参数名称</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数大于最大值时抛出</exception>
        public static void LessThanOrEqual<T>(T argument, T maxValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be less than or equal to {maxValue}.");
        }

        /// <summary>
        /// 确保条件为 true
        /// </summary>
        /// <param name="condition">要检查的条件</param>
        /// <param name="message">错误消息</param>
        /// <exception cref="InvalidOperationException">当条件为 false 时抛出</exception>
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        /// <summary>
        /// 确保条件为 false
        /// </summary>
        /// <param name="condition">要检查的条件</param>
        /// <param name="message">错误消息</param>
        /// <exception cref="InvalidOperationException">当条件为 true 时抛出</exception>
        public static void IsFalse(bool condition, string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        }
    }
}
