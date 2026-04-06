using System;

namespace CoreToolkit.Communication.Models
{
    /// <summary>
    /// TCP 数据接收事件参数
    /// </summary>
    public class TcpEventArgs : EventArgs
    {
        public string RemoteEndPoint { get; }
        public byte[] Data { get; }
        public string Text { get; }
        public DateTime ReceiveTime { get; }

        public TcpEventArgs(string remoteEndPoint, byte[] data, string text = null)
        {
            RemoteEndPoint = remoteEndPoint;
            Data = data;
            Text = text;
            ReceiveTime = DateTime.Now;
        }
    }
}
