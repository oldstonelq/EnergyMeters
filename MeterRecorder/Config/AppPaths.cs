using System;
using System.IO;

namespace MeterRecorder
{
    /// <summary>
    /// 应用的目录与文件位置，构造后只读。
    /// </summary>
    public sealed class AppPaths
    {
        /// <summary>
        /// 按程序所在目录构造路径集合。
        /// </summary>
        /// <param name="startupPath">程序所在目录</param>
        /// <param name="dataRoot">
        /// 数据根目录。为 null 或空白时取程序目录下的 Data，与旧版本行为一致。
        /// </param>
        /// <param name="logDirectory">
        /// 日志目录。为 null 或空白时取程序目录下的 Logs。日志文件按天生成，
        /// 文件名由 <see cref="GetLogFile"/> 决定。
        /// </param>
        public AppPaths(string startupPath, string dataRoot = null, string logDirectory = null)
        {
            StartupPath = startupPath;
            SystemDirectory = Path.Combine(startupPath, "System");
            SystemFile = Path.Combine(SystemDirectory, "system.ini");
            DataDirectory = string.IsNullOrWhiteSpace(dataRoot)
                ? Path.Combine(startupPath, "Data")
                : dataRoot.Trim();
            LogDirectory = string.IsNullOrWhiteSpace(logDirectory)
                ? Path.Combine(startupPath, "Logs")
                : Path.GetFullPath(logDirectory.Trim());
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
        /// 取指定日期的日志文件，如 Logs\2026-11-18.log。按天分文件，写入时不覆盖历史。
        /// </summary>
        /// <param name="date">日志日期</param>
        public string GetLogFile(DateTime date)
        {
            return Path.Combine(LogDirectory, date.ToString("yyyy-MM-dd") + ".log");
        }

        /// <summary>
        /// 取指定表型的数据目录，如 Data\ADL400。
        /// </summary>
        /// <param name="meterType">表型，决定子目录名，如 ADL400</param>
        public string GetDataDirectory(MeterType meterType)
        {
            return Path.Combine(DataDirectory, meterType.ToConfigValue());
        }

        /// <summary>
        /// 只确保配置目录与 ini 文件存在。程序启动时先读配置，再决定数据目录放哪，
        /// 所以建数据目录的事交给 <see cref="EnsureCreated"/>。
        /// </summary>
        public void EnsureConfigFile()
        {
            Directory.CreateDirectory(SystemDirectory);
            if (!File.Exists(SystemFile))
            {
                // File.Create 返回的流必须释放，否则句柄会一直占用到 GC，
                // 导致后续 WritePrivateProfileString 写配置失败。
                using (File.Create(SystemFile))
                {
                }
            }
        }

        /// <summary>
        /// 确保程序运行所需的目录存在。数据目录可能来自配置，不存在时在这里创建。
        /// </summary>
        public void EnsureCreated()
        {
            EnsureConfigFile();
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(LogDirectory);
            foreach (MeterType meterType in new[] { MeterType.Djsf1352, MeterType.Amc, MeterType.Adl400 })
            {
                Directory.CreateDirectory(GetDataDirectory(meterType));
            }
        }
    }
}
