# SqlDemo / CoreToolkit

基于 **.NET Framework 4.7.2** 的工业软件开发工具集，整合了 **SQLite 数据库访问层（DAL）**、**运动控制卡抽象层** 和 **WPF MVVM 桌面辅助库**，采用模块化设计，便于在工作中持续扩展功能。

***

## 技术栈

| 技术/包                    | 版本      | 说明                          |
| :---------------------- | :------ | :---------------------------- |
| .NET Framework          | 4.7.2   | 目标运行框架                    |
| System.Data.SQLite.Core | 1.0.118 | SQLite ADO.NET 驱动           |
| Dapper                  | 2.1.28  | 轻量级 ORM，简化对象映射          |
| Newtonsoft.Json         | 13.0.3  | JSON 序列化/反序列化            |
| WPF                     | -       | 用户界面框架                    |

***

## 项目结构

```
SqlDemo/
├── SqlDemo.sln                         # 统一解决方案文件
├── README.md                           # 本文件
├── CoreToolkit/                        # 核心 DLL（数据库 + 运动控制 + 通用工具）
│   ├── CoreToolkit.csproj
│   ├── Common/                          # 通用工具类（非业务相关）
│   │   ├── Helpers/
│   │   │   ├── Guard.cs                 # 参数校验守卫
│   │   │   ├── EnumHelper.cs            # 枚举辅助
│   │   │   ├── JsonHelper.cs            # JSON 辅助
│   │   │   ├── RetryHelper.cs           # 重试机制
│   │   │   ├── DateTimeHelper.cs        # 时间辅助
│   │   │   ├── StringHelper.cs          # 字符串辅助
│   │   │   └── MathHelper.cs            # 数学辅助
│   │   └── Models/
│   │       └── Result.cs                # 通用操作结果封装
│   ├── Data/                            # 数据访问层
│   │   ├── Database/
│   │   │   ├── SqliteDbContext.cs       # 数据库上下文
│   │   │   ├── RepositoryBase.cs        # 泛型仓储基类
│   │   │   ├── DatabaseConfig.cs        # 数据库配置管理
│   │   │   ├── DatabaseFactory.cs       # 数据库工厂
│   │   │   ├── UserRepository.cs        # 用户仓储
│   │   │   ├── LogRepository.cs         # 日志仓储
│   │   │   ├── AxisParameterRepository.cs
│   │   │   └── IOParameterRepository.cs
│   │   └── Models/
│   │       ├── EntityBase.cs            # 实体基类
│   │       ├── Attributes.cs            # 自定义特性
│   │       ├── User.cs
│   │       ├── Log.cs
│   │       ├── AxisParameter.cs
│   │       └── IOParameter.cs
│   ├── Motion/                          # 运动控制层
│   │   ├── Core/                        # 核心接口与实现
│   │   │   ├── IMotionCard.cs
│   │   │   ├── MotionConfig.cs
│   │   │   ├── AxisStatus.cs
│   │   │   ├── AxisSpeedController.cs
│   │   │   ├── IOCardManager.cs
│   │   │   └── LTDMC.cs
│   │   ├── Factory/
│   │   │   ├── MotionCardFactory.cs
│   │   │   └── IOCardFactory.cs
│   │   ├── Interfaces/
│   │   │   └── IIOCard.cs
│   │   ├── Providers/
│   │   │   ├── Advantech/
│   │   │   │   ├── PCI1203.cs
│   │   │   │   ├── PCI1285.cs
│   │   │   │   └── AdvantechIOCard.cs
│   │   │   └── Leadshine/
│   │   │       └── DMCE3000.cs
│   │   └── Examples/
│   │       └── AxisSpeedExample.cs
│   ├── Vision/                          # 机器视觉层
│   │   ├── Core/
│   │   │   └── ICamera.cs               # 工业相机接口抽象
│   │   ├── Helpers/
│   │   │   ├── CalibrationHelper.cs     # 九点标定、旋转中心计算
│   │   │   └── CoordinateTransform.cs   # 像素坐标与机械坐标转换
│   │   └── Models/
│   │       ├── VisionResult.cs          # 视觉结果基类
│   │       ├── MatchResult.cs           # 模板匹配结果
│   │       └── CalibrationData.cs       # 标定点数据
│   ├── Communication/                   # 通信层
│   │   ├── Core/
│   │   │   ├── ISerialPort.cs           # 串口接口
│   │   │   └── ITcpClient.cs            # TCP 客户端接口
│   │   ├── Helpers/
│   │   │   ├── SerialPortHelper.cs      # 基础串口实现
│   │   │   ├── AdvancedSerialPort.cs    # 增强串口（事件+日志）
│   │   │   ├── SerialPortManager.cs     # 多串口管理器
│   │   │   ├── SerialPortScanner.cs     # 串口扫描与详情查询
│   │   │   ├── SerialFrameParser.cs     # 串口帧解析器
│   │   │   ├── TcpClientHelper.cs       # 基础 TCP 客户端
│   │   │   ├── AdvancedTcpClient.cs     # 增强 TCP 客户端（心跳+自动重连）
│   │   │   ├── TcpServerHelper.cs       # TCP 服务端（多客户端）
│   │   │   ├── TcpFrameParser.cs        # TCP 帧解析器（协议头+心跳识别）
│   │   │   └── ModbusHelper.cs          # Modbus RTU 报文辅助
│   │   └── Models/
│   │       ├── CommunicationResult.cs   # 通信结果
│   │       ├── SerialPortEventArgs.cs   # 串口接收事件参数
│   │       ├── TcpEventArgs.cs          # TCP 接收事件参数
│   │       └── ProtocolFrame.cs         # 协议帧模型
│   ├── Equipment/                       # 设备通用组件层
│   │   ├── Core/
│   │   │   ├── INozzle.cs               # 吸嘴接口
│   │   │   ├── IFeeder.cs               # 供料器接口
│   │   │   └── IHeater.cs               # 加热/温控接口
│   │   ├── Helpers/
│   │   │   ├── NozzleHelper.cs          # 吸嘴选型计算
│   │   │   └── FeederHelper.cs          # 供料器推进计算
│   │   └── Models/
│   │       ├── NozzleInfo.cs            # 吸嘴信息
│   │       ├── FeederInfo.cs            # 供料器信息
│   │       ├── StationInfo.cs           # 料站/料槽信息
│   │       └── PlacementResult.cs       # 贴装/固晶结果
│   ├── MES/                             # MES 系统对接层
│   │   ├── Core/
│   │   │   └── IMesClient.cs            # MES 客户端接口
│   │   ├── Helpers/
│   │   │   ├── MesHttpClient.cs         # HTTP 版 MES 客户端
│   │   │   └── MesHelper.cs             # MES 辅助（流水号、状态映射）
│   │   └── Models/
│   │       ├── MesResponse.cs           # MES 通用响应
│   │       ├── WorkOrderInfo.cs         # 工单信息
│   │       ├── TrackInRequest.cs        # 过站进入
│   │       ├── TrackOutRequest.cs       # 过站离开
│   │       ├── ProcessDataReport.cs     # 过程数据上报
│   │       ├── EquipmentStatusReport.cs # 设备状态上报
│   │       ├── AlarmReport.cs           # 报警上报
│   │       └── MaterialVerifyRequest.cs # 物料校验
│   ├── Files/                           # 文件管理
│   │   ├── Helpers/
│   │   │   ├── RecipeManager.cs         # 配方（Recipe）JSON 管理
│   │   │   ├── CsvHelper.cs             # CSV 读写
│   │   │   └── IniHelper.cs             # INI 读写
│   │   └── Models/
│   │       └── Recipe.cs                # 配方模型
│   └── Algorithm/                       # 算法层
│       ├── Helpers/
│       │   ├── GeometryHelper.cs        # 几何计算（旋转、交点、圆心）
│       │   └── CompensationHelper.cs    # 补偿算法（线性/双线性插值）
│       └── Models/
│           └── Point2D.cs               # 二维点
│
├── CoreToolkit.Desktop/                # WPF MVVM 辅助类库
│   ├── CoreToolkit.Desktop.csproj
│   ├── MVVM/
│   │   ├── ObservableObject.cs          # 可观察对象基类
│   │   ├── RelayCommand.cs              # 无参数命令
│   │   ├── RelayCommandT.cs             # 泛型命令
│   │   └── AsyncRelayCommand.cs         # 异步命令
│   ├── Converters/
│   │   ├── BooleanToVisibilityConverter.cs
│   │   ├── InverseBooleanConverter.cs
│   │   └── BoolToBrushConverter.cs
│   └── Behaviors/
│       └── MouseBehavior.cs             # 鼠标事件附加行为
│
├── CoreToolkit.Tests/                  # 控制台测试程序
│   ├── CoreToolkit.Tests.csproj
│   └── Program.cs
│
└── MotionTest.WPF/                     # WPF 测试应用（MVVM）
    ├── MotionTest.WPF.csproj
    ├── ViewModels/                      # 视图模型
    │   ├── MainWindowViewModel.cs
    │   ├── AxisViewModel.cs
    │   ├── IoItemViewModel.cs
    │   ├── DatabaseManagerViewModel.cs
    │   └── ParameterItemViewModel.cs
    ├── Converters/
    │   └── TabVisibilityConverter.cs
    ├── MainWindow.xaml
    ├── AxisControl.xaml
    └── DatabaseManager.xaml
```

