using System;

namespace CoreToolkit.Equipment.Models
{
    /// <summary>
    /// 贴装/固晶结果记录
    /// </summary>
    public class PlacementResult
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int StationId { get; set; }
        public int NozzleId { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double ActualX { get; set; }
        public double ActualY { get; set; }
        public double DeltaX => ActualX - TargetX;
        public double DeltaY => ActualY - TargetY;
        public double RotationAngle { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
