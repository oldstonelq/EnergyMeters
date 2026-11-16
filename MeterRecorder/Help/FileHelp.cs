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
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
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
        /// 把表格中可见的列导出为 CSV。
        /// </summary>
        /// <param name="grid">要导出的表格</param>
        /// <param name="fileName">目标文件</param>
        /// <param name="error">失败原因；成功时为空字符串</param>
        /// <param name="appendTexts">追加在末尾的额外行</param>
        /// <returns>写入成功返回 true</returns>
        public static bool ExportCsv(DataGridView grid, string fileName, out string error, string[] appendTexts = null)
        {
            List<string> lines = new List<string>();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (grid.Columns[i].Visible)
                {
                    builder.Append(grid.Columns[i].HeaderText + "\t,");
                }
            }
            lines.Add(builder.ToString());

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                builder.Clear();
                for (int j = 0; j < grid.Columns.Count; j++)
                {
                    if (!grid.Columns[j].Visible)
                    {
                        continue;
                    }
                    if (grid.Rows[i].Cells[j].Value != null)
                    {
                        builder.Append(grid.Rows[i].Cells[j].Value.ToString() + "\t");
                    }
                    builder.Append(",");
                }
                lines.Add(builder.ToString());
            }

            if (appendTexts != null && appendTexts.Length > 0)
            {
                lines.AddRange(appendTexts);
            }

            return WriteCsv(fileName, lines.ToArray(), FileMode.Create, out error);
        }

        /// <summary>
        /// 按 GB2312 编码把多行文本写入 CSV 文件。
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="lines">要写入的每一行</param>
        /// <param name="fileMode">文件打开模式</param>
        /// <param name="error">失败原因；成功时为空字符串</param>
        /// <returns>写入成功返回 true</returns>
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
