# MES 制造执行系统模块

提供与 MES（Manufacturing Execution System）的对接能力，支持工单追踪（Track-In/Track-Out）、设备状态上报、工艺数据上传、报警上报、物料校验等标准功能。

## 目录结构

```
MES/
├── Core/
│   └── IMesClient.cs            # MES 客户端接口
├── Helpers/
│   ├── MesHelper.cs             # MES 业务辅助（状态转换、数据组装）
│   └── MesHttpClient.cs         # HTTP 客户端实现
└── Models/
    ├── AlarmReport.cs           # 报警上报模型
    ├── EquipmentStatusReport.cs # 设备状态上报模型
    ├── MaterialVerifyRequest.cs # 物料校验请求
    ├── MesResponse.cs           # MES 通用响应
    ├── ProcessDataReport.cs     # 工艺数据上报模型
    ├── ProcessParameter.cs      # 工艺参数
    ├── ProductInfo.cs           # 产品信息
    ├── TrackInRequest.cs        # 进站请求
    ├── TrackOutRequest.cs       # 出站请求
    └── WorkOrderInfo.cs         # 工单信息
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `IMesClient` | MES 客户端抽象：`TrackIn/TrackOut/UploadProcessData/ReportEquipmentStatus/ReportAlarm/VerifyMaterial` |
| `MesHttpClient` | 基于 `HttpClient` 的 MES HTTP 实现，支持认证头、超时配置、JSON 序列化 |
| `MesHelper` | 业务辅助：设备状态枚举定义、MES 消息组装、响应结果解析 |
| `TrackInRequest` / `TrackOutRequest` | 工单进站/出站请求体：设备ID、工单号、产品码、操作人员 |
| `EquipmentStatusReport` | 设备状态上报：Running/Idle/Down/Maintenance/Alarm |
| `AlarmReport` | 报警上报：报警代码、报警级别、发生时间、清除时间 |

## 使用示例

```csharp
// 创建 MES 客户端
var mes = new MesHttpClient("http://mes.factory.local", timeoutSeconds: 30);
mes.SetAuthorization("Bearer YOUR_TOKEN");

// 工单进站
var trackIn = new TrackInRequest
{
    EquipmentId = "SMT-LINE-01",
    WorkOrderNo = "WO20240421001",
    ProductCode = "PCB-A001",
    OperatorId = "OP001"
};
var response = mes.TrackIn(trackIn);

// 上报设备状态
mes.ReportEquipmentStatus(new EquipmentStatusReport
{
    EquipmentId = "SMT-LINE-01",
    EquipmentStatus = EquipmentStatus.Running,
    StatusDescription = "正常运行"
});
```

## 依赖

- System.Net.Http
- Newtonsoft.Json
