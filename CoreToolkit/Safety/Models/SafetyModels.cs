using System;
using System.Collections.Generic;

namespace CoreToolkit.Safety.Models
{
    /// <summary>
    /// 体积类型
    /// </summary>
    public enum VolumeType
    {
        /// <summary>
        /// 运动部件（位置会随轴变化）
        /// </summary>
        Dynamic,

        /// <summary>
        /// 静止障碍（机架、治具、料架等）
        /// </summary>
        Static,

        /// <summary>
        /// 临时禁区（如维护区域、安全门区域）
        /// </summary>
        Temporary
    }

    /// <summary>
    /// 轴对齐包围盒（AABB）
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// X轴最小值
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// X轴最大值
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Y轴最小值
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Y轴最大值
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Z轴最小值
        /// </summary>
        public double MinZ { get; set; }

        /// <summary>
        /// Z轴最大值
        /// </summary>
        public double MaxZ { get; set; }

        /// <summary>
        /// 中心点X坐标
        /// </summary>
        public double CenterX => (MinX + MaxX) / 2.0;

        /// <summary>
        /// 中心点Y坐标
        /// </summary>
        public double CenterY => (MinY + MaxY) / 2.0;

        /// <summary>
        /// 中心点Z坐标
        /// </summary>
        public double CenterZ => (MinZ + MaxZ) / 2.0;

        /// <summary>
        /// 体积尺寸（X方向）
        /// </summary>
        public double SizeX => MaxX - MinX;

        /// <summary>
        /// 体积尺寸（Y方向）
        /// </summary>
        public double SizeY => MaxY - MinY;

        /// <summary>
        /// 体积尺寸（Z方向）
        /// </summary>
        public double SizeZ => MaxZ - MinZ;

        /// <summary>
        /// 判断是否与其他包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox other)
        {
            if (other == null) return false;
            return !(MaxX < other.MinX || MinX > other.MaxX ||
                     MaxY < other.MinY || MinY > other.MaxY ||
                     MaxZ < other.MinZ || MinZ > other.MaxZ);
        }

        /// <summary>
        /// 计算与另一个包围盒的最小间距
        /// </summary>
        public double GetMinimumDistance(BoundingBox other)
        {
            if (other == null) return double.MaxValue;

            double dx = Math.Max(0, Math.Max(other.MinX - MaxX, MinX - other.MaxX));
            double dy = Math.Max(0, Math.Max(other.MinY - MaxY, MinY - other.MaxY));
            double dz = Math.Max(0, Math.Max(other.MinZ - MaxZ, MinZ - other.MaxZ));

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 按给定偏移量平移包围盒
        /// </summary>
        public BoundingBox Translate(double offsetX, double offsetY, double offsetZ)
        {
            return new BoundingBox
            {
                MinX = MinX + offsetX,
                MaxX = MaxX + offsetX,
                MinY = MinY + offsetY,
                MaxY = MaxY + offsetY,
                MinZ = MinZ + offsetZ,
                MaxZ = MaxZ + offsetZ
            };
        }

        /// <summary>
        /// 扩大包围盒（增加安全余量）
        /// </summary>
        public BoundingBox Inflate(double margin)
        {
            return new BoundingBox
            {
                MinX = MinX - margin,
                MaxX = MaxX + margin,
                MinY = MinY - margin,
                MaxY = MaxY + margin,
                MinZ = MinZ - margin,
                MaxZ = MaxZ + margin
            };
        }

        /// <summary>
        /// 判断是否包含某点
        /// </summary>
        public bool Contains(double x, double y, double z)
        {
            return x >= MinX && x <= MaxX &&
                   y >= MinY && y <= MaxY &&
                   z >= MinZ && z <= MaxZ;
        }

        /// <summary>
        /// 创建复制
        /// </summary>
        public BoundingBox Clone()
        {
            return new BoundingBox
            {
                MinX = MinX,
                MaxX = MaxX,
                MinY = MinY,
                MaxY = MaxY,
                MinZ = MinZ,
                MaxZ = MaxZ
            };
        }

        public override string ToString()
        {
            return $"AABB[({MinX:F2},{MinY:F2},{MinZ:F2}) ~ ({MaxX:F2},{MaxY:F2},{MaxZ:F2})]";
        }
    }

    /// <summary>
    /// 安全体积定义
    /// 表示设备中一个需要保护的空间实体
    /// </summary>
    public class SafetyVolume
    {
        /// <summary>
        /// 体积唯一标识
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// 体积名称（如"吸嘴A"、"基板夹具"）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 体积类型
        /// </summary>
        public VolumeType Type { get; set; }

        /// <summary>
        /// 基础包围盒（局部坐标系下）
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// 安全余量（mm）
        /// </summary>
        public double SafetyMargin { get; set; } = 2.0;

        /// <summary>
        /// 是否激活检测
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 关联的轴索引（Dynamic类型使用）
        /// </summary>
        public int[] LinkedAxes { get; set; }

        /// <summary>
        /// 轴偏移量（Dynamic类型使用，表示包围盒中心相对于轴位置的偏移）
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Y轴偏移量
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// Z轴偏移量
        /// </summary>
        public double OffsetZ { get; set; }

        /// <summary>
        /// 获取带安全余量的包围盒
        /// </summary>
        public BoundingBox GetInflatedBox()
        {
            return BoundingBox?.Inflate(SafetyMargin);
        }

