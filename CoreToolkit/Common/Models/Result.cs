using System;

namespace CoreToolkit.Common.Models
{
    /// <summary>
    /// 通用操作结果封装
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 获取操作是否成功
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// 获取操作消息
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// 获取异常信息
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// 获取操作是否失败
        /// </summary>
        public bool IsFailure => !IsSuccess;

        protected Result() { }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <returns>成功的Result对象</returns>
        public static Result Success(string message = null)
        {
            return new Result { IsSuccess = true, Message = message };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="exception">异常信息</param>
        /// <returns>失败的Result对象</returns>
        public static Result Failure(string message, Exception exception = null)
        {
            return new Result { IsSuccess = false, Message = message, Exception = exception };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="exception">异常信息</param>
        /// <returns>失败的Result对象</returns>
        public static Result Failure(Exception exception)
        {
            return new Result { IsSuccess = false, Message = exception?.Message, Exception = exception };
        }
    }

    /// <summary>
    /// 带返回值的通用操作结果封装
    /// </summary>
    public class Result<T> : Result
    {
        /// <summary>
        /// 获取返回的数据
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="data">返回的数据</param>
        /// <param name="message">成功消息</param>
        /// <returns>成功的Result对象</returns>
        public static Result<T> Success(T data, string message = null)
        {
            return new Result<T> { IsSuccess = true, Data = data, Message = message };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <param name="exception">异常信息</param>
        /// <returns>失败的Result对象</returns>
        public new static Result<T> Failure(string message, Exception exception = null)
        {
            return new Result<T> { IsSuccess = false, Message = message, Exception = exception };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="exception">异常信息</param>
        /// <returns>失败的Result对象</returns>
        public new static Result<T> Failure(Exception exception)
        {
            return new Result<T> { IsSuccess = false, Message = exception?.Message, Exception = exception };
        }
    }
}
