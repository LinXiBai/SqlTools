using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Data;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 轴组、插补、PTP、PVT功能测试
    /// </summary>
    public class AxisGroupTest
    {
        private IMotionCard _motionCard;
        private LogRepository _logRepo;
        private AxisGroupManager _groupManager;
        private SqliteDbContext _logDb;
        private const string LOGGER_NAME = "AxisGroupTest";

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public async Task RunAllTestsAsync()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    轴组、插补、PTP、PVT功能测试");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                // 1. 初始化
                await InitializeAsync();

                // 2. 测试轴组基本功能
                TestAxisGroupBasic();

                // 3. 测试PTP运动
                TestPTPMotion();

                // 4. 测试线性插补
                TestLinearInterpolation();

                // 5. 测试圆弧插补
                TestCircularInterpolation();

                // 6. 测试PVT运动
                TestPVTMotion();

                // 7. 测试多轴同步PVT
                TestPVTSyncMotion();

                // 8. 测试PTP规划器
                TestPTPPlanner();

                // 9. 测试PVT规划器
                TestPVTPlanner();

                // 10. 输出测试报告
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
                Cleanup();
            }
        }

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        private async Task InitializeAsync()
        {
            Console.WriteLine("【步骤1】初始化...");
            Console.WriteLine("-----------------------------------------------");

            // 注册并初始化数据库
            DatabaseConfig.Register("LogDb", "logs.db");
            DatabaseFactory.InitializeAllDatabases();
            _logDb = DatabaseFactory.CreateContext("LogDb");
            _logRepo = new LogRepository(_logDb);

            // 创建控制卡
            _motionCard = MotionCardFactory.CreateCard("PCI1285");
            _motionCard.Initialize(new MotionConfig { CardId = 0 });
            _motionCard.Open();

            // 获取轴组管理器（通过反射获取）
            var groupMgrProp = _motionCard.GetType().GetProperty("GroupManager");
            if (groupMgrProp != null)
            {
                _groupManager = groupMgrProp.GetValue(_motionCard) as AxisGroupManager;
            }
            
            if (_groupManager == null)
            {
                Console.WriteLine("警告: 无法获取轴组管理器，使用默认管理器");
                _groupManager = new AxisGroupManager(_motionCard);
            }

            // 使能前4轴
            for (int i = 0; i < 4; i++)
            {
                _motionCard.SetServoEnable(i, true);
            }

            LogMessage("初始化完成");
            Console.WriteLine("✓ 初始化完成\n");
        }

        #endregion

        #region 轴组基本功能测试

        /// <summary>
        /// 测试轴组基本功能
        /// </summary>
        private void TestAxisGroupBasic()
        {
            Console.WriteLine("【步骤2】轴组基本功能测试...");
            Console.WriteLine("-----------------------------------------------");

            // 创建XY轴组
            var xyGroup = _groupManager.CreateXYGroup(0, 1, "XY_Group");
            Console.WriteLine($"✓ 创建轴组: {xyGroup.GroupName}, 轴数: {xyGroup.AxisCount}");

            // 创建XYZ轴组
            var xyzGroup = _groupManager.CreateXYZGroup(0, 1, 2, "XYZ_Group");
            Console.WriteLine($"✓ 创建轴组: {xyzGroup.GroupName}, 轴数: {xyzGroup.AxisCount}");

            // 启用轴组
            xyGroup.Enable();
            Console.WriteLine($"✓ 轴组 {xyGroup.GroupName} 已启用");

            // 获取状态
            var status = xyGroup.GetStatus();
            Console.WriteLine($"✓ 轴组状态: IsEnabled={status.IsEnabled}, IsMoving={status.IsMoving}");

            // 禁用并删除
            xyGroup.Disable();
            _groupManager.DeleteGroup(xyGroup.GroupId);
            Console.WriteLine($"✓ 轴组 {xyGroup.GroupName} 已删除");

            _groupManager.DeleteGroup(xyzGroup.GroupId);
            Console.WriteLine($"✓ 轴组 {xyzGroup.GroupName} 已删除");

            LogMessage("轴组基本功能测试完成");
            Console.WriteLine();
        }

        #endregion

        #region PTP运动测试

        /// <summary>
        /// 测试PTP运动
        /// </summary>
        private void TestPTPMotion()
        {
            Console.WriteLine("【步骤3】PTP运动测试...");
            Console.WriteLine("-----------------------------------------------");

            var group = _groupManager.CreateXYZGroup(0, 1, 2, "PTP_Test");
            group.Enable();

            // 获取当前位置
            var status = group.GetStatus();
            Console.WriteLine($"当前位置: X={status.Positions[0]:F2}, Y={status.Positions[1]:F2}, Z={status.Positions[2]:F2}");

            // PTP运动到目标位置（时间最优策略）
            Console.WriteLine("\n执行PTP运动（时间最优）...");
            group.MovePTP(new double[] { 1000, 2000, 500 }, 80);
            group.WaitForComplete(10000);
            Console.WriteLine("✓ PTP运动完成");

            // 查看最终位置
            status = group.GetStatus();
            Console.WriteLine($"最终位置: X={status.Positions[0]:F2}, Y={status.Positions[1]:F2}, Z={status.Positions[2]:F2}");

            group.Disable();
            _groupManager.DeleteGroup(group.GroupId);

            LogMessage("PTP运动测试完成");
            Console.WriteLine();
        }

        #endregion

        #region 线性插补测试

        /// <summary>
        /// 测试线性插补
        /// </summary>
        private void TestLinearInterpolation()
        {
            Console.WriteLine("【步骤4】线性插补测试...");
            Console.WriteLine("-----------------------------------------------");

            var group = _groupManager.CreateXYZGroup(0, 1, 2, "Linear_Test");
            group.Enable();

            // 线性插补到绝对位置
            Console.WriteLine("\n执行线性插补（绝对位置）...");
            group.MoveLinearAbs(new double[] { 2000, 3000, 1000 }, 5000, 10000, 10000);
            group.WaitForComplete(10000);
            Console.WriteLine("✓ 线性插补（绝对）完成");

            // 线性插补相对位置
            Console.WriteLine("\n执行线性插补（相对位置）...");
            group.MoveLinearRel(new double[] { 500, 500, 200 }, 3000, 5000, 5000);
            group.WaitForComplete(10000);
            Console.WriteLine("✓ 线性插补（相对）完成");

            // 查看最终位置
            var status = group.GetStatus();
            Console.WriteLine($"最终位置: X={status.Positions[0]:F2}, Y={status.Positions[1]:F2}, Z={status.Positions[2]:F2}");

            group.Disable();
            _groupManager.DeleteGroup(group.GroupId);

            LogMessage("线性插补测试完成");
            Console.WriteLine();
        }

        #endregion

        #region 圆弧插补测试

        /// <summary>
        /// 测试圆弧插补
        /// </summary>
        private void TestCircularInterpolation()
        {
            Console.WriteLine("【步骤5】圆弧插补测试...");
            Console.WriteLine("-----------------------------------------------");

            var group = _groupManager.CreateXYGroup(0, 1, "Circle_Test");
            group.Enable();

            // 先移动到起点
            Console.WriteLine("\n移动到圆弧起点 (1000, 0)...");
            group.MovePTP(new double[] { 1000, 0 }, 50);
            group.WaitForComplete(5000);

            // 顺时针圆弧
            Console.WriteLine("\n执行顺时针圆弧插补...");
            double radius = 1000;
            group.MoveCircularAbs(
                new double[] { 0, 0 },      // 圆心
                new double[] { -1000, 0 },  // 终点
                1,                          // 顺时针
                2000);                      // 速度
            group.WaitForComplete(10000);
            Console.WriteLine("✓ 顺时针圆弧完成");

            // 逆时针圆弧
            Console.WriteLine("\n执行逆时针圆弧插补...");
            group.MoveCircularAbs(
                new double[] { 0, 0 },      // 圆心
                new double[] { 1000, 0 },   // 终点
                -1,                         // 逆时针
                2000);                      // 速度
            group.WaitForComplete(10000);
            Console.WriteLine("✓ 逆时针圆弧完成");

            group.Disable();
            _groupManager.DeleteGroup(group.GroupId);

            LogMessage("圆弧插补测试完成");
            Console.WriteLine();
        }

        #endregion

        #region PVT运动测试

        /// <summary>
        /// 测试PVT运动
        /// </summary>
        private void TestPVTMotion()
        {
            Console.WriteLine("【步骤6】PVT运动测试...");
            Console.WriteLine("-----------------------------------------------");

            var group = _groupManager.CreateGroup("PVT_Test", new[] { 0 });
            group.Enable();

            // 创建PVT数据点 - S曲线
            var pvtData = new List<PVTPoint>
            {
                new PVTPoint(0, 0, 0),          // 起点
                new PVTPoint(500, 2000, 250),   // 加速段
                new PVTPoint(1500, 4000, 500),  // 匀速段
                new PVTPoint(2000, 2000, 250),  // 减速段
                new PVTPoint(2500, 0, 0)        // 终点
            };

            Console.WriteLine("\nPVT数据点:");
            foreach (var pvt in pvtData)
            {
                Console.WriteLine($"  {pvt}");
            }

            Console.WriteLine("\n执行PVT运动...");
            group.MovePVT(pvtData.ToArray(), 0);
            group.WaitForComplete(5000);
            Console.WriteLine("✓ PVT运动完成");

            group.Disable();
            _groupManager.DeleteGroup(group.GroupId);

            LogMessage("PVT运动测试完成");
            Console.WriteLine();
        }

        #endregion

        #region 多轴同步PVT测试

        /// <summary>
        /// 测试多轴同步PVT运动
        /// </summary>
        private void TestPVTSyncMotion()
        {
            Console.WriteLine("【步骤7】多轴同步PVT运动测试...");
            Console.WriteLine("-----------------------------------------------");

            var group = _groupManager.CreateXYGroup(0, 1, "PVT_Sync_Test");
            group.Enable();

            // X轴PVT数据
            var xPVT = new[]
            {
                new PVTPoint(0, 0, 0),
                new PVTPoint(1000, 3000, 300),
                new PVTPoint(2000, 0, 0)
            };

            // Y轴PVT数据
            var yPVT = new[]
            {
                new PVTPoint(0, 0, 0),
                new PVTPoint(500, 1500, 300),
                new PVTPoint(1000, 0, 0)
            };

            Console.WriteLine("\n执行多轴同步PVT运动...");
            group.MovePVTSync(new[] { xPVT, yPVT });
            group.WaitForComplete(5000);
            Console.WriteLine("✓ 多轴同步PVT运动完成");

            group.Disable();
            _groupManager.DeleteGroup(group.GroupId);

            LogMessage("多轴同步PVT运动测试完成");
            Console.WriteLine();
        }

        #endregion

        #region PTP规划器测试

        /// <summary>
        /// 测试PTP规划器
        /// </summary>
        private void TestPTPPlanner()
        {
            Console.WriteLine("【步骤8】PTP规划器测试...");
            Console.WriteLine("-----------------------------------------------");

            double[] maxSpeeds = { 10000, 10000, 5000 };
            double[] maxAccs = { 50000, 50000, 25000 };
            var planner = new PTPPlanner(3, maxSpeeds, maxAccs);

            double[] start = { 0, 0, 0 };
            double[] end = { 5000, 3000, 1000 };

            // 测试时间最优策略
            Console.WriteLine("\n时间最优策略:");
            planner.Strategy = PTPStrategy.TimeOptimal;
            var plan1 = planner.Plan(start, end, 100);
            Console.WriteLine($"  总时间: {plan1.TotalTime:F3}s");
            for (int i = 0; i < 3; i++)
            {
                var profile = plan1.AxisProfiles[i];
                Console.WriteLine($"  轴{i}: 距离={profile.Distance:F2}, 最大速度={profile.MaxSpeed:F2}, 时间={profile.TotalTime:F3}s");
            }

            // 测试同步策略
            Console.WriteLine("\n同步策略:");
            planner.Strategy = PTPStrategy.Synchronized;
            var plan2 = planner.Plan(start, end, 100);
            Console.WriteLine($"  总时间: {plan2.TotalTime:F3}s");
            for (int i = 0; i < 3; i++)
            {
                var profile = plan2.AxisProfiles[i];
                Console.WriteLine($"  轴{i}: 距离={profile.Distance:F2}, 最大速度={profile.MaxSpeed:F2}, 时间={profile.TotalTime:F3}s");
            }

            // 测试轨迹采样
            Console.WriteLine("\n轨迹采样（时间最优策略）:");
            for (double t = 0; t <= plan1.TotalTime; t += 0.1)
            {
                var positions = plan1.GetPositionsAtTime(t);
                Console.WriteLine($"  t={t:F2}s: X={positions[0]:F2}, Y={positions[1]:F2}, Z={positions[2]:F2}");
            }

            LogMessage("PTP规划器测试完成");
            Console.WriteLine();
        }

        #endregion

        #region PVT规划器测试

        /// <summary>
        /// 测试PVT规划器
        /// </summary>
        private void TestPVTPlanner()
        {
            Console.WriteLine("【步骤9】PVT规划器测试...");
            Console.WriteLine("-----------------------------------------------");

            double[] maxVels = { 10000 };
            double[] maxAccs = { 50000 };
            var planner = new PVTPlanner(1, maxVels, maxAccs);

            // 定义PVT关键点
            var keyPoints = new List<PVTPoint>
            {
                new PVTPoint(0, 0, 100),
                new PVTPoint(500, 3000, 200),
                new PVTPoint(1000, 2000, 200),
                new PVTPoint(1500, 0, 100)
            };

            // 测试不同插值方法
            var methods = new[]
            {
                PVTInterpolationMethod.Linear,
                PVTInterpolationMethod.CubicHermite,
                PVTInterpolationMethod.FifthOrder
            };

            foreach (var method in methods)
            {
                Console.WriteLine($"\n{method} 插值方法:");
                planner.InterpolationMethod = method;
                var plan = planner.PlanSingleAxis(keyPoints, 0);

                Console.WriteLine($"  总时间: {plan.TotalTime:F3}s");
                Console.WriteLine($"  采样点数: {plan.SampleCount}");

                // 显示前10个采样点
                Console.WriteLine("  前10个采样点:");
                for (int i = 0; i < Math.Min(10, plan.Trajectory.Count); i++)
                {
                    var pt = plan.Trajectory[i];
                    Console.WriteLine($"    {pt}");
                }
            }

            // 测试从路径生成PVT
            Console.WriteLine("\n从路径生成PVT:");
            double[] path = { 0, 500, 1000, 1500, 2000 };
            double[] times = { 100, 200, 200, 100 };
            var generatedPVT = planner.GeneratePVTFromPath(path, times, 0.5);

            Console.WriteLine("  生成的PVT点:");
            foreach (var pvt in generatedPVT)
            {
                Console.WriteLine($"    {pvt}");
            }

            // 测试S曲线轨迹生成
            Console.WriteLine("\nS曲线轨迹:");
            var sCurvePVT = planner.GenerateSCurveTrajectory(0, 2000, 5000, 25000, 100000, 10);
            Console.WriteLine($"  生成点数: {sCurvePVT.Count}");
            Console.WriteLine("  前5个点:");
            for (int i = 0; i < Math.Min(5, sCurvePVT.Count); i++)
            {
                Console.WriteLine($"    {sCurvePVT[i]}");
            }

            LogMessage("PVT规划器测试完成");
            Console.WriteLine();
        }

        #endregion

        #region 测试报告

        /// <summary>
        /// 输出测试报告
        /// </summary>
        private void PrintTestReport()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("              测试报告汇总");
            Console.WriteLine("=================================================");

            Console.WriteLine("\n【测试内容】");
            Console.WriteLine("  1. 轴组基本功能 - ✓");
            Console.WriteLine("  2. PTP点到点运动 - ✓");
            Console.WriteLine("  3. 线性插补（绝对/相对） - ✓");
            Console.WriteLine("  4. 圆弧插补（顺时针/逆时针） - ✓");
            Console.WriteLine("  5. PVT单轴运动 - ✓");
            Console.WriteLine("  6. 多轴同步PVT - ✓");
            Console.WriteLine("  7. PTP规划器（时间最优/同步策略） - ✓");
            Console.WriteLine("  8. PVT规划器（多种插值方法） - ✓");

            Console.WriteLine("\n【支持的功能】");
            Console.WriteLine("  - 轴组创建和管理");
            Console.WriteLine("  - XY/XYZ/XYZR标准轴组");
            Console.WriteLine("  - PTP运动（时间最优/速度最优/同步策略）");
            Console.WriteLine("  - 线性插补（2-8轴）");
            Console.WriteLine("  - 圆弧插补（XY平面）");
            Console.WriteLine("  - PVT模式（单轴/多轴同步）");
            Console.WriteLine("  - 线性/CubicHermite/五次多项式插值");
            Console.WriteLine("  - S曲线轨迹生成");

            Console.WriteLine("\n【API接口】");
            Console.WriteLine("  - IMotionCard.LinearInterpolation()");
            Console.WriteLine("  - IAxisGroup.MovePTP()");
            Console.WriteLine("  - IAxisGroup.MoveLinearAbs/Rel()");
            Console.WriteLine("  - IAxisGroup.MoveCircularAbs/Rel()");
            Console.WriteLine("  - IAxisGroup.MovePVT()");
            Console.WriteLine("  - IAxisGroup.MovePVTSync()");
            Console.WriteLine("  - PTPPlanner.Plan()");
            Console.WriteLine("  - PVTPlanner.PlanSingleAxis/MultiAxis()");

            Console.WriteLine("\n=================================================");
        }

        #endregion

        #region 辅助方法

        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}] {message}");
            try
            {
                _logRepo?.WriteInfo(message, LOGGER_NAME);
            }
            catch { }
        }

        private void LogError(string message, Exception ex)
        {
            try
            {
                _logRepo?.WriteException(ex, message, LOGGER_NAME);
            }
            catch { }
        }

        private void Cleanup()
        {
            Console.WriteLine("\n正在清理资源...");

            _groupManager?.Clear();

            for (int i = 0; i < 4; i++)
            {
                try { _motionCard?.SetServoEnable(i, false); } catch { }
            }

            _motionCard?.Close();
            _motionCard?.Dispose();
            _logDb?.Dispose();

            Console.WriteLine("资源清理完成");
        }

        #endregion
    }
}
