using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 主窗体：本次采集的实时值、趋势曲线与记录表。
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// 两次采样的间隔，单位毫秒
        /// </summary>
        private const int SampleIntervalMilliseconds = 1000;
        /// <summary>
        /// 本次采集会话：样本集合、保存与清空都交给它。
        /// </summary>
        private RecordingSession _session;
        /// <summary>
        /// 当前表型的通道定义。
        /// </summary>
        private IReadOnlyList<ChannelDescriptor> _channels;
        /// <summary>
        /// 采集服务，窗体关闭时释放。
        /// </summary>
        private MeterSampler _sampler;
        /// <summary>
        /// 是否正在采集，用于控制按钮与状态栏。
        /// </summary>
        private bool _isWorking;
        /// <summary>
        /// 是否已放行关闭。停止完采集准备 Close 时置位，供第二次 FormClosing 放行。
        /// </summary>
        private bool _allowClose;
        /// <summary>
        /// 是否正在为关闭而停止采集。用户连点两次关闭时，第二次只取消这次关闭，不打断停止流程。
        /// </summary>
        private bool _stopBeforeClose;

        /// <summary>
        /// 初始化主窗体。
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.Icon = FormIcon.Load();
        }

        #region 窗体加载与关闭
        /// <summary>
        /// 窗体加载：绑定三个视图控件、创建采集服务并刷新按钮状态。
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            _channels = SystemParameters.EnergyMeters.Channels;
            _session = new RecordingSession(SystemParameters.Device.MeterType, _channels);
            _cardPanel.BindChannels(_channels);
            _sampleTable.Bind(_channels, () => _session.Samples);
            _sampleTable.ActiveGroupChanged += SampleTable_ActiveGroupChanged;
            _trendChart.BindSeries(_channels, GetDefaultChartIndexes());

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "电能表读数记录 v" + version;

            _sampler = new MeterSampler(SystemParameters.EnergyMeters);
            _sampler.SampleReady += OnSampleReady;
            _sampler.SampleFailed += OnSampleFailed;

            UpdateButtons();
            UpdateStatus();

            // 配置的存储目录不可用时会回落到程序目录，这里明确告知，
            // 否则用户以为数据还写在原来的盘里。
            if (!string.IsNullOrEmpty(SystemParameters.StartupWarning))
            {
                MessageBox.Show(SystemParameters.StartupWarning, "存储目录提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 关闭前停止采集并释放服务。采集线程可能正卡在 Invoke 上等界面线程，
        /// 所以先取消这次关闭、异步等线程退出，再真正关闭，避免两边互等到超时。
        /// </summary>
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_sampler == null)
            {
                return;
            }
            if (_allowClose || !_sampler.IsRunning)
            {
                _sampler.Dispose();
                return;
            }
            if (_stopBeforeClose)
            {
                // 已经在为关闭而停止采集，忽略这次重复的关闭请求。
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            _stopBeforeClose = true;
            try
            {
                await _sampler.StopAsync();
            }
            catch (Exception ex)
            {
                Logger.Log("关闭前停止采集失败，异常原因为：" + ex.Message + ex.StackTrace);
            }
            _sampler.Dispose();
            _allowClose = true;
            Close();
        }
        #endregion

        #region 工具条
        /// <summary>
        /// 开始采集。端口打不开时提示用户，并保持空闲状态。
        /// </summary>
        private void _startButton_Click(object sender, EventArgs e)
        {
            try
            {
                _sampler.Start(TimeSpan.FromMilliseconds(SampleIntervalMilliseconds));
            }
            catch (Exception ex)
            {
                Logger.Log("打开串口失败，异常原因为：" + ex.Message + ex.StackTrace);
                MessageBox.Show("设备无法连接，请检查串口是否打开或设备是否连接正常！");
                return;
            }

            _isWorking = true;
            UpdateButtons();
            UpdateStatus();
        }

        /// <summary>
        /// 停止采集。等采集线程真正退出后再落盘，避免漏掉最后几帧。
        /// </summary>
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
                // 等采集线程真正退出，保证此前的样本都已落到 _session 里。
                await _sampler.StopAsync();
            }
            catch (Exception ex)
            {
                Logger.Log("停止读数时出现异常，异常原因为：" + ex.Message + ex.StackTrace);
            }

            UpdateStatus();
            SaveCollectedData();
        }

        /// <summary>
        /// 把当前分组的数据导出为 CSV。
        /// </summary>
        private void _exportButton_Click(object sender, EventArgs e)
        {
            if (_session.Count == 0)
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
                    Logger.Log("导出表格数据失败：" + error);
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

        /// <summary>
        /// 选择数据文件，以模态方式打开回放窗体。
        /// </summary>
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
                    Logger.Log("读取数据文件失败，异常原因为：" + ex.Message + ex.StackTrace);
                    MessageBox.Show("读取失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (document == null || document.Samples.Count == 0)
                {
                    MessageBox.Show("文件里没有可显示的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (ReplayForm form = new ReplayForm(document, dialog.FileName))
                {
                    form.ShowDialog(this);
                }
            }
        }

        /// <summary>
        /// 确认后清空本次采集的数据与界面。
        /// </summary>
        private void _clearButton_Click(object sender, EventArgs e)
        {
            if (_session.Count == 0)
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

        /// <summary>
        /// 打开系统设置窗体，配置保存后需重启才生效。
        /// </summary>
        private void _settingsButton_Click(object sender, EventArgs e)
        {
            using (SystemSettingsForm form = new SystemSettingsForm())
            {
                form.ShowDialog();
            }
        }
        #endregion

        #region 采集回调
        /// <summary>
        /// 采集线程推送样本，切回界面线程追加。
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
        /// 采集中断：记录日志、恢复按钮状态并提示原因。
        /// </summary>
        private void OnSampleFailed(object sender, SamplingErrorEventArgs e)
        {
            Logger.Log("采集线程执行出现异常,异常原因为：" + e.Error.Message + e.Error.StackTrace);
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

        /// <summary>
        /// 把一帧样本同时追加到卡片区、曲线与记录表。
        /// </summary>
        /// <summary>
        /// 把一帧样本同时追加到卡片区、曲线与记录表。
        /// </summary>
        /// <param name="sample">新采样结果</param>
        private void AppendSample(MeterSample sample)
        {
            _session.Add(sample);
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

            int count = Math.Min(TrendChart.MaxSeries, indexes.Length);
            int[] selected = new int[count];
            Array.Copy(indexes, selected, count);

            _trendChart.BindSeries(_channels, selected);
            _trendChart.Reload(_session.Samples);
        }

        /// <summary>
        /// 默认上图通道：由通道定义里的 Key 决定，不再依赖通道在数组里的位置。
        /// </summary>
        private int[] GetDefaultChartIndexes()
        {
            return MeterChannels.GetDefaultChartIndexes(_channels, SystemParameters.Device.MeterType);
        }

        /// <summary>
        /// 把本次采集写入数据目录，成功后清空界面并显示文件名。
        /// </summary>
        private void SaveCollectedData()
        {
            if (_session.Count == 0)
            {
                MessageBox.Show("没有数据可保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int savedCount = _session.Count;
            string fileName;
            try
            {
                fileName = _session.Save(SystemParameters.Paths.GetDataDirectory(SystemParameters.Device.MeterType));
            }
            catch (Exception ex)
            {
                Logger.Log("保存数据时出现异常，异常原因为：", ex);
                MessageBox.Show("数据保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("数据保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetView();
            _statusLabel.Text = string.Format("已保存 {0} 条：{1}", savedCount, Path.GetFileName(fileName));
        }

        /// <summary>
        /// 清空本次采集的样本与界面显示。
        /// </summary>
        private void ResetView()
        {
            _session.Clear();
            _sampleTable.Clear();
            _trendChart.Clear();
            _cardPanel.ResetValues();
            UpdateButtons();
            UpdateStatus();
        }

        /// <summary>
        /// 按采集状态与是否有数据刷新各按钮是否可用。
        /// </summary>
        private void UpdateButtons()
        {
            bool hasData = _session.Count > 0;
            _startButton.Enabled = !_isWorking;
            _stopButton.Enabled = _isWorking;
            _exportButton.Enabled = hasData && !_isWorking;
            _replayButton.Enabled = !_isWorking;
            _clearButton.Enabled = hasData && !_isWorking;
            _settingsButton.Enabled = !_isWorking;
        }

        /// <summary>
        /// 刷新状态栏：串口、表型、采集状态与本次条数。
        /// </summary>
        private void UpdateStatus()
        {
            _statusLabel.Text = string.Format("{0} · {1} · {2} · 本次 {3} 条",
                SystemParameters.Device.PortName,
                SystemParameters.Device.MeterType.ToConfigValue(),
                _isWorking ? "采集中" : "空闲",
                _session.Count);
        }

        #endregion
    }
}
