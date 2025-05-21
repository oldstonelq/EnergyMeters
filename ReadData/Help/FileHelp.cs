using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;


namespace ReadDataSoftware
{
    public  class FileHelp
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 保存ini
        /// </summary>
        /// <param name="section">节点/段落名称</param>
        /// <param name="key">项/Key名称</param>
        /// <param name="value">值</param>
        /// <param name="filePath">ini文件路径</param>
        public static void WriteIniKeys(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        /*
         * 若value为null则会删除配置文件中对应的key
         * 若key value为null则会删除对应的section
        */


        /// <summary>
        /// 根据section，key取值,并设置默认值
        /// </summary>
        /// <param name="section">节点/段落名称</param>
        /// <param name="key">项/Key名称</param>
        /// <param name="def">默认值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>返回指定内容，若不存在则返回默认值def</returns>
        public static string ReadIniKeys(string section, string key, string def, string filePath)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, temp, 1024, filePath);
            return temp.ToString();
        }


        /// <summary>
        /// 读取csv文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadCsvFile(string filePath)
        {
            StringBuilder queryBuilder = new StringBuilder();
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                foreach (string value in values)
                {
                    // 转义双引号
                    string escapedValue = value.Replace("\"", "");
                    queryBuilder.Append(escapedValue);
                    queryBuilder.AppendLine();
                }
            }
            return queryBuilder.ToString();
        }
        /// <summary>
        /// 读取excel文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadExcelFile(string filePath)
        {
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
            Excel.Worksheet worksheet = workbook.Sheets[1];
            Excel.Range range = worksheet.UsedRange;

            StringBuilder queryBuilder = new StringBuilder();
            for (int row = 1; row <= range.Rows.Count; row++)
            {
                for (int col = 1; col <= range.Columns.Count; col++)
                {
                    if (range.Cells[row, col]?.Value2 != null)
                    {
                        queryBuilder.Append(range.Cells[row, col].Value2.ToString());
                        queryBuilder.AppendLine();
                    }
                }
            }

            workbook.Close(false);
            excelApp.Quit();
            return queryBuilder.ToString();
        }
        /// <summary>
        /// 导出DGV表格
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="fileName"></param>
        /// <param name="appendTexts"></param>
        /// <returns></returns>
        public static bool ExportCsv(DataGridView dgv, string fileName, string[] appendTexts = null)
        {
            List<string> mList = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible) sb.Append(dgv.Columns[i].HeaderText + "\t,");
            }
            mList.Add(sb.ToString());
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                sb.Clear();
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    if (!dgv.Columns[j].Visible) continue;
                    if (dgv.Rows[i].Cells[j].Value != null) sb.Append(dgv.Rows[i].Cells[j].Value.ToString() + "\t");
                    sb.Append(",");
                }
                mList.Add(sb.ToString());
            }
            if (appendTexts != null && appendTexts.Length > 0)
            {
                mList.AddRange(appendTexts);
            }
            return FileHelp.WriteCSV(fileName, mList.ToArray(), System.IO.FileMode.Create) == "ok";
        }

        /// <summary>
        /// CSV文件写入多行
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Mess">写入信息内容</param>
        /// <param name="Filemode">文件打开模式</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string WriteCSV(string FilePath, string[] Mess, FileMode Filemode)
        {
            try
            {
                FileStream fs = new FileStream(FilePath, Filemode);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                for (int i = 0; i < Mess.Length; i++)
                {
                    sw.WriteLine(Mess[i]);
                }
                sw.Close();
                fs.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
