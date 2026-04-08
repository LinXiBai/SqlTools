using System;
using CoreToolkit.Motion.Core;

namespace CoreToolkit.Motion.Providers.Advantech
{
    /// <summary>
    /// 研华 PCI-1203 EtherCAT 运动控制卡实现
    /// PCI-1203 支持 16轴 或 32轴 EtherCAT 主站控制
    /// </summary>
    public class PCI1203 : AdvantechMotionCardBase
    {
        #region 属性实现

        /// <summary>
        /// 控制卡名称
        /// </summary>
        public override string CardName { get { return "PCI-1203"; } }

        /// <summary>
        /// 型号
        /// </summary>
        public override string Model { get { return "PCI-1203"; } }

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数（16轴）
        /// </summary>
        public PCI1203() : this(16) { }

        /// <summary>
        /// 指定轴数量
        /// </summary>
        /// <param name="axisCount">轴数量（16或32）</param>
        public PCI1203(int axisCount) : base(axisCount)
        {
            if (axisCount != 16 && axisCount != 32)
            {
                throw new ArgumentException("PCI-1203 仅支持 16轴 或 32轴 配置");
            }
        }

        #endregion
    }
}
