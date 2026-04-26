using System;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 轴速度控制器
    /// 封装了速度管理和运动控制的便捷操作类
    /// </summary>
    public class AxisSpeedController
    {
        private readonly IMotionCard _motionCard;
        private readonly AxisSpeedManager _speedManager;
        private readonly int _axisId;

        /// <summary>
        /// 运动控制卡接口
        /// </summary>
        public IMotionCard MotionCard => _motionCard;

        /// <summary>
        /// 速度管理器
        /// </summary>
        public AxisSpeedManager SpeedManager => _speedManager;

        /// <summary>
        /// 轴号
        /// </summary>
        public int AxisId => _axisId;

        /// <summary>
        /// 当前速度模式
        /// </summary>
        public SpeedMode CurrentSpeedMode => _speedManager.GetCurrentSpeedMode(_axisId);

        /// <summary>
        /// 当前速度参数
        /// </summary>
        public AxisSpeedProfile CurrentProfile => _speedManager.GetCurrentProfile(_axisId);

        /// <summary>
        /// 高速基准参数
        /// </summary>
        public AxisSpeedProfile BaseProfile => _speedManager.GetBaseProfile(_axisId);

        /// <summary>
        /// 速度变化事件
        /// </summary>
        public event EventHandler<SpeedChangedEventArgs> SpeedChanged
        {
            add { _speedManager.SpeedChanged += value; }
            remove { _speedManager.SpeedChanged -= value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionCard">运动控制卡</param>
        /// <param name="axisId">轴号</param>
        public AxisSpeedController(IMotionCard motionCard, int axisId)
        {
            _motionCard = motionCard ?? throw new ArgumentNullException(nameof(motionCard));
            _axisId = axisId;
            _speedManager = new AxisSpeedManager();
        }

        /// <summary>
        /// 从数据库参数初始化（高速基准参数）
        /// </summary>
        public void InitializeFromDatabase(
            double lowSpeed,
            double highSpeed,
            double acc,
            double dec,
            double jerkAcc = 0,
            double jerkDec = 0)
        {
            _speedManager.RegisterAxisFromDatabase(_axisId, lowSpeed, highSpeed, acc, dec, jerkAcc, jerkDec);
        }

        /// <summary>
        /// 从 AxisParameter 实体初始化
        /// </summary>
        public void InitializeFromParameter(CoreToolkit.Data.AxisParameter param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            InitializeFromDatabase(
                param.运动低速,
                param.运动高速,
                param.加速度,
                param.减速度,
                param.加加速度,
                param.减减速度);
        }

        /// <summary>
        /// 设置速度模式并应用到轴
        /// </summary>
        /// <param name="mode">速度模式</param>
        /// <param name="applyImmediately">是否立即应用到运动卡</param>
        public AxisSpeedProfile SetSpeedMode(SpeedMode mode, bool applyImmediately = true)
        {
            var profile = _speedManager.SetSpeedMode(_axisId, mode);

            if (applyImmediately)
            {
                ApplySpeedToMotionCard();
            }

            return profile;
        }

        /// <summary>
        /// 切换到高速模式
        /// </summary>
        public AxisSpeedProfile SetHighSpeed(bool applyImmediately = true)
        {
            return SetSpeedMode(SpeedMode.High, applyImmediately);
        }

        /// <summary>
        /// 切换到中速模式
        /// </summary>
        public AxisSpeedProfile SetMediumSpeed(bool applyImmediately = true)
        {
            return SetSpeedMode(SpeedMode.Medium, applyImmediately);
        }

        /// <summary>
        /// 切换到慢速模式
        /// </summary>
        public AxisSpeedProfile SetSlowSpeed(bool applyImmediately = true)
        {
            return SetSpeedMode(SpeedMode.Slow, applyImmediately);
        }

        /// <summary>
        /// 将当前速度参数应用到运动控制卡
        /// </summary>
        public void ApplySpeedToMotionCard()
        {
            _speedManager.ApplyToMotionCard(_motionCard, _axisId);
        }

        /// <summary>
        /// 验证当前速度参数是否安全
        /// </summary>
        public SpeedValidationResult ValidateCurrentProfile()
        {
            var profile = _speedManager.GetCurrentProfile(_axisId);
            if (profile == null)
                return new SpeedValidationResult { IsValid = false };

            return _speedManager.ValidateProfile(profile);
        }

        /// <summary>
        /// 获取用于运动的速度值
        /// 根据当前模式返回合适的速度
        /// </summary>
        public double GetMotionSpeed()
        {
            var profile = _speedManager.GetCurrentProfile(_axisId);
            return profile?.MaxVelocity ?? 0;
        }

        #region 便捷运动方法

        /// <summary>
        /// 绝对位置运动（使用当前速度模式）
        /// </summary>
        public void MoveAbsolute(double position)
        {
            double speed = GetMotionSpeed();
            _motionCard.MoveAbsolute(_axisId, position, speed);
        }

        /// <summary>
        /// 相对位置运动（使用当前速度模式）
        /// </summary>
        public void MoveRelative(double distance)
        {
            double speed = GetMotionSpeed();
            _motionCard.MoveRelative(_axisId, distance, speed);
        }

        /// <summary>
        /// JOG运动（使用当前速度模式）
        /// </summary>
        public void Jog(int direction)
        {
            double speed = GetMotionSpeed();
            _motionCard.Jog(_axisId, direction, speed);
        }

        /// <summary>
        /// 停止轴
        /// </summary>
        public void Stop(bool emergency = false)
        {
            _motionCard.Stop(_axisId, emergency);
        }

        /// <summary>
        /// 回零（使用回零速度参数，不归速度模式管理）
        /// </summary>
        public void Home(double homeSpeed)
        {
            _motionCard.Home(_axisId, homeSpeed);
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        public AxisStatus GetStatus()
        {
            return _motionCard.GetAxisStatus(_axisId);
        }

        /// <summary>
        /// 伺服使能
        /// </summary>
        public void SetServoEnable(bool enable)
        {
            _motionCard.SetServoEnable(_axisId, enable);
        }

        /// <summary>
        /// 等待运动完成
        /// </summary>
        public bool WaitForMotionComplete(int timeoutMs = 10000)
        {
            return _motionCard.WaitForMotionComplete(_axisId, timeoutMs);
        }

        #endregion
    }
}
