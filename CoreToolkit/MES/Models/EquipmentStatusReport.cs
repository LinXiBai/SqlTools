using System;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 设备状态上报
    /// </summary>
    public class EquipmentStatusReport
    {
        public string EquipmentId { get; set; }
        public string EquipmentStatus { get; set; }
        public string StatusDescription { get; set; }
        public DateTime ReportTime { get; set; } = DateTime.Now;
        public int TotalCount { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
    }
}
