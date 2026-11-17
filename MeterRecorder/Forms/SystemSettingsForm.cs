using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 系统设置窗体：串口参数与电表型号。
    /// </summary>
    public partial class SystemSettingsForm : Form
    {
        /// <summary>
        /// 初始化系统设置窗体。
        /// </summary>
        public SystemSettingsForm()
        {
            InitializeComponent();
            this.Icon = FormIcon.Load();
        }

        /// <summary>
        /// 用当前配置填充各下拉框。
        /// </summary>
        private void InitializeComboBoxes()
        {
            // 端口列表固定给出 COM1~COM10，避免枚举到虚拟串口
            string[] ports = { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10" };
            _portNameCombo.Items.AddRange(ports);
            _portNameCombo.Text = SystemParameters.Device.PortName;

            // 波特率：取值与 DeviceConfig 的范围校验一致（300~115200）
            string[] baudRates = { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "57600", "115200" };
            _baudRateCombo.Items.AddRange(baudRates);
            _baudRateCombo.Text = SystemParameters.Device.BaudRate.ToString();

            // 数据位：允许 5~8
            string[] dataBits = { "5", "6", "7", "8" };
            _dataBitsCombo.Items.AddRange(dataBits);
            _dataBitsCombo.Text = SystemParameters.Device.DataBits.ToString();

            // 停止位：只给枚举里真正定义过的项
            string[] stopBits = { "None", "One", "Two", "OnePointFive" };
            _stopBitsCombo.Items.AddRange(stopBits);
            _stopBitsCombo.Text = SystemParameters.Device.StopBits.ToString();

            // 校验位：旧版本这里曾误用停止位的取值，固定给出合法项避免再写坏 ini
            string[] parity = { "None", "Odd", "Even", "Mark", "Space" };
            _parityCombo.Items.AddRange(parity);
            _parityCombo.Text = SystemParameters.Device.Parity.ToString();

            // 电表型号：与 ini 里的 EnergyMetersType 取值一致
            string[] meterType = { "DJSF1352", "AMC", "ADL400" };
            _energyMetersTypeCombo.Items.AddRange(meterType);
            _energyMetersTypeCombo.Text = SystemParameters.Device.MeterType.ToConfigValue();
        }

        /// <summary>
        /// 显示当前的数据目录与日志文件路径。
        /// </summary>
        private void InitializeTextBoxes()
        {
            _dataPathText.Text = SystemParameters.Paths.DataDirectory;
            _logFileText.Text = SystemParameters.Paths.LogFile;
        }

        /// <summary>
        /// 窗体加载：填充全部输入控件。
        /// </summary>
        private void SystemSettingsForm_Load(object sender, EventArgs e)
        {
            InitializeComboBoxes();
            InitializeTextBoxes();
        }

        /// <summary>
        /// 选择数据保存目录。只把结果填进文本框，确定保存后才写 ini，取消则不影响现有配置。
        /// </summary>
        private void _browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择数据文件的保存目录";
                dialog.ShowNewFolderButton = true;

                string current = _dataPathText.Text.Trim();
                if (current.Length > 0 && Directory.Exists(current))
                {
                    dialog.SelectedPath = current;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                _dataPathText.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 读取界面上的数据目录并校验：为空或无法创建时提示用户，返回 false。
        /// 与默认目录相同时返回 null，表示仍跟随程序目录，搬动机器不用改配置。
        /// </summary>
        /// <param name="dataRoot">校验后的目录；null 表示使用默认目录</param>
        /// <returns>校验通过返回 true</returns>
        private bool TryReadDataDirectory(out string dataRoot)
        {
            dataRoot = null;
            string text = _dataPathText.Text.Trim();
            if (text.Length == 0)
            {
                MessageBox.Show("数据保存路径不能为空。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string full;
            try
            {
                full = Path.GetFullPath(text);
                Directory.CreateDirectory(full);
            }
            catch (Exception ex)
            {
                Logger.Log("数据保存路径不可用，异常原因为：", ex);
                MessageBox.Show("数据保存路径不可用：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string defaultRoot = Path.GetFullPath(Path.Combine(Application.StartupPath, "Data"));
            char separator = Path.DirectorySeparatorChar;
            bool followsStartupPath = string.Equals(
                full.TrimEnd(separator), defaultRoot.TrimEnd(separator), StringComparison.OrdinalIgnoreCase);
            dataRoot = followsStartupPath ? null : full;
            return true;
        }

        /// <summary>
        /// 把界面上的值写回 ini。写入失败时提示原因，内存中的配置保持不变。
        /// </summary>
        private void _saveButton_Click(object sender, EventArgs e)
        {
            // 在现有配置的副本上改，保存失败时内存里的配置不受影响。
            DeviceConfig config = SystemParameters.Device.Clone();
            config.PortName = _portNameCombo.Text.Trim();

            int baudRate;
            if (int.TryParse(_baudRateCombo.Text, out baudRate))
            {
                config.BaudRate = baudRate;
            }
            int dataBits;
            if (int.TryParse(_dataBitsCombo.Text, out dataBits))
            {
                config.DataBits = dataBits;
            }
            StopBits stopBits;
            if (Enum.TryParse(_stopBitsCombo.Text, out stopBits))
            {
                config.StopBits = stopBits;
            }
            Parity parity;
            if (Enum.TryParse(_parityCombo.Text, out parity))
            {
                config.Parity = parity;
            }
            MeterType meterType;
            if (MeterTypeExtensions.TryParseConfigValue(_energyMetersTypeCombo.Text, out meterType))
            {
                config.MeterType = meterType;
            }

            string dataRoot;
            if (!TryReadDataDirectory(out dataRoot))
            {
                return;
            }

            try
            {
                config.Save(SystemParameters.Paths.SystemFile);
                new StorageConfig { DataDirectory = dataRoot }.Save(SystemParameters.Paths.SystemFile);
            }
            catch (Exception ex)
            {
                Logger.Log("保存配置失败，异常原因为：", ex);
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("保存成功，请重启软件生效。\n数据文件按表型存放在该目录下的子目录中。");
        }
    }
}
