using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Files.Helpers;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// FileIndexManagerV2 测试
    /// </summary>
    public class V2Test
    {
        public static void Run()
        {
            string testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "FileIndexSearch_200k");
            
            Console.WriteLine("==============================================");
            Console.WriteLine("    FileIndexManagerV2 测试");
            Console.WriteLine("    实时监控 + 异步预热 + 状态管理");
            Console.WriteLine("==============================================");
            Console.WriteLine($"测试目录: {testDir}");
            Console.WriteLine();

            if (!Directory.Exists(testDir))
            {
                Console.WriteLine("错误: 测试目录不存在");
                return;
            }

            // 测试1: 自动预热
            Console.WriteLine("[1] 创建管理器（自动预热）");
            var manager = new FileIndexManagerV2(testDir, autoWarmup: true);
            Console.WriteLine($"    初始状态: {manager.StatusDescription}");
            
            // 等待预热完成
            Console.WriteLine("    等待预热完成...");
            var sw = Stopwatch.StartNew();
            bool ready = manager.WaitForReady(TimeSpan.FromSeconds(60));
            sw.Stop();
            Console.WriteLine($"    预热完成: {ready}, 耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"    当前状态: {manager.StatusDescription}");
            Console.WriteLine($"    {manager.GetIndexInfo()}");
            Console.WriteLine();

            // 测试2: 预热中搜索（应该返回错误）
            Console.WriteLine("[2] 测试状态检查");
            Console.WriteLine($"    IsReady: {manager.IsReady}");
            Console.WriteLine($"    IsWarmingUp: {manager.IsWarmingUp}");
            
            // 搜索测试
            var result = manager.FindLatestByProductModel("TUM126316D020");
            Console.WriteLine($"    搜索结果: {(result.IsSuccess ? "成功" : "失败")} - {result.Message}");
            Console.WriteLine();

            // 测试3: 性能测试
            Console.WriteLine("[3] 性能测试");
            string[] testCases = { "TUM126316D020", "RTLWOQAL33651018", "SZCCL", "TLPC" };
            foreach (var tc in testCases)
            {
                sw.Restart();
                var r = manager.FindLatestByProductModel(tc);
                sw.Stop();
                Console.WriteLine($"    '{tc}': {sw.ElapsedTicks / 10.0:F2} μs - {(r.IsSuccess ? "找到" : "未找到")}");
            }
            Console.WriteLine();

            // 测试4: 压力测试
            Console.WriteLine("[4] 压力测试（1000次）");
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                manager.FindLatestByProductModel("TUM126316D020");
            }
            sw.Stop();
            Console.WriteLine($"    1000次搜索: {sw.ElapsedMilliseconds} ms, 平均: {sw.ElapsedTicks / 10000.0:F2} μs/次");
            Console.WriteLine();

            // 测试5: 手动刷新
            Console.WriteLine("[5] 手动刷新测试");
            Console.WriteLine($"    刷新前: {manager.GetIndexInfo()}");
            var refreshTask = manager.StartWarmupAsync();
            Console.WriteLine($"    刷新状态: {manager.StatusDescription}");
            refreshTask.Wait();
            Console.WriteLine($"    刷新后: {manager.GetIndexInfo()}");
            Console.WriteLine();

            // 测试6: 检查是否需要刷新
            Console.WriteLine("[6] 检查文件夹修改");
            bool needsRefresh = manager.NeedsRefresh();
            Console.WriteLine($"    是否需要刷新: {needsRefresh}");
            Console.WriteLine();

            Console.WriteLine("==============================================");
            Console.WriteLine("测试完成！");
            Console.WriteLine("==============================================");

            manager.Dispose();
        }
    }
}
