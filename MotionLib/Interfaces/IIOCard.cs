using System;

namespace MotionLib.Interfaces
{
    /// <summary>
    /// IO扩展卡接口
    /// 用于扩展数字输入输出功能
    /// </summary>
    public interface IIOCard : IDisposable
    {
        /// <summary>
        /// IO卡名称
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
        /// 卡号
        /// </summary>
        int CardId { get; }

        /// <summary>
        /// 输入点数量
        /// </summary>
        int InputCount { get; }

        /// <summary>
        /// 输出点数量
        /// </summary>
        int OutputCount { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 初始化IO卡
        /// </summary>
        void Initialize(int cardId);

        /// <summary>
        /// 打开IO卡
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭IO卡
        /// </summary>
        void Close();

        /// <summary>
        /// 读取单个输入点
        /// </summary>
        /// <param name="index">输入点索引</param>
        /// <returns>输入状态</returns>
        bool ReadInput(int index);

        /// <summary>
        /// 读取多个输入点
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="count">数量</param>
        /// <returns>输入状态数组</returns>
        bool[] ReadInputs(int startIndex, int count);

        /// <summary>
        /// 读取输入端口（8位）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口值</returns>
        byte ReadInputPort(int port);

        /// <summary>
        /// 读取单个输出点
        /// </summary>
        /// <param name="index">输出点索引</param>
        /// <returns>输出状态</returns>
        bool ReadOutput(int index);

        /// <summary>
        /// 读取多个输出点
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="count">数量</param>
        /// <returns>输出状态数组</returns>
        bool[] ReadOutputs(int startIndex, int count);

        /// <summary>
        /// 读取输出端口（8位）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口值</returns>
        byte ReadOutputPort(int port);

        /// <summary>
        /// 设置单个输出点
        /// </summary>
        /// <param name="index">输出点索引</param>
        /// <param name="value">输出值</param>
        void WriteOutput(int index, bool value);

        /// <summary>
        /// 设置多个输出点
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="values">输出值数组</param>
        void WriteOutputs(int startIndex, bool[] values);

        /// <summary>
        /// 设置输出端口（8位）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="value">端口值</param>
        void WriteOutputPort(int port, byte value);

        /// <summary>
        /// 翻转输出点状态
        /// </summary>
        /// <param name="index">输出点索引</param>
        void ToggleOutput(int index);

        /// <summary>
        /// 设置所有输出点
        /// </summary>
        /// <param name="value">输出值</param>
        void SetAllOutputs(bool value);

        /// <summary>
        /// 获取最后错误信息
        /// </summary>
        string GetLastError();
    }
}
