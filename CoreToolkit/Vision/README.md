# Vision 视觉模块

提供相机抽象、标定辅助和坐标变换功能，支持工业视觉系统中的图像采集、像素-物理坐标转换、匹配结果处理等通用操作。

## 目录结构

```
Vision/
├── Core/
│   └── ICamera.cs               # 相机接口
├── Helpers/
│   ├── CalibrationHelper.cs     # 相机标定辅助
│   └── CoordinateTransform.cs   # 坐标变换（像素↔物理）
└── Models/
    ├── CalibrationData.cs       # 标定数据
    ├── MatchResult.cs           # 模板匹配结果
    └── VisionResult.cs          # 视觉处理通用结果
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `ICamera` | 相机接口：`Open/Close/Trigger/GrabImage/SetExposure/SetGain`，属性 `Resolution/IsConnected` |
| `CalibrationHelper` | 标定辅助：棋盘格标定、九点标定、畸变校正参数计算 |
| `CoordinateTransform` | 坐标变换：基于标定矩阵将像素坐标 `(u,v)` 转换为物理坐标 `(x,y)`，支持旋转补偿 |
| `MatchResult` | 模板匹配结果：中心坐标、旋转角度、匹配分数、是否成功 |
| `VisionResult<T>` | 视觉处理通用结果封装，继承 `Result<T>` |

## 使用示例

```csharp
// 相机操作
var camera = CameraFactory.Create("GigE", ip: "192.168.1.100");
camera.Open();
camera.SetExposure(5000); // 微秒
camera.SetGain(10);
var image = camera.GrabImage();

// 坐标变换（九点标定后）
var transform = new CoordinateTransform(calibrationData);
var physicalPos = transform.PixelToPhysical(pixelU: 512, pixelV: 384);
Console.WriteLine($"物理坐标: ({physicalPos.X:F3}, {physicalPos.Y:F3})");

// 标定辅助
var calib = new CalibrationHelper();
var calibData = calib.PerformNinePointCalibration(pixelPoints, physicalPoints);
```

## 依赖

- CoreToolkit.Common
- System.Drawing（图像基础操作）
