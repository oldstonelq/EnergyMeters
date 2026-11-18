using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 文件辅助：ini 读写与表格导出为 CSV。
    /// </summary>
    public class FileHelp
    {
        /// <summary>
        /// 写入 ini 的一项；节点或文件不存在时由系统接口负责创建。
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键名</param>
        /// <param name="val">键值</param>
        /// <param name="filePath">ini 文件路径</param>
        /// <returns>写入成功返回非零值</returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读取 ini 的一项。键不存在时返回默认值，由调用方决定如何回落。
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键名</param>
        /// <param name="defaultValue">键不存在时的返回值</param>
        /// <param name="retVal">接收缓冲</param>
        /// <param name="size">缓冲长度</param>
        /// <param name="filePath">ini 文件路径</param>
        /// <returns>实际读取到的字符数</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 保存 ini 中的一项。
        /// </summary>
        /// <param name="section">节点/段落名称</param>
        /// <param name="key">项/Key 名称</param>
        /// <param name="value">值</param>
        /// <param name="filePath">ini 文件路径</param>
        /// <returns>写入成功返回 true；文件只读或路径无效时返回 false</returns>
        public static bool WriteIniKeys(string section, string key, string value, string filePath)
        {
            // 若 value 为 null 则删除该项，若 key 与 value 均为 null 则删除整个 section。
            return WritePrivateProfileString(section, key, value, filePath) != 0;
        }

        /// <summary>
        /// 读取 ini 中的一项。
        /// </summary>
        /// <param name="section">节点/段落名称</param>
        /// <param name="key">项/Key 名称</param>
        /// <param name="defaultValue">项不存在时返回的默认值</param>
        /// <param name="filePath">ini 文件路径</param>
        /// <returns>配置值；项不存在时返回 <paramref name="defaultValue"/></returns>
        public static string ReadIniKeys(string section, string key, string defaultValue, string filePath)
        {
            StringBuilder buffer = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultValue, buffer, 1024, filePath);
            return buffer.ToString();
        }

        /// <summary>
        /// 把表格中可见的列导出为 CSV。字段按标准规则转义：含逗号、引号或换行的值加引号、
        /// 内部引号双写。早先每个字段后先加制表符再补逗号，Excel 看着没问题，但用程序读取
        /// 会得到带制表符的值，每行末尾还会多出一个空列。
        /// </summary>
        /// <param name="grid">要导出的表格</param>
        /// <param name="fileName">目标文件</param>
        /// <param name="error">失败原因；成功时为空字符串</param>
        /// <returns>写入成功返回 true</returns>
        public static bool ExportCsv(DataGridView grid, string fileName, out string error)
        {
            List<int> visibleColumns = new List<int>();
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (grid.Columns[i].Visible)
                {
                    visibleColumns.Add(i);
                }
            }

            string[] headers = new string[visibleColumns.Count];
            for (int i = 0; i < visibleColumns.Count; i++)
            {
                headers[i] = grid.Columns[visibleColumns[i]].HeaderText;
            }

            string[][] rows = new string[grid.Rows.Count][];
            for (int row = 0; row < grid.Rows.Count; row++)
            {
                string[] cells = new string[visibleColumns.Count];
                for (int i = 0; i < visibleColumns.Count; i++)
                {
                    object value = grid.Rows[row].Cells[visibleColumns[i]].Value;
                    cells[i] = value == null ? string.Empty : value.ToString();
                }
                rows[row] = cells;
            }

            return WriteCsv(fileName, BuildCsvLines(headers, rows), FileMode.Create, out error);
        }

        /// <summary>
        /// 把表头与数据行拼成 CSV 文本。抽出来便于单元测试，不必构造 DataGridView。
        /// </summary>
        /// <param name="headers">表头字段</param>
        /// <param name="rows">数据行</param>
        internal static string[] BuildCsvLines(IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
        {
            string[] lines = new string[rows.Count + 1];
            lines[0] = JoinCsvFields(headers);
            for (int i = 0; i < rows.Count; i++)
            {
                lines[i + 1] = JoinCsvFields(rows[i]);
            }
            return lines;
        }

        /// <summary>
        /// 按 CSV 规则拼接一行：字段之间用逗号，字段本身按需加引号。
        /// </summary>
        /// <param name="fields">一行中的各字段</param>
        internal static string JoinCsvFields(IReadOnlyList<string> fields)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(EscapeCsvField(fields[i]));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 转义单个字段：含逗号、引号、回车或换行时整体加引号，内部引号双写。
        /// </summary>
        /// <param name="value">字段值</param>
        internal static string EscapeCsvField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static bool WriteCsv(string filePath, string[] lines, FileMode fileMode, out string error)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, fileMode))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding("gb2312")))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                }
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
