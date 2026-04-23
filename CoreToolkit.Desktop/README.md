# CoreToolkit.Desktop WPF 桌面支持模块

为 WPF 应用程序提供轻量级 MVVM 基础设施：可通知对象、命令、值转换器和行为。不依赖任何第三方 MVVM 框架，保持最小化设计。

## 目录结构

```
CoreToolkit.Desktop/
├── Behaviors/
│   └── MouseBehavior.cs              # 鼠标事件附加行为（Down/Up/DoubleClick/Wheel/Enter/Leave）
├── Converters/
│   ├── BooleanToVisibilityConverter.cs  # bool ↔ Visibility（支持反转、Collapse/Hidden、ConverterParameter）
│   ├── BoolToBrushConverter.cs          # bool ↔ Brush（支持 ConverterParameter 指定颜色）
│   └── InverseBooleanConverter.cs       # bool 取反（安全的 null/UnsetValue 处理）
└── MVVM/
    ├── AsyncRelayCommand.cs          # 异步命令（线程安全、可绑定 IsExecuting、异常捕获）
    ├── ObservableObject.cs           # 可通知对象基类（支持变更前/后通知、批量通知）
    ├── RelayCommand.cs               # 同步命令
    └── RelayCommandT.cs              # 带参数的强类型命令（安全类型检查）
```

## 核心类说明

### ObservableObject
实现 `INotifyPropertyChanged` + `INotifyPropertyChanging`，提供：
- `SetProperty<T>(ref T, T, string)` — 属性变更通知
- `SetProperty<T>(ref T, T, Action onChanging, Action onChanged, string)` — 变更前/后回调
- `OnPropertyChanged(params string[])` — 批量通知多个属性
- `NotifyAllPropertiesChanged()` — 通知所有绑定属性已变更（`string.Empty`）
- **线程安全**：事件 handler 本地缓存后再调用，避免订阅者取消订阅时的竞态条件

### RelayCommand / RelayCommand&lt;T&gt;
- `RelayCommand<T>` 使用 `parameter is T` 模式进行**安全类型检查**，避免 `InvalidCastException`
- 提供强类型 `Execute(T parameter)` 方法，避免值类型装箱

### AsyncRelayCommand
- **线程安全**：使用 `Interlocked.CompareExchange` 防止异步命令重入
- **可绑定状态**：暴露 `IsExecuting` 属性（继承 ObservableObject），XAML 可直接绑定显示加载动画
- **异常捕获**：`ExecutionException` 属性记录最近一次异常
- `ExecuteAsync()` 返回 `Task`，支持 await

### Converters
所有转换器均已增强：
- 正确处理 `null` / `DependencyProperty.UnsetValue`，返回 `Binding.DoNothing`
- `BooleanToVisibilityConverter` 支持 `ConverterParameter="Invert,Hidden"` 快速配置
- `BoolToBrushConverter` 支持 `ConverterParameter="#FF2196F3"` 通过颜色字符串覆盖

### MouseBehavior
附加行为支持的事件：
- `PreviewMouseDownCommand` / `PreviewMouseUpCommand`
- `MouseDoubleClickCommand`
- `MouseWheelCommand`
- `MouseEnterCommand` / `MouseLeaveCommand`

未设置 CommandParameter 时，自动传入事件参数（`MouseButtonEventArgs` / `MouseWheelEventArgs`）。

## 使用示例

```xml
<!-- XAML: 异步命令绑定 + 加载状态 -->
<Button Content="保存" Command="{Binding SaveCommand}"/>
<ProgressBar IsIndeterminate="True" 
             Visibility="{Binding SaveCommand.IsExecuting, Converter={StaticResource BoolToVis}}"/>

<!-- XAML: 鼠标行为 -->
<Border behaviors:MouseBehavior.MouseDoubleClickCommand="{Binding OpenDetailCommand}"
        behaviors:MouseBehavior.MouseWheelCommand="{Binding ZoomCommand}">
    <Image Source="..."/>
</Border>
```

```csharp
// ViewModel
public class MyViewModel : ObservableObject
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public AsyncRelayCommand SaveCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, () => !string.IsNullOrEmpty(Name));
    }

    private async Task ExecuteSaveAsync()
    {
        await Task.Delay(1000);
        // 保存逻辑...
    }
}
```

## 依赖

- PresentationCore / PresentationFramework
- WindowsBase
