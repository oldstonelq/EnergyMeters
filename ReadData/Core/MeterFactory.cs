namespace ReadDataSoftware
{
    /// <summary>
    /// 按配置创建对应表型的实例。
    /// </summary>
    public static class MeterFactory
    {
        /// <summary>
        /// 创建电表实例。
        /// </summary>
        public static IEnergyMeters Create(DeviceConfig config)
        {
            switch (config.MeterType)
            {
                case MeterType.Amc:
                    return new AMC_EnergyMeters(config);
                case MeterType.Adl400:
                    return new ADL400_EnergyMeters(config);
                default:
                    return new DJSF1352_EnergyMeters(config);
            }
        }
    }
}