***

## 快速开始

### 1. 还原依赖

```powershell
dotnet restore
```

### 2. 编译

```powershell
dotnet build
```

### 3. 运行测试

```powershell
# 控制台测试
dotnet run --project CoreToolkit.Tests

# WPF 测试应用（需要 Windows 环境）
dotnet run --project MotionTest.WPF
```

> 首次运行后，会在项目根目录自动生成 `main.db`、`logs.db`、`archive.db` 数据库文件。

***

## 核心模块说明

### 一、CoreToolkit（核心工具库）

#### 1. 通用工具层 `CoreToolkit.Common`

与业务无关的基础工具类，所有项目均可直接使用：

| 类型 | 说明 |
|------|------|
| `Guard` | 参数校验守卫，简化 `ArgumentNullException` 等抛出逻辑 |
| `EnumHelper` | 枚举描述获取、枚举转列表、字符串解析枚举 |
| `JsonHelper` | 基于 `Newtonsoft.Json` 的序列化/反序列化薄封装 |
| `RetryHelper` | 同步/异步重试执行机制 |
| `DateTimeHelper` | 时间戳转换、月初月末、去除毫秒等 |
| `StringHelper` | 安全截取、Base64、中文字符检测、按行分割 |
| `MathHelper` | Clamp、ApproxEqual、Lerp、角度弧度转换 |
| `Result` / `Result<T>` | 统一的操作结果封装，含成功/失败状态、消息、异常 |

