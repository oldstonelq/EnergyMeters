using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    public class ModbusCrcTests
    {
        [Fact]
        public void Compute_ManualExampleFrame_MatchesDatasheet()
        {
            byte[] frame = { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01 };

            ushort crc = ModbusCrc.Compute(frame, frame.Length);

            // 说明书示例：01 03 00 64 00 01 -> C5 D5（低字节先发）
            Assert.Equal(0xD5C5, crc);
        }

        [Fact]
        public void BuildReadRequest_ProducesManualFrame()
        {
            byte[] request = ModbusFrame.BuildReadRequest(0x01, 0x0064, 0x0001);

            Assert.Equal(new byte[] { 0x01, 0x03, 0x00, 0x64, 0x00, 0x01, 0xC5, 0xD5 }, request);
        }

        [Fact]
        public void Verify_DetectsCorruptedByte()
        {
            byte[] frame = ModbusFrame.BuildReadRequest(0x01, 0x0164, 0x0010);
            Assert.True(ModbusCrc.Verify(frame, frame.Length));

            frame[3] ^= 0xFF;

            Assert.False(ModbusCrc.Verify(frame, frame.Length));
        }
    }

    public class ModbusRtuClientTests
    {
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
