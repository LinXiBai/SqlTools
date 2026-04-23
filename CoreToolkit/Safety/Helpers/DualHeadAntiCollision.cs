using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Models;
using System;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 双头防碰撞管理器
    /// 针对同一横梁上的双贴装头/固晶头的防碰撞保护
    /// </summary>
    public class DualHeadAntiCollision
    {
        private readonly IMotionCard _motionCard;
        private readonly int _headAAxis;
        private readonly int _headBAxis;

        /// <summary>
        /// 双头最小安全间距（mm）
        /// </summary>
        public double MinSeparation { get; set; } = 50.0;

        /// <summary>
        /// 头A当前位置
        /// </summary>
        public double HeadAPosition => _motionCard.GetPosition(_headAAxis);

        /// <summary>
        /// 头B当前位置
        /// </summary>
        public double HeadBPosition => _motionCard.GetPosition(_headBAxis);

        /// <summary>
        /// 当前双头间距
        /// </summary>
        public double CurrentSeparation => Math.Abs(HeadBPosition - HeadAPosition);

        /// <summary>
        /// 是否启用防碰撞
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 禁止交叉（头A必须在头B左侧）
        /// </summary>
        public bool PreventCrossing { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DualHeadAntiCollision(IMotionCard motionCard, int headAAxis, int headBAxis)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _headAAxis = headAAxis;
            _headBAxis = headBAxis;
        }

        /// <summary>
        /// 检查头A是否可以移动到目标位置
        /// </summary>
        public MoveSafetyResult CanMoveHeadA(double targetPosition)
        {
            if (!Enabled) return MoveSafetyResult.Allowed();

            double headBPos = HeadBPosition;

            if (PreventCrossing && targetPosition >= headBPos - MinSeparation)
            {
                return MoveSafetyResult.Blocked(
                    $"头A目标位置 {targetPosition:F2} 将越过安全间距（头B在 {headBPos:F2}，最小间距 {MinSeparation:F2}）");
            }

            if (!PreventCrossing && Math.Abs(targetPosition - headBPos) < MinSeparation)
            {
                return MoveSafetyResult.Blocked(
                    $"头A与头B间距将小于安全值 {MinSeparation:F2}");
            }

            return MoveSafetyResult.Allowed();
        }

        /// <summary>
        /// 检查头B是否可以移动到目标位置
        /// </summary>
        public MoveSafetyResult CanMoveHeadB(double targetPosition)
        {
            if (!Enabled) return MoveSafetyResult.Allowed();

            double headAPos = HeadAPosition;

            if (PreventCrossing && targetPosition <= headAPos + MinSeparation)
            {
                return MoveSafetyResult.Blocked(
                    $"头B目标位置 {targetPosition:F2} 将越过安全间距（头A在 {headAPos:F2}，最小间距 {MinSeparation:F2}）");
            }

            if (!PreventCrossing && Math.Abs(targetPosition - headAPos) < MinSeparation)
            {
                return MoveSafetyResult.Blocked(
                    $"头B与头A间距将小于安全值 {MinSeparation:F2}");
            }

            return MoveSafetyResult.Allowed();
        }

        /// <summary>
        /// 安全移动头A
        /// </summary>
        public bool MoveHeadASafe(double targetPosition, double speed)
        {
            var check = CanMoveHeadA(targetPosition);
            if (!check.IsAllowed)
                throw new InvalidOperationException(check.BlockReason);

            _motionCard.MoveAbsolute(_headAAxis, targetPosition, speed);
            return true;
        }

        /// <summary>
        /// 安全移动头B
        /// </summary>
        public bool MoveHeadBSafe(double targetPosition, double speed)
        {
            var check = CanMoveHeadB(targetPosition);
            if (!check.IsAllowed)
                throw new InvalidOperationException(check.BlockReason);

            _motionCard.MoveAbsolute(_headBAxis, targetPosition, speed);
            return true;
        }

        /// <summary>
        /// 同步安全移动双头（保持距离）
        /// </summary>
        public bool MoveBothSafe(double headATarget, double headBTarget, double speed)
        {
            // 检查最终位置
            if (PreventCrossing && headATarget >= headBTarget - MinSeparation)
            {
                throw new InvalidOperationException(
                    $"双头目标位置交叉：头A={headATarget:F2}，头B={headBTarget:F2}，最小间距={MinSeparation:F2}");
            }

            if (Math.Abs(headATarget - headBTarget) < MinSeparation)
            {
                throw new InvalidOperationException(
                    $"双头目标间距 {Math.Abs(headATarget - headBTarget):F2} 小于安全值 {MinSeparation:F2}");
            }

            // 检查运动过程中是否会交叉（简单线性估算）
            double currentA = HeadAPosition;
            double currentB = HeadBPosition;

            if (PreventCrossing)
            {
                // 如果头A向右、头B向左，检查是否会经过危险区域
                bool aMovingRight = headATarget > currentA;
                bool bMovingLeft = headBTarget < currentB;

                if (aMovingRight && bMovingLeft)
                {
                    // 计算是否会交叉（简化：假设同时到达）
                    double midA = (currentA + headATarget) / 2;
                    double midB = (currentB + headBTarget) / 2;
                    if (midA >= midB - MinSeparation)
                    {
                        throw new InvalidOperationException(
                            "双头运动轨迹存在交叉风险，请分步移动");
                    }
                }
            }

            _motionCard.MoveAbsolute(_headAAxis, headATarget, speed);
            _motionCard.MoveAbsolute(_headBAxis, headBTarget, speed);
            return true;
        }

        /// <summary>
        /// 获取安全移动建议（为头A计算最大可移动范围）
        /// </summary>
        public (double MinSafe, double MaxSafe) GetSafeRangeForHeadA()
        {
            double headBPos = HeadBPosition;

            if (PreventCrossing)
            {
                return (double.MinValue, headBPos - MinSeparation);
            }
            else
            {
                return (headBPos - MinSeparation, headBPos + MinSeparation);
            }
        }

        /// <summary>
        /// 获取安全移动建议（为头B计算最大可移动范围）
        /// </summary>
        public (double MinSafe, double MaxSafe) GetSafeRangeForHeadB()
        {
            double headAPos = HeadAPosition;

            if (PreventCrossing)
            {
                return (headAPos + MinSeparation, double.MaxValue);
            }
            else
            {
                return (headAPos - MinSeparation, headAPos + MinSeparation);
            }
        }
    }
}
