using System.IO;
using System.IO.Ports;
using System;

namespace ReadDataSoftware
{
    /// <summary>
    /// 设备配置：串口参数、表型与从站地址。只在装配阶段构造，运行期不再全局可变。
    /// </summary>
    public sealed class DeviceConfig
    {
        /// <summary>
        /// ini 中的节点名。
        /// </summary>
        private const string SectionName = "SystemSet";
        /// <summary>
        /// 波特率的合法下限。
        /// </summary>
        private const int MinBaudRate = 300;
        /// <summary>
        /// 波特率的合法上限。
        /// </summary>
        private const int MaxBaudRate = 115200;

        /// <summary>
        /// 串口名称，如 "COM3"。
        /// </summary>
        public string PortName { get; set; } = "COM3";

        /// <summary>
        /// 波特率。
        /// </summary>
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 数据位。
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 停止位。
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// 校验位。
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// 电表型号。
        /// </summary>
        public MeterType MeterType { get; set; } = MeterType.Djsf1352;

        /// <summary>
        /// 从站地址。为 null 时使用该表型的内置默认值。
        /// </summary>
        public int? SlaveAddress { get; set; }

        /// <summary>
        /// 复制一份配置，便于在设置界面里编辑后再决定是否生效。
        /// </summary>
        public DeviceConfig Clone()
        {
            return new DeviceConfig
            {
                PortName = PortName,
                BaudRate = BaudRate,
                DataBits = DataBits,
                StopBits = StopBits,
                Parity = Parity,
                MeterType = MeterType,
                SlaveAddress = SlaveAddress
            };
        }

        /// <summary>
        /// 从 ini 读取配置。任何一项非法都回落到默认值，不抛异常。
        /// </summary>
        public static DeviceConfig Load(string iniPath)
        {
            DeviceConfig config = new DeviceConfig();

            string portName = FileHelp.ReadIniKeys(SectionName, "PortName", string.Empty, iniPath);
            if (!string.IsNullOrWhiteSpace(portName))
            {
                config.PortName = portName.Trim();
            }

            config.BaudRate = ReadInt(iniPath, "BaudRate", config.BaudRate, MinBaudRate, MaxBaudRate);
            config.DataBits = ReadInt(iniPath, "DataBits", config.DataBits, 5, 8);

            config.StopBits = ReadEnum(iniPath, "StopBits", config.StopBits);
            config.Parity = ReadEnum(iniPath, "Parity", config.Parity);

            string meterType = FileHelp.ReadIniKeys(SectionName, "EnergyMetersType", string.Empty, iniPath);
            MeterType parsedMeterType;
            if (MeterTypeExtensions.TryParseConfigValue(meterType, out parsedMeterType))
            {
                config.MeterType = parsedMeterType;
            }

            int slaveAddress = ReadInt(iniPath, "SlaveAddress", 0, 0, 247);
            config.SlaveAddress = slaveAddress > 0 ? slaveAddress : (int?)null;

            return config;
        }

        /// <summary>
        /// 写回 ini。任一项写入失败都会抛出异常，避免配置文件只写了一半却提示成功。
        /// </summary>
        /// <exception cref="IOException">文件只读、被占用或路径无效</exception>
        public void Save(string iniPath)
        {
            bool written = FileHelp.WriteIniKeys(SectionName, "PortName", PortName, iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "BaudRate", BaudRate.ToString(), iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "DataBits", DataBits.ToString(), iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "StopBits", StopBits.ToString(), iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "Parity", Parity.ToString(), iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "EnergyMetersType", MeterType.ToConfigValue(), iniPath);
            written &= FileHelp.WriteIniKeys(SectionName, "SlaveAddress",
                SlaveAddress.HasValue ? SlaveAddress.Value.ToString() : string.Empty, iniPath);

            if (!written)
            {
                throw new IOException("写入配置文件失败：" + iniPath + "，请检查文件是否只读或被其他程序占用。");
            }
        }

        /// <summary>
        /// 读取整数值。缺失、无法解析或超出范围时返回默认值。
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="key">键名</param>
        /// <param name="defaultValue">回落的默认值</param>
        /// <param name="minValue">允许的最小值</param>
        /// <param name="maxValue">允许的最大值</param>
        private static int ReadInt(string iniPath, string key, int defaultValue, int minValue, int maxValue)
        {
            string text = FileHelp.ReadIniKeys(SectionName, key, string.Empty, iniPath);
            int parsed;
            if (int.TryParse(text, out parsed) && parsed >= minValue && parsed <= maxValue)
            {
                return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// 读取枚举值。解析失败时返回默认值，不抛异常。
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="key">键名</param>
        /// <param name="defaultValue">回落的默认值</param>
        private static TEnum ReadEnum<TEnum>(string iniPath, string key, TEnum defaultValue) where TEnum : struct
        {
            // 旧版本的校验位下拉框误用了停止位的取值，配置文件里可能残留 One / OnePointFive
            // 这类非法值，解析失败时保留默认值即可，避免启动时直接抛异常。
            string text = FileHelp.ReadIniKeys(SectionName, key, string.Empty, iniPath);
            TEnum parsed;
            if (Enum.TryParse(text, out parsed))
            {
                return parsed;
            }
            return defaultValue;
        }
    }
}
