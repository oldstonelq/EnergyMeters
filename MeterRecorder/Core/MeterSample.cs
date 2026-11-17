using System;
using Newtonsoft.Json;

namespace MeterRecorder
{
    /// <summary>
    /// 一次完整采样的结果：一个时间戳加一组按通道顺序排列的值。
    /// </summary>
    public sealed class MeterSample
    {
        /// <summary>
        /// 创建一次采样结果。
        /// </summary>
        /// <param name="time">采样时间</param>
        /// <param name="values">各通道读数，顺序与通道定义一致</param>
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
        /// <param name="channelIndex">通道下标</param>
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
