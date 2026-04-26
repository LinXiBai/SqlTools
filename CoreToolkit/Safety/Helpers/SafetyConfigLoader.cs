using CoreToolkit.Common.Helpers;
using CoreToolkit.Safety.Models;
using System;
using System.IO;

namespace CoreToolkit.Safety.Helpers
{
    /// <summary>
    /// 安全配置加载器
    /// 支持从JSON文件加载安全配置
    /// </summary>
    public class SafetyConfigLoader
    {
        /// <summary>
        /// 从JSON文件加载安全配置
        /// </summary>
        public static SafetyConfig LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("安全配置文件不存在", filePath);

            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }

        /// <summary>
        /// 从JSON字符串加载安全配置
        /// </summary>
        public static SafetyConfig LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new SafetyConfig();

            return JsonHelper.Deserialize<SafetyConfig>(json);
        }

        /// <summary>
        /// 保存安全配置到JSON文件
        /// </summary>
        public static void SaveToFile(SafetyConfig config, string filePath)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            string json = JsonHelper.Serialize(config);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 创建默认安全配置
        /// </summary>
        public static SafetyConfig CreateDefaultConfig()
        {
            return new SafetyConfig
            {
                MonitorIntervalMs = 100,
                EnableBackgroundMonitor = true,
                DualHeadMinSeparation = 50.0,
                ZAxisMaxSafeDepth = 50.0,
                SoftLimits = new System.Collections.Generic.List<SoftLimitConfig>(),
                SafetyVolumes = new System.Collections.Generic.List<SafetyVolume>(),
                InterlockRules = new System.Collections.Generic.List<InterlockRule>()
            };
        }

        /// <summary>
        /// 创建贴片机默认安全配置示例
        /// </summary>
        public static SafetyConfig CreateSmtDefaultConfig()
        {
            var config = CreateDefaultConfig();

            // 默认软限位（典型贴片机行程）
            config.SoftLimits.Add(new SoftLimitConfig { AxisIndex = 0, PositiveLimit = 600, NegativeLimit = 0 });
            config.SoftLimits.Add(new SoftLimitConfig { AxisIndex = 1, PositiveLimit = 500, NegativeLimit = 0 });
            config.SoftLimits.Add(new SoftLimitConfig { AxisIndex = 2, PositiveLimit = 50, NegativeLimit = 0 });

            // 默认安全体积：吸嘴
            config.SafetyVolumes.Add(new SafetyVolume
            {
                Name = "贴装头吸嘴",
                Type = VolumeType.Dynamic,
                BoundingBox = new BoundingBox
                {
                    MinX = -10, MaxX = 10,
                    MinY = -10, MaxY = 10,
                    MinZ = -30, MaxZ = 5
                },
                SafetyMargin = 2.0,
                LinkedAxes = new[] { 0, 1, 2 },
                OffsetX = 0, OffsetY = 0, OffsetZ = 0
            });

            // 默认安全体积：基板区域（静止障碍）
            config.SafetyVolumes.Add(new SafetyVolume
            {
                Name = "基板治具",
                Type = VolumeType.Static,
                BoundingBox = new BoundingBox
                {
                    MinX = 100, MaxX = 500,
                    MinY = 50, MaxY = 450,
                    MinZ = -5, MaxZ = 10
                },
                SafetyMargin = 5.0
            });

            return config;
        }
    }
}
