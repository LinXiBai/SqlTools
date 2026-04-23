using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoreToolkit.Desktop.Converters
{
    /// <summary>
    /// Boolean 转 Visibility 转换器（支持反转、Collapsed/Hidden 模式、ConverterParameter）
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }
        public bool CollapseWhenHidden { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 处理绑定中的 null / UnsetValue
            if (value == null || value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            // 解析 ConverterParameter（支持 "Invert"、"Hidden" 等）
            ParseParameter(parameter);

            bool flag = value is bool b && b;
            if (IsInverted) flag = !flag;

            return flag ? Visibility.Visible : (CollapseWhenHidden ? Visibility.Collapsed : Visibility.Hidden);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            ParseParameter(parameter);

            bool flag = value is Visibility visibility && visibility == Visibility.Visible;
            if (IsInverted) flag = !flag;
            return flag;
        }

        private void ParseParameter(object parameter)
        {
            if (parameter is string param)
            {
                var parts = param.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    switch (part.Trim().ToLowerInvariant())
                    {
                        case "invert":
                        case "inverted":
                            IsInverted = true;
                            break;
                        case "hidden":
                            CollapseWhenHidden = false;
                            break;
                        case "collapse":
                            CollapseWhenHidden = true;
                            break;
                    }
                }
            }
        }
    }
}
