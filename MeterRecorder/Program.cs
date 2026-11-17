using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 程序入口。
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 命名互斥体。必须由静态字段持有：局部变量在优化后可能在 Application.Run
        /// 之前就被回收，句柄一关，第二个实例就能启动并抢同一个串口。
        /// </summary>
        private static Mutex s_instanceMutex;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool canCreateNew = false;
            s_instanceMutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out canCreateNew);
            if (!canCreateNew)
            {
                MessageBox.Show("程序已在运行中，不能同时执行多个程序");
                s_instanceMutex.Dispose();
                return;
            }
            try
            {
                SystemParameters.Load();
                Application.Run(new MainForm());
            }
            finally
            {
                try
                {
                    s_instanceMutex.ReleaseMutex();
                }
                catch (ApplicationException)
                {
                    // 互斥体已不属于当前线程时忽略，进程退出后由系统回收。
                }
                s_instanceMutex.Dispose();
            }
           
        }
    }
}
