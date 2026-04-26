using System;
using System.Threading.Tasks;

namespace CoreToolkit.Communication.Core
{
    /// <summary>
    /// TCP 客户端接口抽象
    /// </summary>
    public interface ITcpClient : IDisposable
    {
        string Host { get; }
        int Port { get; }
        bool IsConnected { get; }
        int ReceiveTimeout { get; set; }
        int SendTimeout { get; set; }

        void Connect();
        Task ConnectAsync();
        void Disconnect();
        void Send(byte[] data);
        Task SendAsync(byte[] data);
        byte[] Receive(int count);
        Task<byte[]> ReceiveAsync(int count);
        byte[] SendReceive(byte[] data, int expectedLength = 0);
    }
}
