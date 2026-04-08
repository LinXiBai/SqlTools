using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 授权码记录仓储：提供授权码生成记录的 CRUD 和查询操作
    /// </summary>
    public class LicenseRecordRepository : RepositoryBase<LicenseRecord>
    {
        public LicenseRecordRepository(SqliteDbContext db) : base(db) { }

        /// <summary>
        /// 按项目号查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByProjectNumber(string projectNumber)
        {
            string whereClause = "ProjectNumber = @ProjectNumber";
            return Query(whereClause, new { ProjectNumber = projectNumber });
        }

        /// <summary>
        /// 异步按项目号查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByProjectNumberAsync(string projectNumber)
        {
            string whereClause = "ProjectNumber = @ProjectNumber";
            return await QueryAsync(whereClause, new { ProjectNumber = projectNumber });
        }

        /// <summary>
        /// 按设备号查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByDeviceNumber(string deviceNumber)
        {
            string whereClause = "DeviceNumber = @DeviceNumber";
            return Query(whereClause, new { DeviceNumber = deviceNumber });
        }

        /// <summary>
        /// 异步按设备号查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByDeviceNumberAsync(string deviceNumber)
        {
            string whereClause = "DeviceNumber = @DeviceNumber";
            return await QueryAsync(whereClause, new { DeviceNumber = deviceNumber });
        }

        /// <summary>
        /// 按部门查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByDepartment(string department)
        {
            string whereClause = "Department = @Department";
            return Query(whereClause, new { Department = department });
        }

        /// <summary>
        /// 异步按部门查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByDepartmentAsync(string department)
        {
            string whereClause = "Department = @Department";
            return await QueryAsync(whereClause, new { Department = department });
        }

        /// <summary>
        /// 按记录人查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByOperator(string operatorName)
        {
            string whereClause = "Operator = @Operator";
            return Query(whereClause, new { Operator = operatorName });
        }

        /// <summary>
        /// 异步按记录人查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByOperatorAsync(string operatorName)
        {
            string whereClause = "Operator = @Operator";
            return await QueryAsync(whereClause, new { Operator = operatorName });
        }

        /// <summary>
        /// 按机器码查询授权记录
        /// </summary>
        public LicenseRecord GetByMachineCode(string machineCode)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineCode = @MachineCode LIMIT 1";
            return _db.QuerySingleOrDefault<LicenseRecord>(sql, new { MachineCode = machineCode });
        }

        /// <summary>
        /// 异步按机器码查询授权记录
        /// </summary>
        public async Task<LicenseRecord> GetByMachineCodeAsync(string machineCode)
        {
            string sql = $"SELECT * FROM {TableName} WHERE MachineCode = @MachineCode LIMIT 1";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<LicenseRecord>(sql, new { MachineCode = machineCode });
            });
        }

        /// <summary>
        /// 按时间范围查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByTimeRange(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE RecordTime BETWEEN @StartDate AND @EndDate ORDER BY RecordTime DESC";
            return _db.Query<LicenseRecord>(sql, new { StartDate = startDate, EndDate = endDate });
        }

        /// <summary>
        /// 异步按时间范围查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            string sql = $"SELECT * FROM {TableName} WHERE RecordTime BETWEEN @StartDate AND @EndDate ORDER BY RecordTime DESC";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<LicenseRecord>(sql, new { StartDate = startDate, EndDate = endDate });
            });
        }

        /// <summary>
        /// 查询最近的授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetRecentRecords(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY RecordTime DESC LIMIT @Count";
            return _db.Query<LicenseRecord>(sql, new { Count = count });
        }

        /// <summary>
        /// 异步查询最近的授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetRecentRecordsAsync(int count)
        {
            string sql = $"SELECT * FROM {TableName} ORDER BY RecordTime DESC LIMIT @Count";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<LicenseRecord>(sql, new { Count = count });
            });
        }

        /// <summary>
        /// 组合条件查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> SearchRecords(string projectNumber = null, string deviceNumber = null, 
            string department = null, string operatorName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(projectNumber))
            {
                conditions.Add("ProjectNumber = @ProjectNumber");
                parameters.Add("ProjectNumber", projectNumber);
            }

            if (!string.IsNullOrEmpty(deviceNumber))
            {
                conditions.Add("DeviceNumber = @DeviceNumber");
                parameters.Add("DeviceNumber", deviceNumber);
            }

            if (!string.IsNullOrEmpty(department))
            {
                conditions.Add("Department = @Department");
                parameters.Add("Department", department);
            }

            if (!string.IsNullOrEmpty(operatorName))
            {
                conditions.Add("Operator = @Operator");
                parameters.Add("Operator", operatorName);
            }

            if (startDate.HasValue)
            {
                conditions.Add("RecordTime >= @StartDate");
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                conditions.Add("RecordTime <= @EndDate");
                parameters.Add("EndDate", endDate.Value);
            }

            string sql;
            if (conditions.Count > 0)
            {
                sql = $"SELECT * FROM {TableName} WHERE {string.Join(" AND ", conditions)} ORDER BY RecordTime DESC";
            }
            else
            {
                sql = $"SELECT * FROM {TableName} ORDER BY RecordTime DESC";
            }

            return _db.Query<LicenseRecord>(sql, parameters);
        }

        /// <summary>
        /// 异步组合条件查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> SearchRecordsAsync(string projectNumber = null, string deviceNumber = null,
            string department = null, string operatorName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(projectNumber))
            {
                conditions.Add("ProjectNumber = @ProjectNumber");
                parameters.Add("ProjectNumber", projectNumber);
            }

            if (!string.IsNullOrEmpty(deviceNumber))
            {
                conditions.Add("DeviceNumber = @DeviceNumber");
                parameters.Add("DeviceNumber", deviceNumber);
            }

            if (!string.IsNullOrEmpty(department))
            {
                conditions.Add("Department = @Department");
                parameters.Add("Department", department);
            }

            if (!string.IsNullOrEmpty(operatorName))
            {
                conditions.Add("Operator = @Operator");
                parameters.Add("Operator", operatorName);
            }

            if (startDate.HasValue)
            {
                conditions.Add("RecordTime >= @StartDate");
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                conditions.Add("RecordTime <= @EndDate");
                parameters.Add("EndDate", endDate.Value);
            }

            string sql;
            if (conditions.Count > 0)
            {
                sql = $"SELECT * FROM {TableName} WHERE {string.Join(" AND ", conditions)} ORDER BY RecordTime DESC";
            }
            else
            {
                sql = $"SELECT * FROM {TableName} ORDER BY RecordTime DESC";
            }

            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<LicenseRecord>(sql, parameters);
            });
        }

        /// <summary>
        /// 检查机器码是否已存在授权记录
        /// </summary>
        public bool MachineCodeExists(string machineCode)
        {
            string sql = $"SELECT COUNT(1) FROM {TableName} WHERE MachineCode = @MachineCode";
            return _db.ExecuteScalar<long>(sql, new { MachineCode = machineCode }) > 0;
        }

        /// <summary>
        /// 异步检查机器码是否已存在授权记录
        /// </summary>
        public async Task<bool> MachineCodeExistsAsync(string machineCode)
        {
            string sql = $"SELECT COUNT(1) FROM {TableName} WHERE MachineCode = @MachineCode";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                var count = await _db.Connection.ExecuteScalarAsync<long>(sql, new { MachineCode = machineCode });
                return count > 0;
            });
        }

        /// <summary>
        /// 清理指定时间之前的记录
        /// </summary>
        public int CleanupRecords(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE RecordTime < @BeforeDate";
            return _db.Execute(sql, new { BeforeDate = beforeDate });
        }

        /// <summary>
        /// 异步清理指定时间之前的记录
        /// </summary>
        public async Task<int> CleanupRecordsAsync(DateTime beforeDate)
        {
            string sql = $"DELETE FROM {TableName} WHERE RecordTime < @BeforeDate";
            return await _db.ExecuteAsync(sql, new { BeforeDate = beforeDate });
        }

        /// <summary>
        /// 获取授权记录统计信息
        /// </summary>
        public LicenseStatistics GetStatistics()
        {
            string sql = $@"
                SELECT 
                    COUNT(*) as TotalCount,
                    COUNT(DISTINCT ProjectNumber) as ProjectCount,
                    COUNT(DISTINCT DeviceNumber) as DeviceCount,
                    COUNT(DISTINCT Department) as DepartmentCount,
                    COUNT(DISTINCT Operator) as OperatorCount,
                    MAX(RecordTime) as LastRecordTime,
                    MIN(RecordTime) as FirstRecordTime
                FROM {TableName}";

            return _db.QuerySingleOrDefault<LicenseStatistics>(sql) ?? new LicenseStatistics();
        }

        /// <summary>
        /// 异步获取授权记录统计信息
        /// </summary>
        public async Task<LicenseStatistics> GetStatisticsAsync()
        {
            string sql = $@"
                SELECT 
                    COUNT(*) as TotalCount,
                    COUNT(DISTINCT ProjectNumber) as ProjectCount,
                    COUNT(DISTINCT DeviceNumber) as DeviceCount,
                    COUNT(DISTINCT Department) as DepartmentCount,
                    COUNT(DISTINCT Operator) as OperatorCount,
                    MAX(RecordTime) as LastRecordTime,
                    MIN(RecordTime) as FirstRecordTime
                FROM {TableName}";

            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<LicenseStatistics>(sql) ?? new LicenseStatistics();
            });
        }
    }

    /// <summary>
    /// 授权记录统计信息
    /// </summary>
    public class LicenseStatistics
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 项目数量
        /// </summary>
        public long ProjectCount { get; set; }

        /// <summary>
        /// 设备数量
        /// </summary>
        public long DeviceCount { get; set; }

        /// <summary>
        /// 部门数量
        /// </summary>
        public long DepartmentCount { get; set; }

        /// <summary>
        /// 记录人数量
        /// </summary>
        public long OperatorCount { get; set; }

        /// <summary>
        /// 最后记录时间
        /// </summary>
        public DateTime? LastRecordTime { get; set; }

        /// <summary>
        /// 最早记录时间
        /// </summary>
        public DateTime? FirstRecordTime { get; set; }
    }
}
