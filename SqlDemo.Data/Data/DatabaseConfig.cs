using System.Collections.Generic;

namespace SqlDemo.Data
{
    /// <summary>
    /// 数据库配置项
    /// </summary>
    public class DatabaseConfigItem
    {
        /// <summary>
        /// 配置名称（如：MainDb、LogDb）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库文件路径
        /// </summary>
        public string DbPath { get; set; }

        /// <summary>
        /// 数据库密码（可选）
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否启用 WAL 模式（提升并发性能）
        /// </summary>
        public bool EnableWalMode { get; set; } = true;
    }

    /// <summary>
    /// 数据库配置管理：用于管理多个数据库连接配置
    /// </summary>
    public static class DatabaseConfig
    {
        private static readonly Dictionary<string, DatabaseConfigItem> _configs = new Dictionary<string, DatabaseConfigItem>();

        /// <summary>
        /// 注册数据库配置
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <param name="dbPath">数据库文件路径</param>
        /// <param name="password">数据库密码（可选）</param>
        /// <param name="enableWalMode">是否启用 WAL 模式</param>
        public static void Register(string name, string dbPath, string password = null, bool enableWalMode = true)
        {
            _configs[name] = new DatabaseConfigItem
            {
                Name = name,
                DbPath = dbPath,
                Password = password,
                EnableWalMode = enableWalMode
            };
        }

        /// <summary>
        /// 注册数据库配置
        /// </summary>
        public static void Register(DatabaseConfigItem config)
        {
            _configs[config.Name] = config;
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>数据库配置项</returns>
        public static DatabaseConfigItem Get(string name)
        {
            if (_configs.TryGetValue(name, out var config))
                return config;
            return null;
        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>是否存在</returns>
        public static bool Contains(string name)
        {
            return _configs.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有配置名称
        /// </summary>
        public static IEnumerable<string> GetAllNames()
        {
            return _configs.Keys;
        }

        /// <summary>
        /// 清除所有配置
        /// </summary>
        public static void Clear()
        {
            _configs.Clear();
        }
    }
}
