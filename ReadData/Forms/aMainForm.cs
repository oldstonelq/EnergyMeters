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
        private bool _isWorking;
        private const int MaxPoints = 1000;
        /// <summary>
        /// 采集数据的读写锁，避免界面刷新与采集线程同时访问集合
        /// </summary>
        private readonly object _sampleLock = new object();
        /// <summary>
        /// 已绘制到图表的数据条数
        /// </summary>
        private int _chartedSampleCount;
        /// <summary>
        /// 读数线程的取消信号
        /// </summary>
        private CancellationTokenSource _cancellation;
        /// <summary>
        /// 读数任务，停止时用于等待线程真正退出
        /// </summary>
        private Task _workTask;
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
            if (SystemParas.EnergyMetersType == "AMC"|| SystemParas.EnergyMetersType == "ADL400")
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
                SystemParas.EnergyMeters.Open();
            }
            catch (Exception ex)
            {
                LogError("打开串口失败，异常原因为：" + ex.Message + ex.StackTrace);
                MessageBox.Show("设备无法连接，请检查串口是否打开或设备是否连接正常！");
                return;
            }

            _isWorking = true;
            btn_Start.Enabled = false;
            btn_End.Enabled = true;
            _cancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellation.Token;
            _workTask = Task.Run(() => Thread_AutoWork(cancellationToken));
        }
        /// <summary>
        /// 手动停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_End_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定停止并保存数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            btn_End.Enabled = false;
            _isWorking = false;
            if (_cancellation != null)
            {
                _cancellation.Cancel();
            }

            try
            {
                // 必须等采集线程真正退出后再落盘，否则保存过程中集合仍会被追加。
                if (_workTask != null)
                {
                    await _workTask;
                }
            }
            catch (Exception ex)
            {
                LogError("停止读数时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }
            finally
            {
                _workTask = null;
                if (_cancellation != null)
                {
                    _cancellation.Dispose();
                    _cancellation = null;
                }
            }

            btn_Start.Enabled = true;
            SaveCollectedData();
        }
        /// <summary>
        /// 导出表格数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ExportCsv_Click(object sender, EventArgs e)
        {
            
            if ((DGV1.Rows.Count < 1&&SystemParas.EnergyMetersType == "DJSF1352")|| (DGV2.Rows.Count < 1 && SystemParas.EnergyMetersType == "AMC") || (DGV2.Rows.Count < 1 && SystemParas.EnergyMetersType == "ADL400"))
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
                    else if (SystemParas.EnergyMetersType == "AMC" || SystemParas.EnergyMetersType == "ADL400")
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
            if (_isWorking)
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
                    List<DJSF1352_DataStructure> loaded = JsonSerializerHelper.LoadFromJsonFile<List<DJSF1352_DataStructure>>(filePath);
                    if (loaded == null)
                    {
                        MessageBox.Show("不是该类型仪表数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (loaded.Count == 0)
                    {
                        MessageBox.Show("无数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    // 校验通过后再替换原有集合，避免读取失败时把当前数据置空。
                    SystemParas.Data1 = loaded;
                    DGV1.Rows.Clear();
                    ResetChart();
                    foreach (DJSF1352_DataStructure dataStructure in loaded)
                    {
                        DGV1.Rows.Add(dataStructure.Time.ToString(), dataStructure.Voltage.ToString(), dataStructure.Current.ToString(), dataStructure.Power.ToString());
                    }
                }
                else if (SystemParas.EnergyMetersType == "AMC" || SystemParas.EnergyMetersType == "ADL400")
                {
                    List<AMC_DataStructure> loaded = JsonSerializerHelper.LoadFromJsonFile<List<AMC_DataStructure>>(filePath);
                    if (loaded == null)
                    {
                        MessageBox.Show("不是该类型仪表数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (loaded.Count == 0)
                    {
                        MessageBox.Show("无数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    SystemParas.Data2 = loaded;
                    DGV2.Rows.Clear();
                    ResetChart();
                    foreach (AMC_DataStructure dataStructure in loaded)
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
        /// <summary>
        /// 清除表格以及图表数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            if (_isWorking)
            {
                MessageBox.Show("工作中，不允许清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (DGV1.Rows.Count > 0 && SystemParas.EnergyMetersType == "DJSF1352")
            {
                SystemParas.Data1.Clear();
                DGV1.Rows.Clear();
                ResetChart();
                MessageBox.Show("表格已清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (DGV2.Rows.Count > 0 && SystemParas.EnergyMetersType == "AMC"|| SystemParas.EnergyMetersType == "ADL400")
            {
                SystemParas.Data2.Clear();
                DGV2.Rows.Clear();
                ResetChart();
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
        /// 把本次采集的数据写入 JSON 文件
        /// </summary>
        private void SaveCollectedData()
        {
            try
            {
                if (SystemParas.EnergyMetersType == "DJSF1352")
                {
                    if (DGV1.RowCount == 0 || SystemParas.Data1.Count == 0)
                    {
                        MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    string startTime = DGV1.Rows[0].Cells[Column_Time.Name].Value.ToString();
                    string endTime = DGV1.Rows[DGV1.RowCount - 1].Cells[Column_Time.Name].Value.ToString();
                    string saveFileName = BuildDataFileName(SystemParas.DJSF1352_DataFilePath, startTime, endTime);
                    if (JsonSerializerHelper.SaveToJsonFile(SystemParas.Data1, saveFileName))
                    {
                        MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SystemParas.Data1 = new List<DJSF1352_DataStructure>();
                    }
                }
                else if (SystemParas.EnergyMetersType == "AMC")
                {
                    SaveThreePhaseData(SystemParas.AMC_DataFilePath);
                }
                else if (SystemParas.EnergyMetersType == "ADL400")
                {
                    SaveThreePhaseData(SystemParas.ADL400_DataFilePath);
                }
                else
                {
                    MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogError("保存数据时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 保存三相表（AMC / ADL400）采集的数据
        /// </summary>
        /// <param name="directory">数据文件目录</param>
        private void SaveThreePhaseData(string directory)
        {
            if (DGV2.RowCount == 0 || SystemParas.Data2.Count == 0)
            {
                MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string startTime = DGV2.Rows[0].Cells[Column_RealTime.Name].Value.ToString();
            string endTime = DGV2.Rows[DGV2.RowCount - 1].Cells[Column_RealTime.Name].Value.ToString();
            string saveFileName = BuildDataFileName(directory, startTime, endTime);
            if (JsonSerializerHelper.SaveToJsonFile(SystemParas.Data2, saveFileName))
            {
                MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SystemParas.Data2 = new List<AMC_DataStructure>();
            }
        }

        /// <summary>
        /// 按起止时间生成数据文件名
        /// </summary>
        private static string BuildDataFileName(string directory, string startTime, string endTime)
        {
            return directory + "\\" + DateTime.Parse(startTime).ToString("yyyyMMddHHmmss") + "至"
                + DateTime.Parse(endTime).ToString("yyyyMMddHHmmss") + ".json";
        }

        /// <summary>
        /// 自动读数线程
        /// </summary>
        /// <param name="cancellationToken">停止信号</param>
        private void Thread_AutoWork(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    double[] voltage = SystemParas.EnergyMeters.ReadVoltage();
                    double[] current = SystemParas.EnergyMeters.ReadCurrent();
                    double[] power = SystemParas.EnergyMeters.ReadPower();
                    AddRow(voltage, current, power);
                    RecordSample(voltage, current, power);
                    // 取消时立即返回，不必等满整个采样周期。
                    cancellationToken.WaitHandle.WaitOne(1000);
                }
            }
            catch (Exception ex)
            {
                LogError("Thread_AutoWork线程执行出现异常,异常原因为：" + ex.Message + ex.StackTrace);
                NotifyWorkerFailed(ex);
            }
            finally
            {
                SystemParas.EnergyMeters.Close();
            }
        }

        /// <summary>
        /// 采集线程异常退出时恢复按钮状态并提示用户
        /// </summary>
        /// <param name="exception">导致线程退出的异常</param>
        private void NotifyWorkerFailed(Exception exception)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            // 用 BeginInvoke 投递，避免本线程阻塞在等待界面线程上。
            BeginInvoke(new Action(() =>
            {
                _isWorking = false;
                btn_Start.Enabled = true;
                btn_End.Enabled = false;
                MessageBox.Show("读数中断：" + exception.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }

        /// <summary>
        /// 把一次采样追加到对应型号的数据集合
        /// </summary>
        /// <param name="voltage">电压值</param>
        /// <param name="current">电流值</param>
        /// <param name="power">功率值</param>
        private void RecordSample(double[] voltage, double[] current, double[] power)
        {
            if (SystemParas.EnergyMetersType == "DJSF1352")
            {
                lock (_sampleLock)
                {
                    SystemParas.Data1.Add(new DJSF1352_DataStructure()
                    {
                        Time = DateTime.Now,
                        Voltage = voltage[0],
                        Current = current[0],
                        Power = power[0]
                    });
                }
            }
            else if (SystemParas.EnergyMetersType == "AMC" || SystemParas.EnergyMetersType == "ADL400")
            {
                lock (_sampleLock)
                {
                    SystemParas.Data2.Add(new AMC_DataStructure()
                    {
                        Time = DateTime.Now,
                        PhaseVoltageA = voltage[0],
                        PhaseVoltageB = voltage[1],
                        PhaseVoltageC = voltage[2],
                        LineVoltageUAB = voltage[3],
                        LineVoltageUBC = voltage[4],
                        LineVoltageUAC = voltage[5],
                        CurrentA = current[0],
                        CurrentB = current[1],
                        CurrentC = current[2],
                        ActivePowerOfPhaseA = power[0],
                        ActivePowerOfPhaseB = power[1],
                        ActivePowerOfPhaseC = power[2],
                        TotalActivePower = power[3],
                        PhaseReactivePowerA = power[4],
                        PhaseReactivePowerB = power[5],
                        PhaseReactivePowerC = power[6],
                        TotalReactivePower = power[7],
                    });
                }
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
            else if (SystemParas.EnergyMetersType == "AMC" || SystemParas.EnergyMetersType == "ADL400")
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
        /// 重置图表：清空曲线并让绘制进度归零
        /// </summary>
        private void ResetChart()
        {
            InitializeChart();
            _chartedSampleCount = 0;
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
            if (_isWorking && SystemParas.EnergyMetersType == "DJSF1352")
            {
                ScrollToBottom(DGV1);
            }
            else if (_isWorking && (SystemParas.EnergyMetersType == "AMC" || SystemParas.EnergyMetersType == "ADL400"))
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
        /// 刷新图表。只追加新增的数据点，不再每次重建全部曲线。
        /// </summary>
        private void RefreshChart()
        {
            if (SystemParas.EnergyMetersType == "DJSF1352")
            {
                RefreshChartFromSinglePhaseData();
            }
            else
            {
                RefreshChartFromThreePhaseData();
            }
        }
        /// <summary>
        /// 刷新直流表(DJSF1352)曲线
        /// </summary>
        private void RefreshChartFromSinglePhaseData()
        {
            // 采集线程仍在追加数据，先取快照再遍历，避免枚举过程中集合被修改。
            List<DJSF1352_DataStructure> samples;
            lock (_sampleLock)
            {
                samples = new List<DJSF1352_DataStructure>(SystemParas.Data1);
            }
            if (samples.Count < _chartedSampleCount)
            {
                // 数据被清空或整体替换，重新开始绘制
                ResetChart();
            }
            if (samples.Count == _chartedSampleCount)
            {
                return;
            }
            for (int i = _chartedSampleCount; i < samples.Count; i++)
            {
                DJSF1352_DataStructure data = samples[i];
                AddDataPoint("电压", data.Time, data.Voltage);
                AddDataPoint("电流", data.Time, data.Current);
                AddDataPoint("功率", data.Time, data.Power);
            }
            _chartedSampleCount = samples.Count;

            LimitDataPoints();
            AdjustAxisRanges();
            chart1.Invalidate();
        }
        /// <summary>
        /// 刷新三相表(AMC / ADL400)曲线，取 A 相电压、A 相电流与总有功功率作代表
        /// </summary>
        private void RefreshChartFromThreePhaseData()
        {
            List<AMC_DataStructure> samples;
            lock (_sampleLock)
            {
                samples = new List<AMC_DataStructure>(SystemParas.Data2);
            }
            if (samples.Count < _chartedSampleCount)
            {
                ResetChart();
            }
            if (samples.Count == _chartedSampleCount)
            {
                return;
            }
            for (int i = _chartedSampleCount; i < samples.Count; i++)
            {
                AMC_DataStructure data = samples[i];
                AddDataPoint("电压", data.Time, data.PhaseVoltageA);
                AddDataPoint("电流", data.Time, data.CurrentA);
                AddDataPoint("功率", data.Time, data.TotalActivePower);
            }
            _chartedSampleCount = samples.Count;

            LimitDataPoints();
            AdjustAxisRanges();
            chart1.Invalidate();
        }
        /// <summary>
        /// 限制数据点数量
        /// </summary>
        private void LimitDataPoints()
        {
            foreach (Series series in chart1.Series)
            {
                while (series.Points.Count > MaxPoints)
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


