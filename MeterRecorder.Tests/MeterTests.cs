using System;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    public class Djsf1352MeterTests
    {
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

    public class Adl400MeterTests
    {
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

        private static void WritePower(ushort[] registers, int itemIndex, int value)
        {
            uint raw = unchecked((uint)value);
            registers[itemIndex * 2] = (ushort)(raw >> 16);
            registers[itemIndex * 2 + 1] = (ushort)raw;
        }
    }

    public class AmcMeterTests
    {
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
    }
}
