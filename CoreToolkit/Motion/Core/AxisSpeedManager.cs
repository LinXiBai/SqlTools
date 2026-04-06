using System;
using System.Collections.Generic;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴速度管理器
    /// 负责根据速度模式计算实际速度参数，并进行安全验证
    /// </summary>
    public class AxisSpeedManager
    {
        // 存储各轴的高速基准参数（从数据库读取的原始参数）
        private readonly Dictionary<int, AxisSpeedProfile> _baseProfiles;
        
        // 存储各轴当前应用的速度参数
        private readonly Dictionary<int, AxisSpeedProfile> _currentProfiles;
        
        // 速度变化时触发的事件
        public event EventHandler<SpeedChangedEventArgs> SpeedChanged;

        /// <summary>
        /// 加速度与速度的最大允许比率（防止飞车）
        /// 加速度不应超过速度的该倍数
        /// </summary>
        public double MaxAccToVelRatio { get; set; } = 2.0;

        /// <summary>
        /// 最小加速度与速度比率
        /// </summary>
        public double MinAccToVelRatio { get; set; } = 0.1;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AxisSpeedManager()
        {
            _baseProfiles = new Dictionary<int, AxisSpeedProfile>();
            _currentProfiles = new Dictionary<int, AxisSpeedProfile>();
        }

        /// <summary>
        /// 注册轴的高速基准参数（从数据库读取）
        /// </summary>
        /// <param name="profile">高速模式下的基准参数</param>
        public void RegisterBaseProfile(AxisSpeedProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            // 存储基准参数（高速模式）
            _baseProfiles[profile.AxisId] = profile.Clone();
            
            // 初始化当前参数为高速模式
            _currentProfiles[profile.AxisId] = profile.Clone();
        }

        /// <summary>
        /// 从数据库参数注册轴
        /// </summary>
        public void RegisterAxisFromDatabase(
            int axisId,
            double lowSpeed,
            double highSpeed,
            double acc,
            double dec,
            double jerkAcc = 0,
            double jerkDec = 0)
        {
            var profile = AxisSpeedProfile.FromDatabase(axisId, lowSpeed, highSpeed, acc, dec, jerkAcc, jerkDec);
            RegisterBaseProfile(profile);
        }

        /// <summary>
        /// 设置轴的速度模式
        /// </summary>
        /// <param name="axisId">轴号</param>
        /// <param name="mode">速度模式</param>
        /// <returns>计算后的速度参数</returns>
        public AxisSpeedProfile SetSpeedMode(int axisId, SpeedMode mode)
        {
            if (!_baseProfiles.ContainsKey(axisId))
                throw new InvalidOperationException(string.Format("轴{0}未注册基准参数", axisId));

            var baseProfile = _baseProfiles[axisId];
            var scaleFactor = mode.GetScaleFactor();

            // 根据模式计算新参数
            var newProfile = new AxisSpeedProfile
            {
                AxisId = axisId,
                CurrentMode = mode,
                // 速度和加速度按比例缩放
                StartVelocity = baseProfile.StartVelocity * scaleFactor,
                MaxVelocity = baseProfile.MaxVelocity * scaleFactor,
                Acceleration = baseProfile.Acceleration * scaleFactor,
                Deceleration = baseProfile.Deceleration * scaleFactor,
                JerkAcceleration = baseProfile.JerkAcceleration * scaleFactor,
                JerkDeceleration = baseProfile.JerkDeceleration * scaleFactor
            };

            // 安全验证和调整
            ValidateAndAdjustProfile(newProfile);

            // 保存当前参数
            _currentProfiles[axisId] = newProfile;

            // 触发事件
            OnSpeedChanged(axisId, mode, newProfile);

            return newProfile;
        }

        /// <summary>
        /// 获取指定轴的当前速度参数
        /// </summary>
        public AxisSpeedProfile GetCurrentProfile(int axisId)
        {
            if (_currentProfiles.ContainsKey(axisId))
                return _currentProfiles[axisId].Clone();
            
            return null;
        }

        /// <summary>
        /// 获取指定轴的高速基准参数
        /// </summary>
        public AxisSpeedProfile GetBaseProfile(int axisId)
        {
            if (_baseProfiles.ContainsKey(axisId))
                return _baseProfiles[axisId].Clone();
            
            return null;
        }

        /// <summary>
        /// 获取指定轴的当前速度模式
        /// </summary>
        public SpeedMode GetCurrentSpeedMode(int axisId)
        {
            if (_currentProfiles.ContainsKey(axisId))
                return _currentProfiles[axisId].CurrentMode;
            
            return SpeedMode.High;
        }

        /// <summary>
        /// 验证并调整速度参数（安全保护）
        /// </summary>
        /// <param name="profile">待验证的速度参数</param>
        private void ValidateAndAdjustProfile(AxisSpeedProfile profile)
        {
            // 规则1: 初始速度不能大于运行速度
            if (profile.StartVelocity > profile.MaxVelocity)
            {
                // 自动调整初始速度为运行速度的80%
                profile.StartVelocity = profile.MaxVelocity * 0.8;
            }

            // 确保初始速度至少为0
            if (profile.StartVelocity < 0)
                profile.StartVelocity = 0;

            // 确保运行速度大于0
            if (profile.MaxVelocity <= 0)
                profile.MaxVelocity = 100; // 给一个默认值

            // 规则2: 加速度不能相对于速度过大（防止飞车）
            // 加速度不应超过运行速度的 MaxAccToVelRatio 倍
            double maxAcc = profile.MaxVelocity * MaxAccToVelRatio;
            if (profile.Acceleration > maxAcc)
            {
                profile.Acceleration = maxAcc;
            }

            if (profile.Deceleration > maxAcc)
            {
                profile.Deceleration = maxAcc;
            }

            // 规则3: 加速度不能过小（运动效率）
            double minAcc = profile.MaxVelocity * MinAccToVelRatio;
            if (profile.Acceleration < minAcc)
            {
                profile.Acceleration = minAcc;
            }

            if (profile.Deceleration < minAcc)
            {
                profile.Deceleration = minAcc;
            }

            // 规则4: 加加速度（Jerk）验证
            // 如果设置了加加速度，需要确保其合理性
            if (profile.JerkAcceleration > 0)
            {
                // 加加速度产生的加速度变化不应超过加速度本身
                double maxJerkAcc = profile.Acceleration * 2;
                if (profile.JerkAcceleration > maxJerkAcc)
                {
                    profile.JerkAcceleration = maxJerkAcc;
                }
            }

            if (profile.JerkDeceleration > 0)
            {
                double maxJerkDec = profile.Deceleration * 2;
                if (profile.JerkDeceleration > maxJerkDec)
                {
                    profile.JerkDeceleration = maxJerkDec;
                }
            }
        }

        /// <summary>
        /// 验证速度参数是否安全（不自动调整，仅返回结果）
        /// </summary>
        public SpeedValidationResult ValidateProfile(AxisSpeedProfile profile)
        {
            var result = new SpeedValidationResult { IsValid = true };

            // 检查初始速度 > 运行速度
            if (profile.StartVelocity > profile.MaxVelocity)
            {
                result.IsValid = false;
                result.Errors.Add("初始速度不能大于运行速度");
            }

            // 检查加速度相对于速度过大
            double maxAcc = profile.MaxVelocity * MaxAccToVelRatio;
            if (profile.Acceleration > maxAcc)
            {
                result.IsValid = false;
                result.Errors.Add(string.Format(
                    "加速度({0:F2})相对于运行速度({1:F2})过大，可能导致飞车。建议加速度不超过 {2:F2}",
                    profile.Acceleration, profile.MaxVelocity, maxAcc));
            }

            if (profile.Deceleration > maxAcc)
            {
                result.IsValid = false;
                result.Errors.Add(string.Format(
                    "减速度({0:F2})相对于运行速度({1:F2})过大，可能导致过冲。建议减速度不超过 {2:F2}",
                    profile.Deceleration, profile.MaxVelocity, maxAcc));
            }

            // 检查速度为0
            if (profile.MaxVelocity <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("运行速度必须大于0");
            }

            return result;
        }

        /// <summary>
        /// 将速度参数应用到运动控制卡
        /// </summary>
        public void ApplyToMotionCard(IMotionCard motionCard, int axisId)
        {
            if (motionCard == null)
                throw new ArgumentNullException(nameof(motionCard));

            var profile = GetCurrentProfile(axisId);
            if (profile == null)
                throw new InvalidOperationException(string.Format("轴{0}的速度参数未设置", axisId));

            // 设置速度曲线参数
            motionCard.SetVelocityProfile(
                axisId,
                profile.Acceleration,
                profile.Deceleration,
                profile.JerkAcceleration);
        }

        /// <summary>
        /// 触发速度变化事件
        /// </summary>
        protected virtual void OnSpeedChanged(int axisId, SpeedMode mode, AxisSpeedProfile profile)
        {
            SpeedChanged?.Invoke(this, new SpeedChangedEventArgs
            {
                AxisId = axisId,
                Mode = mode,
                Profile = profile
            });
        }
    }

    /// <summary>
    /// 速度变化事件参数
    /// </summary>
    public class SpeedChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 轴号
        /// </summary>
        public int AxisId { get; set; }

        /// <summary>
        /// 新的速度模式
        /// </summary>
        public SpeedMode Mode { get; set; }

        /// <summary>
        /// 新的速度参数
        /// </summary>
        public AxisSpeedProfile Profile { get; set; }
    }

    /// <summary>
    /// 速度参数验证结果
    /// </summary>
    public class SpeedValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误信息列表
        /// </summary>
        public List<string> Errors { get; set; }

        public SpeedValidationResult()
        {
            Errors = new List<string>();
        }
    }
}
