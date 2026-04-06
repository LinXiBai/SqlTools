using System;
using System.Linq;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// Modbus RTU / ASCII 辅助类
    /// 提供报文组装和 CRC 校验
    /// </summary>
    public static class ModbusHelper
    {
        /// <summary>
        /// 构建 Modbus RTU 读保持寄存器报文 (功能码 03)
        /// </summary>
        public static byte[] BuildReadHoldingRegisters(byte slaveId, ushort startAddress, ushort count)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveId;
            frame[1] = 0x03;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(count >> 8);
            frame[5] = (byte)(count & 0xFF);
            ushort crc = CalculateCrc16(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);
            return frame;
        }

        /// <summary>
        /// 构建 Modbus RTU 写单个线圈报文 (功能码 05)
        /// </summary>
        public static byte[] BuildWriteSingleCoil(byte slaveId, ushort address, bool value)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveId;
            frame[1] = 0x05;
            frame[2] = (byte)(address >> 8);
            frame[3] = (byte)(address & 0xFF);
            frame[4] = value ? (byte)0xFF : (byte)0x00;
            frame[5] = 0x00;
            ushort crc = CalculateCrc16(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);
            return frame;
        }

        /// <summary>
        /// 验证 Modbus RTU 报文 CRC
        /// </summary>
        public static bool ValidateCrc(byte[] frame)
        {
            if (frame == null || frame.Length < 4) return false;
            ushort receivedCrc = (ushort)((frame[frame.Length - 1] << 8) | frame[frame.Length - 2]);
            ushort calcCrc = CalculateCrc16(frame, frame.Length - 2);
            return receivedCrc == calcCrc;
        }

        /// <summary>
        /// 计算 CRC16
        /// </summary>
        public static ushort CalculateCrc16(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }

        /// <summary>
        /// 将寄存器值转换为 float（ABCD 大端顺序）
        /// </summary>
        public static float RegistersToFloat(ushort high, ushort low)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(high >> 8);
            bytes[1] = (byte)(high & 0xFF);
            bytes[2] = (byte)(low >> 8);
            bytes[3] = (byte)(low & 0xFF);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}
