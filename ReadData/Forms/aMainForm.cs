using ReadData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    public partial class aMainForm : Form
    {
        /// <summary>
        /// 是否正在读数
        /// </summary>
        private bool Working;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        /// <summary>
        /// 构造函数
        /// </summary>
        public aMainForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 开始读数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            Working = true;
            btn_Start.Enabled = false;
            btn_End.Enabled = true;
            SystemParas.energyMeters.Open();
            new Task(() => { Thread_AutoWork(); }).Start();
        }
        /// <summary>
        /// 手动停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_End_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定停止并保存数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _stopEvent.Set(); // 通知数据记录线程停止
                    Working = false;
                    btn_Start.Enabled = true;
                    btn_End.Enabled = false;
                    _stopEvent.WaitOne(); // 等待数据记录线程完成
                    string startTime = DGV1.Rows[0].Cells[Column_Time.Name].Value.ToString();
                    string endTime = DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value.ToString();
                    string savefileName = SystemParas.DataFile+ "\\"+ DateTime.Parse(startTime).ToString("yyyyMMddHHmmss") + "至" + DateTime.Parse(endTime).ToString("yyyyMMddHHmmss") + ".json";
                    bool success = JsonSerializerHelper.SaveToJsonFile(SystemParas.Datas, savefileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    
                }
            }
            //SystemParas.energyMeters.Close();
        }
        /// <summary>
        /// 自动读数线程
        /// </summary>
        private void Thread_AutoWork()
        {
            try
            {
                while (!_stopEvent.WaitOne(0))
                {
                    var Voltage = SystemParas.energyMeters.ReadVoltage();
                    var Current = SystemParas.energyMeters.ReadCurrent();
                    var Power = SystemParas.energyMeters.ReadPower();
                    AddRow(Voltage.ToString(), Current.ToString(), Power.ToString());
                    SystemParas.Datas.Add(new DataStructure() {Time = DateTime.Now, Voltage = Voltage, Current = Current, Power = Power });
                    SystemParas.ChartDatas.Add(new DataStructure() { Time = DateTime.Now, Voltage = Voltage, Current = Current, Power = Power });
                    if (!Working) throw new Exception("手动取消");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                SystemParas.energyMeters.Close();
                _stopEvent.Set(); // 通知主线程数据记录线程已经完成
            }
        }
        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 系统设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_SystemSet form_SystemSet = new Form_SystemSet();
            form_SystemSet.ShowDialog();
            form_SystemSet.Dispose();
        }
        /// <summary>
        /// DGV添加一行数据
        /// </summary>
        /// <param name="Voltage">电压值</param>
        /// <param name="Current">电流值</param>
        /// <param name="Power"></param>
        private void AddRow(string Voltage,string Current,string Power)
        {
            this.Invoke(new Action(() =>
            {
                DGV1.Rows.Add();
                DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Voltage.Name].Value = Voltage;
                DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Current.Name].Value = Current;
                DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Power.Name].Value = Power;
            }));
        }
        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aMainForm_Load(object sender, EventArgs e)
        {
            string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "读数软件 V" + Version;
        }
        /// <summary>
        /// 导出表格数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ExportCsv_Click(object sender, EventArgs e)
        {
            if (DGV1.Rows.Count < 1)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV 文件 (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileHelp.ExportCsv(DGV1,saveFileDialog.FileName);
                    DialogResult result = MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError(ex.Message+ex.StackTrace);
                }
            
            }
        }

        private void 曲线数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Chart form_Chart = new Form_Chart();
            form_Chart.ShowDialog();
            form_Chart.Dispose();
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            // 检查日志文件夹是否存在
            string logFolderPath = Path.GetDirectoryName(SystemParas.logFilePath);
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            // 写入日志
            using (StreamWriter writer = new StreamWriter(SystemParas.logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private void btn_ReadData_Click(object sender, EventArgs e)
        {
            DGV1 .Rows.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                List<DataStructure> loadedData = JsonSerializerHelper.LoadFromJsonFile<List<DataStructure>>(filePath);
                foreach (DataStructure dataStructure in loadedData)
                {
                   DGV1 .Rows.Add(dataStructure.Time .ToString (), dataStructure.Voltage.ToString (),dataStructure.Current.ToString (),dataStructure .Power.ToString ());
                }
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            DGV1.Rows.Clear();  
        }
    }
}