#### 2. 数据库访问层 `CoreToolkit.Data`

**SqliteDbContext**
- 封装 `SQLiteConnection` 的连接打开、关闭与释放
- 提供统一的 SQL 执行入口：`Execute`、`Query<T>`、`QuerySingleOrDefault<T>`、`ExecuteScalar<T>`、`BeginTransaction`
- **线程安全**：使用 `SemaphoreSlim` 确保同一数据库文件的并发操作安全
- **自动添加新列**：当实体类增加属性时，自动检测并添加对应列到数据库表
- **WAL 模式**：默认启用 Write-Ahead Logging，提升并发性能

**RepositoryBase<T>**
- 所有实体仓储的泛型基类，约定：**表名 = 类名复数形式**，**列名 = 属性名**
- 已实现基础 CRUD、批量操作（带事务）、数据导出/导入（JSON / CSV）
- 完整的同步和异步 API

**多数据库支持**
- `DatabaseConfig` + `DatabaseFactory` 统一管理多个数据库连接配置

#### 3. 运动控制层 `CoreToolkit.Motion`

**统一接口 `IMotionCard`**
- 抽象了不同品牌控制卡的通用操作（初始化、打开、关闭、轴运动、IO 操作、状态读取等）

**工厂模式**
- `MotionCardFactory`：创建研华 PCI-1203、PCI-1285 等控制卡实例
- `IOCardFactory`：创建 IO 扩展卡实例

