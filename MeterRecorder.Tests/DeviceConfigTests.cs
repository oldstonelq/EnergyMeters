using System;
using System.IO;
using System.IO.Ports;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 配置文件读写。ini 由人手工编辑，脏值必须回落到默认值而不是让程序起不来；
    /// 每个用例都用独立临时目录，互不影响。
    /// </summary>
    public class DeviceConfigTests
    {
        /// <summary>
        /// 文件不存在时返回一份可直接使用的默认配置。
        /// </summary>
        [Fact]
        public void Load_MissingFile_ReturnsDefaults()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                DeviceConfig config = DeviceConfig.Load(temp.File("missing.ini"));

                Assert.Equal("COM3", config.PortName);
                Assert.Equal(9600, config.BaudRate);
                Assert.Equal(8, config.DataBits);
                Assert.Equal(StopBits.One, config.StopBits);
                Assert.Equal(Parity.None, config.Parity);
                Assert.Equal(MeterType.Djsf1352, config.MeterType);
                Assert.Null(config.SlaveAddress);
            }
        }

        /// <summary>
        /// 合法值逐项读取，包括端口名前后空格与大小写不同的表型写法。
        /// </summary>
        [Fact]
        public void Load_ReadsAllValues()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                WriteIni(path,
                    "PortName=  COM7  ",
                    "BaudRate=19200",
                    "DataBits=7",
                    "StopBits=Two",
                    "Parity=Even",
                    "EnergyMetersType=adl400",
                    "SlaveAddress=17");

                DeviceConfig config = DeviceConfig.Load(path);

                Assert.Equal("COM7", config.PortName);
                Assert.Equal(19200, config.BaudRate);
                Assert.Equal(7, config.DataBits);
                Assert.Equal(StopBits.Two, config.StopBits);
                Assert.Equal(Parity.Even, config.Parity);
                Assert.Equal(MeterType.Adl400, config.MeterType);
                Assert.Equal(17, config.SlaveAddress);
            }
        }

        /// <summary>
        /// 超出范围或无法解析的数值一律回落默认值：
        /// 波特率 100 与 128000 都不合法，数据位 9 也不合法。
        /// 停止位写 "Three"、校验位写 "One"（旧版本误把停止位的取值写进校验位）
        /// 都不是对应枚举的合法值，应回落。
        /// </summary>
        [Fact]
        public void Load_OutOfRangeValues_FallBackToDefaults()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                WriteIni(path,
                    "BaudRate=100",
                    "DataBits=9",
                    "StopBits=Three",
                    "Parity=One",
                    "SlaveAddress=248");

                DeviceConfig config = DeviceConfig.Load(path);

                Assert.Equal(9600, config.BaudRate);
                Assert.Equal(8, config.DataBits);
                Assert.Equal(StopBits.One, config.StopBits);
                Assert.Equal(Parity.None, config.Parity);
                Assert.Null(config.SlaveAddress);
            }
        }

        /// <summary>
        /// Enum.TryParse 对数字字符串一律返回 true：Parity=9、StopBits=7 会得到枚举里
        /// 根本没有的取值。这类值必须在读取时就回落，不能等到打开串口才炸。
        /// </summary>
        [Fact]
        public void Load_NumericEnumOutOfRange_FallsBackToDefaults()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                WriteIni(path, "Parity=9", "StopBits=7");

                DeviceConfig config = DeviceConfig.Load(path);

                Assert.Equal(Parity.None, config.Parity);
                Assert.Equal(StopBits.One, config.StopBits);
            }
        }

        /// <summary>
        /// 从站地址 0 表示未配置，应与「缺省」一样回落到表型内置值，而不是真的发往 0 号。
        /// </summary>
        [Fact]
        public void Load_SlaveAddressZero_TreatedAsUnset()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                WriteIni(path, "SlaveAddress=0");

                Assert.Null(DeviceConfig.Load(path).SlaveAddress);
            }
        }

        /// <summary>
        /// 型号写错时回落到直流表，保证程序仍能启动，用户可在设置界面改正。
        /// </summary>
        [Fact]
        public void Load_UnknownMeterType_FallsBackToDcMeter()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                WriteIni(path, "EnergyMetersType=ADL999");

                Assert.Equal(MeterType.Djsf1352, DeviceConfig.Load(path).MeterType);
            }
        }

        /// <summary>
        /// 保存后再读回，七项配置应完全一致；从站地址为空时按「未配置」保存。
        /// </summary>
        [Fact]
        public void Save_ThenLoad_RoundTrips()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                DeviceConfig saved = new DeviceConfig
                {
                    PortName = "COM9",
                    BaudRate = 4800,
                    DataBits = 7,
                    StopBits = StopBits.Two,
                    Parity = Parity.Odd,
                    MeterType = MeterType.Amc,
                    SlaveAddress = 12
                };

                saved.Save(path);
                DeviceConfig loaded = DeviceConfig.Load(path);

                Assert.Equal(saved.PortName, loaded.PortName);
                Assert.Equal(saved.BaudRate, loaded.BaudRate);
                Assert.Equal(saved.DataBits, loaded.DataBits);
                Assert.Equal(saved.StopBits, loaded.StopBits);
                Assert.Equal(saved.Parity, loaded.Parity);
                Assert.Equal(saved.MeterType, loaded.MeterType);
                Assert.Equal(saved.SlaveAddress, loaded.SlaveAddress);
            }
        }

        /// <summary>
        /// 目录不存在时写入应抛出 IOException 供界面提示，而不是「提示成功但没写进去」。
        /// </summary>
        [Fact]
        public void Save_InvalidPath_ThrowsIOException()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                DeviceConfig config = new DeviceConfig();

                Assert.Throws<IOException>(() => config.Save(temp.File(@"missing\system.ini")));
            }
        }

        /// <summary>
        /// 克隆用于设置界面「改完再决定是否生效」，副本改动不能影响原配置。
        /// </summary>
        [Fact]
        public void Clone_ProducesIndependentCopy()
        {
            DeviceConfig original = new DeviceConfig
            {
                PortName = "COM1",
                BaudRate = 19200,
                MeterType = MeterType.Adl400,
                SlaveAddress = 177
            };

            DeviceConfig copy = original.Clone();
            copy.PortName = "COM5";
            copy.SlaveAddress = null;
            copy.MeterType = MeterType.Amc;

            Assert.Equal("COM1", original.PortName);
            Assert.Equal(177, original.SlaveAddress);
            Assert.Equal(MeterType.Adl400, original.MeterType);
            Assert.Equal(19200, copy.BaudRate);
        }

        private static void WriteIni(string path, params string[] entries)
        {
            File.WriteAllText(path, "[SystemSet]\r\n" + string.Join("\r\n", entries) + "\r\n");
        }
    }

    /// <summary>
    /// 临时目录：用例结束即删除，避免测试在用户机器上留垃圾。
    /// </summary>
    internal sealed class TempDirectory : IDisposable
    {
        /// <summary>
        /// 在系统临时目录下创建一个以 GUID 命名的子目录。
        /// </summary>
        public TempDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), "MeterRecorderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        /// <summary>
        /// 临时目录的完整路径。
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// 拼出目录内的文件路径。
        /// </summary>
        /// <param name="relativePath">相对路径，可含子目录</param>
        public string File(string relativePath)
        {
            return Path.Combine(Root, relativePath);
        }

        /// <summary>
        /// 删除临时目录；文件被占用时忽略，不影响用例结果。
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
