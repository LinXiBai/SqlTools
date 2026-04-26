using System;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// 运动控制异常类
    /// </summary>
    public class MotionException : Exception
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 发生错误的轴号
        /// </summary>
        public int Axis { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionException() : base()
        {
            ErrorCode = 0;
            Axis = -1;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionException(string message) : base(message)
        {
            ErrorCode = 0;
            Axis = -1;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = 0;
            Axis = -1;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
            Axis = -1;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionException(string message, int errorCode, int axis)
            : base(message)
        {
            ErrorCode = errorCode;
            Axis = axis;
        }

        /// <summary>
        /// 获取完整的错误信息
        /// </summary>
        public override string ToString()
        {
            string msg = string.Format("[MotionError] Code={0}", ErrorCode);
            if (Axis >= 0) msg += string.Format(", Axis={0}", Axis);
            msg += string.Format(": {0}", Message);
            return msg;
        }
    }
}
