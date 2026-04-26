# AxisFrameworkTest 轴框架测试

独立的轴控制框架测试控制台程序，用于验证 AxisFramework 的基本功能，不依赖完整的 CoreToolkit 测试体系。

## 功能

- 单轴基础运动测试（点动、定位、回零）
- 轴参数读写验证
- 伺服使能/报警状态检测

## 运行方式

```bash
dotnet run --project AxisFrameworkTest/AxisFrameworkTest.csproj
```

## 依赖

- CoreToolkit
- 研华 AdvMotAPI.dll