**轴控制增强**
- `AxisSpeedController` / `AxisSpeedManager`：速度曲线管理、加减速、S曲线参数封装
- 支持绝对运动、相对运动、JOG、回零、伺服控制、状态监控

**支持的硬件**
- 研华：PCI-1203（16/32轴）、PCI-1285（8轴）、PCI-1750/1756/1730（IO卡）
- 雷赛：DMCE-3000（EtherCAT 控制器）

#### 4. 机器视觉层 `CoreToolkit.Vision`

**ICamera**
- 工业相机接口抽象，支持打开、抓图、曝光、增益设置
- 未来可扩展为 Basler、Hikvision、Daheng 等相机实现

**标定与坐标转换**
- `CalibrationHelper`：九点标定（最小二乘）、旋转中心计算
- `CoordinateTransform`：像素坐标 ↔ 机械坐标双向转换（含缩放、旋转、平移）

**视觉结果模型**
- `MatchResult`：模板匹配结果（X、Y、角度、分数）
- `VisionResult`：通用视觉处理结果基类

#### 5. 通信层 `CoreToolkit.Communication`

**串口通讯**
- `ISerialPort` / `SerialPortHelper`：标准 `System.IO.Ports` 封装，一问一答模式
- `AdvancedSerialPort`：增强版串口，支持 `DataReceived` 事件、连接状态变化、收发日志
- `SerialPortManager`：统一管理多个串口，支持全局事件和日志
- `SerialPortScanner`：扫描系统可用串口，支持 VID/PID 查找 USB 转串口设备
- `SerialFrameParser`：串口数据帧解析器，支持固定长度、结束符、头部+长度三种拆包模式

**TCP/IP**
- `ITcpClient` / `TcpClientHelper`：基础同步/异步 TCP 客户端
- `AdvancedTcpClient`：增强版 TCP 客户端，支持后台事件接收、心跳保活、自动重连、收发日志
- `TcpServerHelper`：TCP 服务端，支持多客户端接入、数据广播、客户端管理
- `TcpFrameParser`：TCP 粘包专用解析器，支持协议头+长度字段、结束符、固定长度三种拆包模式，可识别心跳帧
- `ProtocolFrame`：通用协议帧模型，包含 Header、Command、Payload、Checksum、IsHeartbeat 等字段

**Modbus 辅助**
- `ModbusHelper`：组装 Modbus RTU 报文（03/05 功能码）、CRC16 校验、寄存器转 float

#### 6. 设备通用组件层 `CoreToolkit.Equipment`

针对贴片机、固晶机抽象的通用设备组件：

| 类型 | 说明 |
|------|------|
| `INozzle` / `NozzleInfo` | 吸嘴接口与信息（直径、真空度） |
| `IFeeder` / `FeederInfo` | 供料器接口与信息（Pitch、剩余料数） |
| `IHeater` | 加热/温控接口（固晶机加热台、贴片机温区） |
| `StationInfo` | 料站/料槽位置与元件编码 |
| `PlacementResult` | 单次贴装/固晶结果记录（目标/实际坐标、偏差） |

**辅助计算**
- `NozzleHelper.RecommendOuterDiameter(...)`：根据元件尺寸推荐吸嘴
- `FeederHelper.CalculateAdvanceDistance(...)`：计算料带推进距离

#### 7. MES 对接层 `CoreToolkit.MES`

针对贴片机/固晶机与 MES 系统的标准交互场景封装：

