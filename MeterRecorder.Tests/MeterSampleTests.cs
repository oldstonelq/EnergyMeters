using System;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 样本与通道描述这两个值对象。界面每帧都会调 GetValue 与 Format，
    /// 数据不全时不能抛异常，否则一帧坏数据就会把界面打挂。
    /// </summary>
    public class MeterSampleTests
    {
        /// <summary>
        /// 下标在范围内时原样返回值。
        /// </summary>
        [Fact]
        public void GetValue_InRange_ReturnsValue()
        {
            MeterSample sample = new MeterSample(new DateTime(2026, 11, 17, 9, 0, 0), new[] { 1.5, 2.5 });

            Assert.Equal(1.5, sample.GetValue(0), 3);
            Assert.Equal(2.5, sample.GetValue(1), 3);
        }

        /// <summary>
        /// 下标越界或为负时返回 0：旧文件通道数少于当前表型时会出现这种情况。
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(99)]
        public void GetValue_OutOfRange_ReturnsZero(int index)
        {
            MeterSample sample = new MeterSample(DateTime.Now, new[] { 1.0, 2.0, 3.0 });

            Assert.Equal(0, sample.GetValue(index), 3);
        }

        /// <summary>
        /// 传入 null 时退化成空数组，而不是保留 null 让后续调用抛空引用。
        /// </summary>
        [Fact]
        public void Constructor_NullValues_UsesEmptyArray()
        {
            MeterSample sample = new MeterSample(DateTime.Now, null);

            Assert.NotNull(sample.Values);
            Assert.Empty(sample.Values);
            Assert.Equal(0, sample.GetValue(0), 3);
        }
    }

    /// <summary>
    /// 通道描述的显示格式：卡片与表头都依赖它，单位缺失时不能留下多余的括号。
    /// </summary>
    public class ChannelDescriptorTests
    {
        /// <summary>
        /// 按 Decimals 指定的位数格式化，不足补零。
        /// </summary>
        [Theory]
        [InlineData(1, 220.44, "220.4")]
        [InlineData(2, 9.461, "9.46")]
        [InlineData(3, 3.7, "3.700")]
        public void Format_UsesDecimals(int decimals, double value, string expected)
        {
            ChannelDescriptor channel = new ChannelDescriptor("k", "名称", "V", decimals, "分组");

            Assert.Equal(expected, channel.Format(value));
        }

        /// <summary>
        /// 有单位时拼成「名称 (单位)」，没有单位时只显示名称。
        /// </summary>
        [Fact]
        public void DisplayName_AppendsUnitWhenPresent()
        {
            Assert.Equal("A 相电压 (V)", new ChannelDescriptor("ua", "A 相电压", "V", 1, "相电压").DisplayName);
            Assert.Equal("A 相电压", new ChannelDescriptor("ua", "A 相电压", string.Empty, 1, "相电压").DisplayName);
        }
    }
}
