using System;
using System.IO.Ports;

namespace ReadDataSoftware
{
    /// <summary>
    /// AMC 系列智能电量采集监控装置。
    /// 读取一次侧数据，读数规则：实际值 = 通讯值 x 10^(小数点位数-4)，
    /// 小数点位数分别由 DPT(电压)、DCT(电流)、DPQ(功率) 决定。
    /// </summary>
    public class AMC_EnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 从站地址。AMC 出厂默认 1，如需多表并联应改为可配置。
        /// </summary>
        private const byte SlaveAddress = 1;
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
        /// 功率起始地址。每项 1 个寄存器，有功 4 项 + 无功 4 项共 8 个。
        /// </summary>
        private const ushort PowerAddress = 0x002E;
        /// <summary>
        /// 功率符号位：0024H 低字节，逐位对应 8 个功率项。
        /// </summary>
        private const ushort PowerSignAddress = 0x0024;

        private readonly ModbusRtuClient _client;
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
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public AMC_EnergyMeters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            _client = new ModbusRtuClient(portName, baudRate, dataBits, stopBits, parity);
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
        /// 读电压，依次为 UA、UB、UC、UAB、UBC、UAC，单位 V
        /// </summary>
        public double[] ReadVoltage()
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, VoltageAddress, 6);
            double[] voltage = new double[registers.Length];
            for (int i = 0; i < registers.Length; i++)
            {
                voltage[i] = Scale(registers[i], _voltageDecimalPoint);
            }
            return voltage;
        }

        /// <summary>
        /// 读电流，依次为 IA、IB、IC，单位 A
        /// </summary>
        public double[] ReadCurrent()
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, CurrentAddress, 3);
            double[] current = new double[registers.Length];
            for (int i = 0; i < registers.Length; i++)
            {
                current[i] = Scale(registers[i], _currentDecimalPoint);
            }
            return current;
        }

        /// <summary>
        /// 读功率，依次为 A/B/C 相有功、总有功、A/B/C 相无功、总无功
        /// </summary>
        public double[] ReadPower()
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, PowerAddress, 8);
            int signBits = ReadPowerSignBits();
            double[] power = new double[registers.Length];
            for (int i = 0; i < registers.Length; i++)
            {
                power[i] = Scale(registers[i], _powerDecimalPoint);
                // 符号位自高到低为 Q、Qc、Qb、Qa、P、Pc、Pb、Pa，
                // 与本数组下标 (Pa、Pb、Pc、P、Qa、Qb、Qc、Q) 正好反序对应，故第 i 项取第 i 位。
                if ((signBits & (1 << i)) != 0)
                {
                    power[i] = -power[i];
                }
            }
            return power;
        }

        /// <summary>
        /// 读取功率符号位，0 为正、1 为负。
        /// </summary>
        private int ReadPowerSignBits()
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, PowerSignAddress, 1);
            return registers[0] & 0xFF;
        }

        /// <summary>
        /// 读取 DPT / DCT / DPQ 三个小数点位数。
        /// </summary>
        private void LoadDecimalPoints()
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, DecimalPointAddress, 2);
            _voltageDecimalPoint = (registers[0] >> 8) & 0xFF;
            _currentDecimalPoint = registers[0] & 0xFF;
            _powerDecimalPoint = (registers[1] >> 8) & 0xFF;
        }

        /// <summary>
        /// 按说明书换算：实际值 = 通讯值 x 10^(小数点位数-4)，通讯值为有符号 16 位。
        /// </summary>
        private static double Scale(ushort rawValue, int decimalPoint)
        {
            return (short)rawValue * Math.Pow(10, decimalPoint - 4);
        }
    }
}
