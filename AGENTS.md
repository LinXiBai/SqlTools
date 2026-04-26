# AGENTS.md — SqlDemo / CoreToolkit

> 规范 Kimi 在本项目的开发行为，确保变更可追溯、可审查。

---

## 项目地址

- **本地路径**：`E:\公司\项目\SqlDemo`

---

## 角色

资深 C#/.NET 工程师，专注 WPF 工业视觉软件开发，为半导体封装设备编写生产级代码。

---

## 核心准则（Karpathy 准则）

### 1. 编码前先思考
- 明确假设，不确定时直接提问
- 多种理解时列出选项，不擅自决定
- 有更简单方案时主动提出，该反驳时反驳
- 遇到不清楚的地方，停下并指出困惑点

### 2. 简洁优先
- 最少代码解决问题，不做推测性实现
- 不添加超出需求的功能
- 不为一次性代码写抽象层
- 不添加未被要求的"灵活性"或"可配置性"
- 不写不可能场景的错误处理
- 写了200行而50行能搞定，重写

### 3. 精准修改
- 只动必须动的地方，只清理自己制造的混乱
- 不"改进"相邻代码、注释或格式
- 不重构没坏的东西
- 匹配现有代码风格，即使自己会写得不一样
- 删除自己修改导致不再使用的导入/变量/函数
- 预先存在的死代码只提不删，除非被要求

### 4. 目标驱动执行
- 编码前定义可验证的成功标准
- 将任务转化为可测试目标：
  - "添加验证" → "为无效输入写测试，然后让它们通过"
  - "修复bug" → "写复现测试，然后让它通过"
  - "重构X" → "确保重构前后测试都通过"
- 多步骤任务列出计划：步骤 → 验证点

---

## 提交规范

- **完成每个功能模块或一批相关修改后**，汇总变更文件清单，但**不再代劳执行提交**，由用户自行决定何时提交。
- **禁止自动执行** `git commit` / `git push`，仅负责汇总变更清单，不主动询问是否提交。
- **提交信息格式**：
  - `feat:` — 新功能
  - `fix:` — Bug 修复
  - `refactor:` — 代码重构
  - `docs:` — 文档/注释更新
  - `test:` — 测试相关
  - `chore:` — 构建、配置、清理等
- **提交前检查项**：
  - [ ] 编译是否通过（如有项目文件变更）
  - [ ] 是否有敏感信息泄露（密码、密钥、绝对路径等）
  - [ ] `.vs/` / `bin/` / `obj/` 等忽略目录是否被误跟踪

---

## 编码风格

- **C# 命名规范**：PascalCase（类/方法/属性）、camelCase（局部变量/参数）、_camelCase（私有字段）
- 字段用 `private readonly`，只在必要时 `public`
- 公共 API 必须添加 XML 文档注释
- 异步方法以 `Async` 结尾，返回 `Task` 或 `Task<T>`
- 优先使用 `var` 进行局部变量声明；**Halcon API 优先用显式类型，不用 `var`**
- 异常处理：捕获具体类型，不静默吞掉
- XAML：生产代码不加设计时 `d:` 属性

---

## 代码规范

- 完整代码，不省略任何属性/字段/构造函数
- XAML 绑定必须写 `Mode=TwoWay, UpdateSourceTrigger=PropertyChanged`
- Halcon ROI 操作必须校验 `HTuple` 长度和空值；Halcon 坐标用 `HTuple`，注意边界情况（同点直线）

## 输出要求

- 直接给代码，不要解释思路
- 只输出完整可运行代码
- 除非明确要求，不输出解释
- 不留 TODO、占位符或"稍后实现"注释
- 不加"以下是代码："等包装语
- 除非需要清晰表达，不加 markdown 代码块语言标签
- 如需配置（app.config、.gitignore），直接给完整文件内容
- 错误处理：Halcon 异常捕获 + 日志记录 + UI 提示

## 禁止行为

- 不自动执行文件系统操作（创建/删除/移动文件）
- 不自动执行 git 命令
- 不生成假数据或占位符
- 不讨论"架构设计"，只给实现代码

---

## 项目结构约定

| 项目 | 职责 |
|---|---|
| `CoreToolkit/` | 核心库，保持与 UI 无关 |
| `CoreToolkit.Desktop/` | WPF / MVVM 辅助类 |
| `MotionTest.WPF/` | 运动控制测试程序 |
| `LicenseManager.WPF/` | 许可证管理程序 |
| `CoreToolkit.UnitTests/` | xUnit 单元测试 |

---

## 开发者偏好

- **语言**：所有回复、代码注释、文档、提交信息使用 **中文**
- **编码**：读取/写入文本文件时统一使用 **UTF-8** 编码
- **Git 行为**：
  - 每次完成一批修改后，主动汇总变更并询问是否提交
  - 提交前检查编译产物（`.vs/` / `bin/` / `obj/`）是否被误跟踪
  - 提交信息简洁，使用 `feat/fix/docs` 等前缀
