using System;
using System.Linq;
using SqlDemo.Data;

namespace SqlDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SQLite 多数据库连接示例 ===\n");

            // 1. 注册多个数据库配置
            RegisterDatabaseConfigs();

            // 2. 初始化所有数据库
            Console.WriteLine("正在初始化数据库...");
            DatabaseFactory.InitializeAllDatabases();
            Console.WriteLine("所有数据库初始化完成。\n");

            // 3. 演示多数据库操作
            DemoMultipleDatabases();

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 注册多个数据库配置
        /// </summary>
        static void RegisterDatabaseConfigs()
        {
            // 主数据库：存储业务数据
            DatabaseConfig.Register("MainDb", "main.db", enableWalMode: true);

            // 日志数据库：存储日志数据（分离存储，避免影响业务数据库性能）
            DatabaseConfig.Register("LogDb", "logs.db", enableWalMode: true);

            // 归档数据库：存储历史数据（可选）
            DatabaseConfig.Register("ArchiveDb", "archive.db", enableWalMode: true);

            Console.WriteLine("已注册数据库配置：");
            foreach (var name in DatabaseConfig.GetAllNames())
            {
                var config = DatabaseConfig.Get(name);
                Console.WriteLine($"  - {name}: {config.DbPath}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 演示多数据库操作
        /// </summary>
        static void DemoMultipleDatabases()
        {
            // 使用主数据库操作用户数据
            using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
            {
                var userRepo = new UserRepository(mainDb);

                Console.WriteLine("--- 主数据库操作（Users）---");

                // 插入用户
                var newUser = new User
                {
                    UserName = "张三",
                    Email = "zhangsan@example.com",
                    Age = 28
                };
                long userId = userRepo.Insert(newUser);
                Console.WriteLine($"[MainDb] 插入用户，Id = {userId}");

                // 查询用户
                var user = userRepo.GetById(userId);
                Console.WriteLine($"[MainDb] 查询用户：{user.UserName}, {user.Email}");

                // 条件查询
                var activeUsers = userRepo.GetActiveUsers().ToList();
                Console.WriteLine($"[MainDb] 当前活跃用户数量：{activeUsers.Count}");
            }

            // 使用日志数据库操作日志数据
            using (var logDb = DatabaseFactory.CreateContext("LogDb"))
            {
                var logRepo = new LogRepository(logDb);

                Console.WriteLine("\n--- 日志数据库操作（Logs）---");

                // 写入日志
                long logId1 = logRepo.WriteInfo("应用程序启动成功");
                long logId2 = logRepo.WriteInfo($"用户登录成功，用户Id: 1");
                Console.WriteLine($"[LogDb] 写入日志，Id = {logId1}, {logId2}");

                // 查询日志
                var recentLogs = logRepo.GetRecentLogs(5).ToList();
                Console.WriteLine($"[LogDb] 最近 {recentLogs.Count} 条日志：");
                foreach (var log in recentLogs)
                {
                    Console.WriteLine($"  [{log.Level}] {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.Message}");
                }
            }

            // 演示跨数据库事务（分别操作，非分布式事务）
            Console.WriteLine("\n--- 跨数据库操作演示 ---");
            CrossDatabaseOperation();
        }

        /// <summary>
        /// 跨数据库操作示例
        /// </summary>
        static void CrossDatabaseOperation()
        {
            // 在主数据库创建用户
            using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
            {
                var userRepo = new UserRepository(mainDb);
                var user = new User
                {
                    UserName = "李四",
                    Email = "lisi@example.com",
                    Age = 32
                };
                long userId = userRepo.Insert(user);
                Console.WriteLine($"[MainDb] 创建用户：{user.UserName} (Id: {userId})");
            }

            // 在日志数据库记录操作
            using (var logDb = DatabaseFactory.CreateContext("LogDb"))
            {
                var logRepo = new LogRepository(logDb);
                logRepo.WriteInfo("创建新用户：李四");
                Console.WriteLine("[LogDb] 记录操作日志：创建新用户");
            }

            Console.WriteLine("跨数据库操作完成（注意：SQLite 不支持跨库分布式事务）");
        }
    }
}
