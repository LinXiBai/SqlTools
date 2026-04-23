# Safety 安全防护模块

为贴片机及多轴运动设备提供碰撞检测、互锁规则、软限位、急停控制等完整的安全防护体系。采用分层防护架构，从底层硬件保护到上层逻辑预测全覆盖。

## 目录结构

```
Safety/
├── Core/
│   └── ICollisionDetector.cs    # 碰撞检测器接口
├── Helpers/
│   ├── CollisionDetector.cs     # AABB 碰撞检测引擎
│   ├── DualHeadAntiCollision.cs # 双头防撞逻辑
│   ├── InterlockEngine.cs       # 互锁规则引擎
│   ├── SafeMotionController.cs  # 安全运动控制器
│   ├── SafetyConfigLoader.cs    # 安全配置文件加载
│   ├── SafetyMonitor.cs         # 后台安全监控循环
│   ├── SafetySetupHelper.cs     # 安全系统快速初始化
│   └── SoftLimitGuard.cs        # 软限位守卫
├── Models/
│   └── SafetyModels.cs          # 安全体积、互锁规则等模型
└── Modules/
    ├── SafeAxisMoveModule.cs    # 安全轴移动模块
    └── SafetyCheckModule.cs     # 安全检查模块
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `CollisionDetector` | AABB 轴对齐包围盒碰撞检测，支持 Static/Dynamic/Temporary 三种体积类型 |
| `SafeMotionController` | 预移动检查（PreMoveCheck）+ 实时碰撞预测，防止运动指令导致碰撞 |
| `InterlockEngine` | 互锁规则引擎：条件-动作规则，支持 AND/OR 组合条件 |
| `SoftLimitGuard` | 软限位守卫：在软件层面限制各轴运动范围 |
| `SafetyMonitor` | 100ms 周期的后台监控循环：更新动态体积 → 检测碰撞 → 评估互锁 → 触发急停 |
| `DualHeadAntiCollision` | 双贴装头之间的最小安全间距保护 |
| `SafetySetupHelper` | 一键初始化完整安全系统（体积注册 + 互锁 + 软限位 + 双头防撞） |

## 五层防护架构

```
Layer 5: 碰撞预测 (CollisionDetector + SafeMotionController.PreMoveCheck)
Layer 4: 互锁规则 (InterlockEngine)
Layer 3: 软限位   (SoftLimitGuard)
Layer 2: 硬限位   (硬件限位开关)
Layer 1: 伺服保护 (驱动器过流/过速保护)
```

## 使用示例

```csharp
// 快速初始化安全系统
var safeCtrl = SafetySetupHelper.CreateSafeMotionSystem(motionCard);

// 注册安全体积
safeCtrl.CollisionDetector.RegisterVolume(new SafetyVolume
{
    Id = "贴装头", Name = "贴装头", Type = VolumeType.Dynamic,
    MinX = -10, MaxX = 10, MinY = -10, MaxY = 10, MinZ = -5, MaxZ = 5
});

// 启动安全监控
var monitor = new SafetyMonitor(motionCard, safeCtrl.CollisionDetector, safeCtrl.InterlockEngine);
monitor.CollisionDetected += (s, e) => Console.WriteLine($"碰撞! {e.VolumeA} vs {e.VolumeB}");
monitor.Start();

// 预移动检查
bool safe = safeCtrl.PreMoveCheck(axis: 0, targetPosition: 5000.0);
if (!safe) Console.WriteLine("目标位置不安全，运动被阻止");
```

## 依赖

- CoreToolkit.Motion
- CoreToolkit.Data（日志记录）
