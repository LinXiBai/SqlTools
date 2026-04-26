using System;

namespace CoreToolkit.Data
{
    /// <summary>
    /// 状态机运行记录实体
    /// 持久化存储每次状态机执行的完整信息
    /// </summary>
    public class StateMachineRecord : EntityBase
    {
        private string _machineName;
        private string _description;
        private string _status;
        private DateTime _startTime;
        private DateTime? _endTime;
        private double _durationMs;
        private bool _isSuccess;
        private string _errorMessage;
        private string _exception;
        private string _moduleStatsJson;
        private string _contextId;
        private int _moduleCount;
        private string _resumeDataJson;

        /// <summary>
        /// 状态机名称
        /// </summary>
        [Field("状态机名称", "流程信息", ControlType.String)]
        public string MachineName
        {
            get { return _machineName; }
            set { SetProperty(ref _machineName, value); }
        }

        /// <summary>
        /// 状态机描述
        /// </summary>
        [Field("状态机描述", "流程信息", ControlType.String)]
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        /// <summary>
        /// 最终状态（Idle/Running/Paused/Completed/Error/Stopping）
        /// </summary>
        [Field("最终状态", "流程信息", ControlType.String)]
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Field("开始时间", "执行信息", ControlType.None)]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Field("结束时间", "执行信息", ControlType.None)]
        public DateTime? EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        [Field("耗时(ms)", "执行信息", ControlType.Numeric)]
        public double DurationMs
        {
            get { return _durationMs; }
            set { SetProperty(ref _durationMs, value); }
        }

        /// <summary>
        /// 是否成功完成
        /// </summary>
        [Field("是否成功", "执行信息", ControlType.Bool)]
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set { SetProperty(ref _isSuccess, value); }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        [Field("错误信息", "执行信息", ControlType.String)]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        /// <summary>
        /// 异常详情（完整异常文本）
        /// </summary>
        [Field("异常详情", "执行信息", ControlType.String)]
        public string Exception
        {
            get { return _exception; }
            set { SetProperty(ref _exception, value); }
        }

        /// <summary>
        /// 模块统计 JSON 序列化数据
        /// </summary>
        [Field("模块统计JSON", "执行信息", ControlType.String)]
        public string ModuleStatsJson
        {
            get { return _moduleStatsJson; }
            set { SetProperty(ref _moduleStatsJson, value); }
        }

        /// <summary>
        /// 执行上下文 ID
        /// </summary>
        [Field("上下文ID", "流程信息", ControlType.String)]
        public string ContextId
        {
            get { return _contextId; }
            set { SetProperty(ref _contextId, value); }
        }

        /// <summary>
        /// 模块数量
        /// </summary>
        [Field("模块数量", "执行信息", ControlType.Numeric)]
        public int ModuleCount
        {
            get { return _moduleCount; }
            set { SetProperty(ref _moduleCount, value); }
        }

        /// <summary>
        /// 恢复数据 JSON（序列化的 ExecutionContext Parameters 与 Results）
        /// </summary>
        [Field("恢复数据JSON", "执行信息", ControlType.String)]
        public string ResumeDataJson
        {
            get { return _resumeDataJson; }
            set { SetProperty(ref _resumeDataJson, value); }
        }
    }
}
