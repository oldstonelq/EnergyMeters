using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Chart = System.Windows.Forms.DataVisualization.Charting.Chart;
using ChartArea = System.Windows.Forms.DataVisualization.Charting.ChartArea;
using Font = System.Drawing.Font;
using Series = System.Windows.Forms.DataVisualization.Charting.Series;

namespace ReadData
{
    public partial class Form_Chart : Form
    {
        private Timer timer;
        private Random random;
        private DateTime startTime;
        private const int MaxPoints = 100;
        public Form_Chart()
        {
            InitializeComponent();
            
        }
    }
}
