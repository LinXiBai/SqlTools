using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴组接口
    /// 支持多轴同步运动和插补
    /// </summary>
    public interface IAxisGroup
    {
        /// <summary>
        /// 组ID
        /// </summary>
        int GroupId { get; }

        /// <summary>
        /// 组名称
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// 组内轴数量
        /// </summary>
        int AxisCount { get; }

        /// <summary>
        /// 组内轴号列表
        /// </summary>
        int[] AxisIds { get; }

        /// <summary>
        /// 是否已启用
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 是否正在运动
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// 启用轴组
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用轴组
        /// </summary>
        void Disable();

        /// <summary>
        /// 线性插补运动（绝对位置）
        /// </summary>
        /// <param name="positions">各轴目标位置数组</param>
        /// <param name="speed">合成速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        void MoveLinearAbs(double[] positions, double speed, double acc, double dec);

        /// <summary>
        /// 线性插补运动（相对位置）
        /// </summary>
        /// <param name="distances">各轴距离数组</param>
        /// <param name="speed">合成速度</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        void MoveLinearRel(double[] distances, double speed, double acc, double dec);

        /// <summary>
        /// 圆弧插补（XY平面，绝对位置）
        /// </summary>
        /// <param name="center">圆心坐标 [x, y]</param>
        /// <param name="endPos">终点坐标 [x, y]</param>
        /// <param name="direction">方向：1顺时针，-1逆时针</param>
        /// <param name="speed">速度</param>
        void MoveCircularAbs(double[] center, double[] endPos, int direction, double speed);

        /// <summary>
        /// 圆弧插补（XY平面，相对位置）
        /// </summary>
        /// <param name="centerOffset">圆心偏移 [dx, dy]</param>
        /// <param name="endDist">终点距离 [dx, dy]</param>
        /// <param name="direction">方向：1顺时针，-1逆时针</param>
        /// <param name="speed">速度</param>
        void MoveCircularRel(double[] centerOffset, double[] endDist, int direction, double speed);

        /// <summary>
        /// PTP点到点运动（各轴独立运动，非插补）
        /// </summary>
        /// <param name="positions">各轴目标位置</param>
        /// <param name="speedPercent">速度百分比(0-100)</param>
        void MovePTP(double[] positions, double speedPercent = 100);

        /// <summary>
        /// PVT运动 - 位置速度时间模式
        /// </summary>
        /// <param name="pvtData">PVT数据点数组</param>
        /// <param name="axisIndex">轴组内的轴索引</param>
        void MovePVT(PVTPoint[] pvtData, int axisIndex = 0);

        /// <summary>
        /// 多轴同步PVT运动
        /// </summary>
        /// <param name="pvtDataArray">各轴PVT数据，数组长度等于轴数</param>
        void MovePVTSync(PVTPoint[][] pvtDataArray);

        /// <summary>
        /// 停止轴组运动
        /// </summary>
        /// <param name="emergency">是否急停</param>
        void Stop(bool emergency = false);

        /// <summary>
        /// 等待运动完成
        /// </summary>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>是否在超时前完成</returns>
        bool WaitForComplete(int timeoutMs = 30000);

        /// <summary>
        /// 获取组状态
        /// </summary>
        AxisGroupStatus GetStatus();
    }

    /// <summary>
    /// PVT数据点
    /// Position-Velocity-Time 模式
    /// </summary>
    public struct PVTPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public double Velocity { get; set; }

        /// <summary>
        /// 时间(毫秒)
        /// </summary>
        public double TimeMs { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PVTPoint(double position, double velocity, double timeMs)
        {
            Position = position;
            Velocity = velocity;
            TimeMs = timeMs;
        }

        public override string ToString()
        {
            return $"PVT(P:{Position:F2}, V:{Velocity:F2}, T:{TimeMs:F1}ms)";
        }
    }

    /// <summary>
    /// 轴组状态
    /// </summary>
    public struct AxisGroupStatus
    {
        /// <summary>
        /// 组ID
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否正在运动
        /// </summary>
        public bool IsMoving { get; set; }

        /// <summary>
        /// 当前运动模式
        /// </summary>
        public MotionMode CurrentMode { get; set; }

        /// <summary>
        /// 各轴当前位置
        /// </summary>
        public double[] Positions { get; set; }

        /// <summary>
        /// 各轴当前速度
        /// </summary>
        public double[] Velocities { get; set; }

        /// <summary>
        /// 剩余距离
        /// </summary>
        public double RemainingDistance { get; set; }

        /// <summary>
        /// 缓冲区剩余空间(用于PVT模式)
        /// </summary>
        public int BufferSpace { get; set; }
    }

    /// <summary>
    /// 运动模式
    /// </summary>
    public enum MotionMode
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle,

        /// <summary>
        /// 线性插补
        /// </summary>
        LinearInterpolation,

        /// <summary>
        /// 圆弧插补
        /// </summary>
        CircularInterpolation,

        /// <summary>
        /// PTP点到点
        /// </summary>
        PTP,

        /// <summary>
        /// PVT模式
        /// </summary>
        PVT,

        /// <summary>
        /// JOG模式
        /// </summary>
        Jog
    }
}
