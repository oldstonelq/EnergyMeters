using System;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 存储设置：数据目录记录在 ini 里，留空表示跟随程序目录。
    /// 与串口配置共用 SystemSet 节，写其中一个不能把另一个的键冲掉。
    /// </summary>
    public class StorageConfigTests
    {
        /// <summary>
        /// 没有该键时返回空目录，由 AppPaths 回落到程序目录下的 Data。
        /// </summary>
        [Fact]
        public void Load_MissingKey_ReturnsNull()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");
                File.WriteAllText(path, "[SystemSet]\r\nPortName=COM3\r\n");

                Assert.Null(StorageConfig.Load(path).DataDirectory);
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
                File.WriteAllText(path,
                    "[SystemSet]\r\nDataDirectory=  " + directory + "  \r\n");

                Assert.Equal(directory, StorageConfig.Load(path).DataDirectory);
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
                Directory.CreateDirectory(directory);

                new StorageConfig { DataDirectory = directory }.Save(path);

                Assert.Equal(directory, StorageConfig.Load(path).DataDirectory);
            }
        }

        /// <summary>
        /// 目录为空时写回空值：配置文件里保留这个键，但语义是「跟随程序目录」。
        /// </summary>
        [Fact]
        public void Save_Empty_WritesBlankAndLoadsAsDefault()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("system.ini");

                new StorageConfig { DataDirectory = "  " }.Save(path);

                Assert.Contains("DataDirectory=", File.ReadAllText(path));
                Assert.Null(StorageConfig.Load(path).DataDirectory);
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

                device.Save(path);
                new StorageConfig { DataDirectory = directory }.Save(path);

                DeviceConfig reloaded = DeviceConfig.Load(path);
                Assert.Equal("COM7", reloaded.PortName);
                Assert.Equal(19200, reloaded.BaudRate);
                Assert.Equal(MeterType.Adl400, reloaded.MeterType);
                Assert.Equal(12, reloaded.SlaveAddress);
                Assert.Equal(directory, StorageConfig.Load(path).DataDirectory);
            }
        }
    }

    /// <summary>
    /// 路径装配：数据根目录可以来自配置，其余目录仍跟着程序目录。
    /// </summary>
    public class AppPathsTests
    {
        /// <summary>
        /// 没配置数据目录时用程序目录下的 Data，与旧版本一致。
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
        /// 完整初始化时按表型把子目录一次建好，第一次启动就能直接落盘。
        /// </summary>
        [Fact]
        public void EnsureCreated_CreatesAllMeterDirectories()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string custom = temp.File("external-data");
                AppPaths paths = new AppPaths(temp.Root, custom);

                paths.EnsureCreated();

                Assert.True(Directory.Exists(paths.LogDirectory));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Djsf1352)));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Amc)));
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Adl400)));
            }
        }
    }
}
