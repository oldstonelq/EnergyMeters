using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MeterRecorder
{
    /// <summary>
    /// 趋势曲线。系列由通道定义决定，数据按增量追加，不重建曲线。
    /// </summary>
    public sealed partial class TrendChart : UserControl
    {
        /// <summary>
        /// 曲线配色，通道多于三种时循环取用。
        /// </summary>
        private static readonly Color[] s_seriesColors = { Color.Red, Color.Blue, Color.Green };

        /// <summary>
        /// 当前绑定的通道定义。
        /// </summary>
        private IReadOnlyList<ChannelDescriptor> _channels;
        /// <summary>
        /// 各条曲线对应的通道下标。
        /// </summary>
        private int[] _channelIndexes = new int[0];
        /// <summary>
        /// 各条曲线的名称，同时用作 Series 的键。
        /// </summary>
        private string[] _seriesNames = new string[0];

        /// <summary>
        /// 初始化趋势曲线控件。
        /// </summary>
        public TrendChart()
        {
            InitializeComponent();
            MaxPoints = 1000;
        }

        /// <summary>
        /// 单条曲线保留的最大点数。
        /// </summary>
        public int MaxPoints { get; set; }

        /// <summary>
        /// 按通道重新建立曲线。
        /// </summary>
        /// <param name="channels">通道定义</param>
        /// <param name="channelIndexes">要上图的通道下标</param>
        public void BindSeries(IReadOnlyList<ChannelDescriptor> channels, int[] channelIndexes)
        {
            _channels = channels;
            _channelIndexes = channelIndexes ?? new int[0];
            _seriesNames = new string[_channelIndexes.Length];
            for (int i = 0; i < _channelIndexes.Length; i++)
            {
                _seriesNames[i] = channels[_channelIndexes[i]].DisplayName;
            }

            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Titles.Clear();

            ChartArea chartArea = new ChartArea("Trend");
            chartArea.AxisX.Title = "时间";
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.LabelStyle.IsEndLabelVisible = false;
            chartArea.AxisY.Title = "值";
            chartArea.AxisY.LabelStyle.Format = "0.##";
            chartArea.BackColor = Color.FromArgb(250, 250, 250);
            chartArea.BorderColor = Color.FromArgb(200, 200, 200);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(224, 224, 224);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(224, 224, 224);
            // 没有数据时也给个默认范围，否则坐标轴不显示，看起来像坏掉了。
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 100;
            _chart.ChartAreas.Add(chartArea);

            Title title = new Title(string.Join(" · ", _seriesNames));
            title.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            _chart.Titles.Add(title);

            for (int i = 0; i < _seriesNames.Length; i++)
            {
                Series series = new Series(_seriesNames[i]);
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 2;
                series.Color = s_seriesColors[i % s_seriesColors.Length];
                series.ChartArea = "Trend";
                _chart.Series.Add(series);
            }
        }

        /// <summary>
        /// 追加一帧数据。
        /// </summary>
        public void Append(MeterSample sample)
        {
            if (sample == null || _seriesNames.Length == 0)
            {
                return;
            }

            AddPoints(sample);
            AdjustAxisRanges();
            _chart.Invalidate();
        }

        /// <summary>
        /// 用一批样本重绘曲线。逐点追加后再统一调整坐标，避免每次都重算范围。
        /// </summary>
        public void Reload(IReadOnlyList<MeterSample> samples)
        {
            Clear();
            if (samples == null || _seriesNames.Length == 0)
            {
                return;
            }

            for (int i = 0; i < samples.Count; i++)
            {
                AddPoints(samples[i]);
            }
            AdjustAxisRanges();
            _chart.Invalidate();
        }

        /// <summary>
        /// 清空数据点，保留曲线结构。
        /// </summary>
        public void Clear()
        {
            foreach (Series series in _chart.Series)
            {
                series.Points.Clear();
            }
            _chart.Invalidate();
        }

        /// <summary>
        /// 把一帧数据追加到各曲线，并裁剪超出上限的点。
        /// </summary>
        private void AddPoints(MeterSample sample)
        {
            for (int i = 0; i < _seriesNames.Length; i++)
            {
                Series series = _chart.Series[_seriesNames[i]];
                series.Points.AddXY(sample.Time, sample.GetValue(_channelIndexes[i]));
                while (series.Points.Count > MaxPoints)
                {
                    series.Points.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// 按当前数据点自动调整坐标范围。
        /// </summary>
        private void AdjustAxisRanges()
        {
            if (_chart.ChartAreas.Count == 0 || _chart.Series.Count == 0)
            {
                return;
            }

            double minY = double.MaxValue;
            double maxY = double.MinValue;
            DateTime minTime = DateTime.MaxValue;
            DateTime maxTime = DateTime.MinValue;

            foreach (Series series in _chart.Series)
            {
                foreach (DataPoint point in series.Points)
                {
                    minY = Math.Min(minY, point.YValues[0]);
                    maxY = Math.Max(maxY, point.YValues[0]);

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

            if (minY == double.MaxValue || maxY == double.MinValue)
            {
                return;
            }

            ChartArea chartArea = _chart.ChartAreas[0];
            double yMargin = (maxY - minY) * 0.1;
            if (yMargin < 0.1)
            {
                yMargin = 0.1;
            }
            chartArea.AxisY.Minimum = minY - yMargin;
            chartArea.AxisY.Maximum = maxY + yMargin;

            TimeSpan xMargin = TimeSpan.FromSeconds(5);
            chartArea.AxisX.Minimum = minTime.Add(-xMargin).ToOADate();
            chartArea.AxisX.Maximum = maxTime.Add(xMargin).ToOADate();
        }
    }
}
