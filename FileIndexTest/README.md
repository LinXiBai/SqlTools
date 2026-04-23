# FileIndexTest 文件索引性能测试

专门用于测试和对比 CoreToolkit.Files 模块中文件索引相关类的性能表现。包含多种实现版本的对比测试和基准测试。

## 测试文件

| 文件 | 说明 |
|------|------|
| `CompareTest.cs` | 多版本文件索引实现的功能对比测试 |
| `HighPerfTest.cs` | 高性能场景压力测试 |
| `QuickPerfTest.cs` | 快速性能基准测试 |
| `V2Test.cs` | FileIndexManagerV2 专项测试 |
| `Program.cs` | 测试入口，支持命令行参数选择测试类型 |

## 测试项目

| 项目 | 说明 |
|------|------|
| `CompareTest.csproj` | 对比测试 |
| `FileIndexTest.csproj` | 基础功能测试 |
| `HighPerfTest.csproj` | 高性能测试 |
| `PerformanceTest.csproj` | 综合性能测试 |
| `QuickPerfTest.csproj` | 快速基准测试 |

## 运行方式

```bash
dotnet run --project FileIndexTest/PerformanceTest.csproj
```

## 依赖

- CoreToolkit
