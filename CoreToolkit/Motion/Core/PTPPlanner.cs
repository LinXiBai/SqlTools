using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// PTP (Point-To-Point) 点到点运动规划器
    /// 支持多种路径规划策略
    /// </summary>
    public class PTPPlanner
    {
        private readonly int _axisCount;
        private readonly double[] _maxSpeeds;
        private readonly double[] _maxAccs;

        /// <summary>
        /// 路径规划策略
        /// </summary>
        public PTPStrategy Strategy { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="axisCount">轴数量</param>
        /// <param name="maxSpeeds">各轴最大速度</param>
        /// <param name="maxAccs">各轴最大加速度</param>
        public PTPPlanner(int axisCount, double[] maxSpeeds, double[] maxAccs)
        {
            _axisCount = axisCount;
            _maxSpeeds = maxSpeeds ?? new double[axisCount];
            _maxAccs = maxAccs ?? new double[axisCount];
            Strategy = PTPStrategy.TimeOptimal;
        }

        /// <summary>
        /// 规划PTP运动
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="endPos">目标位置</param>
        /// <param name="speedPercent">速度百分比(0-100)</param>
        /// <returns>规划结果</returns>
        public PTPPlanResult Plan(double[] startPos, double[] endPos, double speedPercent = 100)
        {
            if (startPos == null || endPos == null || startPos.Length != _axisCount || endPos.Length != _axisCount)
                throw new ArgumentException("位置数组长度必须与轴数匹配");

            speedPercent = Math.Max(1, Math.Min(100, speedPercent));
            double scale = speedPercent / 100.0;

            // 计算各轴距离
            double[] distances = new double[_axisCount];
            for (int i = 0; i < _axisCount; i++)
            {
                distances[i] = endPos[i] - startPos[i];
            }

            switch (Strategy)
            {
                case PTPStrategy.TimeOptimal:
                    return PlanTimeOptimal(distances, scale);
                case PTPStrategy.SpeedOptimal:
                    return PlanSpeedOptimal(distances, scale);
                case PTPStrategy.Synchronized:
                    return PlanSynchronized(distances, scale);
                default:
                    return PlanTimeOptimal(distances, scale);
            }
        }

        /// <summary>
        /// 时间最优策略 - 最快到达目标
        /// 各轴以最大能力独立运动
        /// </summary>
        private PTPPlanResult PlanTimeOptimal(double[] distances, double scale)
        {
            var axisProfiles = new AxisProfile[_axisCount];
            double maxTime = 0;

            for (int i = 0; i < _axisCount; i++)
            {
                double speed = _maxSpeeds[i] * scale;
                double acc = _maxAccs[i] * scale;
                var profile = CalculateAxisProfile(distances[i], speed, acc);
                axisProfiles[i] = profile;
                maxTime = Math.Max(maxTime, profile.TotalTime);
            }

            return new PTPPlanResult
            {
                AxisProfiles = axisProfiles,
                TotalTime = maxTime,
                Strategy = PTPStrategy.TimeOptimal,
                IsReachable = true
            };
        }

        /// <summary>
        /// 速度最优策略 - 尽量保持高速运动
        /// </summary>
        private PTPPlanResult PlanSpeedOptimal(double[] distances, double scale)
        {
            // 找出主导轴（距离最长的轴）
            int masterAxis = 0;
            double maxDist = 0;
            for (int i = 0; i < _axisCount; i++)
            {
                double absDist = Math.Abs(distances[i]);
                if (absDist > maxDist)
                {
                    maxDist = absDist;
                    masterAxis = i;
                }
            }

            var axisProfiles = new AxisProfile[_axisCount];

            // 主导轴以最大速度运动
            double masterSpeed = _maxSpeeds[masterAxis] * scale;
            double masterAcc = _maxAccs[masterAxis] * scale;
            axisProfiles[masterAxis] = CalculateAxisProfile(distances[masterAxis], masterSpeed, masterAcc);
            double masterTime = axisProfiles[masterAxis].TotalTime;

            // 其他轴按时间同步
            for (int i = 0; i < _axisCount; i++)
            {
                if (i == masterAxis) continue;

                // 计算需要的速度以达到同步
                double targetTime = masterTime;
                double dist = distances[i];
                double acc = _maxAccs[i] * scale;

                // 使用三角速度曲线计算所需速度
                // s = (v^2) / a  =>  v = sqrt(s * a)
                double minTimeForAcc = 2 * Math.Sqrt(Math.Abs(dist) / acc);
                
                double requiredSpeed;
                if (targetTime <= minTimeForAcc)
                {
                    // 无法达到目标时间，使用最大加速度
                    requiredSpeed = Math.Sqrt(Math.Abs(dist) * acc);
                }
                else
                {
                    // v = s / (t - v/a)，求解二次方程
                    // v^2/a - v*t + s = 0
                    double a = 1.0 / acc;
                    double b = -targetTime;
                    double c = Math.Abs(dist);
                    double discriminant = b * b - 4 * a * c;
                    
                    if (discriminant >= 0)
                    {
                        requiredSpeed = (-b - Math.Sqrt(discriminant)) / (2 * a);
                    }
                    else
                    {
                        requiredSpeed = _maxSpeeds[i] * scale;
                    }
                }

                requiredSpeed = Math.Min(requiredSpeed, _maxSpeeds[i] * scale);
                axisProfiles[i] = CalculateAxisProfile(distances[i], requiredSpeed, acc);
            }

            return new PTPPlanResult
            {
                AxisProfiles = axisProfiles,
                TotalTime = masterTime,
                Strategy = PTPStrategy.SpeedOptimal,
                IsReachable = true
            };
        }

        /// <summary>
        /// 同步策略 - 所有轴同时到达
        /// </summary>
        private PTPPlanResult PlanSynchronized(double[] distances, double scale)
        {
            // 计算各轴独立运动所需时间
            double[] times = new double[_axisCount];
            double[] maxSpeeds = new double[_axisCount];

            for (int i = 0; i < _axisCount; i++)
            {
                double speed = _maxSpeeds[i] * scale;
                double acc = _maxAccs[i] * scale;
                var profile = CalculateAxisProfile(distances[i], speed, acc);
                times[i] = profile.TotalTime;
                maxSpeeds[i] = speed;
            }

            // 以最慢的轴为基准
            double syncTime = times.Max();
            var axisProfiles = new AxisProfile[_axisCount];

            for (int i = 0; i < _axisCount; i++)
            {
                double dist = distances[i];
                double acc = _maxAccs[i] * scale;
                
                // 调整速度以匹配同步时间
                double adjustedSpeed;
                if (syncTime <= 0)
                {
                    adjustedSpeed = 0;
                }
                else
                {
                    // 简化的速度计算：假设梯形速度曲线
                    // T = Ta + Tv + Td = 2*v/a + (s - v^2/a)/v = v/a + s/v
                    // 近似求解：v ≈ s / T (当匀速段较长时)
                    adjustedSpeed = Math.Abs(dist) / syncTime;
                    
                    // 考虑加速度限制：T_min = 2*sqrt(s/a)
                    double minTime = 2 * Math.Sqrt(Math.Abs(dist) / acc);
                    if (syncTime <= minTime)
                    {
                        // 三角形速度曲线
                        adjustedSpeed = Math.Sqrt(Math.Abs(dist) * acc);
                    }
                }

                adjustedSpeed = Math.Min(adjustedSpeed, _maxSpeeds[i] * scale);
                axisProfiles[i] = CalculateAxisProfile(distances[i], adjustedSpeed, acc);
            }

            return new PTPPlanResult
            {
                AxisProfiles = axisProfiles,
                TotalTime = syncTime,
                Strategy = PTPStrategy.Synchronized,
                IsReachable = true
            };
        }

        /// <summary>
        /// 计算单轴速度曲线
        /// </summary>
        private AxisProfile CalculateAxisProfile(double distance, double maxSpeed, double maxAcc)
        {
            if (Math.Abs(distance) < 0.001)
            {
                return new AxisProfile
                {
                    Distance = 0,
                    MaxSpeed = 0,
                    Acceleration = 0,
                    AccelTime = 0,
                    ConstantTime = 0,
                    DecelTime = 0,
                    TotalTime = 0
                };
            }

            double direction = Math.Sign(distance);
            double s = Math.Abs(distance);
            double v = maxSpeed;
            double a = maxAcc;

            // 达到最大速度所需距离：s_acc = v^2 / (2*a)
            double accDist = v * v / (2 * a);

            double tAcc, tConst, tDec, totalTime;

            if (accDist * 2 <= s)
            {
                // 梯形速度曲线（有匀速段）
                tAcc = v / a;
                tDec = v / a;
                double constDist = s - 2 * accDist;
                tConst = constDist / v;
                totalTime = tAcc + tConst + tDec;
            }
            else
            {
                // 三角形速度曲线（无匀速段，达不到最大速度）
                // s = 2 * (0.5 * a * t^2) = a * t^2
                // t = sqrt(s / a)
                tAcc = Math.Sqrt(s / a);
                tDec = tAcc;
                tConst = 0;
                v = a * tAcc; // 实际达到的最大速度
                totalTime = tAcc + tDec;
            }

            return new AxisProfile
            {
                Distance = distance,
                MaxSpeed = v * direction,
                Acceleration = a,
                AccelTime = tAcc,
                ConstantTime = tConst,
                DecelTime = tDec,
                TotalTime = totalTime
            };
        }
    }

    /// <summary>
    /// PTP规划策略
    /// </summary>
    public enum PTPStrategy
    {
        /// <summary>
        /// 时间最优 - 最快到达
        /// </summary>
        TimeOptimal,

        /// <summary>
        /// 速度最优 - 保持高速
        /// </summary>
        SpeedOptimal,

        /// <summary>
        /// 同步运动 - 各轴同时到达
        /// </summary>
        Synchronized
    }

    /// <summary>
    /// PTP规划结果
    /// </summary>
    public class PTPPlanResult
    {
        /// <summary>
        /// 各轴速度曲线
        /// </summary>
        public AxisProfile[] AxisProfiles { get; set; }

        /// <summary>
        /// 总运动时间(秒)
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// 使用的策略
        /// </summary>
        public PTPStrategy Strategy { get; set; }

        /// <summary>
        /// 是否可达
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// 获取指定时刻各轴的位置
        /// </summary>
        public double[] GetPositionsAtTime(double time)
        {
            if (AxisProfiles == null) return null;
            
            return AxisProfiles.Select(p => p.GetPositionAtTime(time)).ToArray();
        }

        /// <summary>
        /// 获取指定时刻各轴的速度
        /// </summary>
        public double[] GetVelocitiesAtTime(double time)
        {
            if (AxisProfiles == null) return null;
            
            return AxisProfiles.Select(p => p.GetVelocityAtTime(time)).ToArray();
        }
    }

    /// <summary>
    /// 单轴速度曲线
    /// </summary>
    public class AxisProfile
    {
        /// <summary>
        /// 目标距离
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// 最大速度
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// 加速度
        /// </summary>
        public double Acceleration { get; set; }

        /// <summary>
        /// 加速段时间
        /// </summary>
        public double AccelTime { get; set; }

        /// <summary>
        /// 匀速段时间
        /// </summary>
        public double ConstantTime { get; set; }

        /// <summary>
        /// 减速段时间
        /// </summary>
        public double DecelTime { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// 获取指定时刻的位置
        /// </summary>
        public double GetPositionAtTime(double t)
        {
            if (t <= 0) return 0;
            if (t >= TotalTime) return Distance;

            double dir = Math.Sign(Distance);
            double s = Math.Abs(Distance);
            double v = Math.Abs(MaxSpeed);
            double a = Acceleration;

            double pos;
            if (t <= AccelTime)
            {
                // 加速段: s = 0.5 * a * t^2
                pos = 0.5 * a * t * t;
            }
            else if (t <= AccelTime + ConstantTime)
            {
                // 匀速段
                double tConst = t - AccelTime;
                double accDist = 0.5 * a * AccelTime * AccelTime;
                pos = accDist + v * tConst;
            }
            else
            {
                // 减速段
                double tDec = t - AccelTime - ConstantTime;
                double accDist = 0.5 * a * AccelTime * AccelTime;
                double constDist = v * ConstantTime;
                double vCurrent = v - a * tDec;
                pos = accDist + constDist + (v + vCurrent) * tDec / 2;
            }

            return pos * dir;
        }

        /// <summary>
        /// 获取指定时刻的速度
        /// </summary>
        public double GetVelocityAtTime(double t)
        {
            if (t <= 0 || t >= TotalTime) return 0;

            double dir = Math.Sign(Distance);
            double v = Math.Abs(MaxSpeed);
            double a = Acceleration;

            if (t <= AccelTime)
            {
                // 加速段
                return a * t * dir;
            }
            else if (t <= AccelTime + ConstantTime)
            {
                // 匀速段
                return v * dir;
            }
            else
            {
                // 减速段
                double tDec = t - AccelTime - ConstantTime;
                return (v - a * tDec) * dir;
            }
        }
    }
}
