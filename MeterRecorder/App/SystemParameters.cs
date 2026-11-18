using System;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 组合根：只在启动时装配目录、配置与电表实例，运行期不再承担其他职责。
    /// </summary>
    public static class SystemParameters
    {
        /// <summary>
        /// 目录与文件位置。
        /// </summary>
        public static AppPaths Paths { get; private set; }

        /// <summary>
        /// 当前设备配置。
        /// </summary>
        public static DeviceConfig Device { get; private set; }

        /// <summary>
        /// 当前电表实例。
        /// </summary>
        public static IEnergyMeter EnergyMeters { get; private set; }

        /// <summary>
        /// 启动时的非致命提示。目前只有一种：配置的存储目录不可用、已回落到程序目录。
        /// 为空表示一切正常，由主窗体加载后提示用户。
        /// </summary>
        public static string StartupWarning { get; private set; }

        /// <summary>
        /// 初始化系统参数并创建电表实例。
        /// </summary>
        public static void Load()
        {
            string startupPath = Application.StartupPath;

            // 先按默认位置建好配置目录与 ini，才能读到数据目录与日志路径要不要改。
            AppPaths defaults = new AppPaths(startupPath);
            defaults.EnsureConfigFile();

            Device = DeviceConfig.Load(defaults.SystemFile);
            StorageConfig storage = StorageConfig.Load(defaults.SystemFile);

            string warning;
            Paths = ResolvePaths(startupPath, storage, out warning);
            StartupWarning = warning;

            // 旧版本把日志写进固定的 log.txt，升级后并入按天文件，目录里不会新旧混放。
            Logger.MigrateLegacyLogFile(Paths.LogDirectory);

            if (!string.IsNullOrEmpty(warning))
            {
                Logger.Log(warning);
            }

            EnergyMeters = MeterFactory.Create(Device);
        }

        /// <summary>
        /// 按配置解析数据与日志目录。配置里的目录建不出来时（U 盘拔出、网络盘断开、
        /// 权限不足）回落到程序目录并给出提示：改目录的入口在设置窗体里，程序若起不来，
        /// 用户就没有办法改回去。
        /// </summary>
        /// <param name="startupPath">程序所在目录</param>
        /// <param name="storage">存储配置，可为 null</param>
        /// <param name="warning">回落时的提示文字；正常时为空</param>
        internal static AppPaths ResolvePaths(string startupPath, StorageConfig storage, out string warning)
        {
            warning = null;

            AppPaths configured = new AppPaths(
                startupPath,
                storage == null ? null : storage.DataDirectory,
                storage == null ? null : storage.LogDirectory);
            try
            {
                configured.EnsureCreated();
                return configured;
            }
            catch (Exception ex)
            {
                AppPaths fallback = new AppPaths(startupPath);
                fallback.EnsureCreated();
                warning = string.Format(
                    "配置的存储目录不可用（{0}），已临时改用程序目录：{1}。\n" +
                    "请检查磁盘是否插好，或到「设置」里重新选择目录。",
                    ex.Message, fallback.DataDirectory);
                return fallback;
            }
        }
    }
}
