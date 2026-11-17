using System;
using System.Collections.Generic;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 通道定义：三款表的通道数量、键唯一性，以及默认曲线通道的解析。
    /// 第 16 项把「默认画哪几条线」从界面挪到了这里，改坏不会有编译错误，只能靠测试发现。
    /// </summary>
    public class MeterChannelsTests
    {
        /// <summary>
        /// 直流表只有电压、电流、功率三个通道，顺序与 Read 组包一致。
        /// </summary>
        [Fact]
        public void GetChannels_DcMeter_ReturnsThreeChannels()
        {
            IReadOnlyList<ChannelDescriptor> channels = MeterChannels.GetChannels(MeterType.Djsf1352);

            Assert.Equal(3, channels.Count);
            Assert.Equal(new[] { "voltage", "current", "power" }, Keys(channels));
        }

        /// <summary>
        /// AMC 与 ADL400 共用三相通道定义，共 17 项；两者必须是同一份列表，
        /// 否则同一份数据文件在两种表型下的列含义会不一致。
        /// </summary>
        [Fact]
        public void GetChannels_ThreePhaseMeters_ShareSeventeenChannels()
        {
            IReadOnlyList<ChannelDescriptor> amc = MeterChannels.GetChannels(MeterType.Amc);
            IReadOnlyList<ChannelDescriptor> adl400 = MeterChannels.GetChannels(MeterType.Adl400);

            Assert.Equal(17, amc.Count);
            Assert.Same(MeterChannels.ThreePhase, amc);
            Assert.Same(MeterChannels.ThreePhase, adl400);
        }

        /// <summary>
        /// 通道键必须唯一：数据文件按 Key 记录通道，重复键会让回放时找不到对应列。
        /// </summary>
        [Fact]
        public void ChannelKeys_AreUnique()
        {
            AssertUniqueKeys(MeterChannels.SinglePhase);
            AssertUniqueKeys(MeterChannels.ThreePhase);
        }

        /// <summary>
        /// 默认曲线通道解析成下标后应与通道顺序对得上：直流是前三项，
        /// 三相是 A 相电压、A 相电流与总有功。
        /// </summary>
        [Fact]
        public void GetDefaultChartIndexes_MatchesChannelOrder()
        {
            Assert.Equal(new[] { 0, 1, 2 }, MeterChannels.GetDefaultChartIndexes(
                MeterChannels.GetChannels(MeterType.Djsf1352), MeterType.Djsf1352));
            Assert.Equal(new[] { 0, 6, 12 }, MeterChannels.GetDefaultChartIndexes(
                MeterChannels.GetChannels(MeterType.Adl400), MeterType.Adl400));
            Assert.Equal(new[] { 0, 6, 12 }, MeterChannels.GetDefaultChartIndexes(
                MeterChannels.GetChannels(MeterType.Amc), MeterType.Amc));
        }

        /// <summary>
        /// 默认曲线的每个 Key 都必须能在对应通道定义里找到，
        /// 防止改名时只改了一边（那种情况界面上会静默少一条线）。
        /// </summary>
        [Theory]
        [InlineData(MeterType.Djsf1352)]
        [InlineData(MeterType.Amc)]
        [InlineData(MeterType.Adl400)]
        public void DefaultChartKeys_ExistInChannelDefinitions(MeterType meterType)
        {
            IReadOnlyList<ChannelDescriptor> channels = MeterChannels.GetChannels(meterType);
            IReadOnlyList<string> keys = MeterChannels.GetDefaultChartKeys(meterType);

            Assert.NotEmpty(keys);
            foreach (string key in keys)
            {
                Assert.Contains(key, Keys(channels));
            }
        }

        /// <summary>
        /// 解析时缺失的 Key 直接跳过，已找到的保持传入顺序，
        /// 这样回放旧文件（通道被裁过）时不会整体错位。
        /// </summary>
        [Fact]
        public void ResolveChannelIndexes_SkipsUnknownKeys_AndKeepsOrder()
        {
            IReadOnlyList<ChannelDescriptor> channels = MeterChannels.GetChannels(MeterType.Adl400);

            int[] indexes = MeterChannels.ResolveChannelIndexes(
                channels, new[] { "pTotal", "noSuchKey", "ua" });

            Assert.Equal(new[] { 12, 0 }, indexes);
        }

        /// <summary>
        /// 通道或 Key 为 null 时返回空数组而不是抛异常，界面上少画几条线即可。
        /// </summary>
        [Fact]
        public void ResolveChannelIndexes_NullArguments_ReturnEmpty()
        {
            Assert.Empty(MeterChannels.ResolveChannelIndexes(null, new[] { "ua" }));
            Assert.Empty(MeterChannels.ResolveChannelIndexes(
                MeterChannels.GetChannels(MeterType.Adl400), null));
        }

        private static void AssertUniqueKeys(IReadOnlyList<ChannelDescriptor> channels)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (ChannelDescriptor channel in channels)
            {
                Assert.False(string.IsNullOrWhiteSpace(channel.Key));
                Assert.True(seen.Add(channel.Key), "通道键重复：" + channel.Key);
            }
        }

        private static string[] Keys(IReadOnlyList<ChannelDescriptor> channels)
        {
            string[] keys = new string[channels.Count];
            for (int i = 0; i < channels.Count; i++)
            {
                keys[i] = channels[i].Key;
            }
            return keys;
        }
    }
}
