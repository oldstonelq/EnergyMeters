using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ReadDataSoftware
{
    /// <summary>
    /// 实时值卡片区。按通道定义分组生成卡片，通道数量变化时自动重排，
    /// 界面不需要知道当前接的是哪款表。
    /// </summary>
    public sealed partial class ChannelCardPanel : UserControl
    {
        private const int CardWidth = 112;
        private const int CardHeight = 56;
        private const int DefaultFontSize = 9;

        private readonly Dictionary<int, Label> _valueLabels = new Dictionary<int, Label>();
        private IReadOnlyList<ChannelDescriptor> _channels;

        public ChannelCardPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 按通道定义重建卡片。
        /// </summary>
        public void BindChannels(IReadOnlyList<ChannelDescriptor> channels)
        {
            _channels = channels;

            _layout.SuspendLayout();
            try
            {
                _layout.Controls.Clear();
                _layout.RowStyles.Clear();
                _layout.RowCount = 0;
                _valueLabels.Clear();

                string currentGroup = null;
                FlowLayoutPanel cardRow = null;

                for (int i = 0; i < channels.Count; i++)
                {
                    ChannelDescriptor channel = channels[i];
                    if (!string.Equals(channel.Group, currentGroup, StringComparison.Ordinal))
                    {
                        currentGroup = channel.Group;
                        cardRow = AddGroupRow(currentGroup);
                    }
                    cardRow.Controls.Add(CreateCard(channel, i));
                }
            }
            finally
            {
                _layout.ResumeLayout(true);
            }
        }

        /// <summary>
        /// 刷新卡片上的读数。
        /// </summary>
        public void UpdateValues(MeterSample sample)
        {
            if (_channels == null || sample == null)
            {
                return;
            }
            foreach (KeyValuePair<int, Label> pair in _valueLabels)
            {
                pair.Value.Text = _channels[pair.Key].Format(sample.GetValue(pair.Key));
            }
        }

        /// <summary>
        /// 把卡片恢复到未采样的状态。
        /// </summary>
        public void ResetValues()
        {
            foreach (Label label in _valueLabels.Values)
            {
                label.Text = "—";
            }
        }

        /// <summary>
        /// 追加一行「分组标题 + 卡片容器」，返回卡片容器供后续填充。
        /// </summary>
        private FlowLayoutPanel AddGroupRow(string groupName)
        {
            Label caption = new Label
            {
                Text = groupName,
                AutoSize = true,
                Margin = new Padding(6, 6, 6, 0),
                ForeColor = SystemColors.GrayText
            };
            _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.Controls.Add(caption, 0, _layout.RowCount - 1);

            FlowLayoutPanel cardRow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                WrapContents = true,
                Margin = new Padding(0)
            };
            _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.Controls.Add(cardRow, 0, _layout.RowCount - 1);
            return cardRow;
        }

        /// <summary>
        /// 生成单张卡片。
        /// </summary>
        private Control CreateCard(ChannelDescriptor channel, int channelIndex)
        {
            Panel card = new Panel
            {
                Width = CardWidth,
                Height = CardHeight,
                Margin = new Padding(6, 2, 6, 2),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label name = new Label
            {
                Text = channel.DisplayName,
                AutoSize = false,
                Location = new Point(8, 6),
                Size = new Size(CardWidth - 16, 16),
                ForeColor = SystemColors.GrayText
            };

            Label value = new Label
            {
                Text = "—",
                AutoSize = false,
                Location = new Point(8, 24),
                Size = new Size(CardWidth - 16, 26),
                Font = new Font(Font.FontFamily, DefaultFontSize + 6F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(value);
            card.Controls.Add(name);
            _valueLabels[channelIndex] = value;
            return card;
        }
    }
}
