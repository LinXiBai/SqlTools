namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// 物料校验请求
    /// </summary>
    public class MaterialVerifyRequest
    {
        public string WorkOrderNumber { get; set; }
        public string EquipmentId { get; set; }
        public string StationId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialLot { get; set; }
        public string SupplierCode { get; set; }
        public string OperatorId { get; set; }
    }
}
