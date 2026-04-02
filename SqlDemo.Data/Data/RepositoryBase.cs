using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace SqlDemo.Data
{
    /// <summary>
    /// 通用泛型仓储基类：为所有实体提供基础 CRUD 操作
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class RepositoryBase<T> where T : class
    {
        protected readonly SqliteDbContext _db;

        public RepositoryBase(SqliteDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 表名（默认使用类名的复数形式）
        /// </summary>
        protected virtual string TableName
        {
            get
            {
                string className = typeof(T).Name;
                // 简单的复数转换规则
                if (className.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                    return className.Substring(0, className.Length - 1) + "ies";
                if (className.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    return className + "es";
                return className + "s";
            }
        }

        /// <summary>
        /// 根据 ID 查询
        /// </summary>
        public T GetById(long id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
            return _db.QuerySingleOrDefault<T>(sql, new { Id = id });
        }

        /// <summary>
        /// 异步根据 ID 查询
        /// </summary>
        public async Task<T> GetByIdAsync(long id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
            });
        }

        /// <summary>
        /// 查询所有记录
        /// </summary>
        public IEnumerable<T> GetAll()
        {
            string sql = $"SELECT * FROM {TableName}";
            return _db.Query<T>(sql);
        }

        /// <summary>
        /// 异步查询所有记录
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            string sql = $"SELECT * FROM {TableName}";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<T>(sql);
            });
        }

        /// <summary>
        /// 插入记录
        /// </summary>
        /// <returns>自增 ID</returns>
        public long Insert(T entity)
        {
            var properties = GetInsertProperties();
            var columns = string.Join(", ", properties.Keys);
            var parameters = string.Join(", ", properties.Values);

            string sql = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters}); SELECT last_insert_rowid();";
            long id = _db.ExecuteScalar<long>(sql, entity);
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Insert", entity, id);
            
            return id;
        }

        /// <summary>
        /// 异步插入记录
        /// </summary>
        /// <returns>自增 ID</returns>
        public async Task<long> InsertAsync(T entity)
        {
            var properties = GetInsertProperties();
            var columns = string.Join(", ", properties.Keys);
            var parameters = string.Join(", ", properties.Values);

            string sql = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters}); SELECT last_insert_rowid();";
            long id = await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.ExecuteScalarAsync<long>(sql, entity);
            });
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Insert", entity, id);
            
            return id;
        }

        /// <summary>
        /// 批量插入（带事务，优化版）
        /// </summary>
        public void InsertBatch(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (entityList.Count == 0) return;
            
            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    var properties = GetInsertProperties();
                    if (properties.Count == 0) return;

                    var columns = string.Join(", ", properties.Keys);
                    var parameters = string.Join(", ", properties.Values);
                    string sql = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters});";

                    foreach (var entity in entityList)
                    {
                        _db.Execute(sql, entity, transaction);
                    }
                    transaction.Commit();
                    
                    // 输出数据库操作词条
                    OutputDatabaseOperationEntry("InsertBatch", null, entityList.Count);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 异步批量插入（带事务，优化版）
        /// </summary>
        public async Task InsertBatchAsync(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (entityList.Count == 0) return;
            
            await _db.ExecuteWithLockAsync<object>(async () =>
            {
                _db.EnsureOpen();
                using (var transaction = _db.Connection.BeginTransaction())
                {
                    try
                    {
                        var properties = GetInsertProperties();
                        if (properties.Count == 0) return null;

                        var columns = string.Join(", ", properties.Keys);
                        var parameters = string.Join(", ", properties.Values);
                        string sql = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters});";

                        foreach (var entity in entityList)
                        {
                            await _db.Connection.ExecuteAsync(sql, entity, transaction);
                        }
                        transaction.Commit();
                        
                        // 输出数据库操作词条
                        OutputDatabaseOperationEntry("InsertBatch", null, entityList.Count);
                        return null;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// 批量更新（带事务）
        /// </summary>
        public void UpdateBatch(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (entityList.Count == 0) return;
            
            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    var updateProperties = GetUpdateProperties();
                    if (updateProperties.Count == 0) return;

                    var setClause = string.Join(", ", updateProperties);
                    string sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";

                    foreach (var entity in entityList)
                    {
                        _db.Execute(sql, entity, transaction);
                    }
                    transaction.Commit();
                    
                    // 输出数据库操作词条
                    OutputDatabaseOperationEntry("UpdateBatch", null, entityList.Count);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 异步批量更新（带事务）
        /// </summary>
        public async Task UpdateBatchAsync(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (entityList.Count == 0) return;
            
            await _db.ExecuteWithLockAsync<object>(async () =>
            {
                _db.EnsureOpen();
                using (var transaction = _db.Connection.BeginTransaction())
                {
                    try
                    {
                        var updateProperties = GetUpdateProperties();
                        if (updateProperties.Count == 0) return null;

                        var setClause = string.Join(", ", updateProperties);
                        string sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";

                        foreach (var entity in entityList)
                        {
                            await _db.Connection.ExecuteAsync(sql, entity, transaction);
                        }
                        transaction.Commit();
                        
                        // 输出数据库操作词条
                        OutputDatabaseOperationEntry("UpdateBatch", null, entityList.Count);
                        return null;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// 批量删除（带事务）
        /// </summary>
        public void DeleteBatch(IEnumerable<long> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return;
            
            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    string sql = $"DELETE FROM {TableName} WHERE Id = @Id";

                    foreach (var id in idList)
                    {
                        _db.Execute(sql, new { Id = id }, transaction);
                    }
                    transaction.Commit();
                    
                    // 输出数据库操作词条
                    OutputDatabaseOperationEntry("DeleteBatch", null, idList.Count);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 异步批量删除（带事务）
        /// </summary>
        public async Task DeleteBatchAsync(IEnumerable<long> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return;
            
            await _db.ExecuteWithLockAsync<object>(async () =>
            {
                _db.EnsureOpen();
                using (var transaction = _db.Connection.BeginTransaction())
                {
                    try
                    {
                        string sql = $"DELETE FROM {TableName} WHERE Id = @Id";

                        foreach (var id in idList)
                        {
                            await _db.Connection.ExecuteAsync(sql, new { Id = id }, transaction);
                        }
                        transaction.Commit();
                        
                        // 输出数据库操作词条
                        OutputDatabaseOperationEntry("DeleteBatch", null, idList.Count);
                        return null;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        public void Update(T entity)
        {
            var properties = GetUpdateProperties();
            var setClause = string.Join(", ", properties);

            string sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";
            int affectedRows = _db.Execute(sql, entity);
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Update", entity, affectedRows);
        }

        /// <summary>
        /// 异步更新记录
        /// </summary>
        public async Task UpdateAsync(T entity)
        {
            var properties = GetUpdateProperties();
            var setClause = string.Join(", ", properties);

            string sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";
            int affectedRows = await _db.ExecuteAsync(sql, entity);
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Update", entity, affectedRows);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        public void Delete(long id)
        {
            string sql = $"DELETE FROM {TableName} WHERE Id = @Id";
            int affectedRows = _db.Execute(sql, new { Id = id });
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Delete", null, affectedRows, id);
        }

        /// <summary>
        /// 异步删除记录
        /// </summary>
        public async Task DeleteAsync(long id)
        {
            string sql = $"DELETE FROM {TableName} WHERE Id = @Id";
            int affectedRows = await _db.ExecuteAsync(sql, new { Id = id });
            
            // 输出数据库操作词条
            OutputDatabaseOperationEntry("Delete", null, affectedRows, id);
        }

        /// <summary>
        /// 条件查询
        /// </summary>
        public IEnumerable<T> Query(string whereClause, object param = null)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {whereClause}";
            return _db.Query<T>(sql, param);
        }

        /// <summary>
        /// 异步条件查询
        /// </summary>
        public async Task<IEnumerable<T>> QueryAsync(string whereClause, object param = null)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {whereClause}";
            return await _db.ExecuteWithLockAsync(async () =>
            {
                _db.EnsureOpen();
                return await _db.Connection.QueryAsync<T>(sql, param);
            });
        }

        /// <summary>
        /// 获取用于插入的属性
        /// </summary>
        private Dictionary<string, string> GetInsertProperties()
        {
            var properties = new Dictionary<string, string>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.Name != "Id" && IsSimpleType(prop.PropertyType))
                {
                    properties[prop.Name] = "@" + prop.Name;
                }
            }
            return properties;
        }

        /// <summary>
        /// 获取用于更新的属性
        /// </summary>
        private List<string> GetUpdateProperties()
        {
            var properties = new List<string>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.Name != "Id" && IsSimpleType(prop.PropertyType))
                {
                    properties.Add($"{prop.Name} = @{prop.Name}");
                }
            }
            return properties;
        }

        /// <summary>
        /// 判断是否为简单类型
        /// </summary>
        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || 
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(bool) ||
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        #region 数据导出/导入

        /// <summary>
        /// 导出数据为 JSON 格式
        /// </summary>
        public string ExportToJson()
        {
            var entities = GetAll();
            return JsonConvert.SerializeObject(entities, Formatting.Indented);
        }

        /// <summary>
        /// 导出数据为 JSON 文件
        /// </summary>
        public void ExportToJsonFile(string filePath)
        {
            var json = ExportToJson();
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// 导出数据为 CSV 格式
        /// </summary>
        public string ExportToCsv()
        {
            var entities = GetAll();
            if (!entities.Any())
                return string.Empty;

            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties().Where(p => IsSimpleType(p.PropertyType)).ToList();

            // 写入表头
            var header = string.Join(",", properties.Select(p => p.Name));
            sb.AppendLine(header);

            // 写入数据行
            foreach (var entity in entities)
            {
                var values = new List<string>();
                foreach (var property in properties)
                {
                    var value = property.GetValue(entity);
                    var stringValue = value?.ToString() ?? "";
                    // 处理包含逗号或引号的值
                    if (stringValue.Contains(",") || stringValue.Contains("\""))
                    {
                        stringValue = $"\"{stringValue.Replace("\"", "\\\"")}\"";
                    }
                    values.Add(stringValue);
                }
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 导出数据为 CSV 文件
        /// </summary>
        public void ExportToCsvFile(string filePath)
        {
            var csv = ExportToCsv();
            File.WriteAllText(filePath, csv, Encoding.UTF8);
        }

        /// <summary>
        /// 从 JSON 导入数据
        /// </summary>
        public void ImportFromJson(string json)
        {
            var entities = JsonConvert.DeserializeObject<List<T>>(json);
            if (entities != null && entities.Count > 0)
            {
                InsertBatch(entities);
            }
        }

        /// <summary>
        /// 从 JSON 文件导入数据
        /// </summary>
        public void ImportFromJsonFile(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            ImportFromJson(json);
        }

        /// <summary>
        /// 从 CSV 导入数据
        /// </summary>
        public void ImportFromCsv(string csv)
        {
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) // 至少需要表头和一行数据
                return;

            var headers = lines[0].Split(',');
            var entities = new List<T>();
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i]);
                var entity = Activator.CreateInstance<T>();

                for (int j = 0; j < Math.Min(headers.Length, values.Count); j++)
                {
                    if (properties.TryGetValue(headers[j], out var property))
                    {
                        try
                        {
                            var value = Convert.ChangeType(values[j], property.PropertyType);
                            property.SetValue(entity, value);
                        }
                        catch
                        {
                            // 忽略类型转换错误
                        }
                    }
                }

                entities.Add(entity);
            }

            if (entities.Count > 0)
            {
                InsertBatch(entities);
            }
        }

        /// <summary>
        /// 从 CSV 文件导入数据
        /// </summary>
        public void ImportFromCsvFile(string filePath)
        {
            var csv = File.ReadAllText(filePath, Encoding.UTF8);
            ImportFromCsv(csv);
        }

        /// <summary>
        /// 解析 CSV 行
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 处理转义的引号
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            result.Add(currentValue.ToString());
            return result;
        }

        /// <summary>
        /// 输出数据库操作词条
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="entity">实体对象</param>
        /// <param name="affectedRows">影响的行数或ID</param>
        /// <param name="entityId">实体ID（用于删除操作）</param>
        protected virtual void OutputDatabaseOperationEntry(string operationType, T entity, long affectedRows, long? entityId = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string entityType = typeof(T).Name;
            
            string entry = string.Empty;
            
            switch (operationType)
            {
                case "Insert":
                    entry = $"[{timestamp}] [Database] [{entityType}] 插入操作: ID = {affectedRows}";
                    break;
                case "Update":
                    long updateId = entityId ?? (entity != null ? GetEntityId(entity) : 0);
                    entry = $"[{timestamp}] [Database] [{entityType}] 更新操作: ID = {updateId}, 影响行数 = {affectedRows}";
                    break;
                case "Delete":
                    entry = $"[{timestamp}] [Database] [{entityType}] 删除操作: ID = {entityId ?? affectedRows}, 影响行数 = {affectedRows}";
                    break;
                case "InsertBatch":
                    entry = $"[{timestamp}] [Database] [{entityType}] 批量插入操作: 插入数量 = {affectedRows}";
                    break;
                case "UpdateBatch":
                    entry = $"[{timestamp}] [Database] [{entityType}] 批量更新操作: 更新数量 = {affectedRows}";
                    break;
                case "DeleteBatch":
                    entry = $"[{timestamp}] [Database] [{entityType}] 批量删除操作: 删除数量 = {affectedRows}";
                    break;
                default:
                    entry = $"[{timestamp}] [Database] [{entityType}] {operationType} 操作: 影响行数 = {affectedRows}";
                    break;
            }
            
            Console.WriteLine(entry);
        }

        /// <summary>
        /// 获取实体ID
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>实体ID</returns>
        private long GetEntityId(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                if (value is long longValue)
                {
                    return longValue;
                }
            }
            return 0;
        }

        #endregion
    }
}
