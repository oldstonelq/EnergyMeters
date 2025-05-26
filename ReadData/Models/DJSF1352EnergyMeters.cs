using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    /// <summary>
    /// ModbusRTU功能码枚举
    /// </summary>
    public enum ModbusRtuFunctionCode : byte
    {
        /// <summary>
        /// 读取线圈状态
        /// </summary>
        ReadCoils = 0x01,
        /// <summary>
        /// 读取离散输入状态
        /// </summary>
        ReadDiscreteInputs = 0x02,
        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        ReadHoldingRegisters = 0x03,
        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        ReadInputRegisters = 0x04,
        /// <summary>
        /// 写单个线圈
        /// </summary>
        WriteSingleCoil = 0x05,
        /// <summary>
        /// 写单个寄存器
        /// </summary>
        WriteSingleRegister = 0x06,
        /// <summary>
        /// 写多个线圈
        /// </summary>
        WriteMultipleCoils = 0x0F,
        /// <summary>
        /// 写多个寄存器
        /// </summary>
        WriteMultipleRegisters = 0x10
    }
    /// <summary>
    /// 电能表实例
    /// </summary>
    public class DJSF1352EnergyMeters
    {
        /// <summary>
        /// 串口实例
        /// </summary>
        private SerialPort serialPort;
        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool isopen
        {
            get { return serialPort.IsOpen; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称，如"COM1"</param>
        /// <param name="baudRate">波特率，如9600</param>
        public DJSF1352EnergyMeters(string portName, int baudRate,int dataBits, StopBits stopBits, Parity parity)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            serialPort.Open();
            
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        { 
            serialPort.Close(); 
        }
        /// <summary>
        /// 计算CRC校验码
        /// </summary>
        /// <param name="data">要计算的数据数组</param>
        /// <param name="length">数据的长度</param>
        /// <returns></returns>
        private ushort CalculateCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }
        /// <summary>
        /// 构建读取类请求帧
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="ModbusRtuFunctionCode">功能码</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="quantity">数量</param>
        /// <returns></returns>
        private byte[] BuildRequest(byte slaveAddress, ModbusRtuFunctionCode ModbusRtuFunctionCode, ushort startAddress, ushort quantity)
        {
            //从站地址（1个字节）+功能码（1个字节）+起始地址高位（1个字节）+起始地址低位(1个字节) +数量高位（1个字节）+数量低位（1个字节） +CRC校验码低位（1个字节）+CRC校验码高位（1个字节）
            byte[] request = new byte[8];
            request[0] = slaveAddress;
            request[1] = (byte)ModbusRtuFunctionCode;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(quantity >> 8);
            request[5] = (byte)quantity;
            ushort crc = CalculateCRC(request, 6);
            request[6] = (byte)crc;
            request[7] = (byte)(crc >> 8);
            return request;
        }
        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private  byte[] ReadData(byte slaveAddress, ushort startAddress, ushort quantity)
        {
            byte[] request = BuildRequest(slaveAddress, ModbusRtuFunctionCode.ReadHoldingRegisters, startAddress, quantity);
            serialPort.Write(request, 0, request.Length);
            System.Threading.Thread.Sleep(100); // 等待响应
            int bytesToRead = serialPort.BytesToRead;
            byte[] response = new byte[bytesToRead];
            serialPort.Read(response, 0, bytesToRead);
            if (response.Length >= 2 && (response[1] & 0x80) != 0)
            {
                // 处理异常响应数据
            }
            else
            {
                // 处理正常响应数据
                //int byteCount = response[2];
                //int registerCount = byteCount / 2;
                //List<ushort> registerValues = new List<ushort>();
                //for (int i = 0; i < registerCount; i++)
                //{
                //    ushort registerValue = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
                //    registerValues.Add(registerValue);
                //}

            }
            return response;
        }
        /// <summary>
        /// 读直流电压值（地址00读两位）
        /// </summary>
        /// <returns></returns>
        public double ReadDirectCurrentVoltage()
        {
            var res = ReadData(1, 0, 2);
            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return 0;
            }
            else if (res.Length < 2)
            {
                return 0;
            }
            else
            {
                //：电压、电流、功率的有效数据与指数位均为有符号数据，若一数读出为“FFFF”，则表示该数据为“-1
                if (res[3] == 0xff && res[4] == 0xff)
                {
                    return -1;
                }
                // 处理正常响应数据
                int byteCount = res[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return registerValues[0] * Math.Pow(10, registerValues[1] - 3);
            }
        }
        /// <summary>
        /// 读直流电流值（地址02读两位）
        /// </summary>
        /// <returns></returns>

        public double ReadDirectCurrent()
        {
            var res=ReadData(1, 2, 2);

            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return 0;
            }
            else if (res.Length < 2)
            {
                return 0;
            }
            else
            {
                // 处理正常响应数据
                //：电压、电流、功率的有效数据与指数位均为有符号数据，若一数读出为“FFFF”，则表示该数据为“-1
                if (res[3] == 0xff && res[4] == 0xff)
                {
                    return -1;
                }
                int byteCount = res[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return registerValues[0] * Math.Pow(10, registerValues[1] - 3);
            }
            
        }
        /// <summary>
        /// 读功率值（地址08读两位）
        /// </summary>
        /// <returns></returns>

        public double  ReadPower()
        {
            var res = ReadData(1, 8, 2);

            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return 0;
            }
            else if (res.Length < 2)
            {
                return 0;
            }
            else
            {
                // 处理正常响应数据
                //电压、电流、功率的有效数据与指数位均为有符号数据，若一数读出为“FFFF”，则表示该数据为-1
                if (res[3] == 0xff&& res[4] == 0xff)
                {
                    return -1;
                }
                int byteCount = res[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return registerValues[0] * Math.Pow(10, registerValues[1] - 3);
            }
        }

        /// <summary>
        /// 读取报警状态（地址19读1位）
        /// </summary>
        /// <returns></returns>
        public bool[] ReadAlarmStateToBoolArray()
        {
            var res = ReadData(1, 19, 1);
            bool[] alarm = new bool[8];
            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return alarm;
            }
            else if (res.Length < 2)
            {
                return alarm;
            }
            else
            {
                // 处理正常响应数据
                int byteCount = res[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                for (int i = 0; i < 8; i++)
                {
                    alarm[7 - i] = (registerValues[0] & (1 << i)) != 0; // 使用位运算提取每一位
                }
                return alarm;
            }
        }

        /// <summary>
        /// 读取报警状态（地址19读1位）
        /// </summary>
        /// <returns></returns>
        public string  ReadAlarmStateToString()
        {
            var res = ReadData(1, 19, 1);
            bool[] alarm = new bool[8];
            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return "异常响应";
            }
            else if (res.Length < 2)
            {
                return "数据长度不足";
            }
            else
            {
                // 处理正常响应数据
                int byteCount = res[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                string binaryString = Convert.ToString(registerValues[0], 2).PadLeft(8, '0'); // 将 byte 转为二进制字符串，左边填充 0
                return binaryString;
            }
        }
    }
}
