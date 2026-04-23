# StateMachine 状态机模块

提供流程模块编排与状态机管理，支持运动、IO、容器等不同类型的流程模块组合运行。内置到位检测和轨迹记录功能。

## 目录结构

```
StateMachine/
├── Core/
│   ├── FlowModuleBase.cs         # 流程模块基类
│   ├── IFlowModule.cs            # 流程模块接口
│   └── StateMachineManager.cs    # 状态机管理器
├── Models/
│   └── StateMachineModels.cs     # 状态机相关模型
├── Modules/
│   ├── ContainerModules.cs       # 容器流程模块
│   ├── IOModules.cs              # IO 流程模块
│   └── MotionModules.cs          # 运动流程模块
└── Monitors/
    ├── InPositionDetector.cs     # 到位检测器
    └── TrajectoryRecorder.cs     # 轨迹记录器
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `IFlowModule` | 流程模块接口：`Initialize/Execute/Abort/Reset`，属性 `Name/Status/IsCompleted` |
| `FlowModuleBase` | 流程模块基类，实现状态管理、事件通知、异常处理 |
| `StateMachineManager` | 状态机管理器：模块注册、顺序/并行执行、状态转换、异常回退 |
| `MotionModules` | 运动相关流程模块：单轴移动、多轴插补、回原点 |
| `IOModules` | IO 流程模块：等待输入信号、设置输出信号、延时 |
| `ContainerModules` | 容器流程模块：托盘进出、轨道定位 |
| `InPositionDetector` | 到位检测：监控轴实际位置与目标位置的偏差，判断是否在允许公差内 |
| `TrajectoryRecorder` | 轨迹记录：实时记录多轴位置序列，用于回放与分析 |

## 使用示例

```csharp
// 创建状态机
var sm = new StateMachineManager();

// 添加流程模块
sm.RegisterModule(new MoveToModule(axisGroup, targetPosition: new[] { 1000.0, 500.0 }));
sm.RegisterModule(new WaitInputModule(ioCard, inputId: 0, expectedState: true, timeoutMs: 5000));
sm.RegisterModule(new SetOutputModule(ioCard, outputId: 1, state: true));

// 执行流程
sm.ExecuteAllAsync().ContinueWith(t =>
{
    if (t.IsCompletedSuccessfully)
        Console.WriteLine("流程执行完成");
    else
        Console.WriteLine($"流程异常: {t.Exception?.InnerException?.Message}");
});

// 轨迹记录
var recorder = new TrajectoryRecorder(motionCard, axisIndices: new[] { 0, 1, 2 }, sampleIntervalMs: 10);
recorder.StartRecording();
// ... 执行运动 ...
var trajectory = recorder.StopRecording();
```

## 依赖

- CoreToolkit.Motion
- CoreToolkit.Equipment
