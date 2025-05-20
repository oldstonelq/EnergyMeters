using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    /// <summary>
    /// 系统参数
    /// </summary>
    public  class SystemParas
    {
        /// <summary>
        /// 系统路径
        /// </summary>
        public static string SystemPath = Application.StartupPath + @"\System";
        /// <summary>
        /// 系统配置文件
        /// </summary>
        public static string SystemFile = SystemPath + @"\system.ini";

        /// <summary>
        /// 初始化系统参数
        /// </summary>
        public static void Load()
        {
            //// 检查系统文件夹是否存在
            if (!Directory.Exists(SystemPath))
            {
                Directory.CreateDirectory(SystemPath);
            }
            //// 检查系统文件是否存在
            if (!File.Exists(SystemFile))
            {
                File.Create(SystemFile);
            }
            //// 读取系统参数
            var str = FileHelp.ReadIniKeys("SystemSet", "API_KEY", "", SystemFile);
            if (!string.IsNullOrEmpty(str))
            {
               
            }
        }

    }
}
