using System;
using System.Diagnostics;
using System.IO;
using CoreToolkit.Files.Helpers;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 对比测试：普通版本 vs 优化版本
    /// </summary>
    public class CompareTest
    {
        public static void Run()
        {
            string testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch_200k");
            
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexSearcher 对比测试");
            Console.WriteLine("    普通版本 vs 缓存优化版本");
            Console.WriteLine("==============================================");
            Console.WriteLine($"测试目录: {testDir}");
            Console.WriteLine();

            if (!Directory.Exists(testDir))
            {
                Console.WriteLine("错误: 测试目录不存在");
                return;
            }

            var normalSearcher = new FileIndexSearcher(testDir);
            var optimizedSearcher = new FileIndexSearcherOptimized(testDir);
            var sw = new Stopwatch();

            // 预热优化版本的缓存
            Console.WriteLine("[1] 预热优化版本缓存...");
            sw.Restart();
            optimizedSearcher.GetFileCount();
            sw.Stop();
            Console.WriteLine($"    缓存加载耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"    {optimizedSearcher.GetCacheInfo()}");
            Console.WriteLine();

            // 对比测试1: 单次搜索
            Console.WriteLine("[2] 单次搜索对比（首次）");
            Console.WriteLine("    索引: 'TUM126316D020'");
            
            sw.Restart();
            var r1 = normalSearcher.FindLatestFile("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    普通版本: {sw.ElapsedMilliseconds} ms - {(r1.IsSuccess ? "找到" : "未找到")}");
            
            sw.Restart();
            var r2 = optimizedSearcher.FindLatestFile("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    优化版本: {sw.ElapsedMilliseconds} ms - {(r2.IsSuccess ? "找到" : "未找到")}");
            Console.WriteLine();

            // 对比测试2: 多次搜索（测试缓存效果）
            Console.WriteLine("[3] 多次搜索对比（测试缓存效果）");
            string[] indexes = { "TUM126316D020", "RTLWOQAL33651018", "SZCCL", "TLPC" };
            
            sw.Restart();
            foreach (var idx in indexes)
            {
                normalSearcher.FindLatestFile(idx);
            }
            sw.Stop();
            Console.WriteLine($"    普通版本（4次）: {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            foreach (var idx in indexes)
            {
                optimizedSearcher.FindLatestFile(idx);
            }
            sw.Stop();
            Console.WriteLine($"    优化版本（4次）: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine();

            // 对比测试3: 查找所有匹配
            Console.WriteLine("[4] 查找所有匹配文件对比");
            
            sw.Restart();
            var a1 = normalSearcher.FindAllFiles("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    普通版本: {sw.ElapsedMilliseconds} ms - {(a1.IsSuccess ? $"找到{a1.Data.Count}个" : "未找到")}");
            
            sw.Restart();
            var a2 = optimizedSearcher.FindAllFiles("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    优化版本: {sw.ElapsedMilliseconds} ms - {(a2.IsSuccess ? $"找到{a2.Data.Count}个" : "未找到")}");
            Console.WriteLine();

            // 对比测试4: 文件统计
            Console.WriteLine("[5] 文件统计对比");
            
            sw.Restart();
            var c1 = normalSearcher.GetFileCount();
            sw.Stop();
            Console.WriteLine($"    普通版本: {sw.ElapsedMilliseconds} ms - {c1}个文件");
            
            sw.Restart();
            var c2 = optimizedSearcher.GetFileCount();
            sw.Stop();
            Console.WriteLine($"    优化版本: {sw.ElapsedMilliseconds} ms - {c2}个文件");
            Console.WriteLine();

            Console.WriteLine("==============================================");
            Console.WriteLine("对比测试完成！");
            Console.WriteLine("==============================================");
        }
    }
}
