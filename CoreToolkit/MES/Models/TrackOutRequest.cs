using System;
using System.Collections.Generic;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 过站离开请求
    /// </summary>
    public class TrackOutRequest
    {
        public string WorkOrderNumber { get; set; }
        public string SerialNumber { get; set; }
        public string ProductCode { get; set; }
        public string EquipmentId { get; set; }
        public string ProcessStep { get; set; }
        public string OperatorId { get; set; }
        public DateTime TrackOutTime { get; set; } = DateTime.Now;
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public string Result { get; set; } = "PASS";
        public string FailureCode { get; set; }
        public string FailureDescription { get; set; }
        public List<ProcessParameter> Parameters { get; set; } = new List<ProcessParameter>();
    }
}
