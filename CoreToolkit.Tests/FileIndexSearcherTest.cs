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
    /// FileIndexSearcher 测试类
    /// </summary>
    public class FileIndexSearcherTest
    {
        private readonly string _testDirectory;
        private readonly Random _random = new Random();

        public FileIndexSearcherTest(string testDirectory = null)
        {
            _testDirectory = testDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch");
        }

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexSearcher 测试程序");
            Console.WriteLine("==============================================\n");

            // 1. 生成测试数据
            Console.WriteLine("[1] 生成测试数据...");
            GenerateTestFiles(200000, 6 * 1024); // 20万个文件，每个约6KB

            // 2. 基础搜索测试
            Console.WriteLine("\n[2] 基础搜索测试...");
            TestBasicSearch();

            // 3. 性能测试
            Console.WriteLine("\n[3] 性能测试...");
            TestPerformance();

            // 4. 高级功能测试
            Console.WriteLine("\n[4] 高级功能测试...");
            TestAdvancedFeatures();

            Console.WriteLine("\n==============================================");
            Console.WriteLine("    所有测试完成！");
            Console.WriteLine("==============================================");
        }

        /// <summary>
        /// 生成测试文件
        /// </summary>
        /// <param name="fileCount">文件数量</param>
        /// <param name="fileSize">文件大小（字节）</param>
        public void GenerateTestFiles(int fileCount, int fileSize)
        {
            Console.WriteLine($"测试目录: {_testDirectory}");
            Console.WriteLine($"计划生成: {fileCount:N0} 个文件，每个约 {fileSize / 1024} KB");
            Console.WriteLine($"预计总大小: {(fileCount * fileSize / 1024.0 / 1024.0):F2} MB");

            // 确保目录存在并清空
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            Directory.CreateDirectory(_testDirectory);

            // 生成一些常用的索引值（模拟产品型号）
            var productModels = new[]
            {
                "TUM126316D020", "TUM126316D021", "TUM126316D022",
                "RTLWOQAL33651018", "RTLWOQAL33651019", "RTLWOQAL33652010",
                "TFB8226327B061", "TFB8226327B062", "TFB8226327B063",
                "TOSS126402C325", "TOSS126402C326", "TOSS126402C327",
                "TQRY126405C435", "TQRY126405C436", "TQRY126405C437",
                "TF8S126403C035", "TF8S126403C036", "TF8S126403C037",
                "TUZA126401D024", "TUZA126401D025", "TUZA126401D026",
                "TUZM126328B052", "TUZM126328B053", "TUZM126328B054",
                "TOSY126403C457", "TOSY126403C458", "TOSY126403C459",
                "TOSI126404C228", "TOSI126404C229", "TOSI126404C230"
            };

            // 生成序列号前缀
            var serialPrefixes = new[] { "SZCCL", "TLPC", "TLQB", "TLPA", "TLQL", "TLCL", "TLCA" };

            var stopwatch = Stopwatch.StartNew();
            int reportInterval = fileCount / 10; // 每10%报告一次

            // 预生成文件内容模板
            string contentTemplate = GenerateFileContent(fileSize);

            Parallel.For(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                // 随机选择产品型号和序列号
                string productModel = productModels[_random.Next(productModels.Length)];
                string serialPrefix = serialPrefixes[_random.Next(serialPrefixes.Length)];
                string serialNumber = serialPrefix + _random.Next(10000000, 99999999);
                
                // 生成日期时间戳（2026年1月到4月之间）
                DateTime randomDate = new DateTime(2026, 1, 1).AddDays(_random.Next(0, 120)).AddHours(_random.Next(0, 24)).AddMinutes(_random.Next(0, 60));
                string dateStamp = randomDate.ToString("yyyyMMdd_HHmmss");
                
                // 文件名格式：产品型号_序列号_时间戳.txt
                string fileName = $"{productModel}_{serialNumber}_{dateStamp}.txt";
                string filePath = Path.Combine(_testDirectory, fileName);

                // 写入文件内容
                File.WriteAllText(filePath, contentTemplate);

                // 设置文件的最后写入时间（用于测试时间排序）
                File.SetLastWriteTime(filePath, randomDate);

                // 报告进度
                if ((i + 1) % reportInterval == 0)
                {
                    int progress = (i + 1) * 100 / fileCount;
                    Console.WriteLine($"  进度: {progress}% ({i + 1:N0}/{fileCount:N0})");
                }
            });

            stopwatch.Stop();

            // 验证生成的文件
            var generatedFiles = Directory.GetFiles(_testDirectory, "*.txt");
            long totalSize = generatedFiles.Sum(f => new FileInfo(f).Length);

            Console.WriteLine($"\n生成完成！");
            Console.WriteLine($"  实际生成: {generatedFiles.Length:N0} 个文件");
            Console.WriteLine($"  总大小: {totalSize / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"  耗时: {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.WriteLine($"  平均速度: {generatedFiles.Length / stopwatch.Elapsed.TotalSeconds:F0} 文件/秒");
        }

        /// <summary>
        /// 生成文件内容
        /// </summary>
        private string GenerateFileContent(int targetSize)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("==============================================");
            sb.AppendLine("    产品测试数据文件");
            sb.AppendLine("==============================================");
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"文件大小: 约 {targetSize / 1024} KB");
            sb.AppendLine();

            // 添加一些随机数据直到达到目标大小
            while (sb.Length * 2 < targetSize) // UTF-16编码，每个字符2字节
            {
                sb.AppendLine($"DataLine_{Guid.NewGuid()}: {_random.NextDouble() * 1000000:F4}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 基础搜索测试
        /// </summary>
        private void TestBasicSearch()
        {
            var searcher = new FileIndexSearcher(_testDirectory);

            // 测试1: 搜索产品型号
            Console.WriteLine("  测试1: 按产品型号搜索");
            var result1 = searcher.FindLatestFile("TUM126316D020");
            PrintResult("  搜索 'TUM126316D020'", result1);

            // 测试2: 搜索序列号
            Console.WriteLine("\n  测试2: 按序列号前缀搜索");
            var result2 = searcher.FindLatestFile("SZCCL");
            PrintResult("  搜索 'SZCCL'", result2);

            // 测试3: 搜索不存在的索引
            Console.WriteLine("\n  测试3: 搜索不存在的索引");
            var result3 = searcher.FindLatestFile("NOTEXIST");
            PrintResult("  搜索 'NOTEXIST'", result3);

            // 测试4: 查找所有匹配文件
            Console.WriteLine("\n  测试4: 查找所有匹配文件");
            var result4 = searcher.FindAllFiles("TUM126316D020");
            if (result4.IsSuccess)
            {
                Console.WriteLine($"    ✓ 找到 {result4.Data.Count} 个文件");
                Console.WriteLine($"    最新文件: {Path.GetFileName(result4.Data[0].FullName)}");
                Console.WriteLine($"    修改时间: {result4.Data[0].LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                Console.WriteLine($"    ✗ 失败: {result4.Message}");
            }
        }

        /// <summary>
        /// 性能测试
        /// </summary>
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
            var tasks = new List<Task>();
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

        /// <summary>
        /// 高级功能测试
        /// </summary>
        private void TestAdvancedFeatures()
        {
            var searcher = new FileIndexSearcher(_testDirectory);

            // 测试1: 多索引值搜索（AND条件）
            Console.WriteLine("  测试1: 多索引值搜索（AND条件）");
            var result1 = searcher.FindLatestFileByMultipleIndexes("TUM126316D020", "SZCCL");
            PrintResult("  搜索同时包含 'TUM126316D020' 和 'SZCCL'", result1);

            // 测试2: 正则表达式搜索
            Console.WriteLine("\n  测试2: 正则表达式搜索");
            var result2 = searcher.FindLatestFileByRegex(@"TUM\d+_SZCCL\d+");
            PrintResult("  搜索匹配 'TUM数字_SZCCL数字' 的文件", result2);

            // 测试3: 获取所有索引值
            Console.WriteLine("\n  测试3: 获取所有产品型号");
            var result3 = searcher.GetAllIndexValues('_', 0);
            if (result3.IsSuccess)
            {
                Console.WriteLine($"    ✓ 找到 {result3.Data.Count} 个不同的产品型号");
                Console.WriteLine($"    前5个: {string.Join(", ", result3.Data.Take(5))}");
            }
            else
            {
                Console.WriteLine($"    ✗ 失败: {result3.Message}");
            }

            // 测试4: 清理旧文件
            Console.WriteLine("\n  测试4: 清理旧文件（保留最新3个）");
            var result4 = searcher.CleanupOldFiles("TUM126316D020", 3);
            if (result4.IsSuccess)
            {
                Console.WriteLine($"    ✓ 清理完成，删除了 {result4.Data} 个旧文件");
            }
            else
            {
                Console.WriteLine($"    ✗ 失败: {result4.Message}");
            }
        }

        /// <summary>
        /// 打印结果
        /// </summary>
        private void PrintResult(string description, Result<string> result)
        {
            Console.WriteLine($"{description}:");
            if (result.IsSuccess)
            {
                Console.WriteLine($"    ✓ 成功: {Path.GetFileName(result.Data)}");
                Console.WriteLine($"    路径: {result.Data}");
                if (!string.IsNullOrEmpty(result.Message))
                    Console.WriteLine($"    消息: {result.Message}");
            }
            else
            {
                Console.WriteLine($"    ✗ 失败: {result.Message}");
            }
        }
    }
}
