namespace SqlDemo.Data
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// 日志实体（用于演示多数据库场景）
    /// </summary>
    public class Log : EntityBase
    {
        private string _level;
        private string _message;
        private System.DateTime _timestamp;
        private string _loggerName;
        private string _exception;
        private string _stackTrace;
        private string _machineName;
        private int _threadId;
        private int _processId;
        private string _additionalInfo;

        [Field("日志级别", "日志信息", ControlType.ComboBox)]
        public string Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        [Field("日志消息", "日志信息", ControlType.String)]
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        [Field("记录时间", "日志信息", ControlType.None)]
        public System.DateTime Timestamp
        {
            get { return _timestamp; }
            set { SetProperty(ref _timestamp, value); }
        }

        [Field("记录器名称", "日志信息", ControlType.String)]
        public string LoggerName
        {
            get { return _loggerName; }
            set { SetProperty(ref _loggerName, value); }
        }

        [Field("异常信息", "日志信息", ControlType.String)]
        public string Exception
        {
            get { return _exception; }
            set { SetProperty(ref _exception, value); }
        }

        [Field("堆栈跟踪", "日志信息", ControlType.String)]
        public string StackTrace
        {
            get { return _stackTrace; }
            set { SetProperty(ref _stackTrace, value); }
        }

        [Field("机器名称", "环境信息", ControlType.String)]
        public string MachineName
        {
            get { return _machineName; }
            set { SetProperty(ref _machineName, value); }
        }

        [Field("线程ID", "环境信息", ControlType.Numeric)]
        public int ThreadId
        {
            get { return _threadId; }
            set { SetProperty(ref _threadId, value); }
        }

        [Field("进程ID", "环境信息", ControlType.Numeric)]
        public int ProcessId
        {
            get { return _processId; }
            set { SetProperty(ref _processId, value); }
        }

        [Field("额外信息", "日志信息", ControlType.String)]
        public string AdditionalInfo
        {
            get { return _additionalInfo; }
            set { SetProperty(ref _additionalInfo, value); }
        }
    }
}
