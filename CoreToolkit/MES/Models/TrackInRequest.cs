using System;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 过站进入请求
    /// </summary>
    public class TrackInRequest
    {
        public string WorkOrderNumber { get; set; }
        public string SerialNumber { get; set; }
        public string ProductCode { get; set; }
        public string EquipmentId { get; set; }
        public string ProcessStep { get; set; }
        public string OperatorId { get; set; }
        public DateTime TrackInTime { get; set; } = DateTime.Now;
    }
}
