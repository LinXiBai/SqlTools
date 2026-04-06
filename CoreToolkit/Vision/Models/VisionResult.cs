using System;

namespace CoreToolkit.Vision.Models
{
    /// <summary>
    /// 视觉处理结果基类
    /// </summary>
    public class VisionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public double ProcessingTimeMs { get; set; }

        public static VisionResult Success(string message = null)
        {
            return new VisionResult { IsSuccess = true, Message = message };
        }

        public static VisionResult Failure(string message)
        {
            return new VisionResult { IsSuccess = false, Message = message };
        }
    }
}
