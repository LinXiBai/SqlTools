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
        string CameraId { get; }
        bool IsOpen { get; }
        int Width { get; }
        int Height { get; }

        void Open();
        void Close();
        void StartGrab();
        void StopGrab();
        Bitmap GrabImage();
        void SetExposure(double microseconds);
        void SetGain(double gain);
    }
}