        /// <summary>
        /// 获取在指定轴位置下的世界坐标包围盒
        /// </summary>
        public BoundingBox GetWorldBox(double axisX, double axisY = 0, double axisZ = 0)
        {
            var box = BoundingBox?.Clone();
            if (box == null) return null;

            double worldX = axisX + OffsetX;
            double worldY = axisY + OffsetY;
            double worldZ = axisZ + OffsetZ;

            return box.Translate(worldX, worldY, worldZ);
        }

        public override string ToString()
        {
            return $"{Name}[{Type}] {BoundingBox} Margin={SafetyMargin}mm";
        }
    }

    /// <summary>
    /// 碰撞检测结果
    /// </summary>
    public class CollisionResult
    {
        /// <summary>
        /// 是否安全（无碰撞）
        /// </summary>
        public bool IsSafe { get; set; }

        /// <summary>
        /// 碰撞的体积A
        /// </summary>
        public string VolumeA { get; set; }

        /// <summary>
        /// 碰撞的体积B
        /// </summary>
        public string VolumeB { get; set; }

        /// <summary>
        /// 碰撞位置（估算）
        /// </summary>
        public (double X, double Y, double Z)? CollisionPoint { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 所有碰撞对（多对碰撞时）
        /// </summary>
        public List<(string A, string B)> AllCollisions { get; set; } = new List<(string, string)>();

        public static CollisionResult Success()
        {
            return new CollisionResult { IsSafe = true };
        }

        public static CollisionResult Failure(string volumeA, string volumeB, string message = null)
        {
            return new CollisionResult
            {
                IsSafe = false,
                VolumeA = volumeA,
                VolumeB = volumeB,
                Message = message ?? $"碰撞风险: {volumeA} ↔ {volumeB}"
            };
        }
    }

    /// <summary>
    /// 软限位配置
    /// </summary>
    public class SoftLimitConfig
    {
        /// <summary>
        /// 轴索引
        /// </summary>
        public int AxisIndex { get; set; }

        /// <summary>
        /// 正向软限位
        /// </summary>
        public double PositiveLimit { get; set; } = double.MaxValue;

        /// <summary>
        /// 负向软限位
        /// </summary>
        public double NegativeLimit { get; set; } = double.MinValue;

        /// <summary>
        /// 是否启用软限位
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 校验目标位置是否在软限位范围内
        /// </summary>
        public bool IsInRange(double position)
        {
            if (!Enabled) return true;
            return position >= NegativeLimit && position <= PositiveLimit;
        }
    }

    /// <summary>
    /// 互锁规则动作类型
    /// </summary>
    public enum InterlockAction
    {
        /// <summary>
        /// 禁止运动
        /// </summary>
        BlockMotion,

        /// <summary>
        /// 立即停止
        /// </summary>
        EmergencyStop,

        /// <summary>
        /// 仅报警
        /// </summary>
        AlarmOnly,

        /// <summary>
        /// 减速停止
        /// </summary>
        DecelerateStop
    }

    /// <summary>
    /// 互锁规则
    /// </summary>
    public class InterlockRule
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// 规则名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 规则描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 条件表达式（如"Z.Position > 40 &amp;&amp; !Vacuum.On"）
        /// 实际使用时通过委托判断
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// 条件判断委托
        /// </summary>
        public Func<bool> Condition { get; set; }

        /// <summary>
        /// 触发动作
        /// </summary>
        public InterlockAction Action { get; set; } = InterlockAction.BlockMotion;

        /// <summary>
        /// 触发时消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 评估规则
        /// </summary>
        public bool Evaluate()
        {
            if (!Enabled || Condition == null) return false;
            return Condition();
        }
    }

    /// <summary>
    /// 安全配置（用于JSON序列化）
    /// </summary>
    public class SafetyConfig
    {
        /// <summary>
        /// 软限位配置列表
        /// </summary>
        public List<SoftLimitConfig> SoftLimits { get; set; } = new List<SoftLimitConfig>();

        /// <summary>
        /// 安全体积定义列表
        /// </summary>
        public List<SafetyVolume> SafetyVolumes { get; set; } = new List<SafetyVolume>();

        /// <summary>
        /// 互锁规则列表
        /// </summary>
        public List<InterlockRule> InterlockRules { get; set; } = new List<InterlockRule>();

        /// <summary>
        /// 安全监控周期（毫秒）
        /// </summary>
        public int MonitorIntervalMs { get; set; } = 100;

        /// <summary>
        /// 是否启用后台碰撞检测
        /// </summary>
        public bool EnableBackgroundMonitor { get; set; } = true;

        /// <summary>
        /// 双头最小安全间距（mm）
        /// </summary>
        public double DualHeadMinSeparation { get; set; } = 50.0;

        /// <summary>
        /// Z轴最大安全下压深度
        /// </summary>
        public double ZAxisMaxSafeDepth { get; set; } = 50.0;
    }

    /// <summary>
    /// 运动安全检查结果
    /// </summary>
    public class MoveSafetyResult
    {
        /// <summary>
        /// 是否允许运动
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// 阻止原因
        /// </summary>
        public string BlockReason { get; set; }

        /// <summary>
        /// 触发规则或检测结果
        /// </summary>
        public object TriggerSource { get; set; }

        public static MoveSafetyResult Allowed()
        {
            return new MoveSafetyResult { IsAllowed = true };
        }

        public static MoveSafetyResult Blocked(string reason, object source = null)
        {
            return new MoveSafetyResult
            {
                IsAllowed = false,
                BlockReason = reason,
                TriggerSource = source
            };
        }
    }
}
