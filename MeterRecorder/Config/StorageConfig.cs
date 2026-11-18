using System.IO;

namespace MeterRecorder
{
    /// <summary>
    /// 存储设置：数据根目录与日志目录。与串口配置同放在 ini 的 SystemSet 节，
    /// 单独一个类是因为它们不属于「设备」。
    /// </summary>
    public sealed class StorageConfig
    {
        /// <summary>
        /// ini 中的节点名，与串口配置共用 SystemSet 节。
        /// </summary>
        private const string SectionName = "SystemSet";

        /// <summary>
        /// 数据目录 Key，写入 ini 时用这个名字。
        /// </summary>
        private const string DataDirectoryKey = "DataDirectory";

        /// <summary>
        /// 日志目录 Key。
        /// </summary>
        private const string LogDirectoryKey = "LogDirectory";

        /// <summary>
        /// 旧版本记录日志完整路径的 Key。只为兼容升级前的配置而读取，
        /// 保存时会删掉它，避免旧路径继续生效。
        /// </summary>
        private const string LegacyLogFileKey = "LogFile";

        /// <summary>
        /// 数据根目录。为 null 或空白表示跟随程序目录下的 Data，
        /// 这样把整个文件夹拷到别的机器上仍然能用。
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// 日志目录。日志文件按天生成（如 2026-11-18.log），写入时不覆盖历史。
        /// 为 null 或空白表示跟随程序目录下的 Logs。
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// 从 ini 读取存储设置。键缺失或为空时表示使用默认目录，不抛异常。
        /// 旧版本存的是 LogFile（完整文件路径），这里取其所在目录。
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

            string logDirectory = FileHelp.ReadIniKeys(SectionName, LogDirectoryKey, string.Empty, iniPath);
            if (!string.IsNullOrWhiteSpace(logDirectory))
            {
                config.LogDirectory = logDirectory.Trim();
            }
            else
            {
                // 升级前的配置只有 LogFile。取它的目录部分，用户不必手工改 ini。
                string legacyLogFile = FileHelp.ReadIniKeys(SectionName, LegacyLogFileKey, string.Empty, iniPath);
                config.LogDirectory = ToDirectory(legacyLogFile);
            }

            return config;
        }

        /// <summary>
        /// 写回 ini。目录为空时写入空值，表示回到「跟随程序目录」。
        /// 同时删除旧版的 LogFile 键，避免它继续影响读取结果。
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <exception cref="IOException">文件只读、被占用或路径无效</exception>
        public void Save(string iniPath)
        {
            string dataDirectory = string.IsNullOrWhiteSpace(DataDirectory) ? string.Empty : DataDirectory.Trim();
            string logDirectory = string.IsNullOrWhiteSpace(LogDirectory) ? string.Empty : LogDirectory.Trim();

            bool written = FileHelp.WriteIniKeys(SectionName, DataDirectoryKey, dataDirectory, iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, LogDirectoryKey, logDirectory, iniPath);
            if (!written)
            {
                throw new IOException("写入配置文件失败：" + iniPath + "，请检查文件是否只读或被其他程序占用。");
            }

            // 传 null 表示删除该项。删不掉不影响本次保存，下次读取也优先用新键。
            FileHelp.WriteIniKeys(SectionName, LegacyLogFileKey, null, iniPath);
        }

        /// <summary>
        /// 把旧版记录的文件路径转成目录；路径无效时返回 null。
        /// </summary>
        /// <param name="logFilePath">旧版 ini 里的 LogFile 取值</param>
        private static string ToDirectory(string logFilePath)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                return null;
            }

            string fullPath = Path.GetFullPath(logFilePath.Trim());
            string directory = Path.GetDirectoryName(fullPath);
            return string.IsNullOrEmpty(directory) ? null : directory;
        }
    }
}
