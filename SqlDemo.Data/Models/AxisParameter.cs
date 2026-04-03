namespace SqlDemo.Data
{
    /// <summary>
    /// 轴参数实体
    /// </summary>
    public class AxisParameter : EntityBase
    {
        private string _轴名称;
        private int _轴号;
        private int _卡号;
        private int _轴类型;
        private int _脉冲当量;
        private int _脉冲当量分母;
        private double _运动低速;
        private double _运动高速;
        private double _加速度;
        private double _减速度;
        private double _加加速度;
        private double _减减速度;
        private int _回原模式;
        private int _回原方向;
        private double _原点高速;
        private double _原点低速;
        private double _原点加速度;
        private double _原点减速度;
        private double _原点偏移;
        private double _正向软极限;
        private double _负向软极限;
        private int _使能IO;

        /// <summary>
        /// 轴名称
        /// </summary>
        [Field("轴名称", "基本信息", ControlType.String)]
        public string 轴名称
        {
            get { return _轴名称; }
            set { SetProperty(ref _轴名称, value); }
        }

        /// <summary>
        /// 轴号
        /// </summary>
        [Field("轴号", "基本信息", ControlType.Numeric)]
        public int 轴号
        {
            get { return _轴号; }
            set { SetProperty(ref _轴号, value); }
        }

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
        /// 轴类型
        /// </summary>
        [Field("轴类型", "基本信息", ControlType.Numeric)]
        public int 轴类型
        {
            get { return _轴类型; }
            set { SetProperty(ref _轴类型, value); }
        }

        /// <summary>
        /// 脉冲当量
        /// </summary>
        [Field("脉冲当量", "运动参数", ControlType.Numeric)]
        public int 脉冲当量
        {
            get { return _脉冲当量; }
            set { SetProperty(ref _脉冲当量, value); }
        }

        /// <summary>
        /// 脉冲当量分母
        /// </summary>
        [Field("脉冲当量分母", "运动参数", ControlType.Numeric)]
        public int 脉冲当量分母
        {
            get { return _脉冲当量分母; }
            set { SetProperty(ref _脉冲当量分母, value); }
        }

        /// <summary>
        /// 运动低速
        /// </summary>
        [Field("运动低速", "运动参数", ControlType.Numeric)]
        public double 运动低速
        {
            get { return _运动低速; }
            set { SetProperty(ref _运动低速, value); }
        }

        /// <summary>
        /// 运动高速
        /// </summary>
        [Field("运动高速", "运动参数", ControlType.Numeric)]
        public double 运动高速
        {
            get { return _运动高速; }
            set { SetProperty(ref _运动高速, value); }
        }

        /// <summary>
        /// 加速度
        /// </summary>
        [Field("加速度", "运动参数", ControlType.Numeric)]
        public double 加速度
        {
            get { return _加速度; }
            set { SetProperty(ref _加速度, value); }
        }

        /// <summary>
        /// 减速度
        /// </summary>
        [Field("减速度", "运动参数", ControlType.Numeric)]
        public double 减速度
        {
            get { return _减速度; }
            set { SetProperty(ref _减速度, value); }
        }

        /// <summary>
        /// 加加速度
        /// </summary>
        [Field("加加速度", "运动参数", ControlType.Numeric)]
        public double 加加速度
        {
            get { return _加加速度; }
            set { SetProperty(ref _加加速度, value); }
        }

        /// <summary>
        /// 减减速度
        /// </summary>
        [Field("减减速度", "运动参数", ControlType.Numeric)]
        public double 减减速度
        {
            get { return _减减速度; }
            set { SetProperty(ref _减减速度, value); }
        }

        /// <summary>
        /// 回原模式
        /// </summary>
        [Field("回原模式", "回零参数", ControlType.Numeric)]
        public int 回原模式
        {
            get { return _回原模式; }
            set { SetProperty(ref _回原模式, value); }
        }

        /// <summary>
        /// 回原方向
        /// </summary>
        [Field("回原方向", "回零参数", ControlType.Numeric)]
        public int 回原方向
        {
            get { return _回原方向; }
            set { SetProperty(ref _回原方向, value); }
        }

        /// <summary>
        /// 原点高速
        /// </summary>
        [Field("原点高速", "回零参数", ControlType.Numeric)]
        public double 原点高速
        {
            get { return _原点高速; }
            set { SetProperty(ref _原点高速, value); }
        }

        /// <summary>
        /// 原点低速
        /// </summary>
        [Field("原点低速", "回零参数", ControlType.Numeric)]
        public double 原点低速
        {
            get { return _原点低速; }
            set { SetProperty(ref _原点低速, value); }
        }

        /// <summary>
        /// 原点加速度
        /// </summary>
        [Field("原点加速度", "回零参数", ControlType.Numeric)]
        public double 原点加速度
        {
            get { return _原点加速度; }
            set { SetProperty(ref _原点加速度, value); }
        }

        /// <summary>
        /// 原点减速度
        /// </summary>
        [Field("原点减速度", "回零参数", ControlType.Numeric)]
        public double 原点减速度
        {
            get { return _原点减速度; }
            set { SetProperty(ref _原点减速度, value); }
        }

        /// <summary>
        /// 原点偏移
        /// </summary>
        [Field("原点偏移", "回零参数", ControlType.Numeric)]
        public double 原点偏移
        {
            get { return _原点偏移; }
            set { SetProperty(ref _原点偏移, value); }
        }

        /// <summary>
        /// 正向软极限
        /// </summary>
        [Field("正向软极限", "安全参数", ControlType.Numeric)]
        public double 正向软极限
        {
            get { return _正向软极限; }
            set { SetProperty(ref _正向软极限, value); }
        }

        /// <summary>
        /// 负向软极限
        /// </summary>
        [Field("负向软极限", "安全参数", ControlType.Numeric)]
        public double 负向软极限
        {
            get { return _负向软极限; }
            set { SetProperty(ref _负向软极限, value); }
        }

        /// <summary>
        /// 使能IO
        /// </summary>
        [Field("使能IO", "IO参数", ControlType.Numeric)]
        public int 使能IO
        {
            get { return _使能IO; }
            set { SetProperty(ref _使能IO, value); }
        }
    }
}
