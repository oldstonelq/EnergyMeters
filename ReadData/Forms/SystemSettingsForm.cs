using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    public partial class SystemSettingsForm : Form
    {
        public SystemSettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComboBoxes()
        {
            //获取当前计算机上所有串口
            //string[] ports = SerialPort.GetPortNames();
            string[] ports = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10" };
            cmb_PortName.Items.AddRange(ports);
            cmb_PortName.Text = SystemParameters.Device.PortName;

            //波特率
            string[] baudRates = { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "57600", "115200" };
            cmb_BaudRate.Items.AddRange(baudRates);
            cmb_BaudRate.Text = SystemParameters.Device.BaudRate.ToString();

            //数据位
            string[] dataBits = { "5", "6", "7", "8" };
            cmb_DataBits.Items.AddRange(dataBits);
            cmb_DataBits.Text = SystemParameters.Device.DataBits.ToString();

            //停止位
            string[] stopBits = { "None", "One", "Two", "OnePointFive" };
            cmb_StopBits.Items.AddRange(stopBits);
            cmb_StopBits.Text = SystemParameters.Device.StopBits.ToString();

            //奇偶位
            string[] parity = { "None", "Odd", "Even", "Mark", "Space" };
            cmb_Parity.Items.AddRange(parity);
            cmb_Parity.Text = SystemParameters.Device.Parity.ToString();

            //电表型号
            string[] meterType = { "DJSF1352", "AMC", "ADL400" };
            cmb_EnergyMetersType.Items.AddRange(meterType);
            cmb_EnergyMetersType.Text = SystemParameters.Device.MeterType.ToConfigValue();
        }

        private void InitializeTextBoxes()
        {
            txt_DataFile.Text = SystemParameters.Paths.DataDirectory;
            txt_LogFile.Text = SystemParameters.Paths.LogFile;
        }

        private void SystemSettingsForm_Load(object sender, EventArgs e)
        {
            InitializeComboBoxes();
            InitializeTextBoxes();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            // 在现有配置的副本上改，保存失败时内存里的配置不受影响。
            DeviceConfig config = SystemParameters.Device.Clone();
            config.PortName = cmb_PortName.Text.Trim();

            int baudRate;
            if (int.TryParse(cmb_BaudRate.Text, out baudRate))
            {
                config.BaudRate = baudRate;
            }
            int dataBits;
            if (int.TryParse(cmb_DataBits.Text, out dataBits))
            {
                config.DataBits = dataBits;
            }
            StopBits stopBits;
            if (Enum.TryParse(cmb_StopBits.Text, out stopBits))
            {
                config.StopBits = stopBits;
            }
            Parity parity;
            if (Enum.TryParse(cmb_Parity.Text, out parity))
            {
                config.Parity = parity;
            }
            MeterType meterType;
            if (MeterTypeExtensions.TryParseConfigValue(cmb_EnergyMetersType.Text, out meterType))
            {
                config.MeterType = meterType;
            }

            try
            {
                config.Save(SystemParameters.Paths.SystemFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("保存成功，请重启软件生效");
        }
    }
}
