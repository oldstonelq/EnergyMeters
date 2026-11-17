namespace MeterRecorder
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._startButton = new System.Windows.Forms.ToolStripButton();
            this._stopButton = new System.Windows.Forms.ToolStripButton();
            this._separator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exportButton = new System.Windows.Forms.ToolStripButton();
            this._replayButton = new System.Windows.Forms.ToolStripButton();
            this._clearButton = new System.Windows.Forms.ToolStripButton();
            this._separator2 = new System.Windows.Forms.ToolStripSeparator();
            this._settingsButton = new System.Windows.Forms.ToolStripButton();
            this._statusLabel = new System.Windows.Forms.ToolStripLabel();
            this._outerSplit = new System.Windows.Forms.SplitContainer();
            this._innerSplit = new System.Windows.Forms.SplitContainer();
            this._cardPanel = new MeterRecorder.ChannelCardPanel();
            this._trendChart = new MeterRecorder.TrendChart();
            this._sampleTable = new MeterRecorder.SampleTable();
            this._toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._outerSplit)).BeginInit();
            this._outerSplit.Panel1.SuspendLayout();
            this._outerSplit.Panel2.SuspendLayout();
            this._outerSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._innerSplit)).BeginInit();
            this._innerSplit.Panel1.SuspendLayout();
            this._innerSplit.Panel2.SuspendLayout();
            this._innerSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStrip
            // 
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._startButton,
            this._stopButton,
            this._separator1,
            this._exportButton,
            this._replayButton,
            this._clearButton,
            this._separator2,
            this._settingsButton,
            this._statusLabel});
            this._toolStrip.Location = new System.Drawing.Point(0, 0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this._toolStrip.Size = new System.Drawing.Size(1280, 31);
            this._toolStrip.TabIndex = 0;
            // 
            // _startButton
            // 
            this._startButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._startButton.Name = "_startButton";
            this._startButton.Size = new System.Drawing.Size(36, 24);
            this._startButton.Text = "开始";
            this._startButton.Click += new System.EventHandler(this._startButton_Click);
            // 
            // _stopButton
            // 
            this._stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._stopButton.Enabled = false;
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(36, 24);
            this._stopButton.Text = "停止";
            this._stopButton.Click += new System.EventHandler(this._stopButton_Click);
            // 
            // _separator1
            // 
            this._separator1.Name = "_separator1";
            this._separator1.Size = new System.Drawing.Size(6, 27);
            // 
            // _exportButton
            // 
            this._exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._exportButton.Name = "_exportButton";
            this._exportButton.Size = new System.Drawing.Size(64, 24);
            this._exportButton.Text = "导出 CSV";
            this._exportButton.Click += new System.EventHandler(this._exportButton_Click);
            // 
            // _replayButton
            // 
            this._replayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._replayButton.Name = "_replayButton";
            this._replayButton.Size = new System.Drawing.Size(64, 24);
            this._replayButton.Text = "打开回放";
            this._replayButton.Click += new System.EventHandler(this._replayButton_Click);
            // 
            // _clearButton
            // 
            this._clearButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(36, 24);
            this._clearButton.Text = "清空";
            this._clearButton.Click += new System.EventHandler(this._clearButton_Click);
            // 
            // _separator2
            // 
            this._separator2.Name = "_separator2";
            this._separator2.Size = new System.Drawing.Size(6, 27);
            // 
            // _settingsButton
            // 
            this._settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._settingsButton.Name = "_settingsButton";
            this._settingsButton.Size = new System.Drawing.Size(36, 24);
            this._settingsButton.Text = "设置";
            this._settingsButton.Click += new System.EventHandler(this._settingsButton_Click);
            // 
            // _statusLabel
            // 
            this._statusLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(44, 24);
            this._statusLabel.Text = "空闲";
            // 
            // _outerSplit
            // 
            this._outerSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outerSplit.Location = new System.Drawing.Point(0, 31);
            this._outerSplit.Name = "_outerSplit";
            this._outerSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _outerSplit.Panel1
            // 
            this._outerSplit.Panel1.Controls.Add(this._innerSplit);
            this._outerSplit.Panel1MinSize = 200;
            // 
            // _outerSplit.Panel2
            // 
            this._outerSplit.Panel2.Controls.Add(this._sampleTable);
            this._outerSplit.Panel2MinSize = 120;
            this._outerSplit.Size = new System.Drawing.Size(1280, 709);
            this._outerSplit.SplitterDistance = 420;
            this._outerSplit.SplitterWidth = 6;
            this._outerSplit.TabIndex = 1;
            // 
            // _innerSplit
            // 
            this._innerSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._innerSplit.Location = new System.Drawing.Point(0, 0);
            this._innerSplit.Name = "_innerSplit";
            // 
            // _innerSplit.Panel1
            // 
            this._innerSplit.Panel1.Controls.Add(this._cardPanel);
            this._innerSplit.Panel1MinSize = 320;
            // 
            // _innerSplit.Panel2
            // 
            this._innerSplit.Panel2.Controls.Add(this._trendChart);
            this._innerSplit.Panel2MinSize = 280;
            this._innerSplit.Size = new System.Drawing.Size(1280, 420);
            this._innerSplit.SplitterDistance = 620;
            this._innerSplit.SplitterWidth = 6;
            this._innerSplit.TabIndex = 0;
            // 
            // _cardPanel
            // 
            this._cardPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cardPanel.Location = new System.Drawing.Point(0, 0);
            this._cardPanel.Name = "_cardPanel";
            this._cardPanel.Size = new System.Drawing.Size(620, 420);
            this._cardPanel.TabIndex = 0;
            // 
            // _trendChart
            // 
            this._trendChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._trendChart.Location = new System.Drawing.Point(0, 0);
            this._trendChart.Name = "_trendChart";
            this._trendChart.Size = new System.Drawing.Size(654, 420);
            this._trendChart.TabIndex = 0;
            // 
            // _sampleTable
            // 
            this._sampleTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sampleTable.Location = new System.Drawing.Point(0, 0);
            this._sampleTable.Name = "_sampleTable";
            this._sampleTable.Size = new System.Drawing.Size(1280, 283);
            this._sampleTable.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 740);
            this.Controls.Add(this._outerSplit);
            this.Controls.Add(this._toolStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "电能表读数记录";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this._outerSplit.Panel1.ResumeLayout(false);
            this._outerSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._outerSplit)).EndInit();
            this._outerSplit.ResumeLayout(false);
            this._innerSplit.Panel1.ResumeLayout(false);
            this._innerSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._innerSplit)).EndInit();
            this._innerSplit.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton _startButton;
        private System.Windows.Forms.ToolStripButton _stopButton;
        private System.Windows.Forms.ToolStripSeparator _separator1;
        private System.Windows.Forms.ToolStripButton _exportButton;
        private System.Windows.Forms.ToolStripButton _replayButton;
        private System.Windows.Forms.ToolStripButton _clearButton;
        private System.Windows.Forms.ToolStripSeparator _separator2;
        private System.Windows.Forms.ToolStripButton _settingsButton;
        private System.Windows.Forms.ToolStripLabel _statusLabel;
        private System.Windows.Forms.SplitContainer _outerSplit;
        private System.Windows.Forms.SplitContainer _innerSplit;
        private ChannelCardPanel _cardPanel;
        private TrendChart _trendChart;
        private SampleTable _sampleTable;
    }
}
