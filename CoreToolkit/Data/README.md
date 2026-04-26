# Data 数据持久化模块

基于 SQLite + Dapper 的数据访问层，提供数据库上下文、仓储基类和具体实体仓储实现。支持日志、用户、轴参数、IO 参数、许可证记录等实体的 CRUD 操作。

## 目录结构

```
Data/
├── Database/
│   ├── AxisParameterRepository.cs    # 轴参数仓储
│   ├── DatabaseConfig.cs             # 数据库配置
│   ├── DatabaseFactory.cs            # 数据库上下文工厂
│   ├── IOParameterRepository.cs      # IO 参数仓储
│   ├── LicenseRecordRepository.cs    # 许可证记录仓储
│   ├── LogRepository.cs              # 日志仓储（含安全事件统计报表）
│   ├── RepositoryBase.cs             # 泛型仓储基类
│   ├── SqliteDbContext.cs            # SQLite 数据库上下文
│   └── UserRepository.cs             # 用户仓储
└── Models/
    ├── Attributes.cs                 # 控件类型枚举/特性
    ├── AxisParameter.cs              # 轴参数实体
    ├── EntityBase.cs                 # 实体基类（INotifyPropertyChanged）
    ├── IOParameter.cs                # IO 参数实体
    ├── LicenseRecord.cs              # 许可证记录实体
    ├── Log.cs                        # 日志实体（级别/消息/时间戳）
    ├── SafetyEventStatistics.cs      # 安全事件统计模型
    └── User.cs                       # 用户实体
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `SqliteDbContext` | SQLite 连接管理、表初始化、事务支持、执行 SQL/Dapper 查询 |
| `RepositoryBase<T>` | 泛型仓储基类，提供 `GetAll/GetById/Insert/Update/Delete/BulkInsert` |
| `LogRepository` | 日志专用仓储：按时间/级别查询、批量写入、安全事件统计报表（按天/周/月聚合） |
| `AxisParameterRepository` | 轴参数（速度、加速度、脉冲当量等）的持久化与查询 |
| `EntityBase` | 所有实体的基类，实现 `INotifyPropertyChanged`，提供 `SetProperty<T>` |

## 使用示例

```csharp
// 创建数据库上下文
using var db = new SqliteDbContext("data/main.db", "MainDb", enableLogging: true);

// 使用仓储
var logRepo = new LogRepository(db);
logRepo.WriteInfo("System", "系统启动完成");

// 安全事件统计
var dailyStats = logRepo.GetSafetyEventDailyStats(
    DateTime.Now.AddDays(-7), DateTime.Now);
foreach (var stat in dailyStats)
{
    Console.WriteLine($"{stat.Date:yyyy-MM-dd}: 碰撞{stat.CollisionCount} 互锁{stat.InterlockCount} 急停{stat.EStopCount}");
}

// 轴参数
var axisRepo = new AxisParameterRepository(db);
var param = axisRepo.GetByCardIdAndAxis(0, 0);
param.MaxSpeed = 50000;
axisRepo.Update(param);
```

## 依赖

- System.Data.SQLite
- Dapper
- Newtonsoft.Json
