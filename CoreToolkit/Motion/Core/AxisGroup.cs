using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴组实现类
    /// 实现多轴插补、PTP、PVT功能
    /// </summary>
    public class AxisGroup : IAxisGroup
    {
        private readonly IMotionCard _motionCard;
        private readonly object _lockObj = new object();
        private bool _isEnabled = false;
        private bool _isMoving = false;
        private MotionMode _currentMode = MotionMode.Idle;
        private PTPPlanner _ptpPlanner;
        private PVTPlanner _pvtPlanner;
        private CancellationTokenSource _motionCts;

        /// <summary>
        /// 组ID
        /// </summary>
        public int GroupId { get; }

        /// <summary>
        /// 组名称
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 轴数量
        /// </summary>
        public int AxisCount => AxisIds?.Length ?? 0;

        /// <summary>
        /// 轴号列表
        /// </summary>
        public int[] AxisIds { get; }

        /// <summary>
        /// 是否已启用
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// 是否正在运动
        /// </summary>
        public bool IsMoving => _isMoving;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AxisGroup(int groupId, string groupName, int[] axisIds, IMotionCard motionCard)
        {
            GroupId = groupId;
            GroupName = groupName;
            AxisIds = axisIds?.Distinct().ToArray() ?? new int[0];
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));

            // 初始化规划器
            int axisCount = AxisCount;
            double[] maxSpeeds = new double[axisCount];
            double[] maxAccs = new double[axisCount];

            for (int i = 0; i < axisCount; i++)
            {
                maxSpeeds[i] = 100000; // 默认值
                maxAccs[i] = 1000000;
            }

            _ptpPlanner = new PTPPlanner(axisCount, maxSpeeds, maxAccs);
            _pvtPlanner = new PVTPlanner(axisCount, maxSpeeds, maxAccs);
        }

        /// <summary>
        /// 启用轴组
        /// </summary>
        public void Enable()
        {
            lock (_lockObj)
            {
                if (_isEnabled) return;

                // 确保所有轴的伺服已使能
                foreach (int axis in AxisIds)
                {
                    _motionCard.SetServoEnable(axis, true);
                }

                _isEnabled = true;
            }
        }

        /// <summary>
        /// 禁用轴组
        /// </summary>
        public void Disable()
        {
            lock (_lockObj)
            {
                if (!_isEnabled) return;

                Stop(true);

                // 关闭伺服
                foreach (int axis in AxisIds)
                {
                    _motionCard.SetServoEnable(axis, false);
                }

                _isEnabled = false;
            }
        }

        /// <summary>
        /// 线性插补（绝对位置）
        /// </summary>
        public void MoveLinearAbs(double[] positions, double speed, double acc, double dec)
        {
            lock (_lockObj)
            {
                CheckEnabled();
                CheckPositions(positions);

                _isMoving = true;
                _currentMode = MotionMode.LinearInterpolation;
                _motionCts = new CancellationTokenSource();

                try
                {
                    // 调用控制卡的线性插补功能
                    _motionCard.LinearInterpolation(AxisIds, positions, speed);
                }
                catch (Exception)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    throw;
                }
            }
        }

        /// <summary>
        /// 线性插补（相对位置）
        /// </summary>
        public void MoveLinearRel(double[] distances, double speed, double acc, double dec)
        {
            lock (_lockObj)
            {
                CheckEnabled();
                CheckPositions(distances);

                // 获取当前位置并计算绝对位置
                double[] absPositions = new double[AxisCount];
                for (int i = 0; i < AxisCount; i++)
                {
                    absPositions[i] = _motionCard.GetPosition(AxisIds[i]) + distances[i];
                }

                MoveLinearAbs(absPositions, speed, acc, dec);
            }
        }

        /// <summary>
        /// 圆弧插补（绝对位置）
        /// </summary>
        public void MoveCircularAbs(double[] center, double[] endPos, int direction, double speed)
        {
            lock (_lockObj)
            {
                CheckEnabled();
                
                if (AxisCount < 2)
                    throw new MotionException("圆弧插补需要至少2个轴", -1);

                _isMoving = true;
                _currentMode = MotionMode.CircularInterpolation;

                try
                {
                    // 获取当前位置（圆弧起点）
                    double startX = _motionCard.GetPosition(AxisIds[0]);
                    double startY = _motionCard.GetPosition(AxisIds[1]);

                    double centerX = center[0];
                    double centerY = center[1];
                    double endX = endPos[0];
                    double endY = endPos[1];

                    // 计算圆弧参数
                    double radius = Math.Sqrt(
                        Math.Pow(startX - centerX, 2) + 
                        Math.Pow(startY - centerY, 2));

                    double startAngle = Math.Atan2(startY - centerY, startX - centerX);
                    double endAngle = Math.Atan2(endY - centerY, endX - centerX);

                    // 规范化角度
                    if (direction > 0) // 顺时针
                    {
                        while (endAngle > startAngle)
                            endAngle -= 2 * Math.PI;
                    }
                    else // 逆时针
                    {
                        while (endAngle < startAngle)
                            endAngle += 2 * Math.PI;
                    }

                    double angleSpan = Math.Abs(endAngle - startAngle);
                    double arcLength = radius * angleSpan;
                    double duration = arcLength / speed;

                    // 圆弧插补 - 使用分段线性逼近
                    int segments = Math.Max(20, (int)(angleSpan * 10)); // 每0.1弧度一个分段
                    double angleStep = (endAngle - startAngle) / segments;
                    double timeStep = duration / segments;

                    for (int i = 1; i <= segments; i++)
                    {
                        double angle = startAngle + angleStep * i;
                        double x = centerX + radius * Math.Cos(angle);
                        double y = centerY + radius * Math.Sin(angle);

                        double[] positions = new double[AxisCount];
                        positions[0] = x;
                        positions[1] = y;
                        for (int j = 2; j < AxisCount; j++)
                        {
                            // 其他轴线性插值
                            double startPos = _motionCard.GetPosition(AxisIds[j]);
                            positions[j] = startPos + (endPos.Length > j ? endPos[j] : 0 - startPos) * i / segments;
                        }

                        // 移动到分段点
                        _motionCard.LinearInterpolation(AxisIds, positions, speed);
                        
                        // 等待运动完成
                        if (i < segments)
                        {
                            WaitForAxisStop(AxisIds[0], (int)(timeStep * 1000));
                        }
                    }
                }
                catch (Exception)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    throw;
                }
            }
        }

        /// <summary>
        /// 圆弧插补（相对位置）
        /// </summary>
        public void MoveCircularRel(double[] centerOffset, double[] endDist, int direction, double speed)
        {
            lock (_lockObj)
            {
                CheckEnabled();

                double startX = _motionCard.GetPosition(AxisIds[0]);
                double startY = _motionCard.GetPosition(AxisIds[1]);

                double[] center = new double[]
                {
                    startX + centerOffset[0],
                    startY + centerOffset[1]
                };

                double[] endPos = new double[]
                {
                    startX + endDist[0],
                    startY + endDist[1]
                };

                MoveCircularAbs(center, endPos, direction, speed);
            }
        }

        /// <summary>
        /// PTP点到点运动
        /// </summary>
        public void MovePTP(double[] positions, double speedPercent = 100)
        {
            lock (_lockObj)
            {
                CheckEnabled();
                CheckPositions(positions);

                _isMoving = true;
                _currentMode = MotionMode.PTP;

                try
                {
                    // 获取当前位置
                    double[] startPos = new double[AxisCount];
                    for (int i = 0; i < AxisCount; i++)
                    {
                        startPos[i] = _motionCard.GetPosition(AxisIds[i]);
                    }

                    // 规划PTP运动
                    var plan = _ptpPlanner.Plan(startPos, positions, speedPercent);

                    // 执行PTP运动 - 各轴独立启动
                    var tasks = new List<Task>();
                    for (int i = 0; i < AxisCount; i++)
                    {
                        int axisIndex = i;
                        var profile = plan.AxisProfiles[i];

                        tasks.Add(Task.Run(() =>
                        {
                            double targetPos = startPos[axisIndex] + profile.Distance;
                            double speed = Math.Abs(profile.MaxSpeed);
                            
                            _motionCard.SetVelocityProfile(AxisIds[axisIndex], 
                                profile.Acceleration, profile.Acceleration, 0);
                            _motionCard.MoveAbsolute(AxisIds[axisIndex], targetPos, speed);
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    throw;
                }
            }
        }

        /// <summary>
        /// PVT运动
        /// </summary>
        public void MovePVT(PVTPoint[] pvtData, int axisIndex = 0)
        {
            lock (_lockObj)
            {
                CheckEnabled();

                if (axisIndex < 0 || axisIndex >= AxisCount)
                    throw new ArgumentException("轴索引超出范围");

                _isMoving = true;
                _currentMode = MotionMode.PVT;

                try
                {
                    var plan = _pvtPlanner.PlanSingleAxis(pvtData.ToList(), axisIndex);
                    ExecutePVTTrajectory(plan.Trajectory, AxisIds[axisIndex]);
                }
                catch (Exception)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    throw;
                }
            }
        }

        /// <summary>
        /// 多轴同步PVT运动
        /// </summary>
        public void MovePVTSync(PVTPoint[][] pvtDataArray)
        {
            lock (_lockObj)
            {
                CheckEnabled();

                if (pvtDataArray == null || pvtDataArray.Length != AxisCount)
                    throw new ArgumentException("PVT数据数组长度必须与轴数匹配");

                _isMoving = true;
                _currentMode = MotionMode.PVT;

                try
                {
                    var plan = _pvtPlanner.PlanMultiAxis(pvtDataArray);

                    // 并行执行各轴PVT
                    var tasks = new List<Task>();
                    for (int i = 0; i < AxisCount; i++)
                    {
                        int axisIdx = i;
                        tasks.Add(Task.Run(() =>
                        {
                            ExecutePVTTrajectory(plan.AxisResults[axisIdx].Trajectory, AxisIds[axisIdx]);
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    throw;
                }
            }
        }

        /// <summary>
        /// 停止运动
        /// </summary>
        public void Stop(bool emergency = false)
        {
            lock (_lockObj)
            {
                _motionCts?.Cancel();

                foreach (int axis in AxisIds)
                {
                    _motionCard.Stop(axis, emergency);
                }

                _isMoving = false;
                _currentMode = MotionMode.Idle;
            }
        }

        /// <summary>
        /// 等待运动完成
        /// </summary>
        public bool WaitForComplete(int timeoutMs = 30000)
        {
            if (!_isMoving) return true;

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (_motionCts?.IsCancellationRequested == true)
                    return false;

                bool allStopped = true;
                foreach (int axis in AxisIds)
                {
                    var status = _motionCard.GetAxisStatus(axis);
                    if (status.IsRunning)
                    {
                        allStopped = false;
                        break;
                    }
                }

                if (allStopped)
                {
                    _isMoving = false;
                    _currentMode = MotionMode.Idle;
                    return true;
                }

                Thread.Sleep(10);
            }

            return false;
        }

        /// <summary>
        /// 获取组状态
        /// </summary>
        public AxisGroupStatus GetStatus()
        {
            lock (_lockObj)
            {
                var positions = new double[AxisCount];
                var velocities = new double[AxisCount];
                bool isMoving = false;

                for (int i = 0; i < AxisCount; i++)
                {
                    var status = _motionCard.GetAxisStatus(AxisIds[i]);
                    positions[i] = status.ActualPosition;
                    velocities[i] = status.CurrentSpeed;
                    if (status.IsRunning) isMoving = true;
                }

                return new AxisGroupStatus
                {
                    GroupId = GroupId,
                    IsEnabled = _isEnabled,
                    IsMoving = isMoving,
                    CurrentMode = _currentMode,
                    Positions = positions,
                    Velocities = velocities,
                    BufferSpace = 1000 // 默认缓冲区空间
                };
            }
        }

        #region 私有方法

        /// <summary>
        /// 检查轴组是否已启用
        /// </summary>
        private void CheckEnabled()
        {
            if (!_isEnabled)
                throw new MotionException("轴组未启用，请先调用Enable()", -1);
        }

        /// <summary>
        /// 检查位置数组
        /// </summary>
        private void CheckPositions(double[] positions)
        {
            if (positions == null || positions.Length != AxisCount)
                throw new ArgumentException($"位置数组长度必须为{AxisCount}");
        }

        /// <summary>
        /// 等待轴停止
        /// </summary>
        private bool WaitForAxisStop(int axis, int timeoutMs)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                var status = _motionCard.GetAxisStatus(axis);
                if (!status.IsRunning) return true;
                Thread.Sleep(5);
            }
            return false;
        }

        /// <summary>
        /// 执行PVT轨迹
        /// </summary>
        private void ExecutePVTTrajectory(List<PVTTrajectoryPoint> trajectory, int axisId)
        {
            if (trajectory == null || trajectory.Count < 2) return;

            DateTime startTime = DateTime.Now;
            int currentIndex = 0;

            while (currentIndex < trajectory.Count)
            {
                if (_motionCts?.IsCancellationRequested == true)
                    break;

                var point = trajectory[currentIndex];
                double elapsed = (DateTime.Now - startTime).TotalSeconds;

                if (elapsed >= point.Time)
                {
                    // 发送位置命令
                    _motionCard.SetPosition(axisId, point.Position);
                    currentIndex++;
                }

                Thread.Sleep(1);
            }
        }

        #endregion
    }
}
