# SqlDemo

基于 **.NET Framework 4.7.2** 的 SQLite 数据库交互示例项目，采用 **Dapper + System.Data.SQLite** 构建轻量级数据访问层（DAL），支持多数据库连接、批量操作、数据导出/导入、多线程安全和异步操作。

---

## 技术栈

| 技术/包 | 版本 | 说明 |
| :--- | :--- | :--- |
| .NET Framework | 4.7.2 | 目标运行框架 |
| System.Data.SQLite.Core | 1.0.118 | SQLite ADO.NET 驱动 |
| Dapper | 2.1.28 | 轻量级 ORM，简化对象映射 |
| Newtonsoft.Json | 13.0.3 | JSON 序列化/反序列化 |

---

## 项目结构

```
SqlDemo/
├── SqlDemo.sln              # 解决方案文件
├── README.md                # 项目说明（本文件）
├── SqlDemo.Data/            # 数据库访问层（DLL 项目）
│   ├── SqlDemo.Data.csproj  # 项目文件
│   ├── Data/                # 数据访问实现
│   │   ├── DatabaseConfig.cs    # 数据库配置管理
│   │   ├── DatabaseFactory.cs   # 数据库上下文工厂
│   │   ├── SqliteDbContext.cs   # 数据库上下文（连接管理与底层执行）
│   │   ├── RepositoryBase.cs    # 通用泛型仓储基类（基础 CRUD）
│   │   ├── UserRepository.cs    # 用户专用仓储（业务扩展查询）
│   │   └── LogRepository.cs     # 日志专用仓储
│   └── Models/              # 实体模型层
│       ├── Attributes.cs    # 自定义特性与枚举
│       ├── EntityBase.cs    # 实体抽象基类（Id、CreatedAt、UpdatedAt）
│       ├── Log.cs           # 日志实体
│       └── User.cs          # 示例实体：用户
├── SqlDemo.Tests/           # 测试项目
│   ├── SqlDemo.Tests.csproj # 项目文件
│   └── Program.cs           # 测试程序
└── SqlDemo/                 # 主项目
    ├── SqlDemo.csproj       # 项目文件
    └── Program.cs           # 控制台入口
```

---

## 快速开始

### 1. 还原依赖

```powershell
dotnet restore
```

### 2. 编译

```powershell
dotnet build
```

### 3. 运行

```powershell
dotnet run
```

> 首次运行后，会在项目根目录自动生成 `main.db`、`logs.db`、`archive.db` 数据库文件。

---

## 架构说明

### 1. SqliteDbContext（数据库上下文）

- 封装 `SQLiteConnection` 的连接打开、关闭与释放。
- 提供统一的 SQL 执行入口：
  - `Execute` / `ExecuteAsync`：非查询语句（INSERT / UPDATE / DELETE）
  - `Query<T>`：查询列表
  - `QuerySingleOrDefault<T>`：查询单条记录
  - `ExecuteScalar<T>`：获取标量值
  - `BeginTransaction`：开启事务
- 内置 `InitDatabase()` 方法，用于首次运行时初始化数据表。
- 支持 WAL 模式配置，提升并发性能。
- **自动添加新列**：当实体类增加属性时，会自动检测并添加对应列到数据库表，无需手动配置。

### 2. RepositoryBase<T>（通用仓储基类）

- 所有实体仓储的泛型基类，约定：**表名 = 类名复数形式**，**列名 = 属性名**。
- 已实现的基础操作：
  - `GetById(long id)` / `GetByIdAsync(long id)`
  - `GetAll()` / `GetAllAsync()`
  - `Insert(T entity)` / `InsertAsync(T entity)` — 返回自增 `rowid`
  - `Update(T entity)` / `UpdateAsync(T entity)`
  - `Delete(long id)` / `DeleteAsync(long id)`
  - `Query(string whereClause, object param)` / `QueryAsync(string whereClause, object param)`
  - `InsertBatch(IEnumerable<T> entities)` / `InsertBatchAsync(IEnumerable<T> entities)` — 带事务批量插入
  - `UpdateBatch(IEnumerable<T> entities)` / `UpdateBatchAsync(IEnumerable<T> entities)` — 带事务批量更新
  - `DeleteBatch(IEnumerable<long> ids)` / `DeleteBatchAsync(IEnumerable<long> ids)` — 带事务批量删除
