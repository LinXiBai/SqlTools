using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreToolkit.Data;
using CoreToolkit.StateMachine.Core;
using CoreToolkit.StateMachine.Modules;
using CoreToolkit.StateMachine.Models;
using Xunit;

namespace CoreToolkit.UnitTests
{
    /// <summary>
    /// 状态机持久化功能单元测试
    /// </summary>
    public class StateMachinePersistenceTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqliteDbContext _dbContext;
        private readonly StateMachineRecordRepository _repository;

        public StateMachinePersistenceTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"StateMachineTest_{Guid.NewGuid():N}.db");
            _dbContext = new SqliteDbContext(_dbPath);
            _dbContext.InitDatabase();
            _repository = new StateMachineRecordRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            try
            {
                if (File.Exists(_dbPath))
                    File.Delete(_dbPath);
            }
            catch
            {
                // 忽略清理错误
            }
        }

        [Fact]
        public void StateMachineRecord_InsertAndQuery_ShouldWork()
        {
            // Arrange
            var record = new StateMachineRecord
            {
                MachineName = "TestMachine",
                Description = "测试状态机",
                Status = "Completed",
                StartTime = DateTime.Now.AddMinutes(-1),
                EndTime = DateTime.Now,
                DurationMs = 5000,
                IsSuccess = true,
                ErrorMessage = null,
                Exception = null,
                ModuleStatsJson = "[{\"ModuleId\":\"m1\",\"ModuleName\":\"Step1\",\"IsSuccess\":true}]",
                ContextId = Guid.NewGuid().ToString("N"),
                ModuleCount = 3
            };

            // Act
            long id = _repository.Insert(record);
            var queried = _repository.GetById(id);

            // Assert
            Assert.True(id > 0);
            Assert.NotNull(queried);
            Assert.Equal("TestMachine", queried.MachineName);
            Assert.Equal("Completed", queried.Status);
            Assert.True(queried.IsSuccess);
            Assert.Equal(3, queried.ModuleCount);
        }

        [Fact]
        public void StateMachineRecord_QueryByMachineName_ShouldReturnCorrectRecords()
        {
            // Arrange
            _repository.Insert(new StateMachineRecord { MachineName = "MachineA", Status = "Completed", StartTime = DateTime.Now });
            _repository.Insert(new StateMachineRecord { MachineName = "MachineA", Status = "Error", StartTime = DateTime.Now });
            _repository.Insert(new StateMachineRecord { MachineName = "MachineB", Status = "Completed", StartTime = DateTime.Now });

            // Act
            var records = _repository.GetByMachineName("MachineA").ToList();

            // Assert
            Assert.Equal(2, records.Count);
            Assert.All(records, r => Assert.Equal("MachineA", r.MachineName));
        }

        [Fact]
        public void StateMachineRecord_QueryByTimeRange_ShouldReturnCorrectRecords()
        {
            // Arrange
            var now = DateTime.Now;
            _repository.Insert(new StateMachineRecord { MachineName = "M1", StartTime = now.AddHours(-2), Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "M1", StartTime = now.AddHours(-1), Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "M1", StartTime = now.AddHours(-5), Status = "Completed" });

            // Act
            var records = _repository.GetByTimeRange(now.AddHours(-3), now).ToList();

            // Assert
            Assert.Equal(2, records.Count);
        }

        [Fact]
        public void StateMachineRecord_GetLatestRecord_ShouldReturnMostRecent()
        {
            // Arrange
            var now = DateTime.Now;
            _repository.Insert(new StateMachineRecord { MachineName = "LatestTest", StartTime = now.AddHours(-2), Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "LatestTest", StartTime = now, Status = "Error" });
            _repository.Insert(new StateMachineRecord { MachineName = "LatestTest", StartTime = now.AddHours(-1), Status = "Completed" });

            // Act
            var latest = _repository.GetLatestRecord("LatestTest");

            // Assert
            Assert.NotNull(latest);
            Assert.Equal("Error", latest.Status);
        }

        [Fact]
        public void StateMachineRecord_GetSummary_ShouldCalculateCorrectly()
        {
            // Arrange
            var now = DateTime.Now;
            _repository.Insert(new StateMachineRecord { MachineName = "SummaryTest", StartTime = now, DurationMs = 1000, IsSuccess = true, Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "SummaryTest", StartTime = now, DurationMs = 2000, IsSuccess = true, Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "SummaryTest", StartTime = now, DurationMs = 3000, IsSuccess = false, Status = "Error" });

            // Act
            var summary = _repository.GetSummary("SummaryTest");

            // Assert
            Assert.Equal(3, summary.TotalCount);
            Assert.Equal(2, summary.SuccessCount);
            Assert.Equal(1, summary.FailedCount);
            Assert.Equal(2000, summary.AverageDurationMs, 0);
            Assert.Equal(66.67, summary.SuccessRate, 2);
        }

        [Fact]
        public void StateMachineRecord_CleanupRecords_ShouldRemoveOldRecords()
        {
            // Arrange
            var now = DateTime.Now;
            _repository.Insert(new StateMachineRecord { MachineName = "CleanupTest", StartTime = now.AddDays(-10), Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "CleanupTest", StartTime = now.AddDays(-5), Status = "Completed" });
            _repository.Insert(new StateMachineRecord { MachineName = "CleanupTest", StartTime = now, Status = "Completed" });

            // Act
            int deleted = _repository.CleanupRecords(now.AddDays(-7));
            var remaining = _repository.GetByMachineName("CleanupTest").ToList();

            // Assert
            Assert.Equal(1, deleted);
            Assert.Equal(2, remaining.Count);
            Assert.All(remaining, r => Assert.True(r.StartTime > now.AddDays(-7)));
        }

        [Fact]
        public async Task StateMachineManager_WithRepository_ShouldPersistOnComplete()
        {
            // Arrange
            var manager = new StateMachineManager(_repository);
            var machine = manager.CreateMachine("PersistTest", "持久化测试");
            var root = new SequentialModule("Root");
            var custom = new CustomActionModule("SuccessStep")
            {
                TimeoutMs = 5000
            };
            custom.SetAction((ctx, token) => Task.FromResult(true));
            root.AddModule(custom);
            machine.SetRootModule(root);

            // Act
            bool success = await manager.StartMachineAsync("PersistTest");
            manager.WaitForPersistenceFlush();
            var records = _repository.GetByMachineName("PersistTest").ToList();

            // Assert
            Assert.True(success);
            Assert.True(records.Count >= 1, $"期望至少1条记录，实际有{records.Count}条");
            var record = records.OrderByDescending(r => r.Id).First();
            Assert.Equal("PersistTest", record.MachineName);
            Assert.True(record.IsSuccess);
            Assert.Equal("Completed", record.Status);
            Assert.Equal(2, record.ModuleCount);
            Assert.NotNull(record.ModuleStatsJson);
        }

        [Fact]
        public async Task StateMachineManager_WithRepository_ShouldPersistOnError()
        {
            // Arrange
            var manager = new StateMachineManager(_repository);
            var machine = manager.CreateMachine("PersistErrorTest", "持久化错误测试");
            var root = new SequentialModule("Root");
            var custom = new CustomActionModule("FailStep")
            {
                TimeoutMs = 5000
            };
            custom.SetAction((ctx, token) => Task.FromException<bool>(new InvalidOperationException("模拟异常")));
            root.AddModule(custom);
            machine.SetRootModule(root);

            // Act
            bool success = await manager.StartMachineAsync("PersistErrorTest");
            manager.WaitForPersistenceFlush();
            var records = _repository.GetByMachineName("PersistErrorTest").ToList();

            // Assert
            Assert.False(success);
            Assert.True(records.Count >= 1, $"期望至少1条记录，实际有{records.Count}条");
            var record = records.OrderByDescending(r => r.Id).First();
            Assert.False(record.IsSuccess);
            Assert.Equal("Error", record.Status);
            Assert.Contains("FailStep", record.ErrorMessage);
        }

        [Fact]
        public async Task StateMachineManager_WithoutRepository_ShouldWorkInMemory()
        {
            // Arrange
            var manager = new StateMachineManager(); // 无仓储
            var machine = manager.CreateMachine("MemoryOnly", "仅内存测试");
            var root = new SequentialModule("Root");
            var custom = new CustomActionModule("Step1")
            {
                TimeoutMs = 5000
            };
            custom.SetAction((ctx, token) => Task.FromResult(true));
            root.AddModule(custom);
            machine.SetRootModule(root);

            // Act & Assert（不应抛出异常）
            bool result = await manager.StartMachineAsync("MemoryOnly");
            Assert.True(result);
        }

        [Fact]
        public async Task RestoreMachine_FromErrorRecord_ShouldResumeFromCorrectIndex()
        {
            // Arrange
            var manager = new StateMachineManager(_repository);
            var machineName = "ResumeTest";
            var machine = manager.CreateMachine(machineName);
            var root = new SequentialModule("Root");

            var step1Executed = false;
            var step2Executed = false;
            var step3Executed = false;

            var step1 = new CustomActionModule("Step1") { TimeoutMs = 5000 };
            step1.SetAction((ctx, token) =>
            {
                step1Executed = true;
                return Task.FromResult(true);
            });

            var step2 = new CustomActionModule("Step2") { TimeoutMs = 5000 };
            step2.SetAction((ctx, token) =>
            {
                // 使用上下文存储失败标记，该标记会随 ResumeDataJson 持久化
                if (!ctx.GetResult<bool>("Step2FailedOnce", false))
                {
                    ctx.SetResult("Step2FailedOnce", true);
                    throw new InvalidOperationException("模拟 Step2 首次失败");
                }
                step2Executed = true;
                return Task.FromResult(true);
            });

            var step3 = new CustomActionModule("Step3") { TimeoutMs = 5000 };
            step3.SetAction((ctx, token) =>
            {
                step3Executed = true;
                return Task.FromResult(true);
            });

            root.AddModule(step1);
            root.AddModule(step2);
            root.AddModule(step3);
            machine.SetRootModule(root);

            // Act - 首次执行，Step2 失败
            var result1 = await manager.StartMachineAsync(machineName);
            manager.WaitForPersistenceFlush();

            // Assert - 首次执行
            Assert.False(result1);
            Assert.True(step1Executed);
            Assert.False(step2Executed);
            Assert.False(step3Executed);

            var errorRecord = _repository.GetLatestRecord(machineName, "Error");
            Assert.NotNull(errorRecord);
            Assert.False(string.IsNullOrEmpty(errorRecord.ResumeDataJson));

            // 重置标志
            step1Executed = false;
            step2Executed = false;
            step3Executed = false;

            // Act - 从最近 Error 记录恢复执行
            var result2 = await manager.RestoreMachineAsync(machineName);
            manager.WaitForPersistenceFlush();

            // Assert - 恢复执行，应从 Step2 继续
            Assert.True(result2);
            Assert.False(step1Executed); // 断点续传，Step1 不应再次执行
            Assert.True(step2Executed);
            Assert.True(step3Executed);

            var completedRecord = _repository.GetLatestRecord(machineName, "Completed");
            Assert.NotNull(completedRecord);
        }
    }
}
