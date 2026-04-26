using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreToolkit.Desktop.MVVM
{
    /// <summary>
    /// 异步命令实现，支持可绑定的 IsExecuting 状态、线程安全的重入保护
    /// </summary>
    public class AsyncRelayCommand : ObservableObject, ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private volatile int _isExecuting;

        /// <summary>
        /// 是否正在执行（可绑定到 UI 显示加载状态）
        /// </summary>
        public bool IsExecuting => _isExecuting != 0;

        /// <summary>
        /// 最近一次执行中的异常（若有）
        /// </summary>
        public Exception ExecutionException { get; private set; }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
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
            if (_isExecuting != 0)
                return false;

            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _ = ExecuteAsync();
        }

        /// <summary>
        /// 异步执行命令，线程安全地防止重入
        /// </summary>
        public async Task ExecuteAsync()
        {
            // 使用 Interlocked 确保只有一个线程能进入执行
            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
                return;

            ExecutionException = null;
            OnPropertyChanged(nameof(IsExecuting));
            RaiseCanExecuteChanged();

            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                ExecutionException = ex;
                System.Diagnostics.Debug.WriteLine($"[AsyncRelayCommand] 执行异常: {ex.Message}");
                throw;
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0);
                OnPropertyChanged(nameof(IsExecuting));
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
