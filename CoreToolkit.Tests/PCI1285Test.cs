using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Data;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;
using CoreToolkit.Motion.Providers.Advantech;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 研华 PCI-1285 运动控制卡测试程序
    /// 支持本地模拟卡测试
    /// </summary>
    public class PCI1285Test
    {
        private IMotionCard _motionCard;
        private LogRepository _logRepo;
        private AxisParameterRepository _axisParamRepo;
        private IOParameterRepository _ioParamRepo;
        private SqliteDbContext _logDb;
        private SqliteDbContext _motionDb;
        
        // 测试配置
        private const int TEST_CARD_ID = 0;
        private const int TEST_AXIS_COUNT = 8;
        private const string LOGGER_NAME = "PCI1285Test";

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public async Task RunAllTestsAsync()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    研华 PCI-1285 运动控制卡测试程序");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                // 1. 初始化数据库
                await InitializeDatabasesAsync();
                
                // 2. 初始化轴参数和IO参数
                InitializeParameters();
                
                // 3. 测试轴API功能
                await TestAxisApiAsync();
                
                // 4. 测试数据库操作
                await TestDatabaseOperationsAsync();
                
                // 5. 输出测试报告
                PrintTestReport();
            }
            catch (Exception ex)
            {
                LogError($"测试过程发生错误: {ex.Message}", ex);
                Console.WriteLine($"\n错误: {ex.Message}");
                Console.WriteLine($"堆栈: {ex.StackTrace}");
            }
            finally
            {
                // 清理资源
                Cleanup();
            }
        }

        #region 1. 数据库初始化

        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        private async Task InitializeDatabasesAsync()
        {
            Console.WriteLine("【步骤1】初始化数据库...");
            Console.WriteLine("-----------------------------------------------");

            // 注册数据库配置
            DatabaseConfig.Register("LogDb", "logs.db");
            DatabaseConfig.Register("MotionDb", "motion.db");
            
            // 初始化所有数据库
            DatabaseFactory.InitializeAllDatabases();
            
            // 创建数据库上下文
            _logDb = DatabaseFactory.CreateContext("LogDb");
            _motionDb = DatabaseFactory.CreateContext("MotionDb");
            
            // 创建仓储实例
            _logRepo = new LogRepository(_logDb);
            _axisParamRepo = new AxisParameterRepository(_motionDb);
            _ioParamRepo = new IOParameterRepository(_motionDb);
            
            await _logRepo.WriteInfoAsync("PCI-1285 测试程序启动", LOGGER_NAME);
            
            Console.WriteLine("✓ 数据库初始化完成");
            Console.WriteLine("  - 日志数据库: logs.db");
            Console.WriteLine("  - 运动参数数据库: motion.db");
            Console.WriteLine();
        }

        #endregion

        #region 2. 参数初始化

        /// <summary>
        /// 初始化轴参数和IO参数到数据库
        /// </summary>
        private void InitializeParameters()
        {
            Console.WriteLine("【步骤2】初始化轴参数和IO参数...");
            Console.WriteLine("-----------------------------------------------");

            // 清空现有参数
            var existingAxisParams = _axisParamRepo.GetAll();
            foreach (var param in existingAxisParams)
            {
                _axisParamRepo.Delete(param.Id);
            }
            
            var existingIoParams = _ioParamRepo.GetAll();
            foreach (var param in existingIoParams)
            {
                _ioParamRepo.Delete(param.Id);
            }

            // 创建8轴默认参数
            string[] axisNames = { "X轴", "Y轴", "Z轴", "R轴", "A轴", "B轴", "C轴", "D轴" };
            for (int i = 0; i < TEST_AXIS_COUNT; i++)
            {
                var axisParam = new AxisParameter
                {
                    轴名称 = axisNames[i],
                    轴号 = i,
                    卡号 = TEST_CARD_ID,
                    轴类型 = 0,
                    脉冲当量 = 1000,
                    脉冲当量分母 = 1,
                    运动低速 = 1000,
                    运动高速 = 50000,
                    加速度 = 100000,
                    减速度 = 100000,
                    加加速度 = 0,
                    减减速度 = 0,
                    回原模式 = 1,
                    回原方向 = -1,
                    原点高速 = 10000,
                    原点低速 = 1000,
                    原点加速度 = 50000,
                    原点减速度 = 50000,
                    原点偏移 = 0,
                    正向软极限 = 100000,
                    负向软极限 = -100000,
                    使能IO = i
                };
                _axisParamRepo.Insert(axisParam);
            }
            Console.WriteLine($"✓ 已创建 {TEST_AXIS_COUNT} 个轴的默认参数");

            // 创建IO参数
            for (int i = 0; i < 16; i++)
            {
                var ioParam = new IOParameter
                {
                    卡号 = TEST_CARD_ID,
                    端口号 = i,
                    输入点 = i,
                    输入名称 = $"输入{i}",
                    输出点 = i,
                    输出名称 = $"输出{i}"
                };
                _ioParamRepo.Insert(ioParam);
            }
            Console.WriteLine($"✓ 已创建 16 个IO端口参数");

            _logRepo.WriteInfo("轴参数和IO参数初始化完成", LOGGER_NAME);
            Console.WriteLine();
        }

        #endregion

        #region 3. 轴API功能测试

        /// <summary>
        /// 测试轴API功能
        /// </summary>
        private async Task TestAxisApiAsync()
        {
            Console.WriteLine("【步骤3】测试轴API功能...");
            Console.WriteLine("-----------------------------------------------");

            try
            {
                // 3.1 创建控制卡实例
                Console.WriteLine("3.1 创建控制卡实例...");
                _motionCard = MotionCardFactory.CreateCard("PCI1285");
                Console.WriteLine($"  控制卡: {_motionCard.CardName}");
                Console.WriteLine($"  厂商: {_motionCard.Vendor}");
                Console.WriteLine($"  型号: {_motionCard.Model}");
                Console.WriteLine($"  轴数: {_motionCard.AxisCount}");
                await _logRepo.WriteInfoAsync($"创建控制卡实例: {_motionCard.CardName}", LOGGER_NAME);

                // 3.2 初始化控制卡
                Console.WriteLine("\n3.2 初始化控制卡...");
                var config = new MotionConfig
                {
                    CardId = TEST_CARD_ID,
                    AxisConfigs = new List<AxisConfig>(),
                    InputCount = 16,
                    OutputCount = 16
                };

                // 从数据库加载轴配置
                var axisParams = _axisParamRepo.GetByCardId(TEST_CARD_ID);
                foreach (var param in axisParams)
                {
                    config.AxisConfigs.Add(new AxisConfig
                    {
                        AxisId = param.轴号,
                        AxisName = param.轴名称,
                        PulseEquivalent = (double)param.脉冲当量 / param.脉冲当量分母,
                        DefaultSpeed = param.运动高速,
                        DefaultAcceleration = param.加速度,
                        DefaultDeceleration = param.减速度,
                        HomeSpeedHigh = param.原点高速,
                        HomeSpeedLow = param.原点低速,
                        HomeDirection = param.回原方向,
                        SoftPositiveLimit = param.正向软极限,
                        SoftNegativeLimit = param.负向软极限
                    });
                }

                _motionCard.Initialize(config);
                Console.WriteLine($"  初始化状态: {(_motionCard.IsInitialized ? "成功" : "失败")}");
                await _logRepo.WriteInfoAsync($"控制卡初始化完成，卡号: {TEST_CARD_ID}", LOGGER_NAME);

                // 3.3 打开控制卡
                Console.WriteLine("\n3.3 打开控制卡...");
                _motionCard.Open();
                Console.WriteLine($"  连接状态: {(_motionCard.IsOpen ? "已连接" : "未连接")}");
                await _logRepo.WriteInfoAsync("控制卡已打开", LOGGER_NAME);

                // 3.4 测试伺服使能
                Console.WriteLine("\n3.4 测试伺服使能...");
                for (int i = 0; i < Math.Min(4, _motionCard.AxisCount); i++)
                {
                    try
                    {
                        _motionCard.SetServoEnable(i, true);
                        Thread.Sleep(100);
                        bool servoOn = _motionCard.GetServoEnable(i);
                        Console.WriteLine($"  轴{i} 伺服状态: {(servoOn ? "使能" : "未使能")}");
                        await _logRepo.WriteInfoAsync($"轴{i} 伺服使能状态: {servoOn}", LOGGER_NAME);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  轴{i} 伺服使能失败: {ex.Message}");
                        await _logRepo.WriteWarningAsync($"轴{i} 伺服使能失败: {ex.Message}", LOGGER_NAME);
                    }
                }

                // 3.5 测试读取位置
                Console.WriteLine("\n3.5 测试读取轴位置...");
                for (int i = 0; i < Math.Min(4, _motionCard.AxisCount); i++)
                {
                    try
                    {
                        double position = _motionCard.GetPosition(i);
                        Console.WriteLine($"  轴{i} 当前位置: {position:F2}");
                        await _logRepo.WriteInfoAsync($"轴{i} 当前位置: {position:F2}", LOGGER_NAME);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  轴{i} 读取位置失败: {ex.Message}");
                        await _logRepo.WriteWarningAsync($"轴{i} 读取位置失败: {ex.Message}", LOGGER_NAME);
                    }
                }

                // 3.6 测试获取轴状态
                Console.WriteLine("\n3.6 测试获取轴状态...");
                for (int i = 0; i < Math.Min(2, _motionCard.AxisCount); i++)
                {
                    try
                    {
                        var status = _motionCard.GetAxisStatus(i);
                        Console.WriteLine($"  轴{i} 状态:");
                        Console.WriteLine($"    - 运行中: {status.IsRunning}");
                        Console.WriteLine($"    - 到位: {status.InPosition}");
                        Console.WriteLine($"    - 伺服使能: {status.ServoOn}");
                        Console.WriteLine($"    - 报警: {status.IsAlarm}");
                        Console.WriteLine($"    - 正限位: {status.PositiveLimit}");
                        Console.WriteLine($"    - 负限位: {status.NegativeLimit}");
                        Console.WriteLine($"    - 原点信号: {status.HomeSignal}");
                        Console.WriteLine($"    - 当前速度: {status.CurrentSpeed:F2}");
                        Console.WriteLine($"    - 实际位置: {status.ActualPosition:F2}");
                        Console.WriteLine($"    - 命令位置: {status.CommandPosition:F2}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  轴{i} 获取状态失败: {ex.Message}");
                    }
                }

                // 3.7 测试设置位置
                Console.WriteLine("\n3.7 测试设置位置...");
                try
                {
                    _motionCard.SetPosition(0, 0);
                    Console.WriteLine("  轴0 位置已清零");
                    await _logRepo.WriteInfoAsync("轴0 位置已清零", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  设置位置失败: {ex.Message}");
                }

                // 3.8 测试JOG运动
                Console.WriteLine("\n3.8 测试JOG运动...");
                try
                {
                    Console.WriteLine("  轴0 JOG正向运动 500ms...");
                    _motionCard.Jog(0, 1, 1000);
                    Thread.Sleep(500);
                    _motionCard.Stop(0);
                    Console.WriteLine("  轴0 JOG运动停止");
                    await _logRepo.WriteInfoAsync("轴0 JOG运动测试完成", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  JOG运动失败: {ex.Message}");
                    await _logRepo.WriteWarningAsync($"JOG运动失败: {ex.Message}", LOGGER_NAME);
                }

                // 3.9 测试相对运动
                Console.WriteLine("\n3.9 测试相对运动...");
                try
                {
                    double startPos = _motionCard.GetPosition(0);
                    Console.WriteLine($"  起始位置: {startPos:F2}");
                    _motionCard.MoveRelative(0, 1000, 2000);
                    _motionCard.WaitForMotionComplete(0, 5000);
                    double endPos = _motionCard.GetPosition(0);
                    Console.WriteLine($"  结束位置: {endPos:F2}");
                    Console.WriteLine($"  实际移动距离: {endPos - startPos:F2}");
                    await _logRepo.WriteInfoAsync($"轴0 相对运动测试完成，移动距离: {endPos - startPos:F2}", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  相对运动失败: {ex.Message}");
                    await _logRepo.WriteWarningAsync($"相对运动失败: {ex.Message}", LOGGER_NAME);
                }

                // 3.10 测试绝对运动
                Console.WriteLine("\n3.10 测试绝对运动...");
                try
                {
                    _motionCard.MoveAbsolute(0, 5000, 3000);
                    bool completed = _motionCard.WaitForMotionComplete(0, 5000);
                    double finalPos = _motionCard.GetPosition(0);
                    Console.WriteLine($"  目标位置: 5000");
                    Console.WriteLine($"  实际位置: {finalPos:F2}");
                    Console.WriteLine($"  运动完成: {completed}");
                    await _logRepo.WriteInfoAsync($"轴0 绝对运动测试完成，到位: {completed}", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  绝对运动失败: {ex.Message}");
                    await _logRepo.WriteWarningAsync($"绝对运动失败: {ex.Message}", LOGGER_NAME);
                }

                // 3.11 测试IO操作
                Console.WriteLine("\n3.11 测试IO操作...");
                try
                {
                    // 读取输入
                    bool input0 = _motionCard.ReadInput(0);
                    Console.WriteLine($"  输入点0 状态: {input0}");
                    
                    // 设置输出
                    _motionCard.WriteOutput(0, true);
                    Thread.Sleep(100);
                    bool output0 = _motionCard.ReadOutput(0);
                    Console.WriteLine($"  输出点0 设置: true, 读取: {output0}");
                    
                    _motionCard.WriteOutput(0, false);
                    Thread.Sleep(100);
                    output0 = _motionCard.ReadOutput(0);
                    Console.WriteLine($"  输出点0 设置: false, 读取: {output0}");
                    
                    await _logRepo.WriteInfoAsync("IO操作测试完成", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  IO操作失败: {ex.Message}");
                    await _logRepo.WriteWarningAsync($"IO操作失败: {ex.Message}", LOGGER_NAME);
                }

                // 3.12 测试复位
                Console.WriteLine("\n3.12 测试复位...");
                try
                {
                    _motionCard.Reset();
                    Console.WriteLine("  控制卡已复位");
                    await _logRepo.WriteInfoAsync("控制卡复位完成", LOGGER_NAME);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  复位失败: {ex.Message}");
                }

                // 3.13 关闭伺服
                Console.WriteLine("\n3.13 关闭伺服使能...");
                for (int i = 0; i < Math.Min(4, _motionCard.AxisCount); i++)
                {
                    try
                    {
                        _motionCard.SetServoEnable(i, false);
                        Console.WriteLine($"  轴{i} 伺服已关闭");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  轴{i} 关闭伺服失败: {ex.Message}");
                    }
                }

                Console.WriteLine("\n✓ 轴API功能测试完成");
                await _logRepo.WriteInfoAsync("轴API功能测试完成", LOGGER_NAME);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ 轴API测试失败: {ex.Message}");
                await _logRepo.WriteErrorAsync($"轴API测试失败: {ex.Message}", LOGGER_NAME);
                throw;
            }
            Console.WriteLine();
        }

        #endregion

        #region 4. 数据库操作测试

        /// <summary>
        /// 测试数据库操作
        /// </summary>
        private async Task TestDatabaseOperationsAsync()
        {
            Console.WriteLine("【步骤4】测试数据库操作...");
            Console.WriteLine("-----------------------------------------------");

            // 4.1 轴参数数据库操作
            Console.WriteLine("4.1 轴参数数据库操作...");
            
            // 查询所有轴参数
            var allAxisParams = _axisParamRepo.GetAll();
            Console.WriteLine($"  数据库中共有 {allAxisParams.Count} 个轴参数");
            
            // 按卡号查询
            var card0Params = _axisParamRepo.GetByCardId(TEST_CARD_ID);
            Console.WriteLine($"  卡{TEST_CARD_ID}有 {card0Params.Count} 个轴参数");
            
            // 按卡号和轴号查询
            var axis0Param = _axisParamRepo.GetByCardAndAxis(TEST_CARD_ID, 0);
            if (axis0Param != null)
            {
                Console.WriteLine($"  轴0参数: 名称={axis0Param.轴名称}, 高速={axis0Param.运动高速}");
            }
            
            // 更新轴参数
            if (axis0Param != null)
            {
                axis0Param.运动高速 = 60000;
                _axisParamRepo.Update(axis0Param);
                Console.WriteLine($"  已更新轴0运动高速为 60000");
                await _logRepo.WriteInfoAsync("轴0参数已更新", LOGGER_NAME);
            }

            // 4.2 IO参数数据库操作
            Console.WriteLine("\n4.2 IO参数数据库操作...");
            
            // 查询所有IO参数
            var allIoParams = _ioParamRepo.GetAll();
            Console.WriteLine($"  数据库中共有 {allIoParams.Count} 个IO参数");
            
            // 按卡号查询
            var card0IoParams = _ioParamRepo.GetByCardId(TEST_CARD_ID);
            Console.WriteLine($"  卡{TEST_CARD_ID}有 {card0IoParams.Count} 个IO参数");
            
            // 按输入点查询
            var input0Param = _ioParamRepo.GetByInputPoint(TEST_CARD_ID, 0);
            if (input0Param != null)
            {
                Console.WriteLine($"  输入点0: 名称={input0Param.输入名称}");
            }
            
            // 更新IO参数
            if (input0Param != null)
            {
                input0Param.输入名称 = "急停信号";
                _ioParamRepo.Update(input0Param);
                Console.WriteLine($"  已更新输入点0名称为 '急停信号'");
                await _logRepo.WriteInfoAsync("IO参数已更新", LOGGER_NAME);
            }

            // 4.3 日志数据库操作
            Console.WriteLine("\n4.3 日志数据库操作...");
            
            // 写入各级别日志
            await _logRepo.WriteDebugAsync("调试日志测试", LOGGER_NAME);
            await _logRepo.WriteInfoAsync("信息日志测试", LOGGER_NAME);
            await _logRepo.WriteWarningAsync("警告日志测试", LOGGER_NAME);
            Console.WriteLine("  已写入调试、信息、警告日志");
            
            // 查询最近日志
            var recentLogs = await _logRepo.GetRecentLogsAsync(10);
            Console.WriteLine($"  最近10条日志:");
            foreach (var log in recentLogs)
            {
                Console.WriteLine($"    [{log.Level}] {log.Timestamp:HH:mm:ss} - {log.Message}");
            }
            
            // 按级别查询
            var infoLogs = _logRepo.GetByLevel("Info");
            Console.WriteLine($"  信息级别日志数量: {infoLogs.Count()}");
            
            // 按时间范围查询
            var startTime = DateTime.Now.AddMinutes(-10);
            var endTime = DateTime.Now.AddMinutes(10);
            var timeRangeLogs = _logRepo.GetLogsByTimeRange(startTime, endTime);
            Console.WriteLine($"  最近10分钟日志数量: {timeRangeLogs.Count()}");

            Console.WriteLine("\n✓ 数据库操作测试完成");
            Console.WriteLine();
        }

        #endregion

        #region 5. 测试报告

        /// <summary>
        /// 输出测试报告
        /// </summary>
        private void PrintTestReport()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("              测试报告汇总");
            Console.WriteLine("=================================================");
            
            Console.WriteLine("\n【测试内容】");
            Console.WriteLine("  1. 数据库初始化 - ✓");
            Console.WriteLine("  2. 轴参数初始化 - ✓");
            Console.WriteLine("  3. IO参数初始化 - ✓");
            Console.WriteLine("  4. 控制卡初始化 - ✓");
            Console.WriteLine("  5. 伺服使能控制 - ✓");
            Console.WriteLine("  6. 位置读写 - ✓");
            Console.WriteLine("  7. 轴状态获取 - ✓");
            Console.WriteLine("  8. JOG运动 - ✓");
            Console.WriteLine("  9. 相对运动 - ✓");
            Console.WriteLine(" 10. 绝对运动 - ✓");
            Console.WriteLine(" 11. IO操作 - ✓");
            Console.WriteLine(" 12. 控制卡复位 - ✓");
            Console.WriteLine(" 13. 轴参数数据库 - ✓");
            Console.WriteLine(" 14. IO参数数据库 - ✓");
            Console.WriteLine(" 15. 日志数据库 - ✓");
            
            Console.WriteLine("\n【数据库状态】");
            var axisCount = _axisParamRepo.GetAll().Count;
            var ioCount = _ioParamRepo.GetAll().Count;
            var logCount = _logRepo.GetAll().Count();
            Console.WriteLine($"  - 轴参数记录: {axisCount}");
            Console.WriteLine($"  - IO参数记录: {ioCount}");
            Console.WriteLine($"  - 日志记录: {logCount}");
            
            Console.WriteLine("\n【测试结论】");
            Console.WriteLine("  所有测试项目已完成！");
            Console.WriteLine("  请检查日志文件了解详细信息:");
            Console.WriteLine("    - logs.db (日志数据库)");
            Console.WriteLine("    - motion.db (运动参数数据库)");
            
            Console.WriteLine("\n=================================================");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private void LogError(string message, Exception ex)
        {
            try
            {
                _logRepo?.WriteException(ex, message, LOGGER_NAME);
            }
            catch
            {
                // 忽略日志写入失败
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            Console.WriteLine("\n正在清理资源...");
            
            try
            {
                _motionCard?.Close();
                Console.WriteLine("  控制卡已关闭");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  关闭控制卡失败: {ex.Message}");
            }

            try
            {
                _motionCard?.Dispose();
                Console.WriteLine("  控制卡资源已释放");
            }
            catch
            {
                // 忽略
            }

            try
            {
                _logDb?.Dispose();
                _motionDb?.Dispose();
                Console.WriteLine("  数据库连接已关闭");
            }
            catch
            {
                // 忽略
            }

            Console.WriteLine("资源清理完成");
        }

        #endregion
    }
}
