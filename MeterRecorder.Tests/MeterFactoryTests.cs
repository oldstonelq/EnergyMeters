using System;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 表型装配：配置里的型号要落到正确的表类上，装错了会按错误的地址去读。
    /// </summary>
    public class MeterFactoryTests
    {
        /// <summary>
        /// 三种配置值分别对应三个表类，且通道定义跟着表型走。
        /// </summary>
        [Theory]
        [InlineData(MeterType.Djsf1352, 3)]
        [InlineData(MeterType.Amc, 17)]
        [InlineData(MeterType.Adl400, 17)]
        public void Create_ReturnsMeterMatchingConfig(MeterType meterType, int expectedChannels)
        {
            DeviceConfig config = new DeviceConfig { MeterType = meterType };

            using (IEnergyMeters meter = MeterFactory.Create(config))
            {
                Assert.Equal(meterType, DetectType(meter));
                Assert.Equal(expectedChannels, meter.Channels.Count);
            }
        }

        /// <summary>
        /// 表型取值越界时必须报错：早先用 default 兜成直流表，程序照常启动，
        /// 却按三个通道去记录三相表，数据错了也看不出来。
        /// </summary>
        [Fact]
        public void Create_UnknownMeterType_Throws()
        {
            DeviceConfig config = new DeviceConfig { MeterType = (MeterType)99 };

            Assert.Throws<ArgumentOutOfRangeException>(() => MeterFactory.Create(config));
        }

        /// <summary>
        /// 未提供配置属于装配错误，应在创建时就说清楚。
        /// </summary>
        [Fact]
        public void Create_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => MeterFactory.Create(null));
        }

        /// <summary>
        /// 从站地址取自配置；未配置时才用表型内置的默认值（ADL400 是 177）。
        /// 这里通过「预置了哪个从站的应答」间接验证，用错地址会直接抛异常。
        /// </summary>
        [Fact]
        public void Adl400_UsesSlaveAddressFromConfig()
        {
            FakeModbusTransport transport = new FakeModbusTransport()
                .Expect(9, 0x0061, 3, 2204, 2198, 2201)
                .Expect(9, 0x0078, 3, 3816, 3812, 3814)
                .Expect(9, 0x0064, 3, 946, 951, 938)
                .Expect(9, 0x0164, 16, new ushort[16]);

            using (Adl400EnergyMeters meter = new Adl400EnergyMeters(
                new DeviceConfig { SlaveAddress = 9 }, transport))
            {
                meter.Read();

                Assert.All(transport.Requests, key => Assert.StartsWith("9:", key));
            }
        }

                /// <summary>
        /// 按实例的实际类型反推表型枚举，避免断言写成「工厂返回了自己传的那个值」。
        /// </summary>
        /// <param name="meter">工厂创建出的表对象</param>
        private static MeterType DetectType(IEnergyMeters meter)
        {
            if (meter is Djsf1352EnergyMeters)
            {
                return MeterType.Djsf1352;
            }
            if (meter is AmcEnergyMeters)
            {
                return MeterType.Amc;
            }
            if (meter is Adl400EnergyMeters)
            {
                return MeterType.Adl400;
            }
            throw new InvalidOperationException("未知表类：" + meter.GetType().Name);
        }
    }

    /// <summary>
    /// 表型枚举与配置取值的互转。配置文件由人手工编辑，大小写与空格都不能成为崩溃点。
    /// </summary>
    public class MeterTypeExtensionsTests
    {
        /// <summary>
        /// 三个型号的配置取值应能原样解析回来，保证写出去的文件能被自己读懂。
        /// </summary>
        [Theory]
        [InlineData(MeterType.Djsf1352)]
        [InlineData(MeterType.Amc)]
        [InlineData(MeterType.Adl400)]
        public void ToConfigValue_RoundTripsThroughTryParse(MeterType meterType)
        {
            string text = meterType.ToConfigValue();

            MeterType parsed;
            Assert.True(MeterTypeExtensions.TryParseConfigValue(text, out parsed));
            Assert.Equal(meterType, parsed);
        }

        /// <summary>
        /// 手工编辑 ini 时大小写和前后空格很常见，都应接受。
        /// </summary>
        [Theory]
        [InlineData("adl400")]
        [InlineData("ADL400")]
        [InlineData(" Adl400 ")]
        [InlineData("amc")]
        public void TryParseConfigValue_IgnoresCaseAndSpaces(string text)
        {
            MeterType parsed;
            Assert.True(MeterTypeExtensions.TryParseConfigValue(text, out parsed));
            Assert.True(parsed == MeterType.Adl400 || parsed == MeterType.Amc);
        }

        /// <summary>
        /// 无法识别的型号返回 false，由调用方决定回落策略，而不是抛异常中断启动。
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ADL40")]
        [InlineData("三相表")]
        public void TryParseConfigValue_Unknown_ReturnsFalse(string text)
        {
            MeterType parsed;
            Assert.False(MeterTypeExtensions.TryParseConfigValue(text, out parsed));
        }

        /// <summary>
        /// 界面显示名要带上型号与类型，避免用户只看「AMC」不知道是交流还是直流。
        /// </summary>
        [Fact]
        public void ToDisplayName_ContainsModelName()
        {
            Assert.Contains("DJSF1352", MeterType.Djsf1352.ToDisplayName());
            Assert.Contains("AMC", MeterType.Amc.ToDisplayName());
            Assert.Contains("ADL400", MeterType.Adl400.ToDisplayName());
        }

        /// <summary>
        /// 枚举值越界时应报错而不是返回一个看似正常的值，否则会静默写出错误的表型。
        /// </summary>
        [Fact]
        public void ToConfigValue_UndefinedValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => ((MeterType)99).ToConfigValue());
        }

        /// <summary>
        /// 显示名同样不接受越界值：设置界面用它填充下拉框，静默返回空串会让用户选到空白项。
        /// </summary>
        [Fact]
        public void ToDisplayName_UndefinedValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => ((MeterType)(-1)).ToDisplayName());
        }
    }
}
