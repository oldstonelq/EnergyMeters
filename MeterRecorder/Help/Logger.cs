using System;
using System.IO;
using System.Text;

namespace MeterRecorder
{
    /// <summary>
    /// 简易日志。按天写入 <see cref="AppPaths.LogDirectory"/> 下的 yyyy-MM-dd.log，
    /// 同一天内追加、不覆盖历史；写不进去时静默忽略，避免日志本身影响采集与保存。
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 多个线程可能同时写日志，串行化文件写入。
        /// </summary>
        private static readonly object s_writeLock = new object();

        /// <summary>
        /// 旧版本使用的固定文件名。升级时把它并入按天文件，目录里不会新旧命名混放。
        /// </summary>
        internal const string LegacyLogFileName = "log.txt";

        /// <summary>
        /// 记录一条消息。
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Log(string message)
        {
            string directory = ResolveDirectory();
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string line = string.Format("{0} {1}{2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message, Environment.NewLine);
                lock (s_writeLock)
                {
                    File.AppendAllText(BuildFilePath(directory, DateTime.Now), line, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // 磁盘满、文件被占用时不再上抛：日志失败不应中断主流程。
            }
        }

        /// <summary>
        /// 记录一条异常，附带上下文与堆栈。
        /// </summary>
        /// <param name="context">发生位置或正在做什么</param>
        /// <param name="error">异常；为 null 时只记录上下文</param>
        public static void Log(string context, Exception error)
        {
            if (error == null)
            {
                Log(context);
                return;
            }
            Log(context + "：" + error.Message + error.StackTrace);
        }

        /// <summary>
        /// 取指定日期对应的日志文件，如 Logs\2026-11-18.log。
        /// </summary>
        /// <param name="directory">日志目录</param>
        /// <param name="date">日志日期</param>
        internal static string BuildFilePath(string directory, DateTime date)
        {
            return Path.Combine(directory, date.ToString("yyyy-MM-dd") + ".log");
        }

        /// <summary>
        /// 把旧版本的 log.txt 并入按天文件，返回迁移后的路径；没有旧文件时返回 null。
        /// 按旧文件的最后修改时间归属日期；目标文件已存在时追加内容而不是覆盖。
        /// </summary>
        /// <param name="directory">日志目录</param>
        internal static string MigrateLegacyLogFile(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return null;
            }

            string legacyPath = Path.Combine(directory, LegacyLogFileName);
            if (!File.Exists(legacyPath))
            {
                return null;
            }

            try
            {
                string targetPath = BuildFilePath(directory, File.GetLastWriteTime(legacyPath));
                if (File.Exists(targetPath))
                {
                    // 目标已有当天的日志，追加进去而不是覆盖。
                    File.AppendAllText(targetPath, File.ReadAllText(legacyPath), Encoding.UTF8);
                    File.Delete(legacyPath);
                }
                else
                {
                    File.Move(legacyPath, targetPath);
                }
                return targetPath;
            }
            catch (Exception)
            {
                // 迁移失败就保持原样：下次启动会再试，不影响正常写日志。
                return null;
            }
        }

        /// <summary>
        /// 取日志目录。组合根尚未初始化时退回程序目录，
        /// 否则「启动装配阶段出错」这类最需要日志的场景反而什么都记不下来。
        /// </summary>
        private static string ResolveDirectory()
        {
            AppPaths paths = SystemParameters.Paths;
            if (paths != null)
            {
                return paths.LogDirectory;
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        }
    }
}
