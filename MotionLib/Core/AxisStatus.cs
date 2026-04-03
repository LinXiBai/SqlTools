namespace MotionLib.Core
{
    /// <summary>
    /// 轴状态结构体
    /// 包含轴的各种运行状态信息
    /// </summary>
    public struct AxisStatus
    {
        /// <summary>
        /// 轴号
        /// </summary>
        public int Axis { get; set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 是否到位
        /// </summary>
        public bool InPosition { get; set; }

        /// <summary>
        /// 伺服是否使能
        /// </summary>
        public bool ServoOn { get; set; }

        /// <summary>
        /// 是否报警
        /// </summary>
        public bool IsAlarm { get; set; }

        /// <summary>
        /// 正限位是否触发
        /// </summary>
        public bool PositiveLimit { get; set; }

        /// <summary>
        /// 负限位是否触发
        /// </summary>
        public bool NegativeLimit { get; set; }

        /// <summary>
        /// 原点信号是否触发
        /// </summary>
        public bool HomeSignal { get; set; }

        /// <summary>
        /// 是否回零完成
        /// </summary>
        public bool Homed { get; set; }

        /// <summary>
        /// 当前速度
        /// </summary>
        public double CurrentSpeed { get; set; }

        /// <summary>
        /// 当前位置（指令位置）
        /// </summary>
        public double CommandPosition { get; set; }

        /// <summary>
        /// 当前位置（实际位置）
        /// </summary>
        public double ActualPosition { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        public int AlarmCode { get; set; }

        /// <summary>
        /// 获取状态摘要信息
        /// </summary>
        public override string ToString()
        {
            return string.Format("轴{0}: 运行={1}, 到位={2}, 伺服={3}, 报警={4}, 位置={5:F3}, 速度={6:F3}",
                Axis, IsRunning, InPosition, ServoOn, IsAlarm, ActualPosition, CurrentSpeed);
        }
    }
}
