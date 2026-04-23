# Motion 运动控制模块

多轴运动控制的核心模块，提供轴组管理、速度规划、运动卡抽象及硬件驱动实现。支持研华（Advantech）PCI-1285 / PCI-1203 和雷赛（Leadshine）DMC-E3000 等主流控制卡。

## 目录结构

```
Motion/
├── Core/
│   ├── AxisGroup.cs              # 轴组实现（多轴联动）
│   ├── AxisGroupManager.cs       # 轴组管理器
│   ├── AxisSpeedController.cs    # 单轴速度控制器
│   ├── AxisSpeedManager.cs       # 多轴速度管理器
│   ├── AxisSpeedProfile.cs       # 速度曲线（S曲线/T曲线）
│   ├── AxisStatus.cs             # 轴状态模型
│   ├── IAxisGroup.cs             # 轴组接口
│   ├── IMotionCard.cs            # 运动卡接口
│   ├── IOCardManager.cs          # IO 卡管理器
│   └── LTDMC.cs                  # 雷赛 DMC 库 P/Invoke 声明
├── Examples/
│   └── AxisSpeedExample.cs       # 速度控制使用示例
├── Factory/
│   ├── IOCardFactory.cs          # IO 卡工厂
│   └── MotionCardFactory.cs      # 运动卡工厂
├── Interfaces/
│   └── IIOCard.cs                # IO 卡接口
└── Providers/
    ├── Advantech/
    │   ├── AdvantechIOCard.cs    # 研华 IO 卡实现
    │   ├── AdvantechMotionCardBase.cs # 研华运动卡基类
    │   ├── PCI1203.cs            # PCI-1203 专用实现
    │   └── PCI1285.cs            # PCI-1285 专用实现
    └── Leadshine/
        └── DMCE3000.cs           # 雷赛 DMC-E3000 实现
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `IMotionCard` | 运动卡接口：`Open/Close/Reset/SetPosition/GetPosition/MoveAbs/MoveRel/Stop/StopAll/WaitDone` |
| `IAxisGroup` | 轴组接口：多轴直线插补、圆弧插补、同步启停 |
| `AxisSpeedProfile` | 速度曲线规划：梯形/S 曲线加减速，支持起始速度、目标速度、加减速时间 |
| `AxisSpeedController` | 单轴速度闭环控制，支持速度限制、平滑过渡 |
| `MotionCardFactory` | 工厂模式创建指定型号的运动卡实例 |
| `AxisStatus` | 轴实时状态：当前位置、速度、伺服使能、报警、正/负限位、原点信号 |
| `PCI1285` | 研华 PCI-1285 专用实现，封装 D2K-DMC 库调用 |

## 使用示例

```csharp
// 创建运动卡
var card = MotionCardFactory.Create("PCI1285", cardId: 0);
card.Open();

// 单轴绝对定位
card.SetServoEnable(0, true);
card.MoveAbs(axis: 0, position: 10000.0, speed: 5000, acc: 10000);
card.WaitDone(0);

// 轴组插补
var group = new AxisGroup(card, new[] { 0, 1, 2 });
group.MoveLinear(new[] { 10000.0, 5000.0, 2000.0 }, speed: 3000);

// 获取轴状态
var status = card.GetAxisStatus(0);
Console.WriteLine($"位置: {status.Position}, 伺服: {status.ServoOn}, 报警: {status.Alarm}");
```

## 依赖

- 研华/雷赛官方驱动库（`LTDMC.dll`、`D2K-DMC.dll` 等）
- CoreToolkit.Common
