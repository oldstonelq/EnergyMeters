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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DGV1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_End = new System.Windows.Forms.Button();
            this.btn_Start = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.系统设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Column_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Voltage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Current = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_Power = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_ExportCsv = new System.Windows.Forms.Button();
            this.曲线数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.16837F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.83164F));
            this.tableLayoutPanel1.Controls.Add(this.DGV1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31.52364F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 68.47636F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1081, 571);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.DGV1.Location = new System.Drawing.Point(3, 3);
            this.DGV1.Name = "DGV1";
            this.DGV1.RowHeadersVisible = false;
            this.tableLayoutPanel1.SetRowSpan(this.DGV1, 2);
            this.DGV1.RowTemplate.Height = 23;
            this.DGV1.Size = new System.Drawing.Size(838, 544);
            this.DGV1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btn_End);
            this.panel1.Controls.Add(this.btn_Start);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(847, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(231, 167);
            this.panel1.TabIndex = 1;
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
            this.panel2.Controls.Add(this.btn_ExportCsv);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(847, 176);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(231, 371);
            this.panel2.TabIndex = 2;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.系统设置ToolStripMenuItem,
            this.曲线数据ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 70);
            // 
            // 系统设置ToolStripMenuItem
            // 
            this.系统设置ToolStripMenuItem.Name = "系统设置ToolStripMenuItem";
            this.系统设置ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.系统设置ToolStripMenuItem.Text = "系统设置";
            this.系统设置ToolStripMenuItem.Click += new System.EventHandler(this.系统设置ToolStripMenuItem_Click);
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
            this.Column_Voltage.HeaderText = "电压";
            this.Column_Voltage.Name = "Column_Voltage";
            this.Column_Voltage.ReadOnly = true;
            this.Column_Voltage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_Current
            // 
            this.Column_Current.HeaderText = "电流";
            this.Column_Current.Name = "Column_Current";
            this.Column_Current.ReadOnly = true;
            this.Column_Current.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column_Power
            // 
            this.Column_Power.HeaderText = "功率";
            this.Column_Power.Name = "Column_Power";
            this.Column_Power.ReadOnly = true;
            this.Column_Power.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // btn_ExportCsv
            // 
            this.btn_ExportCsv.Location = new System.Drawing.Point(34, 17);
            this.btn_ExportCsv.Name = "btn_ExportCsv";
            this.btn_ExportCsv.Size = new System.Drawing.Size(170, 25);
            this.btn_ExportCsv.TabIndex = 0;
            this.btn_ExportCsv.Text = "导出表格数据";
            this.btn_ExportCsv.UseVisualStyleBackColor = true;
            this.btn_ExportCsv.Click += new System.EventHandler(this.btn_ExportCsv_Click);
            // 
            // 曲线数据ToolStripMenuItem
            // 
            this.曲线数据ToolStripMenuItem.Name = "曲线数据ToolStripMenuItem";
            this.曲线数据ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.曲线数据ToolStripMenuItem.Text = "曲线数据";
            this.曲线数据ToolStripMenuItem.Visible = false;
            this.曲线数据ToolStripMenuItem.Click += new System.EventHandler(this.曲线数据ToolStripMenuItem_Click);
            // 
            // aMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1081, 571);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "aMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ReadDataSoftware";
            this.Load += new System.EventHandler(this.aMainForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView DGV1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_End;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 系统设置ToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Voltage;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Current;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Power;
        private System.Windows.Forms.Button btn_ExportCsv;
        private System.Windows.Forms.ToolStripMenuItem 曲线数据ToolStripMenuItem;
    }
}

