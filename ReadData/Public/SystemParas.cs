using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using ReadData;

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
        /// 数据文件路径
        /// </summary>
        public static string DataFile= Application.StartupPath + @"\Data";
        /// <summary>
        /// 日志文件
        /// </summary>
        public static string logFilePath = Application.StartupPath + @"\Logs\log.txt";
        /// <summary>
        /// 串口配置
        /// </summary>
        public static SerialPortSettings SerialPortSettings = new SerialPortSettings();
        /// <summary>
        /// 电表实例
        /// </summary>
        public static EnergyMeters energyMeters;
        /// <summary>
        /// DJSF1352数据队列
        /// </summary>
        public static List <DJSF1352_DataStructure> Data1 = new List <DJSF1352_DataStructure>();
        /// <summary>
        /// AMC数据队列
        /// </summary>
        public static List<AMC_DataStructure> Data2 = new List<AMC_DataStructure>();
        /// <summary>
        /// 电表类型
        /// </summary>
        public static string EnergyMetersType= "DJSF1352";

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
            //// 检查数据文件夹是否存在
            if (!Directory.Exists(DataFile))
            {
                Directory.CreateDirectory(DataFile);
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
            string energyMetersType = FileHelp.ReadIniKeys("SystemSet", "EnergyMetersType", "", SystemFile);
            if (!string.IsNullOrEmpty(energyMetersType))
            {
                EnergyMetersType = energyMetersType;
            }

            if (EnergyMetersType == "AMC")
            {
                energyMeters = new AMC_EnergyMeters(SerialPortSettings.PortName, SerialPortSettings.BaudRate, SerialPortSettings.DataBits, SerialPortSettings.StopBits, SerialPortSettings.Parity);
            }
            else
            {
                energyMeters = new DJSF1352_EnergyMeters(SerialPortSettings.PortName, SerialPortSettings.BaudRate, SerialPortSettings.DataBits, SerialPortSettings.StopBits, SerialPortSettings.Parity);
            }
        }
    }
}