| 功能 | 对应类型 |
|------|---------|
| 过站进入/离开 | `TrackInRequest` / `TrackOutRequest` |
| 上传过程数据 | `ProcessDataReport` |
| 上报设备状态 | `EquipmentStatusReport` |
| 上报报警信息 | `AlarmReport` |
| 物料校验 | `MaterialVerifyRequest` |
| 查询工单 | `QueryWorkOrder` -> `WorkOrderInfo` |

**`MesHttpClient`**
- 基于 `System.Net.Http` 的 REST 风格 MES 客户端
- 支持同步/异步调用
- 内置 JSON 序列化和通用响应封装
- 支持 Bearer Token / Basic Auth 注入

**`MesHelper`**
- `GenerateTransactionId()`：生成交易流水号
- `BuildTrackOutFromPlacement(...)`：从贴装结果批量生成过站请求
- `MapEquipmentStatus(...)`：设备状态枚举转 MES 状态码

#### 8. 文件管理层 `CoreToolkit.Files`

**配方管理 `RecipeManager`**
- 基于 JSON 的配方（Recipe）读写、枚举、删除
- 适配贴片机/固晶机不同产品的参数切换

**CSV / INI 辅助**
- `CsvHelper.Write<T>` / `Read<T>`：对象列表与 CSV 互转
- `IniHelper`：轻量级 INI 读写，支持 `GetInt`、`GetDouble`、`GetBool`

#### 8. 算法层 `CoreToolkit.Algorithm`

**几何计算 `GeometryHelper`**
- 角度弧度转换、两点距离、点绕中心旋转、直线交点、三点求圆心

**补偿算法 `CompensationHelper`**
- `LinearCompensate`：一维线性补偿（常用于单轴补偿表）
- `BilinearCompensate`：双线性插值补偿（常用于平面精度补偿）

### 二、CoreToolkit.Desktop（WPF MVVM 辅助库）

为 WPF 应用提供轻量级的 MVVM 基础设施，避免引入 Prism/MVVM Light 等重型框架。

| 类型 | 说明 | 典型用途 |
|------|------|---------|
| `ObservableObject` | 属性变更通知基类 | 所有 ViewModel 的基类 |
| `RelayCommand` | 无参数命令 | 按钮点击事件 |
| `RelayCommand<T>` | 带参数命令 | 需要传递参数的按钮 |
| `AsyncRelayCommand` | 异步命令 | 执行异步操作的按钮 |
| `BooleanToVisibilityConverter` | bool 转 Visibility | 控制 UI 元素显隐 |
| `BoolToBrushConverter` | bool 转 Brush | 状态指示灯颜色 |
| `InverseBooleanConverter` | bool 取反 | 反逻辑绑定 |
| `MouseBehavior` | 附加行为 | JOG 按钮"按下运动、抬起停止" |

### 三、MotionTest.WPF（MVVM 示例应用）

WPF 测试应用已全面重构为 MVVM 架构：

- **零代码后置**：`MainWindow.xaml.cs`、`AxisControl.xaml.cs`、`DatabaseManager.xaml.cs` 中不再包含业务逻辑
- **ViewModel 驱动**：所有状态、命令、数据操作集中在 `ViewModels` 文件夹
- **数据绑定**：轴状态、IO 状态、日志、数据库管理全部通过 Binding 实现
- **行为扩展**：JOG 按钮通过 `MouseBehavior` 绑定 `PreviewMouseDown/Up` 命令

***

## 使用示例

### 数据库操作

```csharp
using CoreToolkit.Data;

using (var db = new SqliteDbContext("demo.db"))
{
    db.InitDatabase();
    var userRepo = new UserRepository(db);

    var user = new User { UserName = "张三", Email = "zhangsan@example.com", Age = 28 };
    long id = await userRepo.InsertAsync(user);
    var result = await userRepo.GetByIdAsync(id);
}
```

### 运动控制

```csharp
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;

var motionCard = MotionCardFactory.CreateCard("PCI1285");
motionCard.Initialize(new MotionConfig { CardId = 0 });
motionCard.Open();
motionCard.SetServoEnable(0, true);
motionCard.MoveAbsolute(0, 10000, 5000);
```

