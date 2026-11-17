using System;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 存储设置：数据目录与日志文件记录在 ini 里，留空表示跟随程序目录。
    /// 与串口配置共用 SystemSet 节，写其中一个不能把另一个的键冲掉。
    /// </summary>
    public class StorageConfigTests
    {
        /// <summary>
        /// 没有这些键时两个属性都为空，由 AppPaths 回落到程序目录。
        /// </summary>
        [Fact]
        public void Load_MissingKey_ReturnsNull()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                File.WriteAllText(path, "[SystemSet]\r\nPortName=COM3\r\n");

                StorageConfig config = StorageConfig.Load(path);

                Assert.Null(config.DataDirectory);
                Assert.Null(config.LogFile);
            }
        }

        /// <summary>
        /// 读到的路径要去掉手工编辑时留下的空格。
        /// </summary>
        [Fact]
        public void Load_ReadsAndTrimsValue()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                string directory = temp.File("custom data");
                string logFile = temp.File(@"logs\meter.log");
                File.WriteAllText(path,
                    "[SystemSet]\r\nDataDirectory=  " + directory + "  \r\nLogFile=  " + logFile + "  \r\n");

                StorageConfig config = StorageConfig.Load(path);

                Assert.Equal(directory, config.DataDirectory);
                Assert.Equal(logFile, config.LogFile);
            }
        }

        /// <summary>
        /// 写回后立刻读出来应完全一致。
        /// </summary>
        [Fact]
        public void Save_ThenLoad_RoundTrips()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                string directory = temp.File("meter-data");
                string logFile = Path.Combine(temp.File("logs"), "meter.log");
                Directory.CreateDirectory(directory);

                new StorageConfig { DataDirectory = directory, LogFile = logFile }.Save(path);

                StorageConfig reloaded = StorageConfig.Load(path);
                Assert.Equal(directory, reloaded.DataDirectory);
                Assert.Equal(logFile, reloaded.LogFile);
            }
        }

        /// <summary>
        /// 两项都为空时写回空值：配置文件里保留这两个键，但语义是「跟随程序目录」。
        /// </summary>
        [Fact]
        public void Save_Empty_WritesBlankAndLoadsAsDefault()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");

                new StorageConfig { DataDirectory = "  ", LogFile = "  " }.Save(path);

                string ini = File.ReadAllText(path);
                Assert.Contains("DataDirectory=", ini);
                Assert.Contains("LogFile=", ini);

                StorageConfig reloaded = StorageConfig.Load(path);
                Assert.Null(reloaded.DataDirectory);
                Assert.Null(reloaded.LogFile);
            }
        }

        /// <summary>
        /// 目录不存在时写入应抛 IOException，由界面提示用户。
        /// </summary>
        [Fact]
        public void Save_InvalidPath_ThrowsIOException()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                StorageConfig config = new StorageConfig { DataDirectory = "C:\\data" };

                Assert.Throws<IOException>(() => config.Save(temp.File(@"missing\system.ini")));
            }
        }

        /// <summary>
        /// 两个配置类写同一个 ini 的不同键，先后保存不应互相覆盖。
        /// </summary>
        [Fact]
        public void Save_DoesNotOverwriteDeviceKeys()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                DeviceConfig device = new DeviceConfig
                {
                    PortName = "COM7",
                    BaudRate = 19200,
                    MeterType = MeterType.Adl400,
                    SlaveAddress = 12
                };
                string directory = temp.File("data-root");
                string logFile = Path.Combine(temp.File("logs"), "log.txt");

                device.Save(path);
                new StorageConfig { DataDirectory = directory, LogFile = logFile }.Save(path);

                DeviceConfig reloaded = DeviceConfig.Load(path);
                Assert.Equal("COM7", reloaded.PortName);
                Assert.Equal(19200, reloaded.BaudRate);
                Assert.Equal(MeterType.Adl400, reloaded.MeterType);
                Assert.Equal(12, reloaded.SlaveAddress);

                StorageConfig storage = StorageConfig.Load(path);
                Assert.Equal(directory, storage.DataDirectory);
                Assert.Equal(logFile, storage.LogFile);
            }
        }
    }

    /// <summary>
    /// 路径装配：数据根目录与日志文件都可以来自配置，其余目录仍跟着程序目录。
    /// </summary>
    public class AppPathsTests
    {
        /// <summary>
        /// 没配置时用程序目录下的 Data 与 Logs，与旧版本一致。
        /// </summary>
        [Fact]
        public void Constructor_Default_UsesStartupData()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                AppPaths paths = new AppPaths(temp.Root);

                Assert.Equal(Path.Combine(temp.Root, "Data"), paths.DataDirectory);
                Assert.Equal(Path.Combine(temp.Root, "System", "system.ini"), paths.SystemFile);
                Assert.Equal(Path.Combine(temp.Root, "Logs", "log.txt"), paths.LogFile);
            }
        }

        /// <summary>
        /// 指定根目录后，各表型的子目录都挂在这个目录下。
        /// </summary>
        [Fact]
        public void Constructor_CustomRoot_IsUsedForMeterSubdirectories()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string custom = temp.File("external-data");

                AppPaths paths = new AppPaths(temp.Root, custom);

                Assert.Equal(custom, paths.DataDirectory);
                Assert.Equal(Path.Combine(custom, "ADL400"), paths.GetDataDirectory(MeterType.Adl400));
                Assert.Equal(Path.Combine(custom, "DJSF1352"), paths.GetDataDirectory(MeterType.Djsf1352));
            }
        }

        /// <summary>
        /// 指定日志文件后，日志目录从文件路径推导出来，不再固定为 Logs。
        /// </summary>
        [Fact]
        public void Constructor_CustomLogFile_DerivesDirectory()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string logFile = Path.Combine(temp.File("var-log"), "meter.log");

                AppPaths paths = new AppPaths(temp.Root, null, logFile);

                Assert.Equal(logFile, paths.LogFile);
                Assert.Equal(temp.File("var-log"), paths.LogDirectory);
                Assert.Equal(Path.Combine(temp.Root, "Data"), paths.DataDirectory);
            }
        }

        /// <summary>
        /// 日志路径留空时回到程序目录下的 Logs\log.txt。
        /// </summary>
        [Fact]
        public void Constructor_BlankLogFile_FallsBackToDefault()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                AppPaths paths = new AppPaths(temp.Root, null, "   ");

                Assert.Equal(Path.Combine(temp.Root, "Logs", "log.txt"), paths.LogFile);
            }
        }

        /// <summary>
        /// 配置里写了空白字符等同于没配置，不能把 Data 目录建在空白路径上。
        /// </summary>
        [Fact]
        public void Constructor_BlankRoot_FallsBackToDefault()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                Assert.Equal(Path.Combine(temp.Root, "Data"), new AppPaths(temp.Root, "   ").DataDirectory);
            }
        }

        /// <summary>
        /// 启动时先读配置再决定数据目录，所以建 ini 的那一步不能顺手建 Data。
        /// </summary>
        [Fact]
        public void EnsureConfigFile_CreatesIniWithoutDataDirectories()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                AppPaths paths = new AppPaths(temp.Root);

                paths.EnsureConfigFile();

                Assert.True(File.Exists(paths.SystemFile));
                Assert.False(Directory.Exists(paths.DataDirectory));
            }
        }

        /// <summary>
        /// 完整初始化时按配置把数据与日志目录一次建好，第一次启动就能直接落盘。
        /// </summary>
        [Fact]
        public void EnsureCreated_CreatesAllMeterDirectories()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string custom = temp.File("external-data");
                string logFile = Path.Combine(temp.File("custom-logs"), "log.txt");
                AppPaths paths = new AppPaths(temp.Root, custom, logFile);

                paths.EnsureCreated();

                Assert.True(Directory.Exists(paths.LogDirectory));
                Assert.True(Directory.Exists(Path.GetDirectoryName(logFile)));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Djsf1352)));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Amc)));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Adl400)));
            }
        }
    }
}
