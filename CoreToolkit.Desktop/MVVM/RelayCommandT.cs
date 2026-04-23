using System;
using System.Windows.Input;

namespace CoreToolkit.Desktop.MVVM
{
    /// <summary>
    /// 带参数的强类型命令实现
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            // 安全的类型检查，避免 InvalidCastException
            if (parameter is T typedParam)
                return _canExecute(typedParam);

            // 处理值类型的 null（如 default(T)）和未传参的情况
            if (parameter == null && default(T) == null)
                return _canExecute(default);

            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
            {
                _execute(typedParam);
                return;
            }

            if (parameter == null && default(T) == null)
            {
                _execute(default);
                return;
            }

            throw new InvalidOperationException($"命令参数类型不匹配。期望: {typeof(T).Name}, 实际: {parameter?.GetType().Name ?? "null"}");
        }

        /// <summary>
        /// 强类型执行（避免装箱，供代码直接调用）
        /// </summary>
        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
