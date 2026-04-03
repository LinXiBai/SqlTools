# MotionLib 运动控制库说明文档

## 项目概述

MotionLib 是一个基于研华（Advantech）API 的运动控制库，支持多种运动控制卡的封装和管理，提供统一的接口进行运动控制、状态监测和IO操作。

## 目录结构

```
MotionLib/
├── Core/                  # 核心功能
│   ├── AxisStatus.cs      # 轴状态结构体
│   ├── IMotionCard.cs     # 运动控制卡接口
│   ├── IOCardManager.cs   # IO扩展卡管理器
│   ├── LTDMC.cs           # 雷赛控制卡API封装
│   ├── MotionConfig.cs     # 运动控制配置
│   ├── MotionException.cs  # 运动控制异常
│   └── 研华API.log        # 研华API日志
├── Factory/               # 工厂类
│   ├── IOCardFactory.cs   # IO扩展卡工厂
│   └── MotionCardFactory.cs # 运动控制卡工厂
├── Interfaces/            # 接口定义
│   └── IIOCard.cs         # IO扩展卡接口
├── Providers/             # 具体实现
│   ├── Advantech/         # 研华控制卡
│   │   ├── AdvantechIOCard.cs # 研华IO扩展卡
│   │   ├── PCI1203.cs      # PCI-1203控制卡
│   │   └── PCI1285.cs      # PCI-1285控制卡
│   └── Leadshine/         # 雷赛控制卡
│       └── DMCE3000.cs     # DMCE-3000控制卡
├── dll/                   # 第三方库
│   └── AdvMotAPI.dll      # 研华API DLL
├── MotionLib.csproj        # 项目文件
└── README.md              # 说明文档
```

## 核心功能

### 1. 运动控制卡管理

- **统一接口**：通过 `IMotionCard` 接口统一不同品牌控制卡的操作
- **工厂模式**：使用 `MotionCardFactory` 创建控制卡实例
- **多卡支持**：支持研华 PCI-1203、PCI-1285 等控制卡

### 2. 轴控制功能

- **基本运动**：绝对运动、相对运动、JOG运动
- **状态监测**：实时读取轴状态、位置、速度
- **回零操作**：支持多种回零模式
- **伺服控制**：伺服使能/关闭
- **速度曲线**：设置加速度、减速度、S曲线

### 3. IO 控制

- **数字输入**：读取输入点状态
- **数字输出**：设置输出点状态
- **IO扩展**：通过 `IIOCard` 接口支持IO扩展卡
- **IO管理器**：使用 `IOCardManager` 管理多个IO扩展卡

### 4. 高级功能

- **线性插补**：支持多轴线性插补运动
- **状态监控**：实时监测轴状态和IO状态
- **异常处理**：统一的异常处理机制
- **日志记录**：详细的操作日志

## 快速开始

### 1. 初始化控制卡

```csharp
// 创建控制卡实例
var motionCard = MotionCardFactory.CreateCard("PCI1285");

// 初始化控制卡
var config = new MotionConfig { CardId = 0 };
motionCard.Initialize(config);

// 打开控制卡
motionCard.Open();
```

### 2. 轴控制

```csharp
// 伺服使能
motionCard.SetServoEnable(0, true);

// 回零
motionCard.Home(0, 1000);

// 绝对运动
motionCard.MoveAbsolute(0, 10000, 5000);

// 相对运动
motionCard.MoveRelative(0, 5000, 3000);

// JOG运动
motionCard.Jog(0, 1, 2000); // 正向JOG

// 停止
motionCard.Stop(0, false);
```

### 3. IO 操作

```csharp
// 读取输入
bool inputValue = motionCard.ReadInput(0);

// 设置输出
motionCard.WriteOutput(0, true);
```

### 4. IO扩展卡

```csharp
// 创建IO扩展卡
var ioCard = IOCardFactory.CreateCard("PCI-1750", 32, 32);

// 添加到管理器
IOCardManager.Instance.AddIOCard(0, ioCard);

// 初始化和打开
IOCardManager.Instance.InitializeAll();
IOCardManager.Instance.OpenAll();

// 使用全局索引操作IO
IOCardManager.Instance.WriteOutput(0, true);
bool input = IOCardManager.Instance.ReadInput(5);
```

