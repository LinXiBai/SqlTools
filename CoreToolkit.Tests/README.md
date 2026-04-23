# CoreToolkit.Tests 集成测试程序

针对 CoreToolkit 各模块的集成测试与硬件联调控制台程序。测试覆盖轴组控制、运动卡通信、文件索引、状态机等实际硬件相关功能。

## 测试文件

| 文件 | 说明 |
|------|------|
| `AxisGroupTest.cs` | 轴组联动测试：直线插补、圆弧插补、同步启停 |
| `PCI1285Test.cs` | PCI-1285 运动卡基础功能测试：单轴运动、IO 读写、参数配置 |
| `StateMachineTest.cs` | 状态机流程测试：模块编排、异常回滚、到位检测 |
| `FileIndexSearcherTest.cs` | 文件索引搜索功能测试 |
| `FileIndexSearcherDirectTest.cs` | 文件索引直连测试 |
| `FileIndexSearcherQuickTest.cs` | 文件索引快速测试 |
| `FileIndexTestProgram.cs` | 文件索引综合测试程序入口 |
| `Program.cs` | 主入口，可选择运行指定测试套件 |

## 运行方式

```bash
dotnet run --project CoreToolkit.Tests/CoreToolkit.Tests.csproj
```

运行前请确保运动控制卡驱动已正确安装且硬件已连接。

## 依赖

- CoreToolkit
- 研华 AdvMotAPI.dll（PCI-1285 驱动）
