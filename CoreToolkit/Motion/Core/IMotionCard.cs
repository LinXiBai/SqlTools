using System;
using System.Collections.Generic;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 运动控制卡通用接口
    /// 定义所有控制卡必须实现的基础功能
    /// </summary>
    public interface IMotionCard : IDisposable
    {
        /// <summary>
        /// 控制卡名称
        /// </summary>
        string CardName { get; }

        /// <summary>
        /// 厂商名称
        /// </summary>
        string Vendor { get; }

        /// <summary>
        /// 型号
        /// </summary>
        string Model { get; }

        /// <summary>
        /// 卡号/设备索引
        /// </summary>
        int CardId { get; }

        /// <summary>
        /// 轴数量
        /// </summary>
        int AxisCount { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 初始化控制卡
        /// </summary>
        /// <param name="config">配置参数</param>
        void Initialize(MotionConfig config);

        /// <summary>
        /// 打开/连接控制卡
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭控制卡
        /// </summary>
        void Close();

        /// <summary>
        /// 复位控制卡
        /// </summary>
        void Reset();

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="axis">轴号（从0开始）</param>
        /// <returns>当前位置（脉冲或mm）</returns>
        double GetPosition(int axis);

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="position">位置值</param>
        void SetPosition(int axis, double position);

        /// <summary>
        /// 绝对位置运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="position">目标位置</param>
        /// <param name="speed">速度</param>
        void MoveAbsolute(int axis, double position, double speed);

        /// <summary>
        /// 相对位置运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="distance">移动距离</param>
        /// <param name="speed">速度</param>
        void MoveRelative(int axis, double distance, double speed);

        /// <summary>
        /// 连续运动（JOG）
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="direction">方向：1正方向，-1负方向</param>
        /// <param name="speed">速度</param>
        void Jog(int axis, int direction, double speed);

        /// <summary>
        /// 停止轴运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="emergency">是否急停</param>
        void Stop(int axis, bool emergency = false);

        /// <summary>
        /// 停止所有轴
        /// </summary>
        /// <param name="emergency">是否急停</param>
        void StopAll(bool emergency = false);

        /// <summary>
        /// 回零操作
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="speed">回零速度</param>
        void Home(int axis, double speed);

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>轴状态</returns>
        AxisStatus GetAxisStatus(int axis);

        /// <summary>
        /// 检查轴是否运动到位
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>是否到位</returns>
        bool IsInPosition(int axis);

        /// <summary>
        /// 伺服使能
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="enable">使能/禁用</param>
        void SetServoEnable(int axis, bool enable);

        /// <summary>
        /// 获取伺服使能状态
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>是否使能</returns>
        bool GetServoEnable(int axis);

        /// <summary>
        /// 设置轴速度参数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="sCurve">S曲线时间（0为T型曲线）</param>
        void SetVelocityProfile(int axis, double acc, double dec, double sCurve = 0);

        /// <summary>
        /// 读取通用输入
        /// </summary>
        /// <param name="index">输入点索引</param>
        /// <returns>输入状态</returns>
        bool ReadInput(int index);

        /// <summary>
        /// 读取通用输出
        /// </summary>
        /// <param name="index">输出点索引</param>
        /// <returns>输出状态</returns>
        bool ReadOutput(int index);

        /// <summary>
        /// 设置通用输出
        /// </summary>
        /// <param name="index">输出点索引</param>
        /// <param name="value">输出值</param>
        void WriteOutput(int index, bool value);

        /// <summary>
        /// 等待轴运动完成
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>是否成功</returns>
        bool WaitForMotionComplete(int axis, int timeoutMs = 10000);

        /// <summary>
        /// 线性插补运动
        /// </summary>
        /// <param name="axes">轴数组</param>
        /// <param name="positions">目标位置数组</param>
        /// <param name="speed">合成速度</param>
        void LinearInterpolation(int[] axes, double[] positions, double speed);

        /// <summary>
        /// 获取最后错误信息
        /// </summary>
        /// <returns>错误信息</returns>
        string GetLastError();
    }
}
