using System;
using CoreToolkit.Motion.Core;
using CoreToolkit.Motion.Factory;
using CoreToolkit.Data;

namespace CoreToolkit.Motion.Examples
{
    /// <summary>
    /// 轴速度系统使用示例
    /// </summary>
    public class AxisSpeedExample
    {
        /// <summary>
        /// 示例1: 基本用法 - 手动配置速度参数
        /// </summary>
        public void Example1_BasicUsage()
        {
            // 1. 创建运动控制卡
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            // 2. 创建轴速度控制器
            var axisController = new AxisSpeedController(motionCard, axisId: 0);

            // 3. 从数据库参数初始化（高速基准参数）
            // 假设从数据库读取到以下参数：
            // - 运动低速（初始速度）= 1000
            // - 运动高速（运行速度）= 10000
            // - 加速度 = 50000
            // - 减速度 = 50000
            axisController.InitializeFromDatabase(
                lowSpeed: 1000,      // 初始速度
                highSpeed: 10000,    // 运行速度（高速100%）
                acc: 50000,          // 加速度
                dec: 50000,          // 减速度
                jerkAcc: 0,          // 加加速度（S曲线）
                jerkDec: 0           // 减减速度（S曲线）
            );

            // 4. 设置速度模式并自动应用到轴
            // 高速模式：速度 = 10000, 加速度 = 50000
            axisController.SetHighSpeed();
            Console.WriteLine("切换到高速模式: " + axisController.CurrentProfile);

            // 中速模式：速度 = 5000 (50%), 加速度 = 25000 (50%)
            axisController.SetMediumSpeed();
            Console.WriteLine("切换到中速模式: " + axisController.CurrentProfile);

            // 慢速模式：速度 = 1000 (10%), 加速度 = 5000 (10%)
            axisController.SetSlowSpeed();
            Console.WriteLine("切换到慢速模式: " + axisController.CurrentProfile);

            // 5. 使用当前速度模式执行运动
            axisController.MoveAbsolute(position: 1000);  // 使用慢速移动到位置1000
        }

        /// <summary>
        /// 示例2: 使用 AxisParameter 实体初始化
        /// </summary>
        public void Example2_UseAxisParameter()
        {
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            // 假设这是从数据库读取的轴参数
            var axisParam = new AxisParameter
            {
                轴号 = 0,
                轴名称 = "X轴",
                运动低速 = 500,       // 初始速度
                运动高速 = 5000,      // 运行速度（高速100%）
                加速度 = 20000,
                减速度 = 20000,
                加加速度 = 0,
                减减速度 = 0
            };

            // 创建控制器并初始化
            var axisController = new AxisSpeedController(motionCard, axisParam.轴号);
            axisController.InitializeFromParameter(axisParam);

            // 可以在UI中提供速度模式切换按钮
            // 按钮1: 高速 - 用于快速定位
            axisController.SetSpeedMode(SpeedMode.High);
            axisController.MoveAbsolute(10000);

            // 按钮2: 中速 - 用于正常工作
            axisController.SetSpeedMode(SpeedMode.Medium);
            axisController.MoveAbsolute(5000);

            // 按钮3: 慢速 - 用于精密调整
            axisController.SetSpeedMode(SpeedMode.Slow);
            axisController.MoveAbsolute(100);
        }

        /// <summary>
        /// 示例3: 批量管理多个轴
        /// </summary>
        public void Example3_MultiAxisManagement()
        {
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            // 创建共享的速度管理器
            var speedManager = new AxisSpeedManager();

            // 注册3个轴的基准参数
            // X轴 - 高速参数
            speedManager.RegisterAxisFromDatabase(0, 1000, 10000, 50000, 50000);
            
            // Y轴 - 高速参数
            speedManager.RegisterAxisFromDatabase(1, 800, 8000, 40000, 40000);
            
            // Z轴 - 高速参数（通常Z轴较慢）
            speedManager.RegisterAxisFromDatabase(2, 500, 5000, 20000, 20000);

            // 统一设置所有轴为慢速模式（例如：调试阶段）
            speedManager.SetSpeedMode(0, SpeedMode.Slow);
            speedManager.SetSpeedMode(1, SpeedMode.Slow);
            speedManager.SetSpeedMode(2, SpeedMode.Slow);

            // 应用到运动卡
            speedManager.ApplyToMotionCard(motionCard, 0);
            speedManager.ApplyToMotionCard(motionCard, 1);
            speedManager.ApplyToMotionCard(motionCard, 2);
        }

