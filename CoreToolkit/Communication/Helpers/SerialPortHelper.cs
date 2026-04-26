using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using CoreToolkit.Communication.Core;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 基于 System.IO.Ports 的串口实现
    /// </summary>
    public class SerialPortHelper : ISerialPort
    {
        private SerialPort _serialPort;

        public string PortName => _serialPort?.PortName;
        public int BaudRate => _serialPort?.BaudRate ?? 0;
        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public SerialPortHelper(string portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public void Write(byte[] data, int offset, int count)
        {
            _serialPort.Write(data, offset, count);
        }

        public void WriteLine(string text)
        {
            _serialPort.WriteLine(text);
        }

        public byte[] Read(int count, int timeoutMs = 1000)
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
            return result;
        }

        public string ReadLine(int timeoutMs = 1000)
        {
            _serialPort.ReadTimeout = timeoutMs;
            return _serialPort.ReadLine();
        }

        public void ClearBuffers()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        public void Dispose()
        {
            Close();
            _serialPort?.Dispose();
        }
    }
}
