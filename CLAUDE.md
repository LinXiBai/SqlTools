# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概览

SqlDemo / CoreToolkit 是一个基于 **.NET Framework 4.7.2** 的工业设备运动控制核心工具库，面向 SMT/贴片机/半导体封装设备场景。包含数据库访问层、运动控制卡抽象层、安全防护层、状态机流程引擎、WPF MVVM 辅助库及测试程序。

**技术栈**：C# (LangVersion 7.3~8.0) / WPF / SQLite + Dapper / xUnit + Moq

## 常用命令

```bash
# 还原依赖
dotnet restore

# 编译整个解决方案
dotnet build

# 运行单元测试（使用 xUnit + Moq）
dotnet test CoreToolkit.UnitTests

# 运行单个测试类
dotnet test CoreToolkit.UnitTests --filter "FullyQualifiedName~SafetyMonitorViewModelTests"

# 运行单个测试方法
dotnet test CoreToolkit.UnitTests --filter "FullyQualifiedName~Constructor_InitialState_ShouldBeNotInitialized"

# 控制台测试程序
dotnet run --project CoreToolkit.Tests

# WPF 测试应用（需 Windows 环境）
dotnet run --project MotionTest.WPF

# 许可证管理器
dotnet run --project LicenseManager.WPF
```

## 项目分层架构

解决方案包含 5 个项目，引用关系严格单向：

```
CoreToolkit                  ← 核心库（与 UI 无关）
  ↑
CoreToolkit.Desktop          ← WPF MVVM 基础设施
  ↑                    ↑
MotionTest.WPF      LicenseManager.WPF
  ↑
CoreToolkit.UnitTests        ← xUnit + Moq 单元测试
```

**关键约束**：`CoreToolkit` 必须保持与 UI 无关，所有 WPF 相关逻辑只能放在 `CoreToolkit.Desktop`。违反此约束会导致核心库在非 WPF 场景（如后台服务、控制台工具）中无法使用。

### 核心模块架构

**数据层 `CoreToolkit.Data`**：
- `SqliteDbContext` 使用 `SemaphoreSlim` 保证同数据库文件的并发安全，默认启用 WAL 模式
- `RepositoryBase<T>` 为泛型仓储基类，约定：表名 = 类名复数，列名 = 属性名
- `AutoAddMissingColumns` 自动检测实体新增属性并在表末尾加列（无法指定列位置）
- 多数据库通过 `DatabaseConfig` + `DatabaseFactory` 管理

**运动控制 `CoreToolkit.Motion`**：
- `IMotionCard` 统一抽象不同品牌控制卡（研华 PCI-1203/PCI-1285、雷赛 DMCE-3000）
- `MotionCardFactory` / `IOCardFactory` 创建具体实例
- `AxisSpeedController` 管理速度曲线、加减速、S 曲线参数
- 研华 SDK 引用 `AdvMotAPI.dll`（仅 Windows），雷赛 SDK 当前未启用

**状态机 `CoreToolkit.StateMachine`**：
- `FlowModuleBase` 是所有流程模块的基类，提供状态管理、超时检测、统计
- 容器模块：`SequentialModule`（顺序，支持断点续传）、`ParallelModule`（并行，任一失败取消其他）、`ConditionalModule`、`LoopModule`
- `ExecutionContext` 为 `ConcurrentDictionary` 线程安全实现，在模块间传递数据
- **断点续传**：`SequentialModule` 内部通过 `context.SetResult(resumeKey, index)` 记录执行索引；`StateMachineManager.RestoreMachineAsync` 从 `StateMachineRecord` 的 `ResumeDataJson` 恢复上下文并从断点继续

