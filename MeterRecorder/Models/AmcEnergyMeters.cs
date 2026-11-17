using System;
using System.Collections.Generic;

namespace MeterRecorder
{
    /// <summary>
    /// AMC 系列智能电量采集监控装置。
    /// 读取一次侧数据，读数规则：实际值 = 通讯值 x 10^(小数点位数-4)，
    /// 小数点位数分别由 DPT(电压)、DCT(电流)、DPQ(功率) 决定。
    /// </summary>
    public sealed class AmcEnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 协议默认从站地址。AMC 出厂默认 1。
        /// </summary>
        private const byte DefaultSlaveAddress = 1;
        /// <summary>
        /// 小数点信息：0023H 高字节为 DPT、低字节为 DCT，0024H 高字节为 DPQ。
        /// </summary>
        private const ushort DecimalPointAddress = 0x0023;
        /// <summary>
        /// 一次侧相电压 UA/UB/UC 与线电压 UAB/UBC/UAC，共 6 个寄存器。
        /// </summary>
        private const ushort VoltageAddress = 0x0025;
        /// <summary>
        /// 一次侧电流 IA/IB/IC，共 3 个寄存器。
        /// </summary>
        private const ushort CurrentAddress = 0x002B;
        /// <summary>
        /// 一次侧有功、无功，每相各 1 个寄存器加总值，共 8 个寄存器。
        /// </summary>
        private const ushort PowerAddress = 0x002E;
        /// <summary>
        /// 功率符号位：0024H 低字节，逐位对应 8 个功率项。
        /// </summary>
        private const ushort PowerSignAddress = 0x0024;

        /// <summary>
        /// Modbus 主站，串口在 Open 时打开。
        /// </summary>
        private readonly ModbusRtuClient _client;
        /// <summary>
        /// 从站地址，未配置时取本表型的内置默认值。
        /// </summary>
        private readonly byte _slaveAddress;
        /// <summary>
        /// 采样时间的来源，默认系统时间。测试可以注入固定值。
        /// </summary>
        private readonly Func<DateTime> _clock;
        /// <summary>
        /// 电压小数点位数(DPT)
        /// </summary>
        private int _voltageDecimalPoint;
        /// <summary>
        /// 电流小数点位数(DCT)
        /// </summary>
        private int _currentDecimalPoint;
        /// <summary>
        /// 功率小数点位数(DPQ)
        /// </summary>
        private int _powerDecimalPoint;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">设备配置</param>
        public AmcEnergyMeters(DeviceConfig config)
            : this(config, new SerialPortTransport(config.PortName, config.BaudRate, config.DataBits, config.StopBits, config.Parity))
        {
        }

        /// <summary>
        /// 用指定的传输通道创建实例，供测试注入假通道。
        /// </summary>
        /// <param name="config">设备配置</param>
        /// <param name="transport">报文传输通道</param>
        internal AmcEnergyMeters(DeviceConfig config, IModbusTransport transport, Func<DateTime> clock = null)
        {
            _client = new ModbusRtuClient(transport);
            _slaveAddress = (byte)(config.SlaveAddress ?? DefaultSlaveAddress);
            _clock = clock ?? (() => DateTime.Now);
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
        /// 打开串口，并读取一次小数点位数
        /// </summary>
        public void Open()
        {
            _client.Open();
            LoadDecimalPoints();
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
            ushort[] voltage = _client.ReadHoldingRegisters(_slaveAddress, VoltageAddress, 6);
            ushort[] current = _client.ReadHoldingRegisters(_slaveAddress, CurrentAddress, 3);
            ushort[] power = _client.ReadHoldingRegisters(_slaveAddress, PowerAddress, 8);
            int signBits = ReadPowerSignBits();

            double[] values = new double[17];
            for (int i = 0; i < 6; i++)
            {
                values[i] = Scale(voltage[i], _voltageDecimalPoint);
            }
            for (int i = 0; i < 3; i++)
            {
                values[6 + i] = Scale(current[i], _currentDecimalPoint);
            }
            for (int i = 0; i < 8; i++)
            {
                double value = Scale(power[i], _powerDecimalPoint);
                // 符号位自高到低为 Q、Qc、Qb、Qa、P、Pc、Pb、Pa，与功率项顺序正好反序，
                // 故第 i 项取第 i 位。
                if ((signBits & (1 << i)) != 0)
                {
                    value = -value;
                }
                values[9 + i] = value;
            }

            return new MeterSample(_clock(), values);
        }

        /// <summary>
        /// 读取 DPT / DCT / DPQ 三个小数点位数。
        /// </summary>
        private void LoadDecimalPoints()
        {
            ushort[] registers = _client.ReadHoldingRegisters(_slaveAddress, DecimalPointAddress, 2);
            _voltageDecimalPoint = (registers[0] >> 8) & 0xFF;
            _currentDecimalPoint = registers[0] & 0xFF;
            _powerDecimalPoint = (registers[1] >> 8) & 0xFF;
        }

        /// <summary>
        /// 读取功率符号位，0 为正、1 为负。
        /// </summary>
        private int ReadPowerSignBits()
        {
            ushort[] registers = _client.ReadHoldingRegisters(_slaveAddress, PowerSignAddress, 1);
            return registers[0] & 0xFF;
        }

        /// <summary>
        /// 按说明书换算：实际值 = 通讯值 x 10^(小数点位数-4)。
        /// 通讯值按无符号处理：正负由功率符号位寄存器给出，寄存器里存的是幅值。
        /// 早先这里转成 short，超过 32767 的读数会翻成负数（例如 2 位小数的
        /// 380 V 读成 -273.76），再叠加符号位就正负颠倒了。
        /// </summary>
        private static double Scale(ushort rawValue, int decimalPoint)
        {
            return rawValue * Math.Pow(10, decimalPoint - 4);
        }
    }
}
