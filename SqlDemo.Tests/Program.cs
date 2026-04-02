using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SqlDemo.Data;

namespace SqlDemo.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== SQLite 多数据库连接示例 ===");
                Console.WriteLine();

                // 注册数据库配置
                DatabaseConfig.Register("MainDb", "main.db");
                DatabaseConfig.Register("LogDb", "logs.db");
                DatabaseConfig.Register("ArchiveDb", "archive.db");

                Console.WriteLine("已注册数据库配置：");
                foreach (var configName in DatabaseConfig.GetAllNames())
                {
                    var config = DatabaseConfig.Get(configName);
                    Console.WriteLine($"  - {config.Name}: {config.DbPath} (密码: {(string.IsNullOrEmpty(config.Password) ? "无" : "有")})");
                }
                Console.WriteLine();

                // 初始化所有数据库
                Console.WriteLine("正在初始化数据库...");
                DatabaseFactory.InitializeAllDatabases();
                Console.WriteLine("所有数据库初始化完成。");
                Console.WriteLine();

                // 测试主数据库（用户表）
                Console.WriteLine("--- 主数据库操作（Users）---");
                using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                {
                    var userRepo = new UserRepository(mainDb);

                    // 插入用户
                    var user = new User { UserName = "张三", Email = "zhangsan@example.com", Age = 28, IsActive = true };
                    long userId = userRepo.Insert(user);
                    Console.WriteLine($"[MainDb] 插入用户，Id = {userId}");

                    // 查询用户
                    var userResult = userRepo.GetById(userId);
                    if (userResult != null)
                    {
                        Console.WriteLine($"[MainDb] 查询用户：{userResult.UserName}, {userResult.Email}");
                    }

                    // 获取活跃用户数
                    var activeUsers = userRepo.GetActiveUsers();
                    Console.WriteLine($"[MainDb] 当前活跃用户数量：{activeUsers.Count()}");
                }
                Console.WriteLine();

                // 测试日志数据库
                Console.WriteLine("--- 日志数据库操作（Logs）---");
                using (var logDb = DatabaseFactory.CreateContext("LogDb"))
                {
                    var logRepo = new LogRepository(logDb);

                    // 写入不同级别的日志
                    long logId1 = logRepo.WriteDebug("调试信息：应用程序初始化");
                    long logId2 = logRepo.WriteInfo("应用程序启动成功");
                    long logId3 = logRepo.WriteWarning("警告：内存使用过高");
                    long logId4 = logRepo.WriteError("错误：无法连接到外部服务");
                    long logId5 = logRepo.WriteFatal("致命错误：数据库连接失败");
                    Console.WriteLine($"[LogDb] 写入不同级别日志，Id = {logId1}, {logId2}, {logId3}, {logId4}, {logId5}");

                    // 写入异常日志
                    try
                    {
                        throw new Exception("测试异常");
                    }
                    catch (Exception ex)
                    {
                        long logId6 = logRepo.WriteException(ex, "测试异常处理");
                        Console.WriteLine($"[LogDb] 写入异常日志，Id = {logId6}");
                    }

                    // 查询最近的日志
                    var recentLogs = logRepo.GetRecentLogs(10);
                    Console.WriteLine("[LogDb] 最近 10 条日志：");
                    foreach (var log in recentLogs)
                    {
                        Console.WriteLine($"  [{log.Level}] {log.Timestamp} - {log.Message} (Logger: {log.LoggerName}, Machine: {log.MachineName})");
                    }

                    // 按级别查询日志
                    var errorLogs = logRepo.GetByLevel("Error");
                    Console.WriteLine($"[LogDb] 错误级别日志数量：{errorLogs.Count()}");

                    // 按时间范围查询日志
                    var startDate = DateTime.Now.AddHours(-1);
                    var endDate = DateTime.Now.AddHours(1);
                    var timeRangeLogs = logRepo.GetLogsByTimeRange(startDate, endDate);
                    Console.WriteLine($"[LogDb] 时间范围内日志数量：{timeRangeLogs.Count()}");

                    // 测试异步日志操作
                    Console.WriteLine("[LogDb] 测试异步日志操作...");
                    long asyncLogId = await logRepo.WriteInfoAsync("异步日志测试");
                    Console.WriteLine($"[LogDb] 异步写入日志，Id = {asyncLogId}");

                    var asyncRecentLogs = await logRepo.GetRecentLogsAsync(3);
                    Console.WriteLine("[LogDb] 异步查询最近 3 条日志：");
                    foreach (var log in asyncRecentLogs)
                    {
                        Console.WriteLine($"  [{log.Level}] {log.Timestamp} - {log.Message}");
                    }
                }
                Console.WriteLine();

                // 跨数据库操作演示
                Console.WriteLine("--- 跨数据库操作演示 ---");
                using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                using (var logDb = DatabaseFactory.CreateContext("LogDb"))
                {
                    var userRepo = new UserRepository(mainDb);
                    var logRepo = new LogRepository(logDb);

                    // 创建用户
                    var newUser = new User { UserName = "李四", Email = "lisi@example.com", Age = 30, IsActive = true };
                    long newUserId = userRepo.Insert(newUser);
                    Console.WriteLine($"[MainDb] 创建用户：{newUser.UserName} (Id: {newUserId})");

                    // 记录操作日志
                    logRepo.WriteInfo($"创建新用户：{newUser.UserName}");
                    Console.WriteLine("[LogDb] 记录操作日志：创建新用户");

                    Console.WriteLine("跨数据库操作完成（注意：SQLite 不支持跨库分布式事务）");
                }

                // 测试密码连接（仅演示配置，实际会失败）
                Console.WriteLine();
                Console.WriteLine("--- 密码连接测试（演示）---");
                try
                {
                    // 注册带密码的数据库配置
                    DatabaseConfig.Register("SecureDb", "secure.db", "MySecurePassword123");
                    Console.WriteLine("[SecureDb] 已注册带密码的数据库配置");
                    
                    // 尝试初始化（会失败，因为标准 SQLite 不支持加密）
                    using (var secureDb = DatabaseFactory.CreateContext("SecureDb"))
                    {
                        secureDb.InitDatabase();
                        Console.WriteLine("[SecureDb] 初始化成功（需要 SQLite Encryption Extension）");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SecureDb] 密码连接测试：{ex.Message}");
                    Console.WriteLine("提示：要使用密码加密功能，需要安装 SQLite Encryption Extension (SEE)");
                }

                // 测试批量操作
                Console.WriteLine();
                Console.WriteLine("--- 批量操作测试 ---");
                using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                {
                    var userRepo = new UserRepository(mainDb);

                    // 批量插入
                    var users = new List<User>
                    {
                        new User { UserName = "王五", Email = "wangwu@example.com", Age = 25, IsActive = true },
                        new User { UserName = "赵六", Email = "zhaoliu@example.com", Age = 35, IsActive = true },
                        new User { UserName = "孙七", Email = "sunqi@example.com", Age = 40, IsActive = false }
                    };
                    userRepo.InsertBatch(users);
                    Console.WriteLine("[MainDb] 批量插入用户完成");

                    // 批量更新
                    var allUsers = userRepo.GetAll().ToList();
                    foreach (var u in allUsers)
                    {
                        u.Age += 1;
                    }
                    userRepo.UpdateBatch(allUsers);
                    Console.WriteLine("[MainDb] 批量更新用户年龄完成");

                    // 批量删除
                    var userIdsToDelete = allUsers.Take(2).Select(u => u.Id).ToList();
                    userRepo.DeleteBatch(userIdsToDelete);
                    Console.WriteLine("[MainDb] 批量删除用户完成");
                }

                // 测试数据导出/导入
                Console.WriteLine();
                Console.WriteLine("--- 数据导出/导入测试 ---");
                using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                {
                    var userRepo = new UserRepository(mainDb);

                    // 导出为 JSON
                    string json = userRepo.ExportToJson();
                    Console.WriteLine("[MainDb] 导出用户数据为 JSON 完成");
                    File.WriteAllText("users.json", json);

                    // 导出为 CSV
                    string csv = userRepo.ExportToCsv();
                    Console.WriteLine("[MainDb] 导出用户数据为 CSV 完成");
                    File.WriteAllText("users.csv", csv);

                    // 导入 JSON（演示）
                    // userRepo.ImportFromJsonFile("users.json");
                    // Console.WriteLine("[MainDb] 从 JSON 导入用户数据完成");
                }

                // 测试多线程安全
                Console.WriteLine();
                Console.WriteLine("--- 多线程安全测试 ---");
                var tasks = new List<Task>();
                for (int i = 0; i < 5; i++)
                {
                    int threadId = i;
                    tasks.Add(Task.Run(() =>
                    {
                        using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                        {
                            var userRepo = new UserRepository(mainDb);
                            var user = new User { UserName = $"线程用户{threadId}", Email = $"thread{threadId}@example.com", Age = 20 + threadId, IsActive = true };
                            long userId = userRepo.Insert(user);
                            Console.WriteLine($"[线程{threadId}] 插入用户，Id = {userId}");
                        }
                    }));
                }
                await Task.WhenAll(tasks);
                Console.WriteLine("多线程操作完成");

                // 测试异步操作
                Console.WriteLine();
                Console.WriteLine("--- 异步操作测试 ---");
                using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
                {
                    var userRepo = new UserRepository(mainDb);

                    // 异步插入
                    var asyncUser = new User { UserName = "异步用户", Email = "async@example.com", Age = 30, IsActive = true };
                    long asyncUserId = await userRepo.InsertAsync(asyncUser);
                    Console.WriteLine($"[MainDb] 异步插入用户，Id = {asyncUserId}");

                    // 异步查询
                    var asyncUserResult = await userRepo.GetByIdAsync(asyncUserId);
                    if (asyncUserResult != null)
                    {
                        Console.WriteLine($"[MainDb] 异步查询用户：{asyncUserResult.UserName}, {asyncUserResult.Email}");
                    }

                    // 异步批量操作
                    var asyncUsers = new List<User>
                    {
                        new User { UserName = "异步用户1", Email = "async1@example.com", Age = 25, IsActive = true },
                        new User { UserName = "异步用户2", Email = "async2@example.com", Age = 35, IsActive = true }
                    };
                    await userRepo.InsertBatchAsync(asyncUsers);
                    Console.WriteLine("[MainDb] 异步批量插入用户完成");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
                Console.WriteLine($"堆栈：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部错误：{ex.InnerException.Message}");
                }
            }

            // 测试数据库备份
            Console.WriteLine();
            Console.WriteLine("--- 数据库备份测试 ---");
            try
            {
                string backupDir = "backups";
                Console.WriteLine($"正在备份所有数据库到目录：{backupDir}");
                
                // 同步备份所有数据库
                DatabaseFactory.BackupAllDatabases(backupDir);
                Console.WriteLine("同步备份所有数据库完成");

                // 异步备份指定数据库
                string mainDbBackupPath = System.IO.Path.Combine(backupDir, $"MainDb_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                await DatabaseFactory.BackupDatabaseAsync("MainDb", mainDbBackupPath);
                Console.WriteLine($"异步备份 MainDb 完成，备份文件：{mainDbBackupPath}");

                // 验证备份文件存在
                if (System.IO.Directory.Exists(backupDir))
                {
                    var backupFiles = System.IO.Directory.GetFiles(backupDir, "*.db");
                    Console.WriteLine($"备份目录中共有 {backupFiles.Length} 个备份文件");
                    foreach (var file in backupFiles.Take(5)) // 只显示前 5 个文件
                    {
                        Console.WriteLine($"  - {System.IO.Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"备份错误：{ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
