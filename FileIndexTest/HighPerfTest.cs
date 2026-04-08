using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreToolkit.Files.Helpers;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 高性能索引管理器测试
    /// </summary>
    public class HighPerfTest
    {
        public static void Run()
        {
            string testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch_200k");
            
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexManager 高性能测试");
            Console.WriteLine("==============================================");
            Console.WriteLine($"测试目录: {testDir}");
            Console.WriteLine();

            if (!Directory.Exists(testDir))
            {
                Console.WriteLine("错误: 测试目录不存在");
                return;
            }

            // 创建索引管理器
            Console.WriteLine("[1] 构建索引...");
            var manager = new FileIndexManager(testDir);
            var sw = Stopwatch.StartNew();
            manager.BuildIndex();
            sw.Stop();
            Console.WriteLine($"    索引构建耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"    {manager.GetIndexInfo()}");
            Console.WriteLine();

            // 测试1: 按产品型号搜索（毫秒级）
            Console.WriteLine("[2] 按产品型号搜索（字典索引）");
            string[] productModels = { "TUM126316D020", "RTLWOQAL33651018", "TFB8226327B061", "NOTEXIST" };
            foreach (var pm in productModels)
            {
                sw.Restart();
                var result = manager.FindLatestByProductModel(pm);
                sw.Stop();
                Console.WriteLine($"    '{pm}': {sw.ElapsedTicks / 10.0:F2} μs - {(result.IsSuccess ? $"找到 ({result.Message})" : "未找到")}");
            }
            Console.WriteLine();

            // 测试2: 按序列号前缀搜索（毫秒级）
            Console.WriteLine("[3] 按序列号前缀搜索（字典索引）");
            string[] serialPrefixes = { "SZCCL", "TLPC", "TLQB", "NOTEXIST" };
            foreach (var sp in serialPrefixes)
            {
                sw.Restart();
                var result = manager.FindLatestBySerialPrefix(sp);
                sw.Stop();
                Console.WriteLine($"    '{sp}': {sw.ElapsedTicks / 10.0:F2} μs - {(result.IsSuccess ? $"找到 ({result.Message})" : "未找到")}");
            }
            Console.WriteLine();

            // 测试3: 通用搜索（兼容模式）
            Console.WriteLine("[4] 通用搜索（兼容模式）");
            string[] indexes = { "TUM126316D020", "SZCCL", "RTLWOQAL33651018", "NOTEXIST" };
            foreach (var idx in indexes)
            {
                sw.Restart();
                var result = manager.FindLatestFile(idx);
                sw.Stop();
                Console.WriteLine($"    '{idx}': {sw.ElapsedMilliseconds} ms - {(result.IsSuccess ? "找到" : "未找到")}");
            }
            Console.WriteLine();

            // 测试4: 查找所有匹配
            Console.WriteLine("[5] 查找所有匹配文件");
            sw.Restart();
            var allResult = manager.FindAllByProductModel("TUM126316D020");
            sw.Stop();
            if (allResult.IsSuccess)
            {
                Console.WriteLine($"    找到 {allResult.Data.Count} 个文件，耗时: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"    最新: {Path.GetFileName(allResult.Data[0].FullPath)}");
                Console.WriteLine($"    最旧: {Path.GetFileName(allResult.Data[allResult.Data.Count - 1].FullPath)}");
            }
            Console.WriteLine();

            // 测试5: 获取所有索引值
            Console.WriteLine("[6] 获取所有索引值");
            sw.Restart();
            var productModelList = manager.GetAllProductModels();
            sw.Stop();
            Console.WriteLine($"    产品型号数: {productModelList.Count}, 耗时: {sw.ElapsedTicks / 10.0:F2} μs");
            Console.WriteLine($"    前10个: {string.Join(", ", productModelList.Take(10))}");
            Console.WriteLine();

            sw.Restart();
            var serialPrefixList = manager.GetAllSerialPrefixes();
            sw.Stop();
            Console.WriteLine($"    序列号前缀数: {serialPrefixList.Count}, 耗时: {sw.ElapsedTicks / 10.0:F2} μs");
            Console.WriteLine($"    列表: {string.Join(", ", serialPrefixList)}");
            Console.WriteLine();

            // 测试6: 1000次搜索压力测试
            Console.WriteLine("[7] 压力测试（1000次搜索）");
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                manager.FindLatestByProductModel("TUM126316D020");
            }
            sw.Stop();
            Console.WriteLine($"    1000次搜索总耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"    平均每次: {sw.ElapsedTicks / 10000.0:F2} μs");
            Console.WriteLine();

            // 测试7: 与普通版本对比
            Console.WriteLine("[8] 与普通版本对比");
            var normalSearcher = new FileIndexSearcher(testDir);
            
            sw.Restart();
            var r1 = normalSearcher.FindLatestFile("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    普通版本: {sw.ElapsedMilliseconds} ms - {(r1.IsSuccess ? "找到" : "未找到")}");
            
            sw.Restart();
            var r2 = manager.FindLatestByProductModel("TUM126316D020");
            sw.Stop();
            Console.WriteLine($"    优化版本: {sw.ElapsedTicks / 10.0:F2} μs - {(r2.IsSuccess ? "找到" : "未找到")}");
            
            double speedup = sw.ElapsedMilliseconds * 1000.0 / (sw.ElapsedTicks / 10.0);
            Console.WriteLine($"    速度提升: 约 {speedup:F0} 倍");
            Console.WriteLine();

            Console.WriteLine("==============================================");
            Console.WriteLine("测试完成！");
            Console.WriteLine("==============================================");

            manager.Dispose();
        }
    }
}
