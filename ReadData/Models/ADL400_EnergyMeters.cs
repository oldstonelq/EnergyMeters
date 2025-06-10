using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using static ReadDataSoftware.Enums;

namespace ReadDataSoftware
{
    /// <summary>
    /// ADL电表
    /// </summary>
    public  class ADL400_EnergyMeters : EnergyMeters
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
        public ADL400_EnergyMeters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
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
        private byte[] ReadData(byte slaveAddress, ushort startAddress, ushort quantity)
        {
            byte[] request = BuildRequest(slaveAddress, ModbusRtuFunctionCode.ReadHoldingRegisters, startAddress, quantity);
            serialPort.Write(request, 0, request.Length);
            System.Threading.Thread.Sleep(300); // 等待响应
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
                //数据长度不足
                return new double[] { 0 };
            }
            else
            {
                // 处理正常响应数据
                int byteCount = data[2];
                int registerCount = byteCount / 2;
                List<double> registerValues = new List<double>();
                for (int i = 0; i < registerCount; i++)
                {
                    double registerValue = (double)((data[3 + i * 2] << 8) | data[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return registerValues.ToArray ();
            }
        }
        /// <summary>
        /// 读直流电压值（地址97读三位,120读三位 地址120 读三位）
        /// </summary>
        /// <returns></returns>
        public double[] ReadVoltage()
        {
            var res1 = ReadData(177, 97, 3);///相电压
            var res2=ReadData(177, 120, 3);//线电压
            var res3 = ConcatenateArrays(DataProcessing(res1), DataProcessing(res2));
            if (res3.Length == 6)
            {
                for (int i = 0; i < res3.Length; i++)
                {
                    res3[i] = res3[i] / 10;
                }
                return res3;
            }
            else
            { 
              return new double[] { 0,0,0,0,0,0 };
            }
        }
        /// <summary>
        /// 读直流电流值（地址100读三位）
        /// </summary>
        /// <returns></returns>

        public double[] ReadCurrent()
        {
            //读A相电流
            ///查询数据帧01 03 0064 0001 C5 D5
            //返回数据帧01 03 02 03 B2 38 C1
            //处理如下：03 B2（十六进制） = 946（十进制）
            //计算：946 * 0.01 = 9.46 单位：A
            var res = ReadData(177, 100, 3);
            var res1 = DataProcessing(res);
            if (res1.Length == 3)
            {
                for (int i = 0; i < res1.Length; i++)
                {
                    res1[i] = res1[i] / 100;
                }
                return res1;
            }
            else
            {
                return new double[] { 0,0,0 };
            }
        }
        /// <summary>
        /// 读功率值（地址356读八位）
        /// </summary>
        /// <returns></returns>

        public double[] ReadPower()
        {
            var res = ReadData(177, 356, 8);
            var res1 = DataProcessing(res);
            if (res1.Length == 8)
            {
                for (int i = 0; i < res1.Length; i++)
                {
                    res1[i] = res1[i] / 1000;
                }
                return res1;
            }
            else
            {
                return new double[] { 0,0,0,0,0,0,0,0 };
            }
        }
        /// <summary>
        ///拼接两个double数组
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static double[] ConcatenateArrays(double[] array1, double[] array2)
        {
            double[] result = new double[array1.Length + array2.Length];
            Array.Copy(array1, 0, result, 0, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);
            return result;
        }
    }
}
