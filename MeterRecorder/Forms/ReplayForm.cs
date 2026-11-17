using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 历史数据回放。只读、模态打开，与主窗体共用记录表与曲线控件，
    /// 通道定义取自文件本身，因此不受当前程序配置的表型影响。
    /// </summary>
    public partial class ReplayForm : Form
    {
        /// <summary>
        /// 文件内容，通道定义与样本都取自它。
        /// </summary>
        private readonly SampleDocument _document;
        /// <summary>
        /// 文件路径，用于标题与导出时的默认文件名。
        /// </summary>
        private readonly string _filePath;
        /// <summary>
        /// 批量填充勾选列表时置位，避免逐项触发重建曲线。
        /// </summary>
        private bool _suppressItemCheck;

        /// <summary>
        /// 用已加载的采集文件初始化回放窗体。
        /// </summary>
        /// <param name="document">文件内容</param>
        /// <param name="filePath">文件路径，用于显示名称与导出时的默认文件名</param>
        public ReplayForm(SampleDocument document, string filePath)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            _document = document;
            _filePath = filePath;
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载：填元信息与通道列表，并画出默认曲线。
        /// </summary>
        private void ReplayForm_Load(object sender, EventArgs e)
        {
            _fileLabel.Text = Path.GetFileName(_filePath);
            _infoLabel.Text = BuildInfoText();

            BindChannels();
            _sampleTable.Bind(_document.Channels, () => _document.Samples);
            _trendChart.BindSeries(_document.Channels, GetCheckedChannelIndexes());
            _trendChart.Reload(_document.Samples);
        }

        /// <summary>
        /// 元信息：表型、条数与时间范围。
        /// </summary>
        private string BuildInfoText()
        {
            string meterType = string.IsNullOrEmpty(_document.MeterType) ? "未标注表型" : _document.MeterType;
            // 旧文件没有表型标记，解析时按通道推断，这里如实告诉用户。
            string legacyNote = _document.IsLegacyFormat ? " · 旧格式，表型按通道推断" : string.Empty;
            int count = _document.Samples.Count;
            if (count == 0)
            {
                return string.Format("表型 {0} · 0 条{1}", meterType, legacyNote);
            }

            DateTime first = _document.Samples[0].Time;
            DateTime last = _document.Samples[count - 1].Time;
            return string.Format("表型 {0} · {1} 条 · {2} ~ {3}{4}",
                meterType, count,
                first.ToString("yyyy-MM-dd HH:mm:ss"),
                last.ToString("HH:mm:ss"),
                legacyNote);
        }

        /// <summary>
        /// 填充通道勾选列表，默认勾选几个代表性通道。
        /// </summary>
        private void BindChannels()
        {
            _suppressItemCheck = true;
            try
            {
                _channelList.Items.Clear();
                int[] defaults = GetDefaultChannelIndexes();
                for (int i = 0; i < _document.Channels.Count; i++)
                {
                    bool isChecked = Array.IndexOf(defaults, i) >= 0;
                    _channelList.Items.Add(_document.Channels[i].DisplayName, isChecked);
                }
            }
            finally
            {
                _suppressItemCheck = false;
            }
        }

        /// <summary>
        /// 默认勾选：优先用文件表型的默认曲线通道，解析不出来时退回前三个通道。
        /// </summary>
        private int[] GetDefaultChannelIndexes()
        {
            MeterType meterType;
            if (MeterTypeExtensions.TryParseConfigValue(_document.MeterType, out meterType))
            {
                int[] indexes = MeterChannels.GetDefaultChartIndexes(_document.Channels, meterType);
                if (indexes.Length > 0)
                {
                    return TrimToLimit(indexes);
                }
            }

            int count = Math.Min(TrendChart.MaxSeries, _document.Channels.Count);
            int[] fallback = new int[count];
            for (int i = 0; i < count; i++)
            {
                fallback[i] = i;
            }
            return fallback;
        }

        /// <summary>
        /// 只保留前 <see cref="TrendChart.MaxSeries"/> 个通道。
        /// </summary>
        /// <param name="indexes">按优先级排好的通道下标</param>
        private static int[] TrimToLimit(int[] indexes)
        {
            if (indexes.Length <= TrendChart.MaxSeries)
            {
                return indexes;
            }
            int[] trimmed = new int[TrendChart.MaxSeries];
            Array.Copy(indexes, trimmed, trimmed.Length);
            return trimmed;
        }

        /// <summary>
        /// 取当前勾选的通道下标。
        /// </summary>
        private int[] GetCheckedChannelIndexes()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < _channelList.CheckedIndices.Count; i++)
            {
                indexes.Add(_channelList.CheckedIndices[i]);
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// 勾选数量超过上限时拒绝本次勾选，并把重建曲线推迟到状态生效之后。
        /// </summary>
        private void _channelList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_suppressItemCheck)
            {
                return;
            }

            // 超过上限就拒绝这次勾选，保持已有选择不变。
            if (e.NewValue == CheckState.Checked && _channelList.CheckedItems.Count >= TrendChart.MaxSeries)
            {
                e.NewValue = CheckState.Unchecked;
                return;
            }

            // ItemCheck 触发时勾选状态还没更新，等本次消息处理完再重建曲线。
            BeginInvoke(new Action(RebuildChart));
        }

        /// <summary>
        /// 按勾选的通道重建曲线并重灌数据。
        /// </summary>
        private void RebuildChart()
        {
            int[] indexes = GetCheckedChannelIndexes();
            _trendChart.BindSeries(_document.Channels, indexes);
            _trendChart.Reload(_document.Samples);
        }

        /// <summary>
        /// 把表格导出为 CSV，默认文件名与数据文件同名。
        /// </summary>
        private void _exportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV 文件 (*.csv)|*.csv";
                dialog.FileName = Path.GetFileNameWithoutExtension(_filePath) + ".csv";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string error;
                if (!_sampleTable.ExportToCsv(dialog.FileName, out error))
                {
                    Logger.Log("回放窗体导出 CSV 失败：" + error);
                    MessageBox.Show("导出失败：" + error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("导出完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 关闭窗体。
        /// </summary>
        private void _closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
