namespace CoreToolkit.Vision.Models
{
    /// <summary>
    /// 模板匹配结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// 是否找到匹配
        /// </summary>
        public bool IsFound { get; set; }
        
        /// <summary>
        /// X坐标
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        
        /// <summary>
        /// 匹配得分
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"MatchResult: X={X:F3}, Y={Y:F3}, Angle={Angle:F3}, Score={Score:F3}";
        }
    }
}
