using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// CRC 与组帧：用说明书里的示例帧锁死算法，改实现时能立刻发现偏差。
    /// </summary>
    public class ModbusCrcTests
    {
        /// <summary>
        /// 说明书示例：01 03 00 64 00 01 应得 C5 D5（低字节先发），
        /// 因此按字读出的校验值是 0xD5C5。
        /// </summary>
        [Fact]
        public void Compute_ManualExampleFrame_MatchesDatasheet()
        {
            byte[] frame = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };

            ushort crc = ModbusCrc.Compute(frame, frame.Length);

            // 说明书示例：01 03 00 64 00 01 -> C5 D5（低字节先发）
            Assert.Equal(0xD5C5, crc);
        }

        /// <summary>
        /// 组帧结果逐字节比对说明书里的 03H 读请求，含末尾两个 CRC 字节。
        /// </summary>
        [Fact]
        public void BuildReadRequest_ProducesManualFrame()
        {
            byte[] request = ModbusFrame.BuildReadRequest(0x01, 0x0064, 0x0001);

            Assert.Equal(new byte[] { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 }, request);
        }

        /// <summary>
        /// 改掉一个数据字节后校验必须失败，确认校验覆盖整帧而不只是末尾两字节。
        /// </summary>
        [Fact]
        public void Verify_DetectsCorruptedByte()
        {
            byte[] frame = ModbusFrame.BuildReadRequest(0x01, 0x0164, 0x0010);
            Assert.True(ModbusCrc.Verify(frame, frame.Length));

            frame[3] ^= 0xFF;

            Assert.False(ModbusCrc.Verify(frame, frame.Length));
        }
    }

    /// <summary>
    /// 03H 读取的应答处理：正常解析、异常应答、从站不符与 CRC 错误四条路径。
    /// </summary>
    public class ModbusRtuClientTests
    {
        /// <summary>
        /// 寄存器按高字节在前解析且顺序不能颠倒；同时确认请求按预置的从站、
        /// 起始地址与数量发出。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_ReturnsRegistersInOrder()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0064, 2, 0x000A, 0x000B);

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ushort[] registers = client.ReadHoldingRegisters(1, 0x0064, 2);

                Assert.Equal(new ushort[] { 10, 11 }, registers);
                Assert.Equal(new[] { "1:100:2" }, transport.Requests);
            }
        }

        /// <summary>
        /// 功能码最高位为 1 表示异常应答，第三字节是异常码，
        /// 应原样带进 ModbusException 供上层提示。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_ExceptionResponse_ThrowsWithCode()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .ExpectRaw(1, 0x0064, 1, FakeModbusTransport.BuildExceptionResponse(1, 0x02));

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Equal((byte?)0x02, error.ExceptionCode);
            }
        }

        /// <summary>
        /// 应答的从站地址与请求不一致时必须拒绝，避免总线上多台表计时读到别人的数据。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_SlaveMismatch_Throws()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .ExpectRaw(1, 0x0064, 1, FakeModbusTransport.BuildReadResponse(2, new ushort[] { 1 }));

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Contains("从站地址不匹配", error.Message);
            }
        }

        /// <summary>
        /// 应答帧被篡改后应报 CRC 错误，而不是把脏数据当成读数返回。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_CrcFailure_Throws()
        {
            byte[] frame = FakeModbusTransport.BuildReadResponse(1, new ushort[] { 5 });
            frame[3] ^= 0xFF;
            FakeModbusTransport transport = new FakeModbusTransport().ExpectRaw(1, 0x0064, 1, frame);

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Contains("CRC", error.Message);
            }
        }
    }
}
