using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴组管理器
    /// 管理多个轴组的创建、销毁和查询
    /// </summary>
    public class AxisGroupManager
    {
        private readonly IMotionCard _motionCard;
        private readonly Dictionary<int, IAxisGroup> _groups;
        private readonly object _lockObj = new object();
        private int _nextGroupId = 1;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AxisGroupManager(IMotionCard motionCard)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _groups = new Dictionary<int, IAxisGroup>();
        }

        /// <summary>
        /// 创建轴组
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="axisIds">轴号数组</param>
        /// <returns>轴组实例</returns>
        public IAxisGroup CreateGroup(string groupName, int[] axisIds)
        {
            lock (_lockObj)
            {
                // 检查轴是否已被其他组使用
                foreach (var existingGroup in _groups.Values)
                {
                    foreach (int axis in axisIds)
                    {
                        if (existingGroup.AxisIds.Contains(axis))
                        {
                            throw new MotionException($"轴{axis}已被组'{existingGroup.GroupName}'使用", -1);
                        }
                    }
                }

                int groupId = _nextGroupId++;
                var group = new AxisGroup(groupId, groupName, axisIds, _motionCard);
                _groups[groupId] = group;

                return group;
            }
        }

        /// <summary>
        /// 创建标准2轴XY组
        /// </summary>
        public IAxisGroup CreateXYGroup(int xAxisId, int yAxisId, string groupName = "XY")
        {
            return CreateGroup(groupName, new[] { xAxisId, yAxisId });
        }

        /// <summary>
        /// 创建标准3轴XYZ组
        /// </summary>
        public IAxisGroup CreateXYZGroup(int xAxisId, int yAxisId, int zAxisId, string groupName = "XYZ")
        {
            return CreateGroup(groupName, new[] { xAxisId, yAxisId, zAxisId });
        }

        /// <summary>
        /// 创建标准4轴XYZR组
        /// </summary>
        public IAxisGroup CreateXYZRGroup(int xAxisId, int yAxisId, int zAxisId, int rAxisId, string groupName = "XYZR")
        {
            return CreateGroup(groupName, new[] { xAxisId, yAxisId, zAxisId, rAxisId });
        }

        /// <summary>
        /// 删除轴组
        /// </summary>
        public void DeleteGroup(int groupId)
        {
            lock (_lockObj)
            {
                if (_groups.TryGetValue(groupId, out var group))
                {
                    group.Disable();
                    _groups.Remove(groupId);
                }
            }
        }

        /// <summary>
        /// 获取轴组
        /// </summary>
        public IAxisGroup GetGroup(int groupId)
        {
            lock (_lockObj)
            {
                _groups.TryGetValue(groupId, out var group);
                return group;
            }
        }

        /// <summary>
        /// 根据名称获取轴组
        /// </summary>
        public IAxisGroup GetGroupByName(string groupName)
        {
            lock (_lockObj)
            {
                return _groups.Values.FirstOrDefault(g => g.GroupName == groupName);
            }
        }

        /// <summary>
        /// 获取所有轴组
        /// </summary>
        public IEnumerable<IAxisGroup> GetAllGroups()
        {
            lock (_lockObj)
            {
                return _groups.Values.ToList();
            }
        }

        /// <summary>
        /// 获取包含指定轴的所有组
        /// </summary>
        public IEnumerable<IAxisGroup> GetGroupsByAxis(int axisId)
        {
            lock (_lockObj)
            {
                return _groups.Values.Where(g => g.AxisIds.Contains(axisId)).ToList();
            }
        }

        /// <summary>
        /// 删除所有轴组
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                foreach (var group in _groups.Values)
                {
                    group.Disable();
                }
                _groups.Clear();
            }
        }

        /// <summary>
        /// 获取轴组数量
        /// </summary>
        public int Count => _groups.Count;

        /// <summary>
        /// 检查轴是否已被使用
        /// </summary>
        public bool IsAxisUsed(int axisId)
        {
            lock (_lockObj)
            {
                return _groups.Values.Any(g => g.AxisIds.Contains(axisId));
            }
        }

        /// <summary>
        /// 获取所有被使用的轴
        /// </summary>
        public int[] GetUsedAxes()
        {
            lock (_lockObj)
            {
                var axes = new HashSet<int>();
                foreach (var group in _groups.Values)
                {
                    foreach (int axis in group.AxisIds)
                    {
                        axes.Add(axis);
                    }
                }
                return axes.ToArray();
            }
        }
    }
}
