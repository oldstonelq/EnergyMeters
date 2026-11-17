using System.IO;

namespace MeterRecorder
{
    /// <summary>
    /// 存储设置：数据文件存放的根目录。与串口配置同放在 ini 的 SystemSet 节，
    /// 单独一个类是因为它不属于「设备」，将来日志目录等存储项也归这里。
    /// </summary>
    public sealed class StorageConfig
    {
        /// <summary>
        /// ini 中的节点名，与串口配置共用 SystemSet 节。
        /// </summary>
        private const string SectionName = "SystemSet";

        /// <summary>
        /// 数据文件 Key，写入 ini 时用这个名字。
        /// </summary>
        private const string DataDirectoryKey = "DataDirectory";

        /// <summary>
        /// 日志文件 Key，写入 ini 时用这个名字。
        /// </summary>
        private const string LogFileKey = "LogFile";

        /// <summary>
        /// 数据根目录。为 null 或空白表示跟随程序目录下的 Data，
        /// 这样把整个文件夹拷到别的机器上仍然能用。
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// 日志文件的完整路径，含文件名。为 null 或空白表示跟随程序目录下的 Logs\log.txt。
        /// </summary>
        public string LogFile { get; set; }

        /// <summary>
        /// 从 ini 读取存储设置。键缺失或为空时表示使用默认目录，不抛异常。
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        public static StorageConfig Load(string iniPath)
        {
            StorageConfig config = new StorageConfig();

            string directory = FileHelp.ReadIniKeys(SectionName, DataDirectoryKey, string.Empty, iniPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                config.DataDirectory = directory.Trim();
            }

            string logFile = FileHelp.ReadIniKeys(SectionName, LogFileKey, string.Empty, iniPath);
            if (!string.IsNullOrWhiteSpace(logFile))
            {
                config.LogFile = logFile.Trim();
            }

            return config;
        }

        /// <summary>
        /// 写回 ini。目录为空时写入空值，表示回到「跟随程序目录」。
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <exception cref="IOException">文件只读、被占用或路径无效</exception>
        public void Save(string iniPath)
        {
            string directory = string.IsNullOrWhiteSpace(DataDirectory) ? string.Empty : DataDirectory.Trim();
            string logFile = string.IsNullOrWhiteSpace(LogFile) ? string.Empty : LogFile.Trim();
            bool written = FileHelp.WriteIniKeys(SectionName, DataDirectoryKey, directory, iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, LogFileKey, logFile, iniPath);
            if (!written)
            {
                throw new IOException("写入配置文件失败：" + iniPath + "，请检查文件是否只读或被其他程序占用。");
            }
        }
    }
}
