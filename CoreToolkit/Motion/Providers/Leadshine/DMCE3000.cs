using System;
using System.Threading;
using CoreToolkit.Motion.Core;

namespace CoreToolkit.Motion.Providers.Leadshine
{
    /// <summary>
    /// 雷赛 DMC-E3000 系列 EtherCAT 运动控制器实现
    /// DMC-E3000 是雷赛的高性能 EtherCAT 总线运动控制器
    /// 支持 4/6/8 轴（根据具体型号）
    /// </summary>
    public class DMCE3000 : IMotionCard
    {
        private readonly object _lockObj = new object();
        private bool _disposed = false;
        private MotionConfig _config;
        private string _lastError = "";
        private ushort _cardHandle = 0;

        // 轴数量（根据具体型号，默认为4轴）
        private int _axisCount = 4;

        // 轴状态缓存
        private double[] _currentPositions;
        private bool[] _servoStates;
        private bool[] _homedStates;

        #region 属性实现

        public string CardName { get { return "DMC-E3000"; } }
        public string Vendor { get { return "Leadshine"; } }
        public string Model { get { return "DMC-E3000"; } }
        public int CardId { get; private set; }
        public int AxisCount { get { return _axisCount; } }
        public bool IsInitialized { get; private set; }
        public bool IsOpen { get; private set; }

        #endregion

        #region 构造函数

        public DMCE3000() : this(4) { }

        public DMCE3000(int axisCount)
        {
            if (axisCount != 4 && axisCount != 6 && axisCount != 8)
            {
                throw new ArgumentException("DMC-E3000 仅支持 4/6/8 轴配置");
            }
            _axisCount = axisCount;
            _currentPositions = new double[_axisCount];
            _servoStates = new bool[_axisCount];
            _homedStates = new bool[_axisCount];
            IsInitialized = false;
            IsOpen = false;
        }

        #endregion

        #region 生命周期方法

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
                    // 调用雷赛 API 初始化卡
                    short result = LTDMC.dmc_board_init();
                    if (result <= 0 || result <= CardId)
                    {
                        throw new MotionException("未找到控制卡", -1);
                    }

                    // 获取卡号
                    ushort cardNum = 0;
                    uint[] cardTypeList = new uint[8];
                    ushort[] cardIdList = new ushort[8];
                    LTDMC.dmc_get_CardInfList(ref cardNum, cardTypeList, cardIdList);

