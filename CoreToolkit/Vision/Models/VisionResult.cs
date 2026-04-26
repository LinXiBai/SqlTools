using System;

namespace CoreToolkit.Vision.Models
{
    /// <summary>
    /// 视觉处理结果基类
    /// </summary>
    public class VisionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 处理时间（毫秒）
        /// </summary>
        public double ProcessingTimeMs { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <returns>成功结果对象</returns>
        public static VisionResult Success(string message = null)
        {
            return new VisionResult { IsSuccess = true, Message = message };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <returns>失败结果对象</returns>
        public static VisionResult Failure(string message)
        {
            return new VisionResult { IsSuccess = false, Message = message };
        }
    }
}
