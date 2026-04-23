# Files 文件管理模块

提供配方（Recipe）管理、CSV/INI 读写、高性能文件索引与搜索功能。针对工业场景中大数量级（数十万）文件索引做了专门优化。

## 目录结构

```
Files/
├── Helpers/
│   ├── CsvHelper.cs                 # CSV 读写封装
│   ├── FileIndexManager.cs          # 文件索引管理器（V1）
│   ├── FileIndexManagerV2.cs        # 文件索引管理器（V2，性能优化版）
│   ├── FileIndexSearcher.cs         # 文件索引搜索器
│   ├── FileIndexSearcherOptimized.cs# 优化版搜索器
│   ├── IniHelper.cs                 # INI 配置文件读写
│   └── RecipeManager.cs             # 配方管理器
└── Models/
    └── Recipe.cs                    # 配方数据模型
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `RecipeManager` | 配方的增删改查、版本管理、参数持久化（JSON/INI） |
| `FileIndexManager` / `V2` | 建立磁盘文件的内存索引，支持按产品型号、序列号前缀、时间范围快速过滤 |
| `FileIndexSearcher` | 基于索引的高速搜索，支持模糊匹配与复合条件查询 |
| `CsvHelper` | 基于 StreamReader/StreamWriter 的高性能 CSV 读写，支持中文编码 |
| `IniHelper` | 基于 Win32 API 的 INI 文件读写封装，支持节（Section）和键值对 |

## 使用示例

```csharp
// 配方管理
var recipeMgr = new RecipeManager(@"C:\Recipes");
recipeMgr.SaveRecipe(new Recipe
{
    RecipeName = "PCB-A001",
    ProductCode = "A001",
    Parameters = new Dictionary<string, object>
    {
        ["FeedSpeed"] = 1200,
        ["PlacementPressure"] = 0.5
    }
});

// 文件索引
using var index = new FileIndexManagerV2(@"C:\Data");
index.BuildIndex();
var results = index.Search(productModel: "A001", serialPrefix: "SN2024");

// INI 读写
var ini = new IniHelper(@"C:\Config\machine.ini");
ini.SetValue("Axis", "MaxSpeed", 50000);
ini.Save();
```

## 依赖

- System.IO
- Newtonsoft.Json
