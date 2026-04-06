using System;
using System.Text;
using System.Threading;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Interfaces;
using static Advantech.Motion.Motion;
using Advantech.Motion;

namespace CoreToolkit.Motion.Providers.Advantech
{
    /// <summary>
    /// 研华 IO 扩展卡实现
    /// 支持 PCI-1750、PCI-1756 等 IO 扩展卡
    /// </summary>
    public class AdvantechIOCard : IIOCard
    {
        private readonly object _lockObj = new object();
        private bool _disposed = false;
        private string _lastError = "";
        private IntPtr _deviceHandle = IntPtr.Zero;

        // IO 配置
        private int _inputCount = 32;
        private int _outputCount = 32;
        private int _inputPortCount = 4;  // 32位 = 4个8位端口
        private int _outputPortCount = 4; // 32位 = 4个8位端口

        // 可用设备列表
        private DEV_LIST[] _availableDevices;

        #region 属性实现

        /// <summary>
        /// IO卡名称
        /// </summary>
        public string CardName { get; private set; }

        /// <summary>
        /// 厂商名称
        /// </summary>
        public string Vendor { get { return "Advantech"; } }

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; private set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public int CardId { get; private set; }

        /// <summary>
        /// 输入点数量
        /// </summary>
        public int InputCount { get { return _inputCount; } }

        /// <summary>
        /// 输出点数量
        /// </summary>
        public int OutputCount { get { return _outputCount; } }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpen { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数（PCI-1750，32入32出）
        /// </summary>
        public AdvantechIOCard() : this("PCI-1750", 32, 32) { }

        /// <summary>
        /// 指定型号和IO数量
        /// </summary>
        public AdvantechIOCard(string model, int inputCount, int outputCount)
        {
            Model = model;
            CardName = model;
            _inputCount = inputCount;
            _outputCount = outputCount;
            _inputPortCount = (inputCount + 7) / 8;  // 向上取整
            _outputPortCount = (outputCount + 7) / 8; // 向上取整
            IsInitialized = false;
            IsOpen = false;
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化IO卡
        /// </summary>
        public void Initialize(int cardId)
        {
            lock (_lockObj)
            {
                if (IsInitialized)
                {
                    throw new MotionException("IO卡已初始化", 0);
                }

                CardId = cardId;

                try
                {
                    // 列出可用设备
                    _availableDevices = new DEV_LIST[10];
                    uint outEntries = 0;
                    uint result = (uint)mAcm_GetAvailableDevs(_availableDevices, 10, ref outEntries);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("获取设备列表失败，错误码: {0}", result), (int)result);
                    }

                    if (outEntries == 0)
                    {
                        throw new MotionException("未检测到任何IO卡设备，请检查硬件连接和驱动安装", -1);
                    }

                    // 检查用户选择的卡号是否有效
                    if (CardId >= outEntries)
                    {
                        throw new MotionException(string.Format("选择的卡号 {0} 超出可用设备范围 (0-{1})", CardId, outEntries - 1), -1);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 发现 {2} 个设备", Vendor, Model, outEntries));
                    for (int i = 0; i < outEntries; i++)
                    {
                        Console.WriteLine(string.Format("  设备 {0}: {1} (编号: {2})", i, _availableDevices[i].DeviceName, _availableDevices[i].DeviceNum));
                    }

                    IsInitialized = true;
                    Console.WriteLine(string.Format("[{0} {1}] IO卡初始化成功，卡号: {2}, 设备编号: {3}", 
                        Vendor, Model, CardId, _availableDevices[CardId].DeviceNum));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("初始化失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -1);
                }
            }
        }

        /// <summary>
        /// 打开IO卡
        /// </summary>
        public void Open()
        {
            lock (_lockObj)
            {
                if (!IsInitialized)
                {
                    throw new MotionException("IO卡未初始化，请先调用 Initialize", 1);
                }

                if (IsOpen)
                {
                    Console.WriteLine("IO卡已打开");
                    return;
                }

                try
                {
                    // 检查设备列表是否有效
                    if (_availableDevices == null || _availableDevices.Length == 0)
                    {
                        throw new MotionException("设备列表为空，请重新初始化", -1);
                    }

                    // 获取设备编号
                    uint deviceNum = _availableDevices[CardId].DeviceNum;
                    Console.WriteLine(string.Format("[{0} {1}] 正在打开设备，卡号: {2}, 设备编号: {3}", 
                        Vendor, Model, CardId, deviceNum));

                    // 根据指定的板卡编号打开板卡，获取板卡句柄
                    uint result = mAcm_DevOpen(deviceNum, ref _deviceHandle);
                    if (result != 0)
                    {
                        // 获取错误信息
                        StringBuilder errorMsg = new StringBuilder(256);
                        mAcm_GetErrorMessage(result, errorMsg, 256);
                        throw new MotionException(string.Format("打开设备失败，错误码: {0}, 错误信息: {1}", 
                            result, errorMsg.ToString()), (int)result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] IO卡已打开，句柄: {2}", Vendor, Model, _deviceHandle));
                    IsOpen = true;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("打开设备失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -2);
                }
            }
        }

        /// <summary>
        /// 关闭IO卡
        /// </summary>
        public void Close()
        {
            lock (_lockObj)
            {
                if (!IsOpen) return;

                try
                {
                    // 调用研华 API 关闭设备
                    if (_deviceHandle != IntPtr.Zero)
                    {
                        mAcm_DevClose(ref _deviceHandle);
                        _deviceHandle = IntPtr.Zero;
                    }

                    IsOpen = false;
                    Console.WriteLine(string.Format("[{0} {1}] IO卡已关闭", Vendor, Model));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("关闭设备失败: {0}", ex.Message);
                    Console.WriteLine(_lastError);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Close();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 输入操作

        /// <summary>
        /// 读取单个输入点
        /// </summary>
        public bool ReadInput(int index)
        {
            CheckInputIndex(index);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    byte value = 0;
                    uint result = mAcm_DaqDiGetBit(_deviceHandle, (ushort)index, ref value);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("读取输入失败，错误码: {0}", result), (int)result);
                    }

                    return value != 0;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输入失败: {0}", ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// 读取多个输入点
        /// </summary>
        public bool[] ReadInputs(int startIndex, int count)
        {
            CheckInputIndex(startIndex);
            CheckInputIndex(startIndex + count - 1);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    bool[] values = new bool[count];
                    for (int i = 0; i < count; i++)
                    {
                        values[i] = ReadInput(startIndex + i);
                    }
                    return values;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取多个输入失败: {0}", ex.Message);
                    return new bool[count];
                }
            }
        }

        /// <summary>
        /// 读取输入端口（8位）
        /// </summary>
        public byte ReadInputPort(int port)
        {
            if (port < 0 || port >= _inputPortCount)
            {
                throw new MotionException(string.Format("输入端口号 {0} 超出范围 [0, {1}]", port, _inputPortCount - 1), -100);
            }
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    byte value = 0;
                    uint result = mAcm_DaqDiGetByte(_deviceHandle, (ushort)port, ref value);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("读取输入端口失败，错误码: {0}", result), (int)result);
                    }

                    return value;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输入端口失败: {0}", ex.Message);
                    return 0;
                }
            }
        }

