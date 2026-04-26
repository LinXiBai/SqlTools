using CoreToolkit.Motion.Core;
using CoreToolkit.Safety.Core;
using CoreToolkit.Safety.Models;
using System;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 安全系统快速初始化助手
    /// 帮助用户一键搭建完整的安全防护体系
    /// </summary>
    public class SafetySetupHelper
    {
        /// <summary>
        /// 创建完整的安全系统（含碰撞检测、软限位、互锁、后台监控）
        /// </summary>
        public static SafeMotionController CreateSafeMotionSystem(
            IMotionCard motionCard,
            SafetyConfig config = null)
        {
            if (motionCard == null) throw new ArgumentNullException(nameof(motionCard));

            config = config ?? SafetyConfigLoader.CreateDefaultConfig();

            // 创建各组件
            var collisionDetector = new CollisionDetector();
            var softLimitGuard = new SoftLimitGuard();
            var interlockEngine = new InterlockEngine();

            // 加载软限位
            if (config.SoftLimits != null)
            {
                softLimitGuard.SetLimits(config.SoftLimits);
            }

            // 加载安全体积
            if (config.SafetyVolumes != null)
            {
                foreach (var vol in config.SafetyVolumes)
                {
                    collisionDetector.RegisterVolume(vol);
                }
            }

            // 加载互锁规则
            if (config.InterlockRules != null)
            {
                foreach (var rule in config.InterlockRules)
                {
                    interlockEngine.AddRule(rule);
                }
            }

            // 创建安全运动控制器
            var safeController = new SafeMotionController(
                motionCard, collisionDetector, softLimitGuard, interlockEngine);

            return safeController;
        }

        /// <summary>
        /// 创建并启动后台安全监控
        /// </summary>
        public static SafetyMonitor CreateAndStartMonitor(
            IMotionCard motionCard,
            ICollisionDetector collisionDetector,
            InterlockEngine interlockEngine = null,
            int intervalMs = 100)
        {
            var monitor = new SafetyMonitor(motionCard, collisionDetector, interlockEngine)
            {
                IntervalMs = intervalMs
            };
            monitor.Start();
            return monitor;
        }

        /// <summary>
        /// 为双头设备添加防碰撞保护
        /// </summary>
        public static DualHeadAntiCollision SetupDualHeadProtection(
            IMotionCard motionCard,
            int headAAxis,
            int headBAxis,
            double minSeparation = 50.0,
            bool preventCrossing = true)
        {
            var dualHead = new DualHeadAntiCollision(motionCard, headAAxis, headBAxis)
            {
                MinSeparation = minSeparation,
                PreventCrossing = preventCrossing,
                Enabled = true
            };
            return dualHead;
        }

        /// <summary>
        /// 快速添加Z轴下压保护互锁规则
        /// </summary>
        public static InterlockRule AddZAxisDepthProtection(
            InterlockEngine engine,
            Func<double> getZPosition,
            double maxDepth,
            string ruleName = "Z轴下压保护")
        {
            var rule = new InterlockRule
            {
                Name = ruleName,
                Condition = () => getZPosition() > maxDepth,
                Action = InterlockAction.EmergencyStop,
                Message = $"Z轴位置超过最大安全下压深度 {maxDepth}"
            };
            engine.AddRule(rule);
            return rule;
        }

        /// <summary>
        /// 快速添加安全门互锁规则
        /// </summary>
        public static InterlockRule AddSafetyDoorInterlock(
            InterlockEngine engine,
            Func<bool> isDoorOpen,
            string ruleName = "安全门互锁")
        {
            var rule = new InterlockRule
            {
                Name = ruleName,
                Condition = () => isDoorOpen(),
                Action = InterlockAction.BlockMotion,
                Message = "安全门已打开，禁止自动运行"
            };
            engine.AddRule(rule);
            return rule;
        }

        /// <summary>
        /// 快速添加真空丢失保护（贴片机场景）
        /// </summary>
        public static InterlockRule AddVacuumLossProtection(
            InterlockEngine engine,
            Func<bool> isVacuumOn,
            Func<double> getZPosition,
            double zThreshold,
            string ruleName = "真空丢失保护")
        {
            var rule = new InterlockRule
            {
                Name = ruleName,
                Condition = () => getZPosition() > zThreshold && !isVacuumOn(),
                Action = InterlockAction.AlarmOnly,
                Message = $"Z轴超过 {zThreshold} 但真空未建立"
            };
            engine.AddRule(rule);
            return rule;
        }

        /// <summary>
        /// 快速添加伺服未使能互锁
        /// </summary>
        public static InterlockRule AddServoEnableCheck(
            InterlockEngine engine,
            Func<bool> isServoOn,
            string ruleName = "伺服使能检查")
        {
            var rule = new InterlockRule
            {
                Name = ruleName,
                Condition = () => !isServoOn(),
                Action = InterlockAction.BlockMotion,
                Message = "伺服未使能，禁止运动"
            };
            engine.AddRule(rule);
            return rule;
        }
    }
}
