using System;

namespace CoreToolkit.StateMachine.Models
{
    /// <summary>
    /// 模块进度报告
    /// </summary>
    public class ModuleProgress
    {
        /// <summary>模块ID</summary>
        public string ModuleId { get; set; }
        /// <summary>模块名称</summary>
        public string ModuleName { get; set; }
        /// <summary>当前步骤/阶段名称</summary>
        public string StepName { get; set; }
        /// <summary>总体进度 (0.0 - 1.0)</summary>
        public double OverallProgress { get; set; }
        /// <summary>当前步骤进度 (0.0 - 1.0)</summary>
        public double StepProgress { get; set; }
        /// <summary>附加消息</summary>
        public string Message { get; set; }
        /// <summary>时间戳</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
