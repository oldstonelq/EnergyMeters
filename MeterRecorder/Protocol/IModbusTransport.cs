using System;

namespace MeterRecorder
{
    /// <summary>
    /// Modbus 报文的传输通道。默认实现走串口；测试时可以换成内存实现，
    /// 这样组帧、校验与寄存器解析不必依赖真实硬件。
    /// </summary>
    public interface IModbusTransport : IDisposable
    {
        /// <summary>
        /// 通道是否已打开。
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 单次读取的阻塞时长，单位毫秒。
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// 打开通道。
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭通道。
        /// </summary>
        void Close();

        /// <summary>
        /// 丢弃接收缓冲区里的残留字节，避免上一帧污染本次应答。
        /// </summary>
        void DiscardInBuffer();

        /// <summary>
        /// 发送字节。
        /// </summary>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// 读取字节，返回实际读到的数量；超时由实现方以 <see cref="TimeoutException"/> 表示。
        /// </summary>
        int Read(byte[] buffer, int offset, int count);
    }
}
