using System;
using System.Diagnostics;
using System.IO.Ports;

namespace MeterRecorder
{
    /// <summary>
    /// Modbus RTU 主站，仅实现本软件用到的 03H 读保持寄存器。
    /// 负责组帧、CRC 校验、超时控制和帧合法性检查，数值缩放由各表型自行处理。
    /// </summary>
    public sealed class ModbusRtuClient : IDisposable
    {
        /// <summary>
        /// 单次 03H 请求允许的最大寄存器数量。
        /// </summary>
        private const ushort MaxRegistersPerRequest = 125;
        /// <summary>
        /// 单次阻塞读的等待时长，用于让上层超时循环有机会检查总时限。
        /// </summary>
        private const int PortReadSliceMilliseconds = 100;
        /// <summary>
        /// 默认应答超时。9600bps 下一帧不足 40ms，500ms 已足够宽裕。
        /// </summary>
        private const int DefaultResponseTimeoutMilliseconds = 500;

        /// <summary>
        /// 报文传输通道。默认是串口实现，测试时可注入内存实现。
        /// </summary>
        private readonly IModbusTransport _transport;
        /// <summary>
        /// 是否已释放，避免重复 Dispose。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 用指定的传输通道创建 Modbus 主站。
        /// </summary>
        /// <param name="transport">报文传输通道</param>
        public ModbusRtuClient(IModbusTransport transport)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }
            _transport = transport;
            _transport.ReadTimeout = PortReadSliceMilliseconds;
            ResponseTimeoutMilliseconds = DefaultResponseTimeoutMilliseconds;
        }

        /// <summary>
        /// 按串口参数创建 Modbus 主站，串口尚未打开。
        /// </summary>
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public ModbusRtuClient(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
            : this(new SerialPortTransport(portName, baudRate, dataBits, stopBits, parity))
        {
        }

        /// <summary>
        /// 通道是否已打开。
        /// </summary>
        public bool IsOpen
        {
            get { return _transport.IsOpen; }
        }

        /// <summary>
        /// 等待从站应答的总时长，单位毫秒。
        /// </summary>
        public int ResponseTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 打开通道；已打开时不做任何处理。
        /// </summary>
        public void Open()
        {
            _transport.Open();
        }

        /// <summary>
        /// 关闭通道；已关闭时不做任何处理。
        /// </summary>
        public void Close()
        {
            _transport.Close();
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

            byte[] request = ModbusFrame.BuildReadRequest(slaveAddress, startAddress, quantity);

            _transport.DiscardInBuffer();
            _transport.Write(request, 0, request.Length);

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

            if (header[1] != ModbusFrame.ReadHoldingRegistersFunctionCode)
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
        /// 释放传输通道。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _transport.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// 校验整帧尾部的 CRC。
        /// </summary>
        private static void ValidateCrc(byte[] frame, int length)
        {
            if (ModbusCrc.Verify(frame, length))
            {
                return;
            }
            ushort expected = ModbusCrc.Compute(frame, length - 2);
            ushort actual = (ushort)(frame[length - 2] | (frame[length - 1] << 8));
            throw new ModbusException(string.Format(
                "应答帧 CRC 校验失败：计算值 0x{0:X4}，报文值 0x{1:X4}。", expected, actual));
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
                    received += _transport.Read(buffer, offset + received, count - received);
                }
                catch (TimeoutException)
                {
                    // 单次读超时属正常现象，由上面的总时限判定是否放弃。
                }
            }
        }
    }
}
