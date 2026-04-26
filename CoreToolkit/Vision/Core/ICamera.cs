using System;
using System.Drawing;
using CoreToolkit.Vision.Models;

namespace CoreToolkit.Vision.Core
{
    /// <summary>
    /// 工业相机接口抽象
    /// </summary>
    public interface ICamera : IDisposable
    {
        /// <summary>
        /// 相机ID
        /// </summary>
        string CameraId { get; }
        
        /// <summary>
        /// 相机是否打开
        /// </summary>
        bool IsOpen { get; }
        
        /// <summary>
        /// 图像宽度
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// 图像高度
        /// </summary>
        int Height { get; }

        /// <summary>
        /// 打开相机
        /// </summary>
        void Open();
        
        /// <summary>
        /// 关闭相机
        /// </summary>
        void Close();
        
        /// <summary>
        /// 开始采集
        /// </summary>
        void StartGrab();
        
        /// <summary>
        /// 停止采集
        /// </summary>
        void StopGrab();
        
        /// <summary>
        /// 采集图像
        /// </summary>
        /// <returns>采集到的图像</returns>
        Bitmap GrabImage();
        
        /// <summary>
        /// 设置曝光时间
        /// </summary>
        /// <param name="microseconds">曝光时间（微秒）</param>
        void SetExposure(double microseconds);
        
        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="gain">增益值</param>
        void SetGain(double gain);
    }
}
