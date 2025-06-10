using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadDataSoftware
{
    public  class Enums
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


    }
}
