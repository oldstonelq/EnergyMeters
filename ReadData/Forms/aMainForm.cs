using ReadData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            Working = false;
            btn_Start.Enabled = true;
            btn_End.Enabled = false;
            //SystemParas.energyMeters.Close();
        }
        /// <summary>
        /// 自动读数线程
        /// </summary>
        private void Thread_AutoWork()
        {
            try
            {
                while (true)
                {
                    var Voltage = SystemParas.energyMeters.ReadVoltage();
                    var Current = SystemParas.energyMeters.ReadCurrent();
                    var Power = SystemParas.energyMeters.ReadPower();
                    AddRow(Voltage.ToString(), Current.ToString(), Power.ToString());
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
                DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value = DateTime.Now.ToString();
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
                    DialogResult result = MessageBox.Show("是否要打开导出的文件？", "导出成功！", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
            }
        }

        private void 曲线数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Chart form_Chart = new Form_Chart();
            form_Chart.ShowDialog();
            form_Chart.Dispose();
        }
    }
}