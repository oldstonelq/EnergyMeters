using System.IO;

namespace ReadDataSoftware
{
    /// <summary>
    /// 应用的目录与文件位置，构造后只读。
    /// </summary>
    public sealed class AppPaths
    {
        public AppPaths(string startupPath)
        {
            StartupPath = startupPath;
            SystemDirectory = Path.Combine(startupPath, "System");
            SystemFile = Path.Combine(SystemDirectory, "system.ini");
            DataDirectory = Path.Combine(startupPath, "Data");
            LogDirectory = Path.Combine(startupPath, "Logs");
            LogFile = Path.Combine(LogDirectory, "log.txt");
        }

        /// <summary>
        /// 程序所在目录。
        /// </summary>
        public string StartupPath { get; }

        /// <summary>
        /// 配置目录。
        /// </summary>
        public string SystemDirectory { get; }

        /// <summary>
        /// 配置文件路径。
        /// </summary>
        public string SystemFile { get; }

        /// <summary>
        /// 数据根目录。
        /// </summary>
        public string DataDirectory { get; }

        /// <summary>
        /// 日志目录。
        /// </summary>
        public string LogDirectory { get; }

        /// <summary>
        /// 日志文件路径。
        /// </summary>
        public string LogFile { get; }

        /// <summary>
        /// 取指定表型的数据目录，如 Data\ADL400。
        /// </summary>
        public string GetDataDirectory(MeterType meterType)
        {
            return Path.Combine(DataDirectory, meterType.ToConfigValue());
        }

        /// <summary>
        /// 确保程序运行所需的目录与配置文件存在。
        /// </summary>
        public void EnsureCreated()
        {
            Directory.CreateDirectory(SystemDirectory);
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(LogDirectory);
            foreach (MeterType meterType in new[] { MeterType.Djsf1352, MeterType.Amc, MeterType.Adl400 })
            {
                Directory.CreateDirectory(GetDataDirectory(meterType));
            }

            if (!File.Exists(SystemFile))
            {
                // File.Create 返回的流必须释放，否则句柄会一直占用到 GC，
                // 导致后续 WritePrivateProfileString 写配置失败。
                using (File.Create(SystemFile))
                {
                }
            }
        }
    }
}