## 支持的控制卡

### 研华控制卡
- **PCI-1203**：16轴/32轴 EtherCAT 主站控制
- **PCI-1285**：8轴脉冲输出控制
- **PCI-1750**：32入32出数字IO卡
- **PCI-1756**：32入32出数字IO卡
- **PCI-1730**：16入16出数字IO卡

### 雷赛控制卡
- **DMCE-3000**：多轴运动控制器

## 依赖项

- **AdvMotAPI.dll**：研华运动控制API
- **.NET Framework 4.7.2**：项目目标框架

## 错误处理

```csharp
try
{
    motionCard.MoveAbsolute(0, 10000, 5000);
}
catch (MotionException ex)
{
    Console.WriteLine($"运动失败: {ex.Message}, 错误码: {ex.ErrorCode}");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}
```

## 示例应用

项目包含一个 WPF 测试应用 `MotionTest.WPF`，提供以下功能：

- **控制卡操作**：初始化、打开、关闭、复位
- **8轴控制**：每个轴独立的控制面板
- **实时状态**：伺服状态、运行状态、报警状态、原点状态
- **IO控制**：16路输入显示、16路输出控制
- **操作日志**：实时操作记录
- **数据库管理**：轴参数和IO参数的管理与配置

## 数据库设计

### 轴参数数据库

轴参数数据库存储8个轴的详细配置信息，包括：

- **基本信息**：ID、轴名称、轴号、卡号、轴类型
- **运动参数**：脉冲当量、脉冲当量分母、运动低速、运动高速、加速度、减速度、加加速度、减减速度
- **回零参数**：回原模式、回原方向、原点高速、原点低速、原点加速度、原点减速度、原点偏移
- **限位参数**：正向软极限、负向软极限
- **IO配置**：使能IO

### IO参数数据库

IO参数数据库存储16x16 IO点的配置信息，包括：

- **基本信息**：ID、卡号、端口号
- **输入配置**：输入点、输入名称
- **输出配置**：输出点、输出名称

## 数据库管理

WPF应用提供了数据库管理窗口，支持：

- **轴参数管理**：添加、编辑、删除轴参数
- **IO参数管理**：添加、编辑、删除IO参数
- **参数预览**：实时预览参数修改效果
- **参数保存**：保存修改后的参数到数据库

## WPF界面使用说明

### 主窗口

- **控制卡操作区**：控制卡的初始化、打开、关闭、复位操作
- **轴选择区**：选择要操作的轴
- **轴控制区**：轴的基本运动控制（点动、回零、绝对运动、相对运动）
- **状态显示区**：实时显示轴的状态（伺服、运行、报警、原点）
- **IO控制区**：显示输入状态，控制输出状态
- **操作日志区**：显示操作历史和错误信息
- **数据库管理按钮**：打开数据库管理窗口

### 数据库管理窗口

- **轴参数标签页**：管理8个轴的参数
- **IO参数标签页**：管理16x16 IO点的参数
- **参数编辑区**：编辑选中参数的详细信息
- **保存按钮**：保存修改后的参数
- **添加按钮**：添加新的轴或IO参数
- **删除按钮**：删除选中的轴或IO参数

## 项目配置与部署

### 开发配置

1. **克隆仓库**：`git clone https://github.com/LinXiBai/SqlTools.git`
2. **打开解决方案**：在Visual Studio中打开 `SqlDemo.sln`
3. **恢复依赖**：右键解决方案，选择「恢复NuGet包」
4. **生成项目**：构建整个解决方案

### 部署步骤

1. **构建项目**：在Visual Studio中构建解决方案
2. **复制DLL**：确保 `AdvMotAPI.dll` 复制到输出目录
3. **配置文件**：根据实际硬件修改配置
4. **运行应用**：以管理员权限运行 `MotionTest.WPF.exe`

## 常见问题解答

### Q: 无法初始化控制卡

**A:** 请检查：
- 控制卡驱动是否安装
- 控制卡是否正确安装在PCI插槽
- 程序是否以管理员权限运行
- 控制卡型号是否在支持列表中

### Q: 轴运动异常

