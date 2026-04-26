using System;
using System.Collections.Generic;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 过程数据/工艺参数上报
    /// </summary>
    public class ProcessDataReport
    {
        public string WorkOrderNumber { get; set; }
        public string SerialNumber { get; set; }
        public string EquipmentId { get; set; }
        public string ProcessStep { get; set; }
        public DateTime ReportTime { get; set; } = DateTime.Now;
        public List<ProcessParameter> Parameters { get; set; } = new List<ProcessParameter>();
    }
}