        /// <summary>
        /// 示例4: 安全验证示例 - 处理危险参数
        /// </summary>
        public void Example4_SafetyValidation()
        {
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            var axisController = new AxisSpeedController(motionCard, 0);

            // 设置一组危险参数：
            // - 初始速度 > 运行速度（错误）
            // - 加速度 >> 运行速度（会飞车！）
            axisController.InitializeFromDatabase(
                lowSpeed: 5000,      // 初始速度 > 运行速度！
                highSpeed: 1000,     // 运行速度
                acc: 30000,          // 加速度是速度的30倍！飞车风险！
                dec: 30000
            );

            // 验证当前参数
            var validation = axisController.ValidateCurrentProfile();
            if (!validation.IsValid)
            {
                Console.WriteLine("速度参数存在安全问题:");
                foreach (var error in validation.Errors)
                {
                    Console.WriteLine("  - " + error);
                }
            }

            // 切换到慢速模式时，系统会自动修正危险参数
            var profile = axisController.SetSlowSpeed();
            Console.WriteLine("自动修正后的参数: " + profile);
            // 输出结果：
            // - 初始速度会被修正为运行速度的80%：80 (原5000的10%是1000的10%=100，但初始速度>运行速度，所以被调整为80)
            // - 加速度会被限制为运行速度的2倍：200 (100 * 2)
        }

        /// <summary>
        /// 示例5: 速度变化事件处理
        /// </summary>
        public void Example5_SpeedChangeEvent()
        {
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            var axisController = new AxisSpeedController(motionCard, 0);
            axisController.InitializeFromDatabase(1000, 10000, 50000, 50000);

            // 订阅速度变化事件
            axisController.SpeedChanged += (sender, e) =>
            {
                Console.WriteLine(string.Format(
                    "轴{0} 速度模式已切换为: {1}",
                    e.AxisId,
                    e.Mode.GetDisplayName()));
                
                Console.WriteLine(string.Format(
                    "  初始速度: {0:F2} -> {1:F2}",
                    axisController.BaseProfile.StartVelocity * e.Mode.GetScaleFactor(),
                    e.Profile.StartVelocity));
                
                Console.WriteLine(string.Format(
                    "  运行速度: {0:F2} -> {1:F2}",
                    axisController.BaseProfile.MaxVelocity * e.Mode.GetScaleFactor(),
                    e.Profile.MaxVelocity));
            };

            // 切换速度模式时会触发事件
            axisController.SetHighSpeed();
            axisController.SetMediumSpeed();
            axisController.SetSlowSpeed();
        }

        /// <summary>
        /// 示例6: 不同场景应用不同速度
        /// </summary>
        public void Example6_ScenarioBasedSpeed()
        {
            var motionCard = MotionCardFactory.CreateCard("PCI1203", 0);
            motionCard.Initialize(new MotionConfig { CardId = 0 });
            motionCard.Open();

            var axisController = new AxisSpeedController(motionCard, 0);
            axisController.InitializeFromDatabase(500, 5000, 20000, 20000);

            // 场景1: 快速回退（高速）
            axisController.SetHighSpeed();
            axisController.MoveAbsolute(0);
            axisController.WaitForMotionComplete();

            // 场景2: 正常工作（中速）
            axisController.SetMediumSpeed();
            for (int i = 0; i < 10; i++)
            {
                axisController.MoveRelative(100);
                axisController.WaitForMotionComplete();
            }

            // 场景3: 精密定位（慢速）
            axisController.SetSlowSpeed();
            axisController.MoveAbsolute(50);  // 精确移动到50mm位置
            axisController.WaitForMotionComplete();
        }
    }
}
