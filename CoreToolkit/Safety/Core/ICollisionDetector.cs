using CoreToolkit.Safety.Models;
using System.Collections.Generic;

namespace CoreToolkit.Safety.Core
{
    /// <summary>
    /// 碰撞检测器接口
    /// </summary>
    public interface ICollisionDetector
    {
        /// <summary>
        /// 注册安全体积
        /// </summary>
        void RegisterVolume(SafetyVolume volume);

        /// <summary>
        /// 移除安全体积
        /// </summary>
        bool RemoveVolume(string volumeId);

        /// <summary>
        /// 获取所有已注册体积
        /// </summary>
        IEnumerable<SafetyVolume> GetAllVolumes();

        /// <summary>
        /// 更新动态体积的轴位置
        /// </summary>
        void UpdateAxisPosition(string volumeId, double x, double y = 0, double z = 0);

        /// <summary>
        /// 执行碰撞检测
        /// </summary>
        CollisionResult CheckCollision();

        /// <summary>
        /// 预览：假设某轴移动到目标位置后的碰撞状态
        /// </summary>
        CollisionResult PreviewCollision(string volumeId, double targetX, double targetY = 0, double targetZ = 0);

        /// <summary>
        /// 清除所有体积
        /// </summary>
        void Clear();
    }
}
