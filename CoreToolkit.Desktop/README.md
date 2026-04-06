# CoreToolkit.Desktop

基于 **WPF** 的轻量级 **MVVM 基础设施库**，目标框架 **.NET Framework 4.7.2**。为 `MotionTest.WPF` 及未来其他 WPF 项目提供统一的视图模型基类、命令实现、值转换器和附加行为。

***

## 设计原则

- **轻量**：不依赖 Prism、MVVM Light 等重型框架，纯手写核心类，源码可控。
- **够用**：覆盖 WPF 开发中最常用的 80% 场景（命令绑定、属性通知、值转换、鼠标行为）。
- **可扩展**：所有类均为 `partial` / `virtual` 友好设计，方便项目内二次定制。

***

## 目录结构

```
CoreToolkit.Desktop/
├── CoreToolkit.Desktop.csproj    # WPF 类库项目文件
├── README.md                     # 本文件
├── MVVM/
│   ├── ObservableObject.cs       # INotifyPropertyChanged 基类
│   ├── RelayCommand.cs           # 无参数 ICommand 实现
│   ├── RelayCommandT.cs          # 带参数 ICommand<T> 实现
│   └── AsyncRelayCommand.cs      # 异步 ICommand 实现
├── Converters/
│   ├── BooleanToVisibilityConverter.cs
│   ├── InverseBooleanConverter.cs
│   └── BoolToBrushConverter.cs
└── Behaviors/
    └── MouseBehavior.cs          # PreviewMouseDown/Up 命令附加行为
```

***

## 核心类型说明

### ObservableObject

所有 ViewModel 的推荐基类，提供属性变更通知。

```csharp
public class MainViewModel : ObservableObject
{
    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
```

支持变更回调：

```csharp
public string Title
{
    get => _title;
    set => SetProperty(ref _title, value, () => OnTitleChanged());
}
```

### RelayCommand / RelayCommand<T>

轻量级 `ICommand` 实现，自动挂接 `CommandManager.RequerySuggested`。

```csharp
public ICommand SaveCommand { get; }

public MainViewModel()
{
    SaveCommand = new RelayCommand(Save, () => CanSave);
}
```

带参数版本：

```csharp
public ICommand DeleteCommand { get; }

public MainViewModel()
{
    DeleteCommand = new RelayCommand<int>(id => Delete(id), id => id > 0);
}
```

### AsyncRelayCommand

执行 `async Task` 的按钮命令，自动处理 `_isExecuting` 状态，防止重复点击。

```csharp
public ICommand LoadDataCommand { get; }

public MainViewModel()
{
    LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
}

private async Task LoadDataAsync()
{
    await Task.Delay(1000);
    // ...
}
```

### Converters

#### BooleanToVisibilityConverter

```xml
<Window.Resources>
    <conv:BooleanToVisibilityConverter x:Key="BoolToVis"/>
</Window.Resources>

<TextBlock Text="加载中..." Visibility="{Binding IsLoading, Converter={StaticResource BoolToVis}}"/>
```

支持属性：
- `IsInverted`：反转逻辑
- `CollapseWhenHidden`：`false` 时返回 `Hidden` 而非 `Collapsed`

#### BoolToBrushConverter

常用于状态指示灯：

```xml
<conv:BoolToBrushConverter x:Key="StatusBrush" TrueBrush="Lime" FalseBrush="Gray"/>
<Ellipse Fill="{Binding IsOn, Converter={StaticResource StatusBrush}}" Width="20" Height="20"/>
```

#### InverseBooleanConverter

```xml
<conv:InverseBooleanConverter x:Key="InverseBool"/>
<Button Content="禁用" IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBool}}"/>
```

### MouseBehavior

将 `PreviewMouseDown` / `PreviewMouseUp` 事件转换为命令绑定，典型用于 JOG 按钮。

```xml
xmlns:beh="clr-namespace:CoreToolkit.Desktop.Behaviors;assembly=CoreToolkit.Desktop"

<Button Content="JOG+"
        beh:MouseBehavior.PreviewMouseDownCommand="{Binding JogStartCommand}"
        beh:MouseBehavior.PreviewMouseDownCommandParameter="1"
        beh:MouseBehavior.PreviewMouseUpCommand="{Binding JogStopCommand}"/>
```

***

## 在 WPF 项目中引用

```xml
<!-- 在项目 .csproj 中添加 -->
<ItemGroup>
    <ProjectReference Include="..\CoreToolkit.Desktop\CoreToolkit.Desktop.csproj" />
</ItemGroup>
```

然后在 XAML 中引入命名空间即可使用：

```xml
xmlns:conv="clr-namespace:CoreToolkit.Desktop.Converters;assembly=CoreToolkit.Desktop"
xmlns:beh="clr-namespace:CoreToolkit.Desktop.Behaviors;assembly=CoreToolkit.Desktop"
```

***

## 扩展建议

| 需求 | 建议添加位置 |
|------|-------------|
| 数据验证基类 | `MVVM/ValidatableObject.cs` |
| 弹窗服务 | `MVVM/DialogService.cs` |
| 页面导航 | `MVVM/NavigationService.cs` |
| 加载遮罩 | `MVVM/BusyIndicatorService.cs` |
| 更多值转换器 | `Converters/` |
| 更多交互行为 | `Behaviors/` |

***

## 依赖

- `.NET Framework 4.7.2`
- `PresentationCore`
- `PresentationFramework`
- `WindowsBase`