### 视觉与标定

```csharp
using CoreToolkit.Vision.Helpers;
using CoreToolkit.Vision.Models;

// 九点标定
var points = new List<CalibrationData>
{
    new CalibrationData { PixelX = 100, PixelY = 100, MachineX = 10.5, MachineY = 20.3 },
    // ... 至少 3 组
};
var transform = CalibrationHelper.NinePointCalibration(points);
var (mx, my) = transform.PixelToMachine(150, 150);

// 旋转中心计算（取两个角度下的机械坐标）
var (cx, cy) = CalibrationHelper.CalculateRotationCenter(0, 100, 100, 90, 120, 120);
```

### 通信

```csharp
using CoreToolkit.Communication.Helpers;

// 串口管理器：同时管理扫码枪和称重仪
var manager = new SerialPortManager();
manager.LogAction = msg => Console.WriteLine(msg);

// 添加并打开 COM3（扫码枪）
var scanner = manager.AddPort("COM3", 115200);
manager.DataReceived += (s, e) => Console.WriteLine($"[{e.PortName}] 收到: {e.Text ?? BitConverter.ToString(e.Data)}");

// 添加 COM4（称重仪）并绑定帧解析器
var parser = new SerialFrameParser
{
    Mode = ParseMode.EndMarker,
    EndMarker = new byte[] { 0x0D, 0x0A }
};
parser.FrameReceived += frame => Console.WriteLine($"解析到一帧: {BitConverter.ToString(frame)}");

var scale = new AdvancedSerialPort("COM4", 9600);
scale.DataReceived += (s, e) => parser.Feed(e.Data);
scale.Open();

// 扫描可用串口
foreach (var info in SerialPortScanner.GetPortDetails())
{
    Console.WriteLine(info);
}

// TCP 基础客户端
var tcp = new TcpClientHelper("192.168.1.10", 502);
tcp.Connect();
tcp.Send(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02 });
var response = tcp.Receive(8);

// TCP 增强客户端（连智能相机或上位机）
var advancedTcp = new AdvancedTcpClient("192.168.1.20", 5000)
{
    LogAction = msg => Console.WriteLine(msg),
    AutoReconnect = true,
    ReconnectIntervalMs = 3000,
    EnableHeartbeat = true,
    HeartbeatIntervalMs = 5000,
    HeartbeatData = new byte[] { 0x01, 0x02 }
};
advancedTcp.DataReceived += (s, e) => Console.WriteLine($"收到: {BitConverter.ToString(e.Data)}");
advancedTcp.ConnectionStatusChanged += (s, isConnected) => Console.WriteLine($"连接状态: {isConnected}");
await advancedTcp.ConnectAsync();

// TCP 服务端（设备作为服务端被上位机连接）
var server = new TcpServerHelper(6000);
server.LogAction = msg => Console.WriteLine(msg);
server.DataReceived += (s, e) => Console.WriteLine($"[{e.RemoteEndPoint}] {BitConverter.ToString(e.Data)}");
server.ClientConnectionChanged += (s, e) => Console.WriteLine($"客户端 {e.clientId} {(e.isConnected ? "连接" : "断开")}");
server.Start();
server.Broadcast(new byte[] { 0xAA, 0xBB }); // 广播给所有客户端

// TCP 帧解析器（带协议头+心跳识别）
var tcpParser = new TcpFrameParser
{
    Mode = TcpParseMode.HeaderLength,
    FrameHeader = new byte[] { 0xAA, 0xBB },      // 帧头
    HeaderLength = 5,                               // 帧头2 + 命令1 + 长度2
    LengthFieldOffset = 3,
    LengthFieldSize = 2,
    LengthBigEndian = true,
    ChecksumLength = 2,                             // CRC16
    HeartbeatCommand = 0x01                         // 命令字 0x01 为心跳
};

tcpParser.FrameReceived += f => Console.WriteLine($"收到数据帧: {f}");
tcpParser.HeartbeatReceived += f => Console.WriteLine("收到心跳帧");

// 模拟收到粘包数据
var packet1 = new byte[] { 0xAA, 0xBB, 0x10, 0x00, 0x02, 0x01, 0x02, 0xCC, 0xDD };
var packet2 = new byte[] { 0xAA, 0xBB, 0x01, 0x00, 0x00, 0xEE, 0xFF };
tcpParser.Feed(packet1.Concat(packet2).ToArray());

// Modbus RTU 读保持寄存器
var frame = ModbusHelper.BuildReadHoldingRegisters(1, 0, 2);
var floatValue = ModbusHelper.RegistersToFloat(high, low);
```

