using System.IO.Ports;

namespace MeterRecorder
{
    /// <summary>
    /// 基于 <see cref="SerialPort"/> 的传输实现。串口在构造时创建，Open 时才真正打开。
    /// </summary>
    public sealed class SerialPortTransport : IModbusTransport
    {
        /// <summary>
        /// 单次阻塞读的默认等待时长。取较小值，让上层的总超时循环有机会检查总时限。
        /// </summary>
        private const int DefaultReadTimeoutMilliseconds = 100;

                /// <summary>
        /// 串口对象，构造时创建，Open 时才真正打开。
        /// </summary>
        private readonly SerialPort _serialPort;

        /// <summary>
        /// 按串口参数创建传输通道，此时串口尚未打开。
        /// </summary>
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public SerialPortTransport(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = DefaultReadTimeoutMilliseconds,
                WriteTimeout = 1000
            };
        }

        /// <summary>
        /// 串口是否已打开。
        /// </summary>
        public bool IsOpen
        {
            get { return _serialPort.IsOpen; }
        }

        /// <summary>
        /// 单次读取的阻塞时长，单位毫秒。
        /// </summary>
        public int ReadTimeout
        {
            get { return _serialPort.ReadTimeout; }
            set { _serialPort.ReadTimeout = value; }
        }

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
        /// 丢弃接收缓冲区里的残留字节。
        /// </summary>
        public void DiscardInBuffer()
        {
            _serialPort.DiscardInBuffer();
        }

        /// <summary>
        /// 发送字节。
        /// </summary>
        /// <param name="buffer">待发送的报文</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">字节数</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        /// <summary>
        /// 读取字节，单次读超时时抛出 <see cref="System.TimeoutException"/>。
        /// </summary>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="offset">写入起始位置</param>
        /// <param name="count">本次最多读取的字节数</param>
        public int Read(byte[] buffer, int offset, int count)
        {
            return _serialPort.Read(buffer, offset, count);
        }

        /// <summary>
        /// 释放串口资源。
        /// </summary>
        public void Dispose()
        {
            _serialPort.Dispose();
        }
    }
}
