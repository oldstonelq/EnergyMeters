namespace ReadDataSoftware
{
    partial class ReplayForm
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
            this._metaPanel = new System.Windows.Forms.Panel();
            this._fileLabel = new System.Windows.Forms.Label();
            this._infoLabel = new System.Windows.Forms.Label();
            this._footerPanel = new System.Windows.Forms.Panel();
            this._exportButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._outerSplit = new System.Windows.Forms.SplitContainer();
            this._innerSplit = new System.Windows.Forms.SplitContainer();
            this._channelList = new System.Windows.Forms.CheckedListBox();
            this._channelHeader = new System.Windows.Forms.Label();
            this._trendChart = new ReadDataSoftware.TrendChart();
            this._sampleTable = new ReadDataSoftware.SampleTable();
            this._metaPanel.SuspendLayout();
            this._footerPanel.SuspendLayout();
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
            // _metaPanel
            // 
            this._metaPanel.Controls.Add(this._infoLabel);
            this._metaPanel.Controls.Add(this._fileLabel);
            this._metaPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._metaPanel.Location = new System.Drawing.Point(0, 0);
            this._metaPanel.Name = "_metaPanel";
            this._metaPanel.Size = new System.Drawing.Size(1100, 56);
            this._metaPanel.TabIndex = 0;
            // 
            // _fileLabel
            // 
            this._fileLabel.AutoSize = true;
            this._fileLabel.Location = new System.Drawing.Point(12, 10);
            this._fileLabel.Name = "_fileLabel";
            this._fileLabel.Size = new System.Drawing.Size(60, 12);
            this._fileLabel.TabIndex = 0;
            this._fileLabel.Text = "未加载文件";
            // 
            // _infoLabel
            // 
            this._infoLabel.AutoSize = true;
            this._infoLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this._infoLabel.Location = new System.Drawing.Point(12, 32);
            this._infoLabel.Name = "_infoLabel";
            this._infoLabel.Size = new System.Drawing.Size(0, 12);
            this._infoLabel.TabIndex = 1;
            // 
            // _footerPanel
            // 
            this._footerPanel.Controls.Add(this._closeButton);
            this._footerPanel.Controls.Add(this._exportButton);
            this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._footerPanel.Location = new System.Drawing.Point(0, 632);
            this._footerPanel.Name = "_footerPanel";
            this._footerPanel.Size = new System.Drawing.Size(1100, 48);
            this._footerPanel.TabIndex = 2;
            // 
            // _exportButton
            // 
            this._exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._exportButton.Location = new System.Drawing.Point(872, 8);
            this._exportButton.Name = "_exportButton";
            this._exportButton.Size = new System.Drawing.Size(100, 32);
            this._exportButton.TabIndex = 0;
            this._exportButton.Text = "导出 CSV";
            this._exportButton.UseVisualStyleBackColor = true;
            this._exportButton.Click += new System.EventHandler(this._exportButton_Click);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.Location = new System.Drawing.Point(984, 8);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(100, 32);
            this._closeButton.TabIndex = 1;
            this._closeButton.Text = "关闭";
            this._closeButton.UseVisualStyleBackColor = true;
            this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
            // 
            // _outerSplit
            // 
            this._outerSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outerSplit.Location = new System.Drawing.Point(0, 56);
            this._outerSplit.Name = "_outerSplit";
            this._outerSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _outerSplit.Panel1
            // 
            this._outerSplit.Panel1.Controls.Add(this._innerSplit);
            this._outerSplit.Panel1MinSize = 220;
            // 
            // _outerSplit.Panel2
            // 
            this._outerSplit.Panel2.Controls.Add(this._sampleTable);
            this._outerSplit.Panel2MinSize = 120;
            this._outerSplit.Size = new System.Drawing.Size(1100, 576);
            this._outerSplit.SplitterDistance = 340;
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
            this._innerSplit.Panel1.Controls.Add(this._channelList);
            this._innerSplit.Panel1.Controls.Add(this._channelHeader);
            this._innerSplit.Panel1MinSize = 160;
            // 
            // _innerSplit.Panel2
            // 
            this._innerSplit.Panel2.Controls.Add(this._trendChart);
            this._innerSplit.Panel2MinSize = 300;
            this._innerSplit.Size = new System.Drawing.Size(1100, 340);
            this._innerSplit.SplitterDistance = 240;
            this._innerSplit.SplitterWidth = 6;
            this._innerSplit.TabIndex = 0;
            // 
            // _channelList
            // 
            this._channelList.CheckOnClick = true;
            this._channelList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._channelList.FormattingEnabled = true;
            this._channelList.IntegralHeight = false;
            this._channelList.Location = new System.Drawing.Point(0, 26);
            this._channelList.Name = "_channelList";
            this._channelList.Size = new System.Drawing.Size(240, 314);
            this._channelList.TabIndex = 1;
            this._channelList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._channelList_ItemCheck);
            // 
            // _channelHeader
            // 
            this._channelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this._channelHeader.ForeColor = System.Drawing.SystemColors.GrayText;
            this._channelHeader.Location = new System.Drawing.Point(0, 0);
            this._channelHeader.Name = "_channelHeader";
            this._channelHeader.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this._channelHeader.Size = new System.Drawing.Size(240, 26);
            this._channelHeader.TabIndex = 0;
            this._channelHeader.Text = "显示通道（最多 3 条）";
            // 
            // _trendChart
            // 
            this._trendChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._trendChart.Location = new System.Drawing.Point(0, 0);
            this._trendChart.Name = "_trendChart";
            this._trendChart.Size = new System.Drawing.Size(854, 340);
            this._trendChart.TabIndex = 0;
            // 
            // _sampleTable
            // 
            this._sampleTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sampleTable.Location = new System.Drawing.Point(0, 0);
            this._sampleTable.Name = "_sampleTable";
            this._sampleTable.Size = new System.Drawing.Size(1100, 230);
            this._sampleTable.TabIndex = 0;
            // 
            // ReplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this._outerSplit);
            this.Controls.Add(this._footerPanel);
            this.Controls.Add(this._metaPanel);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "ReplayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "历史数据回放";
            this.Load += new System.EventHandler(this.ReplayForm_Load);
            this._metaPanel.ResumeLayout(false);
            this._metaPanel.PerformLayout();
            this._footerPanel.ResumeLayout(false);
            this._outerSplit.Panel1.ResumeLayout(false);
            this._outerSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._outerSplit)).EndInit();
            this._outerSplit.ResumeLayout(false);
            this._innerSplit.Panel1.ResumeLayout(false);
            this._innerSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._innerSplit)).EndInit();
            this._innerSplit.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel _metaPanel;
        private System.Windows.Forms.Label _fileLabel;
        private System.Windows.Forms.Label _infoLabel;
        private System.Windows.Forms.Panel _footerPanel;
        private System.Windows.Forms.Button _exportButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.SplitContainer _outerSplit;
        private System.Windows.Forms.SplitContainer _innerSplit;
        private System.Windows.Forms.CheckedListBox _channelList;
        private System.Windows.Forms.Label _channelHeader;
        private TrendChart _trendChart;
        private SampleTable _sampleTable;
    }
}
