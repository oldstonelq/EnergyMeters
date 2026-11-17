using System;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 直流单路表：三个通道都按指数寄存器还原，验证缩放与无效值约定。
    /// </summary>
    public class Djsf1352MeterTests
    {
        /// <summary>
        /// 三个通道的指数都取 2，按「通讯值 x 10^(指数-3)」还原：
        /// 482 得 48.2、125 得 12.5、60 得 6.0。指数取错会让整表读数差一个数量级。
        /// </summary>
        [Fact]
        public void Read_AppliesExponentScaling()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0, 2, 482, 2)
                .Expect(1, 2, 2, 125, 2)
                .Expect(1, 8, 2, 60, 2);

            using (Djsf1352EnergyMeters meter = new Djsf1352EnergyMeters(new DeviceConfig(), transport))
            {
                MeterSample sample = meter.Read();

                // 实际值 = 有效值 x 10^(指数-3)
                Assert.Equal(48.2, sample.GetValue(0), 3);
                Assert.Equal(12.5, sample.GetValue(1), 3);
                Assert.Equal(6.0, sample.GetValue(2), 3);
            }
        }

        /// <summary>
        /// 指数寄存器与数值寄存器同为 FFFF 表示该通道无效，约定记 -1，
        /// 避免把 65535 当成真实读数写进数据文件。
        /// </summary>
        [Fact]
        public void Read_EmptyRegisterPair_ReturnsMinusOne()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0, 2, 0xFFFF, 0xFFFF)
                .Expect(1, 2, 2, 125, 2)
                .Expect(1, 8, 2, 60, 2);

            using (Djsf1352EnergyMeters meter = new Djsf1352EnergyMeters(new DeviceConfig(), transport))
            {
                Assert.Equal(-1, meter.Read().GetValue(0), 3);
            }
        }

        /// <summary>
        /// 打开与关闭要传递到 Modbus 客户端，界面靠 IsOpen 显示串口状态。
        /// </summary>
        [Fact]
        public void OpenAndClose_TrackClientState()
        {
            FakeModbusTransport transport = new FakeModbusTransport();

            using (Djsf1352EnergyMeters meter = new Djsf1352EnergyMeters(new DeviceConfig(), transport))
            {
                Assert.False(meter.IsOpen);

                meter.Open();
                Assert.True(meter.IsOpen);

                meter.Close();
                Assert.False(meter.IsOpen);
            }
        }

        /// <summary>
        /// 指数档位取 0 / 4 / 5 与负值：10^(指数-3) 的两端都要对，
        /// 指数读错会让整表差若干个数量级，而且现场看不出来。
        /// </summary>
        [Theory]
        [InlineData(1234, 0, 1.234)]
        [InlineData(1234, 4, 12340.0)]
        [InlineData(60, 5, 6000.0)]
        [InlineData(-500, 3, -500.0)]
        public void Read_AppliesExponentBoundaries(int rawValue, int exponent, double expected)
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0, 2, unchecked((ushort)(short)rawValue), (ushort)exponent)
                .Expect(1, 2, 2, 125, 2)
                .Expect(1, 8, 2, 60, 2);

            using (Djsf1352EnergyMeters meter = new Djsf1352EnergyMeters(new DeviceConfig(), transport))
            {
                Assert.Equal(expected, meter.Read().GetValue(0), 3);
            }
        }

        /// <summary>
        /// 注入固定时钟后样本时间应取自注入值而不是 DateTime.Now，
        /// 否则跨零点与系统时间跳变无法在测试里复现。
        /// </summary>
        [Fact]
        public void Read_UsesInjectedClock()
        {
            DateTime stamp = new DateTime(2026, 11, 17, 10, 0, 0);
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0, 2, 482, 2)
                .Expect(1, 2, 2, 125, 2)
                .Expect(1, 8, 2, 60, 2);

            using (Djsf1352EnergyMeters meter =
                new Djsf1352EnergyMeters(new DeviceConfig(), transport, () => stamp))
            {
                Assert.Equal(stamp, meter.Read().Time);
            }
        }
    }

    /// <summary>
    /// 三相表 ADL400：一次读取里同时覆盖相电压、线电压、电流三种缩放，
    /// 以及功率项的高字在前 32 位拼接与符号位。
    /// </summary>
    public class Adl400MeterTests
    {
        /// <summary>
        /// 相电压与线电压除 10、电流除 100、功率除 1000；B 相有功预置为负值，
        /// 用来确认 32 位拼接时符号位没有被当成数值位。
        /// </summary>
        [Fact]
        public void Read_CombinesThirtyTwoBitPowerWithSign()
        {
            ushort[] power = new ushort[16];
            WritePower(power, 0, 3720);
            WritePower(power, 1, -1240);

            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(177, 0x0061, 3, 2204, 2198, 2201)
                .Expect(177, 0x0078, 3, 3816, 3812, 3814)
                .Expect(177, 0x0064, 3, 946, 951, 938)
                .Expect(177, 0x0164, 16, power);

            using (Adl400EnergyMeters meter = new Adl400EnergyMeters(new DeviceConfig(), transport))
            {
                MeterSample sample = meter.Read();

                Assert.Equal(220.4, sample.GetValue(0), 3);
                Assert.Equal(381.6, sample.GetValue(3), 3);
                Assert.Equal(9.46, sample.GetValue(6), 3);
                // 高字在前拼成 32 位有符号数，再除以 1000
                Assert.Equal(3.72, sample.GetValue(9), 3);
                Assert.Equal(-1.24, sample.GetValue(10), 3);
                Assert.Equal(0, sample.GetValue(16), 3);
            }
        }

        /// <summary>
        /// 把一个有符号数按高字在前写进两个寄存器，模拟表计的 32 位功率格式。
        /// </summary>
        /// <param name="registers">功率区寄存器数组</param>
        /// <param name="itemIndex">第几项功率，每项占两个寄存器</param>
        /// <param name="value">要填入的有符号数值</param>
        /// <summary>
        /// 逐项核对 17 个通道：相电压与线电压除 10、电流除 100、功率除 1000。
        /// 默认曲线用的总有功（下标 12）也在这里，写错偏移会整段串位。
        /// </summary>
        [Fact]
        public void Read_CoversAllSeventeenChannels()
        {
            ushort[] power = new ushort[16];
            WritePower(power, 0, 3720);
            WritePower(power, 1, -1240);
            WritePower(power, 2, 1220);
            WritePower(power, 3, 3700);
            WritePower(power, 4, 101);
            WritePower(power, 5, 203);
            WritePower(power, 6, 302);
            WritePower(power, 7, 606);

            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(177, 0x0061, 3, 2204, 2198, 2201)
                .Expect(177, 0x0078, 3, 3816, 3812, 3814)
                .Expect(177, 0x0064, 3, 946, 951, 938)
                .Expect(177, 0x0164, 16, power);

            using (Adl400EnergyMeters meter = new Adl400EnergyMeters(new DeviceConfig(), transport))
            {
                MeterSample sample = meter.Read();

                Assert.Equal(220.4, sample.GetValue(0), 3);
                Assert.Equal(219.8, sample.GetValue(1), 3);
                Assert.Equal(220.1, sample.GetValue(2), 3);
                Assert.Equal(381.6, sample.GetValue(3), 3);
                Assert.Equal(381.2, sample.GetValue(4), 3);
                Assert.Equal(381.4, sample.GetValue(5), 3);
                Assert.Equal(9.46, sample.GetValue(6), 3);
                Assert.Equal(9.51, sample.GetValue(7), 3);
                Assert.Equal(9.38, sample.GetValue(8), 3);
                Assert.Equal(3.72, sample.GetValue(9), 3);
                Assert.Equal(-1.24, sample.GetValue(10), 3);
                Assert.Equal(1.22, sample.GetValue(11), 3);
                Assert.Equal(3.7, sample.GetValue(12), 3);
                Assert.Equal(0.101, sample.GetValue(13), 3);
                Assert.Equal(0.203, sample.GetValue(14), 3);
                Assert.Equal(0.302, sample.GetValue(15), 3);
                Assert.Equal(0.606, sample.GetValue(16), 3);
            }
        }

        /// <summary>
        /// 打开与关闭要传递到 Modbus 客户端，界面靠 IsOpen 显示串口状态。
        /// </summary>
        [Fact]
        public void OpenAndClose_TrackClientState()
        {
            FakeModbusTransport transport = new FakeModbusTransport();

            using (Adl400EnergyMeters meter = new Adl400EnergyMeters(new DeviceConfig(), transport))
            {
                Assert.False(meter.IsOpen);

                meter.Open();
                Assert.True(meter.IsOpen);

                meter.Close();
                Assert.False(meter.IsOpen);
            }
        }

                /// <summary>
        /// 把一个有符号数按高字在前写进两个寄存器，模拟表计的 32 位功率格式。
        /// </summary>
        /// <param name="registers">功率区寄存器数组</param>
        /// <param name="itemIndex">第几项功率，每项占两个寄存器</param>
        /// <param name="value">要填入的有符号数值</param>
        private static void WritePower(ushort[] registers, int itemIndex, int value)
        {
            uint raw = unchecked((uint)value);
            registers[itemIndex * 2] = (ushort)(raw >> 16);
            registers[itemIndex * 2 + 1] = (ushort)raw;
        }
    }

    /// <summary>
    /// 三相表 AMC：开表时先读两组小数位寄存器，读取时据此缩放，
    /// 功率符号字节决定各项的正负。
    /// </summary>
    public class AmcMeterTests
    {
        /// <summary>
        /// 小数位取 3 时按「通讯值 x 10^(3-4)」还原；符号字节第 0 位置 1 使 A 相有功取负、
        /// B 相保持正，确认符号位按项对应而不是整段一起翻转。
        /// </summary>
        [Fact]
        public void Open_ReadsDecimalPoints_AndPowerSignsApply()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0023, 2, 0x0303, 0x0300)
                .Expect(1, 0x0025, 6, 2204, 2198, 2201, 3816, 3812, 3814)
                .Expect(1, 0x002B, 3, 946, 951, 938)
                .Expect(1, 0x002E, 8, 3720, 3720, 3720, 3720, 3720, 3720, 3720, 3720)
                .Expect(1, 0x0024, 1, 0x0001);

            using (AmcEnergyMeters meter = new AmcEnergyMeters(new DeviceConfig(), transport))
            {
                meter.Open();
                MeterSample sample = meter.Read();

                // 小数位 3：实际值 = 通讯值 x 10^(3-4)
                Assert.Equal(220.4, sample.GetValue(0), 3);
                Assert.Equal(94.6, sample.GetValue(6), 3);
                // 符号字节第 0 位为 1，A 相有功取负
                Assert.Equal(-372.0, sample.GetValue(9), 3);
                Assert.Equal(372.0, sample.GetValue(10), 3);
            }
        }

        /// <summary>
        /// 寄存器值超过 32767 时必须保持正数。早先转成 short 会翻成负数
        /// （380 V 系统若用 2 位小数，38160 会读成 -273.76），
        /// 再叠加符号位就成了正负完全颠倒。
        /// </summary>
        [Fact]
        public void Read_RegisterAboveInt16Range_StaysPositive()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0023, 2, 0x0202, 0x0200)
                .Expect(1, 0x0025, 6, 22040, 21980, 22010, 38160, 38120, 38140)
                .Expect(1, 0x002B, 3, 946, 951, 938)
                .Expect(1, 0x002E, 8, 3720, 3720, 3720, 3720, 3720, 3720, 3720, 3720)
                .Expect(1, 0x0024, 1, 0x0000);

            using (AmcEnergyMeters meter = new AmcEnergyMeters(new DeviceConfig(), transport))
            {
                meter.Open();
                MeterSample sample = meter.Read();

                Assert.Equal(220.4, sample.GetValue(0), 3);
                Assert.Equal(381.6, sample.GetValue(3), 3);
                Assert.Equal(9.46, sample.GetValue(6), 3);
                Assert.Equal(37.2, sample.GetValue(9), 3);
            }
        }

        /// <summary>
        /// 符号字节按位对应功率项：第 0 位是 A 相有功、第 7 位是总无功。
        /// 同时用另一组小数位（电压 3、电流 2、功率 2），覆盖 Scale 的不同分支。
        /// </summary>
        [Fact]
        public void Read_AppliesHighOrderSignBitsAndOtherDecimalPoints()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0023, 2, 0x0302, 0x0200)
                .Expect(1, 0x0025, 6, 2204, 2198, 2201, 3816, 3812, 3814)
                .Expect(1, 0x002B, 3, 946, 951, 938)
                .Expect(1, 0x002E, 8, 3720, 3720, 3720, 3720, 3720, 3720, 3720, 3720)
                .Expect(1, 0x0024, 1, 0x0081);

            using (AmcEnergyMeters meter = new AmcEnergyMeters(new DeviceConfig(), transport))
            {
                meter.Open();
                MeterSample sample = meter.Read();

                Assert.Equal(220.4, sample.GetValue(0), 3);
                Assert.Equal(381.6, sample.GetValue(3), 3);
                Assert.Equal(9.46, sample.GetValue(6), 3);
                // 第 0 位与第 7 位为 1：A 相有功与总无功取负，其余各项保持正。
                Assert.Equal(-37.2, sample.GetValue(9), 3);
                Assert.Equal(37.2, sample.GetValue(10), 3);
                Assert.Equal(37.2, sample.GetValue(12), 3);
                Assert.Equal(-37.2, sample.GetValue(16), 3);
            }
        }

        /// <summary>
        /// AMC 打开串口时会顺带读一次小数位，因此关闭时只需验证状态位；
        /// 这里同时确认 Open 发出了小数位请求。
        /// </summary>
        [Fact]
        public void OpenAndClose_TrackClientState()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(1, 0x0023, 2, 0x0303, 0x0300);

            using (AmcEnergyMeters meter = new AmcEnergyMeters(new DeviceConfig(), transport))
            {
                Assert.False(meter.IsOpen);

                meter.Open();
                Assert.True(meter.IsOpen);
                Assert.Equal(new[] { "1:35:2" }, transport.Requests);

                meter.Close();
                Assert.False(meter.IsOpen);
            }
        }
    }
}
