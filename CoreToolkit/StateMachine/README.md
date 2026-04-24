# StateMachine 状态机模块

提供流程模块编排与状态机管理，支持运动、IO、容器等不同类型的流程模块组合运行。内置到位检测和轨迹记录功能。

## 目录结构

```
StateMachine/
├── Core/
│   ├── FlowModuleBase.cs         # 流程模块基类（含 IProgress、异常保存、前后钩子）
│   ├── IFlowModule.cs            # 流程模块接口
│   └── StateMachineManager.cs    # 状态机管理器（含异步停止、完整异常保存）
├── Models/
│   ├── ModuleProgress.cs         # 模块进度报告模型
│   └── StateMachineModels.cs     # 状态机相关模型（线程安全 ExecutionContext）
├── Modules/
│   ├── ContainerModules.cs       # 容器流程模块（Sequential/Parallel/Conditional/Loop）
│   ├── IOModules.cs              # IO 流程模块
│   ├── MotionModules.cs          # 运动流程模块
│   ├── RetryModule.cs            # 重试模块（含退避策略）
│   └── TryCatchModule.cs         # 异常捕获模块（Try/Catch/Finally）
└── Monitors/
    ├── InPositionDetector.cs     # 到位检测器
    └── TrajectoryRecorder.cs     # 轨迹记录器（线程安全）
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `IFlowModule` | 流程模块接口：`Initialize/Execute/Reset`，属性 `Name/Status/TimeoutMs/IsParallel/Parent/Statistics` |
| `FlowModuleBase` | 流程模块基类，实现状态管理、超时控制、进度报告（`IProgress<ModuleProgress>`）、异常保存、前后执行钩子 |
| `StateMachineManager` | 状态机管理器：模块注册、顺序/并行执行、状态转换、异常回退、异步停止 |
| `SequentialModule` | 串行容器：支持断点续传（通过 `ExecutionContext.Results` 保存执行索引） |
| `ParallelModule` | 并行容器：同时执行所有子模块，一个失败取消其余 |
| `RetryModule` | 重试包装器：支持最大重试次数、固定/指数退避延迟 |
| `TryCatchModule` | 异常捕获：Try 失败 → Catch 备用 → Finally 清理 |
| `InPositionDetector` | 到位检测：监控轴实际位置与目标位置的偏差，判断是否在允许公差内 |
| `TrajectoryRecorder` | 轨迹记录：实时记录多轴位置序列，线程安全（`lock` 保护），用于回放与分析 |

## 线程安全改进

- **`ExecutionContext`**：`Parameters` / `Results` / `SharedData` 已改为 `ConcurrentDictionary<string, object>`，支持并行模块安全读写
- **`TrajectoryRecorder`**：`Record.Points` 读写通过 `_recordLock` 保护，避免后台记录与 UI 导出竞争
- **`FlowModuleBase.Cancel()`**：异常不再静默吞掉，记录到 `LastCancelException` 并通过 `Debug.WriteLine` 输出

## 异常处理增强

- `ModuleStatistics.Exception` — 保存完整异常对象（含 `StackTrace` 和 `InnerException`）
- `StateMachine.Exception` — 状态机级别完整异常保存
- `ModuleEventArgs.Exception` — 事件参数携带异常对象
- `FlowModuleBase` 新增 `OnBeforeExecuteAsync` / `OnAfterExecuteAsync` 虚方法钩子

## 进度报告

```csharp
// 使用 IProgress<ModuleProgress>
var module = new SequentialModule("MyFlow");
module.Progress = new Progress<ModuleProgress>(p =>
{
    Console.WriteLine($"[{p.ModuleName}] {p.StepName}: {p.OverallProgress:P0}");
});
```

## 断点续传

`SequentialModule` 自动在 `ExecutionContext.Results` 中保存执行进度。流程中断后重新执行，会从上次失败的步骤继续：

```csharp
var seq = new SequentialModule("MainFlow");
seq.AddModule(step1);
seq.AddModule(step2);
seq.AddModule(step3);

// 第一次执行，step2 失败
await seq.ExecuteAsync(context); // step1 完成，step2 失败

// 修复问题后再次执行，自动从 step2 开始
await seq.ExecuteAsync(context); // 从 step2 继续
```

## 使用示例

```csharp
// 创建状态机
var sm = new StateMachineManager();
var machine = sm.CreateMachine("PickAndPlace");

// 构建流程：带重试的轴移动 + IO 控制
var root = new SequentialModule("Root");
root.AddModule(new RetryModule(new AxisMoveModule("MoveToPick", motionCard))
{
    MaxRetries = 3,
    RetryDelayMs = 500,
    UseExponentialBackoff = true
});
root.AddModule(new IOOutputModule("VacuumOn", ioCard, outputId: 0, state: true));

// Try-Catch：尝试贴装，失败时执行回退
var placeWithFallback = new TryCatchModule(new AxisMoveModule("Place", motionCard));
placeWithFallback.SetCatchModule(new AxisMoveModule("Retreat", motionCard));
root.AddModule(placeWithFallback);

machine.SetRootModule(root);
var success = await machine.StartAsync();
```

## 依赖

- CoreToolkit.Motion
- CoreToolkit.Equipment
