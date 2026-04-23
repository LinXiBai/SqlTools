# CoreToolkit.Safety 安全防护模块文档

> 本文档详细说明 `CoreToolkit.Safety` 防撞击与安全监控模块的接口定义、使用方法和配置规范。

---

## 目录

- [概述](#概述)
- [五层防护模型](#五层防护模型)
- [核心接口速查](#核心接口速查)
- [模型层](#模型层)
- [碰撞检测引擎](#碰撞检测引擎)
- [软限位守卫](#软限位守卫)
- [互锁规则引擎](#互锁规则引擎)
- [安全运动控制器](#安全运动控制器)
- [后台安全监控](#后台安全监控)
- [双头防碰撞](#双头防碰撞)
- [状态机集成](#状态机集成)
- [配置文件](#配置文件)
- [快速开始](#快速开始)
- [性能优化建议](#性能优化建议)
- [常见问题](#常见问题)

---

## 概述

`CoreToolkit.Safety` 是面向工业自动化设备（贴片机、固晶机、CNC等）的**防撞击与安全监控模块**。它通过软件层面的多层防护机制，在设备运行前预判风险、运行中实时监控，最大限度避免机械碰撞、超限运动和违规操作导致的人身伤害与设备损坏。

### 命名空间

```csharp
using CoreToolkit.Safety.Core;      // 接口定义
using CoreToolkit.Safety.Helpers;   // 核心实现
using CoreToolkit.Safety.Models;    // 数据模型
using CoreToolkit.Safety.Modules;   // 状态机模块
```

---

## 五层防护模型

```
┌─────────────────────────────────────────────────────┐
│  Layer 5: 运动前碰撞预测（PreviewCollision）         │
│  SafeMotionController.PreMoveCheck                   │
├─────────────────────────────────────────────────────┤
│  Layer 4: 区域互锁 / 禁区（InterlockEngine）         │
│  安全门、真空、Z轴深度、伺服使能...                   │
├─────────────────────────────────────────────────────┤
│  Layer 3: 软限位（SoftLimitGuard）                   │
│  软件行程边界限制                                     │
├─────────────────────────────────────────────────────┤
│  Layer 2: 硬限位 / 伺服报警                          │
│  IMotionCard 硬件层保护                               │
├─────────────────────────────────────────────────────┤
│  Layer 1: 后台实时监控（SafetyMonitor）              │
│  100ms 周期动态检测，异常自动急停                      │
└─────────────────────────────────────────────────────┘
```

**设计原则**：预判优于止损，分层冗余，配置驱动，实时守护。

---

## 核心接口速查

| 类/接口 | 功能 | 典型场景 |
|---------|------|---------|
| `BoundingBox` | AABB 轴对齐包围盒 | 定义设备部件空间占用 |
| `SafetyVolume` | 安全体积定义 | 注册运动部件/静止障碍 |
| `CollisionDetector` | 碰撞检测引擎 | 两两相交检测、碰撞预览 |
| `SoftLimitGuard` | 软限位守卫 | 轴行程软件限制 |
| `InterlockEngine` | 互锁规则引擎 | 安全门、真空、深度保护 |
| `SafeMotionController` | 安全运动包装器 | 包装 IMotionCard 自动安全检查 |
| `SafetyMonitor` | 后台安全监控 | 独立线程实时守护 |
| `DualHeadAntiCollision` | 双头防碰撞 | 贴片机双头同梁保护 |
| `SafetyCheckModule` | 状态机安全检查 | 嵌入流程节点 |
| `SafeAxisMoveModule` | 安全轴运动模块 | 带防护的轴运动流程 |
| `SafetyConfigLoader` | 配置加载器 | JSON 配置读写 |
| `SafetySetupHelper` | 一键初始化助手 | 快速搭建安全体系 |

---

## 模型层

### BoundingBox（AABB 包围盒）

```csharp
public class BoundingBox
{
    public double MinX, MaxX, MinY, MaxY, MinZ, MaxZ;

    // 核心方法
    public bool Intersects(BoundingBox other);           // 相交判断
    public double GetMinimumDistance(BoundingBox other); // 最小间距
    public BoundingBox Translate(double x, double y, double z); // 平移
    public BoundingBox Inflate(double margin);           // 扩大（安全余量）
    public bool Contains(double x, double y, double z);  // 包含点
    public BoundingBox Clone();
}
```

**说明**：所有碰撞检测基于 AABB（Axis-Aligned Bounding Box），即各边平行于坐标轴的包围盒。对于旋转体或非轴对齐物体，需通过增大 `SafetyMargin` 补偿。

### SafetyVolume（安全体积）

```csharp
public class SafetyVolume
{
    public string Id { get; set; }
    public string Name { get; set; }           // 如"贴装头"、"基板治具"
    public VolumeType Type { get; set; }       // Dynamic / Static / Temporary
    public BoundingBox BoundingBox { get; set; } // 局部坐标系下的包围盒
    public double SafetyMargin { get; set; }   // 安全余量（mm），默认 2.0
    public bool IsActive { get; set; }         // 是否参与检测
    public int[] LinkedAxes { get; set; }      // Dynamic 类型关联的轴索引
    public double OffsetX, OffsetY, OffsetZ;  // 包围盒中心相对于轴位置的偏移

    // 便捷方法
    public BoundingBox GetInflatedBox();       // 返回带安全余量的包围盒
    public BoundingBox GetWorldBox(double axisX, double axisY, double axisZ);
}
```

**体积类型**：
- `Dynamic`：位置随轴运动变化（如贴装头、吸嘴）
- `Static`：位置固定不变（如机架、治具、料架）
- `Temporary`：临时禁区（如维护区域、安全门打开时的禁区）

### CollisionResult（碰撞结果）

```csharp
public class CollisionResult
{
    public bool IsSafe { get; set; }           // true = 无碰撞
    public string VolumeA { get; set; }        // 碰撞对 A 名称
    public string VolumeB { get; set; }        // 碰撞对 B 名称
    public (double X, double Y, double Z)? CollisionPoint { get; set; }
    public string Message { get; set; }
    public List<(string A, string B)> AllCollisions { get; set; }

    public static CollisionResult Success();
    public static CollisionResult Failure(string volumeA, string volumeB, string message);
}
```

### SoftLimitConfig（软限位配置）

```csharp
public class SoftLimitConfig
{
    public int AxisIndex { get; set; }
    public double PositiveLimit { get; set; } = double.MaxValue;
    public double NegativeLimit { get; set; } = double.MinValue;
    public bool Enabled { get; set; } = true;

    public bool IsInRange(double position);
}
```

### InterlockRule（互锁规则）

```csharp
public class InterlockRule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<bool> Condition { get; set; }       // 条件委托
    public InterlockAction Action { get; set; }     // 触发动作
    public string Message { get; set; }
    public bool Enabled { get; set; } = true;

    public bool Evaluate();
}
```

**动作类型优先级**（从高到低）：

| 动作 | 枚举值 | 说明 |
|------|--------|------|
| 急停 | `EmergencyStop` | 立即停止所有轴，最高优先级 |
| 减速停 | `DecelerateStop` | 减速停止 |
| 禁止运动 | `BlockMotion` | 阻止新运动指令执行 |
| 仅报警 | `AlarmOnly` | 记录日志，不干预运动 |

### MoveSafetyResult（运动安全检查结果）

```csharp
public class MoveSafetyResult
{
    public bool IsAllowed { get; set; }
    public string BlockReason { get; set; }
    public object TriggerSource { get; set; }

    public static MoveSafetyResult Allowed();
    public static MoveSafetyResult Blocked(string reason, object source);
}
```

---

## 碰撞检测引擎

### CollisionDetector

```csharp
public class CollisionDetector : ICollisionDetector
{
    // 体积管理
    public void RegisterVolume(SafetyVolume volume);
    public bool RemoveVolume(string volumeId);
    public IEnumerable<SafetyVolume> GetAllVolumes();
    public void Clear();

    // 动态体积位置更新
    public void UpdateAxisPosition(string volumeId, double x, double y, double z);
    public void UpdateAxisPositions(Dictionary<string, (double X, double Y, double Z)> positions);

    // 碰撞检测
    public CollisionResult CheckCollision();
    public CollisionResult PreviewCollision(string volumeId, double targetX, double targetY, double targetZ);

    // 辅助检测
    public bool IsPointInAnyVolume(double x, double y, double z, VolumeType? typeFilter);
    public double GetMinimumDistance(string volumeIdA, string volumeIdB);
}
```

**使用示例**：

```csharp
var detector = new CollisionDetector();

detector.RegisterVolume(new SafetyVolume
{
    Name = "贴装头",
    Type = VolumeType.Dynamic,
    BoundingBox = new BoundingBox { MinX = -10, MaxX = 10, MinY = -10, MaxY = 10, MinZ = -30, MaxZ = 5 },
    SafetyMargin = 2.0,
    LinkedAxes = new[] { 0, 1, 2 }
});

detector.RegisterVolume(new SafetyVolume
{
    Name = "基板治具",
    Type = VolumeType.Static,
    BoundingBox = new BoundingBox { MinX = 100, MaxX = 500, MinY = 50, MaxY = 450, MinZ = -5, MaxZ = 10 },
    SafetyMargin = 5.0
});

// 更新动态体积当前位置
detector.UpdateAxisPosition("贴装头", x: 150, y: 200, z: 10);

// 检测当前是否碰撞
var result = detector.CheckCollision();
if (!result.IsSafe)
{
    Console.WriteLine($"碰撞: {result.Message}");
}

// 预览：假设移动到目标位置是否会碰撞
var preview = detector.PreviewCollision("贴装头", targetX: 300, targetY: 250, targetZ: 15);
if (!preview.IsSafe)
    Console.WriteLine($"目标位置不安全: {preview.Message}");
```

---

## 软限位守卫

### SoftLimitGuard

```csharp
public class SoftLimitGuard
{
    public void SetLimit(int axisIndex, double positiveLimit, double negativeLimit, bool enabled = true);
    public void SetLimit(SoftLimitConfig config);
    public void SetLimits(IEnumerable<SoftLimitConfig> configs);
    public SoftLimitConfig GetLimit(int axisIndex);

    public MoveSafetyResult CheckPosition(int axisIndex, double targetPosition);
    public MoveSafetyResult CheckPositions(int[] axisIndices, double[] targetPositions);

    public List<SoftLimitConfig> GetAllLimits();
    public void EnableLimit(int axisIndex);
    public void DisableLimit(int axisIndex);
    public void Clear();
}
```

**使用示例**：

```csharp
var guard = new SoftLimitGuard();

guard.SetLimit(0, positiveLimit: 600, negativeLimit: 0);   // X轴
guard.SetLimit(1, positiveLimit: 500, negativeLimit: 0);   // Y轴
guard.SetLimit(2, positiveLimit: 50,  negativeLimit: 0);   // Z轴

var result = guard.CheckPosition(0, targetPosition: 650);
if (!result.IsAllowed)
    Console.WriteLine(result.BlockReason);  // "轴0目标位置 650.00 超出正向软限位 600.00"
```

---

## 互锁规则引擎

### InterlockEngine

```csharp
public class InterlockEngine
{
    public event EventHandler<InterlockRule> RuleTriggered;

    public void AddRule(InterlockRule rule);
    public bool RemoveRule(string ruleId);

    public InterlockEvaluationResult EvaluateAll();           // 评估所有规则
    public InterlockEvaluationResult EvaluateBeforeMotion();  // 仅评估运动相关规则

    public List<InterlockRule> GetAllRules();
    public void EnableRule(string ruleId);
    public void DisableRule(string ruleId);
    public void Clear();
}
```

### InterlockEvaluationResult

```csharp
public class InterlockEvaluationResult
{
    public bool IsSafe { get; set; }
    public List<InterlockRule> TriggeredRules { get; set; }
    public InterlockAction RecommendedAction { get; set; }
    public string BlockReason { get; set; }
}
```

**使用示例**：

```csharp
var engine = new InterlockEngine();

// 安全门互锁
engine.AddRule(new InterlockRule
{
    Name = "安全门互锁",
    Condition = () => motionCard.ReadInput(10),  // IO10=安全门信号
    Action = InterlockAction.BlockMotion,
    Message = "安全门已打开，禁止自动运行"
});

// Z轴下压保护
engine.AddRule(new InterlockRule
{
    Name = "Z轴下压保护",
    Condition = () => motionCard.GetPosition(2) > 45,
    Action = InterlockAction.EmergencyStop,
    Message = "Z轴超过最大安全下压深度"
});

// 伺服未使能检查
engine.AddRule(new InterlockRule
{
    Name = "伺服使能检查",
    Condition = () => !motionCard.GetServoEnable(0),
    Action = InterlockAction.BlockMotion,
    Message = "伺服未使能"
});

var result = engine.EvaluateBeforeMotion();
if (!result.IsSafe)
{
    Console.WriteLine($"互锁触发: {result.BlockReason}");
    // result.RecommendedAction = EmergencyStop / BlockMotion / ...
}
```

---

## 安全运动控制器

### SafeMotionController

```csharp
public class SafeMotionController : IDisposable
{
    public SafeMotionController(IMotionCard motionCard,
        ICollisionDetector collisionDetector = null,
        SoftLimitGuard softLimitGuard = null,
        InterlockEngine interlockEngine = null);

    // 属性
    public bool SafetyEnabled { get; set; } = true;   // 调试时可临时关闭（严禁发布时禁用）
    public IMotionCard MotionCard { get; }
    public ICollisionDetector CollisionDetector { get; }
    public SoftLimitGuard SoftLimitGuard { get; }
    public InterlockEngine InterlockEngine { get; }

    // 事件
    public event EventHandler<MoveSafetyResult> SafetyViolation;

    // 安全运动方法（自动执行 PreMoveCheck）
    public Result MoveAbsoluteSafe(int axis, double position, double speed);
    public Result MoveRelativeSafe(int axis, double distance, double speed);
    public Result MoveMultiAxisAbsoluteSafe(int[] axes, double[] positions, double[] speeds);
    public Result LinearInterpolationSafe(int[] axes, double[] positions, double speed);
    public Result JogSafe(int axis, int direction, double speed);

    // 急停
    public void EmergencyStop();
    public void EmergencyStop(int axis);

    // 位置更新（用于碰撞检测）
    public void UpdateDynamicVolume(string volumeId, double x, double y, double z);
    public void UpdateDynamicVolumes(Dictionary<string, (double X, double Y, double Z)> positions);

    // 碰撞检测
    public CollisionResult CheckCollision();

    // 预检查（可单独调用）
    public MoveSafetyResult PreMoveCheck(int axis, double targetPosition);
    public MoveSafetyResult PreMoveCheck(int[] axes, double[] targetPositions);
}
```

**三层检查顺序**：

```
PreMoveCheck
├── 1. 互锁检查 (InterlockEngine.EvaluateBeforeMotion)
├── 2. 软限位检查 (SoftLimitGuard.CheckPosition)
└── 3. 碰撞预测 (CollisionDetector.PreviewCollision)
    └── 任一失败 → MoveSafetyResult.Blocked(reason)
```

**使用示例**：

```csharp
var safe = new SafeMotionController(motionCard, detector, softLimit, interlock);

// 订阅安全违规事件
safe.SafetyViolation += (s, e) =>
{
    Console.WriteLine($"安全违规: {e.BlockReason}");
};

// 执行安全运动
var result = safe.MoveAbsoluteSafe(axis: 0, position: 300, speed: 5000);
if (result.IsFailure)
{
    // 运动被阻止，result.Message 包含具体原因
    Console.WriteLine($"运动失败: {result.Message}");
}
```

---

## 后台安全监控

### SafetyMonitor

```csharp
public class SafetyMonitor : IDisposable
{
    public SafetyMonitor(IMotionCard motionCard,
        ICollisionDetector collisionDetector,
        InterlockEngine interlockEngine = null);

    // 配置
    public int IntervalMs { get; set; } = 100;
    public bool IsRunning { get; }
    public bool IsCurrentlySafe { get; private set; }

    // 事件
    public event EventHandler<CollisionResult> CollisionDetected;
    public event EventHandler<InterlockEvaluationResult> InterlockTriggered;
    public event EventHandler<bool> SafetyStatusChanged;

    // 动态体积映射注册
    public void RegisterVolumeAxisMapping(string volumeId, params int[] axisIndices);

    // 控制
    public void Start();
    public void Stop();
}
```

**监控循环逻辑**：

```
每 IntervalMs 毫秒执行一次：
1. 读取各轴当前位置
2. 更新所有 Dynamic 体积的世界坐标
3. 执行碰撞检测 CheckCollision()
   └── 发现碰撞 → 触发 CollisionDetected 事件 → 自动 StopAll(true)
4. 执行互锁评估 EvaluateAll()
   └── 发现触发 → 触发 InterlockTriggered 事件 → 按 RecommendedAction 执行
5. 更新总体安全状态 IsCurrentlySafe
   └── 状态变化 → 触发 SafetyStatusChanged 事件
```

**使用示例**：

```csharp
var monitor = new SafetyMonitor(motionCard, detector, interlock)
{
    IntervalMs = 100  // 100ms 检测周期
};

// 注册体积与轴的映射（告诉监控器如何更新动态体积）
monitor.RegisterVolumeAxisMapping("贴装头", 0, 1, 2);

// 事件订阅
monitor.CollisionDetected += (s, e) =>
{
    Console.WriteLine($"🚨 碰撞: {e.Message}");
};
monitor.InterlockTriggered += (s, e) =>
{
    Console.WriteLine($"⚠️ 互锁: {e.BlockReason}");
};
monitor.SafetyStatusChanged += (s, isSafe) =>
{
    Console.WriteLine($"安全状态: {(isSafe ? "✅ 安全" : "❌ 异常")}");
};

monitor.Start();

// 关闭时
monitor.Dispose();
```

---

## 双头防碰撞

### DualHeadAntiCollision

```csharp
public class DualHeadAntiCollision
{
    public DualHeadAntiCollision(IMotionCard motionCard, int headAAxis, int headBAxis);

    public double MinSeparation { get; set; } = 50.0;    // 最小安全间距
    public bool Enabled { get; set; } = true;
    public bool PreventCrossing { get; set; } = true;    // 是否禁止交叉过头

    public double HeadAPosition { get; }
    public double HeadBPosition { get; }
    public double CurrentSeparation { get; }

    // 移动可行性检查
    public MoveSafetyResult CanMoveHeadA(double targetPosition);
    public MoveSafetyResult CanMoveHeadB(double targetPosition);

    // 安全移动
    public bool MoveHeadASafe(double targetPosition, double speed);
    public bool MoveHeadBSafe(double targetPosition, double speed);
    public bool MoveBothSafe(double headATarget, double headBTarget, double speed);

    // 安全范围查询
    public (double MinSafe, double MaxSafe) GetSafeRangeForHeadA();
    public (double MinSafe, double MaxSafe) GetSafeRangeForHeadB();
}
```

**使用示例**：

```csharp
var dualHead = new DualHeadAntiCollision(motionCard, headAAxis: 0, headBAxis: 1)
{
    MinSeparation = 50.0,
    PreventCrossing = true  // 头A必须在头B左侧
};

// 检查可行性
var check = dualHead.CanMoveHeadA(targetPosition: 200);
if (!check.IsAllowed)
    Console.WriteLine(check.BlockReason);

// 安全移动
dualHead.MoveHeadASafe(targetPosition: 100, speed: 5000);
dualHead.MoveHeadBSafe(targetPosition: 200, speed: 5000);

// 查询安全范围
var (min, max) = dualHead.GetSafeRangeForHeadA();
Console.WriteLine($"头A可移动: {min:F1} ~ {max:F1}");
```

---

## 状态机集成

### SafetyCheckModule

```csharp
public class SafetyCheckModule : FlowModuleBase
{
    public SafetyCheckModule(ICollisionDetector collisionDetector,
        InterlockEngine interlockEngine = null, string name = null);

    public SafetyCheckModule(ICollisionDetector collisionDetector,
        InterlockEngine interlockEngine, SafetyCheckMode checkMode, string name = null);

    public bool TriggerEmergencyStopOnFailure { get; set; } = true;
    public string FailureMessage { get; private set; }
}

public enum SafetyCheckMode
{
    FullCheck,       // 碰撞 + 互锁
    CollisionOnly,   // 仅碰撞
    InterlockOnly    // 仅互锁
}
```

### SafeAxisMoveModule

```csharp
public class SafeAxisMoveModule : FlowModuleBase
{
    // 使用外部 SafeMotionController
    public SafeAxisMoveModule(SafeMotionController safeController, string name = null);

    // 或独立创建
    public SafeAxisMoveModule(IMotionCard motionCard,
        ICollisionDetector collisionDetector = null,
        SoftLimitGuard softLimitGuard = null,
        InterlockEngine interlockEngine = null,
        string name = null);

    public int[] AxisIndices { get; set; }
    public double[] TargetPositions { get; set; }
    public double[] Velocities { get; set; }
    public bool IsAbsolute { get; set; } = true;
    public bool EnableSafetyCheck { get; set; } = true;
    // ... 其他属性与 AxisMoveModule 相同
}
```

**使用示例**：

```csharp
var root = new SequentialModule("贴装流程");

// 安全检查节点
root.AddModule(new SafetyCheckModule(
    safe.CollisionDetector, safe.InterlockEngine, name: "运动前安全检查"));

// 带安全防护的轴运动
root.AddModule(new SafeAxisMoveModule(safe, "移动到取料位")
{
    AxisIndices = new[] { 0, 1, 2 },
    TargetPositions = new[] { 100.0, 200.0, 15.0 },
    Velocities = new[] { 5000.0, 5000.0, 3000.0 },
    IsAbsolute = true
});

// 运动后确认
root.AddModule(new SafetyCheckModule(
    safe.CollisionDetector, name: "运动后确认"));
```

---

## 配置文件

### SafetyConfig（JSON 模型）

```csharp
public class SafetyConfig
{
    public List<SoftLimitConfig> SoftLimits { get; set; }
    public List<SafetyVolume> SafetyVolumes { get; set; }
    public List<InterlockRule> InterlockRules { get; set; }
    public int MonitorIntervalMs { get; set; } = 100;
    public bool EnableBackgroundMonitor { get; set; } = true;
    public double DualHeadMinSeparation { get; set; } = 50.0;
    public double ZAxisMaxSafeDepth { get; set; } = 50.0;
}
```

### 配置示例（JSON）

```json
{
  "MonitorIntervalMs": 100,
  "EnableBackgroundMonitor": true,
  "DualHeadMinSeparation": 50.0,
  "ZAxisMaxSafeDepth": 50.0,
  "SoftLimits": [
    { "AxisIndex": 0, "PositiveLimit": 600, "NegativeLimit": 0, "Enabled": true },
    { "AxisIndex": 1, "PositiveLimit": 500, "NegativeLimit": 0, "Enabled": true },
    { "AxisIndex": 2, "PositiveLimit": 50,  "NegativeLimit": 0, "Enabled": true }
  ],
  "SafetyVolumes": [
    {
      "Name": "贴装头",
      "Type": "Dynamic",
      "BoundingBox": { "MinX": -10, "MaxX": 10, "MinY": -10, "MaxY": 10, "MinZ": -30, "MaxZ": 5 },
      "SafetyMargin": 2.0,
      "LinkedAxes": [0, 1, 2],
      "OffsetX": 0, "OffsetY": 0, "OffsetZ": 0,
      "IsActive": true
    },
    {
      "Name": "基板治具",
      "Type": "Static",
      "BoundingBox": { "MinX": 100, "MaxX": 500, "MinY": 50, "MaxY": 450, "MinZ": -5, "MaxZ": 10 },
      "SafetyMargin": 5.0,
      "IsActive": true
    }
  ],
  "InterlockRules": [
    {
      "Name": "Z轴下压保护",
      "ConditionExpression": "Z.Position > 45",
      "Action": "EmergencyStop",
      "Message": "Z轴超过最大安全下压深度",
      "Enabled": true
    }
  ]
}
```

### 配置加载/保存

```csharp
// 加载
var config = SafetyConfigLoader.LoadFromFile(@"Config\safety.json");
var config2 = SafetyConfigLoader.LoadFromJson(jsonString);

// 创建默认配置
var defaultConfig = SafetyConfigLoader.CreateDefaultConfig();
var smtConfig = SafetyConfigLoader.CreateSmtDefaultConfig();

// 保存
SafetyConfigLoader.SaveToFile(config, @"Config\safety.json");
```

---

## 快速开始

### 方式一：一键搭建（推荐）

```csharp
var motionCard = MotionCardFactory.CreateCard("PCI1285");
motionCard.Initialize(new MotionConfig { CardId = 0 });
motionCard.Open();

// 一键创建完整安全系统
var safe = SafetySetupHelper.CreateSafeMotionSystem(motionCard);

// 启动后台监控
var monitor = SafetySetupHelper.CreateAndStartMonitor(
    motionCard, safe.CollisionDetector, safe.InterlockEngine);

// 执行安全运动
var result = safe.MoveAbsoluteSafe(0, position: 300, speed: 5000);
```

### 方式二：手动配置

```csharp
var detector = new CollisionDetector();
var softLimit = new SoftLimitGuard();
var interlock = new InterlockEngine();

// 配置各组件...
// （详见上方各章节示例）

var safe = new SafeMotionController(motionCard, detector, softLimit, interlock);
```

### 方式三：从配置文件加载

```csharp
var config = SafetyConfigLoader.LoadFromFile(@"Config\safety.json");
var safe = SafetySetupHelper.CreateSafeMotionSystem(motionCard, config);
```

---

## 性能优化建议

| 体积数量 | 建议算法 | 说明 |
|---------|---------|------|
| < 20 | `O(N^2)` 两两检测 | 现有实现，无需改动 |
| 20 ~ 50 | 两两检测 + 空间预过滤 | 按坐标范围快速排除不相交对 |
| > 50 | **八叉树 (Octree)** | 将空间递归分割，复杂度降为 `O(N log N)` |
| > 100 | **均匀网格 (Uniform Grid)** | 适合动态物体密集场景 |

**监控周期建议**：
- 自动运行模式：≤ 100ms
- 手动 / JOG 模式：≤ 50ms
- 高速精密设备：≤ 20ms

**线程安全**：
- `CollisionDetector`、`SoftLimitGuard`、`InterlockEngine` 内部均使用 `lock` 保证线程安全
- `SafetyMonitor` 在独立后台线程运行，与主线程互不阻塞
- `SafeMotionController` 的运动前检查是同步的，不会引入显著延迟（通常 < 1ms）

---

## 常见问题

**Q1: 为什么碰撞检测使用 AABB 而不是更精确的 OBB 或 GJK？**

A: AABB 计算简单、性能高，适合工业设备中大量轴对齐运动的场景。对于旋转体（如旋转的 R 轴），可通过增大 `SafetyMargin` 补偿。若确实需要高精度（如复杂多关节机器人），可在 `CollisionDetector` 中扩展 `OBB` 或 `GJK` 检测算法。

**Q2: `SafetyEnabled = false` 可以在生产环境使用吗？**

A: **绝对禁止**。该属性仅在调试阶段（如排查是否为安全模块误拦截）临时关闭，正式发布时必须保持 `true`。建议在代码中增加编译期断言：

```csharp
#if !DEBUG
    if (!safeController.SafetyEnabled)
        throw new InvalidOperationException("生产环境严禁禁用安全防护");
#endif
```

**Q3: 互锁规则的 `Condition` 委托中可以执行耗时操作吗？**

A: **不可以**。`Condition` 委托在 `SafetyMonitor` 后台线程中每 100ms 执行一次，若阻塞会导致监控延迟。所有 IO 读取、轴位置获取应在委托执行前缓存到局部变量：

```csharp
// ✅ 正确：先读取，再闭包
bool isDoorOpen = motionCard.ReadInput(10);
engine.AddRule(new InterlockRule
{
    Condition = () => isDoorOpen,  // 闭包捕获的是快照值
    // ...
});

// ❌ 错误：委托中直接读取 IO
engine.AddRule(new InterlockRule
{
    Condition = () => motionCard.ReadInput(10),  // 每100ms调用一次
    // ...
});
```

**Q4: 双头防碰撞的 `PreventCrossing = false` 时有什么风险？**

A: 允许双头交叉意味着两个头可以越过对方，此时仅靠 `MinSeparation` 控制最小间距。若机械结构不允许交叉（如共享导轨、电缆长度限制），强行交叉会导致**物理碰撞**或**线缆拉扯**。该选项仅在有特殊工艺需求（如双头可独立升降避免干涉）且经过风险评估后开启。

**Q5: 临时禁区 `Temporary` 体积如何使用？**

A: 典型场景是维护模式或安全门打开时动态添加禁区：

```csharp
// 安全门打开时添加临时禁区
var doorZone = new SafetyVolume
{
    Name = "安全门区域",
    Type = VolumeType.Temporary,
    BoundingBox = new BoundingBox { MinX = 0, MaxX = 100, MinY = 0, MaxY = 500, MinZ = 0, MaxZ = 200 },
    IsActive = true
};
detector.RegisterVolume(doorZone);

// 安全门关闭后移除
detector.RemoveVolume(doorZone.Id);
```

**Q6: 碰撞检测报警后如何恢复？**

A: 流程建议：
1. `SafetyMonitor` 自动执行急停 → 设备停止
2. 操作员排查报警原因（如异物、参数错误）
3. 清除异常状态（移开异物、修正参数）
4. 手动 JOG 将设备移动到安全位置
5. 复位安全状态，重新启动自动流程

---

## 版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0.0 | 2026-04-21 | 初始版本：AABB碰撞检测、软限位、互锁引擎、安全运动控制器、后台监控、双头防碰撞、状态机集成 |
