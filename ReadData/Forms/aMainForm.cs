using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    public partial class aMainForm : Form
    {
        /// <summary>
        /// 两次采样的间隔，单位毫秒
        /// </summary>
        private const int SampleIntervalMilliseconds = 1000;
        /// <summary>
        /// 曲线上最多的通道数量
        /// </summary>
        private const int MaxChartSeries = 3;

        private readonly List<MeterSample> _samples = new List<MeterSample>();
        private IReadOnlyList<ChannelDescriptor> _channels;
        private MeterSampler _sampler;
        private bool _isWorking;

        public aMainForm()
        {
            InitializeComponent();
        }

        #region 窗体加载与关闭
        private void aMainForm_Load(object sender, EventArgs e)
        {
            _channels = SystemParas.EnergyMeters.Channels;
            _cardPanel.BindChannels(_channels);
            _sampleTable.Bind(_channels, () => _samples);
            _sampleTable.ActiveGroupChanged += SampleTable_ActiveGroupChanged;
            _trendChart.BindSeries(_channels, GetDefaultChartIndexes());

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "读数软件 Version-" + version;

            _sampler = new MeterSampler(SystemParas.EnergyMeters);
            _sampler.SampleReady += OnSampleReady;
            _sampler.SampleFailed += OnSampleFailed;

            UpdateButtons();
            UpdateStatus();
        }

        private void aMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_sampler != null)
            {
                _sampler.Dispose();
            }
        }
        #endregion

        #region 工具条
        private void _startButton_Click(object sender, EventArgs e)
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
            UpdateButtons();
            UpdateStatus();
        }

        private async void _stopButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定停止并保存数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _isWorking = false;
            UpdateButtons();

            try
            {
                // 等采集线程真正退出，保证此前的样本都已落到 _samples 里。
                await _sampler.StopAsync();
            }
            catch (Exception ex)
            {
                LogError("停止读数时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }

            UpdateStatus();
            SaveCollectedData();
        }

        private void _exportButton_Click(object sender, EventArgs e)
        {
            if (_samples.Count == 0)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV 文件 (*.csv)|*.csv";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string error;
                if (!_sampleTable.ExportToCsv(dialog.FileName, out error))
                {
                    LogError("导出表格数据失败：" + error);
                    MessageBox.Show("导出失败：" + error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult openResult = MessageBox.Show("是否要打开导出的文件？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (openResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(dialog.FileName);
                }
            }
        }

        private void _replayButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON 文件 (*.json)|*.json";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                SampleDocument document;
                try
                {
                    document = SampleRepository.Load(dialog.FileName);
                }
                catch (Exception ex)
                {
                    LogError("读取数据文件失败，异常原因为：" + ex.Message + ex.StackTrace);
                    MessageBox.Show("读取失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (document == null || document.Samples.Count == 0)
                {
                    MessageBox.Show("文件里没有可显示的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (Form_Replay form = new Form_Replay(document, dialog.FileName))
                {
                    form.ShowDialog(this);
                }
            }
        }

        private void _clearButton_Click(object sender, EventArgs e)
        {
            if (_samples.Count == 0)
            {
                MessageBox.Show("没有数据可以清除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("确定清空本次采集的数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            ResetView();
        }

        private void _settingsButton_Click(object sender, EventArgs e)
        {
            using (Form_SystemSet form = new Form_SystemSet())
            {
                form.ShowDialog();
            }
        }
        #endregion

        #region 采集回调
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
                UpdateButtons();
                UpdateStatus();
                MessageBox.Show("读数中断：" + e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }

        private void AppendSample(MeterSample sample)
        {
            _samples.Add(sample);
            _cardPanel.UpdateValues(sample);
            _trendChart.Append(sample);
            _sampleTable.Append(sample);
            _sampleTable.ScrollToLast();
            UpdateButtons();
            UpdateStatus();
        }
        #endregion

        #region 视图与存储
        /// <summary>
        /// 记录表切换分组后，曲线跟着改为该组的通道。
        /// </summary>
        private void SampleTable_ActiveGroupChanged(object sender, EventArgs e)
        {
            int[] indexes = _sampleTable.ActiveChannelIndexes;
            if (indexes == null || indexes.Length == 0)
            {
                return;
            }

            int count = Math.Min(MaxChartSeries, indexes.Length);
            int[] selected = new int[count];
            Array.Copy(indexes, selected, count);

            _trendChart.BindSeries(_channels, selected);
            _trendChart.Reload(_samples);
        }

        /// <summary>
        /// 默认上图通道：直流表画全部三项，三相表画 A 相电压、A 相电流与总有功。
        /// </summary>
        private int[] GetDefaultChartIndexes()
        {
            if (_channels.Count == 3)
            {
                return new[] { 0, 1, 2 };
            }
            return new[] { 0, 6, 12 };
        }

        private void SaveCollectedData()
        {
            if (_samples.Count == 0)
            {
                MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int savedCount = _samples.Count;
            string fileName;
            try
            {
                MeterType meterType = SystemParas.Device.MeterType;
                fileName = Path.Combine(SystemParas.Paths.GetDataDirectory(meterType), BuildDataFileName());
                SampleRepository.Save(fileName, meterType, _channels, _samples);
            }
            catch (Exception ex)
            {
                LogError("保存数据时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
                MessageBox.Show("数据保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetView();
            _statusLabel.Text = string.Format("已保存 {0} 条：{1}", savedCount, Path.GetFileName(fileName));
        }

        private string BuildDataFileName()
        {
            string startTime = _samples[0].Time.ToString("yyyyMMddHHmmss");
            string endTime = _samples[_samples.Count - 1].Time.ToString("yyyyMMddHHmmss");
            return startTime + "至" + endTime + ".json";
        }

        /// <summary>
        /// 清空本次采集的样本与界面显示。
        /// </summary>
        private void ResetView()
        {
            _samples.Clear();
            _sampleTable.Clear();
            _trendChart.Clear();
            _cardPanel.ResetValues();
            UpdateButtons();
            UpdateStatus();
        }

        private void UpdateButtons()
        {
            bool hasData = _samples.Count > 0;
            _startButton.Enabled = !_isWorking;
            _stopButton.Enabled = _isWorking;
            _exportButton.Enabled = hasData && !_isWorking;
            _replayButton.Enabled = !_isWorking;
            _clearButton.Enabled = hasData && !_isWorking;
            _settingsButton.Enabled = !_isWorking;
        }

        private void UpdateStatus()
        {
            _statusLabel.Text = string.Format("{0} · {1} · {2} · 本次 {3} 条",
                SystemParas.Device.PortName,
                SystemParas.Device.MeterType.ToConfigValue(),
                _isWorking ? "采集中" : "空闲",
                _samples.Count);
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
    }
}
