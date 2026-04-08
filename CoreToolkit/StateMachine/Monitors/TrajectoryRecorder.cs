using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Motion.Core;
using CoreToolkit.StateMachine.Models;

namespace CoreToolkit.StateMachine.Monitors
{
    /// <summary>
    /// 轨迹记录器
    /// 记录轴运动过程中的位置、速度、状态信息
    /// </summary>
    public class TrajectoryRecorder
    {
        private readonly IMotionCard _motionCard;
        private readonly IAxisGroup _axisGroup;
        private Task _recordTask;
        private CancellationTokenSource _cts;
        private readonly Stopwatch _sw = new Stopwatch();

        /// <summary>
        /// 轨迹记录
        /// </summary>
        public TrajectoryRecord Record { get; private set; }
        
        /// <summary>
        /// 是否正在记录
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 构造函数（使用运动控制卡）
        /// </summary>
        /// <param name="motionCard">运动控制卡实例</param>
        public TrajectoryRecorder(IMotionCard motionCard)
        {
            _motionCard = motionCard;
        }

        /// <summary>
        /// 构造函数（使用轴组）
        /// </summary>
        /// <param name="axisGroup">轴组实例</param>
        public TrajectoryRecorder(IAxisGroup axisGroup)
        {
            _axisGroup = axisGroup;
        }

        /// <summary>
        /// 开始记录
        /// </summary>
        public void Start(int[] axisIndices, int sampleIntervalMs = 10, string recordName = null)
        {
            if (IsRecording) return;

            Record = new TrajectoryRecord
            {
                Name = recordName ?? $"Record_{DateTime.Now:yyyyMMdd_HHmmss}",
                AxisCount = axisIndices.Length,
                AxisNames = new string[axisIndices.Length],
                StartTime = DateTime.Now,
                SampleIntervalMs = sampleIntervalMs
            };

            // 设置轴名称
            for (int i = 0; i < axisIndices.Length; i++)
            {
                Record.AxisNames[i] = $"Axis{axisIndices[i]}";
            }

            IsRecording = true;
            _cts = new CancellationTokenSource();
            _sw.Restart();

            _recordTask = RecordLoop(axisIndices, sampleIntervalMs, _cts.Token);
        }

        /// <summary>
        /// 停止记录
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsRecording) return;

            _cts?.Cancel();

            if (_recordTask != null)
            {
                try
                {
                    await _recordTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception)
                {
                }
            }

