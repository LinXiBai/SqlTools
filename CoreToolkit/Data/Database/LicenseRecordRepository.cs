using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreToolkit.Data.Models;
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
        /// 按申请人查询授权记录
        /// </summary>
        public IEnumerable<LicenseRecord> GetByApplicant(string applicant)
        {
            string whereClause = "Applicant = @Applicant";
            return Query(whereClause, new { Applicant = applicant });
        }

        /// <summary>
        /// 异步按申请人查询授权记录
        /// </summary>
        public async Task<IEnumerable<LicenseRecord>> GetByApplicantAsync(string applicant)
        {
            string whereClause = "Applicant = @Applicant";
            return await QueryAsync(whereClause, new { Applicant = applicant });
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
            string department = null, string operatorName = null, string applicant = null, DateTime? startDate = null, DateTime? endDate = null)
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

            if (!string.IsNullOrEmpty(applicant))
            {
                conditions.Add("Applicant = @Applicant");
                parameters.Add("Applicant", applicant);
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
            string department = null, string operatorName = null, string applicant = null, DateTime? startDate = null, DateTime? endDate = null)
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

            if (!string.IsNullOrEmpty(applicant))
            {
                conditions.Add("Applicant = @Applicant");
                parameters.Add("Applicant", applicant);
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
        /// 分页查询授权记录（支持模糊搜索和复合条件）
        /// </summary>
        public PagedResult<LicenseRecord> SearchPaged(string keyword = null,
            string department = null, string operatorName = null, string applicant = null,
            string deviceType = null, DateTime? startDate = null, DateTime? endDate = null,
            string sortColumn = "RecordTime", bool sortDescending = true,
            int pageIndex = 0, int pageSize = 50)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            // 全局关键词模糊搜索
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                conditions.Add("(ProjectNumber LIKE @Keyword OR DeviceNumber LIKE @Keyword OR Applicant LIKE @Keyword OR Operator LIKE @Keyword OR Department LIKE @Keyword)");
                parameters.Add("Keyword", $"%{keyword}%");
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

            if (!string.IsNullOrEmpty(applicant))
            {
                conditions.Add("Applicant = @Applicant");
                parameters.Add("Applicant", applicant);
            }

            if (!string.IsNullOrEmpty(deviceType))
            {
                conditions.Add("DeviceType = @DeviceType");
                parameters.Add("DeviceType", deviceType);
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

            var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            // 排序列白名单，防止 SQL 注入
            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id", "RecordTime", "Department", "Operator", "Applicant",
                "ProjectNumber", "DeviceNumber", "DeviceType", "CreatedAt"
            };
            if (!allowedSortColumns.Contains(sortColumn))
                sortColumn = "RecordTime";
            var sortDir = sortDescending ? "DESC" : "ASC";

            // 查询总数
            var countSql = $"SELECT COUNT(*) FROM {TableName} {whereClause}";
            var totalCount = _db.ExecuteScalar<long>(countSql, parameters);

            // 查询分页数据
            var dataSql = $"SELECT * FROM {TableName} {whereClause} ORDER BY {sortColumn} {sortDir} LIMIT @PageSize OFFSET @Offset";
            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", pageIndex * pageSize);

            var items = _db.Query<LicenseRecord>(dataSql, parameters);

            return new PagedResult<LicenseRecord>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 异步分页查询授权记录
        /// </summary>
        public async Task<PagedResult<LicenseRecord>> SearchPagedAsync(string keyword = null,
            string department = null, string operatorName = null, string applicant = null,
            string deviceType = null, DateTime? startDate = null, DateTime? endDate = null,
            string sortColumn = "RecordTime", bool sortDescending = true,
            int pageIndex = 0, int pageSize = 50)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                conditions.Add("(ProjectNumber LIKE @Keyword OR DeviceNumber LIKE @Keyword OR Applicant LIKE @Keyword OR Operator LIKE @Keyword OR Department LIKE @Keyword)");
                parameters.Add("Keyword", $"%{keyword}%");
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

            if (!string.IsNullOrEmpty(applicant))
            {
                conditions.Add("Applicant = @Applicant");
                parameters.Add("Applicant", applicant);
            }

            if (!string.IsNullOrEmpty(deviceType))
            {
                conditions.Add("DeviceType = @DeviceType");
                parameters.Add("DeviceType", deviceType);
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

            var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id", "RecordTime", "Department", "Operator", "Applicant",
                "ProjectNumber", "DeviceNumber", "DeviceType", "CreatedAt"
            };
            if (!allowedSortColumns.Contains(sortColumn))
                sortColumn = "RecordTime";
            var sortDir = sortDescending ? "DESC" : "ASC";

            var countSql = $"SELECT COUNT(*) FROM {TableName} {whereClause}";
            var totalCount = await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.ExecuteScalarAsync<long>(countSql, parameters);
            });

            var dataSql = $"SELECT * FROM {TableName} {whereClause} ORDER BY {sortColumn} {sortDir} LIMIT @PageSize OFFSET @Offset";
            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", pageIndex * pageSize);

            var items = await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<LicenseRecord>(dataSql, parameters);
            });

            return new PagedResult<LicenseRecord>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 获取去重后的设备类型列表
        /// </summary>
        public IEnumerable<string> GetDistinctDeviceTypes()
        {
            string sql = $"SELECT DISTINCT DeviceType FROM {TableName} WHERE DeviceType IS NOT NULL AND DeviceType != '' ORDER BY DeviceType";
            return _db.Query<string>(sql);
        }

        /// <summary>
        /// 获取去重后的部门列表
        /// </summary>
        public IEnumerable<string> GetDistinctDepartments()
        {
            string sql = $"SELECT DISTINCT Department FROM {TableName} WHERE Department IS NOT NULL AND Department != '' ORDER BY Department";
            return _db.Query<string>(sql);
        }

        /// <summary>
        /// 获取去重后的记录人列表
        /// </summary>
        public IEnumerable<string> GetDistinctOperators()
        {
            string sql = $"SELECT DISTINCT Operator FROM {TableName} WHERE Operator IS NOT NULL AND Operator != '' ORDER BY Operator";
            return _db.Query<string>(sql);
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
                    COUNT(DISTINCT Applicant) as ApplicantCount,
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
                    COUNT(DISTINCT Applicant) as ApplicantCount,
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
        /// 申请人数量
        /// </summary>
        public long ApplicantCount { get; set; }

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
