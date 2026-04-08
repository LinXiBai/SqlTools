using System;
using System.Collections.Generic;
using System.Linq;
using CoreToolkit.Motion.Core;

namespace CoreToolkit.Motion.Providers.Advantech
{
    /// <summary>
    /// 研华 PCI-1285 运动控制卡实现
    /// PCI-1285 支持 8轴 脉冲输出运动控制
    /// 支持轴组、插补、PTP、PVT功能
    /// </summary>
    public class PCI1285 : AdvantechMotionCardBase
    {
        #region 字段

        // 轴组管理器
        private AxisGroupManager _groupManager;

        #endregion

        #region 属性实现

        /// <summary>
        /// 控制卡名称
        /// </summary>
        public override string CardName { get { return "PCI-1285"; } }

        /// <summary>
        /// 型号
        /// </summary>
        public override string Model { get { return "PCI-1285"; } }

        /// <summary>
        /// 轴组管理器
        /// </summary>
        public AxisGroupManager GroupManager
        {
            get
            {
                if (_groupManager == null && IsOpen)
                {
                    _groupManager = new AxisGroupManager(this);
                }
                return _groupManager;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数（8轴）
        /// </summary>
        public PCI1285() : base(8) { }

        #endregion

        #region 重写钩子方法

        /// <summary>
        /// 打开后初始化轴组管理器
        /// </summary>
        protected override void OnAfterOpen()
        {
            // 初始化轴组管理器
            _groupManager = new AxisGroupManager(this);
        }

        #endregion

        #region 轴组和高级功能

        /// <summary>
        /// 创建轴组
        /// </summary>
        public IAxisGroup CreateAxisGroup(string groupName, int[] axisIds)
        {
            return GroupManager?.CreateGroup(groupName, axisIds);
        }

        /// <summary>
        /// 创建XY轴组
        /// </summary>
        public IAxisGroup CreateXYGroup(int xAxis, int yAxis, string name = "XY")
        {
            return GroupManager?.CreateXYGroup(xAxis, yAxis, name);
        }

        /// <summary>
        /// 创建XYZ轴组
        /// </summary>
        public IAxisGroup CreateXYZGroup(int xAxis, int yAxis, int zAxis, string name = "XYZ")
        {
            return GroupManager?.CreateXYZGroup(xAxis, yAxis, zAxis, name);
        }

        /// <summary>
        /// 执行PTP运动
        /// </summary>
        public void MovePTP(int[] axes, double[] positions, double speedPercent = 100)
        {
            CheckConnection();

            var group = CreateAxisGroup($"PTP_{DateTime.Now:HHmmssfff}", axes);
            try
            {
                group.Enable();
                group.MovePTP(positions, speedPercent);
                group.WaitForComplete(30000);
            }
            finally
            {
                group.Disable();
                GroupManager.DeleteGroup(group.GroupId);
            }
        }

        /// <summary>
        /// 执行PVT运动
        /// </summary>
        public void MovePVT(int axis, PVTPoint[] pvtData)
        {
            CheckConnection();
            CheckAxis(axis);

            var group = CreateAxisGroup($"PVT_{DateTime.Now:HHmmssfff}", new[] { axis });
            try
            {
                group.Enable();
                group.MovePVT(pvtData, 0);
                group.WaitForComplete((int)(pvtData.Sum(p => p.TimeMs) + 5000));
            }
            finally
            {
                group.Disable();
                GroupManager.DeleteGroup(group.GroupId);
            }
        }

        /// <summary>
        /// 执行多轴同步PVT运动
        /// </summary>
        public void MovePVTSync(int[] axes, PVTPoint[][] pvtDataArray)
        {
            CheckConnection();

            var group = CreateAxisGroup($"PVTSync_{DateTime.Now:HHmmssfff}", axes);
            try
            {
                group.Enable();
                group.MovePVTSync(pvtDataArray);
                group.WaitForComplete((int)(pvtDataArray[0].Sum(p => p.TimeMs) + 5000));
            }
            finally
            {
                group.Disable();
                GroupManager.DeleteGroup(group.GroupId);
            }
        }

        /// <summary>
        /// 圆弧插补（使用轴组）
        /// </summary>
        public void MoveCircular(int[] axes, double[] center, double[] endPos, int direction, double speed)
        {
            CheckConnection();
            if (axes.Length < 2)
                throw new MotionException("圆弧插补需要至少2个轴", -1);

            var group = CreateAxisGroup($"Circle_{DateTime.Now:HHmmssfff}", axes);
            try
            {
                group.Enable();
                group.MoveCircularAbs(center, endPos, direction, speed);
                group.WaitForComplete(30000);
            }
            finally
            {
                group.Disable();
                GroupManager.DeleteGroup(group.GroupId);
            }
        }

        #endregion
    }

    /// <summary>
    /// 插补缓冲区（辅助类）
    /// </summary>
    internal class InterpolationBuffer
    {
        public int BufferId { get; set; }
        public int[] AxisIds { get; set; }
        public List<double[]> PositionQueue { get; set; } = new List<double[]>();
        public int Capacity { get; set; } = 1000;
    }
}
