using System;

namespace MeterRecorder
{
    /// <summary>
    /// 表型枚举与配置文件取值之间的转换。
    /// </summary>
    public static class MeterTypeExtensions
    {
        /// <summary>
        /// 转换为配置文件里使用的取值，如 "DJSF1352"。
        /// </summary>
        /// <param name="meterType">表型</param>
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
        /// <param name="meterType">表型</param>
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
        /// <param name="value">配置文件里的取值，大小写与前后空格不敏感</param>
        /// <param name="meterType">解析结果；失败时为默认表型</param>
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
