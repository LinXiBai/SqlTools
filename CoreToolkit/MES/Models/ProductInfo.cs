namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 产品/板信息
    /// </summary>
    public class ProductInfo
    {
        public string SerialNumber { get; set; }
        public string ProductCode { get; set; }
        public string WorkOrderNumber { get; set; }
        public string ProcessStep { get; set; }
        public string CurrentStatus { get; set; }
    }
}
