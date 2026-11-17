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
        private readonly Dictionary<string, ushort[]> _registers = new Dictionary<string, ushort[]>();
        private readonly Dictionary<string, byte[]> _rawFrames = new Dictionary<string, byte[]>();
        private byte[] _pending = new byte[0];
        private int _pendingOffset;
        private int _pendingLength;

        public bool IsOpen { get; private set; }

        public int ReadTimeout { get; set; }

        /// <summary>
        /// 依次收到的请求，便于断言读取顺序。
        /// </summary>
        public List<string> Requests { get; } = new List<string>();

        /// <summary>
        /// 预置一个正常应答。
        /// </summary>
        public FakeModbusTransport Expect(byte slave, ushort start, ushort quantity, params ushort[] registers)
        {
            _registers[Key(slave, start, quantity)] = registers;
            return this;
        }

        /// <summary>
        /// 预置一个整帧应答，用于构造异常应答或坏帧。
        /// </summary>
        public FakeModbusTransport ExpectRaw(byte slave, ushort start, ushort quantity, byte[] frame)
        {
            _rawFrames[Key(slave, start, quantity)] = frame;
            return this;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void DiscardInBuffer()
        {
            _pending = new byte[0];
            _pendingOffset = 0;
            _pendingLength = 0;
        }

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

        public void Dispose()
        {
            _pending = new byte[0];
            _pendingLength = 0;
        }

        /// <summary>
        /// 组装 03H 正常应答帧。
        /// </summary>
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
        /// 组装异常应答帧：功能码最高位置 1，第三字节为异常码。
        /// </summary>
        public static byte[] BuildExceptionResponse(byte slave, byte exceptionCode)
        {
            byte[] frame = { slave, 0x83, exceptionCode, 0, 0 };
            ModbusCrc.WriteTo(frame, 3);
            return frame;
        }

        private static string Key(byte slave, ushort start, ushort quantity)
        {
            return slave + ":" + start + ":" + quantity;
        }
    }
}
