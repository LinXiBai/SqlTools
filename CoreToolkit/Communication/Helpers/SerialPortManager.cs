using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using CoreToolkit.Communication.Helpers;
using CoreToolkit.Communication.Models;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 串口管理器：统一管理多个串口连接
    /// </summary>
    public class SerialPortManager : IDisposable
    {
        private readonly Dictionary<string, AdvancedSerialPort> _ports = new Dictionary<string, AdvancedSerialPort>(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new object();

        /// <summary>任意串口收到数据时触发</summary>
        public event EventHandler<SerialPortEventArgs> DataReceived;

        /// <summary>串口状态变化时触发</summary>
        public event EventHandler<(string portName, bool isOpen)> ConnectionStatusChanged;

        /// <summary>全局日志回调</summary>
        public Action<string> LogAction { get; set; }

        /// <summary>注册并打开串口</summary>
        public AdvancedSerialPort AddPort(string portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            lock (_lock)
            {
                if (_ports.TryGetValue(portName, out var existing))
                {
                    if (!existing.IsOpen)
                        existing.Open();
                    return existing;
                }

                var port = new AdvancedSerialPort(portName, baudRate, dataBits, parity, stopBits)
                {
                    LogAction = msg =>
                    {
                        LogAction?.Invoke(msg);
                    }
                };
                port.DataReceived += (s, e) => DataReceived?.Invoke(s, e);
                port.ConnectionStatusChanged += (s, isOpen) => ConnectionStatusChanged?.Invoke(s, (port.PortName, isOpen));
                port.Open();
                _ports[portName] = port;
                return port;
            }
        }

        /// <summary>获取已注册的串口</summary>
        public AdvancedSerialPort GetPort(string portName)
        {
            lock (_lock)
            {
                _ports.TryGetValue(portName, out var port);
                return port;
            }
        }

        /// <summary>向指定串口发送数据</summary>
        public void Write(string portName, byte[] data)
        {
            lock (_lock)
            {
                var port = GetPort(portName);
                if (port == null || !port.IsOpen)
                    throw new InvalidOperationException($"串口 {portName} 未打开或未注册。");
                port.Write(data, 0, data.Length);
            }
        }

        /// <summary>关闭并移除指定串口</summary>
        public void RemovePort(string portName)
        {
            lock (_lock)
            {
                if (_ports.TryGetValue(portName, out var port))
                {
                    port.Dispose();
                    _ports.Remove(portName);
                }
            }
        }

        /// <summary>获取所有已注册串口名称</summary>
        public string[] GetPortNames()
        {
            lock (_lock)
            {
                return _ports.Keys.ToArray();
            }
        }

        /// <summary>关闭所有串口</summary>
        public void CloseAll()
        {
            lock (_lock)
            {
                foreach (var port in _ports.Values)
                    port.Close();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var port in _ports.Values)
                    port.Dispose();
                _ports.Clear();
            }
        }
    }
}
