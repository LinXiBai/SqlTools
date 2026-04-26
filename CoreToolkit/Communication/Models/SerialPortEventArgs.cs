using System;

namespace CoreToolkit.Communication.Models
{
    /// <summary>
    /// 串口数据接收事件参数
    /// </summary>
    public class SerialPortEventArgs : EventArgs
    {
        public string PortName { get; }
        public byte[] Data { get; }
        public string Text { get; }
        public DateTime ReceiveTime { get; }

        public SerialPortEventArgs(string portName, byte[] data, string text = null)
        {
            PortName = portName;
            Data = data;
            Text = text;
            ReceiveTime = DateTime.Now;
        }
    }
}
