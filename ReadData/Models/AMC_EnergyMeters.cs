using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace ReadData
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
    public class AMC_EnergyMeters: EnergyMeters
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
        /// 小数点U(DPT)
        /// </summary>
        private double U_DecimalPointTermination;
        /// <summary>
        /// 小数点I(DCT)
        /// </summary>
        private double I_DecimalPointTermination;
        /// <summary>
        /// 小数点PQ(DPQ)
        /// </summary>
        private double PQ_DecimalPointTermination;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称，如"COM1"</param>
        /// <param name="baudRate">波特率，如9600</param>
        public AMC_EnergyMeters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            serialPort.Open();
            GETDecimalPointTermination();
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
            System.Threading.Thread.Sleep(100); // 等待响应
            int bytesToRead = serialPort.BytesToRead;
            byte[] response = new byte[bytesToRead];
            serialPort.Read(response, 0, bytesToRead);
            return response;
        }
        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private static List <short> DataProcessing(byte[] res)
        {
            List<short> registerValues = new List<short>();
            if (res.Length >= 2 && (res[1] & 0x80) != 0)
            {
                // 处理异常响应数据
                return registerValues;
            }
            else if (res.Length < 2)
            {
                //数据长度不足
                return registerValues;
            }
            else
            {
                // 处理正常响应数据
                int byteCount = res[2];
                int registerCount = byteCount / 2;

                for (int i = 0; i < registerCount; i++)
                {
                    short registerValue = (short)((res[3 + i * 2] << 8) | res[4 + i * 2]);
                    registerValues.Add(registerValue);
                }
                return registerValues;
            }
        }
        /// <summary>
        /// 获取小数点位（地址23读2位）
        /// </summary>
        private  void GETDecimalPointTermination()
        {
            var DecimalPointTerminationList = ReadData(1, 23, 2);
            if (DecimalPointTerminationList.Length > 0)
            {
                U_DecimalPointTermination = DecimalPointTerminationList[3];
                I_DecimalPointTermination = DecimalPointTerminationList[4];
                PQ_DecimalPointTermination = DecimalPointTerminationList[5];
            }
        }
        /// <summary>
        /// 读电压（地址25读12位）
        /// </summary>
        /// <returns></returns>
        public double[] ReadVoltage()
        {
            var data = ReadData(1, 37, 6);
            var res= DataProcessing(data);
            double[] voltage = new double[res.Count];
            for (int i = 0; i < res.Count; i ++)
            {
                voltage[i] = res[i];
                voltage[i] = voltage[i] * Math.Pow(10, U_DecimalPointTermination-4);
            }
            return voltage;
        }
        

        /// <summary>
        /// 读电流值（地址2B读3位）
        /// </summary>
        /// <returns></returns>

        public double[] ReadCurrent()
        {
            var data = ReadData(1, 43, 3);
            var res = DataProcessing(data);
            double[] current = new double[res.Count];
            for (int i = 0; i < res.Count; i++)
            {
                current[i] = res[i];
                current[i] = current[i] * Math.Pow(10, U_DecimalPointTermination - 4);
            }
            return current;
        }
        /// <summary>
        /// 读功率值（地址2E读8位）
        /// </summary>
        /// <returns></returns>

        public double[] ReadPower()
        {
            var data = ReadData(1, 46, 8);
            var res = DataProcessing(data);
            double[] power = new double[res.Count];
            for (int i = 0; i < res.Count; i++)
            {
                power[i] = res[i];
                power[i] = power[i] * Math.Pow(10, U_DecimalPointTermination - 4);
            }
            return power;
        }
    }
}
