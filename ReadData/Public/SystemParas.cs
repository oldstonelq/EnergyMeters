using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace ReadDataSoftware
{
    /// <summary>
    /// 系统参数
    /// </summary>
    public  class SystemParas
    {
        /// <summary>
        /// 系统路径
        /// </summary>
        public static string SystemPath = Application.StartupPath + @"\System";
        /// <summary>
        /// 系统配置文件
        /// </summary>
        public static string SystemFile = SystemPath + @"\system.ini";
        /// <summary>
        /// 串口配置
        /// </summary>
        public static SerialPortSettings SerialPortSettings = new SerialPortSettings();
        /// <summary>
        /// 电表实例
        /// </summary>
        public static EnergyMeters energyMeters;

        /// <summary>
        /// 初始化系统参数
        /// </summary>
        public static void Load()
        {
            //// 检查系统文件夹是否存在
            if (!Directory.Exists(SystemPath))
            {
                Directory.CreateDirectory(SystemPath);
            }
            //// 检查系统文件是否存在
            if (!File.Exists(SystemFile))
            {
                File.Create(SystemFile);
            }
           //串口配置
            string portName = FileHelp.ReadIniKeys("SystemSet", "PortName", "", SystemFile);
            if (!string.IsNullOrEmpty(portName))
            {
                SerialPortSettings.PortName = portName;
            }
            //波特率设置
            string baudRate = FileHelp.ReadIniKeys("SystemSet", "BaudRate", "", SystemFile);
            if (!string.IsNullOrEmpty(baudRate))
            {
                SerialPortSettings.BaudRate = int.Parse(baudRate);
            }
            //数据位设置
            string dataBits = FileHelp.ReadIniKeys("SystemSet", "DataBits", "", SystemFile);
            if (!string.IsNullOrEmpty(dataBits))
            {
                SerialPortSettings.DataBits = int.Parse(dataBits);
            }
            //停止位设置
            string stopBits = FileHelp.ReadIniKeys("SystemSet", "StopBits", "", SystemFile);
            if (!string.IsNullOrEmpty(stopBits))
            {
                SerialPortSettings.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            }
            //校验位设置
            string parity = FileHelp.ReadIniKeys("SystemSet", "Parity", "", SystemFile);
            if (!string.IsNullOrEmpty(parity))
            {
                SerialPortSettings.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            }

            energyMeters = new EnergyMeters(SerialPortSettings.PortName,SerialPortSettings.BaudRate,SerialPortSettings.DataBits,SerialPortSettings .StopBits,SerialPortSettings .Parity);
        }

    }
}
