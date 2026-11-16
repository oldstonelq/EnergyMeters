using System;
using System.Collections.Generic;

namespace ReadDataSoftware
{
    /// <summary>
    /// DJSF1352-RN 导轨式直流电能表。
    /// 读数规则：实际值 = 有效值 x 10^(指数-3)，有效值与指数均为有符号 16 位。
    /// </summary>
    public sealed class Djsf1352EnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 协议默认从站地址。V2.5 起选配双路时第二路地址顺延。
        /// </summary>
        private const byte DefaultSlaveAddress = 1;
        /// <summary>
        /// 电压值 + 小数点指数，占 2 个寄存器。
        /// </summary>
        private const ushort VoltageAddress = 0;
        /// <summary>
        /// 电流值 + 小数点指数，占 2 个寄存器。
        /// </summary>
        private const ushort CurrentAddress = 2;
        /// <summary>
        /// 功率值 + 小数点指数，占 2 个寄存器。
        /// </summary>
        private const ushort PowerAddress = 8;

        /// <summary>
        /// Modbus 主站，串口在 Open 时打开。
        /// </summary>
        private readonly ModbusRtuClient _client;
        /// <summary>
        /// 从站地址，未配置时取本表型的内置默认值。
        /// </summary>
        private readonly byte _slaveAddress;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">设备配置</param>
        public Djsf1352EnergyMeters(DeviceConfig config)
        {
            _client = new ModbusRtuClient(config.PortName, config.BaudRate, config.DataBits, config.StopBits, config.Parity);
            _slaveAddress = (byte)(config.SlaveAddress ?? DefaultSlaveAddress);
        }

        /// <summary>
        /// 通道定义：电压、电流、功率
        /// </summary>
        public IReadOnlyList<ChannelDescriptor> Channels
        {
            get { return MeterChannels.SinglePhase; }
        }

        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool IsOpen
        {
            get { return _client.IsOpen; }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            _client.Open();
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            _client.Close();
        }

        /// <summary>
        /// 采集一次，返回电压、电流、功率三个值
        /// </summary>
        public MeterSample Read()
        {
            double[] values =
            {
                ReadScaledValue(VoltageAddress),
                ReadScaledValue(CurrentAddress),
                ReadScaledValue(PowerAddress)
            };
            return new MeterSample(DateTime.Now, values);
        }

        /// <summary>
        /// 读取「有效值 + 指数」构成的双寄存器项并换算为实际值。
        /// </summary>
        private double ReadScaledValue(ushort address)
        {
            ushort[] registers = _client.ReadHoldingRegisters(_slaveAddress, address, 2);

            // 说明书：电压、电流、功率的有效数据与指数位均为有符号数，读数为 FFFF 表示 -1。
            // 指数位若也是 FFFF，说明该项尚未建立有效数据，直接返回 -1。
            if (registers[0] == 0xFFFF && registers[1] == 0xFFFF)
            {
                return -1;
            }

            short value = (short)registers[0];
            short exponent = (short)registers[1];
            return value * Math.Pow(10, exponent - 3);
        }
    }
}
