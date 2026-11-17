using System;
using System.IO.Ports;
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

        /// <summary>
        /// 入参为空或长度越界时应立刻报错：这类错误来自调用方，
        /// 静默算出一个错的校验值会让整条链路都读错。
        /// </summary>
        [Fact]
        public void Compute_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => ModbusCrc.Compute(null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ModbusCrc.Compute(new byte[2], 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => ModbusCrc.Compute(new byte[2], -1));
        }

        /// <summary>
        /// 缓冲区末尾没有两个空位时不能写 CRC，否则会越界破坏相邻数据。
        /// </summary>
        [Fact]
        public void WriteTo_BufferTooSmall_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ModbusCrc.WriteTo(null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ModbusCrc.WriteTo(new byte[2], 1));
        }

        /// <summary>
        /// 帧长不足 3 字节时无法容纳地址、功能码与校验，直接判为校验不通过。
        /// </summary>
        [Fact]
        public void Verify_TooShortFrame_ReturnsFalse()
        {
            Assert.False(ModbusCrc.Verify(null, 4));
            Assert.False(ModbusCrc.Verify(new byte[2], 2));
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

        /// <summary>
        /// 数量为 0 或超过 125 属于调用方错误，应在发报文之前就拒绝，
        /// 而不是组出非法帧去等超时。
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(126)]
        public void ReadHoldingRegisters_InvalidQuantity_Throws(int quantity)
        {
            FakeModbusTransport transport = new FakeModbusTransport();

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, (ushort)quantity));
                Assert.Empty(transport.Requests);
            }
        }

        /// <summary>
        /// 应答功能码不是 03H（例如从站回了 04H）时必须拒绝，
        /// 否则会把输入寄存器的数据当成保持寄存器写进样本。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_FunctionCodeMismatch_Throws()
        {
            byte[] frame = { 1, 0x04, 2, 0, 10, 0, 0 };
            ModbusCrc.WriteTo(frame, 5);

            FakeModbusTransport transport = new FakeModbusTransport().ExpectRaw(1, 0x0064, 1, frame);

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Contains("功能码不匹配", error.Message);
            }
        }

        /// <summary>
        /// 从站声明的字节数与请求数量不符时必须拒绝：数量对不上说明帧已经错位，
        /// 再按声明长度解析只会得到错位的寄存器值。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_ByteCountMismatch_Throws()
        {
            byte[] frame = { 1, 0x03, 4, 0, 10, 0, 11, 0, 0 };
            ModbusCrc.WriteTo(frame, 7);

            FakeModbusTransport transport = new FakeModbusTransport().ExpectRaw(1, 0x0064, 1, frame);

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Contains("数据长度不符", error.Message);
            }
        }

        /// <summary>
        /// 串口一帧数据会分多次到达，客户端必须循环读满再解析。
        /// 这里让通道每次只给 1 个字节，验证拼帧逻辑。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_FragmentedResponse_IsAssembled()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0064, 2, 0x000A, 0x000B);
            transport.MaxChunkSize = 1;

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                Assert.Equal(new ushort[] { 10, 11 }, client.ReadHoldingRegisters(1, 0x0064, 2));
            }
        }

        /// <summary>
        /// 从站不应答时应在总时限后抛超时异常，而不是无限等待把界面卡死。
        /// </summary>
        [Fact]
        public void ReadHoldingRegisters_NoResponse_ThrowsTimeout()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .ExpectRaw(1, 0x0064, 1, new byte[0]);

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                client.ResponseTimeoutMilliseconds = 20;

                ModbusException error = Assert.Throws<ModbusException>(
                    () => client.ReadHoldingRegisters(1, 0x0064, 1));

                Assert.Contains("超时", error.Message);
            }
        }

        /// <summary>
        /// 未提供通道属于装配错误，应在构造时就报出来。
        /// </summary>
        [Fact]
        public void Constructor_NullTransport_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ModbusRtuClient((IModbusTransport)null));
        }

        /// <summary>
        /// 重复释放不应重复调用通道的 Dispose：串口二次释放会抛异常。
        /// </summary>
        [Fact]
        public void Dispose_IsIdempotent()
        {
            FakeModbusTransport transport = new FakeModbusTransport();
            ModbusRtuClient client = new ModbusRtuClient(transport);

            client.Dispose();
            client.Dispose();

            Assert.True(transport.IsDisposed);
        }

        /// <summary>
        /// 打开与关闭要传递到通道上，界面靠它显示串口状态。
        /// </summary>
        /// <summary>
        /// 按串口参数构造时只是建对象，不应真的打开串口，
        /// 否则没接表的机器上第一次启动就会失败。
        /// </summary>
        [Fact]
        public void Constructor_WithPortSettings_LeavesPortClosed()
        {
            using (ModbusRtuClient client = new ModbusRtuClient("COM1", 9600, 8, StopBits.One, Parity.None))
            {
                Assert.False(client.IsOpen);
            }
        }

        /// <summary>
        /// 打开与关闭要传递到通道上，界面靠它显示串口状态。
        /// </summary>
        [Fact]
        public void OpenAndClose_ReflectTransportState()
        {
            FakeModbusTransport transport = new FakeModbusTransport();

            using (ModbusRtuClient client = new ModbusRtuClient(transport))
            {
                Assert.False(client.IsOpen);

                client.Open();
                Assert.True(client.IsOpen);

                client.Close();
                Assert.False(client.IsOpen);
            }
        }
    }

    /// <summary>
    /// Modbus 异常：异常码要翻译成用户能看懂的说明，并保留原始码与从站地址，
    /// 便于现场排查是「地址错了」还是「设备忙」。
    /// </summary>
    public class ModbusExceptionTests
    {
        /// <summary>
        /// 六个标准异常码与未定义码都要有对应说明。
        /// </summary>
        [Theory]
        [InlineData(0x01, "非法功能码")]
        [InlineData(0x02, "非法数据地址")]
        [InlineData(0x03, "非法数据值")]
        [InlineData(0x04, "从站设备故障")]
        [InlineData(0x05, "应答超时")]
        [InlineData(0x06, "从站设备忙")]
        [InlineData(0x7F, "未定义的异常码")]
        public void Constructor_ExceptionCode_DescribesCode(byte code, string expected)
        {
            ModbusException error = new ModbusException(code, 5);

            Assert.Contains(expected, error.Message);
            Assert.Equal((byte?)code, error.ExceptionCode);
            Assert.Equal((byte?)5, error.SlaveAddress);
        }

        /// <summary>
        /// 本地错误（超时、CRC 失败）应保留内部异常与原始说明，异常码为空。
        /// </summary>
        [Fact]
        public void Constructor_WithInnerException_KeepsInnerAndCode()
        {
            InvalidOperationException inner = new InvalidOperationException("端口已关闭");

            ModbusException error = new ModbusException("读取失败", inner);

            Assert.Same(inner, error.InnerException);
            Assert.Equal("读取失败", error.Message);
            Assert.Null(error.ExceptionCode);
        }
    }
}
