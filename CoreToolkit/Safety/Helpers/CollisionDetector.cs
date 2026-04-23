using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 碰撞检测引擎
    /// 基于AABB包围盒的碰撞检测
    /// </summary>
    public class CollisionDetector : ICollisionDetector
    {
        private readonly Dictionary<string, SafetyVolume> _volumes = new Dictionary<string, SafetyVolume>();
        private readonly Dictionary<string, (double X, double Y, double Z)> _dynamicPositions = new Dictionary<string, (double, double, double)>();
        private readonly object _lock = new object();

        /// <summary>
        /// 注册安全体积
        /// </summary>
        public void RegisterVolume(SafetyVolume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            if (string.IsNullOrEmpty(volume.Id)) volume.Id = Guid.NewGuid().ToString("N").Substring(0, 8);

            lock (_lock)
            {
                _volumes[volume.Id] = volume;
                if (volume.Type == VolumeType.Dynamic)
                {
                    _dynamicPositions[volume.Id] = (0, 0, 0);
                }
            }
        }

        /// <summary>
        /// 移除安全体积
        /// </summary>
        public bool RemoveVolume(string volumeId)
        {
            lock (_lock)
            {
                _dynamicPositions.Remove(volumeId);
                return _volumes.Remove(volumeId);
            }
        }

        /// <summary>
        /// 获取所有已注册体积
        /// </summary>
        public IEnumerable<SafetyVolume> GetAllVolumes()
        {
            lock (_lock)
            {
                return _volumes.Values.ToList();
            }
        }

        /// <summary>
        /// 更新动态体积的轴位置
        /// </summary>
        public void UpdateAxisPosition(string volumeId, double x, double y = 0, double z = 0)
        {
            lock (_lock)
            {
                if (_volumes.ContainsKey(volumeId) && _volumes[volumeId].Type == VolumeType.Dynamic)
                {
                    _dynamicPositions[volumeId] = (x, y, z);
                }
            }
        }

        /// <summary>
        /// 批量更新多个轴位置
        /// </summary>
        public void UpdateAxisPositions(Dictionary<string, (double X, double Y, double Z)> positions)
        {
            lock (_lock)
            {
                foreach (var kv in positions)
                {
                    if (_dynamicPositions.ContainsKey(kv.Key))
                    {
                        _dynamicPositions[kv.Key] = kv.Value;
                    }
                }
            }
        }

        /// <summary>
        /// 执行碰撞检测
        /// </summary>
        public CollisionResult CheckCollision()
        {
            lock (_lock)
            {
                var activeVolumes = _volumes.Values.Where(v => v.IsActive).ToList();
                var worldBoxes = new Dictionary<string, BoundingBox>();

                // 计算所有体积的世界坐标包围盒
                foreach (var vol in activeVolumes)
                {
                    worldBoxes[vol.Id] = GetWorldBoundingBox(vol);
                }

                var result = CollisionResult.Success();

                // 两两检测（O(N^2)，N较小时可用；N大时建议改用空间分割）
                for (int i = 0; i < activeVolumes.Count; i++)
                {
                    for (int j = i + 1; j < activeVolumes.Count; j++)
                    {
                        var a = activeVolumes[i];
                        var b = activeVolumes[j];

                        // 静止物体之间无需检测
                        if (a.Type == VolumeType.Static && b.Type == VolumeType.Static)
                            continue;

                        var boxA = worldBoxes[a.Id];
                        var boxB = worldBoxes[b.Id];

                        if (boxA != null && boxB != null && boxA.Intersects(boxB))
                        {
                            result.IsSafe = false;
                            result.AllCollisions.Add((a.Name, b.Name));

                            // 记录第一对碰撞
                            if (result.VolumeA == null)
                            {
                                result.VolumeA = a.Name;
                                result.VolumeB = b.Name;
                                result.Message = $"碰撞风险: {a.Name} ↔ {b.Name}";
                                result.CollisionPoint = EstimateCollisionPoint(boxA, boxB);
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 预览：假设某轴移动到目标位置后的碰撞状态
        /// </summary>
        public CollisionResult PreviewCollision(string volumeId, double targetX, double targetY = 0, double targetZ = 0)
        {
            lock (_lock)
            {
                if (!_volumes.TryGetValue(volumeId, out var volume) || !volume.IsActive)
                    return CollisionResult.Success();

                var previewBox = volume.GetWorldBox(targetX, targetY, targetZ)?.Inflate(volume.SafetyMargin);
                if (previewBox == null) return CollisionResult.Success();

                var result = CollisionResult.Success();

                foreach (var other in _volumes.Values.Where(v => v.IsActive && v.Id != volumeId))
                {
                    // 静止对静止不检测
                    if (volume.Type == VolumeType.Static && other.Type == VolumeType.Static)
                        continue;

                    var otherBox = GetWorldBoundingBox(other);
                    if (otherBox != null && previewBox.Intersects(otherBox))
                    {
                        result.IsSafe = false;
                        result.AllCollisions.Add((volume.Name, other.Name));

                        if (result.VolumeA == null)
                        {
                            result.VolumeA = volume.Name;
                            result.VolumeB = other.Name;
                            result.Message = $"预览碰撞: {volume.Name} 在 ({targetX:F2},{targetY:F2},{targetZ:F2}) 将与 {other.Name} 碰撞";
                            result.CollisionPoint = EstimateCollisionPoint(previewBox, otherBox);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 检测某点是否在任何安全体积内（用于禁区检测）
        /// </summary>
        public bool IsPointInAnyVolume(double x, double y, double z, VolumeType? typeFilter = null)
        {
            lock (_lock)
            {
                foreach (var vol in _volumes.Values.Where(v => v.IsActive))
                {
                    if (typeFilter.HasValue && vol.Type != typeFilter.Value)
                        continue;

                    var box = GetWorldBoundingBox(vol);
                    if (box != null && box.Contains(x, y, z))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取两个体积之间的最小距离
        /// </summary>
        public double GetMinimumDistance(string volumeIdA, string volumeIdB)
        {
            lock (_lock)
            {
                if (!_volumes.TryGetValue(volumeIdA, out var volA) || !_volumes.TryGetValue(volumeIdB, out var volB))
                    return double.MaxValue;

                var boxA = GetWorldBoundingBox(volA);
                var boxB = GetWorldBoundingBox(volB);

                if (boxA == null || boxB == null) return double.MaxValue;
                return boxA.GetMinimumDistance(boxB);
            }
        }

        /// <summary>
        /// 清除所有体积
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _volumes.Clear();
                _dynamicPositions.Clear();
            }
        }

        /// <summary>
        /// 获取体积的世界坐标包围盒
        /// </summary>
        private BoundingBox GetWorldBoundingBox(SafetyVolume volume)
        {
            if (volume.BoundingBox == null) return null;

            if (volume.Type == VolumeType.Dynamic && _dynamicPositions.TryGetValue(volume.Id, out var pos))
            {
                return volume.GetWorldBox(pos.X, pos.Y, pos.Z)?.Inflate(volume.SafetyMargin);
            }
            else if (volume.Type == VolumeType.Dynamic)
            {
                return volume.GetWorldBox(0, 0, 0)?.Inflate(volume.SafetyMargin);
            }
            else
            {
                return volume.GetInflatedBox();
            }
        }

        /// <summary>
        /// 估算碰撞点（两盒相交中心）
        /// </summary>
        private (double X, double Y, double Z)? EstimateCollisionPoint(BoundingBox a, BoundingBox b)
        {
            double cx = (Math.Max(a.MinX, b.MinX) + Math.Min(a.MaxX, b.MaxX)) / 2.0;
            double cy = (Math.Max(a.MinY, b.MinY) + Math.Min(a.MaxY, b.MaxY)) / 2.0;
            double cz = (Math.Max(a.MinZ, b.MinZ) + Math.Min(a.MaxZ, b.MaxZ)) / 2.0;
            return (cx, cy, cz);
        }
    }
}
