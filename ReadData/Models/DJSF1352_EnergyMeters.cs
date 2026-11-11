using System;
using System.IO.Ports;

namespace ReadDataSoftware
{
    /// <summary>
    /// DJSF1352-RN 导轨式直流电能表。
    /// 读数规则：实际值 = 有效值 x 10^(指数-3)，有效值与指数均为有符号 16 位。
    /// </summary>
    public class DJSF1352_EnergyMeters : IEnergyMeters
    {
        /// <summary>
        /// 从站地址。V2.5 起选配双路时第二路地址顺延，届时需改为可配置。
        /// </summary>
        private const byte SlaveAddress = 1;
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

        private readonly ModbusRtuClient _client;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称，如 "COM1"</param>
        /// <param name="baudRate">波特率，如 9600</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        public DJSF1352_EnergyMeters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
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
        /// 读直流电压，返回单元素数组，单位 V
        /// </summary>
        public double[] ReadVoltage()
        {
            return new[] { ReadScaledValue(VoltageAddress) };
        }

        /// <summary>
        /// 读直流电流，返回单元素数组，单位 A
        /// </summary>
        public double[] ReadCurrent()
        {
            return new[] { ReadScaledValue(CurrentAddress) };
        }

        /// <summary>
        /// 读直流功率，返回单元素数组，单位 W
        /// </summary>
        public double[] ReadPower()
        {
            return new[] { ReadScaledValue(PowerAddress) };
        }

        /// <summary>
        /// 读取「有效值 + 指数」构成的双寄存器项并换算为实际值。
        /// </summary>
        private double ReadScaledValue(ushort address)
        {
            ushort[] registers = _client.ReadHoldingRegisters(SlaveAddress, address, 2);

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
