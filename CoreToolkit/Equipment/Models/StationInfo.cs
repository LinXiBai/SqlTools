namespace CoreToolkit.Equipment.Models
{
    /// <summary>
    /// 料站/料槽信息（贴片机料站、固晶机晶圆环位置等）
    /// </summary>
    public class StationInfo
    {
        public int StationId { get; set; }
        public string Name { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public int FeederId { get; set; }
        public string ComponentCode { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
