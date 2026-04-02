using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace SqlDemo.Data
{
    /// <summary>
    /// SQLite 数据库上下文：负责连接管理与最底层的命令执行
    /// </summary>
    public class SqliteDbContext : IDisposable
    {
        private readonly string _connectionString;
        private readonly bool _enableWalMode;
        private readonly string _password;
        private SQLiteConnection _connection;
        private bool _disposed = false;
        private static readonly Dictionary<string, SemaphoreSlim> _connectionLocks = new Dictionary<string, SemaphoreSlim>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 数据库文件路径
        /// </summary>
        public string DatabasePath { get; }

        public SqliteDbContext(string dbPath, string password = null, bool enableWalMode = true)
        {
            // 如果不存在会自动创建空数据库文件
            DatabasePath = dbPath;
            _enableWalMode = enableWalMode;
            _password = password;
            
            // 构建连接字符串
            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3
            };
            
            // 注意：标准 System.Data.SQLite.Core 不支持密码加密
            // 如果提供了密码，会在打开连接时尝试设置
            
            _connectionString = connectionStringBuilder.ToString();
        }

        /// <summary>获取当前连接（懒加载）</summary>
        public SQLiteConnection Connection
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SqliteDbContext));
                    
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(_connectionString);
                }
                return _connection;
            }
        }

        /// <summary>获取连接锁</summary>
        private SemaphoreSlim GetConnectionLock()
        {
            lock (_lockObject)
            {
                if (!_connectionLocks.TryGetValue(DatabasePath, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _connectionLocks[DatabasePath] = semaphore;
                }
                return semaphore;
            }
        }

        /// <summary>确保连接已打开（线程安全）</summary>
        public void EnsureOpen()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SqliteDbContext));
                
            if (Connection.State != ConnectionState.Open)
            {
                try
                {
                    // 如果有密码，尝试设置密码
                    if (!string.IsNullOrEmpty(_password))
                    {
                        // 注意：这需要 SQLite Encryption Extension (SEE) 支持
                        // 标准 System.Data.SQLite.Core 不包含此功能
                        Connection.SetPassword(_password);
                    }
                    Connection.Open();
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(_password) && ex.Message.Contains("SEE.License"))
                    {
                        throw new Exception("SQLite 加密功能需要 SQLite Encryption Extension (SEE) 支持。请使用无密码连接或安装 SEE 组件。", ex);
                    }
                    throw;
                }
            }
        }

        /// <summary>执行操作（线程安全）</summary>
        public T ExecuteWithLock<T>(Func<T> action)
        {
            var semaphore = GetConnectionLock();
            try
            {
                semaphore.Wait();
                return action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>执行操作（线程安全，异步）</summary>
        public async Task<T> ExecuteWithLockAsync<T>(Func<Task<T>> action)
        {
            var semaphore = GetConnectionLock();
            try
            {
                await semaphore.WaitAsync();
                return await action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>执行非查询 SQL（INSERT/UPDATE/DELETE）</summary>
        public int Execute(string sql, object param = null, IDbTransaction transaction = null)
        {
            return ExecuteWithLock(() =>
            {
                EnsureOpen();
                return Connection.Execute(sql, param, transaction);
            });
        }

        /// <summary>异步执行非查询 SQL</summary>
        public Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null)
        {
            return ExecuteWithLockAsync(async () =>
            {
                EnsureOpen();
                return await Connection.ExecuteAsync(sql, param, transaction);
            });
        }

        /// <summary>查询单个对象</summary>
        public T QuerySingleOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return ExecuteWithLock(() =>
            {
                EnsureOpen();
                return Connection.QuerySingleOrDefault<T>(sql, param, transaction);
            });
        }

        /// <summary>查询列表</summary>
        public IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return ExecuteWithLock(() =>
            {
                EnsureOpen();
                return Connection.Query<T>(sql, param, transaction);
            });
        }

        /// <summary>获取标量值</summary>
        public T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return ExecuteWithLock(() =>
            {
                EnsureOpen();
                return Connection.ExecuteScalar<T>(sql, param, transaction);
            });
        }

        /// <summary>开启事务</summary>
        public IDbTransaction BeginTransaction()
        {
            return ExecuteWithLock(() =>
            {
                EnsureOpen();
                return Connection.BeginTransaction();
            });
        }

        /// <summary>初始化数据库（建表等）</summary>
        public void InitDatabase()
        {
            EnsureOpen();

            // 启用 WAL 模式（如果配置启用）
            if (_enableWalMode)
            {
                Execute("PRAGMA journal_mode=WAL;");
            }

            // 初始化所有数据表
            InitUsersTable();
            InitLogsTable();
        }

        /// <summary>初始化用户表</summary>
        private void InitUsersTable()
        {
            // 创建表（如果不存在）
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    Email TEXT,
                    Age INTEGER DEFAULT 0,
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";
            Execute(createTableSql);

            // 自动检测并添加新列
            AutoAddMissingColumns<User>("Users");
        }

        /// <summary>初始化日志表</summary>
        private void InitLogsTable()
        {
            // 创建表（如果不存在）
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Level TEXT NOT NULL,
                    Message TEXT,
                    Timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";
            Execute(createTableSql);

            // 自动检测并添加新列
            AutoAddMissingColumns<Log>("Logs");
        }

        /// <summary>自动检测并添加缺失的列</summary>
        private void AutoAddMissingColumns<T>(string tableName)
        {
            try
            {
                // 获取数据库中已存在的列
                string checkSql = $"PRAGMA table_info({tableName});";
                var existingColumns = Query<ColumnInfo>(checkSql);
                var existingColumnNames = new HashSet<string>(
                    existingColumns.Select(c => c.name),
                    StringComparer.OrdinalIgnoreCase
                );

                // 获取实体类的所有公共属性
                var entityProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in entityProperties)
                {
                    // 跳过索引器
                    if (property.GetIndexParameters().Length > 0)
                        continue;

                    // 如果列已存在，跳过
                    if (existingColumnNames.Contains(property.Name))
                        continue;

                    // 获取对应的 SQLite 数据类型
                    string sqliteType = GetSqliteType(property.PropertyType);

                    // 添加新列
                    string addColumnSql = $"ALTER TABLE {tableName} ADD COLUMN {property.Name} {sqliteType};";
                    Execute(addColumnSql);
                }
            }
            catch (SQLiteException)
            {
                // 忽略错误（例如列已存在）
            }
        }

        /// <summary>获取属性类型对应的 SQLite 数据类型</summary>
        private string GetSqliteType(Type propertyType)
        {
            if (propertyType == typeof(string))
                return "TEXT";
            if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(short))
                return "INTEGER";
            if (propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float))
                return "REAL";
            if (propertyType == typeof(bool))
                return "INTEGER";
            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
                return "TEXT";
            if (propertyType == typeof(byte[]))
                return "BLOB";

            // 默认使用 TEXT
            return "TEXT";
        }

        /// <summary>列信息结构体</summary>
        private class ColumnInfo
        {
            public string name { get; set; }
        }

        /// <summary>备份数据库到指定路径</summary>
        public void BackupDatabase(string backupPath)
        {
            ExecuteWithLock<object>(() =>
            {
                // 关闭连接以确保文件不被锁定
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                
                try
                {
                    // 复制数据库文件
                    if (System.IO.File.Exists(DatabasePath))
                    {
                        // 确保目标目录存在
                        string backupDir = System.IO.Path.GetDirectoryName(backupPath);
                        if (!string.IsNullOrEmpty(backupDir) && !System.IO.Directory.Exists(backupDir))
                        {
                            System.IO.Directory.CreateDirectory(backupDir);
                        }
                        
                        // 复制文件
                        System.IO.File.Copy(DatabasePath, backupPath, true);
                    }
                }
                finally
                {
                    // 重新打开连接
                    if (_connection != null && _connection.State != ConnectionState.Open)
                    {
                        try
                        {
                            _connection.Open();
                        }
                        catch
                        {
                            // 忽略错误
                        }
                    }
                }
                return null;
            });
        }

        /// <summary>异步备份数据库到指定路径</summary>
        public async Task BackupDatabaseAsync(string backupPath)
        {
            await ExecuteWithLockAsync<object>(async () =>
            {
                // 关闭连接以确保文件不被锁定
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                
                try
                {
                    // 复制数据库文件
                    if (System.IO.File.Exists(DatabasePath))
                    {
                        // 确保目标目录存在
                        string backupDir = System.IO.Path.GetDirectoryName(backupPath);
                        if (!string.IsNullOrEmpty(backupDir) && !System.IO.Directory.Exists(backupDir))
                        {
                            System.IO.Directory.CreateDirectory(backupDir);
                        }
                        
                        // 复制文件（异步）
                        using (var sourceStream = new System.IO.FileStream(DatabasePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                        using (var destinationStream = new System.IO.FileStream(backupPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                    }
                }
                finally
                {
                    // 重新打开连接
                    if (_connection != null && _connection.State != ConnectionState.Open)
                    {
                        try
                        {
                            _connection.Open();
                        }
                        catch
                        {
                            // 忽略错误
                        }
                    }
                }
                return null;
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    try
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }
                    catch
                    {
                        // 忽略清理时的错误
                    }
                    finally
                    {
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }
    }
}
