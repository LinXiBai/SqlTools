using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace SqlDemo.Data
{
    /// <summary>
    /// 日志仓储：用于演示跨数据库操作
    /// </summary>
    public class LogRepository : RepositoryBase<Log>
    {
        public LogRepository(SqliteDbContext db) : base(db) { }

        /// <summary>
        /// 按级别查询日志
        /// </summary>
        public IEnumerable<Log> GetByLevel(string level)
        {
            string whereClause = "Level = @Level";
            return Query(whereClause, new { Level = level });
        }

        /// <summary>
        /// 异步按级别查询日志
        /// </summary>
        public async Task<IEnumerable<Log>> GetByLevelAsync(string level)
        {
            string whereClause = "Level = @Level";
            return await QueryAsync(whereClause, new { Level = level });
        }

        /// <summary>
        /// 查询最近的日志
        /// </summary>
        public IEnumerable<Log> GetRecentLogs(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY Timestamp DESC LIMIT @Count";
            return _db.Query<Log>(sql, new { Count = count });
        }

        /// <summary>
        /// 异步查询最近的日志
        /// </summary>
        public async Task<IEnumerable<Log>> GetRecentLogsAsync(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY Timestamp DESC LIMIT @Count";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<Log>(sql, new { Count = count });
            });
        }

        /// <summary>
        /// 按时间范围查询日志
        /// </summary>
        public IEnumerable<Log> GetLogsByTimeRange(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Timestamp BETWEEN @StartDate AND @EndDate ORDER BY Timestamp DESC";
            return _db.Query<Log>(sql, new { StartDate = startDate, EndDate = endDate });
        }

        /// <summary>
        /// 异步按时间范围查询日志
        /// </summary>
        public async Task<IEnumerable<Log>> GetLogsByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Timestamp BETWEEN @StartDate AND @EndDate ORDER BY Timestamp DESC";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<Log>(sql, new { StartDate = startDate, EndDate = endDate });
            });
        }

        /// <summary>
        /// 按级别和时间范围查询日志
        /// </summary>
        public IEnumerable<Log> GetLogsByLevelAndTimeRange(string level, DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Level = @Level AND Timestamp BETWEEN @StartDate AND @EndDate ORDER BY Timestamp DESC";
            return _db.Query<Log>(sql, new { Level = level, StartDate = startDate, EndDate = endDate });
        }

        /// <summary>
        /// 异步按级别和时间范围查询日志
        /// </summary>
        public async Task<IEnumerable<Log>> GetLogsByLevelAndTimeRangeAsync(string level, DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Level = @Level AND Timestamp BETWEEN @StartDate AND @EndDate ORDER BY Timestamp DESC";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<Log>(sql, new { Level = level, StartDate = startDate, EndDate = endDate });
            });
        }

        /// <summary>
        /// 写入调试日志
        /// </summary>
        public long WriteDebug(string message, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Debug, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入调试日志
        /// </summary>
        public async Task<long> WriteDebugAsync(string message, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Debug, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入信息日志
        /// </summary>
        public long WriteInfo(string message, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Info, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入信息日志
        /// </summary>
        public async Task<long> WriteInfoAsync(string message, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Info, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        public long WriteWarning(string message, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Warning, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入警告日志
        /// </summary>
        public async Task<long> WriteWarningAsync(string message, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Warning, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        public long WriteError(string message, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Error, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入错误日志
        /// </summary>
        public async Task<long> WriteErrorAsync(string message, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Error, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入致命错误日志
        /// </summary>
        public long WriteFatal(string message, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Fatal, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入致命错误日志
        /// </summary>
        public async Task<long> WriteFatalAsync(string message, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Fatal, message, null, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        public long WriteException(Exception ex, string message = null, string loggerName = null, string additionalInfo = null)
        {
            return WriteLog(LogLevel.Error, message ?? ex.Message, ex, loggerName, additionalInfo);
        }

        /// <summary>
        /// 异步写入异常日志
        /// </summary>
        public async Task<long> WriteExceptionAsync(Exception ex, string message = null, string loggerName = null, string additionalInfo = null)
        {
            return await WriteLogAsync(LogLevel.Error, message ?? ex.Message, ex, loggerName, additionalInfo);
        }

        /// <summary>
        /// 写入日志（核心方法）
        /// </summary>
        private long WriteLog(LogLevel level, string message, Exception ex, string loggerName = null, string additionalInfo = null)
        {
            var log = new Log
            {
                Level = level.ToString(),
                Message = message,
                Timestamp = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LoggerName = loggerName ?? GetCallingMethodName(),
                Exception = ex?.ToString(),
                StackTrace = ex?.StackTrace,
                MachineName = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                ProcessId = Process.GetCurrentProcess().Id,
                AdditionalInfo = additionalInfo
            };

            return Insert(log);
        }

        /// <summary>
        /// 异步写入日志（核心方法）
        /// </summary>
        private async Task<long> WriteLogAsync(LogLevel level, string message, Exception ex, string loggerName = null, string additionalInfo = null)
        {
            var log = new Log
            {
                Level = level.ToString(),
                Message = message,
                Timestamp = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LoggerName = loggerName ?? GetCallingMethodName(),
                Exception = ex?.ToString(),
                StackTrace = ex?.StackTrace,
                MachineName = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                ProcessId = Process.GetCurrentProcess().Id,
                AdditionalInfo = additionalInfo
            };

            return await InsertAsync(log);
        }

        /// <summary>
        /// 获取调用方法名称
        /// </summary>
        private string GetCallingMethodName()
        {
            try
            {
                var stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(3); // 跳过当前方法、WriteLog 方法和调用 WriteLog 的方法
                if (frame != null)
                {
                    var method = frame.GetMethod();
                    return $"{method.DeclaringType?.FullName}.{method.Name}";
                }
            }
            catch
            {
                // 忽略异常
            }
            return "Unknown";
        }

        /// <summary>
        /// 清理指定时间之前的日志
        /// </summary>
        public int CleanupLogs(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE Timestamp < @BeforeDate";
            return _db.Execute(sql, new { BeforeDate = beforeDate });
        }

        /// <summary>
        /// 异步清理指定时间之前的日志
        /// </summary>
        public async Task<int> CleanupLogsAsync(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE Timestamp < @BeforeDate";
            return await _db.ExecuteAsync(sql, new { BeforeDate = beforeDate });
        }

        /// <summary>
        /// 清理指定级别的日志
        /// </summary>
        public int CleanupLogsByLevel(string level)
        {
            string sql = $"DELETE FROM {TableName} WHERE Level = @Level";
            return _db.Execute(sql, new { Level = level });
        }

        /// <summary>
        /// 异步清理指定级别的日志
        /// </summary>
        public async Task<int> CleanupLogsByLevelAsync(string level)
        {
            string sql = $"DELETE FROM {TableName} WHERE Level = @Level";
            return await _db.ExecuteAsync(sql, new { Level = level });
        }
    }
}
