namespace CoreToolkit.Data
{
    /// <summary>
    /// IO参数实体
    /// </summary>
    public class IOParameter : EntityBase
    {
        private int _卡号;
        private int _端口号;
        private int _输入点;
        private string _输入名称;
        private int _输出点;
        private string _输出名称;

        /// <summary>
        /// 卡号
        /// </summary>
        [Field("卡号", "基本信息", ControlType.Numeric)]
        public int 卡号
        {
            get { return _卡号; }
            set { SetProperty(ref _卡号, value); }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        [Field("端口号", "基本信息", ControlType.Numeric)]
        public int 端口号
        {
            get { return _端口号; }
            set { SetProperty(ref _端口号, value); }
        }

        /// <summary>
        /// 输入点
        /// </summary>
        [Field("输入点", "输入参数", ControlType.Numeric)]
        public int 输入点
        {
            get { return _输入点; }
            set { SetProperty(ref _输入点, value); }
        }

        /// <summary>
        /// 输入名称
        /// </summary>
        [Field("输入名称", "输入参数", ControlType.String)]
        public string 输入名称
        {
            get { return _输入名称; }
            set { SetProperty(ref _输入名称, value); }
        }

        /// <summary>
        /// 输出点
        /// </summary>
        [Field("输出点", "输出参数", ControlType.Numeric)]
        public int 输出点
        {
            get { return _输出点; }
            set { SetProperty(ref _输出点, value); }
        }

        /// <summary>
        /// 输出名称
        /// </summary>
        [Field("输出名称", "输出参数", ControlType.String)]
        public string 输出名称
        {
            get { return _输出名称; }
            set { SetProperty(ref _输出名称, value); }
        }
    }
}
