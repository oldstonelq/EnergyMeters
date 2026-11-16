using System;

namespace MeterRecorder
{
    /// <summary>
    /// 支持的电能表型号。
    /// </summary>
    public enum MeterType
    {
        /// <summary>
        /// DJSF1352-RN 导轨式直流电能表。
        /// </summary>
        Djsf1352,
        /// <summary>
        /// AMC 系列智能电量采集监控装置。
        /// </summary>
        Amc,
        /// <summary>
        /// ADL400 导轨式多功能电能表。
        /// </summary>
        Adl400
    }

    /// <summary>
    /// 表型枚举与配置文件取值之间的转换。
    /// </summary>
    public static class MeterTypeExtensions
    {
        /// <summary>
        /// 转换为配置文件里使用的取值，如 "DJSF1352"。
        /// </summary>
        public static string ToConfigValue(this MeterType meterType)
        {
            switch (meterType)
            {
                case MeterType.Djsf1352: return "DJSF1352";
                case MeterType.Amc: return "AMC";
                case MeterType.Adl400: return "ADL400";
                default: throw new ArgumentOutOfRangeException(nameof(meterType));
            }
        }

        /// <summary>
        /// 转换为界面显示名称。
        /// </summary>
        public static string ToDisplayName(this MeterType meterType)
        {
            switch (meterType)
            {
                case MeterType.Djsf1352: return "DJSF1352（直流）";
                case MeterType.Amc: return "AMC（交流三相）";
                case MeterType.Adl400: return "ADL400（交流三相）";
                default: throw new ArgumentOutOfRangeException(nameof(meterType));
            }
        }

        /// <summary>
        /// 解析配置文件里的取值，大小写不敏感。
        /// </summary>
        public static bool TryParseConfigValue(string value, out MeterType meterType)
        {
            meterType = MeterType.Djsf1352;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = value.Trim();
            if (string.Equals(normalized, "DJSF1352", StringComparison.OrdinalIgnoreCase))
            {
                meterType = MeterType.Djsf1352;
                return true;
            }
            if (string.Equals(normalized, "AMC", StringComparison.OrdinalIgnoreCase))
            {
                meterType = MeterType.Amc;
                return true;
            }
            if (string.Equals(normalized, "ADL400", StringComparison.OrdinalIgnoreCase))
            {
                meterType = MeterType.Adl400;
                return true;
            }
            return false;
        }
    }
}
