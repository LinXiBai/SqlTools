using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Communication.Core;
using CoreToolkit.Communication.Models;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 增强版 TCP 客户端：支持事件接收、心跳、自动重连、收发日志
    /// </summary>
    public class AdvancedTcpClient : ITcpClient, IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private Task _heartbeatTask;
        private readonly object _lock = new object();
        private int _isReconnecting; // 0: false, 1: true
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public string Host { get; }
        public int Port { get; }
        public bool IsConnected => _client?.Connected ?? false;
        public int ReceiveTimeout { get; set; } = 5000;
        public int SendTimeout { get; set; } = 5000;

        /// <summary>收到数据时触发</summary>
        public event EventHandler<TcpEventArgs> DataReceived;

        /// <summary>连接状态变化时触发</summary>
        public event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>日志输出</summary>
        public Action<string> LogAction { get; set; }

        /// <summary>是否启用自动重连</summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>自动重连间隔（毫秒）</summary>
        public int ReconnectIntervalMs { get; set; } = 3000;

        /// <summary>最大重连次数，0 表示无限</summary>
        public int MaxReconnectAttempts { get; set; } = 0;

        /// <summary>是否启用心跳</summary>
        public bool EnableHeartbeat { get; set; } = false;

        /// <summary>心跳间隔（毫秒）</summary>
        public int HeartbeatIntervalMs { get; set; } = 5000;

        /// <summary>心跳数据包</summary>
        public byte[] HeartbeatData { get; set; } = new byte[] { 0x00 };

        public AdvancedTcpClient(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void Connect()
        {
            ConnectAsync().GetAwaiter().GetResult();
        }

        public async Task ConnectAsync()
        {
            DisconnectInternal(false);

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(Host, Port).ConfigureAwait(false);
                _stream = _client.GetStream();
                _stream.ReadTimeout = ReceiveTimeout;
                _stream.WriteTimeout = SendTimeout;

                _cts = new CancellationTokenSource();
                _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token), _cts.Token);

                if (EnableHeartbeat)
                {
                    _heartbeatTask = Task.Run(() => HeartbeatLoop(_cts.Token), _cts.Token);
                }

                Log($"TCP 已连接到 {Host}:{Port}");
                ConnectionStatusChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Log($"TCP 连接失败: {ex.Message}");
                ConnectionStatusChanged?.Invoke(this, false);

                if (AutoReconnect && Interlocked.CompareExchange(ref _isReconnecting, 0, 0) == 0)
                {
                    _ = Task.Run(() => TryReconnect(), CancellationToken.None);
                }
            }
        }

        public void Disconnect()
        {
            DisconnectInternal(true);
        }

        public void Send(byte[] data)
        {
            lock (_lock)
            {
                if (_stream == null || !IsConnected)
                    throw new InvalidOperationException("TCP 未连接。");

                _stream.Write(data, 0, data.Length);
                Log($"[TX] {BitConverter.ToString(data).Replace("-", " ")}");
            }
        }

        public async Task SendAsync(byte[] data)
        {
            await _asyncLock.WaitAsync();
            try
            {
                if (_stream == null || !IsConnected)
                    throw new InvalidOperationException("TCP 未连接。");

                await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                Log($"[TX] {BitConverter.ToString(data).Replace("-", " ")}");
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        public byte[] Receive(int count)
        {
            lock (_lock)
            {
                if (_stream == null || !IsConnected)
                    throw new InvalidOperationException("TCP 未连接。");

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
                if (read > 0)
                    Log($"[RX-Read] {BitConverter.ToString(result).Replace("-", " ")}");
                return result;
            }
        }

        public async Task<byte[]> ReceiveAsync(int count)
        {
            await _asyncLock.WaitAsync();
            try
            {
                if (_stream == null || !IsConnected)
                    throw new InvalidOperationException("TCP 未连接。");

                var buffer = new byte[count];
                int read = 0;
                while (read < count)
                {
                    int n = await _stream.ReadAsync(buffer, read, count - read).ConfigureAwait(false);
                    if (n == 0) break;
                    read += n;
                }
                var result = new byte[read];
                Array.Copy(buffer, result, read);
                if (read > 0)
                    Log($"[RX-Read] {BitConverter.ToString(result).Replace("-", " ")}");
                return result;
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        public byte[] SendReceive(byte[] data, int expectedLength = 0)
        {
            Send(data);
            if (expectedLength > 0)
                return Receive(expectedLength);
            return new byte[0];
        }

        private void DisconnectInternal(bool notify)
        {
            _cts?.Cancel();

            // 等待任务结束
            try { _receiveTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
            try { _heartbeatTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }

            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }

            _receiveTask?.Dispose();
            _heartbeatTask?.Dispose();
            _cts?.Dispose();

            _stream = null;
            _client = null;
            _cts = null;
            _receiveTask = null;
            _heartbeatTask = null;

            if (notify)
            {
                Log("TCP 已断开连接");
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    int read = 0;
                    try
                    {
                        read = await _stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        read = 0;
                    }

                    if (read == 0)
                    {
                        Log("TCP 连接已断开（对端关闭）");
                        ConnectionStatusChanged?.Invoke(this, false);

                        if (AutoReconnect && Interlocked.CompareExchange(ref _isReconnecting, 0, 0) == 0)
                        {
                            _ = Task.Run(() => TryReconnect(), CancellationToken.None);
                        }
                        break;
                    }

                    var data = new byte[read];
                    Array.Copy(buffer, data, read);
                    Log($"[RX-Event] {BitConverter.ToString(data).Replace("-", " ")}");
                    DataReceived?.Invoke(this, new TcpEventArgs($"{Host}:{Port}", data));
                }
            }
            catch (Exception ex)
            {
                Log($"接收循环异常: {ex.Message}");
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        private async Task HeartbeatLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(HeartbeatIntervalMs, token).ConfigureAwait(false);
                    if (IsConnected && HeartbeatData != null && HeartbeatData.Length > 0)
                    {
                        await SendAsync(HeartbeatData);
                        Log("[Heartbeat] 发送心跳包");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"心跳异常: {ex.Message}");
                }
            }
        }

        private async Task TryReconnect()
        {
            if (Interlocked.CompareExchange(ref _isReconnecting, 1, 0) == 1)
                return;

            try
            {
                int attempt = 0;
                while (AutoReconnect && !IsConnected)
                {
                    if (MaxReconnectAttempts > 0 && attempt >= MaxReconnectAttempts)
                    {
                        Log($"TCP 重连已达到最大次数 {MaxReconnectAttempts}，停止重连。");
                        break;
                    }

                    attempt++;
                    Log($"TCP 正在尝试第 {attempt} 次重连...");
                    await Task.Delay(ReconnectIntervalMs).ConfigureAwait(false);
                    await ConnectAsync();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isReconnecting, 0);
            }
        }

        private void Log(string message)
        {
            LogAction?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] [TCP {Host}:{Port}] {message}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Disconnect();
                _asyncLock?.Dispose();
            }

            _disposed = true;
        }

        ~AdvancedTcpClient()
        {
            Dispose(false);
        }
    }
}