### 设备组件

```csharp
using CoreToolkit.Equipment.Helpers;
using CoreToolkit.Equipment.Models;

// 吸嘴选型
double nozzleDiameter = NozzleHelper.RecommendOuterDiameter(3.2, 1.6);
int vacuum = NozzleHelper.EstimateVacuumLevel(0.05, 3.14);

// 供料器推进
var feeder = new FeederInfo { Pitch = 4.0, TotalComponents = 3000, UsedComponents = 150 };
double distance = FeederHelper.CalculateAdvanceDistance(feeder, 1);
bool enough = FeederHelper.HasEnoughComponents(feeder, 10);
```

### MES 对接

```csharp
using CoreToolkit.MES.Helpers;
using CoreToolkit.MES.Models;

// 创建 MES 客户端
var mes = new MesHttpClient("http://mes-server/api");
mes.SetAuthorization("Bearer your-token");

// 过站进入
var trackIn = new TrackInRequest
{
    WorkOrderNumber = "WO20250404001",
    SerialNumber = "SN123456789",
    EquipmentId = "SMT-001",
    ProcessStep = "SMT_Top",
    OperatorId = "OP001"
};
var result = mes.TrackIn(trackIn);

// 上报过程数据
var report = new ProcessDataReport
{
    SerialNumber = "SN123456789",
    EquipmentId = "SMT-001",
    Parameters = new List<ProcessParameter>
    {
        new ProcessParameter { ParameterName = "PlacementSpeed", ParameterValue = "5000", Unit = "mm/s" }
    }
};
mes.UploadProcessData(report);

// 从贴装结果生成过站离开请求
var trackOut = MesHelper.BuildTrackOutFromPlacement("WO20250404001", "SN123456789", "SMT-001", "SMT_Top", placementResults);
var trackOutResult = mes.TrackOut(trackOut);
```

### WPF MVVM

```csharp
using CoreToolkit.Desktop.MVVM;

public class MyViewModel : ObservableObject
{
    private string _userName;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public ICommand SaveCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new RelayCommand(Save);
    }

    private void Save()
    {
        // ...
    }
}
```

```xml
<TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}"/>
<Button Content="保存" Command="{Binding SaveCommand}"/>
```

***

## 扩展指南

### 向 CoreToolkit 添加新功能

`CoreToolkit` 是你未来工作中持续扩展的核心 DLL，建议按以下方式组织：

```
CoreToolkit/
├── Common/         # 通用工具类（随时可用）
├── Data/           # 数据库相关
├── Motion/         # 运动控制相关
├── Vision/         # 机器视觉
├── Communication/  # 串口/网口通信
├── Equipment/      # 设备通用组件（吸嘴、供料器、加热等）
├── MES/            # MES 系统对接
├── Files/          # 文件管理（配方、CSV、INI）
└── Algorithm/      # 算法工具
```

命名空间统一使用 `CoreToolkit.XXX`，例如：
- `CoreToolkit.Common.Helpers`
- `CoreToolkit.Vision`
- `CoreToolkit.Communication`
- `CoreToolkit.Equipment`
- `CoreToolkit.MES`
- `CoreToolkit.Files`

### 新增数据表

