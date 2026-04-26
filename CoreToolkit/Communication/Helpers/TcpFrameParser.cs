using System;
using System.Collections.Generic;
using System.Linq;
using CoreToolkit.Communication.Models;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// TCP 数据帧解析器：处理粘包/拆包，支持协议头+长度、结束符、固定长度三种模式
    /// </summary>
    public class TcpFrameParser
    {
        private readonly List<byte> _buffer = new List<byte>();
        private readonly object _lock = new object();

        /// <summary>最大缓冲区大小（64KB），超过将清空并触发错误</summary>
        public const int MaxBufferSize = 65536;

        /// <summary>解析模式</summary>
        public TcpParseMode Mode { get; set; } = TcpParseMode.HeaderLength;

        #region 固定长度模式

        /// <summary>固定长度模式下每帧字节数</summary>
        public int FixedLength { get; set; } = 1;

        #endregion

        #region 结束符模式

        /// <summary>结束符模式下的帧尾标识</summary>
        public byte[] EndMarker { get; set; } = new byte[] { 0x0D, 0x0A };

        #endregion

        #region 协议头+长度模式

        /// <summary>帧头标识字节数组（如 0xAA 0xBB）</summary>
        public byte[] FrameHeader { get; set; } = new byte[] { 0xAA, 0xBB };

        /// <summary>头部长度（包含帧头+功能码+长度字段等固定前缀）</summary>
        public int HeaderLength { get; set; } = 4;

        /// <summary>长度字段在头部中的偏移量（从帧头开始算）</summary>
        public int LengthFieldOffset { get; set; } = 2;

        /// <summary>长度字段占用的字节数（1、2 或 4）</summary>
        public int LengthFieldSize { get; set; } = 2;

        /// <summary>长度值是否包含头部长度本身</summary>
        public bool LengthIncludesHeader { get; set; } = false;

        /// <summary>长度字段字节序：true=大端，false=小端</summary>
        public bool LengthBigEndian { get; set; } = true;

        /// <summary>帧尾校验码长度（如 CRC16=2，XOR=1，无校验=0）</summary>
        public int ChecksumLength { get; set; } = 0;

        /// <summary>心跳帧的命令字（功能码），用于识别心跳</summary>
        public byte HeartbeatCommand { get; set; } = 0x00;

        /// <summary>心跳帧的完整数据特征（可选，优先级高于 Command）</summary>
        public byte[] HeartbeatPattern { get; set; }

        #endregion

        /// <summary>解析到完整帧时触发</summary>
        public event Action<ProtocolFrame> FrameReceived;

        /// <summary>收到心跳帧时单独触发</summary>
        public event Action<ProtocolFrame> HeartbeatReceived;

        /// <summary>发生错误时触发（如缓冲区溢出）</summary>
        public event Action<string> ErrorOccurred;

        /// <summary>向缓冲区写入新数据并尝试解析</summary>
        public void Feed(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            lock (_lock)
            {
                // 检查缓冲区是否会超过最大限制
                if (_buffer.Count + data.Length > MaxBufferSize)
                {
                    _buffer.Clear();
                    ErrorOccurred?.Invoke($"Buffer overflow: data size ({data.Length}) exceeds remaining capacity. Buffer cleared.");
                }
                _buffer.AddRange(data);
                TryParse();
            }
        }

        /// <summary>清空缓冲区</summary>
        public void Clear()
        {
            lock (_lock)
            {
                _buffer.Clear();
            }
        }

        /// <summary>获取当前缓冲区中未解析的数据</summary>
        public byte[] GetPendingData()
        {
            lock (_lock)
            {
                return _buffer.ToArray();
            }
        }

        private void TryParse()
        {
            while (_buffer.Count > 0)
            {
                // 每次解析前检查缓冲区大小，防止无限增长
                if (_buffer.Count > MaxBufferSize)
                {
                    _buffer.Clear();
                    ErrorOccurred?.Invoke($"Buffer overflow: buffer size ({_buffer.Count}) exceeds max size ({MaxBufferSize}). Buffer cleared.");
                    break;
                }

                ProtocolFrame frame = null;

                switch (Mode)
                {
                    case TcpParseMode.FixedLength:
                        frame = TryParseFixedLength();
                        break;
                    case TcpParseMode.EndMarker:
                        frame = TryParseEndMarker();
                        break;
                    case TcpParseMode.HeaderLength:
                        frame = TryParseHeaderLength();
                        break;
                }

                if (frame == null) break;

                if (frame.IsHeartbeat)
                    HeartbeatReceived?.Invoke(frame);
                else
                    FrameReceived?.Invoke(frame);
            }
        }

        private ProtocolFrame TryParseFixedLength()
        {
            if (_buffer.Count < FixedLength) return null;
            var raw = _buffer.Take(FixedLength).ToArray();
            _buffer.RemoveRange(0, FixedLength);

            return new ProtocolFrame
            {
                RawData = raw,
                Length = FixedLength,
                IsHeartbeat = CheckIsHeartbeat(raw)
            };
        }

        private ProtocolFrame TryParseEndMarker()
        {
            if (EndMarker == null || EndMarker.Length == 0) return null;
            for (int i = 0; i <= _buffer.Count - EndMarker.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < EndMarker.Length; j++)
                {
                    if (_buffer[i + j] != EndMarker[j])
                    { match = false; break; }
                }
                if (match)
                {
                    int frameLen = i + EndMarker.Length;
                    var raw = _buffer.Take(frameLen).ToArray();
                    _buffer.RemoveRange(0, frameLen);
                    return new ProtocolFrame
                    {
                        RawData = raw,
                        Length = frameLen,
                        IsHeartbeat = CheckIsHeartbeat(raw)
                    };
                }
            }
            return null;
        }

        private ProtocolFrame TryParseHeaderLength()
        {
            if (FrameHeader == null || FrameHeader.Length == 0) return null;

            // 查找帧头位置
            int headerIndex = FindHeaderIndex();
            if (headerIndex < 0) return null;

            // 丢弃帧头之前的垃圾数据
            if (headerIndex > 0)
            {
                _buffer.RemoveRange(0, headerIndex);
            }

            // 至少需要头部完整
            if (_buffer.Count < HeaderLength) return null;

            // 读取长度字段
            int payloadLength = ReadLengthField();
            if (payloadLength < 0) return null;

            int totalLength = LengthIncludesHeader
                ? payloadLength
                : HeaderLength + payloadLength + ChecksumLength;

            if (totalLength <= 0 || _buffer.Count < totalLength) return null;

            var raw = _buffer.Take(totalLength).ToArray();
            _buffer.RemoveRange(0, totalLength);

            var frame = BuildFrameFromRaw(raw);
            return frame;
        }

        private int FindHeaderIndex()
        {
            for (int i = 0; i <= _buffer.Count - FrameHeader.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < FrameHeader.Length; j++)
                {
                    if (_buffer[i + j] != FrameHeader[j])
                    { match = false; break; }
                }
                if (match) return i;
            }
            return -1;
        }

        private int ReadLengthField()
        {
            try
            {
                if (LengthFieldSize == 1)
                    return _buffer[LengthFieldOffset];

                if (LengthFieldSize == 2)
                {
                    byte b1 = _buffer[LengthFieldOffset];
                    byte b2 = _buffer[LengthFieldOffset + 1];
                    return LengthBigEndian ? (b1 << 8 | b2) : (b2 << 8 | b1);
                }

                if (LengthFieldSize == 4)
                {
                    byte[] lenBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                        lenBytes[i] = _buffer[LengthFieldOffset + i];
                    if (LengthBigEndian) Array.Reverse(lenBytes);
                    return BitConverter.ToInt32(lenBytes, 0);
                }
            }
            catch { }
            return -1;
        }

        private ProtocolFrame BuildFrameFromRaw(byte[] raw)
        {
            var frame = new ProtocolFrame
            {
                RawData = raw,
                Length = raw.Length,
                Header = FrameHeader?.ToArray()
            };

            // 提取命令字（假设命令字紧跟在帧头后面）
            if (raw.Length > FrameHeader?.Length)
            {
                frame.Command = raw[FrameHeader.Length];
            }

            // 提取数据体
            int payloadStart = HeaderLength;
            int payloadLength = raw.Length - ChecksumLength - HeaderLength;

            if (payloadLength > 0 && payloadStart + payloadLength <= raw.Length)
            {
                frame.Payload = new byte[payloadLength];
                Array.Copy(raw, payloadStart, frame.Payload, 0, payloadLength);
            }

            // 提取校验码
            if (ChecksumLength > 0 && raw.Length >= ChecksumLength)
            {
                frame.Checksum = new byte[ChecksumLength];
                Array.Copy(raw, raw.Length - ChecksumLength, frame.Checksum, 0, ChecksumLength);
            }

            frame.IsHeartbeat = CheckIsHeartbeat(raw, frame.Command);
            return frame;
        }

        private bool CheckIsHeartbeat(byte[] raw, byte command = 0x00)
        {
            if (HeartbeatPattern != null && HeartbeatPattern.Length > 0)
            {
                if (raw.Length >= HeartbeatPattern.Length)
                {
                    for (int i = 0; i < HeartbeatPattern.Length; i++)
                        if (raw[i] != HeartbeatPattern[i]) return false;
                    return true;
                }
                return false;
            }

            if (HeartbeatCommand != 0x00 && command == HeartbeatCommand)
                return true;

            return false;
        }
    }

    /// <summary>
    /// TCP 帧解析模式
    /// </summary>
    public enum TcpParseMode
    {
        /// <summary>固定长度</summary>
        FixedLength,
        /// <summary>结束符</summary>
        EndMarker,
        /// <summary>协议头+长度字段</summary>
        HeaderLength
    }
}
