using System;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 日志轮转：超过阈值后归档为 .1，当前文件重新开始；轮转失败不能影响写日志。
    /// </summary>
    public class LoggerTests
    {
        /// <summary>
        /// 没到阈值时保持原文件，不做多余的文件操作。
        /// </summary>
        [Fact]
        public void RotateIfTooLarge_SmallFile_IsKept()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("log.txt");
                File.WriteAllText(path, "small");

                Logger.RotateIfTooLarge(path, 1024);

                Assert.True(File.Exists(path));
                Assert.False(File.Exists(path + ".1"));
                Assert.Equal("small", File.ReadAllText(path));
            }
        }

        /// <summary>
        /// 超过阈值时改名为 .1，原路径腾空供后续写入。
        /// </summary>
        [Fact]
        public void RotateIfTooLarge_LargeFile_IsArchived()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("log.txt");
                File.WriteAllText(path, new string('x', 2048));

                Logger.RotateIfTooLarge(path, 1024);

                Assert.False(File.Exists(path));
                Assert.True(File.Exists(path + ".1"));
                Assert.Equal(2048, new FileInfo(path + ".1").Length);
            }
        }

        /// <summary>
        /// 已有上一次的归档时用新的覆盖，避免归档文件无限累积。
        /// </summary>
        [Fact]
        public void RotateIfTooLarge_ReplacesPreviousArchive()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("log.txt");
                File.WriteAllText(path + ".1", "old");
                File.WriteAllText(path, new string('y', 2048));

                Logger.RotateIfTooLarge(path, 1024);

                Assert.Equal(2048, new FileInfo(path + ".1").Length);
                Assert.DoesNotContain("old", File.ReadAllText(path + ".1"));
            }
        }

        /// <summary>
        /// 文件不存在时什么都不做，首次写日志不该抛异常。
        /// </summary>
        [Fact]
        public void RotateIfTooLarge_MissingFile_DoesNothing()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("missing.txt");

                Logger.RotateIfTooLarge(path, 1024);

                Assert.False(File.Exists(path));
                Assert.False(File.Exists(path + ".1"));
            }
        }
    }
}
