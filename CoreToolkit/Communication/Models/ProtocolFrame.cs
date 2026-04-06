using System;

namespace CoreToolkit.Communication.Models
{
    /// <summary>
    /// 协议帧模型（通用协议头结构）
    /// </summary>
    public class ProtocolFrame
    {
        /// <summary>帧头标识（如 0xAA 0xBB）</summary>
        public byte[] Header { get; set; }

        /// <summary>功能码/命令字</summary>
        public byte Command { get; set; }

        /// <summary>数据体长度</summary>
        public int Length { get; set; }

        /// <summary>数据体</summary>
        public byte[] Payload { get; set; }

        /// <summary>校验码（如 CRC、XOR、SumCheck）</summary>
        public byte[] Checksum { get; set; }

        /// <summary>是否为心跳帧</summary>
        public bool IsHeartbeat { get; set; }

        /// <summary>完整帧字节数组</summary>
        public byte[] RawData { get; set; }

        /// <summary>获取数据体文本表示</summary>
        public string PayloadText => Payload != null ? System.Text.Encoding.ASCII.GetString(Payload) : "";

        public override string ToString()
        {
            string headerStr = Header != null ? BitConverter.ToString(Header).Replace("-", " ") : "";
            string payloadStr = Payload != null ? BitConverter.ToString(Payload).Replace("-", " ") : "";
            string checksumStr = Checksum != null ? BitConverter.ToString(Checksum).Replace("-", " ") : "";
            string hbFlag = IsHeartbeat ? " [HEARTBEAT]" : "";
            return $"[Header={headerStr}] [Cmd=0x{Command:X2}] [Len={Length}] [Payload={payloadStr}] [Checksum={checksumStr}]{hbFlag}";
        }
    }
}