**A:** 请检查：
- 轴参数是否正确配置
- 伺服使能是否开启
- 限位开关是否触发
- 控制卡与伺服驱动器连接是否正常

### Q: IO操作无响应

**A:** 请检查：
- IO卡是否正确安装
- IO参数是否正确配置
- IO卡驱动是否安装
- 物理连接是否正常

### Q: 数据库操作失败

**A:** 请检查：
- 数据库文件是否存在
- 数据库文件权限是否正确
- 数据库表结构是否完整

## 项目结构说明

### 解决方案结构

```
SqlDemo/
├── MotionLib/            # 运动控制库
├── SqlDemo.Data/         # 数据访问层
├── MotionTest.WPF/       # WPF测试应用
└── SqlDemo.sln           # 解决方案文件
```

### 核心模块关系

1. **MotionLib**：提供控制卡封装和运动控制功能
2. **SqlDemo.Data**：提供数据库访问和参数管理
3. **MotionTest.WPF**：提供用户界面和操作体验

## 技术栈

- **开发语言**：C#
- **框架**：.NET Framework 4.7.2
- **UI框架**：WPF
- **数据库**：SQLite
- **控制卡API**：研华 AdvMotAPI

## 扩展与定制

### 添加新控制卡

1. 在 `Providers` 目录下创建新的控制卡实现类
2. 实现 `IMotionCard` 接口
3. 在 `MotionCardFactory` 中添加创建逻辑

### 添加新IO卡

1. 在 `Providers` 目录下创建新的IO卡实现类
2. 实现 `IIOCard` 接口
3. 在 `IOCardFactory` 中添加创建逻辑

### 扩展数据库字段

1. 修改 `AxisParameter` 或 `IOParameter` 实体类
2. 重新构建项目，数据库表结构会自动更新

## 注意事项

1. **驱动安装**：使用研华控制卡前，请确保已安装研华驱动
2. **权限**：程序需要以管理员权限运行
3. **DLL路径**：AdvMotAPI.dll 需要在程序运行目录
4. **虚拟轴卡**：支持研华虚拟轴卡进行测试

## 开发环境

- **开发工具**：Visual Studio 2022+
- **目标框架**：.NET Framework 4.7.2
- **语言**：C#

## 版本历史

- **v1.0.0**：初始版本
  - 支持研华 PCI-1203、PCI-1285 控制卡
  - 支持研华 IO 扩展卡
  - 提供 WPF 测试应用

## 联系方式

如有问题或建议，请联系：
- 邮箱：your-email@example.com
- 电话：123-4567-8910

---

## 快速接入指南

### 新对话快速接入步骤

1. **克隆仓库**：`git clone https://github.com/LinXiBai/SqlTools.git`
2. **打开解决方案**：在Visual Studio中打开 `SqlDemo.sln`
3. **构建项目**：生成整个解决方案
4. **运行测试应用**：启动 `MotionTest.WPF` 项目
5. **初始化数据库**：首次运行会自动创建数据库文件
6. **配置参数**：使用数据库管理窗口配置轴和IO参数
7. **测试控制**：使用主窗口测试控制卡和轴操作

### 关键文件说明

- **MotionLib/Providers/Advantech/**：研华控制卡实现
- **SqlDemo.Data/Models/**：数据库实体模型
- **MotionTest.WPF/**：WPF测试应用
- **MotionLib/README.md**：项目说明文档

### 核心功能快速入口

- **控制卡初始化**：`MotionCardFactory.CreateCard()`
- **轴控制**：`motionCard.MoveAbsolute()`, `motionCard.Home()`
- **IO操作**：`motionCard.ReadInput()`, `motionCard.WriteOutput()`
- **数据库操作**：`AxisParameterRepository`, `IOParameterRepository`
- **参数管理**：数据库管理窗口

### 测试环境搭建

1. **安装研华驱动**：从研华官网下载并安装控制卡驱动
2. **安装虚拟轴卡**：使用研华虚拟轴卡进行测试
3. **配置项目**：确保 `AdvMotAPI.dll` 路径正确
4. **运行应用**：以管理员权限运行测试应用

---

**© 2026 MotionLib 项目组**