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
using System.Windows.Forms.DataVisualization.Charting;

namespace ReadDataSoftware
{
    public partial class aMainForm : Form
    {
        #region 构造函数·私有变量·窗体加载
        /// <summary>
        /// 是否正在读数
        /// </summary>
        private bool Working;
        private const int MaxPoints = 1000;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        /// <summary>
        /// 构造函数
        /// </summary>
        public aMainForm()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aMainForm_Load(object sender, EventArgs e)
        {
            InitializeChart();
            string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "读数软件 V" + Version;
            // 创建标题
            Title title = new Title("电压、电流和功率随时间变化");
            title.Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold);
            chart1.Titles.Add(title);
        }
        #endregion

        #region 按键方法
        /// <summary>
        /// 开始读数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            try
            {
                SystemParas.energyMeters.Open();
                Working = true;
                btn_Start.Enabled = false;
                btn_End.Enabled = true;
                _stopEvent.Reset(); // 重置 _stopEvent
                new Task(() => { Thread_AutoWork(); }).Start();
            }
            catch (Exception)
            {
                MessageBox.Show("设备无法连接，请检查串口是否打开或设备是否连接正常！");
            }
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
                    if (DGV1.RowCount > 0)
                    {
                        string startTime = DGV1.Rows[0].Cells[Column_Time.Name].Value.ToString();
                        string endTime = DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value.ToString();
                        string savefileName = SystemParas.DataFile + "\\" + DateTime.Parse(startTime).ToString("yyyyMMddHHmmss") + "至" + DateTime.Parse(endTime).ToString("yyyyMMddHHmmss") + ".json";
                        if (SystemParas.Datas.Count > 0)
                        {
                            bool success = JsonSerializerHelper.SaveToJsonFile(SystemParas.Datas, savefileName);
                            if (success)
                            {
                                MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                SystemParas.Datas = new List<DataStructure>();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
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
                    FileHelp.ExportCsv(DGV1, saveFileDialog.FileName);
                    DialogResult result = MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError(ex.Message + ex.StackTrace);
                }

            }
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ReadData_Click(object sender, EventArgs e)
        {
            if (Working)
            {
                MessageBox.Show ("工作中，不允许读取", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DGV1.Rows.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON 文件 (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                SystemParas.Datas = JsonSerializerHelper.LoadFromJsonFile<List<DataStructure>>(filePath);
                foreach (DataStructure dataStructure in SystemParas.Datas)
                {
                    DGV1.Rows.Add(dataStructure.Time.ToString(), dataStructure.Voltage.ToString(), dataStructure.Current.ToString(), dataStructure.Power.ToString());
                }
            }
        }
        /// <summary>
        /// 清除表格以及图表数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            if (Working)
            {
                MessageBox.Show("工作中，不允许清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (DGV1.Rows.Count > 0)
            {
                SystemParas.Datas.Clear();
                DGV1.Rows.Clear();
                ClearChart();
                MessageBox.Show("表格已清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("没有数据可以清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Set_Click(object sender, EventArgs e)
        {
            Form_SystemSet form_SystemSet = new Form_SystemSet();
            form_SystemSet.ShowDialog();
            form_SystemSet.Dispose();
        }
        #endregion

        #region 私有方法
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
                    var alarm = SystemParas.energyMeters.ReadAlarm();
                    foreach (var v in alarm)
                    {
                        if (v)
                        {
                            MessageBox.Show("");
                        }
                    }
                    AddRow(Voltage.ToString(), Current.ToString(), Power.ToString());
                    //AddRow(0.ToString(), 0.ToString (), Power.ToString());
                    SystemParas.Datas.Add(new DataStructure() { Time = DateTime.Now, Voltage = Voltage, Current = Current, Power = Power });
                    //SystemParas.ChartDatas.Add(new DataStructure() { Time = DateTime.Now, Voltage = Voltage, Current = Current, Power = Power });
                    if (!Working) throw new Exception();
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                SystemParas.energyMeters.Close();
                _stopEvent.Set(); // 通知主线程数据记录线程已经完成
            }
        }
        /// <summary>
        /// DGV添加一行数据
        /// </summary>
        /// <param name="Voltage">电压值</param>
        /// <param name="Current">电流值</param>
        /// <param name="Power"></param>
        private void AddRow(string Voltage, string Current, string Power)
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
        /// 设置滑动条
        /// </summary>
        /// <param name="dataGridView"></param>
        private void ScrollToBottom(DataGridView dataGridView)
        {
            // 获取总行数
            int rowCount = dataGridView.Rows.Count;

            // 如果行数大于0，滚动到最后一行
            if (rowCount > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = rowCount - 1;
            }
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
        #endregion

        #region 曲线绘制相关
        /// <summary>
        /// 初始化图表
        /// </summary>
        private void InitializeChart()
        {
            // 清除可能存在的旧系列和图表区域
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            // 创建图表区域
            ChartArea chartArea = new ChartArea("ElectricalParameters");
            chartArea.AxisX.Title = "时间";
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartArea.AxisX.Interval = 5;
            chartArea.AxisY.Title = "值";
            chartArea.BackColor = Color.FromArgb(240, 240, 240);
            chartArea.BorderColor = Color.FromArgb(200, 200, 200);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            chart1.ChartAreas.Add(chartArea);
            // 创建三个系列：电压、电流和功率
            CreateSeries("电压", Color.Red);
            CreateSeries("电流", Color.Blue);
            CreateSeries("功率", Color.Green);
        }
        /// <summary>
        /// 清除图表
        /// </summary>
        private void ClearChart()
        {
            // 清除可能存在的旧系列和图表区域
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

        }
        /// <summary>
        /// 创建曲线实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        private void CreateSeries(string name, Color color)
        {
            Series series = new Series(name);
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2;
            series.Color = color;
            series.ChartArea = "ElectricalParameters";
            chart1.Series.Add(series);
        }
        /// <summary>
        /// 定时刷新图表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            ScrollToBottom(DGV1);
            RefreshChart();
        }
        /// <summary>
        /// 增加数据点
        /// </summary>
        /// <param name="seriesName"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        private void AddDataPoint(string seriesName, DateTime time, double value)
        {
            chart1.Series[seriesName].Points.AddXY(time, value);
        }
        /// <summary>
        /// 刷新图表
        /// </summary>
        private void RefreshChart()
        {
            if (SystemParas.Datas.Count<=0)
            {
                return;
            }
            //重绘图表
            InitializeChart();
            // 添加所有数据点
            foreach (var data in SystemParas.Datas)
            {
                AddDataPoint("电压", data.Time, data.Voltage);
                AddDataPoint("电流", data.Time, data.Current);
                AddDataPoint("功率", data.Time, data.Power);
            }

            // 限制数据点数量
            LimitDataPoints();

            // 自动调整Y轴范围
            AdjustAxisRanges();

            // 更新图表
            chart1.Invalidate();
        }
        /// <summary>
        /// 限制数据点数量
        /// </summary>
        private void LimitDataPoints()
        {
            foreach (Series series in chart1.Series)
            {
                if (series.Points.Count > MaxPoints)
                {
                    series.Points.RemoveAt(0);
                }
            }
        }
        /// <summary>
        /// 调整xy轴范围
        /// </summary>
        private void AdjustAxisRanges()
        {
            // 调整Y轴范围
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (Series series in chart1.Series)
            {
                foreach (DataPoint point in series.Points)
                {
                    minY = Math.Min(minY, point.YValues[0]);
                    maxY = Math.Max(maxY, point.YValues[0]);
                }
            }

            double yMargin = (maxY - minY) * 0.1;
            if (yMargin < 0.1) yMargin = 0.1;

            chart1.ChartAreas[0].AxisY.Minimum = minY - yMargin;
            chart1.ChartAreas[0].AxisY.Maximum = maxY + yMargin;

            // 调整X轴范围，显示最近的20个数据点时间范围
            if (chart1.Series[0].Points.Count > 0)
            {
                DateTime minTime = DateTime.MaxValue;
                DateTime maxTime = DateTime.MinValue;

                foreach (Series series in chart1.Series)
                {
                    foreach (DataPoint point in series.Points)
                    {
                        DateTime pointTime = DateTime.FromOADate(point.XValue);
                        minTime = DateTime.MinValue.Equals(minTime) ? pointTime :
                                  (pointTime < minTime ? pointTime : minTime);
                        maxTime = DateTime.MinValue.Equals(maxTime) ? pointTime :
                                  (pointTime > maxTime ? pointTime : maxTime);
                    }
                }

                // 添加一些边距
                TimeSpan xMargin = TimeSpan.FromSeconds(5);
                chart1.ChartAreas[0].AxisX.Minimum = minTime.Add(-xMargin).ToOADate();
                chart1.ChartAreas[0].AxisX.Maximum = maxTime.Add(xMargin).ToOADate();
            }
        }
        #endregion

    }
}


