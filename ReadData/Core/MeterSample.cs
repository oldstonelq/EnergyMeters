using System;
using Newtonsoft.Json;

namespace ReadDataSoftware
{
    /// <summary>
    /// 一次完整采样的结果：一个时间戳加一组按通道顺序排列的值。
    /// </summary>
    public sealed class MeterSample
    {
        [JsonConstructor]
        public MeterSample(DateTime time, double[] values)
        {
            Time = time;
            Values = values ?? new double[0];
        }

        /// <summary>
        /// 采样时间。
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// 各通道的读数，顺序与对应的通道描述一致。
        /// </summary>
        public double[] Values { get; }

        /// <summary>
        /// 按通道下标取值；下标越界时返回 0，避免界面因数据不全而抛异常。
        /// </summary>
        public double GetValue(int channelIndex)
        {
            if (Values == null || channelIndex < 0 || channelIndex >= Values.Length)
            {
                return 0;
            }
            return Values[channelIndex];
        }
    }
}
