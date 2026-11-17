using System;
using System.Collections.Generic;

namespace MeterRecorder
{
    /// <summary>
    /// ADL400 导轨式多功能电能表。
    /// 读取二次侧数据：电压保留 1 位小数，电流保留 2 位小数，
    /// 功率为 32 位有符号整型、保留 3 位小数，单位 kW/kvar。
    /// </summary>
    public sealed class Adl400EnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 协议默认从站地址。Modbus 地址范围 1~254。
        /// </summary>
        private const byte DefaultSlaveAddress = 177;
        /// <summary>
        /// 二次侧 A/B/C 相电压，共 3 个寄存器，单位 V 保留 1 位小数。
        /// </summary>
        private const ushort PhaseVoltageAddress = 0x0061;
        /// <summary>
        /// 二次侧 A/B/C 相电流，共 3 个寄存器，单位 A 保留 2 位小数。
        /// </summary>
        private const ushort CurrentAddress = 0x0064;
        /// <summary>
        /// 二次侧 A-B、C-B、A-C 线电压，共 3 个寄存器。
        /// </summary>
        private const ushort LineVoltageAddress = 0x0078;
        /// <summary>
        /// 有功、无功功率起始地址。每项 4 字节(2 个寄存器)，8 项共 16 个寄存器。
        /// </summary>
        private const ushort PowerAddress = 0x0164;
        /// <summary>
        /// 功率项数量：A/B/C 相有功、总有功、A/B/C 相无功、总无功。
        /// </summary>
        private const int PowerItemCount = 8;
        /// <summary>
        /// 每个功率项占用的寄存器数量，对应 32 位整型。
        /// </summary>
        private const int RegistersPerPowerItem = 2;

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
        public Adl400EnergyMeters(DeviceConfig config)
        {
            _client = new ModbusRtuClient(config.PortName, config.BaudRate, config.DataBits, config.StopBits, config.Parity);
            _slaveAddress = (byte)(config.SlaveAddress ?? DefaultSlaveAddress);
        }

        /// <summary>
        /// 通道定义：相电压、线电压、电流、有功、无功
        /// </summary>
        public IReadOnlyList<ChannelDescriptor> Channels
        {
            get { return MeterChannels.ThreePhase; }
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
        /// 关闭端口并释放串口资源。
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// 采集一次，按通道顺序返回 17 个值
        /// </summary>
        public MeterSample Read()
        {
            ushort[] phaseVoltage = _client.ReadHoldingRegisters(_slaveAddress, PhaseVoltageAddress, 3);
            ushort[] lineVoltage = _client.ReadHoldingRegisters(_slaveAddress, LineVoltageAddress, 3);
            ushort[] current = _client.ReadHoldingRegisters(_slaveAddress, CurrentAddress, 3);
            ushort quantity = (ushort)(PowerItemCount * RegistersPerPowerItem);
            ushort[] power = _client.ReadHoldingRegisters(_slaveAddress, PowerAddress, quantity);

            double[] values = new double[17];
            for (int i = 0; i < 3; i++)
            {
                values[i] = phaseVoltage[i] / 10.0;
                values[3 + i] = lineVoltage[i] / 10.0;
                values[6 + i] = current[i] / 100.0;
            }
            for (int i = 0; i < PowerItemCount; i++)
            {
                int rawValue = ToInt32(power[i * RegistersPerPowerItem], power[i * RegistersPerPowerItem + 1]);
                values[9 + i] = rawValue / 1000.0;
            }

            return new MeterSample(DateTime.Now, values);
        }

        /// <summary>
        /// 将高字在前的两个寄存器拼接为 32 位有符号整型。
        /// </summary>
        private static int ToInt32(ushort highWord, ushort lowWord)
        {
            return (int)(((uint)highWord << 16) | lowWord);
        }
    }
}
