namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 过程参数项
    /// </summary>
    public class ProcessParameter
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string Unit { get; set; }
        public string UpperLimit { get; set; }
        public string LowerLimit { get; set; }
    }
}
