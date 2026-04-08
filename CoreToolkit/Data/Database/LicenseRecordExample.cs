using System;
using System.Linq;
using CoreToolkit.Common;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 授权码记录使用示例
    /// </summary>
    public class LicenseRecordExample
    {
        /// <summary>
        /// 演示完整的使用流程
        /// </summary>
        public static void Demo()
        {
            // 1. 初始化数据库上下文
            using (var dbContext = new SqliteDbContext("license.db"))
            {
                dbContext.InitDatabase();

                // 2. 创建仓储实例
                var repository = new LicenseRecordRepository(dbContext);

                // 3. 生成机器码信息
                var machineCodeInfo = LicenseSerializer.GenerateMachineCode("额外信息: 生产线A");
                string machineCodeJson = LicenseSerializer.SerializeMachineCode(machineCodeInfo);
                Console.WriteLine($"机器码 JSON: {machineCodeJson}");

                // 4. 创建授权记录
                var record = new LicenseRecord
                {
                    RecordTime = DateTime.Now,
                    Department = "技术部",
                    Operator = "张三",
                    ProjectNumber = "PRJ-2024-001",
                    DeviceNumber = "DEV-001",
                    MachineCode = machineCodeJson  // 序列化的机器码
                };

                // 5. 插入记录到数据库
                long recordId = repository.Insert(record);
                Console.WriteLine($"授权记录已创建，ID: {recordId}");

                // 6. 查询记录
                var retrievedRecord = repository.GetById(recordId);
                Console.WriteLine($"查询到记录: {retrievedRecord?.ProjectNumber}, {retrievedRecord?.DeviceNumber}");

                // 7. 反序列化存储的数据
                var retrievedMachineCode = LicenseSerializer.DeserializeMachineCode(retrievedRecord.MachineCode);

                Console.WriteLine($"机器信息: CPU={retrievedMachineCode?.CpuId}, MAC={retrievedMachineCode?.MacAddress}");

                // 8. 使用加密存储（可选）
                string encryptedMachineCode = LicenseSerializer.SerializeMachineCodeEncrypted(machineCodeInfo, "MySecretKey");

                var encryptedRecord = new LicenseRecord
                {
                    RecordTime = DateTime.Now,
                    Department = "销售部",
                    Operator = "李四",
                    ProjectNumber = "PRJ-2024-002",
                    DeviceNumber = "DEV-002",
                    MachineCode = encryptedMachineCode
                };
                repository.Insert(encryptedRecord);

                // 9. 解密读取
                var encryptedRetrieved = repository.GetByProjectNumber("PRJ-2024-002").FirstOrDefault();
                if (encryptedRetrieved != null)
                {
                    var decryptedMachineCode = LicenseSerializer.DeserializeMachineCodeEncrypted(encryptedRetrieved.MachineCode, "MySecretKey");
                    Console.WriteLine($"解密成功: {decryptedMachineCode?.MachineName}");
                }

                // 10. 查询统计信息
                var stats = repository.GetStatistics();
                Console.WriteLine($"统计: 总记录={stats.TotalCount}, 项目数={stats.ProjectCount}, 设备数={stats.DeviceCount}");

                // 11. 条件查询
                var searchResults = repository.SearchRecords(
                    department: "技术部",
                    startDate: DateTime.Now.AddDays(-7)
                );
                Console.WriteLine($"技术部近7天记录数: {searchResults.Count()}");

                // 12. 导出数据
                string jsonExport = repository.ExportToJson();
                Console.WriteLine($"导出 JSON 长度: {jsonExport.Length}");
            }
        }

        /// <summary>
        /// 演示授权验证流程
        /// </summary>
        public static void DemoLicenseValidation()
        {
            using (var dbContext = new SqliteDbContext("license.db"))
            {
                dbContext.InitDatabase();
                var repository = new LicenseRecordRepository(dbContext);

                // 模拟验证：根据机器码查找授权
                string targetMachineCode = "模拟机器码JSON字符串";
                
                var record = repository.GetByMachineCode(targetMachineCode);
                if (record != null)
                {
                    Console.WriteLine("找到该机器的授权记录");
                }
                else
                {
                    Console.WriteLine("未找到该机器的授权记录");
                }
            }
        }
    }
}
