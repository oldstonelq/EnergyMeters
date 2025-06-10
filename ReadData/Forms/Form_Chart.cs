using Microsoft.Office.Interop.Excel;
using ReadDataSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ChartArea = System.Windows.Forms.DataVisualization.Charting.ChartArea;
using Font = System.Drawing.Font;
using Series = System.Windows.Forms.DataVisualization.Charting.Series;

namespace ReadDataSoftware
{
    public partial class Form_Chart : Form
    {
        private Timer timer;
        private DateTime startTime;
        private const int MaxPoints = 1000;
        private bool isClosing = false; // 新增：标记窗体是否正在关闭
        private List<DJSF1352_DataStructure> ChartDatas = new List<DJSF1352_DataStructure>();
       
        public Form_Chart()
        {
            InitializeComponent();
           
            InitializeChart();
            InitializeTimer();
            startTime = DateTime.Now;
        }
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

            // 创建标题
            Title title = new Title("电压、电流和功率随时间变化");
            title.Font = new Font("Microsoft YaHei", 14F, FontStyle.Bold);
            chart1.Titles.Add(title);

            // 创建三个系列：电压、电流和功率
            CreateSeries("电压", Color.Red);
            CreateSeries("电流", Color.Blue);
            CreateSeries("功率", Color.Green);
        }

        private void CreateSeries(string name, Color color)
        {
            Series series = new Series(name);
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2;
            series.Color = color;
            series.ChartArea = "ElectricalParameters";
            chart1.Series.Add(series);
        }

        private void InitializeTimer()
        {
            timer = new Timer
            {
                Interval = 1000, // 1秒
                Enabled = true
            };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isClosing || chart1.IsDisposed)
                return;
            
            // 添加新数据点
            if (ChartDatas.Count > 0)
            {
                var data=ChartDatas[0];
                AddDataPoint("电压", data.Time, data.Voltage);
                AddDataPoint("电流", data.Time,data .Current);
                AddDataPoint("功率", data.Time, data .Power);
                ChartDatas.RemoveAt(0);
            }

            // 限制数据点数量
            LimitDataPoints();

            // 自动调整Y轴范围
            AdjustAxisRanges();

            // 更新图表
            chart1.Invalidate();
        }

        private void AddDataPoint(string seriesName, DateTime time, double value)
        {
            chart1.Series[seriesName].Points.AddXY(time, value);
        }

        private void LimitDataPoints()
        {
            foreach (Series series in chart1.Series)
            {
                if (series.Points.Count > MaxPoints)
                {
                    series.Points.RemoveAt(0);
                }
            }
        }

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

        private void Form_Chart_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 标记窗体正在关闭
            isClosing = true;

            // 停止计时器
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
            }
        }

        private void Form_Chart_Load(object sender, EventArgs e)
        {
            this.Text = "电气参数实时监测";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
