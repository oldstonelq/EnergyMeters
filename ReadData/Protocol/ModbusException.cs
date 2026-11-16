using System;

namespace ReadDataSoftware
{
    /// <summary>
    /// Modbus 通讯异常。涵盖超时、帧校验失败和从站返回的异常应答。
    /// </summary>
    public sealed class ModbusException : Exception
    {
        /// <summary>
        /// 从站返回的异常码；本地帧错误时为 null。
        /// </summary>
        public byte? ExceptionCode { get; }

        /// <summary>
        /// 从站地址；本地帧错误且未解析出地址时为 null。
        /// </summary>
        public byte? SlaveAddress { get; }

        /// <summary>
        /// 用说明文字创建异常。
        /// </summary>
        /// <param name="message">异常说明</param>
        public ModbusException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 用说明文字与内部异常创建异常。
        /// </summary>
        /// <param name="message">异常说明</param>
        /// <param name="innerException">引发本异常的内部异常</param>
        public ModbusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 用从站返回的异常码创建异常。
        /// </summary>
        /// <param name="exceptionCode">Modbus 异常码</param>
        /// <param name="slaveAddress">返回异常的从站地址</param>
        public ModbusException(byte exceptionCode, byte slaveAddress)
            : base(string.Format("从站 {0} 返回异常应答，异常码 0x{1:X2}（{2}）。",
                slaveAddress, exceptionCode, DescribeExceptionCode(exceptionCode)))
        {
            ExceptionCode = exceptionCode;
            SlaveAddress = slaveAddress;
        }

        private static string DescribeExceptionCode(byte exceptionCode)
        {
            switch (exceptionCode)
            {
                case 0x01: return "非法功能码";
                case 0x02: return "非法数据地址";
                case 0x03: return "非法数据值";
                case 0x04: return "从站设备故障";
                case 0x05: return "应答超时，命令正在处理";
                case 0x06: return "从站设备忙";
                default: return "未定义的异常码";
            }
        }
    }
}
