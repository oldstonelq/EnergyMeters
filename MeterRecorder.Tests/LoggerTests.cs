using System;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 日志按天分文件：文件名的日期取自写入当天；旧版 log.txt 在启动时并入对应日期的文件，
    /// 只追加、不覆盖。
    /// </summary>
    public class LoggerTests
    {
        /// <summary>
        /// 文件名固定为 yyyy-MM-dd.log，方便按日期直接定位。
        /// </summary>
        [Fact]
        public void BuildFilePath_UsesDateAndLogExtension()
        {
            string path = Logger.BuildFilePath(@"D:\logs", new DateTime(2026, 11, 18, 9, 30, 0));

            Assert.Equal(@"D:\logs\2026-11-18.log", path);
        }

        /// <summary>
        /// 没有旧文件时什么都不做，返回 null。
        /// </summary>
        [Fact]
        public void MigrateLegacyLogFile_NoLegacyFile_ReturnsNull()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                Assert.Null(Logger.MigrateLegacyLogFile(temp.Root));
                Assert.Empty(Directory.GetFiles(temp.Root));
            }
        }

        /// <summary>
        /// 旧 log.txt 按最后修改时间归到对应日期，原文件删除，内容完整保留。
        /// </summary>
        [Fact]
        public void MigrateLegacyLogFile_MovesByLastWriteDate()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("logs");
                Directory.CreateDirectory(directory);
                string legacy = Path.Combine(directory, "log.txt");
                File.WriteAllText(legacy, "old-line");
                File.SetLastWriteTime(legacy, new DateTime(2026, 11, 17, 10, 0, 0));

                string migrated = Logger.MigrateLegacyLogFile(directory);

                Assert.Equal(Path.Combine(directory, "2026-11-17.log"), migrated);
                Assert.False(File.Exists(legacy));
                Assert.Contains("old-line", File.ReadAllText(migrated));
            }
        }

        /// <summary>
        /// 目标日期已有日志时追加进去，不能覆盖当天已经写下的内容。
        /// </summary>
        [Fact]
        public void MigrateLegacyLogFile_AppendsWhenTargetExists()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string directory = temp.File("logs");
                Directory.CreateDirectory(directory);
                string target = Path.Combine(directory, "2026-11-17.log");
                File.WriteAllText(target, "today-line\r\n");

                string legacy = Path.Combine(directory, "log.txt");
                File.WriteAllText(legacy, "old-line");
                File.SetLastWriteTime(legacy, new DateTime(2026, 11, 17, 9, 0, 0));

                string migrated = Logger.MigrateLegacyLogFile(directory);

                Assert.Equal(target, migrated);
                Assert.False(File.Exists(legacy));
                string content = File.ReadAllText(target);
                Assert.Contains("today-line", content);
                Assert.Contains("old-line", content);
            }
        }

        /// <summary>
        /// 目录不存在或为空时不动作，首次启动不该抛异常。
        /// </summary>
        [Fact]
        public void MigrateLegacyLogFile_BlankDirectory_ReturnsNull()
        {
            Assert.Null(Logger.MigrateLegacyLogFile(null));
            Assert.Null(Logger.MigrateLegacyLogFile("  "));
        }
    }
}
