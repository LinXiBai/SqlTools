using CoreToolkit.Common.Models;
using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 安全运动控制器
    /// 包装 IMotionCard，在运动前执行多层安全检查
    /// </summary>
    public class SafeMotionController : IDisposable
    {
        private readonly IMotionCard _motionCard;
        private readonly ICollisionDetector _collisionDetector;
        private readonly SoftLimitGuard _softLimitGuard;
        private readonly InterlockEngine _interlockEngine;
        private bool _disposed;

        /// <summary>
        /// 安全事件
        /// </summary>
        public event EventHandler<MoveSafetyResult> SafetyViolation;

        /// <summary>
        /// 碰撞检测器
        /// </summary>
        public ICollisionDetector CollisionDetector => _collisionDetector;

        /// <summary>
        /// 软限位守卫
        /// </summary>
        public SoftLimitGuard SoftLimitGuard => _softLimitGuard;

        /// <summary>
        /// 互锁引擎
        /// </summary>
        public InterlockEngine InterlockEngine => _interlockEngine;

        /// <summary>
        /// 是否启用安全检查（调试用，可临时关闭）
        /// </summary>
        public bool SafetyEnabled { get; set; } = true;

        /// <summary>
        /// 底层运动卡
        /// </summary>
        public IMotionCard MotionCard => _motionCard;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SafeMotionController(IMotionCard motionCard,
            ICollisionDetector collisionDetector = null,
            SoftLimitGuard softLimitGuard = null,
            InterlockEngine interlockEngine = null)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _collisionDetector = collisionDetector ?? new CollisionDetector();
            _softLimitGuard = softLimitGuard ?? new SoftLimitGuard();
            _interlockEngine = interlockEngine ?? new InterlockEngine();
        }

        /// <summary>
        /// 绝对位置安全运动
        /// </summary>
        public Result MoveAbsoluteSafe(int axis, double position, double speed)
        {
            var check = PreMoveCheck(axis, position);
            if (!check.IsAllowed)
            {
                SafetyViolation?.Invoke(this, check);
                return Result.Failure(check.BlockReason);
            }

            _motionCard.MoveAbsolute(axis, position, speed);
            return Result.Success();
        }

        /// <summary>
        /// 相对位置安全运动
        /// </summary>
        public Result MoveRelativeSafe(int axis, double distance, double speed)
        {
            double currentPos = _motionCard.GetPosition(axis);
            double targetPos = currentPos + distance;

            var check = PreMoveCheck(axis, targetPos);
            if (!check.IsAllowed)
            {
                SafetyViolation?.Invoke(this, check);
                return Result.Failure(check.BlockReason);
            }

            _motionCard.MoveRelative(axis, distance, speed);
            return Result.Success();
        }

        /// <summary>
        /// 多轴绝对位置安全运动
        /// </summary>
        public Result MoveMultiAxisAbsoluteSafe(int[] axes, double[] positions, double[] speeds)
        {
            if (axes == null || positions == null || axes.Length != positions.Length)
                return Result.Failure("轴与位置参数不匹配");

            var check = PreMoveCheck(axes, positions);
            if (!check.IsAllowed)
            {
                SafetyViolation?.Invoke(this, check);
                return Result.Failure(check.BlockReason);
            }

            for (int i = 0; i < axes.Length; i++)
            {
                _motionCard.MoveAbsolute(axes[i], positions[i], speeds?[i] ?? 10000);
            }
            return Result.Success();
        }

        /// <summary>
        /// 线性插补安全运动
        /// </summary>
        public Result LinearInterpolationSafe(int[] axes, double[] positions, double speed)
        {
            var check = PreMoveCheck(axes, positions);
            if (!check.IsAllowed)
            {
                SafetyViolation?.Invoke(this, check);
                return Result.Failure(check.BlockReason);
            }

            _motionCard.LinearInterpolation(axes, positions, speed);
            return Result.Success();
        }

        /// <summary>
        /// JOG安全运动（只检查方向）
        /// </summary>
        public Result JogSafe(int axis, int direction, double speed)
        {
            if (!SafetyEnabled)
            {
                _motionCard.Jog(axis, direction, speed);
                return Result.Success();
            }

            // JOG时检查当前是否已经在限位边缘
            double currentPos = _motionCard.GetPosition(axis);
            var limitCheck = _softLimitGuard.CheckPosition(axis, currentPos);
            if (!limitCheck.IsAllowed)
            {
                // 已经在限位外，禁止继续向该方向运动
                var status = _motionCard.GetAxisStatus(axis);
                bool atPositiveLimit = status.PositiveLimit;
                bool atNegativeLimit = status.NegativeLimit;

                if ((direction > 0 && atPositiveLimit) || (direction < 0 && atNegativeLimit))
                {
                    return Result.Failure($"轴{axis}已在限位位置，禁止向该方向JOG");
                }
            }

            _motionCard.Jog(axis, direction, speed);
            return Result.Success();
        }

        /// <summary>
        /// 急停所有轴
        /// </summary>
        public void EmergencyStop()
        {
            _motionCard.StopAll(true);
        }

        /// <summary>
        /// 急停指定轴
        /// </summary>
        public void EmergencyStop(int axis)
        {
            _motionCard.Stop(axis, true);
        }

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        public double GetPosition(int axis) => _motionCard.GetPosition(axis);

        /// <summary>
        /// 更新动态体积位置（通常在运动完成后或周期监控中调用）
        /// </summary>
        public void UpdateDynamicVolume(string volumeId, double x, double y = 0, double z = 0)
        {
            _collisionDetector?.UpdateAxisPosition(volumeId, x, y, z);
        }

        /// <summary>
        /// 批量更新动态体积位置
        /// </summary>
        public void UpdateDynamicVolumes(Dictionary<string, (double X, double Y, double Z)> positions)
        {
            if (_collisionDetector is CollisionDetector cd)
            {
                cd.UpdateAxisPositions(positions);
            }
            else
            {
                foreach (var kv in positions)
                {
                    _collisionDetector?.UpdateAxisPosition(kv.Key, kv.Value.X, kv.Value.Y, kv.Value.Z);
                }
            }
        }

        /// <summary>
        /// 执行完整碰撞检测
        /// </summary>
        public CollisionResult CheckCollision()
        {
            return _collisionDetector?.CheckCollision() ?? CollisionResult.Success();
        }

        /// <summary>
        /// 运动前安全检查（单轴）
        /// </summary>
        public MoveSafetyResult PreMoveCheck(int axis, double targetPosition)
        {
            if (!SafetyEnabled) return MoveSafetyResult.Allowed();

            // 1. 互锁规则检查
            var interlockResult = _interlockEngine.EvaluateBeforeMotion();
            if (!interlockResult.IsSafe)
            {
                return MoveSafetyResult.Blocked($"[互锁] {interlockResult.BlockReason}", interlockResult);
            }

            // 2. 软限位检查
            var limitResult = _softLimitGuard.CheckPosition(axis, targetPosition);
            if (!limitResult.IsAllowed)
            {
                return limitResult;
            }

            // 3. 碰撞预测检查
            if (_collisionDetector != null)
            {
                var volumes = _collisionDetector.GetAllVolumes()
                    .Where(v => v.Type == VolumeType.Dynamic && v.LinkedAxes != null && v.LinkedAxes.Contains(axis));

                foreach (var vol in volumes)
                {
                    // 获取其他轴当前位置
                    double x = targetPosition;
                    double y = 0, z = 0;

                    if (vol.LinkedAxes.Length > 1)
                    {
                        // 多轴联动体积，获取其他关联轴当前位置
                        y = _motionCard.GetPosition(vol.LinkedAxes[1]);
                        if (vol.LinkedAxes.Length > 2)
                            z = _motionCard.GetPosition(vol.LinkedAxes[2]);
                    }

                    var collision = _collisionDetector.PreviewCollision(vol.Id, x, y, z);
                    if (!collision.IsSafe)
                    {
                        return MoveSafetyResult.Blocked($"[碰撞预测] {collision.Message}", collision);
                    }
                }
            }

            return MoveSafetyResult.Allowed();
        }

        /// <summary>
        /// 运动前安全检查（多轴）
        /// </summary>
        public MoveSafetyResult PreMoveCheck(int[] axes, double[] targetPositions)
        {
            if (!SafetyEnabled) return MoveSafetyResult.Allowed();
            if (axes == null || targetPositions == null)
                return MoveSafetyResult.Allowed();

            // 1. 互锁检查
            var interlockResult = _interlockEngine.EvaluateBeforeMotion();
            if (!interlockResult.IsSafe)
            {
                return MoveSafetyResult.Blocked($"[互锁] {interlockResult.BlockReason}", interlockResult);
            }

            // 2. 软限位检查
            var limitResult = _softLimitGuard.CheckPositions(axes, targetPositions);
            if (!limitResult.IsAllowed)
            {
                return limitResult;
            }

            // 3. 碰撞预测（简化版：取第一个轴作为主位置更新）
            if (_collisionDetector != null && axes.Length > 0)
            {
                var volumes = _collisionDetector.GetAllVolumes()
                    .Where(v => v.Type == VolumeType.Dynamic);

                foreach (var vol in volumes)
                {
                    double x = 0, y = 0, z = 0;
                    bool hasX = false, hasY = false, hasZ = false;

                    for (int i = 0; i < axes.Length; i++)
                    {
                        if (vol.LinkedAxes != null && vol.LinkedAxes.Contains(axes[i]))
                        {
                            int idx = Array.IndexOf(vol.LinkedAxes, axes[i]);
                            if (idx == 0) { x = targetPositions[i]; hasX = true; }
                            else if (idx == 1) { y = targetPositions[i]; hasY = true; }
                            else if (idx == 2) { z = targetPositions[i]; hasZ = true; }
                        }
                    }

                    // 未在本次运动中更新的轴，取当前位置
                    if (!hasX && vol.LinkedAxes != null && vol.LinkedAxes.Length > 0)
                        x = _motionCard.GetPosition(vol.LinkedAxes[0]);
                    if (!hasY && vol.LinkedAxes != null && vol.LinkedAxes.Length > 1)
                        y = _motionCard.GetPosition(vol.LinkedAxes[1]);
                    if (!hasZ && vol.LinkedAxes != null && vol.LinkedAxes.Length > 2)
                        z = _motionCard.GetPosition(vol.LinkedAxes[2]);

                    var collision = _collisionDetector.PreviewCollision(vol.Id, x, y, z);
                    if (!collision.IsSafe)
                    {
                        return MoveSafetyResult.Blocked($"[碰撞预测] {collision.Message}", collision);
                    }
                }
            }

            return MoveSafetyResult.Allowed();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _motionCard?.Dispose();
                _disposed = true;
            }
        }
    }
}
