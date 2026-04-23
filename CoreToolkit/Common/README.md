# Common 通用工具模块

跨项目共享的基础工具类与通用模型，所有其他模块均可依赖此模块。

## 目录结构

```
Common/
├── Helpers/
│   ├── DateTimeHelper.cs        # 日期时间格式化与解析
│   ├── EnumHelper.cs            # 枚举描述/解析扩展
│   ├── Guard.cs                 # 参数守卫（前置条件检查）
│   ├── JsonHelper.cs            # JSON 序列化/反序列化封装
│   ├── LicenseSerializer.cs     # 许可证序列化
│   ├── MathHelper.cs            # 数学工具（插值、取整、精度处理）
│   ├── ProcessHelper.cs         # 进程管理辅助
│   ├── RetryHelper.cs           # 重试策略（指数退避）
│   └── StringHelper.cs          # 字符串扩展方法
└── Models/
    ├── PathConfig.cs            # 路径配置模型
    └── Result.cs                # 通用结果封装（成功/失败模式）
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `Result<T>` | 操作结果封装，避免使用异常控制流程。`Result.Success(value)` / `Result.Failure(message)` |
| `Guard` | 参数校验工具：`Guard.NotNull(obj, nameof(obj))`、`Guard.IsTrue(condition)` |
| `RetryHelper` | 支持重试次数、退避间隔、异常过滤的重试执行器 |
| `JsonHelper` | 基于 Newtonsoft.Json 的封装，提供默认配置（忽略循环引用、日期格式等） |
| `MathHelper` | 线性插值、限制在范围内、近似相等比较、角度归一化 |

## 使用示例

```csharp
// Result 模式
public Result<double> Divide(double a, double b)
{
    if (b == 0) return Result<double>.Failure("除数不能为零");
    return Result<double>.Success(a / b);
}

// 参数守卫
public void MoveTo(double position)
{
    Guard.IsTrue(position >= 0, nameof(position), "位置不能为负数");
    // ...
}

// 重试执行
var result = RetryHelper.Execute(() => ConnectDevice(), maxRetries: 3, delayMs: 500);
```

## 依赖

- Newtonsoft.Json