1. 在 `CoreToolkit/Data/Models` 下新建类并继承 `EntityBase`
2. 在 `CoreToolkit/Data/Database` 下新建仓储并继承 `RepositoryBase<T>`
3. 在 `SqliteDbContext.InitDatabase()` 中添加 `CREATE TABLE IF NOT EXISTS` SQL

### 新增控制卡支持

1. 在 `CoreToolkit/Motion/Providers` 下创建新的厂商文件夹
2. 实现 `IMotionCard` 接口
3. 在 `MotionCardFactory` 中添加创建逻辑

### Common 工具类使用

```csharp
using CoreToolkit.Common.Helpers;
using CoreToolkit.Common.Models;

// 参数校验
Guard.NotNull(user, nameof(user));
Guard.GreaterThan(age, 0, nameof(age));

// 操作结果封装
var result = Result.Success("保存成功");
var dataResult = Result<int>.Success(42, "获取成功");

// JSON 序列化
string json = JsonHelper.Serialize(user);
var obj = JsonHelper.Deserialize<User>(json);

// 重试机制
var retryResult = RetryHelper.Execute(() => ReadSensorData(), retryCount: 3, delayMilliseconds: 200);
```

### 新增视觉硬件支持

1. 在 `CoreToolkit/Vision/Providers` 下创建相机实现文件夹（如 `Hikvision`、`Basler`）
2. 实现 `ICamera` 接口
3. 在 `Vision/Core/CameraFactory.cs`（未来添加）中注册创建逻辑

### 新增通信协议

1. 在 `CoreToolkit/Communication/Helpers` 下添加协议报文组装类
2. 基于 `ISerialPort` / `ITcpClient` 发送报文

### 新增设备组件

1. 在 `CoreToolkit/Equipment/Core` 下定义接口（如 `IPressureSensor`）
2. 在 `CoreToolkit/Equipment/Models` 下添加信息模型
3. 在 `CoreToolkit/Equipment/Helpers` 下添加选型/计算辅助

### 新增 MES 对接功能

1. 在 `CoreToolkit/MES/Models` 下添加新的请求/响应模型（继承 `MesResponse`）
2. 在 `CoreToolkit/MES/Core/IMesClient.cs` 中增加方法签名
3. 在 `MesHttpClient` 中实现对应的 HTTP 调用逻辑
4. 如果 MES 采用 WebService / MQ 等其他协议，可新建 `MesSoapClient` / `MesMqClient` 并同样实现 `IMesClient`

### 配方管理示例

```csharp
using CoreToolkit.Files.Helpers;
using CoreToolkit.Files.Models;

var recipeManager = new RecipeManager(@"D:\Recipes");
var recipe = new Recipe
{
    RecipeName = "Product-A",
    ProductCode = "PA-2025-001",
    Parameters = new Dictionary<string, object>
    {
        ["FeederPitch"] = 4.0,
        ["PlacementSpeed"] = 5000,
        ["VisionExposure"] = 12000
    }
};
recipeManager.Save(recipe);
var loaded = recipeManager.Load("Product-A");
```

### 扩展 CoreToolkit.Desktop

1. 在 `MVVM/` 下添加新的基类或命令类型
2. 在 `Converters/` 下添加新的值转换器
3. 在 `Behaviors/` 下添加新的附加行为

***

## 注意事项

1. **线程安全**：数据库操作通过 `SemaphoreSlim` 保证线程安全，推荐每次操作都使用 `using (var db = new SqliteDbContext(...))` 模式。
2. **并发写入**：SQLite 并发写入能力有限，系统已启用 WAL 模式提升性能。
3. **WPF 权限**：使用研华控制卡时，程序需要以管理员权限运行，且 `AdvMotAPI.dll` 必须在输出目录中。
4. **异步操作**：异步 API 使用 `async/await` 模式，调用方需标记 `async`。
5. **自动迁移**：`AutoAddMissingColumns` 仅支持在表末尾添加列，无法指定列位置。

***

## 作者

*项目创建于 2026-04-02，重构于 2026-04-04*
