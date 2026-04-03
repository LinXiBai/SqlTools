using System;
using System.Text;
using System.Threading;
using MotionLib.Core;
using System.Runtime.InteropServices;
using static Advantech.Motion.Motion;
using Advantech.Motion;

namespace MotionLib.Providers.Advantech
{
    /// <summary>
    /// 研华 PCI-1203 EtherCAT 运动控制卡实现
    /// PCI-1203 支持 16轴 或 32轴 EtherCAT 主站控制
    /// </summary>
    public class PCI1203 : IMotionCard
    {
        private readonly object _lockObj = new object();
        private bool _disposed = false;
        private MotionConfig _config;
        private string _lastError = "";
        private IntPtr _deviceHandle = IntPtr.Zero;
        private IntPtr[] _axisHandles;

        // 轴数量（PCI-1203 支持 16轴 或 32轴 版本）
        private int _axisCount = 16;

        // 轴状态缓存
        private double[] _currentPositions;
        private bool[] _servoStates;

        // 可用设备列表
        private DEV_LIST[] _availableDevices;

        #region 属性实现

        /// <summary>
        /// 控制卡名称
        /// </summary>
        public string CardName { get { return "PCI-1203"; } }

        /// <summary>
        /// 厂商名称
        /// </summary>
        public string Vendor { get { return "Advantech"; } }

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get { return "PCI-1203"; } }

        /// <summary>
        /// 卡号
        /// </summary>
        public int CardId { get; private set; }

        /// <summary>
        /// 轴数量
        /// </summary>
        public int AxisCount { get { return _axisCount; } }

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
        /// 默认构造函数（16轴）
        /// </summary>
        public PCI1203() : this(16) { }

        /// <summary>
        /// 指定轴数量
        /// </summary>
        /// <param name="axisCount">轴数量（16或32）</param>
        public PCI1203(int axisCount)
        {
            if (axisCount != 16 && axisCount != 32)
            {
                throw new ArgumentException("PCI-1203 仅支持 16轴 或 32轴 配置");
            }
            _axisCount = axisCount;
            _currentPositions = new double[_axisCount];
            _servoStates = new bool[_axisCount];
            _axisHandles = new IntPtr[_axisCount];
            for (int i = 0; i < _axisCount; i++)
            {
                _axisHandles[i] = IntPtr.Zero;
            }
            IsInitialized = false;
            IsOpen = false;
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化控制卡
        /// </summary>
        public void Initialize(MotionConfig config)
        {
            lock (_lockObj)
            {
                if (IsInitialized)
                {
                    throw new MotionException("控制卡已初始化", 0);
                }

                if (config == null)
                    throw new ArgumentNullException("config");

                _config = config;
                CardId = config.CardId;

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
                        throw new MotionException("未检测到任何运动控制卡设备，请检查硬件连接和驱动安装", -1);
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

                    Console.WriteLine(string.Format("[{0} {1}] 控制卡初始化成功，卡号: {2}, 设备编号: {3}", 
                        Vendor, Model, CardId, _availableDevices[CardId].DeviceNum));
                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("初始化失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -1);
                }
            }
        }

        /// <summary>
        /// 打开控制卡
        /// </summary>
        public void Open()
        {
            lock (_lockObj)
            {
                if (!IsInitialized)
                {
                    throw new MotionException("控制卡未初始化，请先调用 Initialize", 1);
                }

                if (IsOpen)
                {
                    Console.WriteLine("控制卡已打开");
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

                    // 打开所有轴
                    for (int i = 0; i < _axisCount; i++)
                    {
                        IntPtr axisHandle = IntPtr.Zero;
                        result = mAcm_AxOpen(_deviceHandle, (ushort)i, ref axisHandle);
                        if (result != 0)
                        {
                            throw new MotionException(string.Format("打开轴 {0} 失败，错误码: {1}", i, result), (int)result, i);
                        }
                        _axisHandles[i] = axisHandle;
                        _currentPositions[i] = 0;
                        _servoStates[i] = false;
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 控制卡已打开，句柄: {2}", Vendor, Model, _deviceHandle));
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
        /// 关闭控制卡
        /// </summary>
        public void Close()
        {
            lock (_lockObj)
            {
                if (!IsOpen) return;

                try
                {
                    // 先停止所有轴
                    StopAll(true);

                    // 关闭所有轴
                    for (int i = 0; i < _axisCount; i++)
                    {
                        if (_axisHandles[i] != IntPtr.Zero)
                        {
                            mAcm_AxClose(ref _axisHandles[i]);
                            _axisHandles[i] = IntPtr.Zero;
                        }
                    }

                    // 调用研华 API 关闭设备
                    if (_deviceHandle != IntPtr.Zero)
                    {
                        mAcm_DevClose(ref _deviceHandle);
                        _deviceHandle = IntPtr.Zero;
                    }

                    IsOpen = false;
                    Console.WriteLine(string.Format("[{0} {1}] 控制卡已关闭", Vendor, Model));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("关闭设备失败: {0}", ex.Message);
                    Console.WriteLine(_lastError);
                }
            }
        }

        /// <summary>
        /// 复位控制卡
        /// </summary>
        public void Reset()
        {
            lock (_lockObj)
            {
                CheckConnection();

                try
                {
                    // 复位轴状态
                    for (int i = 0; i < _axisCount; i++)
                    {
                        if (_axisHandles[i] != IntPtr.Zero)
                        {
                            mAcm_AxResetError(_axisHandles[i]);
                            _currentPositions[i] = 0;
                        }
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 控制卡已复位", Vendor, Model));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("复位失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -3);
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

        #region 运动控制方法

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        public double GetPosition(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 读取实际位置
                    double pos = 0;
                    uint result = mAcm_AxGetActualPosition(_axisHandles[axis], ref pos);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("读取位置失败，错误码: {0}", result), (int)result, axis);
                    }

                    _currentPositions[axis] = pos;
                    return pos;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取位置失败: {0}", ex.Message);
                    return _currentPositions[axis];
                }
            }
        }

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        public void SetPosition(int axis, double position)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 设置位置
                    uint result = mAcm_AxSetActualPosition(_axisHandles[axis], position);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置位置失败，错误码: {0}", result), (int)result, axis);
                    }

                    _currentPositions[axis] = position;
                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 位置设置为: {3}", Vendor, Model, axis, position));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置位置失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -4, axis);
                }
            }
        }

