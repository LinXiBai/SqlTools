using System.Threading.Tasks;
using CoreToolkit.MES.Models;

namespace CoreToolkit.MES.Core
{
    /// <summary>
    /// MES 客户端接口抽象
    /// </summary>
    public interface IMesClient
    {
        /// <summary>
        /// MES 服务端基础地址
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// 设置认证信息（如 Bearer Token 或 Basic Auth）
        /// </summary>
        void SetAuthorization(string authHeaderValue);

        /// <summary>
        /// 过站进入（Track In）
        /// </summary>
        MesResponse TrackIn(TrackInRequest request);

        /// <summary>
        /// 过站进入（异步）
        /// </summary>
        Task<MesResponse> TrackInAsync(TrackInRequest request);

        /// <summary>
        /// 过站离开（Track Out）
        /// </summary>
        MesResponse TrackOut(TrackOutRequest request);

        /// <summary>
        /// 过站离开（异步）
        /// </summary>
        Task<MesResponse> TrackOutAsync(TrackOutRequest request);

        /// <summary>
        /// 上传过程数据/工艺参数
        /// </summary>
        MesResponse UploadProcessData(ProcessDataReport report);

        /// <summary>
        /// 上传过程数据（异步）
        /// </summary>
        Task<MesResponse> UploadProcessDataAsync(ProcessDataReport report);

        /// <summary>
        /// 上报设备状态
        /// </summary>
        MesResponse ReportEquipmentStatus(EquipmentStatusReport report);

        /// <summary>
        /// 上报设备状态（异步）
        /// </summary>
        Task<MesResponse> ReportEquipmentStatusAsync(EquipmentStatusReport report);

        /// <summary>
        /// 上报报警信息
        /// </summary>
        MesResponse ReportAlarm(AlarmReport report);

        /// <summary>
        /// 上报报警信息（异步）
        /// </summary>
        Task<MesResponse> ReportAlarmAsync(AlarmReport report);

        /// <summary>
        /// 物料校验
        /// </summary>
        MesResponse VerifyMaterial(MaterialVerifyRequest request);

        /// <summary>
        /// 物料校验（异步）
        /// </summary>
        Task<MesResponse> VerifyMaterialAsync(MaterialVerifyRequest request);

        /// <summary>
        /// 查询工单信息
        /// </summary>
        MesResponse<WorkOrderInfo> QueryWorkOrder(string workOrderNumber);

        /// <summary>
        /// 查询工单信息（异步）
        /// </summary>
        Task<MesResponse<WorkOrderInfo>> QueryWorkOrderAsync(string workOrderNumber);

        /// <summary>
        /// 通用 Post 请求
        /// </summary>
        MesResponse Post(string actionUrl, object data);

        /// <summary>
        /// 通用 Post 请求（异步）
        /// </summary>
        Task<MesResponse> PostAsync(string actionUrl, object data);
    }
}
