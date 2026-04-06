using System;

namespace CoreToolkit.Communication.Core
{
    /// <summary>
    /// 串口通信接口抽象
    /// </summary>
    public interface ISerialPort : IDisposable
    {
        string PortName { get; }
        int BaudRate { get; }
        bool IsOpen { get; }

        void Open();
        void Close();
        void Write(byte[] data, int offset, int count);
        void WriteLine(string text);
        byte[] Read(int count, int timeoutMs = 1000);
        string ReadLine(int timeoutMs = 1000);
        void ClearBuffers();
    }
}
