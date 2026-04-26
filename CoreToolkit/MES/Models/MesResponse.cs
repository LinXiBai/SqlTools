namespace CoreToolkit.MES.Models
{
    /// <summary>
    /// MES 通用响应
    /// </summary>
    public class MesResponse
    {
        public bool IsSuccess { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }

        public static MesResponse Success(string message = null, string transactionId = null)
        {
            return new MesResponse { IsSuccess = true, Message = message, TransactionId = transactionId };
        }

        public static MesResponse Failure(string code, string message, string transactionId = null)
        {
            return new MesResponse { IsSuccess = false, Code = code, Message = message, TransactionId = transactionId };
        }
    }

    /// <summary>
    /// 带数据体的 MES 响应
    /// </summary>
    public class MesResponse<T> : MesResponse
    {
        public T Data { get; set; }

        public static MesResponse<T> Success(T data, string message = null, string transactionId = null)
        {
            return new MesResponse<T> { IsSuccess = true, Data = data, Message = message, TransactionId = transactionId };
        }

        public new static MesResponse<T> Failure(string code, string message, string transactionId = null)
        {
            return new MesResponse<T> { IsSuccess = false, Code = code, Message = message, TransactionId = transactionId };
        }
    }
}
