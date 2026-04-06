using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 串口数据帧解析器
    /// 支持：固定长度、结束符、头部+长度 三种模式
    /// </summary>
    public class SerialFrameParser
    {
        private readonly List<byte> _buffer = new List<byte>();
        private readonly object _lock = new object();

        /// <summary>最大缓冲区大小（64KB），防止内存无限增长</summary>
        public const int MaxBufferSize = 65536;

        /// <summary>解析模式</summary>
        public ParseMode Mode { get; set; } = ParseMode.FixedLength;

        /// <summary>固定长度模式：每帧字节数</summary>
        public int FixedLength { get; set; } = 1;

        /// <summary>结束符模式：帧尾标识字节数组</summary>
        public byte[] EndMarker { get; set; } = new byte[] { 0x0D, 0x0A };

        /// <summary>头部+长度模式：长度字段在头部中的偏移</summary>
        public int LengthFieldOffset { get; set; } = 1;

        /// <summary>头部+长度模式：长度字段占用的字节数（1 或 2）</summary>
        public int LengthFieldSize { get; set; } = 1;

        /// <summary>头部+长度模式：长度值是否包含头部本身的长度</summary>
        public bool LengthIncludesHeader { get; set; } = false;

        /// <summary>头部+长度模式：固定头部长度（长度字段前的字节数 + 长度字段本身）</summary>
        public int HeaderLength { get; set; } = 2;

        /// <summary>解析到完整帧时触发</summary>
        public event Action<byte[]> FrameReceived;

        /// <summary>发生错误时触发（如缓冲区溢出）</summary>
        public event Action<string> ErrorOccurred;

        /// <summary>向缓冲区写入新数据并尝试解析</summary>
        public void Feed(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            lock (_lock)
            {
                _buffer.AddRange(data);

                // 检查缓冲区大小，防止无限增长
                if (_buffer.Count > MaxBufferSize)
                {
                    _buffer.Clear();
                    ErrorOccurred?.Invoke($"缓冲区溢出（超过 {MaxBufferSize} 字节），已清空缓冲区");
                    return;
                }

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

        private void TryParse()
        {
            while (_buffer.Count > 0)
            {
                // 每次解析前检查缓冲区大小，防止无限增长
                if (_buffer.Count > MaxBufferSize)
                {
                    _buffer.Clear();
                    ErrorOccurred?.Invoke($"缓冲区溢出（超过 {MaxBufferSize} 字节），已清空缓冲区");
                    break;
                }

                byte[] frame = null;

                switch (Mode)
                {
                    case ParseMode.FixedLength:
                        frame = TryParseFixedLength();
                        break;
                    case ParseMode.EndMarker:
                        frame = TryParseEndMarker();
                        break;
                    case ParseMode.HeaderLength:
                        frame = TryParseHeaderLength();
                        break;
                }

                if (frame == null) break;
                FrameReceived?.Invoke(frame);
            }
        }

        private byte[] TryParseFixedLength()
        {
            if (_buffer.Count < FixedLength) return null;
            var frame = _buffer.Take(FixedLength).ToArray();
            _buffer.RemoveRange(0, FixedLength);
            return frame;
        }

        private byte[] TryParseEndMarker()
        {
            if (EndMarker == null || EndMarker.Length == 0) return null;
            for (int i = 0; i <= _buffer.Count - EndMarker.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < EndMarker.Length; j++)
                {
                    if (_buffer[i + j] != EndMarker[j])
                    {
                        match = false; break;
                    }
                }
                if (match)
                {
                    int frameLength = i + EndMarker.Length;
                    var frame = _buffer.Take(frameLength).ToArray();
                    _buffer.RemoveRange(0, frameLength);
                    return frame;
                }
            }
            return null;
        }

        private byte[] TryParseHeaderLength()
        {
            if (_buffer.Count < HeaderLength) return null;
            int payloadLength = 0;
            if (LengthFieldSize == 1)
                payloadLength = _buffer[LengthFieldOffset];
            else if (LengthFieldSize == 2)
                payloadLength = (_buffer[LengthFieldOffset] << 8) | _buffer[LengthFieldOffset + 1];

            int totalLength = LengthIncludesHeader ? payloadLength : (HeaderLength + payloadLength);
            if (totalLength <= 0 || _buffer.Count < totalLength) return null;

            var frame = _buffer.Take(totalLength).ToArray();
            _buffer.RemoveRange(0, totalLength);
            return frame;
        }
    }

    /// <summary>
    /// 串口帧解析模式
    /// </summary>
    public enum ParseMode
    {
        /// <summary>固定长度</summary>
        FixedLength,
        /// <summary>结束符</summary>
        EndMarker,
        /// <summary>头部+长度字段</summary>
        HeaderLength
    }
}
