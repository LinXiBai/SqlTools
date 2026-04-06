namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴速度参数配置
    /// 包含完整的速度曲线参数
    /// </summary>
    public class AxisSpeedProfile
    {
        /// <summary>
        /// 轴号
        /// </summary>
        public int AxisId { get; set; }

        /// <summary>
        /// 初始速度（起始速度）- 对应数据库中的"运动低速"
        /// </summary>
        public double StartVelocity { get; set; }

        /// <summary>
        /// 运行速度（最大速度）- 对应数据库中的"运动高速"
        /// </summary>
        public double MaxVelocity { get; set; }

        /// <summary>
        /// 加速度
        /// </summary>
        public double Acceleration { get; set; }

        /// <summary>
        /// 减速度
        /// </summary>
        public double Deceleration { get; set; }

        /// <summary>
        /// 加加速度（S曲线加加速度）
        /// </summary>
        public double JerkAcceleration { get; set; }

        /// <summary>
        /// 减减速度（S曲线减减速度）
        /// </summary>
        public double JerkDeceleration { get; set; }

        /// <summary>
        /// 当前速度模式
        /// </summary>
        public SpeedMode CurrentMode { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AxisSpeedProfile()
        {
            AxisId = 0;
            StartVelocity = 0;
            MaxVelocity = 10000;
            Acceleration = 50000;
            Deceleration = 50000;
            JerkAcceleration = 0;
            JerkDeceleration = 0;
            CurrentMode = SpeedMode.High;
        }

        /// <summary>
        /// 从数据库参数创建速度配置（高速模式）
        /// </summary>
        /// <param name="axisId">轴号</param>
        /// <param name="lowSpeed">运动低速（初始速度）</param>
        /// <param name="highSpeed">运动高速（运行速度）</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="jerkAcc">加加速度</param>
        /// <param name="jerkDec">减减速度</param>
        /// <returns>速度配置对象</returns>
        public static AxisSpeedProfile FromDatabase(
            int axisId,
            double lowSpeed,
            double highSpeed,
            double acc,
            double dec,
            double jerkAcc = 0,
            double jerkDec = 0)
        {
            return new AxisSpeedProfile
            {
                AxisId = axisId,
                StartVelocity = lowSpeed,
                MaxVelocity = highSpeed,
                Acceleration = acc,
                Deceleration = dec,
                JerkAcceleration = jerkAcc,
                JerkDeceleration = jerkDec,
                CurrentMode = SpeedMode.High
            };
        }

        /// <summary>
        /// 创建副本
        /// </summary>
        public AxisSpeedProfile Clone()
        {
            return new AxisSpeedProfile
            {
                AxisId = this.AxisId,
                StartVelocity = this.StartVelocity,
                MaxVelocity = this.MaxVelocity,
                Acceleration = this.Acceleration,
                Deceleration = this.Deceleration,
                JerkAcceleration = this.JerkAcceleration,
                JerkDeceleration = this.JerkDeceleration,
                CurrentMode = this.CurrentMode
            };
        }

        /// <summary>
        /// 获取参数摘要信息
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "轴{0}[{1}]: 初始={2:F2}, 运行={3:F2}, 加速={4:F2}, 减速={5:F2}",
                AxisId,
                CurrentMode.GetDisplayName(),
                StartVelocity,
                MaxVelocity,
                Acceleration,
                Deceleration);
        }
    }
}
