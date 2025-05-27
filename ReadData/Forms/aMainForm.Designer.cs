namespace ReadDataSoftware
{
    partial class aMainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(aMainForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_End = new System.Windows.Forms.Button();
            this.btn_Start = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_Set = new System.Windows.Forms.Button();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.btn_ReadData = new System.Windows.Forms.Button();
            this.btn_ExportCsv = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel4 = new System.Windows.Forms.Panel();
            this.DGV2 = new System.Windows.Forms.DataGridView();
            this.Column_RealTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_TotalActivePower = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseVoltageA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseVoltageB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseVoltageC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_LineVoltageUAB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_LineVoltageUBC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_LineVoltageUAC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_CurrentA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_CurrentB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_CurrentC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_ActivePowerOfPhaseA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_ActivePowerOfPhaseB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_ActivePowerOfPhaseC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseReactivePowerA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseReactivePowerB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_PhaseReactivePowerC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_TotalReactivePower = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV1 = new System.Windows.Forms.DataGridView();
            this.Column_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Voltage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Current = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Power = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54.87603F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.12397F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 235F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31.52364F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 68.47636F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1446, 635);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btn_End);
            this.panel1.Controls.Add(this.btn_Start);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1213, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(230, 194);
            this.panel1.TabIndex = 4;
            // 
            // btn_End
            // 
            this.btn_End.Enabled = false;
            this.btn_End.Location = new System.Drawing.Point(34, 90);
            this.btn_End.Name = "btn_End";
            this.btn_End.Size = new System.Drawing.Size(170, 70);
            this.btn_End.TabIndex = 1;
            this.btn_End.Text = "停止";
            this.btn_End.UseVisualStyleBackColor = true;
            this.btn_End.Click += new System.EventHandler(this.btn_End_Click);
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(34, 9);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(170, 70);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "开始";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btn_Set);
            this.panel2.Controls.Add(this.btn_Clear);
            this.panel2.Controls.Add(this.btn_ReadData);
            this.panel2.Controls.Add(this.btn_ExportCsv);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(1213, 203);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(230, 429);
            this.panel2.TabIndex = 2;
            // 
            // btn_Set
            // 
            this.btn_Set.Location = new System.Drawing.Point(34, 146);
            this.btn_Set.Name = "btn_Set";
            this.btn_Set.Size = new System.Drawing.Size(170, 23);
            this.btn_Set.TabIndex = 3;
            this.btn_Set.Text = "系统设置";
            this.btn_Set.UseVisualStyleBackColor = true;
            this.btn_Set.Click += new System.EventHandler(this.btn_Set_Click);
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(34, 105);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(170, 23);
            this.btn_Clear.TabIndex = 2;
            this.btn_Clear.Text = "清除当前表格数据";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btn_ReadData
            // 
            this.btn_ReadData.Location = new System.Drawing.Point(34, 61);
            this.btn_ReadData.Name = "btn_ReadData";
            this.btn_ReadData.Size = new System.Drawing.Size(170, 23);
            this.btn_ReadData.TabIndex = 1;
            this.btn_ReadData.Text = "从文件读取数据显示到表格";
            this.btn_ReadData.UseVisualStyleBackColor = true;
            this.btn_ReadData.Click += new System.EventHandler(this.btn_ReadData_Click);
            // 
            // btn_ExportCsv
            // 
            this.btn_ExportCsv.Location = new System.Drawing.Point(34, 17);
            this.btn_ExportCsv.Name = "btn_ExportCsv";
            this.btn_ExportCsv.Size = new System.Drawing.Size(170, 25);
            this.btn_ExportCsv.TabIndex = 0;
            this.btn_ExportCsv.Text = "导出当前表格数据到csv";
            this.btn_ExportCsv.UseVisualStyleBackColor = true;
            this.btn_ExportCsv.Click += new System.EventHandler(this.btn_ExportCsv_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.chart1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(667, 3);
            this.panel3.Name = "panel3";
            this.tableLayoutPanel1.SetRowSpan(this.panel3, 2);
            this.panel3.Size = new System.Drawing.Size(540, 629);
            this.panel3.TabIndex = 5;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(538, 627);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.DGV2);
            this.panel4.Controls.Add(this.DGV1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.tableLayoutPanel1.SetRowSpan(this.panel4, 2);
            this.panel4.Size = new System.Drawing.Size(658, 629);
            this.panel4.TabIndex = 6;
            // 
            // DGV2
            // 
            this.DGV2.AllowUserToAddRows = false;
            this.DGV2.AllowUserToDeleteRows = false;
            this.DGV2.AllowUserToResizeColumns = false;
            this.DGV2.AllowUserToResizeRows = false;
            this.DGV2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV2.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.DGV2.BackgroundColor = System.Drawing.Color.Gainsboro;
            this.DGV2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_RealTime,
            this.Column_TotalActivePower,
            this.Column_PhaseVoltageA,
            this.Column_PhaseVoltageB,
            this.Column_PhaseVoltageC,
            this.Column_LineVoltageUAB,
            this.Column_LineVoltageUBC,
            this.Column_LineVoltageUAC,
            this.Column_CurrentA,
            this.Column_CurrentB,
            this.Column_CurrentC,
            this.Column_ActivePowerOfPhaseA,
            this.Column_ActivePowerOfPhaseB,
            this.Column_ActivePowerOfPhaseC,
            this.Column_PhaseReactivePowerA,
            this.Column_PhaseReactivePowerB,
            this.Column_PhaseReactivePowerC,
            this.Column_TotalReactivePower});
            this.DGV2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV2.Location = new System.Drawing.Point(0, 0);
            this.DGV2.Name = "DGV2";
            this.DGV2.RowHeadersVisible = false;
            this.DGV2.RowTemplate.Height = 23;
            this.DGV2.Size = new System.Drawing.Size(658, 629);
            this.DGV2.TabIndex = 8;
            // 
            // Column_RealTime
            // 
            this.Column_RealTime.HeaderText = "时间";
            this.Column_RealTime.Name = "Column_RealTime";
            this.Column_RealTime.ReadOnly = true;
            this.Column_RealTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_TotalActivePower
            // 
            this.Column_TotalActivePower.HeaderText = "总有功功率";
            this.Column_TotalActivePower.Name = "Column_TotalActivePower";
            this.Column_TotalActivePower.ReadOnly = true;
            this.Column_TotalActivePower.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseVoltageA
            // 
            this.Column_PhaseVoltageA.HeaderText = "相电压UA";
            this.Column_PhaseVoltageA.Name = "Column_PhaseVoltageA";
            this.Column_PhaseVoltageA.ReadOnly = true;
            this.Column_PhaseVoltageA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseVoltageB
            // 
            this.Column_PhaseVoltageB.HeaderText = "相电压UB";
            this.Column_PhaseVoltageB.Name = "Column_PhaseVoltageB";
            this.Column_PhaseVoltageB.ReadOnly = true;
            this.Column_PhaseVoltageB.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseVoltageC
            // 
            this.Column_PhaseVoltageC.HeaderText = "相电压UC";
            this.Column_PhaseVoltageC.Name = "Column_PhaseVoltageC";
            this.Column_PhaseVoltageC.ReadOnly = true;
            this.Column_PhaseVoltageC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_LineVoltageUAB
            // 
            this.Column_LineVoltageUAB.HeaderText = "线电压UAB";
            this.Column_LineVoltageUAB.Name = "Column_LineVoltageUAB";
            this.Column_LineVoltageUAB.ReadOnly = true;
            this.Column_LineVoltageUAB.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_LineVoltageUBC
            // 
            this.Column_LineVoltageUBC.HeaderText = "线电压UBC";
            this.Column_LineVoltageUBC.Name = "Column_LineVoltageUBC";
            this.Column_LineVoltageUBC.ReadOnly = true;
            this.Column_LineVoltageUBC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_LineVoltageUAC
            // 
            this.Column_LineVoltageUAC.HeaderText = "线电压UAC";
            this.Column_LineVoltageUAC.Name = "Column_LineVoltageUAC";
            this.Column_LineVoltageUAC.ReadOnly = true;
            this.Column_LineVoltageUAC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_CurrentA
            // 
            this.Column_CurrentA.HeaderText = "电流IA";
            this.Column_CurrentA.Name = "Column_CurrentA";
            this.Column_CurrentA.ReadOnly = true;
            this.Column_CurrentA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_CurrentB
            // 
            this.Column_CurrentB.HeaderText = "电流IB";
            this.Column_CurrentB.Name = "Column_CurrentB";
            this.Column_CurrentB.ReadOnly = true;
            this.Column_CurrentB.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_CurrentC
            // 
            this.Column_CurrentC.HeaderText = "电流IC";
            this.Column_CurrentC.Name = "Column_CurrentC";
            this.Column_CurrentC.ReadOnly = true;
            this.Column_CurrentC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_ActivePowerOfPhaseA
            // 
            this.Column_ActivePowerOfPhaseA.HeaderText = "A相有功功率";
            this.Column_ActivePowerOfPhaseA.Name = "Column_ActivePowerOfPhaseA";
            this.Column_ActivePowerOfPhaseA.ReadOnly = true;
            this.Column_ActivePowerOfPhaseA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_ActivePowerOfPhaseB
            // 
            this.Column_ActivePowerOfPhaseB.HeaderText = "B相有功功率";
            this.Column_ActivePowerOfPhaseB.Name = "Column_ActivePowerOfPhaseB";
            this.Column_ActivePowerOfPhaseB.ReadOnly = true;
            this.Column_ActivePowerOfPhaseB.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_ActivePowerOfPhaseC
            // 
            this.Column_ActivePowerOfPhaseC.HeaderText = "C相有功功率";
            this.Column_ActivePowerOfPhaseC.Name = "Column_ActivePowerOfPhaseC";
            this.Column_ActivePowerOfPhaseC.ReadOnly = true;
            this.Column_ActivePowerOfPhaseC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseReactivePowerA
            // 
            this.Column_PhaseReactivePowerA.HeaderText = "A相无功功率";
            this.Column_PhaseReactivePowerA.Name = "Column_PhaseReactivePowerA";
            this.Column_PhaseReactivePowerA.ReadOnly = true;
            this.Column_PhaseReactivePowerA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseReactivePowerB
            // 
            this.Column_PhaseReactivePowerB.HeaderText = "B相无功功率";
            this.Column_PhaseReactivePowerB.Name = "Column_PhaseReactivePowerB";
            this.Column_PhaseReactivePowerB.ReadOnly = true;
            this.Column_PhaseReactivePowerB.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_PhaseReactivePowerC
            // 
            this.Column_PhaseReactivePowerC.HeaderText = "C相无功功率";
            this.Column_PhaseReactivePowerC.Name = "Column_PhaseReactivePowerC";
            this.Column_PhaseReactivePowerC.ReadOnly = true;
            this.Column_PhaseReactivePowerC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_TotalReactivePower
            // 
            this.Column_TotalReactivePower.HeaderText = "总无功功率";
            this.Column_TotalReactivePower.Name = "Column_TotalReactivePower";
            this.Column_TotalReactivePower.ReadOnly = true;
            this.Column_TotalReactivePower.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV1
            // 
            this.DGV1.AllowUserToAddRows = false;
            this.DGV1.AllowUserToDeleteRows = false;
            this.DGV1.AllowUserToResizeColumns = false;
            this.DGV1.AllowUserToResizeRows = false;
            this.DGV1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGV1.BackgroundColor = System.Drawing.Color.Gainsboro;
            this.DGV1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_Time,
            this.Column_Voltage,
            this.Column_Current,
            this.Column_Power});
            this.DGV1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV1.Location = new System.Drawing.Point(0, 0);
            this.DGV1.Name = "DGV1";
            this.DGV1.RowHeadersVisible = false;
            this.DGV1.RowTemplate.Height = 23;
            this.DGV1.Size = new System.Drawing.Size(658, 629);
            this.DGV1.TabIndex = 1;
            // 
            // Column_Time
            // 
            this.Column_Time.HeaderText = "时间";
            this.Column_Time.Name = "Column_Time";
            this.Column_Time.ReadOnly = true;
            this.Column_Time.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_Voltage
            // 
            this.Column_Voltage.HeaderText = "电压(v)";
            this.Column_Voltage.Name = "Column_Voltage";
            this.Column_Voltage.ReadOnly = true;
            this.Column_Voltage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_Current
            // 
            this.Column_Current.HeaderText = "电流(A)";
            this.Column_Current.Name = "Column_Current";
            this.Column_Current.ReadOnly = true;
            this.Column_Current.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_Power
            // 
            this.Column_Power.HeaderText = "功率(w)";
            this.Column_Power.Name = "Column_Power";
            this.Column_Power.ReadOnly = true;
            this.Column_Power.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // aMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1446, 635);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "aMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ReadDataSoftware";
            this.Load += new System.EventHandler(this.aMainForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DGV2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_End;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_Set;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.Button btn_ReadData;
        private System.Windows.Forms.Button btn_ExportCsv;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataGridView DGV2;
        private System.Windows.Forms.DataGridView DGV1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Voltage;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Current;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Power;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_RealTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_TotalActivePower;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseVoltageA;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseVoltageB;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseVoltageC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_LineVoltageUAB;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_LineVoltageUBC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_LineVoltageUAC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_CurrentA;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_CurrentB;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_CurrentC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_ActivePowerOfPhaseA;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_ActivePowerOfPhaseB;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_ActivePowerOfPhaseC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseReactivePowerA;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseReactivePowerB;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_PhaseReactivePowerC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_TotalReactivePower;
    }
}

