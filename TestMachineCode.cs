using System;
using System.IO;
using CoreToolkit.Data;
using CoreToolkit.Common;

namespace TestMachineCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试机器码存储和导出功能");
            Console.WriteLine("============================");

            // 测试原始机器码字符串
            string originalMachineCode = "T8N0CX02J84035BBFEBFBFF000C0652";
            Console.WriteLine($"原始机器码: {originalMachineCode}");

            // 测试1: 直接存储和读取
            TestDirectStorage(originalMachineCode);

            // 测试2: 模拟拖入文件的情况
            TestFileDropScenario(originalMachineCode);

            // 测试3: 测试序列化和反序列化
            TestSerialization(originalMachineCode);

            Console.WriteLine("\n测试完成，按任意键退出...");
            Console.ReadKey();
        }

        static void TestDirectStorage(string originalMachineCode)
        {
            Console.WriteLine("\n测试1: 直接存储和读取");
            Console.WriteLine("------------------------");

            try
            {
                // 创建临时数据库
                string dbPath = Path.Combine(Environment.CurrentDirectory, "test_license.db");
                if (File.Exists(dbPath))
                    File.Delete(dbPath);

                var dbContext = new SqliteDbContext(dbPath);
                dbContext.InitDatabase();
                var repository = new LicenseRecordRepository(dbContext);

                // 创建记录
                var record = new LicenseRecord
                {
                    RecordTime = DateTime.Now,
                    Department = "技术部",
                    Operator = "测试用户",
                    Applicant = "申请人张三",
                    ProjectNumber = "TEST-20260408",
                    DeviceNumber = "高速贴片机-TEST001",
                    MachineCode = originalMachineCode
                };

                long id = repository.Insert(record);
                Console.WriteLine($"记录已插入，ID: {id}");

                // 读取记录
                var retrievedRecord = repository.GetById(id);
                if (retrievedRecord != null)
                {
                    Console.WriteLine($"读取的机器码: {retrievedRecord.MachineCode}");
                    Console.WriteLine($"是否与原始值相同: {retrievedRecord.MachineCode == originalMachineCode}");
                }
                else
                {
                    Console.WriteLine("无法读取记录");
                }

                // 清理
                dbContext.Dispose();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);

            } catch (Exception ex)
            {
                Console.WriteLine($"测试1失败: {ex.Message}");
            }
        }

        static void TestFileDropScenario(string originalMachineCode)
        {
            Console.WriteLine("\n测试2: 模拟拖入文件的情况");
            Console.WriteLine("----------------------------");

            try
            {
                // 创建临时机器码文件
                string tempFile = Path.Combine(Environment.CurrentDirectory, "test_machine_code.txt");
                File.WriteAllText(tempFile, originalMachineCode);
                Console.WriteLine($"创建临时文件: {tempFile}");
                Console.WriteLine($"文件内容: {File.ReadAllText(tempFile)}");

                // 模拟拖入文件后的处理
                string machineCodeContent = tempFile;
                if (File.Exists(machineCodeContent))
                {
                    machineCodeContent = File.ReadAllText(machineCodeContent);
                    Console.WriteLine($"读取文件后的内容: {machineCodeContent}");
                    Console.WriteLine($"是否与原始值相同: {machineCodeContent == originalMachineCode}");
                }

                // 清理
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

            } catch (Exception ex)
            {
                Console.WriteLine($"测试2失败: {ex.Message}");
            }
        }

        static void TestSerialization(string originalMachineCode)
        {
            Console.WriteLine("\n测试3: 测试序列化和反序列化");
            Console.WriteLine("------------------------------");

            try
            {
                // 测试将原始机器码作为ExtraInfo序列化
                var machineCodeInfo = new MachineCodeInfo
                {
                    MachineName = "TestMachine",
                    OsVersion = "Windows 10",
                    ExtraInfo = originalMachineCode,
                    GeneratedAt = DateTime.Now
                };

                string serialized = LicenseSerializer.SerializeMachineCode(machineCodeInfo);
                Console.WriteLine($"序列化后: {serialized}");

                var deserialized = LicenseSerializer.DeserializeMachineCode(serialized);
                if (deserialized != null)
                {
                    Console.WriteLine($"反序列化后ExtraInfo: {deserialized.ExtraInfo}");
                    Console.WriteLine($"是否与原始值相同: {deserialized.ExtraInfo == originalMachineCode}");
                }
                else
                {
                    Console.WriteLine("反序列化失败");
                }

            } catch (Exception ex)
            {
                Console.WriteLine($"测试3失败: {ex.Message}");
            }
        }
    }
}