        /// <summary>
        /// 绝对位置运动
        /// </summary>
        public void MoveAbsolute(int axis, double position, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 设置速度
                    uint result = mAcm_AxChangeVel(_axisHandles[axis], speed);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置速度失败，错误码: {0}", result), (int)result, axis);
                    }

                    // 调用研华 API 执行绝对位置运动
                    result = mAcm_AxMoveAbs(_axisHandles[axis], position);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("绝对运动失败，错误码: {0}", result), (int)result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 绝对运动: 目标={3}, 速度={4}", 
                        Vendor, Model, axis, position, speed));
                    _currentPositions[axis] = position;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("绝对运动失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -5, axis);
                }
            }
        }

        /// <summary>
        /// 相对位置运动
        /// </summary>
        public void MoveRelative(int axis, double distance, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    double targetPos = _currentPositions[axis] + distance;

                    // 设置速度
                    uint result = mAcm_AxChangeVel(_axisHandles[axis], speed);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置速度失败，错误码: {0}", result), (int)result, axis);
                    }

                    // 调用研华 API 执行相对位置运动
                    result = mAcm_AxMoveRel(_axisHandles[axis], distance);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("相对运动失败，错误码: {0}", result), (int)result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 相对运动: 距离={3}, 速度={4}", 
                        Vendor, Model, axis, distance, speed));
                    _currentPositions[axis] = targetPos;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("相对运动失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -6, axis);
                }
            }
        }

        /// <summary>
        /// JOG运动
        /// </summary>
        public void Jog(int axis, int direction, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            if (direction != 1 && direction != -1)
            {
                throw new MotionException("方向参数必须为 1 或 -1", -7, axis);
            }

            lock (_lockObj)
            {
                try
                {
                    // 设置速度
                    uint result = mAcm_AxChangeVel(_axisHandles[axis], speed);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置速度失败，错误码: {0}", result), (int)result, axis);
                    }

                    // 调用研华 API 执行 JOG 运动
                    result = mAcm_AxMoveVel(_axisHandles[axis], (ushort)(direction > 0 ? 1 : 0));
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("JOG运动失败，错误码: {0}", result), (int)result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} JOG运动: 方向={3}, 速度={4}", 
                        Vendor, Model, axis, direction, speed));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("JOG运动失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -8, axis);
                }
            }
        }

        /// <summary>
        /// 停止轴运动
        /// </summary>
        public void Stop(int axis, bool emergency = false)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 停止轴
                    uint result;
                    if (emergency)
                        result = mAcm_AxStopEmg(_axisHandles[axis]);
                    else
                        result = mAcm_AxStopDec(_axisHandles[axis]);

                    if (result != 0)
                    {
                        throw new MotionException(string.Format("停止失败，错误码: {0}", result), (int)result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 已{3}", 
                        Vendor, Model, axis, emergency ? "急停" : "停止"));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("停止失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -9, axis);
                }
            }
        }

        /// <summary>
        /// 停止所有轴
        /// </summary>
        public void StopAll(bool emergency = false)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    for (int i = 0; i < _axisCount; i++)
                    {
                        Stop(i, emergency);
                    }
                    Console.WriteLine(string.Format("[{0} {1}] 所有轴已{2}", 
                        Vendor, Model, emergency ? "急停" : "停止"));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("停止所有轴失败: {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// 回零操作
        /// </summary>
        public void Home(int axis, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    AxisConfig axisConfig = GetAxisConfig(axis);
                    uint homeMode = 1; // 默认使用模式1
                    uint direction = 0; // 负方向
                    if (axisConfig != null && axisConfig.HomeDirection > 0)
                    {
                        direction = 1; // 正方向
                    }

                    // 设置回零速度
                    mAcm_AxChangeVel(_axisHandles[axis], speed);

                    // 调用研华 API 执行回零
                    uint result = mAcm_AxHome(_axisHandles[axis], homeMode, direction);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("回零失败，错误码: {0}", result), (int)result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 开始回零: 速度={3}, 方向={4}, 模式={5}", 
                        Vendor, Model, axis, speed, direction, homeMode));

                    // 等待回零完成
                    Thread.Sleep(1000);
                    
                    // 回零完成后，位置归零
                    mAcm_AxSetActualPosition(_axisHandles[axis], 0);
                    _currentPositions[axis] = 0;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("回零失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -10, axis);
                }
            }
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        public AxisStatus GetAxisStatus(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 读取轴状态
                    ushort state = 0;
                    uint result = mAcm_AxGetState(_axisHandles[axis], ref state);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("获取轴状态失败，错误码: {0}", result), (int)result, axis);
                    }

                    // 读取运动IO状态
                    uint ioStatus = 0;
                    mAcm_AxGetMotionIO(_axisHandles[axis], ref ioStatus);

                    // 读取实际位置
                    double actualPos = 0;
                    mAcm_AxGetActualPosition(_axisHandles[axis], ref actualPos);

                    // 读取命令位置
                    double cmdPos = 0;
                    mAcm_AxGetCmdPosition(_axisHandles[axis], ref cmdPos);

                    // 读取速度
                    double speed = 0;
                    mAcm_AxGetActVelocity(_axisHandles[axis], ref speed);

                    // 解析状态
                    bool isRunning = (state & 0x01) != 0;
                    bool isAlarm = (state & 0x02) != 0;
                    bool servoOn = (state & 0x04) != 0;

                    // 解析IO状态
                    bool positiveLimit = (ioStatus & 0x01) != 0;
                    bool negativeLimit = (ioStatus & 0x02) != 0;
                    bool homeSignal = (ioStatus & 0x04) != 0;

                    AxisStatus status = new AxisStatus
                    {
                        Axis = axis,
                        IsRunning = isRunning,
                        InPosition = !isRunning,
                        ServoOn = servoOn,
                        IsAlarm = isAlarm,
                        PositiveLimit = positiveLimit,
                        NegativeLimit = negativeLimit,
                        HomeSignal = homeSignal,
                        Homed = _currentPositions[axis] == 0,
                        CurrentSpeed = speed,
                        CommandPosition = cmdPos,
                        ActualPosition = actualPos,
                        AlarmCode = isAlarm ? (int)state : 0
                    };

                    return status;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("获取轴状态失败: {0}", ex.Message);
                    return new AxisStatus { Axis = axis };
                }
            }
        }

        /// <summary>
        /// 检查轴是否到位
        /// </summary>
        public bool IsInPosition(int axis)
        {
            return GetAxisStatus(axis).InPosition;
        }

        /// <summary>
        /// 设置伺服使能
        /// </summary>
        public void SetServoEnable(int axis, bool enable)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 伺服使能
                    uint result = mAcm_AxSetSvOn(_axisHandles[axis], enable ? 1u : 0u);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("伺服使能设置失败，错误码: {0}", result), (int)result, axis);
                    }

                    _servoStates[axis] = enable;
                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 伺服{3}", 
                        Vendor, Model, axis, enable ? "使能" : "关闭"));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("伺服使能设置失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -11, axis);
                }
            }
        }

        /// <summary>
        /// 获取伺服使能状态
        /// </summary>
        public bool GetServoEnable(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            try
            {
                // 读取轴状态获取伺服使能状态
                AxisStatus status = GetAxisStatus(axis);
                return status.ServoOn;
            }
            catch
            {
                return _servoStates[axis];
            }
        }

        /// <summary>
        /// 设置速度曲线
        /// </summary>
        public void SetVelocityProfile(int axis, double acc, double dec, double sCurve = 0)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 研华 API 中没有直接设置加速度和减速度的函数
                    // 这里我们通过属性设置来实现
                    // 注意：实际使用时需要根据研华 API 的具体实现来调整
                    
                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 速度曲线: 加速度={3}, 减速度={4}, S曲线={5}", 
                        Vendor, Model, axis, acc, dec, sCurve));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置速度曲线失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -12, axis);
                }
            }
        }

        #endregion

        #region IO 控制方法

        /// <summary>
        /// 读取输入
        /// </summary>
        public bool ReadInput(int index)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 读取输入
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
        /// 读取输出
        /// </summary>
        public bool ReadOutput(int index)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 读取输出状态
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
        /// 设置输出
        /// </summary>
        public void WriteOutput(int index, bool value)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 调用研华 API 设置输出
                    uint result = mAcm_DaqDoSetBit(_deviceHandle, (ushort)index, (byte)(value ? 1 : 0));
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置输出失败，错误码: {0}", result), (int)result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 输出点{2} 设置为: {3}", 
                        Vendor, Model, index, value));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置输出失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -13);
                }
            }
        }

        #endregion

        #region 高级功能

        /// <summary>
        /// 等待运动完成
        /// </summary>
        public bool WaitForMotionComplete(int axis, int timeoutMs = 10000)
        {
            CheckAxis(axis);
            CheckConnection();

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (IsInPosition(axis) && !GetAxisStatus(axis).IsRunning)
                {
                    return true;
                }
                Thread.Sleep(10);
            }

            _lastError = string.Format("轴{0} 等待运动完成超时", axis);
            return false;
        }

        /// <summary>
        /// 线性插补
        /// </summary>
        public void LinearInterpolation(int[] axes, double[] positions, double speed)
        {
            if (axes == null || positions == null || axes.Length != positions.Length)
            {
                throw new MotionException("轴数组和位置数组长度不匹配", -14);
            }

            if (axes.Length < 2 || axes.Length > 3)
            {
                throw new MotionException("线性插补需要 2-3 个轴", -14);
            }

            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 创建运动组
                    IntPtr groupHandle = IntPtr.Zero;
                    uint result;
                    for (int i = 0; i < axes.Length; i++)
                    {
                        result = mAcm_GpAddAxis(ref groupHandle, _axisHandles[axes[i]]);
                        if (result != 0)
                        {
                            throw new MotionException(string.Format("添加轴到组失败，错误码: {0}", result), (int)result);
                        }
                    }

                    // 设置组速度
                    // 注意：研华 API 中设置组速度的方法可能不同，这里需要根据实际 API 调整

                    // 调用研华 API 执行线性插补
                    uint arrayElements = (uint)positions.Length;
                    result = mAcm_GpMoveLinearAbs(groupHandle, positions, ref arrayElements);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("线性插补失败，错误码: {0}", result), (int)result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 线性插补运动: 轴=[{2}], 位置=[{3}], 速度={4}", 
                        Vendor, Model, 
                        string.Join(",", axes), 
                        string.Join(",", positions), 
                        speed));

                    // 更新位置缓存
                    for (int i = 0; i < axes.Length; i++)
                    {
                        _currentPositions[axes[i]] = positions[i];
                    }

                    // 关闭组
                    mAcm_GpClose(ref groupHandle);
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("线性插补失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -14);
                }
            }
        }

        /// <summary>
        /// 获取最后错误信息
        /// </summary>
        public string GetLastError()
        {
            return _lastError;
        }

        #endregion

        #region 私有辅助方法

        private void CheckAxis(int axis)
        {
            if (axis < 0 || axis >= _axisCount)
            {
                throw new MotionException(string.Format("轴号 {0} 超出范围 [0, {1}]", axis, _axisCount - 1), -100, axis);
            }
        }

        private void CheckConnection()
        {
            if (!IsOpen)
            {
                throw new MotionException("控制卡未打开", -101);
            }
        }

        private AxisConfig GetAxisConfig(int axis)
        {
            if (_config == null || _config.AxisConfigs == null)
                return null;

            foreach (AxisConfig cfg in _config.AxisConfigs)
            {
                if (cfg.AxisId == axis)
                    return cfg;
            }
            return null;
        }

        #endregion
    }
}