            _sw.Stop();
            IsRecording = false;
        }

        /// <summary>
        /// 同步停止记录（兼容旧接口）
        /// </summary>
        public void Stop()
        {
            if (!IsRecording) return;

            _cts?.Cancel();
            _recordTask?.Wait(3000);

            if (_recordTask != null && !_recordTask.IsCompleted && !_recordTask.IsFaulted && !_recordTask.IsCanceled)
            {
                try
                {
                    _recordTask.GetAwaiter().GetResult();
                }
                catch
                {
                }
            }

            _sw.Stop();
            IsRecording = false;
        }

        /// <summary>
        /// 记录循环
        /// </summary>
        private async Task RecordLoop(int[] axisIndices, int sampleIntervalMs, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var point = new TrajectoryPoint
                    {
                        Timestamp = DateTime.Now,
                        ElapsedMs = _sw.ElapsedMilliseconds,
                        Positions = new double[axisIndices.Length],
                        Velocities = new double[axisIndices.Length],
                        Statuses = new int[axisIndices.Length]
                    };

                    if (_motionCard != null)
                    {
                        for (int i = 0; i < axisIndices.Length; i++)
                        {
                            var axis = axisIndices[i];
                            point.Positions[i] = _motionCard.GetPosition(axis);
                            point.Velocities[i] = 0;
                            point.Statuses[i] = _motionCard.IsInPosition(axis) ? 1 : 0;
                        }
                    }
                    else if (_axisGroup != null)
                    {
                        var groupStatus = _axisGroup.GetStatus();
                        for (int i = 0; i < axisIndices.Length && i < groupStatus.Positions.Length; i++)
                        {
                            point.Positions[i] = groupStatus.Positions[i];
                            point.Velocities[i] = groupStatus.Velocities[i];
                            point.Statuses[i] = groupStatus.IsMoving ? 1 : 0;
                        }
                    }

                    Record.Points.Add(point);

                    await Task.Delay(sampleIntervalMs, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// 获取简化后的轨迹点（降低数据密度）
        /// </summary>
        public TrajectoryPoint[] GetSimplifiedPoints(int targetCount = 1000)
        {
            if (Record?.Points == null || Record.Points.Count == 0)
                return new TrajectoryPoint[0];

            if (Record.Points.Count <= targetCount)
                return Record.Points.ToArray();

            var result = new List<TrajectoryPoint>();
            var step = (double)Record.Points.Count / targetCount;

            for (int i = 0; i < targetCount; i++)
            {
                int index = (int)(i * step);
                result.Add(Record.Points[index]);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 计算轨迹统计信息
        /// </summary>
        public TrajectoryStatistics CalculateStatistics()
        {
            if (Record?.Points == null || Record.Points.Count < 2)
                return null;

            var stats = new TrajectoryStatistics
            {
                DurationMs = Record.Points[Record.Points.Count - 1].ElapsedMs - Record.Points[0].ElapsedMs,
                AxisStatistics = new AxisTrajectoryStatistics[Record.AxisCount]
            };

            for (int axis = 0; axis < Record.AxisCount; axis++)
            {
                var axisStats = new AxisTrajectoryStatistics
                {
                    AxisIndex = axis,
                    AxisName = Record.AxisNames[axis]
                };

                double minPos = double.MaxValue, maxPos = double.MinValue;
                double maxVel = 0;
                double totalDistance = 0;
                double? lastPos = null;

                foreach (var point in Record.Points)
                {
                    var pos = point.Positions[axis];
                    var vel = point.Velocities[axis];

                    minPos = Math.Min(minPos, pos);
                    maxPos = Math.Max(maxPos, pos);
                    maxVel = Math.Max(maxVel, Math.Abs(vel));

                    if (lastPos.HasValue)
                    {
                        totalDistance += Math.Abs(pos - lastPos.Value);
                    }
                    lastPos = pos;
                }

                axisStats.MinPosition = minPos;
                axisStats.MaxPosition = maxPos;
                axisStats.TravelDistance = maxPos - minPos;
                axisStats.TotalDistance = totalDistance;
                axisStats.MaxVelocity = maxVel;

                axisStats.AverageVelocity = totalDistance / (stats.DurationMs / 1000.0);

                axisStats.StartPosition = Record.Points[0].Positions[axis];
                axisStats.EndPosition = Record.Points[Record.Points.Count - 1].Positions[axis];

                // axisStats assigned to parent already
            }

            return stats;
        }

        /// <summary>
        /// 导出为CSV格式
        /// </summary>
        public string ExportToCsv()
        {
            if (Record?.Points == null || Record.Points.Count == 0)
                return string.Empty;

            var lines = new List<string>();

            // 表头
            var headers = new List<string> { "Time(ms)" };
            for (int i = 0; i < Record.AxisCount; i++)
            {
                headers.Add($"{Record.AxisNames[i]}_Pos");
                headers.Add($"{Record.AxisNames[i]}_Vel");
                headers.Add($"{Record.AxisNames[i]}_Status");
            }
            lines.Add(string.Join(",", headers));

            // 数据
            foreach (var point in Record.Points)
            {
                var values = new List<string> { point.ElapsedMs.ToString("F2") };
                for (int i = 0; i < Record.AxisCount; i++)
                {
                    values.Add(point.Positions[i].ToString("F4"));
                    values.Add(point.Velocities[i].ToString("F4"));
                    values.Add(point.Statuses[i].ToString());
                }
                lines.Add(string.Join(",", values));
            }

            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// 轨迹统计信息
    /// </summary>
    public class TrajectoryStatistics
    {
        /// <summary>总时长(毫秒)</summary>
        public double DurationMs { get; set; }
        /// <summary>各轴统计</summary>
        public AxisTrajectoryStatistics[] AxisStatistics { get; set; }
    }

    /// <summary>
    /// 单轴轨迹统计
    /// </summary>
    public class AxisTrajectoryStatistics
    {
        /// <summary>轴索引</summary>
        public int AxisIndex { get; set; }
        /// <summary>轴名称</summary>
        public string AxisName { get; set; }
        /// <summary>起始位置</summary>
        public double StartPosition { get; set; }
        /// <summary>结束位置</summary>
        public double EndPosition { get; set; }
        /// <summary>最小位置</summary>
        public double MinPosition { get; set; }
        /// <summary>最大位置</summary>
        public double MaxPosition { get; set; }
        /// <summary>行程距离(终点-起点)</summary>
        public double TravelDistance { get; set; }
        /// <summary>总移动距离(含往复)</summary>
        public double TotalDistance { get; set; }
        /// <summary>最大速度</summary>
        public double MaxVelocity { get; set; }
        /// <summary>平均速度</summary>
        public double AverageVelocity { get; set; }
    }
}
