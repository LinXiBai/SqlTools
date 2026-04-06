using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Communication.Models;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// TCP 服务端：支持多客户端连接、数据转发、客户端管理
    /// </summary>
    public class TcpServerHelper : IDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private Task _acceptTask;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _clientLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public int Port { get; }
        public bool IsRunning { get; private set; }

        /// <summary>收到客户端数据时触发</summary>
        public event EventHandler<TcpEventArgs> DataReceived;

        /// <summary>客户端连接/断开时触发</summary>
        public event EventHandler<(string clientId, bool isConnected)> ClientConnectionChanged;

        /// <summary>日志输出</summary>
        public Action<string> LogAction { get; set; }

        public TcpServerHelper(int port)
        {
            Port = port;
        }

        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            IsRunning = true;

            _acceptTask = Task.Run(() => AcceptLoop(_cts.Token));
            Log($"TCP 服务端已启动，端口 {Port}");
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            _listener?.Stop();
            IsRunning = false;

            foreach (var client in _clients.Values.ToList())
            {
                try { client.Close(); } catch { }
            }
            _clients.Clear();

            foreach (var clientLock in _clientLocks.Values.ToList())
            {
                try { clientLock.Dispose(); } catch { }
            }
            _clientLocks.Clear();

            Log("TCP 服务端已停止");
        }

        public void Send(string clientId, byte[] data)
        {
            if (!_clients.TryGetValue(clientId, out var client) || !client.Connected)
                return;

            var clientLock = _clientLocks.GetOrAdd(clientId, _ => new SemaphoreSlim(1, 1));
            var stream = client.GetStream();

            clientLock.Wait();
            try
            {
                stream.Write(data, 0, data.Length);
                Log($"[TX -> {clientId}] {BitConverter.ToString(data).Replace("-", " ")}");
            }
            finally
            {
                clientLock.Release();
            }
        }

        public async Task SendAsync(string clientId, byte[] data)
        {
            if (!_clients.TryGetValue(clientId, out var client) || !client.Connected)
                return;

            var clientLock = _clientLocks.GetOrAdd(clientId, _ => new SemaphoreSlim(1, 1));
            var stream = client.GetStream();

            await clientLock.WaitAsync();
            try
            {
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                Log($"[TX -> {clientId}] {BitConverter.ToString(data).Replace("-", " ")}");
            }
            finally
            {
                clientLock.Release();
            }
        }

        public void Broadcast(byte[] data)
        {
            foreach (var clientId in _clients.Keys.ToList())
            {
                Send(clientId, data);
            }
        }

        public void DisconnectClient(string clientId)
        {
            if (_clients.TryRemove(clientId, out var client))
            {
                try { client.Close(); } catch { }
            }

            if (_clientLocks.TryRemove(clientId, out var clientLock))
            {
                try { clientLock.Dispose(); } catch { }
            }

            ClientConnectionChanged?.Invoke(this, (clientId, false));
            Log($"客户端 {clientId} 已断开");
        }

        public List<string> GetConnectedClients()
        {
            return _clients.Keys.ToList();
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && IsRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

                    if (_clients.TryGetValue(endpoint, out var oldClient))
                    {
                        try { oldClient.Close(); } catch { }
                        _clients.TryRemove(endpoint, out _);
                    }

                    if (!_clients.TryAdd(endpoint, client))
                    {
                        try { client.Close(); } catch { }
                        return;
                    }

                    _clientLocks.GetOrAdd(endpoint, _ => new SemaphoreSlim(1, 1));

                    ClientConnectionChanged?.Invoke(this, (endpoint, true));
                    Log($"客户端 {endpoint} 已连接");

                    _ = Task.Run(() => ClientReceiveLoop(client, endpoint, token), token);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"接受连接异常: {ex.Message}");
                }
            }
        }

        private async Task ClientReceiveLoop(TcpClient client, string clientId, CancellationToken token)
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested && client.Connected)
                {
                    int read = 0;
                    try
                    {
                        read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    }
                    catch
                    {
                        read = 0;
                    }

                    if (read == 0)
                    {
                        break;
                    }

                    var data = new byte[read];
                    Array.Copy(buffer, data, read);
                    Log($"[RX <- {clientId}] {BitConverter.ToString(data).Replace("-", " ")}");
                    DataReceived?.Invoke(this, new TcpEventArgs(clientId, data));
                }
            }
            catch (Exception ex)
            {
                Log($"客户端 {clientId} 接收异常: {ex.Message}");
            }
            finally
            {
                _clients.TryRemove(clientId, out _);
                if (_clientLocks.TryRemove(clientId, out var clientLock))
                {
                    try { clientLock.Dispose(); } catch { }
                }
                try { client.Close(); } catch { }
                ClientConnectionChanged?.Invoke(this, (clientId, false));
                Log($"客户端 {clientId} 已断开");
            }
        }

        private void Log(string message)
        {
            LogAction?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] [TCP Server:{Port}] {message}");
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
                Stop();
                _asyncLock?.Dispose();
            }

            _disposed = true;
        }
    }
}
