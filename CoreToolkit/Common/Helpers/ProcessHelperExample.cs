using System;
using System.Diagnostics;

namespace CoreToolkit.Common
{
    /// <summary>
    /// ProcessHelper 使用示例
    /// </summary>
    public class ProcessHelperExample
    {
        /// <summary>
        /// 演示完整的使用流程
        /// </summary>
        public static void Demo()
        {
            Console.WriteLine("=== ProcessHelper 使用示例 ===\n");

            // 1. 检查可执行文件是否存在
            Console.WriteLine("1. 检查文件是否存在");
            bool exists = ProcessHelper.ExecutableExists("notepad.exe");
            Console.WriteLine($"   notepad.exe 存在: {exists}");

            exists = ProcessHelper.ExecutableExists("Tools\\myapp.exe");
            Console.WriteLine($"   Tools\\myapp.exe 存在: {exists}");

            // 2. 解析路径
            Console.WriteLine("\n2. 解析可执行文件路径");
            string resolvedPath = ProcessHelper.ResolveExecutablePath("notepad.exe");
            Console.WriteLine($"   notepad.exe 解析结果: {resolvedPath}");

            // 3. 启动外部程序（不等待）
            Console.WriteLine("\n3. 启动记事本（不等待）");
            var startResult = ProcessHelper.Start("notepad.exe");
            if (startResult)
            {
                Console.WriteLine($"   启动成功，进程ID: {startResult.ProcessId}");
                // 稍等后关闭
                System.Threading.Thread.Sleep(1000);
                ProcessHelper.KillProcess(startResult.ProcessId);
            }
            else
            {
                Console.WriteLine($"   启动失败: {startResult.ErrorMessage}");
            }

            // 4. 启动并等待退出（带参数）
            Console.WriteLine("\n4. 执行命令并捕获输出");
            var execResult = ProcessHelper.Execute(
                executablePath: "cmd.exe",
                arguments: "/c echo Hello World && echo Error Test >&2",
                captureOutput: true,
                timeout: 5000
            );

            if (execResult)
            {
                Console.WriteLine($"   执行成功，退出码: {execResult.ExitCode}");
                Console.WriteLine($"   耗时: {execResult.ElapsedMilliseconds}ms");
                Console.WriteLine($"   标准输出: {execResult.StandardOutput?.Trim()}");
                Console.WriteLine($"   标准错误: {execResult.StandardError?.Trim()}");
            }
            else
            {
                Console.WriteLine($"   执行失败: {execResult.ErrorMessage}");
            }

            // 5. 使用相对路径启动
            Console.WriteLine("\n5. 使用相对路径启动");
            // 假设程序目录下有 Tools\helper.exe
            var relativeResult = ProcessHelper.Start(
                executablePath: @"Tools\helper.exe",
                arguments: "--help",
                baseDirectory: AppDomain.CurrentDomain.BaseDirectory
            );

            if (!relativeResult)
            {
                Console.WriteLine($"   相对路径启动失败（预期）: {relativeResult.ErrorMessage}");
            }

            // 6. 打开文件（使用默认程序）
            Console.WriteLine("\n6. 打开文件");
            var fileResult = ProcessHelper.OpenFile("README.md");
            if (fileResult)
            {
                Console.WriteLine($"   文件打开成功，进程ID: {fileResult.ProcessId}");
            }
            else
            {
                Console.WriteLine($"   文件打开失败: {fileResult.ErrorMessage}");
            }

            // 7. 打开 URL
            Console.WriteLine("\n7. 打开 URL");
            var urlResult = ProcessHelper.OpenUrl("www.baidu.com");
            if (urlResult)
            {
                Console.WriteLine($"   URL 打开成功，进程ID: {urlResult.ProcessId}");
            }
            else
            {
                Console.WriteLine($"   URL 打开失败: {urlResult.ErrorMessage}");
            }

            // 8. 打开文件夹
            Console.WriteLine("\n8. 打开文件夹");
            var folderResult = ProcessHelper.OpenFolder(@".\Logs");
            if (folderResult)
            {
                Console.WriteLine($"   文件夹打开成功，进程ID: {folderResult.ProcessId}");
            }
            else
            {
                Console.WriteLine($"   文件夹打开失败: {folderResult.ErrorMessage}");
            }

            // 9. 进程管理
            Console.WriteLine("\n9. 进程管理");
            bool isRunning = ProcessHelper.IsProcessRunning("notepad");
            Console.WriteLine($"   记事本是否运行: {isRunning}");

            // 10. 异步执行
            Console.WriteLine("\n10. 异步执行");
            DemoAsync().Wait();
        }

        /// <summary>
        /// 异步演示
        /// </summary>
        private static async System.Threading.Tasks.Task DemoAsync()
        {
            var result = await ProcessHelper.ExecuteAsync(
                executablePath: "cmd.exe",
                arguments: "/c dir /b",
                captureOutput: true,
                timeout: 5000
            );

            if (result)
            {
                Console.WriteLine($"   异步执行成功，输出前3行:");
                var lines = result.StandardOutput?.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < System.Math.Min(3, lines?.Length ?? 0); i++)
                {
                    Console.WriteLine($"     {lines[i]}");
                }
            }
        }

        /// <summary>
        /// 实际应用场景示例
        /// </summary>
        public static void RealWorldExamples()
        {
            Console.WriteLine("\n=== 实际应用场景 ===\n");

            // 场景1: 启动配置工具
            Console.WriteLine("场景1: 启动配置工具");
            var configTool = ProcessHelper.Start(@"Tools\ConfigTool.exe");
            if (!configTool)
            {
                Console.WriteLine($"   配置工具启动失败: {configTool.ErrorMessage}");
            }

            // 场景2: 执行批处理脚本
            Console.WriteLine("\n场景2: 执行批处理脚本");
            var batchResult = ProcessHelper.Execute(
                @"Scripts\backup.bat",
                workingDirectory: @"C:\MyApp\Scripts",
                timeout: 60000,
                captureOutput: true
            );

            if (batchResult)
            {
                Console.WriteLine($"   脚本执行完成，退出码: {batchResult.ExitCode}");
                if (!string.IsNullOrEmpty(batchResult.StandardOutput))
                {
                    Console.WriteLine($"   输出: {batchResult.StandardOutput}");
                }
            }

            // 场景3: 调用外部程序处理数据
            Console.WriteLine("\n场景3: 调用外部程序处理数据");
            var processResult = ProcessHelper.Execute(
                @"External\DataProcessor.exe",
                arguments: @"--input ""data\input.csv"" --output ""data\output.json""",
                workingDirectory: AppDomain.CurrentDomain.BaseDirectory,
                timeout: 300000,  // 5分钟超时
                captureOutput: true
            );

            if (processResult && processResult.ExitCode == 0)
            {
                Console.WriteLine("   数据处理成功");
            }
            else
            {
                Console.WriteLine($"   数据处理失败: {processResult.ErrorMessage ?? processResult.StandardError}");
            }

            // 场景4: 打开帮助文档
            Console.WriteLine("\n场景4: 打开帮助文档");
            var helpResult = ProcessHelper.OpenFile(@"Docs\Manual.pdf");
            if (!helpResult)
            {
                // 如果本地文档不存在，打开在线文档
                ProcessHelper.OpenUrl("https://docs.example.com/help");
            }

            // 场景5: 打开日志文件夹
            Console.WriteLine("\n场景5: 打开日志文件夹");
            ProcessHelper.OpenFolder(@"Logs");
        }
    }
}
