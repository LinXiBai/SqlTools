using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// PVT (Position-Velocity-Time) 运动规划器
    /// 支持位置-速度-时间模式的轨迹规划
    /// </summary>
    public class PVTPlanner
    {
        private readonly int _axisCount;
        private readonly double[] _maxVelocities;
        private readonly double[] _maxAccelerations;

        /// <summary>
        /// 插值方法
        /// </summary>
        public PVTInterpolationMethod InterpolationMethod { get; set; }

        /// <summary>
        /// 时间单位(毫秒或秒)
        /// </summary>
        public TimeUnit TimeUnit { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PVTPlanner(int axisCount, double[] maxVelocities, double[] maxAccelerations)
        {
            _axisCount = axisCount;
            _maxVelocities = maxVelocities ?? new double[axisCount];
            _maxAccelerations = maxAccelerations ?? new double[axisCount];
            InterpolationMethod = PVTInterpolationMethod.CubicSpline;
            TimeUnit = TimeUnit.Millisecond;
        }

        /// <summary>
        /// 规划单轴PVT运动
        /// </summary>
        /// <param name="points">PVT关键点</param>
        /// <param name="axisIndex">轴索引</param>
        /// <returns>规划结果</returns>
        public PVTPlanResult PlanSingleAxis(List<PVTPoint> points, int axisIndex = 0)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("PVT点至少需要2个");

            ValidatePoints(points, axisIndex);

            var trajectory = new List<PVTTrajectoryPoint>();
            double timeScale = TimeUnit == TimeUnit.Millisecond ? 0.001 : 1.0;

            switch (InterpolationMethod)
            {
                case PVTInterpolationMethod.Linear:
                    trajectory = InterpolateLinear(points, timeScale);
                    break;
                case PVTInterpolationMethod.CubicSpline:
                    trajectory = InterpolateCubicSpline(points, timeScale);
                    break;
                case PVTInterpolationMethod.CubicHermite:
                    trajectory = InterpolateCubicHermite(points, timeScale);
                    break;
                case PVTInterpolationMethod.FifthOrder:
                    trajectory = InterpolateFifthOrder(points, timeScale);
                    break;
            }

            return new PVTPlanResult
            {
                Trajectory = trajectory,
                OriginalPoints = points,
                TotalTime = points.Sum(p => p.TimeMs) * timeScale,
                AxisIndex = axisIndex,
                SampleCount = trajectory.Count
            };
        }

        /// <summary>
        /// 规划多轴同步PVT运动
        /// </summary>
        public MultiAxisPVTResult PlanMultiAxis(PVTPoint[][] axisPoints)
        {
            if (axisPoints == null || axisPoints.Length != _axisCount)
                throw new ArgumentException("轴数不匹配");

            var results = new PVTPlanResult[_axisCount];
            double maxTime = 0;

            for (int i = 0; i < _axisCount; i++)
            {
                results[i] = PlanSingleAxis(axisPoints[i].ToList(), i);
                maxTime = Math.Max(maxTime, results[i].TotalTime);
            }

            return new MultiAxisPVTResult
            {
                AxisResults = results,
                TotalTime = maxTime,
                IsSynchronized = true
            };
        }

        /// <summary>
        /// 从路径点生成PVT数据（自动计算速度）
        /// </summary>
        /// <param name="pathPoints">路径点位置数组</param>
        /// <param name="times">各段时间数组</param>
        /// <param name="smoothness">平滑系数(0-1)</param>
        /// <returns>PVT点列表</returns>
        public List<PVTPoint> GeneratePVTFromPath(double[] pathPoints, double[] times, double smoothness = 0.5)
        {
            if (pathPoints == null || times == null || pathPoints.Length != times.Length)
                throw new ArgumentException("路径点和时间数组长度必须相同");

            int n = pathPoints.Length;
            var pvtPoints = new List<PVTPoint>();

            // 使用梯形速度曲线计算各点速度
            double[] velocities = new double[n];

            // 起点速度为0
            velocities[0] = 0;

            // 中间点速度
            for (int i = 1; i < n - 1; i++)
            {
                double prevDist = pathPoints[i] - pathPoints[i - 1];
                double nextDist = pathPoints[i + 1] - pathPoints[i];
                double prevTime = times[i - 1];
                double nextTime = times[i];

                // 平均速度
                double avgVel = (Math.Abs(prevDist) / prevTime + Math.Abs(nextDist) / nextTime) / 2;

                // 根据曲率调整速度
                double curvature = Math.Abs(nextDist - prevDist) / (prevTime + nextTime);
                double speedFactor = Math.Max(0.1, 1 - curvature * smoothness);

                velocities[i] = avgVel * speedFactor * Math.Sign(nextDist != 0 ? nextDist : prevDist);
            }

            // 终点速度为0
            velocities[n - 1] = 0;

            // 创建PVT点
            for (int i = 0; i < n; i++)
            {
                pvtPoints.Add(new PVTPoint
                {
                    Position = pathPoints[i],
                    Velocity = velocities[i],
                    TimeMs = times[i]
                });
            }

            return pvtPoints;
        }

        /// <summary>
        /// 生成连续轨迹PVT（S曲线加减速）
        /// </summary>
        public List<PVTPoint> GenerateSCurveTrajectory(double startPos, double endPos, 
            double maxVel, double maxAcc, double jerk, double segmentTimeMs)
        {
            double distance = endPos - startPos;
            double direction = Math.Sign(distance);
            double s = Math.Abs(distance);
            double v = maxVel;
            double a = maxAcc;
            double j = jerk;

            var points = new List<PVTPoint>();

            // S曲线加减速分为7段：加加速、匀加速、减加速、匀速、加减速、匀减速、减减速
            // 简化实现：使用3段式（加速、匀速、减速）
            
            // 计算加减速时间
            double tAcc = v / a;
            double accDist = 0.5 * a * tAcc * tAcc;

            double currentPos = startPos;
            double currentTime = 0;

            // 加速段
            for (double t = 0; t < tAcc; t += segmentTimeMs)
            {
                double vt = a * t;
                double st = 0.5 * a * t * t;
                points.Add(new PVTPoint(
                    startPos + st * direction,
                    vt * direction,
                    segmentTimeMs
                ));
            }

            currentPos = startPos + accDist * direction;
            currentTime = tAcc;

            // 匀速段
            double totalAccDist = accDist * 2;
            if (totalAccDist < s)
            {
                double constDist = s - totalAccDist;
                double tConst = constDist / v;

                for (double t = 0; t < tConst; t += segmentTimeMs)
                {
                    points.Add(new PVTPoint(
                        currentPos + v * t * direction,
                        v * direction,
                        segmentTimeMs
                    ));
                }

                currentPos += constDist * direction;
                currentTime += tConst;
            }

            // 减速段
            for (double t = 0; t < tAcc; t += segmentTimeMs)
            {
                double vt = v - a * t;
                double st = v * t - 0.5 * a * t * t;
                points.Add(new PVTPoint(
                    currentPos + st * direction,
                    vt * direction,
                    segmentTimeMs
                ));
            }

            // 确保终点准确
            points.Add(new PVTPoint(endPos, 0, segmentTimeMs));

            return points;
        }

        #region 插值方法

        /// <summary>
        /// 线性插值
        /// </summary>
        private List<PVTTrajectoryPoint> InterpolateLinear(List<PVTPoint> points, double timeScale)
        {
            var trajectory = new List<PVTTrajectoryPoint>();
            double currentTime = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                double dt = p2.TimeMs * timeScale;
                double steps = Math.Max(1, dt / 0.001); // 1ms采样

                for (int j = 0; j < steps; j++)
                {
                    double t = j / steps;
                    double pos = p1.Position + (p2.Position - p1.Position) * t;
                    double vel = p1.Velocity + (p2.Velocity - p1.Velocity) * t;

                    trajectory.Add(new PVTTrajectoryPoint
                    {
                        Position = pos,
                        Velocity = vel,
                        Acceleration = (p2.Velocity - p1.Velocity) / dt,
                        Time = currentTime + j * dt / steps
                    });
                }

                currentTime += dt;
            }

            // 添加最后一点
            var last = points[points.Count - 1];
            trajectory.Add(new PVTTrajectoryPoint
            {
                Position = last.Position,
                Velocity = last.Velocity,
                Acceleration = 0,
                Time = currentTime
            });

            return trajectory;
        }

        /// <summary>
        /// 三次样条插值
        /// </summary>
        private List<PVTTrajectoryPoint> InterpolateCubicSpline(List<PVTPoint> points, double timeScale)
        {
            int n = points.Count;
            double[] t = new double[n];
            double[] p = new double[n];
            double[] v = new double[n];

            // 累计时间
            double cumTime = 0;
            for (int i = 0; i < n; i++)
            {
                t[i] = cumTime;
                p[i] = points[i].Position;
                v[i] = points[i].Velocity;
                cumTime += points[i].TimeMs * timeScale;
            }

            // 计算三次样条系数
            var spline = ComputeCubicSpline(t, p, v);

            // 生成轨迹点
            var trajectory = new List<PVTTrajectoryPoint>();
            double sampleInterval = 0.001; // 1ms

            for (double time = 0; time <= t[n - 1]; time += sampleInterval)
            {
                // 找到所在区间
                int segment = 0;
                for (int i = 0; i < n - 1; i++)
                {
                    if (time >= t[i] && time <= t[i + 1])
                    {
                        segment = i;
                        break;
                    }
                }

                double dt = time - t[segment];
                double dt2 = dt * dt;
                double dt3 = dt2 * dt;

                var coef = spline[segment];
                double pos = coef.A + coef.B * dt + coef.C * dt2 + coef.D * dt3;
                double vel = coef.B + 2 * coef.C * dt + 3 * coef.D * dt2;
                double acc = 2 * coef.C + 6 * coef.D * dt;

                trajectory.Add(new PVTTrajectoryPoint
                {
                    Position = pos,
                    Velocity = vel,
                    Acceleration = acc,
                    Time = time
                });
            }

            return trajectory;
        }

        /// <summary>
        /// 三次Hermite插值
        /// </summary>
        private List<PVTTrajectoryPoint> InterpolateCubicHermite(List<PVTPoint> points, double timeScale)
        {
            var trajectory = new List<PVTTrajectoryPoint>();
            double currentTime = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[i];
                var p1 = points[i + 1];
                double dt = p1.TimeMs * timeScale;

                // 计算切线（速度）
                double m0 = p0.Velocity;
                double m1 = p1.Velocity;

                // 归一化时间采样
                int steps = Math.Max(10, (int)(dt / 0.001));
                for (int j = 0; j <= steps; j++)
                {
                    double t = (double)j / steps;
                    double t2 = t * t;
                    double t3 = t2 * t;

                    // Hermite基函数
                    double h00 = 2 * t3 - 3 * t2 + 1;
                    double h10 = t3 - 2 * t2 + t;
                    double h01 = -2 * t3 + 3 * t2;
                    double h11 = t3 - t2;

                    // 插值
                    double pos = h00 * p0.Position + h10 * dt * m0 + h01 * p1.Position + h11 * dt * m1;
                    double vel = (6 * t2 - 6 * t) * p0.Position / dt + (3 * t2 - 4 * t + 1) * m0 +
                                 (-6 * t2 + 6 * t) * p1.Position / dt + (3 * t2 - 2 * t) * m1;

                    trajectory.Add(new PVTTrajectoryPoint
                    {
                        Position = pos,
                        Velocity = vel,
                        Time = currentTime + t * dt
                    });
                }

                currentTime += dt;
            }

            return trajectory;
        }

        /// <summary>
        /// 五次多项式插值
        /// </summary>
        private List<PVTTrajectoryPoint> InterpolateFifthOrder(List<PVTPoint> points, double timeScale)
        {
            var trajectory = new List<PVTTrajectoryPoint>();
            double currentTime = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[i];
                var p1 = points[i + 1];
                double dt = p1.TimeMs * timeScale;

                double pStart = p0.Position;
                double pEnd = p1.Position;
                double vStart = p0.Velocity;
                double vEnd = p1.Velocity;
                double aStart = 0; // 假设起点加速度为0
                double aEnd = 0;   // 假设终点加速度为0

                // 计算五次多项式系数
                // p(t) = a0 + a1*t + a2*t^2 + a3*t^3 + a4*t^4 + a5*t^5
                double a0 = pStart;
                double a1 = vStart;
                double a2 = aStart / 2;
                double a3 = (20 * (pEnd - pStart) - (8 * vEnd + 12 * vStart) * dt - (3 * aStart - aEnd) * dt * dt) / (2 * dt * dt * dt);
                double a4 = (-30 * (pEnd - pStart) + (14 * vEnd + 16 * vStart) * dt + (3 * aStart - 2 * aEnd) * dt * dt) / (2 * dt * dt * dt * dt);
                double a5 = (12 * (pEnd - pStart) - 6 * (vEnd + vStart) * dt + (aEnd - aStart) * dt * dt) / (2 * dt * dt * dt * dt * dt);

                int steps = Math.Max(10, (int)(dt / 0.001));
                for (int j = 0; j <= steps; j++)
                {
                    double t = (double)j / steps * dt;
                    double t2 = t * t;
                    double t3 = t2 * t;
                    double t4 = t3 * t;
                    double t5 = t4 * t;

                    double pos = a0 + a1 * t + a2 * t2 + a3 * t3 + a4 * t4 + a5 * t5;
                    double vel = a1 + 2 * a2 * t + 3 * a3 * t2 + 4 * a4 * t3 + 5 * a5 * t4;
                    double acc = 2 * a2 + 6 * a3 * t + 12 * a4 * t2 + 20 * a5 * t3;

                    trajectory.Add(new PVTTrajectoryPoint
                    {
                        Position = pos,
                        Velocity = vel,
                        Acceleration = acc,
                        Time = currentTime + t
                    });
                }

                currentTime += dt;
            }

            return trajectory;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 验证PVT点
        /// </summary>
        private void ValidatePoints(List<PVTPoint> points, int axisIndex)
        {
            double maxVel = _maxVelocities[axisIndex];
            double maxAcc = _maxAccelerations[axisIndex];

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];

                // 检查速度限制
                if (Math.Abs(p.Velocity) > maxVel)
                {
                    throw new MotionException($"点{i}的速度{p.Velocity}超过最大速度{maxVel}", 0, axisIndex);
                }

                // 检查时间
                if (p.TimeMs <= 0)
                {
                    throw new MotionException($"点{i}的时间必须大于0", 0, axisIndex);
                }

                // 检查加速度（如果有前一个点）
                if (i > 0)
                {
                    var prev = points[i - 1];
                    double dt = p.TimeMs * (TimeUnit == TimeUnit.Millisecond ? 0.001 : 1.0);
                    double dv = p.Velocity - prev.Velocity;
                    double acc = Math.Abs(dv / dt);

                    if (acc > maxAcc)
                    {
                        throw new MotionException($"点{i}的加速度{acc}超过最大加速度{maxAcc}", 0, axisIndex);
                    }
                }
            }
        }

        /// <summary>
        /// 计算三次样条系数
        /// </summary>
        private List<CubicCoefficients> ComputeCubicSpline(double[] t, double[] p, double[] v)
        {
            int n = t.Length;
            var coefs = new List<CubicCoefficients>();

            for (int i = 0; i < n - 1; i++)
            {
                double dt = t[i + 1] - t[i];
                double dp = p[i + 1] - p[i];

                // 使用端点速度和位置确定三次曲线
                double a = p[i];
                double b = v[i];
                double c = (3 * dp / dt - 2 * v[i] - v[i + 1]) / dt;
                double d = (-2 * dp / dt + v[i] + v[i + 1]) / (dt * dt);

                coefs.Add(new CubicCoefficients { A = a, B = b, C = c, D = d });
            }

            return coefs;
        }

        #endregion
    }

    /// <summary>
    /// PVT插值方法
    /// </summary>
    public enum PVTInterpolationMethod
    {
        /// <summary>
        /// 线性插值
        /// </summary>
        Linear,

        /// <summary>
        /// 三次样条插值
        /// </summary>
        CubicSpline,

        /// <summary>
        /// 三次Hermite插值
        /// </summary>
        CubicHermite,

        /// <summary>
        /// 五次多项式插值
        /// </summary>
        FifthOrder
    }

    /// <summary>
    /// 时间单位
    /// </summary>
    public enum TimeUnit
    {
        Millisecond,
        Second
    }

    /// <summary>
    /// PVT轨迹点
    /// </summary>
    public struct PVTTrajectoryPoint
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
        /// 加速度
        /// </summary>
        public double Acceleration { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public double Time { get; set; }

        public override string ToString()
        {
            return $"T:{Time:F3}s P:{Position:F2} V:{Velocity:F2} A:{Acceleration:F2}";
        }
    }

    /// <summary>
    /// PVT规划结果
    /// </summary>
    public class PVTPlanResult
    {
        /// <summary>
        /// 轨迹点列表
        /// </summary>
        public List<PVTTrajectoryPoint> Trajectory { get; set; }

        /// <summary>
        /// 原始PVT点
        /// </summary>
        public List<PVTPoint> OriginalPoints { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// 轴索引
        /// </summary>
        public int AxisIndex { get; set; }

        /// <summary>
        /// 采样点数量
        /// </summary>
        public int SampleCount { get; set; }
    }

    /// <summary>
    /// 多轴PVT结果
    /// </summary>
    public class MultiAxisPVTResult
    {
        /// <summary>
        /// 各轴结果
        /// </summary>
        public PVTPlanResult[] AxisResults { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// 是否同步
        /// </summary>
        public bool IsSynchronized { get; set; }
    }

    /// <summary>
    /// 三次多项式系数
    /// </summary>
    internal struct CubicCoefficients
    {
        public double A;
        public double B;
        public double C;
        public double D;
    }
}
