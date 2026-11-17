using System;
using System.Collections.Generic;

namespace MeterRecorder
{
    /// <summary>
    /// 各表型的通道定义。表型差异只体现为这份列表的内容，界面与采集服务不再区分型号。
    /// </summary>
    public static class MeterChannels
    {
        /// <summary>
        /// 直流单路表：电压、电流、功率。
        /// </summary>
        public static readonly IReadOnlyList<ChannelDescriptor> SinglePhase = new[]
        {
            new ChannelDescriptor("voltage", "电压",     "V", 3, "电压"),
            new ChannelDescriptor("current", "电流",     "A", 3, "电流"),
            new ChannelDescriptor("power",   "功率",     "W", 3, "功率")
        };

        /// <summary>
        /// 交流三相表：相电压、线电压、电流、有功、无功，共 17 个通道。
        /// 顺序必须与各表型 Read 方法组包时的顺序一致。
        /// </summary>
        public static readonly IReadOnlyList<ChannelDescriptor> ThreePhase = new[]
        {
            new ChannelDescriptor("ua",     "A 相电压", "V",    1, "相电压"),
            new ChannelDescriptor("ub",     "B 相电压", "V",    1, "相电压"),
            new ChannelDescriptor("uc",     "C 相电压", "V",    1, "相电压"),
            new ChannelDescriptor("uab",    "AB 线电压", "V",   1, "线电压"),
            new ChannelDescriptor("ubc",    "BC 线电压", "V",   1, "线电压"),
            new ChannelDescriptor("uac",    "AC 线电压", "V",   1, "线电压"),
            new ChannelDescriptor("ia",     "A 相电流", "A",    3, "电流"),
            new ChannelDescriptor("ib",     "B 相电流", "A",    3, "电流"),
            new ChannelDescriptor("ic",     "C 相电流", "A",    3, "电流"),
            new ChannelDescriptor("pa",     "A 相有功", "kW",   3, "有功功率"),
            new ChannelDescriptor("pb",     "B 相有功", "kW",   3, "有功功率"),
            new ChannelDescriptor("pc",     "C 相有功", "kW",   3, "有功功率"),
            new ChannelDescriptor("pTotal", "总有功",   "kW",   3, "有功功率"),
            new ChannelDescriptor("qa",     "A 相无功", "kvar", 3, "无功功率"),
            new ChannelDescriptor("qb",     "B 相无功", "kvar", 3, "无功功率"),
            new ChannelDescriptor("qc",     "C 相无功", "kvar", 3, "无功功率"),
            new ChannelDescriptor("qTotal", "总无功",   "kvar", 3, "无功功率")
        };

        /// <summary>
        /// 直流表的默认曲线通道：三项都画。
        /// </summary>
        private static readonly string[] s_singlePhaseChartKeys = { "voltage", "current", "power" };

        /// <summary>
        /// 三相表的默认曲线通道：A 相电压、A 相电流与总有功。
        /// </summary>
        private static readonly string[] s_threePhaseChartKeys = { "ua", "ia", "pTotal" };

        /// <summary>
        /// 取指定表型的通道定义。
        /// </summary>
        public static IReadOnlyList<ChannelDescriptor> GetChannels(MeterType meterType)
        {
            return meterType == MeterType.Djsf1352 ? SinglePhase : ThreePhase;
        }

        /// <summary>
        /// 取指定表型的默认曲线通道 Key。界面按 Key 解析下标，
        /// 不再假设「第 7 个是 A 相电流」这类位置关系。
        /// </summary>
        public static IReadOnlyList<string> GetDefaultChartKeys(MeterType meterType)
        {
            return meterType == MeterType.Djsf1352 ? s_singlePhaseChartKeys : s_threePhaseChartKeys;
        }

        /// <summary>
        /// 把通道 Key 解析成通道下标。找不到的 Key 跳过，返回顺序与入参一致。
        /// </summary>
        public static int[] ResolveChannelIndexes(IReadOnlyList<ChannelDescriptor> channels, IReadOnlyList<string> keys)
        {
            List<int> indexes = new List<int>();
            if (channels == null || keys == null)
            {
                return indexes.ToArray();
            }
            for (int k = 0; k < keys.Count; k++)
            {
                for (int i = 0; i < channels.Count; i++)
                {
                    if (string.Equals(channels[i].Key, keys[k], StringComparison.Ordinal))
                    {
                        indexes.Add(i);
                        break;
                    }
                }
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// 取指定表型的默认曲线通道下标。
        /// </summary>
        public static int[] GetDefaultChartIndexes(IReadOnlyList<ChannelDescriptor> channels, MeterType meterType)
        {
            return ResolveChannelIndexes(channels, GetDefaultChartKeys(meterType));
        }
    }
}
