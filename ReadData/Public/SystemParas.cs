using System.Windows.Forms;

namespace ReadDataSoftware
{
    /// <summary>
    /// 组合根：只在启动时装配目录、配置与电表实例，运行期不再承担其他职责。
    /// </summary>
    public static class SystemParas
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
        public static IEnergyMeters EnergyMeters { get; private set; }

        /// <summary>
        /// 初始化系统参数并创建电表实例。
        /// </summary>
        public static void Load()
        {
            Paths = new AppPaths(Application.StartupPath);
            Paths.EnsureCreated();
            Device = DeviceConfig.Load(Paths.SystemFile);
            EnergyMeters = MeterFactory.Create(Device);
        }
    }
}
