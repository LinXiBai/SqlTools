namespace CoreToolkit.Vision.Models
{
    /// <summary>
    /// 模板匹配结果
    /// </summary>
    public class MatchResult
    {
        public bool IsFound { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double Score { get; set; }

        public override string ToString()
        {
            return $"MatchResult: X={X:F3}, Y={Y:F3}, Angle={Angle:F3}, Score={Score:F3}";
        }
    }
}
