using System;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 路径规范化：设置窗体与启动装配共用这套规则，重点是「以分隔符结尾的日志路径」
    /// 要补上文件名，否则日志会写到一个目录路径上、异常被吞掉后一条都留不下。
    /// </summary>
    public class StoragePathTests
    {
        /// <summary>
        /// 空白输入表示「不配置」，返回 null 由调用方回落默认目录。
        /// </summary>
        [Fact]
        public void NormalizeDirectory_Blank_ReturnsNull()
        {
            Assert.Null(StoragePath.NormalizeDirectory(null));
            Assert.Null(StoragePath.NormalizeDirectory("   "));
        }

        /// <summary>
        /// 去掉手工输入的首尾空格，并转成完整路径。
        /// </summary>
        [Fact]
        public void NormalizeDirectory_TrimsAndMakesFullPath()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("data");

                Assert.Equal(directory, StoragePath.NormalizeDirectory("  " + directory + "  "));
            }
        }

        /// <summary>
        /// 日志路径为空时同样返回 null。
        /// </summary>
        [Fact]
        public void NormalizeLogFile_Blank_ReturnsNull()
        {
            Assert.Null(StoragePath.NormalizeLogFile(null));
            Assert.Null(StoragePath.NormalizeLogFile("  "));
        }

        /// <summary>
        /// 以分隔符结尾说明给的是目录（哪怕目录还不存在），要补上默认文件名。
        /// </summary>
        [Fact]
        public void NormalizeLogFile_TrailingSeparator_AppendsFileName()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("not-created") + Path.DirectorySeparatorChar;

                Assert.Equal(Path.Combine(temp.File("not-created"), "log.txt"),
                    StoragePath.NormalizeLogFile(directory));
            }
        }

        /// <summary>
        /// 已存在的目录同样补文件名，不依赖用户是否写了结尾分隔符。
        /// </summary>
        [Fact]
        public void NormalizeLogFile_ExistingDirectory_AppendsFileName()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("logs");
                Directory.CreateDirectory(directory);

                Assert.Equal(Path.Combine(directory, "log.txt"), StoragePath.NormalizeLogFile(directory));
            }
        }

        /// <summary>
        /// 已经带文件名的路径保持原样，允许用户自定义日志文件名。
        /// </summary>
        [Fact]
        public void NormalizeLogFile_FilePath_KeepsName()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string file = Path.Combine(temp.File("logs"), "meter.log");

                Assert.Equal(file, StoragePath.NormalizeLogFile(file));
            }
        }

        /// <summary>
        /// 比较路径时忽略大小写与结尾分隔符，避免同一目录被当成两处。
        /// </summary>
        [Fact]
        public void AreSame_IgnoresCaseAndTrailingSeparator()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("Data");

                Assert.True(StoragePath.AreSame(directory.ToUpperInvariant() + Path.DirectorySeparatorChar, directory));
                Assert.False(StoragePath.AreSame(directory, temp.File("Logs")));
                Assert.False(StoragePath.AreSame(null, directory));
            }
        }
    }

    /// <summary>
    /// 启动装配：配置的目录建不出来时要回落到程序目录，并把原因带回给界面。
    /// 程序若直接起不来，用户就没法进设置改回去。
    /// </summary>
    public class SystemParametersTests
    {
        /// <summary>
        /// 配置的目录可用时按配置建目录，不产生警告。
        /// </summary>
        [Fact]
        public void ResolvePaths_ConfiguredDirectory_IsUsed()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string custom = temp.File("custom-data");
                StorageConfig storage = new StorageConfig { DataDirectory = custom };

                string warning;
                AppPaths paths = SystemParameters.ResolvePaths(temp.Root, storage, out warning);

                Assert.Null(warning);
                Assert.Equal(custom, paths.DataDirectory);
                Assert.True(Directory.Exists(paths.GetDataDirectory(MeterType.Adl400)));
            }
        }

        /// <summary>
        /// 目录建不出来（路径中间是个文件、盘符不存在等）时回落到程序目录并给出提示。
        /// </summary>
        [Fact]
        public void ResolvePaths_UnavailableDirectory_FallsBackWithWarning()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string blocker = temp.File("blocker.txt");
                File.WriteAllText(blocker, "x");
                StorageConfig storage = new StorageConfig { DataDirectory = Path.Combine(blocker, "data") };

                string warning;
                AppPaths paths = SystemParameters.ResolvePaths(temp.Root, storage, out warning);

                Assert.False(string.IsNullOrEmpty(warning));
                Assert.Equal(Path.Combine(temp.Root, "Data"), paths.DataDirectory);
                Assert.True(Directory.Exists(paths.DataDirectory));
            }
        }
    }
}
