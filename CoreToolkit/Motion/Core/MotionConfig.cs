using System.Collections.Generic;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴配置参数
    /// </summary>
    public class AxisConfig
    {
        /// <summary>
        /// 轴号
        /// </summary>
        public int AxisId { get; set; }

        /// <summary>
        /// 轴名称
        /// </summary>
        public string AxisName { get; set; }

        /// <summary>
        /// 脉冲当量（mm/脉冲）
        /// </summary>
        public double PulseEquivalent { get; set; }

        /// <summary>
        /// 最大速度
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// 最大加速度
        /// </summary>
        public double MaxAcceleration { get; set; }

        /// <summary>
        /// 默认速度
        /// </summary>
        public double DefaultSpeed { get; set; }

        /// <summary>
        /// 默认加速度
        /// </summary>
        public double DefaultAcceleration { get; set; }

        /// <summary>
        /// 默认减速度
        /// </summary>
        public double DefaultDeceleration { get; set; }

        /// <summary>
        /// 回零速度（高速）
        /// </summary>
        public double HomeSpeedHigh { get; set; }

        /// <summary>
        /// 回零速度（低速）
        /// </summary>
        public double HomeSpeedLow { get; set; }

        /// <summary>
        /// 回零方向：1正方向，-1负方向
        /// </summary>
        public int HomeDirection { get; set; }

        /// <summary>
        /// 软正限位位置
        /// </summary>
        public double SoftPositiveLimit { get; set; }

        /// <summary>
        /// 软负限位位置
        /// </summary>
        public double SoftNegativeLimit { get; set; }

        /// <summary>
        /// 是否启用软限位
        /// </summary>
        public bool EnableSoftLimit { get; set; }

        /// <summary>
        /// 到位精度（脉冲）
        /// </summary>
        public double InPositionRange { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AxisConfig()
        {
            AxisName = "";
            PulseEquivalent = 0.001;
            MaxSpeed = 100000;
            MaxAcceleration = 1000000;
            DefaultSpeed = 50000;
            DefaultAcceleration = 500000;
            DefaultDeceleration = 500000;
            HomeSpeedHigh = 10000;
            HomeSpeedLow = 1000;
            HomeDirection = -1;
            SoftPositiveLimit = 999999999;
            SoftNegativeLimit = -999999999;
            EnableSoftLimit = false;
            InPositionRange = 10;
        }
    }

    /// <summary>
    /// 控制卡配置参数
    /// </summary>
    public class MotionConfig
    {
        /// <summary>
        /// 设备索引/卡号
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 轴配置列表
        /// </summary>
        public List<AxisConfig> AxisConfigs { get; set; }

        /// <summary>
        /// 输入点数量
        /// </summary>
        public int InputCount { get; set; }

        /// <summary>
        /// 输出点数量
        /// </summary>
        public int OutputCount { get; set; }

        /// <summary>
        /// 厂商特定的配置参数（Key-Value形式）
        /// </summary>
        public Dictionary<string, object> VendorSpecific { get; set; }

        /// <summary>
        /// 配置文件路径（可选）
        /// </summary>
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionConfig()
        {
            CardId = 0;
            AxisConfigs = new List<AxisConfig>();
            InputCount = 16;
            OutputCount = 16;
            VendorSpecific = new Dictionary<string, object>();
            ConfigFilePath = "";
        }
    }
}
