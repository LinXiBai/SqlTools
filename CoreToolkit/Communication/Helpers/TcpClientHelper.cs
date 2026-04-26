using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using CoreToolkit.Communication.Core;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 基于 System.Net.Sockets 的 TCP 客户端实现
    /// </summary>
    public class TcpClientHelper : ITcpClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly object _lock = new object();

        public string Host { get; }
        public int Port { get; }
        public bool IsConnected => _client?.Connected ?? false;
        public int ReceiveTimeout { get; set; } = 5000;
        public int SendTimeout { get; set; } = 5000;

        public TcpClientHelper(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void Connect()
        {
            Disconnect();
            _client = new TcpClient();
            _client.Connect(Host, Port);
            _stream = _client.GetStream();
            _stream.ReadTimeout = ReceiveTimeout;
            _stream.WriteTimeout = SendTimeout;
        }

        public async Task ConnectAsync()
        {
            Disconnect();
            _client = new TcpClient();
            await _client.ConnectAsync(Host, Port);
            _stream = _client.GetStream();
            _stream.ReadTimeout = ReceiveTimeout;
            _stream.WriteTimeout = SendTimeout;
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }

        public void Send(byte[] data)
        {
            lock (_lock)
            {
                if (_stream == null) throw new InvalidOperationException("TCP not connected.");
                _stream.Write(data, 0, data.Length);
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_stream == null) throw new InvalidOperationException("TCP not connected.");
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public byte[] Receive(int count)
        {
            lock (_lock)
            {
                if (_stream == null) throw new InvalidOperationException("TCP not connected.");
                var buffer = new byte[count];
                int read = 0;
                while (read < count)
                {
                    int n = _stream.Read(buffer, read, count - read);
                    if (n == 0) break;
                    read += n;
                }
                var result = new byte[read];
                Array.Copy(buffer, result, read);
                return result;
            }
        }

        public async Task<byte[]> ReceiveAsync(int count)
        {
            if (_stream == null) throw new InvalidOperationException("TCP not connected.");
            var buffer = new byte[count];
            int read = 0;
            while (read < count)
            {
                int n = await _stream.ReadAsync(buffer, read, count - read);
                if (n == 0) break;
                read += n;
            }
            var result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }

        public byte[] SendReceive(byte[] data, int expectedLength = 0)
        {
            Send(data);
            if (expectedLength > 0)
                return Receive(expectedLength);
            return new byte[0];
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
