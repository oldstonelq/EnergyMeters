using System;
using System.IO;
using System.Text;

namespace MeterRecorder
{
    /// <summary>
    /// 简易日志。统一追加写入 <see cref="AppPaths.LogFile"/>，写不进去时静默忽略，
    /// 避免日志本身影响采集与保存。
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 多个线程可能同时写日志，串行化文件写入。
        /// </summary>
        private static readonly object s_writeLock = new object();

        /// <summary>
        /// 单个日志文件的大小上限。超过后改名为 log.txt.1，当前文件重新开始，
        /// 避免长期运行时文件无限增长、出问题不好翻。
        /// </summary>
        internal const long MaxLogBytes = 1024 * 1024;

        /// <summary>
        /// 记录一条消息。
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Log(string message)
        {
            string filePath = ResolveFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string line = string.Format("{0} {1}{2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message, Environment.NewLine);
                lock (s_writeLock)
                {
                    RotateIfTooLarge(filePath, MaxLogBytes);
                    File.AppendAllText(filePath, line, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // 磁盘满、文件被占用时不再上抛：日志失败不应中断主流程。
            }
        }

        /// <summary>
        /// 日志超过阈值时改名为「原文件名.1」，上一次的归档会被覆盖。
        /// 轮转失败不影响本次写入，继续追加到原文件即可。
        /// </summary>
        /// <param name="filePath">当前日志文件</param>
        /// <param name="maxBytes">大小上限</param>
        internal static void RotateIfTooLarge(string filePath, long maxBytes)
        {
            try
            {
                FileInfo info = new FileInfo(filePath);
                if (!info.Exists || info.Length < maxBytes)
                {
                    return;
                }

                string archivePath = filePath + ".1";
                if (File.Exists(archivePath))
                {
                    File.Delete(archivePath);
                }
                File.Move(filePath, archivePath);
            }
            catch (Exception)
            {
                // 文件被占用时轮转不了，继续写原文件，不能因此丢日志。
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
        /// 取日志文件路径。组合根尚未初始化时退回程序目录，
        /// 否则「启动装配阶段出错」这类最需要日志的场景反而什么都记不下来。
        /// </summary>
        private static string ResolveFilePath()
        {
            AppPaths paths = SystemParameters.Paths;
            if (paths != null)
            {
                return paths.LogFile;
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
        }
    }
}
