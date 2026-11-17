using System;
using System.Collections.Generic;
using MeterRecorder;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 内存版 Modbus 通道：按「从站 + 起始地址 + 数量」返回预置应答，
    /// 让协议与表计的解析逻辑可以在没有串口的情况下被测试。
    /// </summary>
    internal sealed class FakeModbusTransport : IModbusTransport
    {
        /// <summary>
        /// 按「从站:起始地址:数量」预置的寄存器值，收到请求时据此组装正常应答。
        /// </summary>
        private readonly Dictionary<string, ushort[]> _registers = new Dictionary<string, ushort[]>();
        /// <summary>
        /// 同键预置的整帧应答，优先级高于寄存器值，用来构造异常应答或坏帧。
        /// </summary>
        private readonly Dictionary<string, byte[]> _rawFrames = new Dictionary<string, byte[]>();
        /// <summary>
        /// 当前待读的应答帧。每次写入请求都重新装填，模拟串口接收缓冲。
        /// </summary>
        private byte[] _pending = new byte[0];
        /// <summary>
        /// 已经从 _pending 读走的字节数。
        /// </summary>
        private int _pendingOffset;
        /// <summary>
        /// _pending 的总长度，与偏移一起判断这一帧是否读完。
        /// </summary>
        private int _pendingLength;

        /// <summary>
        /// 是否已打开。内存通道只记录状态，不做实际动作。
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// 读超时，单位毫秒。内存通道不会真的等待，保留它是为了满足接口约定。
        /// </summary>
        public int ReadTimeout { get; set; }

        /// <summary>
        /// 依次收到的请求，便于断言读取顺序。
        /// </summary>
        public List<string> Requests { get; } = new List<string>();

        /// <summary>
        /// 预置一个正常应答，键由从站、起始地址与寄存器数量三个要素决定。
        /// 同一请求被读多次时返回同一份数据。
        /// </summary>
        /// <param name="slave">从站地址</param>
        /// <param name="start">起始寄存器地址</param>
        /// <param name="quantity">寄存器数量</param>
        /// <param name="registers">按顺序应答的寄存器值</param>
        /// <returns>自身，便于链式预置多段数据</returns>
        public FakeModbusTransport Expect(byte slave, ushort start, ushort quantity, params ushort[] registers)
        {
            _registers[Key(slave, start, quantity)] = registers;
            return this;
        }

        /// <summary>
        /// 预置一个整帧应答，用于构造异常应答、CRC 错误的帧或从站地址不符的帧。
        /// 整帧优先于 Expect 预置的寄存器值。
        /// </summary>
        /// <param name="slave">从站地址</param>
        /// <param name="start">起始寄存器地址</param>
        /// <param name="quantity">寄存器数量</param>
        /// <param name="frame">要原样返回的完整应答帧</param>
        /// <returns>自身，便于链式预置多段数据</returns>
        public FakeModbusTransport ExpectRaw(byte slave, ushort start, ushort quantity, byte[] frame)
        {
            _rawFrames[Key(slave, start, quantity)] = frame;
            return this;
        }

        /// <summary>
        /// 打开通道。内存实现只置状态位。
        /// </summary>
        public void Open()
        {
            IsOpen = true;
        }

        /// <summary>
        /// 关闭通道。内存实现只清状态位。
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }

        /// <summary>
        /// 丢弃尚未读走的应答，模拟真实串口清空接收缓冲，避免上一帧的残留被当成本帧。
        /// </summary>
        public void DiscardInBuffer()
        {
            _pending = new byte[0];
            _pendingOffset = 0;
            _pendingLength = 0;
        }

        /// <summary>
        /// 接收一帧 03H 请求：解析出从站、起始地址与数量，按预置内容准备应答。
        /// 没有对应预置时直接抛异常，让漏配的用例当场失败，而不是等读超时。
        /// </summary>
        /// <param name="buffer">请求字节数组</param>
        /// <param name="offset">请求起始位置</param>
        /// <param name="count">请求字节数</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            byte slave = buffer[offset];
            ushort start = (ushort)((buffer[offset + 2] << 8) | buffer[offset + 3]);
            ushort quantity = (ushort)((buffer[offset + 4] << 8) | buffer[offset + 5]);
            string key = Key(slave, start, quantity);
            Requests.Add(key);

            byte[] raw;
            if (_rawFrames.TryGetValue(key, out raw))
            {
                _pending = raw;
            }
            else
            {
                ushort[] registers;
                if (!_registers.TryGetValue(key, out registers))
                {
                    throw new InvalidOperationException("测试没有为该请求准备应答：" + key);
                }
                _pending = BuildReadResponse(slave, registers);
            }
            _pendingOffset = 0;
            _pendingLength = _pending.Length;
        }

        /// <summary>
        /// 按调用方给的缓冲区大小分段返回应答；读完后再读会抛超时，
        /// 与真实串口在帧尾无数据时的表现一致。
        /// </summary>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="offset">写入起始位置</param>
        /// <param name="count">最多可读的字节数</param>
        /// <returns>实际读到的字节数</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (_pendingOffset >= _pendingLength)
            {
                throw new TimeoutException("没有更多待读数据");
            }
            int length = Math.Min(count, _pendingLength - _pendingOffset);
            Array.Copy(_pending, _pendingOffset, buffer, offset, length);
            _pendingOffset += length;
            return length;
        }

        /// <summary>
        /// 释放通道。内存实现只清空待读数据。
        /// </summary>
        public void Dispose()
        {
            _pending = new byte[0];
            _pendingLength = 0;
        }

        /// <summary>
        /// 组装 03H 正常应答帧：从站、功能码、字节数、寄存器值，末尾补 CRC。
        /// </summary>
        /// <param name="slave">从站地址</param>
        /// <param name="registers">应答的寄存器值</param>
        /// <returns>完整应答帧</returns>
        public static byte[] BuildReadResponse(byte slave, ushort[] registers)
        {
            int byteCount = registers.Length * 2;
            byte[] frame = new byte[3 + byteCount + 2];
            frame[0] = slave;
            frame[1] = ModbusFrame.ReadHoldingRegistersFunctionCode;
            frame[2] = (byte)byteCount;
            for (int i = 0; i < registers.Length; i++)
            {
                frame[3 + i * 2] = (byte)(registers[i] >> 8);
                frame[4 + i * 2] = (byte)registers[i];
            }
            ModbusCrc.WriteTo(frame, frame.Length - 2);
            return frame;
        }

        /// <summary>
        /// 组装异常应答帧：功能码最高位置 1（03H 变 83H），第三字节为异常码。
        /// </summary>
        /// <param name="slave">从站地址</param>
        /// <param name="exceptionCode">要返回的异常码</param>
        /// <returns>完整应答帧</returns>
        public static byte[] BuildExceptionResponse(byte slave, byte exceptionCode)
        {
            byte[] frame = { slave, 0x83, exceptionCode, 0, 0 };
            ModbusCrc.WriteTo(frame, 3);
            return frame;
        }

        /// <summary>
        /// 把从站、起始地址与数量拼成字典键。三个要素都参与，
        /// 同一从站读取相邻区段时不会互相覆盖。
        /// </summary>
        private static string Key(byte slave, ushort start, ushort quantity)
        {
            return slave + ":" + start + ":" + quantity;
        }
    }
}