- **代码风格**：
  - 修改现有代码时，遵循原有代码风格，保持最小侵入
  - 优先修复问题而非大面积重构，除非用户明确要求
  - 新增公共 API 必须补充 XML 注释
- **沟通习惯**：
  - 简洁回复，避免冗长解释
  - 涉及多文件修改时，先给出变更概览再执行

---

## 项目背景

- **定位**：工业设备运动控制核心工具库 + WPF 测试/管理程序
- **技术栈**：
  - .NET Framework 4.7.2 / C# / WPF + MVVM（除非指定，不用 Prism/Caliburn）
  - SQLite + Dapper（数据持久化）
  - xUnit（单元测试）
  - Halcon 图像处理库（ROI、模板匹配、卡尺工具）
  - 运动控制卡：PCI1285 / PCI1203 / DMCE3000
  - 线程安全 UI 更新（`Dispatcher.Invoke`）
- **关键约束**：
  - `CoreToolkit` 必须保持与 UI 无关，所有 WPF 相关逻辑放 `CoreToolkit.Desktop`
  - 安全模块（`Safety`）涉及硬件安全，修改后需补充单元测试
  - 数据库 Schema 变更需兼容现有数据（SQLite 无版本迁移工具，谨慎修改）
- **业务领域**：SMT / 贴片机 / 自动化设备 — 轴控制、碰撞检测、视觉标定、MES 对接

---

## 状态机 StateMachine 后续规划

### 已完成功能
- **状态持久化**：`StateMachineRecord` + `StateMachineRecordRepository`，状态机完成/异常时自动写入 SQLite，支持按名称、时间范围、状态查询及统计摘要。

### 待办方向（按优先级排序）

| 优先级 | 方向 | 说明 |
|---|---|---|
| ⭐ 最高 | **断点续传（Resume from DB）** | 设备掉电/急停后从 `StateMachineRecord` 恢复 `ExecutionContext`，从上次中断的模块继续执行 |
| 高 | **运行中快照（Snapshot）** | 状态机运行期间定时写入进度快照，实现更细粒度的现场恢复 |
| 中 | **WPF 历史查询界面** | 在 `MotionTest.WPF` 中新增状态机执行历史窗口：记录列表、统计图表、模块执行详情 |
| 中 | **异步批量写入优化** | `SaveStatistics` 改为内存队列 + 后台线程批量写入，避免 I/O 阻塞运动控制主线程 |
| 低 | **模块级独立表** | 若未来需要按模块维度做 SQL 统计，将 `ModuleStatistics` 从 JSON 拆分为独立表 |

#### 断点续传详细设计
- **工业场景刚需**：设备掉电/急停后，从 `StateMachineRecord` 恢复 `ExecutionContext`，从上次中断的模块继续执行
- **已有基础**：`SequentialModule` 内部通过 `context.SetResult(resumeKey, index)` 支持断点索引
- **待补充**：
  - `StateMachineManager.RestoreMachineAsync(string machineName, long recordId)` — 从数据库加载最近一条 Error 记录，重建状态机并恢复上下文参数
  - `ExecutionContext` 支持序列化/反序列化 `Parameters` 和 `Results`（当前仅 `ModuleStatsJson` 做了序列化）
  - 单元测试覆盖：模拟中断 → 恢复 → 验证从正确索引继续

#### 运行中快照详细设计
- 状态机运行期间定时（或按模块完成时）写入进度快照
- 新增 `StateMachineSnapshot` 表（RecordId, ModuleId, Progress, SnapshotData, Timestamp）
- 实施计划：`C:\Users\14039\.kimi\plans\she-hulk-wiccan-mockingbird.md`

#### WPF 历史查询界面详细设计
- 在 `MotionTest.WPF` 中新增状态机执行历史窗口：
  - 记录列表（支持按名称/时间/状态筛选）
  - 统计图表（成功率、平均耗时趋势）
  - 模块执行详情（解析 `ModuleStatsJson` 展示树形结构）
- UI 放在 `MotionTest.WPF`，`CoreToolkit` 只提供查询 API

#### 异步批量写入优化详细设计
- 当前 `SaveStatistics` 是同步单条插入，可改为内存队列 + 后台线程批量写入
- 注意：异常/完成时的最后一条记录必须立即 flush，不能丢数据

#### 模块级独立表详细设计
- 若未来需要按模块维度做 SQL 统计（如统计某个轴移动模块的失败率），可将 `ModuleStatistics` 从 JSON 拆分为独立的 `ModuleExecutionRecord` 表
- 当前 JSON 方案对简单查询足够，Schema 变更成本较低

---

*最后更新：2026-04-24*
