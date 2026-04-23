using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreToolkit.Desktop.Behaviors
{
    /// <summary>
    /// 鼠标事件附加行为，支持将鼠标事件绑定到命令
    /// 包括：PreviewMouseDown、PreviewMouseUp、MouseDoubleClick、MouseWheel、MouseEnter、MouseLeave
    /// </summary>
    public static class MouseBehavior
    {
        #region PreviewMouseDown

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

        public static ICommand GetPreviewMouseDownCommand(DependencyObject obj) => (ICommand)obj.GetValue(PreviewMouseDownCommandProperty);
        public static void SetPreviewMouseDownCommand(DependencyObject obj, ICommand value) => obj.SetValue(PreviewMouseDownCommandProperty, value);
        public static object GetPreviewMouseDownCommandParameter(DependencyObject obj) => obj.GetValue(PreviewMouseDownCommandParameterProperty);
        public static void SetPreviewMouseDownCommandParameter(DependencyObject obj, object value) => obj.SetValue(PreviewMouseDownCommandParameterProperty, value);

        private static void OnPreviewMouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseDown -= OnPreviewMouseDown;
                if (e.NewValue != null)
                    element.PreviewMouseDown += OnPreviewMouseDown;
            }
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetPreviewMouseDownCommand(d);
                var parameter = GetPreviewMouseDownCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion

        #region PreviewMouseUp

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

        public static ICommand GetPreviewMouseUpCommand(DependencyObject obj) => (ICommand)obj.GetValue(PreviewMouseUpCommandProperty);
        public static void SetPreviewMouseUpCommand(DependencyObject obj, ICommand value) => obj.SetValue(PreviewMouseUpCommandProperty, value);
        public static object GetPreviewMouseUpCommandParameter(DependencyObject obj) => obj.GetValue(PreviewMouseUpCommandParameterProperty);
        public static void SetPreviewMouseUpCommandParameter(DependencyObject obj, object value) => obj.SetValue(PreviewMouseUpCommandParameterProperty, value);

        private static void OnPreviewMouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewMouseUp -= OnPreviewMouseUp;
                if (e.NewValue != null)
                    element.PreviewMouseUp += OnPreviewMouseUp;
            }
        }

        private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetPreviewMouseUpCommand(d);
                var parameter = GetPreviewMouseUpCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion

        #region MouseDoubleClick

        public static readonly DependencyProperty MouseDoubleClickCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseDoubleClickCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnMouseDoubleClickCommandChanged));

        public static readonly DependencyProperty MouseDoubleClickCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseDoubleClickCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static ICommand GetMouseDoubleClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(MouseDoubleClickCommandProperty);
        public static void SetMouseDoubleClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(MouseDoubleClickCommandProperty, value);
        public static object GetMouseDoubleClickCommandParameter(DependencyObject obj) => obj.GetValue(MouseDoubleClickCommandParameterProperty);
        public static void SetMouseDoubleClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(MouseDoubleClickCommandParameterProperty, value);

        private static void OnMouseDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.MouseDoubleClick -= OnMouseDoubleClick;
                if (e.NewValue != null)
                    control.MouseDoubleClick += OnMouseDoubleClick;
            }
        }

        private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetMouseDoubleClickCommand(d);
                var parameter = GetMouseDoubleClickCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion

        #region MouseWheel

        public static readonly DependencyProperty MouseWheelCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseWheelCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnMouseWheelCommandChanged));

        public static readonly DependencyProperty MouseWheelCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseWheelCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static ICommand GetMouseWheelCommand(DependencyObject obj) => (ICommand)obj.GetValue(MouseWheelCommandProperty);
        public static void SetMouseWheelCommand(DependencyObject obj, ICommand value) => obj.SetValue(MouseWheelCommandProperty, value);
        public static object GetMouseWheelCommandParameter(DependencyObject obj) => obj.GetValue(MouseWheelCommandParameterProperty);
        public static void SetMouseWheelCommandParameter(DependencyObject obj, object value) => obj.SetValue(MouseWheelCommandParameterProperty, value);

        private static void OnMouseWheelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseWheel -= OnMouseWheel;
                if (e.NewValue != null)
                    element.MouseWheel += OnMouseWheel;
            }
        }

        private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetMouseWheelCommand(d);
                var parameter = GetMouseWheelCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion

        #region MouseEnter

        public static readonly DependencyProperty MouseEnterCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseEnterCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnMouseEnterCommandChanged));

        public static readonly DependencyProperty MouseEnterCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseEnterCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static ICommand GetMouseEnterCommand(DependencyObject obj) => (ICommand)obj.GetValue(MouseEnterCommandProperty);
        public static void SetMouseEnterCommand(DependencyObject obj, ICommand value) => obj.SetValue(MouseEnterCommandProperty, value);
        public static object GetMouseEnterCommandParameter(DependencyObject obj) => obj.GetValue(MouseEnterCommandParameterProperty);
        public static void SetMouseEnterCommandParameter(DependencyObject obj, object value) => obj.SetValue(MouseEnterCommandParameterProperty, value);

        private static void OnMouseEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseEnter -= OnMouseEnter;
                if (e.NewValue != null)
                    element.MouseEnter += OnMouseEnter;
            }
        }

        private static void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetMouseEnterCommand(d);
                var parameter = GetMouseEnterCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion

        #region MouseLeave

        public static readonly DependencyProperty MouseLeaveCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseLeaveCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new PropertyMetadata(null, OnMouseLeaveCommandChanged));

        public static readonly DependencyProperty MouseLeaveCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseLeaveCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new PropertyMetadata(null));

        public static ICommand GetMouseLeaveCommand(DependencyObject obj) => (ICommand)obj.GetValue(MouseLeaveCommandProperty);
        public static void SetMouseLeaveCommand(DependencyObject obj, ICommand value) => obj.SetValue(MouseLeaveCommandProperty, value);
        public static object GetMouseLeaveCommandParameter(DependencyObject obj) => obj.GetValue(MouseLeaveCommandParameterProperty);
        public static void SetMouseLeaveCommandParameter(DependencyObject obj, object value) => obj.SetValue(MouseLeaveCommandParameterProperty, value);

        private static void OnMouseLeaveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseLeave -= OnMouseLeave;
                if (e.NewValue != null)
                    element.MouseLeave += OnMouseLeave;
            }
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var command = GetMouseLeaveCommand(d);
                var parameter = GetMouseLeaveCommandParameter(d) ?? e;
                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        #endregion
    }
}
