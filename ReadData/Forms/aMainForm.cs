using ReadData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
            this.Text = "读数软件 Version-" + Version;
            if (SystemParas.EnergyMetersType == "AMC")
            {
                DGV1.Visible = false;
                DGV1.Enabled = false;
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 99.46f);  // 第一列，占总宽度的 50%
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0.54f);  // 第二列，占总宽度的 30%
                tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, 250f);  // 第三列，占总宽度的250像素
            }
            else
            { 
                DGV2.Visible = false;
                DGV2.Enabled = false;
            }
            //this.Width = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * 0.8);
            //this.Height = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Height * 0.8);
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
            catch (Exception ex)
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
                    if (DGV1.RowCount > 0 && SystemParas.EnergyMetersType == "DJSF1352")
                    {
                        string startTime = DGV1.Rows[0].Cells[Column_Time.Name].Value.ToString();
                        string endTime = DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value.ToString();
                        string savefileName = SystemParas.DJSF1352_DataFilePath + "\\" + DateTime.Parse(startTime).ToString("yyyyMMddHHmmss") + "至" + DateTime.Parse(endTime).ToString("yyyyMMddHHmmss") + ".json";
                        if (SystemParas.Data1.Count > 0)
                        {
                            bool success = JsonSerializerHelper.SaveToJsonFile(SystemParas.Data1, savefileName);
                            if (success)
                            {
                                MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                SystemParas.Data1 = new List<DJSF1352_DataStructure>();
                            }
                        }
                    }
                    else if (DGV2.RowCount > 0 && SystemParas.EnergyMetersType == "AMC")
                    {
                        string startTime = DGV2.Rows[0].Cells[Column_RealTime.Name].Value.ToString();
                        string endTime = DGV2.Rows[DGV2.RowCount - 1].Cells[Column_RealTime.Name].Value.ToString();
                        string savefileName = SystemParas.AMC_DataFilePath + "\\" + DateTime.Parse(startTime).ToString("yyyyMMddHHmmss") + "至" + DateTime.Parse(endTime).ToString("yyyyMMddHHmmss") + ".json";
                        if (SystemParas.Data2.Count > 0)
                        {
                            bool success = JsonSerializerHelper.SaveToJsonFile(SystemParas.Data2, savefileName);
                            if (success)
                            {
                                MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                SystemParas.Data2 = new List<AMC_DataStructure>();
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
                    LogError("手动停止工作时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
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
            
            if ((DGV1.Rows.Count < 1&&SystemParas.EnergyMetersType == "DJSF1352")|| (DGV2.Rows.Count < 1 && SystemParas.EnergyMetersType == "AMC"))
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
                    if (SystemParas.EnergyMetersType == "DJSF1352")
                    {
                        FileHelp.ExportCsv(DGV1, saveFileDialog.FileName);
                    }
                    else if (SystemParas.EnergyMetersType == "AMC")
                    {
                        FileHelp.ExportCsv(DGV2, saveFileDialog.FileName);
                    }
                    DialogResult result = MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError("导出表格数据时出现异常，异常原因为："+ex.Message + ex.StackTrace);
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON 文件 (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (SystemParas.EnergyMetersType == "DJSF1352")
                {
                    DGV1.Rows.Clear();
                    SystemParas.Data1 = JsonSerializerHelper.LoadFromJsonFile<List<DJSF1352_DataStructure>>(filePath);
                    if (SystemParas.Data1 == null)
                    {
                        MessageBox.Show("不是该类型仪表数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (SystemParas.Data1.Count == 0)
                    {
                        MessageBox.Show("无数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    foreach (var dataStructure in SystemParas.Data1)
                    {
                        if (dataStructure is DJSF1352_DataStructure data)
                        {
                            DGV1.Rows.Add(dataStructure.Time.ToString(), dataStructure.Voltage.ToString(), dataStructure.Current.ToString(), dataStructure.Power.ToString());
                        }
                    }
                }
                else if (SystemParas.EnergyMetersType == "AMC")
                {
                    DGV2.Rows.Clear();
                    SystemParas.Data2 = JsonSerializerHelper.LoadFromJsonFile<List<AMC_DataStructure>>(filePath);
                    if (SystemParas.Data2 == null)
                    {
                        MessageBox.Show("不是该类型仪表数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (SystemParas.Data2.Count == 0)
                    {
                        MessageBox.Show("无数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    foreach (var dataStructure in SystemParas.Data2)
                    {
                        if (dataStructure is AMC_DataStructure data)
                        {
                            DGV2.Rows.Add(
                                dataStructure.Time.ToString(),
                                dataStructure.PhaseVoltageA.ToString(),
                                dataStructure.PhaseVoltageB.ToString(),
                                dataStructure.PhaseVoltageC.ToString(),
                                dataStructure.LineVoltageUAB.ToString(),
                                dataStructure.LineVoltageUBC.ToString(),
                                dataStructure.LineVoltageUAC.ToString(),
                                dataStructure.CurrentA.ToString(),
                                dataStructure.CurrentB.ToString(),
                                dataStructure.CurrentC.ToString(),
                                dataStructure.ActivePowerOfPhaseA.ToString(),
                                dataStructure.ActivePowerOfPhaseB.ToString(),
                                dataStructure.ActivePowerOfPhaseC.ToString(),
                                dataStructure.TotalActivePower.ToString(),
                                dataStructure.PhaseReactivePowerA.ToString(),
                                dataStructure.PhaseReactivePowerB.ToString(),
                                dataStructure.PhaseReactivePowerC.ToString(),
                                dataStructure.TotalReactivePower.ToString()
                                );
                        }
                    }
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

            if (DGV1.Rows.Count > 0 && SystemParas.EnergyMetersType == "DJSF1352")
            {
                SystemParas.Data1.Clear();
                DGV1.Rows.Clear();
                ClearChart();
                MessageBox.Show("表格已清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (DGV2.Rows.Count > 0 && SystemParas.EnergyMetersType == "AMC")
            {
                SystemParas.Data2.Clear();
                DGV2.Rows.Clear();
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
            form_SystemSet.SettingsSaved += SettingsForm_SettingsSaved;
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
                    AddRow(Voltage, Current, Power);
                    if (SystemParas.EnergyMetersType == "DJSF1352")
                    {
                        SystemParas.Data1.Add(new DJSF1352_DataStructure() 
                        { Time = DateTime.Now, Voltage = Voltage[0], Current = Current[0], Power = Power[0] });
                    }
                    else if (SystemParas.EnergyMetersType == "AMC")
                    {
                        SystemParas.Data2.Add(new AMC_DataStructure()
                        { 
                            Time = DateTime.Now,
                            PhaseVoltageA = Voltage[0],
                            PhaseVoltageB = Voltage[1],
                            PhaseVoltageC = Voltage[2],
                            LineVoltageUAB = Voltage[3],
                            LineVoltageUBC = Voltage[4],
                            LineVoltageUAC = Voltage[5],
                            CurrentA = Current[0],
                            CurrentB = Current[1],
                            CurrentC = Current[2],
                            ActivePowerOfPhaseA= Power[0],
                            ActivePowerOfPhaseB = Power[1],
                            ActivePowerOfPhaseC= Power[2],
                            TotalActivePower = Power[3],
                            PhaseReactivePowerA= Power[4],
                            PhaseReactivePowerB= Power[5],
                            PhaseReactivePowerC= Power[6],
                            TotalReactivePower= Power[7],
                        }
                        );
                    }
                    if (!Working) throw new Exception("手动停止工作");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                LogError("Thread_AutoWork线程执行出现异常,异常原因为：" +ex.Message + ex.StackTrace);
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
        private void AddRow(double[] Voltage, double[] Current, double[] Power)
        {
            if (SystemParas.EnergyMetersType == "DJSF1352")
            {
                this.Invoke(new Action(() =>
                {
                    DGV1.Rows.Add();
                    DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Voltage.Name].Value = Voltage[0].ToString();
                    DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Current.Name].Value = Current[0].ToString();
                    DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Power.Name].Value = Power[0].ToString();
                }));
            }
            else if (SystemParas.EnergyMetersType == "AMC")
            {
                this.Invoke(new Action(() =>
                {
                    DGV2.Rows.Add();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_RealTime.Name].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseVoltageA.Name].Value = Voltage[0].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseVoltageB.Name].Value = Voltage[1].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseVoltageC.Name].Value = Voltage[2].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_LineVoltageUAB.Name].Value = Voltage[3].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_LineVoltageUBC.Name].Value = Voltage[4].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_LineVoltageUAC.Name].Value = Voltage[5].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_CurrentA.Name].Value = Current[0].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_CurrentB.Name].Value = Current[1].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_CurrentC.Name].Value = Current[2].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_ActivePowerOfPhaseA.Name].Value = Power[0].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_ActivePowerOfPhaseB.Name].Value = Power[1].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_ActivePowerOfPhaseC.Name].Value = Power[2].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_TotalActivePower.Name].Value = Power[3].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseReactivePowerA.Name].Value = Power[4].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseReactivePowerB.Name].Value = Power[5].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_PhaseReactivePowerC.Name].Value = Power[6].ToString();
                    DGV2.Rows[DGV2.RowCount - 1].Cells[Column_TotalReactivePower.Name].Value = Power[7].ToString();
                }));
            }
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
        private void SettingsForm_SettingsSaved(object sender, EventArgs e)
        {
            //this .Close();
            //// 保存设置后重启应用程序
            //Application.Restart();
            //Environment.Exit(0);
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
            if (Working && SystemParas.EnergyMetersType == "DJSF1352")
            {
                ScrollToBottom(DGV1);
            }
            else if (Working && SystemParas.EnergyMetersType == "AMC")
            {
                ScrollToBottom(DGV2);
            }
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
            if (SystemParas.Data1 == null||SystemParas.Data1.Count<=0)
            {
                return;
            }
            //重绘图表
            InitializeChart();
            // 添加所有数据点
            foreach (var data in SystemParas.Data1)
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


