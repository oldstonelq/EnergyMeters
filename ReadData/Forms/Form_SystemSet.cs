using ReadDataSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ReadData
{
    public partial class Form_SystemSet : Form
    {
        public Form_SystemSet()
        {
            InitializeComponent();
        }

        private void  InitComBox()
        {
            //获取当前计算机上所有串口
            //string[] ports = SerialPort.GetPortNames();
            string[] ports = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10" };
            cmb_PortName.Items.AddRange(ports);
            cmb_PortName.Text = SystemParas.SerialPortSettings.PortName;

            //波特率
            string[] baudRates = { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "57600", "115200" };
            cmb_BaudRate.Items.AddRange(baudRates);
            cmb_BaudRate.Text = SystemParas.SerialPortSettings.BaudRate.ToString();

            //数据位
            string[] dataBits = { "5", "6", "7", "8" };
            cmb_DataBits.Items.AddRange(dataBits);
            cmb_DataBits.Text = SystemParas.SerialPortSettings.DataBits.ToString();

            //停止位
            string[] stopBits = { "None", "One" , "Two", "OnePointFive" };
            cmb_StopBits.Items.AddRange(stopBits);
            cmb_StopBits.Text = SystemParas.SerialPortSettings.StopBits.ToString();


            //奇偶位
            string[] Parity = { "None", "One", "OnePointFive", "Two" };
            cmb_Parity.Items.AddRange(Parity);
            cmb_Parity.Text = SystemParas.SerialPortSettings.Parity.ToString();
        }

        private void InitTextBox()
        {
            txt_DataFile.Text = SystemParas.DataFile;
            txt_LogFile.Text = SystemParas.logFilePath;
        }

        private void SystemSet_Load(object sender, EventArgs e)
        {
            InitComBox();
            InitTextBox();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            FileHelp.WriteIniKeys("SystemSet", "PortName",cmb_PortName.Text, SystemParas.SystemFile);
            FileHelp.WriteIniKeys("SystemSet", "BaudRate", cmb_BaudRate.Text, SystemParas.SystemFile);
            FileHelp.WriteIniKeys("SystemSet", "DataBits", cmb_DataBits.Text, SystemParas.SystemFile);
            FileHelp.WriteIniKeys("SystemSet", "StopBits", cmb_StopBits.Text, SystemParas.SystemFile);
            FileHelp.WriteIniKeys("SystemSet", "Parity", cmb_Parity.Text, SystemParas.SystemFile);
            MessageBox.Show("保存成功");
        }
    }
}
