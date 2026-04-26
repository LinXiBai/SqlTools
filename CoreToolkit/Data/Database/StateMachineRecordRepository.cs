using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 状态机运行记录仓储
    /// 提供状态机执行记录的持久化与查询能力
    /// </summary>
    public class StateMachineRecordRepository : RepositoryBase<StateMachineRecord>
    {
        public StateMachineRecordRepository(SqliteDbContext db) : base(db) { }

        /// <summary>
        /// 按状态机名称查询记录
        /// </summary>
        public IEnumerable<StateMachineRecord> GetByMachineName(string machineName)
        {
            return QueryByField("MachineName", machineName);
        }

        /// <summary>
        /// 异步按状态机名称查询记录
        /// </summary>
        public async Task<IEnumerable<StateMachineRecord>> GetByMachineNameAsync(string machineName)
        {
            return await QueryByFieldAsync("MachineName", machineName);
        }

        /// <summary>
        /// 按时间范围查询记录
        /// </summary>
        public IEnumerable<StateMachineRecord> GetByTimeRange(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE StartTime BETWEEN @StartDate AND @EndDate ORDER BY StartTime DESC";
            return _db.Query<StateMachineRecord>(sql, new { StartDate = startDate, EndDate = endDate });
        }

        /// <summary>
        /// 异步按时间范围查询记录
        /// </summary>
        public async Task<IEnumerable<StateMachineRecord>> GetByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE StartTime BETWEEN @StartDate AND @EndDate ORDER BY StartTime DESC";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<StateMachineRecord>(sql, new { StartDate = startDate, EndDate = endDate });
            });
        }

        /// <summary>
        /// 查询最近 N 条记录
        /// </summary>
        public IEnumerable<StateMachineRecord> GetRecentRecords(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY StartTime DESC LIMIT @Count";
            return _db.Query<StateMachineRecord>(sql, new { Count = count });
        }

        /// <summary>
        /// 异步查询最近 N 条记录
        /// </summary>
        public async Task<IEnumerable<StateMachineRecord>> GetRecentRecordsAsync(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY StartTime DESC LIMIT @Count";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<StateMachineRecord>(sql, new { Count = count });
            });
        }

        /// <summary>
        /// 按状态查询记录
        /// </summary>
        public IEnumerable<StateMachineRecord> GetByStatus(string status)
        {
            return QueryByField("Status", status);
        }

        /// <summary>
        /// 异步按状态查询记录
        /// </summary>
        public async Task<IEnumerable<StateMachineRecord>> GetByStatusAsync(string status)
        {
            return await QueryByFieldAsync("Status", status);
        }

        /// <summary>
        /// 按状态机名称和时间范围查询
        /// </summary>
        public IEnumerable<StateMachineRecord> GetByMachineAndTimeRange(string machineName, DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName AND StartTime BETWEEN @StartDate AND @EndDate ORDER BY StartTime DESC";
            return _db.Query<StateMachineRecord>(sql, new { MachineName = machineName, StartDate = startDate, EndDate = endDate });
        }

        /// <summary>
        /// 异步按状态机名称和时间范围查询
        /// </summary>
        public async Task<IEnumerable<StateMachineRecord>> GetByMachineAndTimeRangeAsync(string machineName, DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName AND StartTime BETWEEN @StartDate AND @EndDate ORDER BY StartTime DESC";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<StateMachineRecord>(sql, new { MachineName = machineName, StartDate = startDate, EndDate = endDate });
            });
        }

        /// <summary>
        /// 获取指定状态机的最新一条记录
        /// </summary>
        public StateMachineRecord GetLatestRecord(string machineName)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName ORDER BY StartTime DESC LIMIT 1";
            return _db.QuerySingleOrDefault<StateMachineRecord>(sql, new { MachineName = machineName });
        }

        /// <summary>
        /// 异步获取指定状态机的最新一条记录
        /// </summary>
        public async Task<StateMachineRecord> GetLatestRecordAsync(string machineName)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName ORDER BY StartTime DESC LIMIT 1";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<StateMachineRecord>(sql, new { MachineName = machineName });
            });
        }

        /// <summary>
        /// 获取指定状态机指定状态的最新一条记录
        /// </summary>
        public StateMachineRecord GetLatestRecord(string machineName, string status)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName AND Status = @Status ORDER BY StartTime DESC LIMIT 1";
            return _db.QuerySingleOrDefault<StateMachineRecord>(sql, new { MachineName = machineName, Status = status });
        }

        /// <summary>
        /// 异步获取指定状态机指定状态的最新一条记录
        /// </summary>
        public async Task<StateMachineRecord> GetLatestRecordAsync(string machineName, string status)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineName = @MachineName AND Status = @Status ORDER BY StartTime DESC LIMIT 1";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<StateMachineRecord>(sql, new { MachineName = machineName, Status = status });
            });
        }

        /// <summary>
        /// 获取成功/失败统计摘要
        /// </summary>
        public StateMachineSummary GetSummary(string machineName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            string sql = $@"SELECT 
                                COUNT(*) as TotalCount,
                                SUM(CASE WHEN IsSuccess = 1 THEN 1 ELSE 0 END) as SuccessCount,
                                SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) as FailedCount,
                                AVG(DurationMs) as AverageDurationMs,
                                MAX(DurationMs) as MaxDurationMs,
                                MIN(DurationMs) as MinDurationMs
                            FROM {TableName}
                            WHERE (@MachineName IS NULL OR MachineName = @MachineName)
                            AND (@StartDate IS NULL OR StartTime >= @StartDate)
                            AND (@EndDate IS NULL OR StartTime <= @EndDate)";
            return _db.QuerySingleOrDefault<StateMachineSummary>(sql, new
            {
                MachineName = machineName,
                StartDate = startDate,
                EndDate = endDate
            }) ?? new StateMachineSummary();
        }

        /// <summary>
        /// 异步获取成功/失败统计摘要
        /// </summary>
        public async Task<StateMachineSummary> GetSummaryAsync(string machineName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            string sql = $@"SELECT 
                                COUNT(*) as TotalCount,
                                SUM(CASE WHEN IsSuccess = 1 THEN 1 ELSE 0 END) as SuccessCount,
                                SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) as FailedCount,
                                AVG(DurationMs) as AverageDurationMs,
                                MAX(DurationMs) as MaxDurationMs,
                                MIN(DurationMs) as MinDurationMs
                            FROM {TableName}
                            WHERE (@MachineName IS NULL OR MachineName = @MachineName)
                            AND (@StartDate IS NULL OR StartTime >= @StartDate)
                            AND (@EndDate IS NULL OR StartTime <= @EndDate)";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<StateMachineSummary>(sql, new
                {
                    MachineName = machineName,
                    StartDate = startDate,
                    EndDate = endDate
                });
            }) ?? new StateMachineSummary();
        }

        /// <summary>
        /// 清理指定时间之前的记录
        /// </summary>
        public int CleanupRecords(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE StartTime < @BeforeDate";
            return _db.Execute(sql, new { BeforeDate = beforeDate });
        }

        /// <summary>
        /// 异步清理指定时间之前的记录
        /// </summary>
        public async Task<int> CleanupRecordsAsync(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE StartTime < @BeforeDate";
            return await _db.ExecuteAsync(sql, new { BeforeDate = beforeDate });
        }
    }

    /// <summary>
    /// 状态机执行统计摘要
    /// </summary>
    public class StateMachineSummary
    {
        /// <summary>总执行次数</summary>
        public int TotalCount { get; set; }
        /// <summary>成功次数</summary>
        public int SuccessCount { get; set; }
        /// <summary>失败次数</summary>
        public int FailedCount { get; set; }
        /// <summary>平均耗时（毫秒）</summary>
        public double AverageDurationMs { get; set; }
        /// <summary>最大耗时（毫秒）</summary>
        public double MaxDurationMs { get; set; }
        /// <summary>最小耗时（毫秒）</summary>
        public double MinDurationMs { get; set; }
        /// <summary>成功率</summary>
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    }
}