                    if (cardNum > 0)
                    {
                        _cardHandle = (ushort)CardId;
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 控制卡初始化成功，卡号: {2}，轴数: {3}",
                        Vendor, Model, CardId, _axisCount));
                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("初始化失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -1);
                }
            }
        }

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
                    // 初始化轴参数
                    for (int i = 0; i < _axisCount; i++)
                    {
                        double pos = 0;
                        LTDMC.dmc_get_position_unit(_cardHandle, (ushort)i, ref pos);
                        _currentPositions[i] = pos;

                        // 读取伺服状态
                        short sevon = LTDMC.dmc_read_sevon_pin(_cardHandle, (ushort)i);
                        _servoStates[i] = (sevon == 0); // 0=使能，1=关闭

                        _homedStates[i] = false;
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 控制卡已打开，句柄: {2}",
                        Vendor, Model, _cardHandle));
                    IsOpen = true;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("打开设备失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -2);
                }
            }
        }

        public void Close()
        {
            lock (_lockObj)
            {
                if (!IsOpen) return;

                try
                {
                    // 先停止所有轴
                    StopAll(true);

                    // 关闭控制卡
                    LTDMC.dmc_board_close();

                    _cardHandle = 0;
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

        public void Reset()
        {
            lock (_lockObj)
            {
                CheckConnection();

                try
                {
                    // 硬件复位
                    LTDMC.dmc_board_reset();

                    // 复位轴状态
                    for (int i = 0; i < _axisCount; i++)
                    {
                        _currentPositions[i] = 0;
                        _homedStates[i] = false;
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

        public void Dispose()
        {
            if (_disposed) return;

            Close();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 运动控制方法

        public double GetPosition(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    double pos = 0;
                    short result = LTDMC.dmc_get_position_unit(_cardHandle, (ushort)axis, ref pos);
                    if (result == 0)
                    {
                        _currentPositions[axis] = pos;
                        return pos;
                    }
                    return _currentPositions[axis];
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取位置失败: {0}", ex.Message);
                    return _currentPositions[axis];
                }
            }
        }

        public void SetPosition(int axis, double position)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    short result = LTDMC.dmc_set_position_unit(_cardHandle, (ushort)axis, position);
                    if (result == 0)
                    {
                        _currentPositions[axis] = position;
                        Console.WriteLine(string.Format("[{0} {1}] 轴{2} 位置设置为: {3}",
                            Vendor, Model, axis, position));
                    }
                    else
                    {
                        throw new MotionException(string.Format("设置位置失败，错误码: {0}", result), result, axis);
                    }
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置位置失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -4, axis);
                }
            }
        }

        public void MoveAbsolute(int axis, double position, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    AxisConfig axisConfig = GetAxisConfig(axis);
                    double acc = axisConfig != null ? axisConfig.DefaultAcceleration : 100000;

                    // 设置速度曲线
                    SetVelocityProfile(axis, acc, acc, 0);

                    // 执行绝对位置运动 (1=绝对坐标模式)
                    short result = LTDMC.dmc_pmove_unit(_cardHandle, (ushort)axis, position, 1);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("绝对运动失败，错误码: {0}", result), result, axis);
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

        public void MoveRelative(int axis, double distance, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    double targetPos = _currentPositions[axis] + distance;

                    AxisConfig axisConfig = GetAxisConfig(axis);
                    double acc = axisConfig != null ? axisConfig.DefaultAcceleration : 100000;

                    // 设置速度曲线
                    SetVelocityProfile(axis, acc, acc, 0);

                    // 执行相对位置运动 (0=相对坐标模式)
                    short result = LTDMC.dmc_pmove_unit(_cardHandle, (ushort)axis, distance, 0);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("相对运动失败，错误码: {0}", result), result, axis);
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
                    // JOG运动 (0=负方向, 1=正方向)
                    ushort dir = (ushort)(direction > 0 ? 1 : 0);
                    short result = LTDMC.dmc_vmove(_cardHandle, (ushort)axis, dir);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("JOG运动失败，错误码: {0}", result), result, axis);
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

        public void Stop(int axis, bool emergency = false)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    short result;
                    if (emergency)
                    {
                        result = LTDMC.dmc_emg_stop(_cardHandle);
                    }
                    else
                    {
                        // 0=减速停止
                        result = LTDMC.dmc_stop(_cardHandle, (ushort)axis, 0);
                    }

                    if (result != 0)
                    {
                        throw new MotionException(string.Format("停止失败，错误码: {0}", result), result, axis);
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

        public void StopAll(bool emergency = false)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    if (emergency)
                    {
                        LTDMC.dmc_emg_stop(_cardHandle);
                    }
                    else
                    {
                        for (int i = 0; i < _axisCount; i++)
                        {
                            LTDMC.dmc_stop(_cardHandle, (ushort)i, 0);
                        }
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

        public void Home(int axis, double speed)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    AxisConfig axisConfig = GetAxisConfig(axis);
                    double highSpeed = axisConfig != null ? axisConfig.HomeSpeedHigh : 10000;
                    double lowSpeed = axisConfig != null ? axisConfig.HomeSpeedLow : 1000;
                    double tacc = axisConfig != null ? axisConfig.DefaultAcceleration : 500000;
                    double tdec = axisConfig != null ? axisConfig.DefaultDeceleration : 500000;

                    // 设置回零速度参数
                    LTDMC.dmc_set_home_profile_unit(_cardHandle, (ushort)axis, lowSpeed, highSpeed, tacc, tdec);

                    // 执行回零
                    short result = LTDMC.dmc_home_move(_cardHandle, (ushort)axis);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("回零失败，错误码: {0}", result), result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 开始回零: 高速={3}, 低速={4}",
                        Vendor, Model, axis, highSpeed, lowSpeed));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("回零失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -10, axis);
                }
            }
        }

        public AxisStatus GetAxisStatus(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 检查运动状态 (0=运行中, 1=停止)
                    short runState = LTDMC.dmc_check_done(_cardHandle, (ushort)axis);

                    // 读取IO状态
                    uint ioStatus = LTDMC.dmc_axis_io_status(_cardHandle, (ushort)axis);

                    // 读取实际位置
                    double pos = 0;
                    LTDMC.dmc_get_position_unit(_cardHandle, (ushort)axis, ref pos);

                    // 读取速度
                    double speed = 0;
                    LTDMC.dmc_read_current_speed_unit(_cardHandle, (ushort)axis, ref speed);

                    // 读取伺服状态
                    short sevon = LTDMC.dmc_read_sevon_pin(_cardHandle, (ushort)axis);

                    // 解析IO状态位
                    // bit0: 伺服报警, bit1: 正限位, bit2: 负限位, bit3: 原点
                    bool isAlarm = (ioStatus & 0x01) != 0;
                    bool posLimit = (ioStatus & 0x02) != 0;
                    bool negLimit = (ioStatus & 0x04) != 0;
                    bool homeSignal = (ioStatus & 0x08) != 0;

                    AxisStatus status = new AxisStatus
                    {
                        Axis = axis,
                        IsRunning = (runState == 0),
                        InPosition = (runState == 1),
                        ServoOn = (sevon == 0), // 0=使能
                        IsAlarm = isAlarm,
                        PositiveLimit = posLimit,
                        NegativeLimit = negLimit,
                        HomeSignal = homeSignal,
                        Homed = _homedStates[axis],
                        CurrentSpeed = speed,
                        CommandPosition = _currentPositions[axis],
                        ActualPosition = pos,
                        AlarmCode = isAlarm ? 1 : 0
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

        public bool IsInPosition(int axis)
        {
            return GetAxisStatus(axis).InPosition;
        }

        public void SetServoEnable(int axis, bool enable)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 0=使能, 1=关闭
                    short result = LTDMC.dmc_write_sevon_pin(_cardHandle, (ushort)axis, (ushort)(enable ? 0 : 1));
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("伺服使能设置失败，错误码: {0}", result), result, axis);
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

        public bool GetServoEnable(int axis)
        {
            CheckAxis(axis);
            CheckConnection();

            try
            {
                short sevon = LTDMC.dmc_read_sevon_pin(_cardHandle, (ushort)axis);
                return (sevon == 0); // 0=使能
            }
            catch
            {
                return _servoStates[axis];
            }
        }

        public void SetVelocityProfile(int axis, double acc, double dec, double sCurve = 0)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    AxisConfig axisConfig = GetAxisConfig(axis);
                    double minVel = axisConfig != null ? 0 : 0;
                    double maxVel = axisConfig != null ? axisConfig.MaxSpeed : 100000;
                    double stopVel = axisConfig != null ? 0 : 0;

                    // 设置速度曲线
                    short result = LTDMC.dmc_set_profile_unit(_cardHandle, (ushort)axis, minVel, maxVel, acc, dec, stopVel);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置速度曲线失败，错误码: {0}", result), result, axis);
                    }

                    // 设置S曲线参数
                    if (sCurve > 0)
                    {
                        LTDMC.dmc_set_s_profile(_cardHandle, (ushort)axis, 0, sCurve);
                    }

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

        public bool ReadInput(int index)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 读取单个输入点
                    short value = LTDMC.dmc_read_inbit(_cardHandle, (ushort)index);
                    return (value == 0); // 0=ON, 1=OFF（根据实际接线可能相反）
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输入失败: {0}", ex.Message);
                    return false;
                }
            }
        }

        public bool ReadOutput(int index)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 读取单个输出点
                    short value = LTDMC.dmc_read_outbit(_cardHandle, (ushort)index);
                    return (value == 0); // 0=ON, 1=OFF
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("读取输出失败: {0}", ex.Message);
                    return false;
                }
            }
        }

        public void WriteOutput(int index, bool value)
        {
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 0=ON, 1=OFF
                    ushort onOff = (ushort)(value ? 0 : 1);
                    short result = LTDMC.dmc_write_outbit(_cardHandle, (ushort)index, onOff);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置输出失败，错误码: {0}", result), result);
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

        public bool WaitForMotionComplete(int axis, int timeoutMs = 10000)
        {
            CheckAxis(axis);
            CheckConnection();

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                // 检查运动状态 (0=运行中, 1=停止)
                short runState = LTDMC.dmc_check_done(_cardHandle, (ushort)axis);
                if (runState == 1)
                {
                    return true;
                }
                Thread.Sleep(10);
            }

            _lastError = string.Format("轴{0} 等待运动完成超时", axis);
            return false;
        }

        public void LinearInterpolation(int[] axes, double[] positions, double speed)
        {
            if (axes == null || positions == null || axes.Length != positions.Length)
            {
                throw new MotionException("轴数组和位置数组长度不匹配", -14);
            }

            if (axes.Length < 2 || axes.Length > 4)
            {
                throw new MotionException("线性插补需要 2-4 个轴", -14);
            }

            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 使用坐标系0
                    ushort crd = 0;
                    ushort axisNum = (ushort)axes.Length;
                    ushort[] axisList = new ushort[axes.Length];
                    for (int i = 0; i < axes.Length; i++)
                    {
                        axisList[i] = (ushort)axes[i];
                    }

                    // 设置插补速度参数
                    AxisConfig cfg = GetAxisConfig(axes[0]);
                    double acc = cfg != null ? cfg.DefaultAcceleration : 500000;
                    LTDMC.dmc_set_vector_profile_unit(_cardHandle, crd, 0, speed, acc, acc, 0);

                    // 执行直线插补 (1=绝对坐标模式)
                    short result = LTDMC.dmc_line_unit(_cardHandle, crd, axisNum, axisList, positions, 1);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("线性插补失败，错误码: {0}", result), result);
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
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("线性插补失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -14);
                }
            }
        }

        public string GetLastError()
        {
            return _lastError;
        }

        #endregion

        #region 雷赛特有功能

        public void SetGearRatio(int axis, int gearNumerator, int gearDenominator)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 设置电子齿轮比
                    short result = LTDMC.dmc_SetGearProfile(_cardHandle, (ushort)axis, 0, 0,
                        (Int32)gearNumerator, (Int32)gearDenominator, 0);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("设置电子齿轮比失败，错误码: {0}", result), result, axis);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 电子齿轮比设置为: {3}/{4}",
                        Vendor, Model, axis, gearNumerator, gearDenominator));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("设置电子齿轮比失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -20, axis);
                }
            }
        }

        public void HandWheelMove(int axis, int ratio)
        {
            CheckAxis(axis);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 设置手轮模式
                    short result = LTDMC.dmc_set_handwheel_inmode(_cardHandle, (ushort)axis, 0, ratio, 0);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("手轮运动设置失败，错误码: {0}", result), result, axis);
                    }

                    // 启动手轮运动
                    LTDMC.dmc_handwheel_move(_cardHandle, (ushort)axis);

                    Console.WriteLine(string.Format("[{0} {1}] 轴{2} 手轮运动: 倍率={3}",
                        Vendor, Model, axis, ratio));
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("手轮运动设置失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -21, axis);
                }
            }
        }

        public void ArcInterpolation(int axis1, int axis2, double targetPos1, double targetPos2,
            double center1, double center2, int direction, double speed)
        {
            CheckAxis(axis1);
            CheckAxis(axis2);
            CheckConnection();

            lock (_lockObj)
            {
                try
                {
                    // 使用坐标系0
                    ushort crd = 0;
                    ushort[] axisList = new ushort[] { (ushort)axis1, (ushort)axis2 };
                    double[] targetPos = new double[] { targetPos1, targetPos2 };
                    double[] centerPos = new double[] { center1, center2 };
                    ushort arcDir = (ushort)(direction > 0 ? 1 : 0); // 0=顺时针, 1=逆时针

                    // 设置插补速度参数
                    AxisConfig cfg = GetAxisConfig(axis1);
                    double acc = cfg != null ? cfg.DefaultAcceleration : 500000;
                    LTDMC.dmc_set_vector_profile_unit(_cardHandle, crd, 0, speed, acc, acc, 0);

                    // 执行圆弧插补 (Circle=0表示不是整圆, 1=绝对坐标模式)
                    short result = LTDMC.dmc_arc_move_center_unit(_cardHandle, crd, 2, axisList,
                        targetPos, centerPos, arcDir, 0, 1);
                    if (result != 0)
                    {
                        throw new MotionException(string.Format("圆弧插补失败，错误码: {0}", result), result);
                    }

                    Console.WriteLine(string.Format("[{0} {1}] 圆弧插补: 轴[{2},{3}], 目标[({4},{5})], 圆心[({6},{7})], 方向={8}, 速度={9}",
                        Vendor, Model, axis1, axis2, targetPos1, targetPos2, center1, center2, direction, speed));

                    _currentPositions[axis1] = targetPos1;
                    _currentPositions[axis2] = targetPos2;
                }
                catch (Exception ex)
                {
                    _lastError = string.Format("圆弧插补失败: {0}", ex.Message);
                    throw new MotionException(_lastError, -22);
                }
            }
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
            if (_config == null || _config.AxisConfigs == null) return null;

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
