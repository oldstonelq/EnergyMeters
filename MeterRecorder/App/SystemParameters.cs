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
        /// 初始化系统参数并创建电表实例。
        /// </summary>
        public static void Load()
        {
            string startupPath = Application.StartupPath;

            // 先按默认位置建好配置目录与 ini，才能读到数据目录要不要改。
            AppPaths defaults = new AppPaths(startupPath);
            defaults.EnsureConfigFile();

            Device = DeviceConfig.Load(defaults.SystemFile);
            StorageConfig storage = StorageConfig.Load(defaults.SystemFile);

            // 数据目录来自配置；为空则仍是程序目录下的 Data。
            Paths = new AppPaths(startupPath, storage.DataDirectory);
            Paths.EnsureCreated();

            EnergyMeters = MeterFactory.Create(Device);
        }
    }
}
