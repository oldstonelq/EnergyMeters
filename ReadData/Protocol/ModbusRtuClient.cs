using System;
using System.Diagnostics;
using System.IO.Ports;

namespace ReadDataSoftware
{
    /// <summary>
    /// Modbus RTU 主站，仅实现本软件用到的 03H 读保持寄存器。
    /// 负责组帧、CRC 校验、超时控制和帧合法性检查，数值缩放由各表型自行处理。
    /// </summary>
    public sealed class ModbusRtuClient : IDisposable
    {
        /// <summary>
        /// 读保持寄存器功能码。
        /// </summary>
        private const byte ReadHoldingRegistersFunctionCode = 0x03;
        /// <summary>
        /// 单次 03H 请求允许的最大寄存器数量。
        /// </summary>
        private const ushort MaxRegistersPerRequest = 125;
        /// <summary>
        /// 串口单次阻塞读的等待时长，用于让上层超时循环有机会检查总时限。
        /// </summary>
        private const int PortReadSliceMilliseconds = 100;
        /// <summary>
        /// 默认应答超时。9600bps 下一帧不足 40ms，500ms 已足够宽裕。
        /// </summary>
        private const int DefaultResponseTimeoutMilliseconds = 500;

        /// <summary>
        /// 串口对象，构造时创建，Open 时才真正打开。
        /// </summary>
        private readonly SerialPort _serialPort;
        /// <summary>
        /// 是否已释放，避免重复 Dispose。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建 Modbus 主站实例，串口尚未打开。
        /// </summary>
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public ModbusRtuClient(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = PortReadSliceMilliseconds,
                WriteTimeout = 1000
            };
            ResponseTimeoutMilliseconds = DefaultResponseTimeoutMilliseconds;
        }

        /// <summary>
        /// 串口是否已打开。
        /// </summary>
        public bool IsOpen
        {
            get { return _serialPort.IsOpen; }
        }

        /// <summary>
        /// 等待从站应答的总时长，单位毫秒。
        /// </summary>
        public int ResponseTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 打开串口；已打开时不做任何处理。
        /// </summary>
        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        /// <summary>
        /// 关闭串口；已关闭时不做任何处理。
        /// </summary>
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// 读取保持寄存器。
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">起始寄存器地址</param>
        /// <param name="quantity">寄存器数量</param>
        /// <returns>按报文顺序排列的寄存器值</returns>
        /// <exception cref="ModbusException">超时、帧校验失败或从站返回异常应答</exception>
        public ushort[] ReadHoldingRegisters(byte slaveAddress, ushort startAddress, ushort quantity)
        {
            if (quantity == 0 || quantity > MaxRegistersPerRequest)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity),
                    string.Format("单次读取寄存器数量须在 1~{0} 之间，实际为 {1}。", MaxRegistersPerRequest, quantity));
            }

            byte[] request = BuildReadHoldingRegistersRequest(slaveAddress, startAddress, quantity);

            _serialPort.DiscardInBuffer();
            _serialPort.Write(request, 0, request.Length);

            byte[] header = new byte[3];
            ReadExactly(header, 0, header.Length, slaveAddress);

            if (header[0] != slaveAddress)
            {
                throw new ModbusException(string.Format(
                    "应答从站地址不匹配：请求 {0}，收到 {1}。", slaveAddress, header[0]));
            }

            // 功能码最高位置 1 表示异常应答，报文为 地址+功能码+异常码+CRC(2)。
            if ((header[1] & 0x80) != 0)
            {
                byte[] exceptionTail = new byte[2];
                ReadExactly(exceptionTail, 0, exceptionTail.Length, slaveAddress);

                byte[] exceptionFrame = { header[0], header[1], header[2], exceptionTail[0], exceptionTail[1] };
                ValidateCrc(exceptionFrame, exceptionFrame.Length);

                throw new ModbusException(header[2], slaveAddress);
            }

            if (header[1] != ReadHoldingRegistersFunctionCode)
            {
                throw new ModbusException(string.Format(
                    "应答功能码不匹配：期望 0x03，收到 0x{0:X2}。", header[1]));
            }

            int byteCount = header[2];
            if (byteCount != quantity * 2)
            {
                throw new ModbusException(string.Format(
                    "应答数据长度不符：请求 {0} 个寄存器（{1} 字节），从站声明 {2} 字节。",
                    quantity, quantity * 2, byteCount));
            }

            // 剩余部分为 数据区 + CRC(2)。
            byte[] payload = new byte[byteCount + 2];
            ReadExactly(payload, 0, payload.Length, slaveAddress);

            byte[] frame = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, frame, 0, header.Length);
            Array.Copy(payload, 0, frame, header.Length, payload.Length);
            ValidateCrc(frame, frame.Length);

            ushort[] registers = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                registers[i] = (ushort)((payload[i * 2] << 8) | payload[i * 2 + 1]);
            }
            return registers;
        }

        /// <summary>
        /// 释放串口资源。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _serialPort.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// 组装 03H 请求帧：从站地址 + 功能码 + 起始地址 + 寄存器数 + CRC。
        /// </summary>
        private static byte[] BuildReadHoldingRegistersRequest(byte slaveAddress, ushort startAddress, ushort quantity)
        {
            byte[] request = new byte[8];
            request[0] = slaveAddress;
            request[1] = ReadHoldingRegistersFunctionCode;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(quantity >> 8);
            request[5] = (byte)quantity;

            ushort crc = CalculateCrc(request, 6);
            request[6] = (byte)crc;
            request[7] = (byte)(crc >> 8);
            return request;
        }

        /// <summary>
        /// 计算 CRC16/Modbus，返回值的低字节先发。
        /// </summary>
        private static ushort CalculateCrc(byte[] data, int length)
        {
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
        /// 校验整帧尾部的 CRC。
        /// </summary>
        private static void ValidateCrc(byte[] frame, int length)
        {
            ushort expected = CalculateCrc(frame, length - 2);
            ushort actual = (ushort)(frame[length - 2] | (frame[length - 1] << 8));
            if (expected != actual)
            {
                throw new ModbusException(string.Format(
                    "应答帧 CRC 校验失败：计算值 0x{0:X4}，报文值 0x{1:X4}。", expected, actual));
            }
        }

        /// <summary>
        /// 读满指定字节数，超过总时限则抛出超时异常。
        /// </summary>
        private void ReadExactly(byte[] buffer, int offset, int count, byte slaveAddress)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int received = 0;
            while (received < count)
            {
                if (stopwatch.ElapsedMilliseconds > ResponseTimeoutMilliseconds)
                {
                    throw new ModbusException(string.Format(
                        "等待从站 {0} 应答超时（{1} ms），已接收 {2}/{3} 字节。",
                        slaveAddress, ResponseTimeoutMilliseconds, received, count));
                }

                try
                {
                    received += _serialPort.Read(buffer, offset + received, count - received);
                }
                catch (TimeoutException)
                {
                    // 单次读超时属正常现象，由上面的总时限判定是否放弃。
                }
            }
        }
    }
}
