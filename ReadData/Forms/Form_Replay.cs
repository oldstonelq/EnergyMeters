using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    /// <summary>
    /// 历史数据回放。只读、模态打开，与主窗体共用记录表与曲线控件，
    /// 通道定义取自文件本身，因此不受当前程序配置的表型影响。
    /// </summary>
    public partial class Form_Replay : Form
    {
        /// <summary>
        /// 曲线上最多的通道数量
        /// </summary>
        private const int MaxSeries = 3;

        private readonly SampleDocument _document;
        private readonly string _filePath;
        private bool _suppressItemCheck;

        public Form_Replay(SampleDocument document, string filePath)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            _document = document;
            _filePath = filePath;
            InitializeComponent();
        }

        private void Form_Replay_Load(object sender, EventArgs e)
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
            int count = _document.Samples.Count;
            if (count == 0)
            {
                return string.Format("表型 {0} · 0 条", meterType);
            }

            DateTime first = _document.Samples[0].Time;
            DateTime last = _document.Samples[count - 1].Time;
            return string.Format("表型 {0} · {1} 条 · {2} ~ {3}",
                meterType, count,
                first.ToString("yyyy-MM-dd HH:mm:ss"),
                last.ToString("HH:mm:ss"));
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
        /// 默认勾选：通道少就全选，三相表取 A 相电压、A 相电流与总有功。
        /// </summary>
        private int[] GetDefaultChannelIndexes()
        {
            int count = _document.Channels.Count;
            if (count <= MaxSeries)
            {
                int[] all = new int[count];
                for (int i = 0; i < count; i++)
                {
                    all[i] = i;
                }
                return all;
            }
            if (count > 12)
            {
                return new[] { 0, 6, 12 };
            }
            return new[] { 0, 1, 2 };
        }

        private int[] GetCheckedChannelIndexes()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < _channelList.CheckedIndices.Count; i++)
            {
                indexes.Add(_channelList.CheckedIndices[i]);
            }
            return indexes.ToArray();
        }

        private void _channelList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_suppressItemCheck)
            {
                return;
            }

            // 超过上限就拒绝这次勾选，保持已有选择不变。
            if (e.NewValue == CheckState.Checked && _channelList.CheckedItems.Count >= MaxSeries)
            {
                e.NewValue = CheckState.Unchecked;
                return;
            }

            // ItemCheck 触发时勾选状态还没更新，等本次消息处理完再重建曲线。
            BeginInvoke(new Action(RebuildChart));
        }

        private void RebuildChart()
        {
            int[] indexes = GetCheckedChannelIndexes();
            _trendChart.BindSeries(_document.Channels, indexes);
            _trendChart.Reload(_document.Samples);
        }

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
                    MessageBox.Show("导出失败：" + error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("导出完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void _closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