- **数据导出/导入**：
  - `ExportToJson()` / `ExportToJsonFile(string filePath)` — 导出为 JSON 格式
  - `ExportToCsv()` / `ExportToCsvFile(string filePath)` — 导出为 CSV 格式
  - `ImportFromJson(string json)` / `ImportFromJsonFile(string filePath)` — 从 JSON 导入
  - `ImportFromCsv(string csv)` / `ImportFromCsvFile(string filePath)` — 从 CSV 导入
- 通过反射自动识别简单类型属性生成 SQL，排除复杂对象和导航属性。

### 3. 多数据库支持

- **DatabaseConfig**：管理多个数据库连接配置
- **DatabaseFactory**：统一创建和管理多个数据库上下文
- 支持同时操作多个 SQLite 数据库文件

### 4. 多线程安全

- **连接锁机制**：使用 `SemaphoreSlim` 确保同一数据库文件的并发操作安全
- **线程安全执行**：所有数据库操作都通过锁机制保证线程安全
- **异步锁支持**：提供异步版本的锁机制，支持 `async/await`

### 5. 异步操作支持

- **完整的异步 API**：所有操作都提供异步版本
- **支持 async/await**：使用 `Task` 和 `Task<T>` 返回类型
- **异步事务**：支持异步批量操作和事务处理

### 6. 实体模型增强

- **INotifyPropertyChanged**：所有实体类支持属性变化通知，便于数据绑定
- **FieldAttribute**：为属性添加元数据，包括：
  - 显示名称
  - 分类名称
  - 控件类型（String、Numeric、Bool、ComboBox）
  - 是否隐藏

### 7. 业务仓储

- **UserRepository**：用户业务仓储
  - `SearchByName(string keyword)`：按用户名模糊查询
  - `GetActiveUsers()`：获取所有活跃用户

- **LogRepository**：日志业务仓储
  - `GetByLevel(string level)` / `GetByLevelAsync(string level)`：按级别查询日志
  - `GetRecentLogs(int count)` / `GetRecentLogsAsync(int count)`：查询最近的日志
  - `GetLogsByTimeRange(DateTime startDate, DateTime endDate)` / `GetLogsByTimeRangeAsync(DateTime startDate, DateTime endDate)`：按时间范围查询日志
  - `GetLogsByLevelAndTimeRange(string level, DateTime startDate, DateTime endDate)` / `GetLogsByLevelAndTimeRangeAsync(string level, DateTime startDate, DateTime endDate)`：按级别和时间范围查询日志
  - `WriteDebug(string message, string loggerName = null, string additionalInfo = null)` / `WriteDebugAsync(...)`：写入调试日志
  - `WriteInfo(string message, string loggerName = null, string additionalInfo = null)` / `WriteInfoAsync(...)`：写入信息日志
  - `WriteWarning(string message, string loggerName = null, string additionalInfo = null)` / `WriteWarningAsync(...)`：写入警告日志
  - `WriteError(string message, string loggerName = null, string additionalInfo = null)` / `WriteErrorAsync(...)`：写入错误日志
  - `WriteFatal(string message, string loggerName = null, string additionalInfo = null)` / `WriteFatalAsync(...)`：写入致命错误日志
  - `WriteException(Exception ex, string message = null, string loggerName = null, string additionalInfo = null)` / `WriteExceptionAsync(...)`：写入异常日志
  - `CleanupLogs(DateTime beforeDate)` / `CleanupLogsAsync(DateTime beforeDate)`：清理指定时间之前的日志
  - `CleanupLogsByLevel(string level)` / `CleanupLogsByLevelAsync(string level)`：清理指定级别的日志

### 8. 数据库备份

- **SqliteDbContext**：提供数据库备份方法
  - `BackupDatabase(string backupPath)`：备份数据库到指定路径
  - `BackupDatabaseAsync(string backupPath)`：异步备份数据库到指定路径

