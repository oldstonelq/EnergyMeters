using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ReadDataSoftware
{
    public partial class aMainForm : Form
    {
        #region 构造函数·私有变量·窗体加载
        /// <summary>
        /// 单条曲线保留的最大点数
        /// </summary>
        private const int MaxPoints = 1000;
        /// <summary>
        /// 两次采样的间隔，单位毫秒
        /// </summary>
        private const int SampleIntervalMilliseconds = 1000;

        /// <summary>
        /// 是否正在读数
        /// </summary>
        private bool _isWorking;
        /// <summary>
        /// 已绘制到图表的数据条数
        /// </summary>
        private int _chartedSampleCount;
        /// <summary>
        /// 采集服务
        /// </summary>
        private MeterSampler _sampler;
        /// <summary>
        /// 本次采集的样本，只在界面线程访问
        /// </summary>
        private List<MeterSample> _samples = new List<MeterSample>();
        /// <summary>
        /// 当前表型的通道定义
        /// </summary>
        private IReadOnlyList<ChannelDescriptor> _channels;
        /// <summary>
        /// 上图的通道下标
        /// </summary>
        private int[] _chartChannelIndexes;
        /// <summary>
        /// 上图的曲线名称，与通道下标一一对应
        /// </summary>
        private string[] _chartSeriesNames;

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
        private void aMainForm_Load(object sender, EventArgs e)
        {
            _channels = SystemParas.EnergyMeters.Channels;
            InitializeChartContext();
            InitializeChart();

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "读数软件 Version-" + version;

            if (SystemParas.Device.MeterType == MeterType.Djsf1352)
            {
                DGV2.Visible = false;
                DGV2.Enabled = false;
            }
            else
            {
                DGV1.Visible = false;
                DGV1.Enabled = false;
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 99.46f);
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0.54f);
                tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, 250f);
            }

            _sampler = new MeterSampler(SystemParas.EnergyMeters);
            _sampler.SampleReady += OnSampleReady;
            _sampler.SampleFailed += OnSampleFailed;
        }

        /// <summary>
        /// 窗体关闭时确保采集线程退出并释放串口
        /// </summary>
        private void aMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_sampler != null)
            {
                _sampler.Dispose();
            }
        }
        #endregion

        #region 按键方法
        /// <summary>
        /// 开始读数
        /// </summary>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            try
            {
                _sampler.Start(TimeSpan.FromMilliseconds(SampleIntervalMilliseconds));
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
        }

        /// <summary>
        /// 手动停止
        /// </summary>
        private async void btn_End_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定停止并保存数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            btn_End.Enabled = false;
            _isWorking = false;

            try
            {
                // 等采集线程真正退出，保证此前的样本都已落到 _samples 里。
                await _sampler.StopAsync();
            }
            catch (Exception ex)
            {
                LogError("停止读数时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }

            btn_Start.Enabled = true;
            SaveCollectedData();
        }

        /// <summary>
        /// 导出表格数据
        /// </summary>
        private void btn_ExportCsv_Click(object sender, EventArgs e)
        {
            DataGridView grid = GetActiveGrid();
            if (grid.Rows.Count < 1)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV 文件 (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                string error;
                if (!FileHelp.ExportCsv(grid, saveFileDialog.FileName, out error))
                {
                    LogError("导出表格数据失败：" + error);
                    MessageBox.Show("导出失败：" + error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult openResult = MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (openResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError("导出表格数据时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 读取已保存的数据文件
        /// </summary>
        private void btn_ReadData_Click(object sender, EventArgs e)
        {
            if (_isWorking)
            {
                MessageBox.Show("工作中，不允许读取", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON 文件 (*.json)|*.json";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            SampleDocument document;
            try
            {
                document = SampleRepository.Load(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError("读取数据文件失败，异常原因为：" + ex.Message + ex.StackTrace);
                return;
            }

            if (document == null)
            {
                MessageBox.Show("文件内容无法识别", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (document.Samples.Count == 0)
            {
                MessageBox.Show("无数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (document.Channels.Count != _channels.Count)
            {
                MessageBox.Show("文件中的表型与当前配置不一致，无法显示", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 校验通过后再替换现有数据，避免读取失败时把当前采集结果清空。
            _samples = new List<MeterSample>(document.Samples);
            GetActiveGrid().Rows.Clear();
            ResetChart();
            RestoreRows(document);
        }

        /// <summary>
        /// 清除表格以及图表数据
        /// </summary>
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            if (_isWorking)
            {
                MessageBox.Show("工作中，不允许清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_samples.Count == 0)
            {
                MessageBox.Show("没有数据可以清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _samples.Clear();
            GetActiveGrid().Rows.Clear();
            ResetChart();
            MessageBox.Show("表格已清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        private void btn_Set_Click(object sender, EventArgs e)
        {
            Form_SystemSet form_SystemSet = new Form_SystemSet();
            form_SystemSet.ShowDialog();
            form_SystemSet.Dispose();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 采集线程拿到数据后切回界面线程追加
        /// </summary>
        private void OnSampleReady(object sender, MeterSampleEventArgs e)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            try
            {
                // 用同步 Invoke 保证顺序，界面此时不会阻塞在采集线程上，不存在死锁。
                Invoke(new Action(() => AppendSample(e.Sample)));
            }
            catch (InvalidOperationException)
            {
                // 窗体正在关闭，丢弃这一帧即可。
            }
        }

        /// <summary>
        /// 采集中断：恢复按钮状态并提示用户
        /// </summary>
        private void OnSampleFailed(object sender, SamplingErrorEventArgs e)
        {
            LogError("采集线程执行出现异常,异常原因为：" + e.Error.Message + e.Error.StackTrace);
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            BeginInvoke(new Action(() =>
            {
                _isWorking = false;
                btn_Start.Enabled = true;
                btn_End.Enabled = false;
                MessageBox.Show("读数中断：" + e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }

        /// <summary>
        /// 追加一帧样本到界面
        /// </summary>
        private void AppendSample(MeterSample sample)
        {
            _samples.Add(sample);
            AddRow(sample);
        }

        /// <summary>
        /// 把样本追加为表格的一行
        /// </summary>
        private void AddRow(MeterSample sample)
        {
            string time = sample.Time.ToString("yyyy-MM-dd HH:mm:ss");
            if (SystemParas.Device.MeterType == MeterType.Djsf1352)
            {
                DGV1.Rows.Add(time,
                    _channels[0].Format(sample.GetValue(0)),
                    _channels[1].Format(sample.GetValue(1)),
                    _channels[2].Format(sample.GetValue(2)));
                return;
            }

            object[] cells = new object[_channels.Count + 1];
            cells[0] = time;
            for (int i = 0; i < _channels.Count; i++)
            {
                cells[i + 1] = _channels[i].Format(sample.GetValue(i));
            }
            DGV2.Rows.Add(cells);
        }

        /// <summary>
        /// 把已保存文件的样本回填到表格
        /// </summary>
        private void RestoreRows(SampleDocument document)
        {
            bool isSinglePhase = SystemParas.Device.MeterType == MeterType.Djsf1352;
            foreach (MeterSample sample in document.Samples)
            {
                string time = sample.Time.ToString("yyyy-MM-dd HH:mm:ss");
                if (isSinglePhase)
                {
                    DGV1.Rows.Add(time,
                        document.Channels[0].Format(sample.GetValue(0)),
                        document.Channels[1].Format(sample.GetValue(1)),
                        document.Channels[2].Format(sample.GetValue(2)));
                    continue;
                }

                object[] cells = new object[document.Channels.Count + 1];
                cells[0] = time;
                for (int i = 0; i < document.Channels.Count; i++)
                {
                    cells[i + 1] = document.Channels[i].Format(sample.GetValue(i));
                }
                DGV2.Rows.Add(cells);
            }
        }

        /// <summary>
        /// 取当前表型对应的表格
        /// </summary>
        private DataGridView GetActiveGrid()
        {
            return SystemParas.Device.MeterType == MeterType.Djsf1352 ? DGV1 : DGV2;
        }

        /// <summary>
        /// 把本次采集的数据写入 JSON 文件
        /// </summary>
        private void SaveCollectedData()
        {
            if (_samples.Count == 0)
            {
                MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                MeterType meterType = SystemParas.Device.MeterType;
                string directory = SystemParas.Paths.GetDataDirectory(meterType);
                string fileName = Path.Combine(directory, BuildDataFileName());
                SampleRepository.Save(fileName, meterType, _channels, _samples);
                MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _samples = new List<MeterSample>();
            }
            catch (Exception ex)
            {
                LogError("保存数据时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
                MessageBox.Show("数据保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 按首末样本时间生成数据文件名
        /// </summary>
        private string BuildDataFileName()
        {
            string startTime = _samples[0].Time.ToString("yyyyMMddHHmmss");
            string endTime = _samples[_samples.Count - 1].Time.ToString("yyyyMMddHHmmss");
            return startTime + "至" + endTime + ".json";
        }

        /// <summary>
        /// 设置滑动条
        /// </summary>
        private void ScrollToBottom(DataGridView dataGridView)
        {
            int rowCount = dataGridView.Rows.Count;
            if (rowCount > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = rowCount - 1;
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        public static void LogError(string message)
        {
            try
            {
                string logFolderPath = Path.GetDirectoryName(SystemParas.Paths.LogFile);
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                using (StreamWriter writer = new StreamWriter(SystemParas.Paths.LogFile, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception)
            {
                // 日志写不进去时不要影响主流程。
            }
        }
        #endregion

        #region 曲线绘制相关
        /// <summary>
        /// 决定哪几个通道上图：直流表画全部三项，三相表画 A 相电压、A 相电流与总有功。
        /// </summary>
        private void InitializeChartContext()
        {
            _chartChannelIndexes = _channels.Count == 3
                ? new[] { 0, 1, 2 }
                : new[] { 0, 6, 12 };
            _chartSeriesNames = _chartChannelIndexes.Select(index => _channels[index].Name).ToArray();
        }

        /// <summary>
        /// 初始化图表
        /// </summary>
        private void InitializeChart()
        {
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Titles.Clear();

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

            Title title = new Title(string.Join(" · ", _chartSeriesNames));
            title.Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold);
            chart1.Titles.Add(title);

            Color[] colors = { Color.Red, Color.Blue, Color.Green };
            for (int i = 0; i < _chartSeriesNames.Length; i++)
            {
                CreateSeries(_chartSeriesNames[i], colors[i % colors.Length]);
            }
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
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isWorking)
            {
                ScrollToBottom(GetActiveGrid());
            }
            RefreshChart();
        }

        /// <summary>
        /// 增加数据点
        /// </summary>
        private void AddDataPoint(string seriesName, DateTime time, double value)
        {
            chart1.Series[seriesName].Points.AddXY(time, value);
        }

        /// <summary>
        /// 刷新图表。只追加新增的数据点，不再每次重建全部曲线。
        /// </summary>
        private void RefreshChart()
        {
            if (_samples.Count < _chartedSampleCount)
            {
                // 数据被清空或整体替换，重新开始绘制
                ResetChart();
            }
            if (_samples.Count == _chartedSampleCount)
            {
                return;
            }

            for (int i = _chartedSampleCount; i < _samples.Count; i++)
            {
                MeterSample sample = _samples[i];
                for (int c = 0; c < _chartSeriesNames.Length; c++)
                {
                    AddDataPoint(_chartSeriesNames[c], sample.Time, sample.GetValue(_chartChannelIndexes[c]));
                }
            }
            _chartedSampleCount = _samples.Count;

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

            if (minY == double.MaxValue || maxY == double.MinValue)
            {
                return;
            }

            double yMargin = (maxY - minY) * 0.1;
            if (yMargin < 0.1)
            {
                yMargin = 0.1;
            }

            chart1.ChartAreas[0].AxisY.Minimum = minY - yMargin;
            chart1.ChartAreas[0].AxisY.Maximum = maxY + yMargin;

            if (chart1.Series[0].Points.Count > 0)
            {
                DateTime minTime = DateTime.MaxValue;
                DateTime maxTime = DateTime.MinValue;

                foreach (Series series in chart1.Series)
                {
                    foreach (DataPoint point in series.Points)
                    {
                        DateTime pointTime = DateTime.FromOADate(point.XValue);
                        if (pointTime < minTime)
                        {
                            minTime = pointTime;
                        }
                        if (pointTime > maxTime)
                        {
                            maxTime = pointTime;
                        }
                    }
                }

                TimeSpan xMargin = TimeSpan.FromSeconds(5);
                chart1.ChartAreas[0].AxisX.Minimum = minTime.Add(-xMargin).ToOADate();
                chart1.ChartAreas[0].AxisX.Maximum = maxTime.Add(xMargin).ToOADate();
            }
        }
        #endregion
    }
}
