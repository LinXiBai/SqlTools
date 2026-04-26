using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 数据库上下文工厂：统一管理多个数据库连接的创建
    /// </summary>
    public static class DatabaseFactory
    {
        /// <summary>
        /// 创建新的数据库上下文
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>数据库上下文</returns>
        public static SqliteDbContext CreateContext(string configName)
        {
            var config = DatabaseConfig.Get(configName);
            if (config == null)
                throw new Exception($"数据库配置 '{configName}' 未找到");

            return new SqliteDbContext(config.DbPath, config.Password, config.EnableWalMode);
        }

        /// <summary>
        /// 初始化所有注册的数据库
        /// </summary>
        public static void InitializeAllDatabases()
        {
            foreach (var configName in DatabaseConfig.GetAllNames())
            {
                using (var context = CreateContext(configName))
                {
                    context.InitDatabase();
                }
            }
        }

        /// <summary>
        /// 初始化指定数据库
        /// </summary>
        public static void InitializeDatabase(string configName)
        {
            using (var context = CreateContext(configName))
            {
                context.InitDatabase();
            }
        }

        /// <summary>
        /// 备份指定数据库
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <param name="backupPath">备份文件路径</param>
        public static void BackupDatabase(string configName, string backupPath)
        {
            using (var context = CreateContext(configName))
            {
                context.BackupDatabase(backupPath);
            }
        }

        /// <summary>
        /// 异步备份指定数据库
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <param name="backupPath">备份文件路径</param>
        public static async Task BackupDatabaseAsync(string configName, string backupPath)
        {
            using (var context = CreateContext(configName))
            {
                await context.BackupDatabaseAsync(backupPath);
            }
        }

        /// <summary>
        /// 备份所有注册的数据库
        /// </summary>
        /// <param name="backupDirectory">备份目录路径</param>
        public static void BackupAllDatabases(string backupDirectory)
        {
            // 确保备份目录存在
            if (!System.IO.Directory.Exists(backupDirectory))
            {
                System.IO.Directory.CreateDirectory(backupDirectory);
            }

            foreach (var configName in DatabaseConfig.GetAllNames())
            {
                var config = DatabaseConfig.Get(configName);
                if (config != null)
                {
                    string backupPath = System.IO.Path.Combine(backupDirectory, $"{configName}_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                    BackupDatabase(configName, backupPath);
                }
            }
        }

        /// <summary>
        /// 异步备份所有注册的数据库
        /// </summary>
        /// <param name="backupDirectory">备份目录路径</param>
        public static async Task BackupAllDatabasesAsync(string backupDirectory)
        {
            // 确保备份目录存在
            if (!System.IO.Directory.Exists(backupDirectory))
            {
                System.IO.Directory.CreateDirectory(backupDirectory);
            }

            foreach (var configName in DatabaseConfig.GetAllNames())
            {
                var config = DatabaseConfig.Get(configName);
                if (config != null)
                {
                    string backupPath = System.IO.Path.Combine(backupDirectory, $"{configName}_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                    await BackupDatabaseAsync(configName, backupPath);
                }
            }
        }
    }
}
