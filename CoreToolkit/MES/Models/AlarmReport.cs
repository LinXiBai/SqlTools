using System;

namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 报警信息上报
    /// </summary>
    public class AlarmReport
    {
        public string EquipmentId { get; set; }
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public string AlarmLevel { get; set; }
        public DateTime OccurTime { get; set; } = DateTime.Now;
        public DateTime? ClearTime { get; set; }
        public bool IsCleared { get; set; }
    }
}
