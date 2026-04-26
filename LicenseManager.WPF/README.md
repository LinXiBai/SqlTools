# LicenseManager.WPF 许可证管理器

WPF 桌面应用程序，用于管理软件许可证的生成、验证和查看。支持硬件码绑定、有效期控制和许可证详情展示。

## 功能

- 许可证信息查看与验证
- 许可证详情弹窗（`DetailWindow`）
- 基于硬件码的许可证绑定

## 窗口

| 窗口 | 说明 |
|------|------|
| `MainWindow` | 主界面，显示许可证列表和状态 |
| `DetailWindow` | 许可证详情查看窗口 |

## 运行方式

```bash
dotnet run --project LicenseManager.WPF/LicenseManager.WPF.csproj
```

## 数据存储

- **数据库文件**：`%LocalAppData%\LicenseManager\license.db`（标准 SQLite）
- **自动备份**：应用启动时自动复制数据库到同级 `backups\license_yyyyMMdd_HHmmss.db`，保留最近 10 个备份
- **WAL 模式**：已启用，支持读写并发

## 依赖

- CoreToolkit
- CoreToolkit.Desktop
