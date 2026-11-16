using System.Collections.Generic;

namespace ReadDataSoftware
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
        /// 取指定表型的通道定义。
        /// </summary>
        public static IReadOnlyList<ChannelDescriptor> GetChannels(MeterType meterType)
        {
            return meterType == MeterType.Djsf1352 ? SinglePhase : ThreePhase;
        }
    }
}
