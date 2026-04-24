using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CoreToolkit.Data
{
    /// <summary>
    /// DPAPI 密码加密辅助类（当前用户级别保护）
    /// </summary>
    internal static class PasswordProtector
    {
        /// <summary>
        /// 加密明文密码
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 解密密码。如果解密失败（可能是旧版明文），则返回原文以兼容旧数据。
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (FormatException)
            {
                // 不是 Base64，说明是旧版明文，直接返回
                return cipherText;
            }
            catch (CryptographicException)
            {
                // 解密失败（可能在其他用户环境下），返回原文尝试兼容
                return cipherText;
            }
        }
    }
    /// <summary>
    /// 数据库配置项
    /// </summary>
    public class DatabaseConfigItem
    {
        private string _encryptedPassword;

        /// <summary>
        /// 配置名称（如：MainDb、LogDb）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库文件路径
        /// </summary>
        public string DbPath { get; set; }

        /// <summary>
        /// 数据库密码（可选）。设置时会自动加密，读取时自动解密。
        /// </summary>
        public string Password
        {
            get => string.IsNullOrEmpty(_encryptedPassword) ? null : PasswordProtector.Decrypt(_encryptedPassword);
            set => _encryptedPassword = string.IsNullOrEmpty(value) ? null : PasswordProtector.Encrypt(value);
        }

        /// <summary>
        /// 获取加密后的密码（用于持久化存储）
        /// </summary>
        internal string EncryptedPassword => _encryptedPassword;

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
