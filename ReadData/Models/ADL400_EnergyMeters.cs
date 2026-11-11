using System;
using System.IO.Ports;

namespace ReadDataSoftware
{
    /// <summary>
    /// ADL400 导轨式多功能电能表。
    /// 读取二次侧数据：电压保留 1 位小数，电流保留 2 位小数，
    /// 功率为 32 位有符号整型、保留 3 位小数，单位 kW/kvar。
    /// </summary>
    public class ADL400_EnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 从站地址。Modbus 地址范围 1~254。
        /// </summary>
        private const byte SlaveAddress = 177;
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
        private const ushort PowerItemCount = 8;
        /// <summary>
        /// 每个功率项占用的寄存器数量，对应 32 位整型。
        /// </summary>
        private const int RegistersPerPowerItem = 2;

        private readonly ModbusRtuClient _client;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public ADL400_EnergyMeters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
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
        /// 读电压，依次为相电压 UA、UB、UC 与线电压 UAB、UBC、UAC，单位 V
        /// </summary>
        public double[] ReadVoltage()
        {
            ushort[] phaseVoltage = _client.ReadHoldingRegisters(SlaveAddress, PhaseVoltageAddress, 3);
            ushort[] lineVoltage = _client.ReadHoldingRegisters(SlaveAddress, LineVoltageAddress, 3);

            double[] voltage = new double[phaseVoltage.Length + lineVoltage.Length];
            for (int i = 0; i < phaseVoltage.Length; i++)
            {
                voltage[i] = phaseVoltage[i] / 10.0;
            }
            for (int i = 0; i < lineVoltage.Length; i++)
            {
                voltage[phaseVoltage.Length + i] = lineVoltage[i] / 10.0;
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
                current[i] = registers[i] / 100.0;
            }
            return current;
        }

        /// <summary>
        /// 读功率，依次为 A/B/C 相有功、总有功、A/B/C 相无功、总无功，单位 kW / kvar
        /// </summary>
        public double[] ReadPower()
        {
            ushort quantity = (ushort)(PowerItemCount * RegistersPerPowerItem);
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, PowerAddress, quantity);

            double[] power = new double[PowerItemCount];
            for (int i = 0; i < PowerItemCount; i++)
            {
                int rawValue = ToInt32(registers[i * RegistersPerPowerItem], registers[i * RegistersPerPowerItem + 1]);
                power[i] = rawValue / 1000.0;
            }
            return power;
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
