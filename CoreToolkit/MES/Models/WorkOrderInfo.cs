using System;
using System.Collections.Generic;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 工单信息
    /// </summary>
    public class WorkOrderInfo
    {
        public string WorkOrderNumber { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public int PlannedQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public string WorkCenter { get; set; }
        public string ProcessStep { get; set; }
        public DateTime? PlanStartTime { get; set; }
        public DateTime? PlanEndTime { get; set; }
        public List<string> AllowedMaterials { get; set; } = new List<string>();
    }
}
