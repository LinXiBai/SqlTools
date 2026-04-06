using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// 重试机制辅助类
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// 同步重试执行
        /// </summary>
        public static T Execute<T>(Func<T> action, int retryCount = 3, int delayMilliseconds = 100)
        {
            Exception lastException = null;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < retryCount - 1 && delayMilliseconds > 0)
                    {
                        Thread.Sleep(delayMilliseconds);
                    }
                }
            }
            throw lastException;
        }

        /// <summary>
        /// 同步重试执行（无返回值）
        /// </summary>
        public static void Execute(Action action, int retryCount = 3, int delayMilliseconds = 100)
        {
            Execute(() => { action(); return true; }, retryCount, delayMilliseconds);
        }

        /// <summary>
        /// 异步重试执行
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, int retryCount = 3, int delayMilliseconds = 100)
        {
            Exception lastException = null;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < retryCount - 1 && delayMilliseconds > 0)
                    {
                        await Task.Delay(delayMilliseconds);
                    }
                }
            }
            throw lastException;
        }

        /// <summary>
        /// 异步重试执行（无返回值）
        /// </summary>
        public static async Task ExecuteAsync(Func<Task> action, int retryCount = 3, int delayMilliseconds = 100)
        {
            await ExecuteAsync(async () => { await action(); return true; }, retryCount, delayMilliseconds);
        }
    }
}
