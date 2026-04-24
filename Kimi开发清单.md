# Kimi 开发清单

> 本清单用于跟踪 SqlDemo / CoreToolkit 项目的开发进度，方便对话时快速定位已完成和待开发的功能。

---

## ✅ 已完成

### 一、项目基础设施

| # | 功能 | 说明 | 文件位置 |
|---|------|------|----------|
| 1.1 | **全模块 README** | 为 17 个功能模块编写 README（含目录结构、核心类、使用示例） | 各模块根目录 `README.md` |
| 1.2 | **根项目 README** | 项目整体介绍、技术栈、项目结构 | `README.md` |
| 1.3 | **Safety 模块文档** | 独立接口文档 SAFETY.md（12 章节） | `SAFETY.md` |

### 二、CoreToolkit 核心库

#### Algorithm 算法
- [x] `CompensationHelper` — 螺距补偿、反向间隙补偿
- [x] `GeometryHelper` — 几何计算（距离、角度、交点）
- [x] `Point2D` — 二维坐标点模型

#### Common 通用工具
- [x] `Result<T>` — 通用结果封装
- [x] `Guard` — 参数守卫
- [x] `RetryHelper` — 重试策略
- [x] `JsonHelper` — JSON 序列化封装
- [x] `MathHelper` — 数学工具

#### Communication 通信
- [x] `ISerialPort` / `ITcpClient` 接口抽象
- [x] `SerialPortManager` — 多实例串口管理
- [x] `SerialPortScanner` — 串口自动扫描
- [x] `ModbusHelper` — Modbus 协议辅助
- [x] `SerialFrameParser` — 帧解析（防粘包）

#### Data 数据持久化
- [x] `SqliteDbContext` — SQLite 连接管理、WAL 模式
- [x] `RepositoryBase<T>` — 泛型仓储基类
- [x] `LogRepository` — 日志仓储（含安全事件统计报表：按天/周/月）
- [x] `AxisParameterRepository` — 轴参数仓储
- [x] `UserRepository` — 用户仓储
- [x] `LicenseRecordRepository` — 许可证记录仓储（含分页模糊搜索）
- [x] `PagedResult<T>` — 通用分页结果模型

#### Equipment 设备抽象
- [x] `IFeeder` / `INozzle` / `IHeater` 接口
- [x] `PlacementResult` / `FeederInfo` / `StationInfo` 模型

#### Files 文件管理
- [x] `RecipeManager` — 配方管理
- [x] `FileIndexManager` / `V2` — 高性能文件索引
- [x] `CsvHelper` / `IniHelper`

#### MES 制造执行系统
- [x] `IMesClient` / `MesHttpClient`
- [x] Track-In/Track-Out、报警上报、设备状态上报

#### Motion 运动控制
- [x] `IMotionCard` / `IAxisGroup` 接口
- [x] `AxisSpeedProfile` / `AxisSpeedController` — 速度规划
- [x] `MotionCardFactory` — 工厂模式
- [x] `PCI1285` / `PCI1203` / `DMCE3000` 硬件驱动实现

#### Safety 安全防护（新增模块）
- [x] `CollisionDetector` — AABB 碰撞检测（Static/Dynamic/Temporary）
- [x] `SafeMotionController` — 预移动检查
- [x] `InterlockEngine` — 互锁规则引擎
- [x] `SoftLimitGuard` — 软限位守卫
- [x] `SafetyMonitor` — 100ms 后台安全监控循环
- [x] `DualHeadAntiCollision` — 双头防撞
- [x] `SafetySetupHelper` — 一键初始化安全系统
- [x] `SafetyCheckModule` / `SafeAxisMoveModule`
- [x] 安全事件自动写入数据库（LoggerName="SafetyMonitor"）

