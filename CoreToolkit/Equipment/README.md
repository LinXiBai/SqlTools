# Equipment 设备抽象模块

贴片机/自动化设备的核心部件抽象层，定义了供料器、加热器、吸嘴等设备的统一接口与数据模型。

## 目录结构

```
Equipment/
├── Core/
│   ├── IFeeder.cs               # 供料器接口
│   ├── IHeater.cs               # 加热器接口
│   └── INozzle.cs               # 吸嘴接口
├── Helpers/
│   ├── FeederHelper.cs          # 供料器控制辅助
│   └── NozzleHelper.cs          # 吸嘴控制辅助
└── Models/
    ├── FeederInfo.cs            # 供料器信息
    ├── NozzleInfo.cs            # 吸嘴信息
    ├── PlacementResult.cs       # 贴装结果
    └── StationInfo.cs           # 贴装工位信息
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `IFeeder` | 供料器接口：`Advance/Reset/Peel`，属性 `FeederId/IsReady` |
| `INozzle` | 吸嘴接口：`Pick/Place/Blow/SetVacuum/CheckVacuumSensor` |
| `IHeater` | 加热器接口：`StartHeat/StopHeat`，属性 `CurrentTemperature/TargetTemperature` |
| `PlacementResult` | 单次贴装结果：目标位置、实际位置、偏差 DeltaX/Y、旋转角度、是否成功 |
| `FeederInfo` | 供料器元数据：名称、型号、Pitch、总料数、剩余料数 |
| `StationInfo` | 贴装工位信息：名称、坐标(X,Y,Z)、关联 FeederId、元件代码 |

## 使用示例

```csharp
// 吸嘴操作
public void PickComponent(INozzle nozzle, StationInfo station)
{
    nozzle.SetVacuum(80);
    nozzle.Pick();
    if (!nozzle.CheckVacuumSensor())
        throw new InvalidOperationException("真空检测失败，可能未吸到元件");
}

// 供料器进料
feeder.Advance();
if (!feeder.IsReady)
    throw new InvalidOperationException($"供料器 {feeder.FeederId} 未就绪");

// 记录贴装结果
var result = new PlacementResult
{
    StationId = station.StationId,
    TargetX = station.PositionX,
    TargetY = station.PositionY,
    ActualX = actualX,
    ActualY = actualY,
    IsSuccess = Math.Abs(deltaX) < 0.05 && Math.Abs(deltaY) < 0.05
};
```

## 依赖

- 无外部依赖（纯接口与模型）
