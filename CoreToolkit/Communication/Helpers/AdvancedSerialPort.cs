using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using CoreToolkit.Communication.Core;
using CoreToolkit.Communication.Models;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 增强版串口：支持事件驱动接收、日志输出、自动清空缓冲区
    /// </summary>
    public class AdvancedSerialPort : ISerialPort
    {
        private SerialPort _serialPort;
        private readonly StringBuilder _receiveBuffer = new StringBuilder();
        private readonly object _lock = new object();
        private bool _disposed;

        public string PortName => _serialPort?.PortName;
        public int BaudRate => _serialPort?.BaudRate ?? 0;
        public bool IsOpen => _serialPort?.IsOpen ?? false;

        /// <summary>接收到数据时触发</summary>
        public event EventHandler<SerialPortEventArgs> DataReceived;

        /// <summary>串口打开/关闭状态变化时触发</summary>
        public event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>日志输出</summary>
        public Action<string> LogAction { get; set; }

        public AdvancedSerialPort(string portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                ReceivedBytesThreshold = 1
            };
            _serialPort.DataReceived += OnDataReceived;
        }

        public void Open()
        {
            if (_serialPort.IsOpen) return;
            _serialPort.Open();
            Log($"串口 {PortName} 已打开，波特率 {BaudRate}");
            ConnectionStatusChanged?.Invoke(this, true);
        }

        public void Close()
        {
            if (!_serialPort.IsOpen) return;
            _serialPort.Close();
            Log($"串口 {PortName} 已关闭");
            ConnectionStatusChanged?.Invoke(this, false);
        }

        public void Write(byte[] data, int offset, int count)
        {
            lock (_lock)
            {
                _serialPort.Write(data, offset, count);
                Log($"[TX] {BitConverter.ToString(data, offset, count).Replace("-", " ")}");
            }
        }

        public void WriteLine(string text)
        {
            lock (_lock)
            {
                _serialPort.WriteLine(text);
                Log($"[TX] {text}");
            }
        }

        public byte[] Read(int count, int timeoutMs = 1000)
        {
            int originalTimeout = _serialPort.ReadTimeout;
            try
            {
                _serialPort.ReadTimeout = timeoutMs;
                var buffer = new byte[count];
                int read = 0;
                while (read < count)
                {
                    int n = _serialPort.Read(buffer, read, count - read);
                    read += n;
                    if (n == 0) break;
                }
                var result = new byte[read];
                Array.Copy(buffer, result, read);
                if (read > 0)
                    Log($"[RX-Read] {BitConverter.ToString(result).Replace("-", " ")}");
                return result;
            }
            finally
            {
                _serialPort.ReadTimeout = originalTimeout;
            }
        }

        public string ReadLine(int timeoutMs = 1000)
        {
            int originalTimeout = _serialPort.ReadTimeout;
            try
            {
                _serialPort.ReadTimeout = timeoutMs;
                var line = _serialPort.ReadLine();
                Log($"[RX-Line] {line}");
                return line;
            }
            finally
            {
                _serialPort.ReadTimeout = originalTimeout;
            }
        }

        public void ClearBuffers()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _receiveBuffer.Clear();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int count = _serialPort.BytesToRead;
                if (count <= 0) return;
                var buffer = new byte[count];
                _serialPort.Read(buffer, 0, count);
                Log($"[RX-Event] {BitConverter.ToString(buffer).Replace("-", " ")}");
                DataReceived?.Invoke(this, new SerialPortEventArgs(PortName, buffer));
            }
            catch (Exception ex)
            {
                Log($"[RX-Error] {ex.Message}");
            }
        }

        private void Log(string message)
        {
            LogAction?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] [{PortName}] {message}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                    if (_serialPort != null)
                    {
                        _serialPort.DataReceived -= OnDataReceived;
                        _serialPort.Dispose();
                        _serialPort = null;
                    }
                }
                _disposed = true;
            }
        }
    }
}
