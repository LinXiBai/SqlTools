# Algorithm 算法模块

提供工业自动化场景常用的几何计算与补偿算法。

## 目录结构

```
Algorithm/
├── Helpers/
│   ├── CompensationHelper.cs    # 补偿计算（螺距补偿、反向间隙补偿）
│   └── GeometryHelper.cs        # 几何辅助（距离、角度、交点）
└── Models/
    └── Point2D.cs               # 二维坐标点
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `Point2D` | 二维平面点 `(X, Y)`，支持距离计算、向量运算 |
| `GeometryHelper` | 几何工具：点到线距离、两线交点、角度计算、三点圆弧拟合 |
| `CompensationHelper` | 螺距误差补偿、反向间隙补偿、热膨胀补偿计算 |

## 使用示例

```csharp
// 二维点运算
var p1 = new Point2D(100.0, 200.0);
var p2 = new Point2D(150.0, 250.0);
double dist = p1.DistanceTo(p2);

// 螺距补偿
var comp = new CompensationHelper(pitchErrorTable);
double compensated = comp.CompensatePosition(axisPosition, direction);
```

## 依赖

- 无外部依赖
