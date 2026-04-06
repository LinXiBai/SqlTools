using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoreToolkit.Desktop.Converters
{
    /// <summary>
    /// Boolean 转 Visibility 转换器
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }
        public bool CollapseWhenHidden { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (IsInverted) flag = !flag;

            return flag ? Visibility.Visible : (CollapseWhenHidden ? Visibility.Collapsed : Visibility.Hidden);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is Visibility visibility && visibility == Visibility.Visible;
            if (IsInverted) flag = !flag;
            return flag;
        }
    }
}
