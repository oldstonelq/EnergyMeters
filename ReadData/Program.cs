using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool canCreateNew = false;
            Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out canCreateNew);
            if (!canCreateNew)
            {
                MessageBox.Show("程序已在运行中，不能同时执行多个程序");
                Environment.Exit(0);
            }
            SystemParas.Load();
            Application.Run(new aMainForm());
           
        }
    }
}
