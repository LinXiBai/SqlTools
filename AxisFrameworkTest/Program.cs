using System;
using System.Threading;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;
using CoreToolkit.Motion.Providers.Advantech;

namespace AxisFrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    轴框架重构测试程序");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            TestPCI1203();
            Console.WriteLine();
            TestPCI1285();

            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static void TestPCI1203()
        {
            Console.WriteLine("【测试 PCI-1203 控制卡】");
            Console.WriteLine("-----------------------------------------------");

            try
            {
                // 创建 PCI-1203 实例（16轴）
                var pci1203 = new PCI1203();
                Console.WriteLine($"✓ 创建 PCI-1203 控制卡实例");
                Console.WriteLine($"  卡名称: {pci1203.CardName}");
                Console.WriteLine($"  厂商: {pci1203.Vendor}");
                Console.WriteLine($"  型号: {pci1203.Model}");
                Console.WriteLine($"  轴数: {pci1203.AxisCount}");

                // 创建配置
                var config = new MotionConfig
                {
                    CardId = 0,
                    AxisConfigs = new System.Collections.Generic.List<AxisConfig>(),
                    InputCount = 16,
                    OutputCount = 16
                };

                // 测试基本属性
                Console.WriteLine($"✓ 测试基本属性");
                Console.WriteLine($"  已初始化: {pci1203.IsInitialized}");
                Console.WriteLine($"  已打开: {pci1203.IsOpen}");

                Console.WriteLine("✓ PCI-1203 测试完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 测试失败: {ex.Message}");
            }
        }

        static void TestPCI1285()
        {
            Console.WriteLine("【测试 PCI-1285 控制卡】");
            Console.WriteLine("-----------------------------------------------");

            try
            {
                // 创建 PCI-1285 实例（8轴）
                var pci1285 = new PCI1285();
                Console.WriteLine($"✓ 创建 PCI-1285 控制卡实例");
                Console.WriteLine($"  卡名称: {pci1285.CardName}");
                Console.WriteLine($"  厂商: {pci1285.Vendor}");
                Console.WriteLine($"  型号: {pci1285.Model}");
                Console.WriteLine($"  轴数: {pci1285.AxisCount}");

                // 创建配置
                var config = new MotionConfig
                {
                    CardId = 0,
                    AxisConfigs = new System.Collections.Generic.List<AxisConfig>(),
                    InputCount = 16,
                    OutputCount = 16
                };

                // 测试基本属性
                Console.WriteLine($"✓ 测试基本属性");
                Console.WriteLine($"  已初始化: {pci1285.IsInitialized}");
                Console.WriteLine($"  已打开: {pci1285.IsOpen}");

                // 测试轴组管理器
                Console.WriteLine($"✓ 测试轴组管理器");
                var groupManager = pci1285.GroupManager;
                string groupStatus = groupManager != null ? "可用" : "不可用";
                Console.WriteLine($"  轴组管理器: {groupStatus}");

                Console.WriteLine("✓ PCI-1285 测试完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 测试失败: {ex.Message}");
            }
        }
    }
}
