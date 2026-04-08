using System;
using CoreToolkit.Tests;

namespace FileIndexTest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("请选择测试:");
            Console.WriteLine("1. FileIndexManagerV2 测试（实时监控+异步预热+状态管理）");
            Console.WriteLine("2. FileIndexManager 高性能测试");
            Console.WriteLine("3. 快速性能测试（普通版本）");
            Console.Write("选择 (1-3): ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "3":
                    QuickPerfTest.Run();
                    break;
                case "2":
                    HighPerfTest.Run();
                    break;
                case "1":
                default:
                    V2Test.Run();
                    break;
            }
            
            Console.WriteLine();
            Console.WriteLine("按Enter键退出...");
            Console.ReadLine();
        }
    }
}
