using System;

namespace MeterRecorder
{
    /// <summary>
    /// CRC16/Modbus 的计算与校验。抽成静态方法，便于在没有串口的情况下做单测。
    /// </summary>
    public static class ModbusCrc
    {
        /// <summary>
        /// 计算 CRC16/Modbus，返回值的低字节应先行发送。
        /// </summary>
        /// <param name="data">报文缓冲</param>
        /// <param name="length">参与计算的字节数</param>
        public static ushort Compute(byte[] data, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (length < 0 || length > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length),
                    string.Format("参与计算的长度须在 0~{0} 之间，实际为 {1}。", data.Length, length));
            }

            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }

        /// <summary>
        /// 把 CRC 按低字节在前写入帧尾。
        /// </summary>
        /// <param name="frame">报文缓冲，须预留两个字节</param>
        /// <param name="dataLength">参与计算的字节数，也即 CRC 的起始下标</param>
        public static void WriteTo(byte[] frame, int dataLength)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            if (dataLength < 0 || dataLength + 2 > frame.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dataLength),
                    "写入 CRC 需要缓冲区至少预留两个字节。");
            }
            ushort crc = Compute(frame, dataLength);
            frame[dataLength] = (byte)crc;
            frame[dataLength + 1] = (byte)(crc >> 8);
        }

        /// <summary>
        /// 校验帧尾 CRC。
        /// </summary>
        /// <param name="frame">完整报文</param>
        /// <param name="length">报文长度，末两字节为 CRC</param>
        public static bool Verify(byte[] frame, int length)
        {
            if (frame == null || length < 3 || length > frame.Length)
            {
                return false;
            }
            ushort expected = Compute(frame, length - 2);
            ushort actual = (ushort)(frame[length - 2] | (frame[length - 1] << 8));
            return expected == actual;
        }
    }
}
