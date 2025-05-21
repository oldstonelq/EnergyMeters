using System.IO.Ports;

namespace ReadDataSoftware
{
    public class SerialPortSettings
    {
        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }
        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits { get; set; }
        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }
        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { get; set; }

        public SerialPortSettings()
        {
            PortName = "COM1";
            BaudRate = 9600;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;
        }
    }
}

