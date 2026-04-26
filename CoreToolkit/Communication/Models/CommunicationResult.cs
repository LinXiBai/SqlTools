namespace CoreToolkit.Communication.Models
{
    /// <summary>
    /// 通信操作结果
    /// </summary>
    public class CommunicationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public byte[] Data { get; set; }

        public static CommunicationResult Success(byte[] data = null, string message = null)
        {
            return new CommunicationResult { IsSuccess = true, Data = data, Message = message };
        }

        public static CommunicationResult Failure(string message)
        {
            return new CommunicationResult { IsSuccess = false, Message = message };
        }
    }
}
