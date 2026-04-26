namespace CoreToolkit.Equipment.Models
{
    /// <summary>
    /// 供料器信息
    /// </summary>
    public class FeederInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double Pitch { get; set; }
        public int TotalComponents { get; set; }
        public int UsedComponents { get; set; }
        public int RemainingComponents => TotalComponents - UsedComponents;
        public bool IsEnabled { get; set; } = true;
    }
}