#### StateMachine 状态机
- [x] `StateMachineManager` / `StateMachine` — 流程编排
- [x] `FlowModuleBase` — 模块基类（超时、取消、进度、钩子）
- [x] `SequentialModule` — 串行容器（**支持断点续传**）
- [x] `ParallelModule` — 并行容器
- [x] `ConditionalModule` / `LoopModule`
- [x] `RetryModule` — **新增** 重试模块（退避策略）
- [x] `TryCatchModule` — **新增** 异常捕获模块（Try/Catch/Finally）
- [x] `InPositionDetector` — 到位检测
- [x] `TrajectoryRecorder` — 轨迹记录（线程安全）
- [x] **优化**：ExecutionContext ConcurrentDictionary 线程安全
- [x] **优化**：FlowModuleBase 异常完整保存、Cancel 异常记录
- [x] **优化**：IProgress<ModuleProgress> 进度报告
- [x] **优化**：StateMachine.StopAsync 异步停止

#### Vision 视觉
- [x] `ICamera` 接口
- [x] `CalibrationHelper` — 标定辅助
- [x] `CoordinateTransform` — 像素↔物理坐标转换

### 三、CoreToolkit.Desktop WPF 支持

- [x] `ObservableObject` — INotifyPropertyChanged + INotifyPropertyChanging
- [x] `RelayCommand` / `RelayCommand<T>` / `AsyncRelayCommand`
- [x] `BooleanToVisibilityConverter` / `BoolToBrushConverter` / `InverseBooleanConverter`
- [x] `MouseBehavior` — 附加行为（MouseDoubleClick/MouseWheel/MouseEnter/MouseLeave）
- [x] **优化**：ObservableObject 线程安全事件调用、批量通知
- [x] **优化**：AsyncRelayCommand Interlocked 重入保护、可绑定 IsExecuting
- [x] **优化**：RelayCommand<T> 安全类型检查
- [x] **优化**：Converters 空值处理（Binding.DoNothing）

### 四、MotionTest.WPF 测试程序

- [x] `SafetyMonitorWindow` — 安全监控窗口（实时体积、碰撞状态、急停）
- [x] `SafetyEventHistoryWindow` — 安全事件历史查询（时间/级别/关键词过滤、导出）
- [x] `SafetyStatisticsWindow` — **新增** 安全统计报表（按天/周/月、均值/峰值、双击穿透）
- [x] `SafetyMonitorViewModel` — MVVM 重构（751行）
- [x] `SafetyEventHistoryViewModel` / `SafetyStatisticsViewModel`
- [x] `TrajectoryChart` — 轨迹图表自定义控件
- [x] `FlowControlWindow` — 流程控制窗口

### 五、LicenseManager.WPF 许可证管理（重构完成）

- [x] **MVVM 重构**：`LicenseManagerViewModel` + `LicenseRecordViewModel`
- [x] **统计仪表盘**：总记录数/项目数/设备数/部门数/记录人数
- [x] **模糊搜索**：跨字段 LIKE 搜索（项目号/设备号/申请人/记录人/部门）
- [x] **复合筛选**：部门 + 记录人 + 申请人 + 设备类型 + 时间范围
- [x] **分页查询**：SQL LIMIT/OFFSET 分页，每页50条
- [x] **实时搜索**：Debounce 300ms 自动触发
- [x] **批量操作**：复选框多选 + 批量删除
- [x] **导出**：导出当前筛选的全部记录为 CSV
- [x] **空数据提示** / **Loading 遮罩**
- [x] `PagedResult<T>` 分页模型
- [x] `LicenseRecordRepository.SearchPaged()` 分页查询

### 六、测试

- [x] `SafetyMonitorViewModelTests` — 16 个 xUnit 测试（Mock<IMotionCard>）

---

## 🚧 待开发 / 可优化

### 一、CoreToolkit 核心库

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T1.1 | **StateMachine 状态持久化** | 高 | 将 FlowStatistics/执行状态序列化到 SQLite，支持崩溃恢复 |
| T1.2 | **StateMachine 可视化导出** | 中 | 导出模块树为 Mermaid/PlantUML 流程图 |
| T1.3 | **StateMachine 审计日志表** | 中 | 结构化审计日志写入数据库（非内存 TextBox） |
| T1.4 | **Data 数据库迁移工具** | 中 | 版本化 Schema 迁移（当前只有 AutoAddMissingColumns） |
| T1.5 | **Algorithm 3D 几何** | 低 | Point3D、空间距离、三维碰撞检测 |
| T1.6 | **Vision 相机实现** | 低 | GigE/USB 相机具体实现类 |
| T1.7 | **Communication OPC UA** | 低 | OPC UA 客户端支持 |