- **DatabaseFactory**：提供数据库备份静态方法
  - `BackupDatabase(string configName, string backupPath)`：备份指定数据库
  - `BackupDatabaseAsync(string configName, string backupPath)`：异步备份指定数据库
  - `BackupAllDatabases(string backupDirectory)`：备份所有注册的数据库
  - `BackupAllDatabasesAsync(string backupDirectory)`：异步备份所有注册的数据库

---

## 使用示例

### 基本使用

```csharp
using (var db = new SqliteDbContext("demo.db"))
{
    // 初始化数据库（建表）
    db.InitDatabase();

    var userRepo = new UserRepository(db);

    // 插入
    var user = new User { UserName = "张三", Email = "zhangsan@example.com", Age = 28 };
    long id = userRepo.Insert(user);

    // 查询
    var result = userRepo.GetById(id);

    // 更新
    result.Age = 29;
    userRepo.Update(result);

    // 条件查询
    var activeUsers = userRepo.GetActiveUsers();
}
```

### 多数据库使用

```csharp
// 注册多个数据库配置
DatabaseConfig.Register("MainDb", "main.db");
DatabaseConfig.Register("LogDb", "logs.db");

// 初始化所有数据库
DatabaseFactory.InitializeAllDatabases();

// 使用主数据库
using (var mainDb = DatabaseFactory.CreateContext("MainDb"))
{
    var userRepo = new UserRepository(mainDb);
    // 操作用户数据...
}

// 使用日志数据库
using (var logDb = DatabaseFactory.CreateContext("LogDb"))
{
    var logRepo = new LogRepository(logDb);
    // 操作日志数据...
}
```

### 批量操作

```csharp
using (var db = new SqliteDbContext("demo.db"))
{
    var userRepo = new UserRepository(db);

    // 批量插入
    var users = new List<User>
    {
        new User { UserName = "王五", Email = "wangwu@example.com", Age = 25 },
        new User { UserName = "赵六", Email = "zhaoliu@example.com", Age = 35 }
    };
    userRepo.InsertBatch(users);

    // 批量更新
    var allUsers = userRepo.GetAll().ToList();
    foreach (var u in allUsers)
    {
        u.Age += 1;
    }
    userRepo.UpdateBatch(allUsers);

    // 批量删除
    var userIds = allUsers.Take(2).Select(u => u.Id).ToList();
    userRepo.DeleteBatch(userIds);
}
```

### 数据导出/导入

```csharp
using (var db = new SqliteDbContext("demo.db"))
{
    var userRepo = new UserRepository(db);

    // 导出为 JSON
    userRepo.ExportToJsonFile("users.json");

    // 导出为 CSV
    userRepo.ExportToCsvFile("users.csv");

    // 从 JSON 导入
    userRepo.ImportFromJsonFile("users.json");

    // 从 CSV 导入
    userRepo.ImportFromCsvFile("users.csv");
}
```

### 异步操作

```csharp
using (var db = new SqliteDbContext("demo.db"))
{
    var userRepo = new UserRepository(db);

    // 异步插入
    var user = new User { UserName = "异步用户", Email = "async@example.com", Age = 30 };
    long id = await userRepo.InsertAsync(user);

    // 异步查询
    var result = await userRepo.GetByIdAsync(id);

    // 异步更新
    result.Age = 31;
    await userRepo.UpdateAsync(result);

    // 异步批量操作
    var users = new List<User>
    {
        new User { UserName = "异步用户1", Email = "async1@example.com", Age = 25 },
        new User { UserName = "异步用户2", Email = "async2@example.com", Age = 35 }
    };
    await userRepo.InsertBatchAsync(users);
}
```

### 多线程安全

```csharp
// 多线程操作同一数据库
var tasks = new List<Task>();
for (int i = 0; i < 5; i++)
{
    int threadId = i;
    tasks.Add(Task.Run(() =>
    {
        using (var db = new SqliteDbContext("demo.db"))
        {
            var userRepo = new UserRepository(db);
            var user = new User { UserName = $"线程用户{threadId}", Email = $"thread{threadId}@example.com", Age = 20 + threadId };
            long userId = userRepo.Insert(user);
            Console.WriteLine($"[线程{threadId}] 插入用户，Id = {userId}");
        }
    }));
}
await Task.WhenAll(tasks);
```

