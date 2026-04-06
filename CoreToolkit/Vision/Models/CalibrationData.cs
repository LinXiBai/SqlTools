namespace CoreToolkit.Vision.Models
{
    /// <summary>
    /// 标定数据（像素坐标与机械坐标的映射关系）
    /// </summary>
    public class CalibrationData
    {
        /// <summary>像素坐标 X</summary>
        public double PixelX { get; set; }
        /// <summary>像素坐标 Y</summary>
        public double PixelY { get; set; }
        /// <summary>机械坐标 X</summary>
        public double MachineX { get; set; }
        /// <summary>机械坐标 Y</summary>
        public double MachineY { get; set; }
    }
}
