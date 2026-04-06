using System;
using System.Threading.Tasks;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Interfaces;
using CoreToolkit.StateMachine.Core;
using CoreToolkit.StateMachine.Modules;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.Tests
{
    public class StateMachineTest
    {
        private readonly MockMotionCard _mockMotionCard;
        private readonly MockIOCard _mockIOCard;
        private readonly StateMachineManager _manager;

        public StateMachineTest()
        {
            _mockMotionCard = new MockMotionCard();
            _mockIOCard = new MockIOCard();
            _manager = new StateMachineManager();
        }

        public async Task RunAssemblyTestAsync()
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("    状态机组装测试");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            
            // 创建状态机
            var machine = _manager.CreateMachine("AssemblyProcess", "组装流程测试");
            
            // 创建串行容器作为根模块
            var rootModule = new SequentialModule("AssemblyRoot");
            machine.SetRootModule(rootModule);

            // === 流程 A: 上基板 ===
            var flowA = new SequentialModule("FlowA_上基板");
            
            // 1. 移动到上料位置
            var moveToLoad = new AxisMoveModule(_mockMotionCard, "移动到上料位置")
            {
                AxisIndices = new int[] { 0, 1 }, // X, Y 轴
                TargetPositions = new double[] { 1000, 500 },
                Velocities = new double[] { 5000, 5000 },
                Accelerations = new double[] { 100000, 100000 },
                Decelerations = new double[] { 100000, 100000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowA.AddModule(moveToLoad);

            // 2. 打开基板夹具
            var openClamp = new IOOutputModule(_mockIOCard, "打开基板夹具")
            {
                IoIndex = 0,
                OutputState = true,
                DelayAfterSetMs = 500
            };
            flowA.AddModule(openClamp);

            // 3. 等待基板到位
            var waitBoard = new IOInputModule(_mockIOCard, "等待基板到位")
            {
                IoIndex = 1,
                ExpectedState = true,
                TimeoutMs = 10000
            };
            flowA.AddModule(waitBoard);

            // 4. 关闭基板夹具
            var closeClamp = new IOOutputModule(_mockIOCard, "关闭基板夹具")
            {
                IoIndex = 0,
                OutputState = false,
                DelayAfterSetMs = 500
            };
            flowA.AddModule(closeClamp);

            // 5. 移动到组装位置
            var moveToAssembly = new AxisMoveModule(_mockMotionCard, "移动到组装位置")
            {
                AxisIndices = new int[] { 0, 1 },
                TargetPositions = new double[] { 2000, 1000 },
                Velocities = new double[] { 5000, 5000 },
                Accelerations = new double[] { 100000, 100000 },
                Decelerations = new double[] { 100000, 100000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowA.AddModule(moveToAssembly);

            // === 流程 B: 取芯片 A1 ===
            var flowB = new SequentialModule("FlowB_取芯片A1");

            // 1. 移动到芯片A1位置
            var moveToChipA1 = new AxisMoveModule(_mockMotionCard, "移动到芯片A1位置")
            {
                AxisIndices = new int[] { 0, 1, 2 }, // X, Y, Z 轴
                TargetPositions = new double[] { 500, 300, 0 },
                Velocities = new double[] { 5000, 5000, 2000 },
                Accelerations = new double[] { 100000, 100000, 50000 },
                Decelerations = new double[] { 100000, 100000, 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowB.AddModule(moveToChipA1);

            // 2. 下降吸取芯片
            var downToChip = new AxisMoveModule(_mockMotionCard, "下降吸取芯片")
            {
                AxisIndices = new int[] { 2 }, // Z 轴
                TargetPositions = new double[] { -10 },
                Velocities = new double[] { 1000 },
                Accelerations = new double[] { 20000 },
                Decelerations = new double[] { 20000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowB.AddModule(downToChip);

            // 3. 打开真空
            var openVacuum = new IOOutputModule(_mockIOCard, "打开真空")
            {
                IoIndex = 2,
                OutputState = true,
                DelayAfterSetMs = 300
            };
            flowB.AddModule(openVacuum);

            // 4. 上升
            var upFromChip = new AxisMoveModule(_mockMotionCard, "上升")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { 0 },
                Velocities = new double[] { 2000 },
                Accelerations = new double[] { 50000 },
                Decelerations = new double[] { 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowB.AddModule(upFromChip);

            // 5. 移动到组装位置
            var moveToAssemblyB = new AxisMoveModule(_mockMotionCard, "移动到组装位置")
            {
                AxisIndices = new int[] { 0, 1 },
                TargetPositions = new double[] { 2050, 1050 },
                Velocities = new double[] { 5000, 5000 },
                Accelerations = new double[] { 100000, 100000 },
                Decelerations = new double[] { 100000, 100000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowB.AddModule(moveToAssemblyB);

            // === 流程 C: 取芯片 B1 ===
            var flowC = new SequentialModule("FlowC_取芯片B1");

            // 1. 移动到芯片B1位置
            var moveToChipB1 = new AxisMoveModule(_mockMotionCard, "移动到芯片B1位置")
            {
                AxisIndices = new int[] { 0, 1, 2 },
                TargetPositions = new double[] { 800, 300, 0 },
                Velocities = new double[] { 5000, 5000, 2000 },
                Accelerations = new double[] { 100000, 100000, 50000 },
                Decelerations = new double[] { 100000, 100000, 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowC.AddModule(moveToChipB1);

            // 2. 下降吸取芯片
            var downToChipB = new AxisMoveModule(_mockMotionCard, "下降吸取芯片B1")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { -10 },
                Velocities = new double[] { 1000 },
                Accelerations = new double[] { 20000 },
                Decelerations = new double[] { 20000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowC.AddModule(downToChipB);

            // 3. 打开真空
            var openVacuumB = new IOOutputModule(_mockIOCard, "打开真空B")
            {
                IoIndex = 3,
                OutputState = true,
                DelayAfterSetMs = 300
            };
            flowC.AddModule(openVacuumB);

            // 4. 上升
            var upFromChipB = new AxisMoveModule(_mockMotionCard, "上升B")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { 0 },
                Velocities = new double[] { 2000 },
                Accelerations = new double[] { 50000 },
                Decelerations = new double[] { 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowC.AddModule(upFromChipB);

            // 5. 移动到组装位置
            var moveToAssemblyC = new AxisMoveModule(_mockMotionCard, "移动到组装位置C")
            {
                AxisIndices = new int[] { 0, 1 },
                TargetPositions = new double[] { 1950, 1050 },
                Velocities = new double[] { 5000, 5000 },
                Accelerations = new double[] { 100000, 100000 },
                Decelerations = new double[] { 100000, 100000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            flowC.AddModule(moveToAssemblyC);

            // === 最后：组装步骤 ===
            var assemblySteps = new SequentialModule("组装步骤");

            // 1. 芯片A1下降
            var placeChipA = new AxisMoveModule(_mockMotionCard, "芯片A1下降")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { -5 },
                Velocities = new double[] { 500 },
                Accelerations = new double[] { 10000 },
                Decelerations = new double[] { 10000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            assemblySteps.AddModule(placeChipA);

            // 2. 关闭真空A
            var closeVacuumA = new IOOutputModule(_mockIOCard, "关闭真空A")
            {
                IoIndex = 2,
                OutputState = false,
                DelayAfterSetMs = 200
            };
            assemblySteps.AddModule(closeVacuumA);

            // 3. 上升
            var upAfterPlaceA = new AxisMoveModule(_mockMotionCard, "上升")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { 0 },
                Velocities = new double[] { 2000 },
                Accelerations = new double[] { 50000 },
                Decelerations = new double[] { 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            assemblySteps.AddModule(upAfterPlaceA);

            // 4. 芯片B1下降
            var placeChipB = new AxisMoveModule(_mockMotionCard, "芯片B1下降")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { -5 },
                Velocities = new double[] { 500 },
                Accelerations = new double[] { 10000 },
                Decelerations = new double[] { 10000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            assemblySteps.AddModule(placeChipB);

            // 5. 关闭真空B
            var closeVacuumB = new IOOutputModule(_mockIOCard, "关闭真空B")
            {
                IoIndex = 3,
                OutputState = false,
                DelayAfterSetMs = 200
            };
            assemblySteps.AddModule(closeVacuumB);

            // 6. 上升
            var upAfterPlaceB = new AxisMoveModule(_mockMotionCard, "最终上升")
            {
                AxisIndices = new int[] { 2 },
                TargetPositions = new double[] { 0 },
                Velocities = new double[] { 2000 },
                Accelerations = new double[] { 50000 },
                Decelerations = new double[] { 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            assemblySteps.AddModule(upAfterPlaceB);

            // 7. 移动到原点
            var moveToHome = new AxisMoveModule(_mockMotionCard, "移动到原点")
            {
                AxisIndices = new int[] { 0, 1, 2 },
                TargetPositions = new double[] { 0, 0, 0 },
                Velocities = new double[] { 5000, 5000, 2000 },
                Accelerations = new double[] { 100000, 100000, 50000 },
                Decelerations = new double[] { 100000, 100000, 50000 },
                IsAbsolute = true,
                EnableInPositionCheck = true
            };
            assemblySteps.AddModule(moveToHome);

            // 8. 完成提示
            var finishStep = new CustomActionModule("完成")
            {
                TimeoutMs = 1000
            };
            finishStep.SetAction((context, token) =>
            {
                Console.WriteLine("🎉 组装流程完成！");
                return Task.FromResult(true);
            });
            assemblySteps.AddModule(finishStep);

            // === 构建完整流程 ===
            rootModule.AddModule(flowA); // 上基板
            
            // 并行执行 取芯片A1 和 取芯片B1
            var parallelModule = new ParallelModule("并行取芯片");
            parallelModule.AddModule(flowB); // 取芯片A1
            parallelModule.AddModule(flowC); // 取芯片B1
            rootModule.AddModule(parallelModule);
            
            rootModule.AddModule(assemblySteps); // 组装

            // 订阅事件
            machine.OnStatusChanged += (s, e) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 状态机 '{e.MachineName}' 状态: {e.OldStatus} → {e.NewStatus}");
                if (!string.IsNullOrEmpty(e.Message))
                {
                    Console.WriteLine($"  消息: {e.Message}");
                }
            };

            machine.OnModuleExecuting += (s, e) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 执行模块: {e.ModuleName} ({e.ModuleId})");
            };

            machine.OnModuleCompleted += (s, e) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 模块完成: {e.ModuleName} → {e.NewStatus} (耗时: {e.DurationMs:F1}ms)");
                if (!string.IsNullOrEmpty(e.ErrorMessage))
                {
                    Console.WriteLine($"  错误: {e.ErrorMessage}");
                }
            };

            // 启动状态机
            Console.WriteLine("开始执行组装流程...");
            Console.WriteLine();

            var context = new ExecutionContext();
            var success = await _manager.StartMachineAsync("AssemblyProcess", context);

            Console.WriteLine();
            Console.WriteLine("=====================================");
            Console.WriteLine($"流程执行结果: {(success ? "成功" : "失败")}");
            
            // 输出统计信息
            var stats = _manager.GetStatistics("AssemblyProcess");
            if (stats != null)
            {
                Console.WriteLine($"总耗时: {stats.TotalDuration.TotalSeconds:F2}秒");
                Console.WriteLine($"模块数量: {stats.ModuleStats.Count}");
                Console.WriteLine($"成功率: {(stats.IsSuccess ? "100%" : "0%")}");
            }

            Console.WriteLine("=====================================");
        }

        // 模拟运动控制卡
        private class MockMotionCard : IMotionCard
        {
            private readonly double[] _positions = new double[3]; // X, Y, Z

            public string CardName => "MockMotionCard";
            public string Vendor => "MockVendor";
            public string Model => "MockModel";
            public int CardId => 0;
            public int AxisCount => 3;
            public bool IsInitialized => true;
            public bool IsOpen => true;

            public void Initialize(MotionConfig config) { }
            public void Open() { }
            public void Close() { }
            public void Reset() { }

            public double GetPosition(int axis)
            {
                if (axis >= 0 && axis < _positions.Length)
                    return _positions[axis];
                return 0;
            }

            public void SetPosition(int axis, double position)
            {
                if (axis >= 0 && axis < _positions.Length)
                    _positions[axis] = position;
            }

            public void MoveAbsolute(int axis, double position, double speed) { }
            public void MoveRelative(int axis, double distance, double speed) { }
            public void JOG(int axis, double speed) { }
            public void Stop(int axis) { }
            public void StopAll() { }
            public void Home(int axis) { }
            public void HomeAll() { }
            public void SetServoEnable(int axis, bool enable) { }
            public void SetServoEnableAll(bool enable) { }
            public void SetVelocity(int axis, double velocity) { }
            public void SetAcceleration(int axis, double acceleration) { }
            public void SetDeceleration(int axis, double deceleration) { }
            public double GetVelocity(int axis) { return 0; }
            public double GetAcceleration(int axis) { return 0; }
            public double GetDeceleration(int axis) { return 0; }
            public int GetAxisStatus(int axis) { return 0; }
            public bool IsInPosition(int axis) { return true; }
            public bool IsMotionDone(int axis) { return true; }
            public void LinearInterpolation(double[] positions, double[] velocities, double[] accelerations) { }
            public void WaitForMotionComplete(int axis) { }
            public void WaitForMotionCompleteAll() { }
            public void SetHomeOffset(int axis, double offset) { }
            public double GetHomeOffset(int axis) { return 0; }
            public void SetSoftLimit(int axis, double min, double max) { }
            public (double, double) GetSoftLimit(int axis) { return (0, 0); }
            public void SetHardLimit(int axis, double min, double max) { }
            public (double, double) GetHardLimit(int axis) { return (0, 0); }
        }

        // 模拟IO卡
        private class MockIOCard : IIOCard
        {
            private readonly bool[] _outputs = new bool[4];

            public string CardName => "MockIOCard";
            public string Vendor => "MockVendor";
            public string Model => "MockModel";
            public int CardId => 0;
            public int InputCount => 4;
            public int OutputCount => 4;
            public bool IsInitialized => true;
            public bool IsOpen => true;

            public void Initialize(IoConfig config) { }
            public void Open() { }
            public void Close() { }
            public void Reset() { }

            public bool ReadInput(int port)
            {
                // 模拟输入：1号端口（基板到位）返回true
                if (port == 1) return true;
                return false;
            }

            public void WriteOutput(int port, bool state)
            {
                if (port >= 0 && port < _outputs.Length)
                    _outputs[port] = state;
            }

            public bool ReadOutput(int port)
            {
                if (port >= 0 && port < _outputs.Length)
                    return _outputs[port];
                return false;
            }

            public void WriteAllOutputs(bool state) { }
            public int ReadAllInputs() { return 0; }
            public void WriteAllOutputs(int value) { }
            public void SetDebounceTime(int port, int milliseconds) { }
            public int GetDebounceTime(int port) { return 0; }
        }
    }
}