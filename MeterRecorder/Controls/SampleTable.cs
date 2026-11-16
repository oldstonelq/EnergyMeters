using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 采集记录表。列由「当前通道组」运行时生成，切换分组只重建列，
    /// 不更换控件，也不再为每种表型准备一张表。
    /// </summary>
    public sealed partial class SampleTable : UserControl
    {
        /// <summary>
        /// 「全部」分组的名称，所有通道显示在同一张表里时用它。
        /// </summary>
        private const string AllGroups = "全部";

        /// <summary>
        /// 当前绑定的通道定义。
        /// </summary>
        private IReadOnlyList<ChannelDescriptor> _channels;
        /// <summary>
        /// 取当前样本集合的委托，切换分组或重填时调用。
        /// </summary>
        private Func<IReadOnlyList<MeterSample>> _samplesProvider;
        /// <summary>
        /// 当前选中的分组名称。
        /// </summary>
        private string _activeGroup = AllGroups;
        /// <summary>
        /// 各数据列对应的通道下标，不含固定在首列的时间。
        /// </summary>
        private int[] _columnChannelIndexes = new int[0];
        /// <summary>
        /// 重建分组按钮时置位，避免程序性勾选被当成用户切换。
        /// </summary>
        private bool _suppressGroupChange;

        /// <summary>
        /// 当前显示的分组发生变化。
        /// </summary>
        public event EventHandler ActiveGroupChanged;

        /// <summary>
        /// 初始化采集记录表。
        /// </summary>
        public SampleTable()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 绑定通道定义与数据来源。
        /// </summary>
        /// <param name="channels">通道定义</param>
        /// <param name="samplesProvider">取当前样本集合的委托，切换分组时用它重填</param>
        public void Bind(IReadOnlyList<ChannelDescriptor> channels, Func<IReadOnlyList<MeterSample>> samplesProvider)
        {
            _channels = channels;
            _samplesProvider = samplesProvider;

            // 通道多时默认只看第一个分组，否则十几列挤在一屏里根本读不清；
            // 通道少的表（直流三项）直接看「全部」。
            List<string> groups = CollectGroups();
            _activeGroup = channels.Count > 8 && groups.Count > 1 ? groups[1] : AllGroups;

            BuildGroupBar();
            RebuildColumns();
            Reload();
        }

        /// <summary>
        /// 按数据来源重填整张表。
        /// </summary>
        public void Reload()
        {
            _grid.SuspendLayout();
            try
            {
                _grid.Rows.Clear();
                if (_samplesProvider != null)
                {
                    IReadOnlyList<MeterSample> samples = _samplesProvider();
                    if (samples != null)
                    {
                        for (int i = 0; i < samples.Count; i++)
                        {
                            Append(samples[i]);
                        }
                    }
                }
            }
            finally
            {
                _grid.ResumeLayout();
            }
        }

        /// <summary>
        /// 追加一行。
        /// </summary>
        public void Append(MeterSample sample)
        {
            if (sample == null || _columnChannelIndexes.Length == 0)
            {
                return;
            }

            object[] cells = new object[_columnChannelIndexes.Length + 1];
            cells[0] = sample.Time.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < _columnChannelIndexes.Length; i++)
            {
                int channelIndex = _columnChannelIndexes[i];
                cells[i + 1] = _channels[channelIndex].Format(sample.GetValue(channelIndex));
            }
            _grid.Rows.Add(cells);
        }

        /// <summary>
        /// 清空表格。
        /// </summary>
        public void Clear()
        {
            _grid.Rows.Clear();
        }

        /// <summary>
        /// 滚动到最后一行。
        /// </summary>
        public void ScrollToLast()
        {
            int rowCount = _grid.Rows.Count;
            if (rowCount > 0)
            {
                _grid.FirstDisplayedScrollingRowIndex = rowCount - 1;
            }
        }

        /// <summary>
        /// 导出当前表格到 CSV。
        /// </summary>
        public bool ExportToCsv(string filePath, out string error)
        {
            return FileHelp.ExportCsv(_grid, filePath, out error);
        }

        /// <summary>
        /// 当前显示的行数。
        /// </summary>
        public int RowCount
        {
            get { return _grid.Rows.Count; }
        }

        /// <summary>
        /// 当前分组对应的通道下标，供曲线跟随使用。
        /// </summary>
        public int[] ActiveChannelIndexes
        {
            get { return _columnChannelIndexes; }
        }

        /// <summary>
        /// 按通道分组生成筛选按钮，同一个容器里的 RadioButton 天然互斥。
        /// </summary>
        private void BuildGroupBar()
        {
            _suppressGroupChange = true;
            try
            {
                _groupBar.Controls.Clear();
                foreach (string group in CollectGroups())
                {
                    RadioButton button = new RadioButton
                    {
                        Text = group,
                        Appearance = Appearance.Button,
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Margin = new Padding(4, 2, 4, 2),
                        Padding = new Padding(8, 2, 8, 2),
                        Checked = group == _activeGroup
                    };
                    button.CheckedChanged += GroupButton_CheckedChanged;
                    _groupBar.Controls.Add(button);
                }
            }
            finally
            {
                _suppressGroupChange = false;
            }
        }

        /// <summary>
        /// 收集全部分组名称，「全部」固定排在最前。
        /// </summary>
        private List<string> CollectGroups()
        {
            List<string> groups = new List<string> { AllGroups };
            if (_channels == null)
            {
                return groups;
            }
            foreach (ChannelDescriptor channel in _channels)
            {
                if (!groups.Contains(channel.Group))
                {
                    groups.Add(channel.Group);
                }
            }
            return groups;
        }

        /// <summary>
        /// 分组按钮被选中：重建列、重填表格，并通知外部。
        /// </summary>
        private void GroupButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressGroupChange)
            {
                return;
            }
            RadioButton button = sender as RadioButton;
            if (button == null || !button.Checked)
            {
                return;
            }

            _activeGroup = button.Text;
            RebuildColumns();
            Reload();

            EventHandler handler = ActiveGroupChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 按当前分组重建列。
        /// </summary>
        private void RebuildColumns()
        {
            _grid.SuspendLayout();
            try
            {
                _grid.Columns.Clear();
                _grid.Rows.Clear();

                if (_channels == null)
                {
                    _columnChannelIndexes = new int[0];
                    return;
                }

                _grid.Columns.Add(CreateColumn("时间", "time", 2F));

                List<int> indexes = new List<int>();
                for (int i = 0; i < _channels.Count; i++)
                {
                    if (_activeGroup != AllGroups && _channels[i].Group != _activeGroup)
                    {
                        continue;
                    }
                    indexes.Add(i);
                    _grid.Columns.Add(CreateColumn(_channels[i].DisplayName, _channels[i].Key, 1F));
                }
                _columnChannelIndexes = indexes.ToArray();
            }
            finally
            {
                _grid.ResumeLayout();
            }
        }

        /// <summary>
        /// 按统一格式创建只读文本列。
        /// </summary>
        /// <param name="headerText">列标题</param>
        /// <param name="name">列名</param>
        /// <param name="fillWeight">列宽占比</param>
        private static DataGridViewTextBoxColumn CreateColumn(string headerText, string name, float fillWeight)
        {
            return new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Name = name,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = fillWeight,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }
    }
}
