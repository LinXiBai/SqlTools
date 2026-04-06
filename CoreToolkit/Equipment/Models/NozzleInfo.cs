namespace CoreToolkit.Equipment.Models
{
    /// <summary>
    /// 吸嘴信息
    /// </summary>
    public class NozzleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double OuterDiameter { get; set; }
        public double InnerDiameter { get; set; }
        public int VacuumLevel { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
