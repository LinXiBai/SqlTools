using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreToolkit.Desktop.Behaviors
{
    /// <summary>
    /// 鼠标事件附加行为，用于将 PreviewMouseDown/PreviewMouseUp 绑定到命令
    /// </summary>
    public static class MouseBehavior
    {
        public static readonly DependencyProperty PreviewMouseDownCommandProperty =
            DependencyProperty.RegisterAttached(
                "PreviewMouseDownCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnPreviewMouseDownCommandChanged));

        public static readonly DependencyProperty PreviewMouseDownCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "PreviewMouseDownCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PreviewMouseUpCommandProperty =
            DependencyProperty.RegisterAttached(
                "PreviewMouseUpCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnPreviewMouseUpCommandChanged));

        public static readonly DependencyProperty PreviewMouseUpCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "PreviewMouseUpCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static ICommand GetPreviewMouseDownCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(PreviewMouseDownCommandProperty);
        }

        public static void SetPreviewMouseDownCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(PreviewMouseDownCommandProperty, value);
        }

        public static object GetPreviewMouseDownCommandParameter(DependencyObject obj)
        {
            return obj.GetValue(PreviewMouseDownCommandParameterProperty);
        }

        public static void SetPreviewMouseDownCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(PreviewMouseDownCommandParameterProperty, value);
        }

        public static ICommand GetPreviewMouseUpCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(PreviewMouseUpCommandProperty);
        }

        public static void SetPreviewMouseUpCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(PreviewMouseUpCommandProperty, value);
        }

        public static object GetPreviewMouseUpCommandParameter(DependencyObject obj)
        {
            return obj.GetValue(PreviewMouseUpCommandParameterProperty);
        }

        public static void SetPreviewMouseUpCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(PreviewMouseUpCommandParameterProperty, value);
        }

        private static void OnPreviewMouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseDown -= OnPreviewMouseDown;
                if (e.NewValue != null)
                {
                    element.PreviewMouseDown += OnPreviewMouseDown;
                }
            }
        }

        private static void OnPreviewMouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseUp -= OnPreviewMouseUp;
                if (e.NewValue != null)
                {
                    element.PreviewMouseUp += OnPreviewMouseUp;
                }
            }
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetPreviewMouseDownCommand(d);
                var parameter = GetPreviewMouseDownCommandParameter(d);
                if (command != null && command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }

        private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetPreviewMouseUpCommand(d);
                var parameter = GetPreviewMouseUpCommandParameter(d);
                if (command != null && command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }
    }
}
