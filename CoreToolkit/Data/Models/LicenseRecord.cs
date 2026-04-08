using System;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 授权码生成记录实体
    /// </summary>
    public class LicenseRecord : EntityBase
    {
        private DateTime _recordTime;
        private string _department;
        private string _operator;
        private string _projectNumber;
        private string _deviceNumber;
        private string _machineCode;

        /// <summary>
        /// 记录时间
        /// </summary>
        [Field("记录时间", "授权信息", ControlType.None)]
        public DateTime RecordTime
        {
            get { return _recordTime; }
            set { SetProperty(ref _recordTime, value); }
        }

        /// <summary>
        /// 部门
        /// </summary>
        [Field("部门", "授权信息", ControlType.String)]
        public string Department
        {
            get { return _department; }
            set { SetProperty(ref _department, value); }
        }

        /// <summary>
        /// 记录人
        /// </summary>
        [Field("记录人", "授权信息", ControlType.String)]
        public string Operator
        {
            get { return _operator; }
            set { SetProperty(ref _operator, value); }
        }

        /// <summary>
        /// 项目号
        /// </summary>
        [Field("项目号", "设备信息", ControlType.String)]
        public string ProjectNumber
        {
            get { return _projectNumber; }
            set { SetProperty(ref _projectNumber, value); }
        }

        /// <summary>
        /// 设备号
        /// </summary>
        [Field("设备号", "设备信息", ControlType.String)]
        public string DeviceNumber
        {
            get { return _deviceNumber; }
            set { SetProperty(ref _deviceNumber, value); }
        }

        /// <summary>
        /// 机器码（序列化存储）
        /// </summary>
        [Field("机器码", "授权信息", ControlType.String)]
        public string MachineCode
        {
            get { return _machineCode; }
            set { SetProperty(ref _machineCode, value); }
        }
    }
}
