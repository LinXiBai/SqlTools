using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreToolkit.Common.Models;
using CoreToolkit.Files.Helpers;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// FileIndexSearcher 直接测试程序 - 无需用户输入，直接运行完整测试
    /// </summary>
    public class FileIndexSearcherDirectTest
    {
        public static void Run()
        {
            string testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch_200k");
            var test = new FileIndexSearcherTester(testDir);
            test.RunTest(fileCount: 200000);
            
            Console.WriteLine();
            Console.WriteLine("测试完成！按任意键退出...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// FileIndexSearcher 测试器
    /// </summary>
    public class FileIndexSearcherTester
    {
        private readonly string _testDirectory;
        private readonly Random _random = new Random();

        public FileIndexSearcherTester(string testDirectory)
        {
            _testDirectory = testDirectory;
        }

        public void RunTest(int fileCount)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexSearcher 完整测试 (20万文件)");
            Console.WriteLine("==============================================\n");

            // 1. 生成测试数据
            Console.WriteLine($"[1] 生成 {fileCount:N0} 个测试文件...");
            Console.WriteLine($"    目标目录: {_testDirectory}");
            
            // 检查是否已有足够数据
            if (Directory.Exists(_testDirectory))
            {
                var existingFiles = Directory.GetFiles(_testDirectory, "*.txt");
                if (existingFiles.Length >= fileCount * 0.95)
                {
                    Console.WriteLine($"    检测到已有足够的测试数据 ({existingFiles.Length:N0} 个文件)，跳过生成步骤。");
                }
                else
                {
                    Console.WriteLine($"    现有文件数: {existingFiles.Length:N0}，需要重新生成...");
                    GenerateTestFiles(fileCount);
                }
            }
            else
            {
                GenerateTestFiles(fileCount);
            }

            // 2. 测试搜索功能
            Console.WriteLine("\n[2] 测试搜索功能...");
            TestSearch();

            // 3. 性能测试
            Console.WriteLine("\n[3] 性能测试...");
            TestPerformance();

            // 4. 高级功能测试
            Console.WriteLine("\n[4] 高级功能测试...");
            TestAdvancedFeatures();

            Console.WriteLine("\n==============================================");
            Console.WriteLine("    测试完成！");
            Console.WriteLine($"    测试数据保留在: {_testDirectory}");
            Console.WriteLine("==============================================");
        }

        private void GenerateTestFiles(int fileCount)
        {
            // 确保目录存在并清空
            if (Directory.Exists(_testDirectory))
            {
                Console.WriteLine("    清理旧数据...");
                Directory.Delete(_testDirectory, true);
            }
            Directory.CreateDirectory(_testDirectory);

            // 产品型号（参考截图中的命名格式）
            var productModels = new[]
            {
                "TUM126316D020", "RTLWOQAL33651018", "RTLWOQAL29651015", "TFB8226327B061",
                "TOSS126402C325", "TOSS126404C228", "TQRY126405C435", "TF8S126403C035",
                "TUZA126401D024", "TUZM126328B052", "TOSY126403C457", "TUZA126401D048",
                "RTLWOQAL33652010", "TOSI126404C228", "TF8S126403C035_TLPA", "TOSI126404C228_TLQB"
            };

            // 序列号前缀
            var serialPrefixes = new[] { "SZCCL", "TLPC", "TLQB", "TLPA", "TLQL", "TLCL", "TLCA" };

            var stopwatch = Stopwatch.StartNew();
            int reportInterval = fileCount / 20; // 每5%报告一次
            long totalBytes = 0;
            object lockObj = new object();
            int successCount = 0;
            int errorCount = 0;

            // 使用并行循环生成文件，但添加异常处理
            Parallel.For(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                try
                {
                    string productModel = productModels[_random.Next(productModels.Length)];
                    string serialPrefix = serialPrefixes[_random.Next(serialPrefixes.Length)];
                    string serialNumber = serialPrefix + _random.Next(10000000, 99999999);
                    
                    // 生成2026年1月到4月之间的随机日期
                    DateTime randomDate = new DateTime(2026, 1, 1).AddDays(_random.Next(0, 120)).AddHours(_random.Next(0, 24)).AddMinutes(_random.Next(0, 60));
                    string dateStamp = randomDate.ToString("yyyyMMdd_HHmmss");
                    
                    // 文件名格式：产品型号_序列号_时间戳_索引.txt（添加索引避免冲突）
                    string fileName = $"{productModel}_{serialNumber}_{dateStamp}_{i:D8}.txt";
                    string filePath = Path.Combine(_testDirectory, fileName);

                    // 生成约6KB的内容
                    string content = GenerateFileContent(6 * 1024, i);
                    
                    // 写入文件
                    File.WriteAllText(filePath, content);
                    
                    // 设置文件时间（使用try-catch防止冲突）
                    try
                    {
                        File.SetLastWriteTime(filePath, randomDate);
                    }
                    catch { /* 忽略时间设置错误 */ }

                    lock (lockObj)
                    {
                        totalBytes += content.Length * 2; // UTF-16编码
                        successCount++;
                    }

                    // 报告进度
                    if ((i + 1) % reportInterval == 0)
                    {
                        int progress = (i + 1) * 100 / fileCount;
                        Console.WriteLine($"    进度: {progress}% ({i + 1:N0}/{fileCount:N0})");
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        errorCount++;
                        if (errorCount <= 5) // 只显示前5个错误
                        {
                            Console.WriteLine($"    错误 [{i}]: {ex.Message}");
                        }
                    }
                }
            });

            stopwatch.Stop();

            // 验证生成的文件
            var generatedFiles = Directory.GetFiles(_testDirectory, "*.txt");
            Console.WriteLine($"\n    生成完成！");
            Console.WriteLine($"    文件数量: {generatedFiles.Length:N0} (成功: {successCount}, 失败: {errorCount})");
            Console.WriteLine($"    总大小: {totalBytes / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"    耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            if (generatedFiles.Length > 0)
            {
                Console.WriteLine($"    平均速度: {generatedFiles.Length / stopwatch.Elapsed.TotalSeconds:F0} 文件/秒");
            }
        }

        private string GenerateFileContent(int targetSize, int index)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("==============================================");
            sb.AppendLine("    产品测试数据文件");
            sb.AppendLine("==============================================");
            sb.AppendLine($"文件索引: {index:D8}");
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"文件大小: 约 {targetSize / 1024} KB");
            sb.AppendLine($"GUID: {Guid.NewGuid()}");
            sb.AppendLine();

            // 添加随机数据直到达到目标大小
            int lineCount = 0;
            while (sb.Length * 2 < targetSize)
            {
                sb.AppendLine($"DataLine_{lineCount++:D6}: {_random.NextDouble() * 1000000:F4}, {_random.NextDouble() * 1000000:F4}, {_random.NextDouble() * 1000000:F4}");
            }

            return sb.ToString();
        }

        private void TestSearch()
        {
            var searcher = new FileIndexSearcher(_testDirectory);

            // 测试1: 按产品型号搜索
            Console.WriteLine("  测试1: 按产品型号搜索 'TUM126316D020'");
            var result1 = searcher.FindLatestFile("TUM126316D020");
            PrintResult(result1);

            // 测试2: 按序列号前缀搜索
            Console.WriteLine("\n  测试2: 按序列号前缀搜索 'SZCCL'");
            var result2 = searcher.FindLatestFile("SZCCL");
            PrintResult(result2);

            // 测试3: 查找所有匹配文件
            Console.WriteLine("\n  测试3: 查找所有匹配 'RTLWOQAL33651018' 的文件");
            var result3 = searcher.FindAllFiles("RTLWOQAL33651018");
            if (result3.IsSuccess)
            {
                Console.WriteLine($"    ✓ 找到 {result3.Data.Count} 个文件");
                Console.WriteLine($"    最新: {Path.GetFileName(result3.Data[0].FullName)}");
                Console.WriteLine($"    修改时间: {result3.Data[0].LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                if (result3.Data.Count > 1)
                {
                    Console.WriteLine($"    最旧: {Path.GetFileName(result3.Data[result3.Data.Count - 1].FullName)}");
                    Console.WriteLine($"    修改时间: {result3.Data[result3.Data.Count - 1].LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                }
            }

            // 测试4: 多索引值搜索
            Console.WriteLine("\n  测试4: 多索引值搜索 'TUM126316D020' + 'SZCCL'");
            var result4 = searcher.FindLatestFileByMultipleIndexes("TUM126316D020", "SZCCL");
            PrintResult(result4);

            // 测试5: 获取所有索引值
            Console.WriteLine("\n  测试5: 获取所有产品型号");
            var result5 = searcher.GetAllIndexValues('_', 0);
            if (result5.IsSuccess)
            {
                Console.WriteLine($"    ✓ 找到 {result5.Data.Count} 个产品型号");
                Console.WriteLine($"    列表: {string.Join(", ", result5.Data.Take(10))}{(result5.Data.Count > 10 ? "..." : "")}");
            }
        }

        private void TestPerformance()
        {
            var searcher = new FileIndexSearcher(_testDirectory);
            var stopwatch = new Stopwatch();

            // 预热
            Console.WriteLine("  预热...");
            for (int i = 0; i < 10; i++)
            {
                searcher.FindLatestFile("TUM126316D020");
            }

            // 测试1: 单次搜索性能
            Console.WriteLine("\n  测试1: 单次搜索性能（100次平均）");
            stopwatch.Restart();
            int testCount = 100;
            for (int i = 0; i < testCount; i++)
            {
                searcher.FindLatestFile("TUM126316D020");
            }
            stopwatch.Stop();
            Console.WriteLine($"    总耗时: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"    平均每次: {stopwatch.Elapsed.TotalMilliseconds / testCount:F2} ms");

            // 测试2: 不同索引值的搜索
            Console.WriteLine("\n  测试2: 不同索引值搜索性能");
            var testIndexes = new[] { "TUM126316D020", "RTLWOQAL33651018", "SZCCL", "TLPC", "NOTEXIST" };
            foreach (var index in testIndexes)
            {
                stopwatch.Restart();
                var result = searcher.FindLatestFile(index);
                stopwatch.Stop();
                Console.WriteLine($"    搜索 '{index}': {stopwatch.Elapsed.TotalMilliseconds:F2} ms - {(result.IsSuccess ? "找到" : "未找到")}");
            }

            // 测试3: 异步搜索性能
            Console.WriteLine("\n  测试3: 异步搜索性能");
            stopwatch.Restart();
            var tasks = new List<Task<Result<string>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(searcher.FindLatestFileAsync("TUM126316D020"));
            }
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            Console.WriteLine($"    10次并发异步搜索总耗时: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

            // 测试4: 获取文件数量
            Console.WriteLine("\n  测试4: 目录统计");
            stopwatch.Restart();
            int fileCount = searcher.GetFileCount();
            stopwatch.Stop();
            Console.WriteLine($"    文件总数: {fileCount:N0}");
            Console.WriteLine($"    统计耗时: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
        }

        private void TestAdvancedFeatures()
        {
            var searcher = new FileIndexSearcher(_testDirectory);

            // 测试1: 正则表达式搜索
            Console.WriteLine("  测试1: 正则表达式搜索");
            var result1 = searcher.FindLatestFileByRegex(@"TUM\d+_SZCCL\d+");
            PrintResult(result1);

            // 测试2: 清理旧文件（演示，保留最新5个）
            Console.WriteLine("\n  测试2: 清理旧文件（保留最新5个）");
            var result2 = searcher.CleanupOldFiles("TUM126316D020", 5);
            if (result2.IsSuccess)
            {
                Console.WriteLine($"    ✓ 清理完成，删除了 {result2.Data} 个旧文件");
            }
            else
            {
                Console.WriteLine($"    ! {result2.Message}");
            }
        }

        private void PrintResult(Result<string> result)
        {
            if (result.IsSuccess)
            {
                Console.WriteLine($"    ✓ 找到: {Path.GetFileName(result.Data)}");
                var fileInfo = new FileInfo(result.Data);
                Console.WriteLine($"      大小: {fileInfo.Length / 1024} KB, 修改时间: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                Console.WriteLine($"    ✗ 未找到: {result.Message}");
            }
        }
    }
}
