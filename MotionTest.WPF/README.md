# MotionTest.WPF 测试程序

基于 WPF 的 PCI-1285 运动控制卡测试与演示程序，集成轴控制、IO 监控、安全防护、流程控制、轨迹可视化等功能。

## 目录结构

```
MotionTest.WPF/
├── Controls/
│   └── TrajectoryChart.xaml      # 轨迹图表自定义控件
├── ViewModels/
│   ├── SafetyEventHistoryViewModel.cs   # 安全事件历史查询 VM
│   ├── SafetyMonitorViewModel.cs        # 安全监控 VM
│   ├── SafetyReportViewModel.cs         # 安全统计报表 VM（旧版）
│   └── SafetyStatisticsViewModel.cs     # 安全统计报表 VM（新版，支持天/周/月）
├── Views/
│   └── SafetyStatisticsWindow.xaml      # 安全统计报表窗口
├── App.xaml / App.xaml.cs
├── FlowControlWindow.xaml / .xaml.cs    # 流程控制窗口
├── MainWindow.xaml / .xaml.cs           # 主窗口（轴控制、IO、安全入口）
├── SafetyEventHistoryWindow.xaml / .xaml.cs  # 安全事件历史窗口
├── SafetyMonitorWindow.xaml / .xaml.cs  # 安全监控窗口
└── SafetyReportWindow.xaml / .xaml.cs   # 安全统计报表窗口（旧版）
```

## 功能模块

| 窗口/功能 | 说明 |
|-----------|------|
| **主窗口** | 轴参数配置、单轴点动/定位、IO 监控、安全功能入口 |
| **安全监控窗口** | 实时显示安全体积、碰撞状态、互锁规则、急停按钮 |
| **安全事件历史** | 按时间范围/级别/关键词查询安全日志，支持 CSV/JSON 导出 |
| **安全统计报表** | 按天/周/月统计碰撞/互锁/急停次数，均值/峰值洞察，双击穿透查详情 |
| **流程控制窗口** | 状态机流程编排与执行监控 |
| **轨迹图表** | 实时显示多轴运动轨迹曲线 |

## 启动方式

```bash
dotnet run --project MotionTest.WPF/MotionTest.WPF.csproj
```

或从 Visual Studio 直接启动 `MotionTest.WPF` 项目。

## 依赖

- CoreToolkit
- CoreToolkit.Desktop
