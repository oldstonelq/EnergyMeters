using ReadData;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using static ReadDataSoftware.Enums;

namespace ReadDataSoftware
{
 
    /// <summary>
    /// 电能表实例
    /// </summary>
    public class DJSF1352_EnergyMeters: EnergyMeters
    {
        /// <summary>
        /// 串口实例
        /// </summary>
        private SerialPort serialPort;
        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称，如"COM1"</param>
        /// <param name="baudRate">波特率，如9600</param>
        public DJSF1352_EnergyMeters(string portName, int baudRate,int dataBits, StopBits stopBits, Parity parity)
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
            return response;
        }
        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private double[] DataProcessing(byte[] data)
        {
            if (data.Length >= 2 && (data[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return new double[] { 0 };
            }
            else if (data.Length < 2)
            {
                return new double[] { 0 };
            }
            else
            {
                //：电压、电流、功率的有效数据与指数位均为有符号数据，若一数读出为“FFFF”，则表示该数据为“-1
                if (data[3] == 0xff && data[4] == 0xff)
                {
                    return new double[] { -1 };
                }
                // 处理正常响应数据
                int byteCount = data[2];
                int registerCount = byteCount / 2;
                List<short> registerValues = new List<short>();
                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((data[3 + i * 2] << 8) | data[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return new double[] { registerValues[0] * Math.Pow(10, registerValues[1] - 3) };
            }
        }
        /// <summary>
        /// 读直流电压值（地址00读两位）
        /// </summary>
        /// <returns></returns>
        public double[] ReadVoltage()
        {
            var res = ReadData(1, 0, 2);
            return DataProcessing(res);
        }
        /// <summary>
        /// 读直流电流值（地址02读两位）
        /// </summary>
        /// <returns></returns>

        public double[] ReadCurrent()
        {
            //查询报文 01 03 00 02 00 02 65 cb
            //应答报文 01 03 04 03 b2 00 00 5a 50
            //处理如下：03 b2(16 进制) = 946(10 进制电流数据) 00 00(16 进制) = 0(10 进制小数点数据）
            //计算：946乘以10的（0- 3）次方 = 0.946；单位：安培（A）
            var res =ReadData(1, 2, 2);
            return DataProcessing(res);
        }
        /// <summary>
        /// 读功率值（地址08读两位）
        /// </summary>
        /// <returns></returns>

        public double[]  ReadPower()
        {
            var res = ReadData(1, 8, 2);
            return DataProcessing(res);
        }
    }
}
