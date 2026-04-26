using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace LicenseManager.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            BackupLicenseDatabase();
        }

        /// <summary>
        /// 获取 license.db 的持久化存储路径（%LocalAppData%/LicenseManager/）
        /// </summary>
        public static string GetLicenseDbPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(appData, "LicenseManager");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "license.db");
        }

        /// <summary>
        /// 启动时自动备份 license.db，保留最近 10 个备份
        /// </summary>
        private void BackupLicenseDatabase()
        {
            var dbPath = GetLicenseDbPath();
            if (!File.Exists(dbPath))
                return;

            var backupDir = Path.Combine(Path.GetDirectoryName(dbPath), "backups");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"license_{timestamp}.db");

            try
            {
                File.Copy(dbPath, backupPath, overwrite: true);

                var oldBackups = new DirectoryInfo(backupDir)
                    .GetFiles("license_*.db")
                    .OrderByDescending(f => f.LastWriteTime)
                    .Skip(10);

                foreach (var old in oldBackups)
                {
                    try { old.Delete(); }
                    catch { }
                }
            }
            catch
            {
                // 备份失败不应阻止应用启动
            }
        }
    }
}
