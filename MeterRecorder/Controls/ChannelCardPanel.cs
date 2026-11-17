using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MeterRecorder
{
    /// <summary>
    /// 实时值卡片区。按通道定义分组生成卡片，通道数量变化时自动重排，
    /// 界面不需要知道当前接的是哪款表。
    /// </summary>
    public sealed partial class ChannelCardPanel : UserControl
    {
        /// <summary>
        /// 单张卡片的宽度，单位像素。
        /// </summary>
        private const int CardWidth = 112;
        /// <summary>
        /// 单张卡片的高度，单位像素。
        /// </summary>
        private const int CardHeight = 56;
        /// <summary>
        /// 数值字体的基准字号，显示时在此基础上放大。
        /// </summary>
        private const int DefaultFontSize = 9;

        private readonly Dictionary<int, Label> _valueLabels = new Dictionary<int, Label>();
        /// <summary>
        /// 当前绑定的通道定义，下标与卡片一一对应。
        /// </summary>
        private IReadOnlyList<ChannelDescriptor> _channels;

        /// <summary>
        /// 初始化实时值卡片区。
        /// </summary>
        public ChannelCardPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 按通道定义重建卡片。
        /// </summary>
        /// <param name="channels">通道定义，按 Group 连续排列，分组一换就另起一段</param>
        public void BindChannels(IReadOnlyList<ChannelDescriptor> channels)
        {
            if (channels == null)
            {
                channels = new ChannelDescriptor[0];
            }
            _channels = channels;

            _layout.SuspendLayout();
            try
            {
                _layout.Controls.Clear();
                _layout.RowStyles.Clear();
                _valueLabels.Clear();

                // 行数要一次定好：TableLayoutPanel 允许 RowCount 为 0（表示不限行数），
                // 但设计器不允许，打开这个控件就会报 RowCount 必须大于零。
                int rowCount = Math.Max(1, CountGroups(channels) * 2);
                _layout.RowCount = rowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }

                string currentGroup = null;
                FlowLayoutPanel cardRow = null;
                int rowIndex = 0;

                for (int i = 0; i < channels.Count; i++)
                {
                    ChannelDescriptor channel = channels[i];
                    if (cardRow == null || !string.Equals(channel.Group, currentGroup, StringComparison.Ordinal))
                    {
                        currentGroup = channel.Group;
                        cardRow = AddGroupRow(currentGroup, rowIndex);
                        rowIndex += 2;
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
        /// <param name="sample">新采样结果，按通道下标取值</param>
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
        /// 统计分组数。通道按分组连续排列，分组一换就另起一段。
        /// </summary>
        /// <param name="channels">通道定义</param>
        private static int CountGroups(IReadOnlyList<ChannelDescriptor> channels)
        {
            if (channels.Count == 0)
            {
                return 0;
            }
            int count = 1;
            for (int i = 1; i < channels.Count; i++)
            {
                if (!string.Equals(channels[i].Group, channels[i - 1].Group, StringComparison.Ordinal))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 在第 <paramref name="rowIndex"/> 行放分组标题、下一行放卡片容器，返回卡片容器供后续填充。
        /// </summary>
        /// <param name="groupName">分组名称，作为该段的小标题</param>
        /// <param name="rowIndex">标题所在行；卡片容器放在下一行</param>
        private FlowLayoutPanel AddGroupRow(string groupName, int rowIndex)
        {
            Label caption = new Label
            {
                Text = groupName,
                AutoSize = true,
                Margin = new Padding(6, 6, 6, 0),
                ForeColor = SystemColors.GrayText
            };
            _layout.Controls.Add(caption, 0, rowIndex);

            FlowLayoutPanel cardRow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                WrapContents = true,
                Margin = new Padding(0)
            };
            _layout.Controls.Add(cardRow, 0, rowIndex + 1);
            return cardRow;
        }

        /// <summary>
        /// 生成单张卡片。
        /// </summary>
        /// <param name="channel">通道定义，决定标题、单位与小数位</param>
        /// <param name="channelIndex">通道下标，刷新读数时用来定位卡片</param>
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
