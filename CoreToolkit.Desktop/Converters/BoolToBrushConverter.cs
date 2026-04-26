using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CoreToolkit.Desktop.Converters
{
    /// <summary>
    /// Boolean 转 Brush 转换器（支持 ConverterParameter 快速指定颜色）
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = Brushes.Green;
        public Brush FalseBrush { get; set; } = Brushes.Gray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            // ConverterParameter 可覆盖 TrueBrush 颜色，例如 ConverterParameter="#FF2196F3"
            if (parameter is string colorStr && !string.IsNullOrWhiteSpace(colorStr))
            {
                try
                {
                    var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
                    brush.Freeze();
                    return value is bool flag && flag ? brush : FalseBrush;
                }
                catch
                {
                    // 颜色解析失败时回退到默认
                }
            }

            return value is bool val && val ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
