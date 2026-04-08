using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CoreToolkit.Files.Helpers;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 快速性能测试
    /// </summary>
    public class QuickPerfTest
    {
        public static void Run()
        {
            string testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch_200k");
            
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexSearcher 快速性能测试");
            Console.WriteLine("==============================================");
            Console.WriteLine($"测试目录: {testDir}");
            Console.WriteLine();

            if (!Directory.Exists(testDir))
            {
                Console.WriteLine("错误: 测试目录不存在");
                return;
            }

            var searcher = new FileIndexSearcher(testDir);
            var sw = new Stopwatch();

            // 1. 文件统计
            Console.WriteLine("[1] 文件统计");
            sw.Restart();
            int count = searcher.GetFileCount();
            sw.Stop();
            Console.WriteLine($"    文件数: {count:N0}, 耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine();

            // 2. 单次搜索
            Console.WriteLine("[2] 单次搜索测试");
            string[] indexes = { "TUM126316D020", "RTLWOQAL33651018", "SZCCL", "TLPC", "NOTEXIST" };
            foreach (var idx in indexes)
            {
                sw.Restart();
                var result = searcher.FindLatestFile(idx);
                sw.Stop();
                Console.WriteLine($"    搜索 '{idx}': {sw.ElapsedMilliseconds} ms - {(result.IsSuccess ? "找到" : "未找到")}");
            }
            Console.WriteLine();

            // 3. 查找所有匹配
            Console.WriteLine("[3] 查找所有匹配文件");
            sw.Restart();
            var all = searcher.FindAllFiles("TUM126316D020");
            sw.Stop();
            if (all.IsSuccess)
            {
                Console.WriteLine($"    找到 {all.Data.Count} 个文件，耗时: {sw.ElapsedMilliseconds} ms");
            }
            Console.WriteLine();

            // 4. 异步搜索
            Console.WriteLine("[4] 异步搜索（5次并发）");
            sw.Restart();
            var tasks = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = searcher.FindLatestFileAsync("TUM126316D020");
            }
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine($"    5次并发异步搜索耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine();

            Console.WriteLine("==============================================");
            Console.WriteLine("测试完成！");
            Console.WriteLine("==============================================");
        }
    }
}
