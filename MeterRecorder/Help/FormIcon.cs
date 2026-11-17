using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 窗体图标。图标作为 exe 资源随程序发布（工程里的 <c>ApplicationIcon</c>），
    /// 不放在窗体的 resx 里 —— 那里存的是二进制对象，会让命令行构建额外依赖
    /// System.Resources.Extensions，而 Visual Studio 不需要它，两边行为不一致。
    /// </summary>
    internal static class FormIcon
    {
        /// <summary>
        /// 取 exe 的应用图标。设计时或读取失败时返回 null，窗体沿用默认图标。
        /// </summary>
        public static Icon Load()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return null;
            }
            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