        #endregion

        #region 输出操作

        /// <summary>
        /// 读取单个输出点
        /// </summary>
        public bool ReadOutput(int index)
        {
            CheckOutputIndex(index);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    byte value = 0;
                    uint result = mAcm_DaqDoGetBit(_deviceHandle, (ushort)index, ref value);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("读取输出失败，错误码: {0}", result), (int)result);
                    }

                    return value != 0;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输出失败: {0}", ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// 读取多个输出点
        /// </summary>
        public bool[] ReadOutputs(int startIndex, int count)
        {
            CheckOutputIndex(startIndex);
            CheckOutputIndex(startIndex + count - 1);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    bool[] values = new bool[count];
                    for (int i = 0; i < count; i++)
                    {
                        values[i] = ReadOutput(startIndex + i);
                    }
                    return values;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取多个输出失败: {0}", ex.Message);
                    return new bool[count];
                }
            }
        }

        /// <summary>
        /// 读取输出端口（8位）
        /// </summary>
        public byte ReadOutputPort(int port)
        {
            if (port < 0 || port >= _outputPortCount)
            {
                throw new MotionException(string.Format("输出端口号 {0} 超出范围 [0, {1}]", port, _outputPortCount - 1), -100);
            }
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    byte value = 0;
                    uint result = mAcm_DaqDoGetByte(_deviceHandle, (ushort)port, ref value);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("读取输出端口失败，错误码: {0}", result), (int)result);
                    }

                    return value;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输出端口失败: {0}", ex.Message);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 设置单个输出点
        /// </summary>
        public void WriteOutput(int index, bool value)
        {
            CheckOutputIndex(index);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    uint result = mAcm_DaqDoSetBit(_deviceHandle, (ushort)index, (byte)(value ? 1 : 0));
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置输出失败，错误码: {0}", result), (int)result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 输出点{2} 设置为: {3}", Vendor, Model, index, value));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置输出失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        /// <summary>
        /// 设置多个输出点
        /// </summary>
        public void WriteOutputs(int startIndex, bool[] values)
        {
            if (values == null) return;

            CheckOutputIndex(startIndex);
            CheckOutputIndex(startIndex + values.Length - 1);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        WriteOutput(startIndex + i, values[i]);
                    }
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置多个输出失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        /// <summary>
        /// 设置输出端口（8位）
        /// </summary>
        public void WriteOutputPort(int port, byte value)
        {
            if (port < 0 || port >= _outputPortCount)
            {
                throw new MotionException(string.Format("输出端口号 {0} 超出范围 [0, {1}]", port, _outputPortCount - 1), -100);
            }
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    uint result = mAcm_DaqDoSetByte(_deviceHandle, (ushort)port, value);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置输出端口失败，错误码: {0}", result), (int)result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 输出端口{2} 设置为: 0x{3:X2}", Vendor, Model, port, value));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置输出端口失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        /// <summary>
        /// 翻转输出点状态
        /// </summary>
        public void ToggleOutput(int index)
        {
            CheckOutputIndex(index);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    bool currentValue = ReadOutput(index);
                    WriteOutput(index, !currentValue);
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("翻转输出失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        /// <summary>
        /// 设置所有输出点
        /// </summary>
        public void SetAllOutputs(bool value)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    byte byteValue = value ? (byte)0xFF : (byte)0x00;
                    for (int port = 0; port < _outputPortCount; port++)
                    {
                        WriteOutputPort(port, byteValue);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 所有输出点设置为: {2}", Vendor, Model, value));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置所有输出失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取最后错误信息
        /// </summary>
        public string GetLastError()
        {
            return _lastError;
        }

        private void CheckInputIndex(int index)
        {
            if (index < 0 || index >= _inputCount)
            {
                throw new MotionException(string.Format("输入点索引 {0} 超出范围 [0, {1}]", index, _inputCount - 1), -100);
            }
        }

        private void CheckOutputIndex(int index)
        {
            if (index < 0 || index >= _outputCount)
            {
                throw new MotionException(string.Format("输出点索引 {0} 超出范围 [0, {1}]", index, _outputCount - 1), -100);
            }
        }

        private void CheckConnection()
        {
            if (!IsOpen)
            {
                throw new MotionException("IO卡未打开", -101);
            }
        }

        #endregion
    }
}
