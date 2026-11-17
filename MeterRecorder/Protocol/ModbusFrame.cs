namespace MeterRecorder
{
    /// <summary>
    /// Modbus 请求帧的组装。抽成静态方法，便于在没有串口的情况下做单测。
    /// </summary>
    public static class ModbusFrame
    {
        /// <summary>
        /// 读保持寄存器功能码。
        /// </summary>
        public const byte ReadHoldingRegistersFunctionCode = 0x03;

        /// <summary>
        /// 组装 03H 请求帧：从站地址 + 功能码 + 起始地址 + 寄存器数 + CRC。
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">起始寄存器地址</param>
        /// <param name="quantity">寄存器数量</param>
        public static byte[] BuildReadRequest(byte slaveAddress, ushort startAddress, ushort quantity)
        {
            byte[] request = new byte[8];
            request[0] = slaveAddress;
            request[1] = ReadHoldingRegistersFunctionCode;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(quantity >> 8);
            request[5] = (byte)quantity;
            ModbusCrc.WriteTo(request, 6);
            return request;
        }
    }
}