**安全防护 `CoreToolkit.Safety`**（五层模型）：
- Layer 5: `SafeMotionController.PreMoveCheck` — 运动前碰撞预测
- Layer 4: `InterlockEngine` — 区域互锁/禁区规则引擎
- Layer 3: `SoftLimitGuard` — 软限位守卫
- Layer 2: 硬限位/伺服报警（`IMotionCard` 层面）
- Layer 1: `SafetyMonitor` — 100ms 周期后台实时动态检测
- `CollisionDetector` 基于 AABB 包围盒，支持 `Dynamic`/`Static`/`Temporary` 三种体积类型
- `DualHeadAntiCollision` 针对双贴装头同横梁场景
- 状态机集成：`SafetyCheckModule`（安全检查节点）、`SafeAxisMoveModule`（自带防护的轴运动）

**WPF MVVM `CoreToolkit.Desktop`**：
- 轻量级基础设施，不引入 Prism/MVVM Light 等重型框架
- `ObservableObject` 提供 `SetProperty` + 批量通知 + 线程安全事件调用
- `AsyncRelayCommand` 使用 `Interlocked` 重入保护，可绑定 `IsExecuting`
- XAML 绑定约定：`Mode=TwoWay, UpdateSourceTrigger=PropertyChanged`

## 测试策略

- 单元测试集中在 `CoreToolkit.UnitTests`，使用 **xUnit + Moq**
- `SafetyMonitorViewModelTests` 使用 `Mock<IMotionCard>` 模拟硬件，需在 STA 线程创建 WPF Dispatcher
- 数据库测试使用临时 SQLite 文件（`Path.GetTempPath()`），`Dispose` 中清理
- **安全模块修改后必须补充单元测试**（项目硬性要求）

## 编码规范

- 命名：PascalCase（类/方法/属性）、camelCase（局部变量/参数）、`_camelCase`（私有字段）
- 异步方法以 `Async` 结尾
- 公共 API 必须添加 XML 文档注释
- 优先使用 `var`；Halcon API 优先用显式类型
- 异常处理：捕获具体类型，不静默吞掉
- XAML 生产代码不加设计时 `d:` 属性
- 所有回复、代码注释、文档、提交信息使用 **中文**
- 文件读写统一使用 **UTF-8** 编码

## 数据库变更注意事项

SQLite 无版本迁移工具，Schema 变更需兼容现有数据：
- 新增表：在 `SqliteDbContext.InitDatabase()` 中添加 `CREATE TABLE IF NOT EXISTS`
- 新增列：依赖 `AutoAddMissingColumns` 自动在表末尾添加
- 修改列类型/删除列：需手动写迁移 SQL，无法自动处理
- 首次运行后自动生成 `main.db`、`logs.db`、`archive.db`

## 状态机持久化与断点续传

`StateMachineManager` 可接受 `StateMachineRecordRepository` 实现持久化：
- 状态机完成/异常时自动写入 `StateMachineRecord` 表（含 `ModuleStatsJson`、`ResumeDataJson`）
- `RestoreMachineAsync(machineName)` 从最近一条 `Error` 记录恢复 `ExecutionContext`，重建状态机并从断点模块继续执行
- `SequentialModule` 的 `resumeKey` 格式为 `"SequentialModule:{Name}"`

## 扩展模式

- **新增控制卡**：在 `CoreToolkit/Motion/Providers` 下创建厂商文件夹 → 实现 `IMotionCard` → 在 `MotionCardFactory` 注册
- **新增数据表**：`CoreToolkit/Data/Models` 继承 `EntityBase` → `CoreToolkit/Data/Database` 继承 `RepositoryBase<T>` → `SqliteDbContext.InitDatabase()` 添加建表 SQL
- **新增状态机模块**：继承 `FlowModuleBase` → 重写 `ExecuteInternalAsync`
- **新增安全体积**：在 `CollisionDetector` 注册 `SafetyVolume`，通过 `LinkedAxes` 关联轴索引实现位置同步

## 提交规范

- 前缀：`feat:` / `fix:` / `refactor:` / `docs:` / `test:` / `chore:`
- 不自动执行 `git commit` / `git push`，仅汇总变更清单由用户自行决定
- 提交前检查：编译通过、无敏感信息泄露、`.vs/` / `bin/` / `obj/` 未被误跟踪
