using System;
using System.IO;

namespace MeterRecorder
{
    /// <summary>
    /// 存储路径的规范化与比较。设置窗体与启动装配共用同一套规则，
    /// 避免「界面上按一种写法保存、启动时按另一种解释」。
    /// </summary>
    public static class StoragePath
    {
        /// <summary>
        /// 规范化数据目录：去掉首尾空白并转成完整路径，空白输入返回 null。
        /// </summary>
        /// <param name="text">用户输入或 ini 里的路径</param>
        /// <returns>完整路径；输入为空白时返回 null</returns>
        public static string NormalizeDirectory(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            return Path.GetFullPath(text.Trim());
        }

        /// <summary>
        /// 判断两个路径是否指向同一位置：忽略大小写与结尾的分隔符。
        /// 用来识别「用户选的就是默认目录」，这种情况配置里写空值，搬动机器仍能跟随。
        /// </summary>
        /// <param name="left">第一个路径</param>
        /// <param name="right">第二个路径</param>
        public static bool AreSame(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            char separator = Path.DirectorySeparatorChar;
            return string.Equals(
                left.Trim().TrimEnd(separator),
                right.Trim().TrimEnd(separator),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