### 二、CoreToolkit.Desktop

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T2.1 | **ObservableValidator** | 中 | 继承 ObservableObject + INotifyDataErrorInfo，支持数据验证 |
| T2.2 | **Messenger / Mediator** | 低 | 弱引用消息总线，用于 ViewModel 间通信 |
| T2.3 | **DialogService** | 低 | 抽象弹窗服务，便于单元测试 |

### 三、MotionTest.WPF

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T3.1 | **Safety 模块单元测试扩展** | 高 | 补充 CollisionDetector、InterlockEngine 的单元测试 |
| T3.2 | **StateMachine 可视化设计器** | 中 | WPF 拖拽式流程设计器 |
| T3.3 | **实时轨迹图表增强** | 中 | 多轴同步显示、缩放、测量工具 |
| T3.4 | **MES 模拟器** | 低 | 模拟 MES 服务端用于离线测试 |

### 四、LicenseManager.WPF

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T4.1 | **授权码有效期管理** | 高 | LicenseCode 字段已存在但未使用，需添加有效期/类型/状态管理 |
| T4.2 | **客户信息管理** | 高 | 添加客户表，关联授权记录 |
| T4.3 | **数据备份/恢复** | 中 | 调用 SqliteDbContext.BackupDatabase，UI 增加备份按钮 |
| T4.4 | **打印/打印预览** | 中 | 授权记录报表打印 |
| T4.5 | **邮件通知** | 低 | 授权即将过期邮件提醒 |
| T4.6 | **数据加密存储** | 低 | 使用 LicenseSerializer 的 AES 加密存储机器码 |

### 五、测试与质量

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T5.1 | **CoreToolkit 单元测试覆盖** | 高 | 当前仅 Safety 有测试，需覆盖 Data、StateMachine、Motion |
| T5.2 | **集成测试** | 中 | CoreToolkit.Tests 硬件联调测试完善 |
| T5.3 | **性能基准测试** | 低 | FileIndexTest 持续优化 |

### 六、工程与文档

| # | 功能 | 优先级 | 说明 |
|---|------|--------|------|
| T6.1 | **AGENTS.md 项目规范** | 中 | 编写 Agent 开发规范（编码风格、提交规范） |
| T6.2 | **API 文档生成** | 低 | 基于 XML 注释生成 HTML/API 文档 |
| T6.3 | **CI/CD 构建脚本** | 低 | GitHub Actions / Azure DevOps 自动构建 |

---

## 📁 快速索引

| 模块 | README 路径 | 核心代码路径 |
|------|-------------|--------------|
| Algorithm | `CoreToolkit/Algorithm/README.md` | `CoreToolkit/Algorithm/` |
| Common | `CoreToolkit/Common/README.md` | `CoreToolkit/Common/` |
| Communication | `CoreToolkit/Communication/README.md` | `CoreToolkit/Communication/` |
| Data | `CoreToolkit/Data/README.md` | `CoreToolkit/Data/` |
| Equipment | `CoreToolkit/Equipment/README.md` | `CoreToolkit/Equipment/` |
| Files | `CoreToolkit/Files/README.md` | `CoreToolkit/Files/` |
| MES | `CoreToolkit/MES/README.md` | `CoreToolkit/MES/` |
| Motion | `CoreToolkit/Motion/README.md` | `CoreToolkit/Motion/` |
| Safety | `CoreToolkit/Safety/README.md` | `CoreToolkit/Safety/` |
| StateMachine | `CoreToolkit/StateMachine/README.md` | `CoreToolkit/StateMachine/` |
| Vision | `CoreToolkit/Vision/README.md` | `CoreToolkit/Vision/` |
| Desktop | `CoreToolkit.Desktop/README.md` | `CoreToolkit.Desktop/` |
| MotionTest.WPF | `MotionTest.WPF/README.md` | `MotionTest.WPF/` |
| LicenseManager.WPF | `LicenseManager.WPF/README.md` | `LicenseManager.WPF/` |

---

*最后更新：2026-04-21*
