using System;

namespace CoreToolkit.Common.Models
{
    /// <summary>
    /// 通用操作结果封装
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; }
        public Exception Exception { get; protected set; }

        public bool IsFailure => !IsSuccess;

        protected Result() { }

        public static Result Success(string message = null)
        {
            return new Result { IsSuccess = true, Message = message };
        }

        public static Result Failure(string message, Exception exception = null)
        {
            return new Result { IsSuccess = false, Message = message, Exception = exception };
        }

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
        public T Data { get; private set; }

        public static Result<T> Success(T data, string message = null)
        {
            return new Result<T> { IsSuccess = true, Data = data, Message = message };
        }

        public new static Result<T> Failure(string message, Exception exception = null)
        {
            return new Result<T> { IsSuccess = false, Message = message, Exception = exception };
        }

        public new static Result<T> Failure(Exception exception)
        {
            return new Result<T> { IsSuccess = false, Message = exception?.Message, Exception = exception };
        }
    }
}
