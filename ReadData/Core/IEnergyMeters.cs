using System.Collections.Generic;

namespace ReadDataSoftware
{
    /// <summary>
    /// 电能表统一读取接口。一个实现对应一款表，读数以「通道 + 值」的形式返回，
    /// 调用方不需要知道具体型号与寄存器布局。
    /// </summary>
    public interface IEnergyMeters
    {
        /// <summary>
        /// 该表型提供的通道定义，顺序与 <see cref="MeterSample.Values"/> 一致。
        /// </summary>
        IReadOnlyList<ChannelDescriptor> Channels { get; }

        /// <summary>
        /// 通讯端口是否已打开。
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 打开通讯端口。AMC 会在此时读取小数位设置。
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭通讯端口。
        /// </summary>
        void Close();

        /// <summary>
        /// 采集一次，返回一个完整样本。
        /// </summary>
        MeterSample Read();
    }
}