### 数据库备份

```csharp
// 备份指定数据库
DatabaseFactory.BackupDatabase("MainDb", "main_backup.db");

// 异步备份指定数据库
await DatabaseFactory.BackupDatabaseAsync("LogDb", "logs_backup.db");

// 备份所有注册的数据库到指定目录
string backupDir = "backups";
DatabaseFactory.BackupAllDatabases(backupDir);

// 异步备份所有注册的数据库
await DatabaseFactory.BackupAllDatabasesAsync(backupDir);

// 使用 SqliteDbContext 直接备份
using (var db = new SqliteDbContext("demo.db"))
{
    db.BackupDatabase("demo_backup.db");
    
    // 异步备份
    await db.BackupDatabaseAsync("demo_backup_async.db");
}
```

---

## 扩展指南

### 新增一张数据表

1. **定义实体**：在 `Models` 文件夹下新建类并继承 `EntityBase`
2. **定义仓储**：在 `Data` 文件夹下新建类并继承 `RepositoryBase<T>`
3. **初始化表结构**：在 `SqliteDbContext.InitDatabase()` 中添加 `CREATE TABLE IF NOT EXISTS` SQL
4. **添加字段属性**：为实体属性添加 `[Field]` 特性，定义元数据

示例：

```csharp
// Models/Order.cs
public class Order : EntityBase
{
    [Field("订单号", "基本信息", ControlType.String)]
    public string OrderNo { get; set; }

    [Field("总金额", "基本信息", ControlType.Numeric)]
    public decimal TotalAmount { get; set; }

    [Field("用户ID", "关联信息", ControlType.Numeric)]
    public long UserId { get; set; }
}

// Data/OrderRepository.cs
public class OrderRepository : RepositoryBase<Order>
{
    public OrderRepository(SqliteDbContext db) : base(db) { }

    public IEnumerable<Order> GetByUser(long userId)
    {
        return Query("UserId = @UserId", new { UserId = userId });
    }
}
```

### 为实体类添加新属性

**只需在实体类中添加属性，无需手动配置数据库！**

1. **在实体类中添加属性**：
   ```csharp
   [Field("电话", "联系信息", ControlType.String)]
   public string Phone { get; set; }
   ```

2. **运行程序**：程序会自动检测并添加新列到数据库表中。

系统会自动：
- 检测数据库表中已存在的列
- 对比实体类的所有公共属性
- 自动添加缺失的列
- 根据属性类型自动选择合适的 SQLite 数据类型（TEXT、INTEGER、REAL、BLOB）

---

## 注意事项

1. **线程安全**：系统已实现线程安全机制，通过 `SemaphoreSlim` 确保同一数据库文件的并发操作安全。推荐每次数据库操作都使用 `using (var db = new SqliteDbContext(...))` 模式。
2. **并发写入**：SQLite 对并发写入支持有限。系统已启用 **WAL（Write-Ahead Logging）** 模式以提升并发性能。
3. **DateTime 存储**：示例采用 `TEXT` 类型存储时间，Dapper 会自动完成 `DateTime` 与字符串的映射转换。
4. **多数据库**：SQLite 不支持跨库分布式事务，跨数据库操作需要在应用层面保证一致性。
5. **新列添加**：SQLite 的 `ALTER TABLE ADD COLUMN` 只能在表末尾添加列，无法指定列的位置。
6. **批量操作**：批量操作使用事务确保原子性，对于大量数据（如超过 1000 条），建议分批处理以避免内存占用过高。
7. **数据导出/导入**：导出/导入操作会加载所有数据到内存，对于大数据量请谨慎使用。
8. **异步操作**：异步操作使用 `async/await` 模式，需要在调用方法上添加 `async` 修饰符。
9. **密码连接**：SQLite 加密功能需要 SQLite Encryption Extension (SEE) 支持，标准 System.Data.SQLite.Core 不包含此功能。

---

## 作者

*项目创建于 2026-04-02*