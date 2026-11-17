using System;

namespace MeterRecorder
{
    /// <summary>
    /// 按配置创建对应表型的实例。
    /// </summary>
    public static class MeterFactory
    {
        /// <summary>
        /// 创建电表实例。
        /// </summary>
        /// <param name="config">设备配置</param>
        /// <exception cref="ArgumentNullException">配置为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">配置里的表型不是已知的三种之一</exception>
        public static IEnergyMeters Create(DeviceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            switch (config.MeterType)
            {
                case MeterType.Djsf1352:
                    return new Djsf1352EnergyMeters(config);
                case MeterType.Amc:
                    return new AmcEnergyMeters(config);
                case MeterType.Adl400:
                    return new Adl400EnergyMeters(config);
                default:
                    // 早先这里用 default 兜成直流表：枚举值越界时程序照常启动，
                    // 然后按直流的三个通道去记录三相表，数据错了也看不出来。
                    throw new ArgumentOutOfRangeException(nameof(config.MeterType),
                        string.Format("未知的表型取值 {0}，无法确定要采集哪种表。", (int)config.MeterType));
            }
        }
    }
}
