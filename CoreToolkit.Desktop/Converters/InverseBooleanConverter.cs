using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoreToolkit.Desktop.Converters
{
    /// <summary>
    /// Boolean 取反转换器
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            return value is bool b ? !b : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            return value is bool b ? !b : Binding.DoNothing;
        }
    }
}
