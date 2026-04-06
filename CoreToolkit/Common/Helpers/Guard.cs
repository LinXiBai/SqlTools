using System;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 参数守卫工具类，用于简化参数校验
    /// </summary>
    public static class Guard
    {
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void NotNullOrEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException($"{argumentName} cannot be null or empty.", argumentName);
        }

        public static void NotDefault<T>(T argument, string argumentName) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException($"{argumentName} cannot be default value.", argumentName);
        }

        public static void GreaterThan<T>(T argument, T minValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(minValue) <= 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be greater than {minValue}.");
        }

        public static void GreaterThanOrEqual<T>(T argument, T minValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(minValue) < 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be greater than or equal to {minValue}.");
        }

        public static void LessThan<T>(T argument, T maxValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(maxValue) >= 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be less than {maxValue}.");
        }

        public static void LessThanOrEqual<T>(T argument, T maxValue, string argumentName) where T : IComparable<T>
        {
            if (argument.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} must be less than or equal to {maxValue}.");
        }

        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        public static void IsFalse(bool condition, string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        }
    }
}
