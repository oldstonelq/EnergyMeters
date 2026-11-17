namespace MeterRecorder
{
    partial class SystemSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._mainPanel = new System.Windows.Forms.Panel();
            this._energyMetersTypeCombo = new System.Windows.Forms.ComboBox();
            this._energyMetersTypeLabel = new System.Windows.Forms.Label();
            this._logFileText = new System.Windows.Forms.TextBox();
            this._logFileLabel = new System.Windows.Forms.Label();
            this._portNameCombo = new System.Windows.Forms.ComboBox();
            this._portNameLabel = new System.Windows.Forms.Label();
            this._parityCombo = new System.Windows.Forms.ComboBox();
            this._stopBitsCombo = new System.Windows.Forms.ComboBox();
            this._dataBitsCombo = new System.Windows.Forms.ComboBox();
            this._baudRateCombo = new System.Windows.Forms.ComboBox();
            this._dataPathText = new System.Windows.Forms.TextBox();
            this._browseButton = new System.Windows.Forms.Button();
            this._dataPathLabel = new System.Windows.Forms.Label();
            this._saveButton = new System.Windows.Forms.Button();
            this._parityLabel = new System.Windows.Forms.Label();
            this._stopBitsLabel = new System.Windows.Forms.Label();
            this._dataBitsLabel = new System.Windows.Forms.Label();
            this._baudRateLabel = new System.Windows.Forms.Label();
            this._mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainPanel
            // 
            this._mainPanel.Controls.Add(this._energyMetersTypeCombo);
            this._mainPanel.Controls.Add(this._energyMetersTypeLabel);
            this._mainPanel.Controls.Add(this._logFileText);
            this._mainPanel.Controls.Add(this._logFileLabel);
            this._mainPanel.Controls.Add(this._portNameCombo);
            this._mainPanel.Controls.Add(this._portNameLabel);
            this._mainPanel.Controls.Add(this._parityCombo);
            this._mainPanel.Controls.Add(this._stopBitsCombo);
            this._mainPanel.Controls.Add(this._dataBitsCombo);
            this._mainPanel.Controls.Add(this._baudRateCombo);
            this._mainPanel.Controls.Add(this._dataPathText);
            this._mainPanel.Controls.Add(this._browseButton);
            this._mainPanel.Controls.Add(this._dataPathLabel);
            this._mainPanel.Controls.Add(this._saveButton);
            this._mainPanel.Controls.Add(this._parityLabel);
            this._mainPanel.Controls.Add(this._stopBitsLabel);
            this._mainPanel.Controls.Add(this._dataBitsLabel);
            this._mainPanel.Controls.Add(this._baudRateLabel);
            this._mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainPanel.Location = new System.Drawing.Point(0, 0);
            this._mainPanel.Name = "_mainPanel";
            this._mainPanel.Size = new System.Drawing.Size(523, 356);
            this._mainPanel.TabIndex = 0;
            // 
            // _energyMetersTypeCombo
            // 
            this._energyMetersTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._energyMetersTypeCombo.FormattingEnabled = true;
            this._energyMetersTypeCombo.Location = new System.Drawing.Point(129, 27);
            this._energyMetersTypeCombo.Name = "_energyMetersTypeCombo";
            this._energyMetersTypeCombo.Size = new System.Drawing.Size(121, 20);
            this._energyMetersTypeCombo.TabIndex = 16;
            // 
            // _energyMetersTypeLabel
            // 
            this._energyMetersTypeLabel.AutoSize = true;
            this._energyMetersTypeLabel.Location = new System.Drawing.Point(41, 27);
            this._energyMetersTypeLabel.Name = "_energyMetersTypeLabel";
            this._energyMetersTypeLabel.Size = new System.Drawing.Size(53, 12);
            this._energyMetersTypeLabel.TabIndex = 15;
            this._energyMetersTypeLabel.Text = "电表型号";
            // 
            // _logFileText
            // 
            this._logFileText.Location = new System.Drawing.Point(129, 259);
            this._logFileText.Name = "_logFileText";
            this._logFileText.Size = new System.Drawing.Size(373, 21);
            this._logFileText.TabIndex = 14;
            // 
            // _logFileLabel
            // 
            this._logFileLabel.AutoSize = true;
            this._logFileLabel.Location = new System.Drawing.Point(45, 259);
            this._logFileLabel.Name = "_logFileLabel";
            this._logFileLabel.Size = new System.Drawing.Size(53, 12);
            this._logFileLabel.TabIndex = 13;
            this._logFileLabel.Text = "日志路径";
            // 
            // _portNameCombo
            // 
            this._portNameCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._portNameCombo.FormattingEnabled = true;
            this._portNameCombo.Location = new System.Drawing.Point(129, 187);
            this._portNameCombo.Name = "_portNameCombo";
            this._portNameCombo.Size = new System.Drawing.Size(121, 20);
            this._portNameCombo.TabIndex = 12;
            // 
            // _portNameLabel
            // 
            this._portNameLabel.AutoSize = true;
            this._portNameLabel.Location = new System.Drawing.Point(43, 187);
            this._portNameLabel.Name = "_portNameLabel";
            this._portNameLabel.Size = new System.Drawing.Size(41, 12);
            this._portNameLabel.TabIndex = 11;
            this._portNameLabel.Text = "串口号";
            // 
            // _parityCombo
            // 
            this._parityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._parityCombo.FormattingEnabled = true;
            this._parityCombo.Location = new System.Drawing.Point(129, 156);
            this._parityCombo.Name = "_parityCombo";
            this._parityCombo.Size = new System.Drawing.Size(121, 20);
            this._parityCombo.TabIndex = 10;
            // 
            // _stopBitsCombo
            // 
            this._stopBitsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._stopBitsCombo.FormattingEnabled = true;
            this._stopBitsCombo.Location = new System.Drawing.Point(129, 125);
            this._stopBitsCombo.Name = "_stopBitsCombo";
            this._stopBitsCombo.Size = new System.Drawing.Size(121, 20);
            this._stopBitsCombo.TabIndex = 9;
            // 
            // _dataBitsCombo
            // 
            this._dataBitsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._dataBitsCombo.FormattingEnabled = true;
            this._dataBitsCombo.Location = new System.Drawing.Point(129, 94);
            this._dataBitsCombo.Name = "_dataBitsCombo";
            this._dataBitsCombo.Size = new System.Drawing.Size(121, 20);
            this._dataBitsCombo.TabIndex = 8;
            // 
            // _baudRateCombo
            // 
            this._baudRateCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._baudRateCombo.FormattingEnabled = true;
            this._baudRateCombo.Location = new System.Drawing.Point(129, 63);
            this._baudRateCombo.Name = "_baudRateCombo";
            this._baudRateCombo.Size = new System.Drawing.Size(121, 20);
            this._baudRateCombo.TabIndex = 7;
            // 
            // _dataPathText
            // 
            this._dataPathText.Location = new System.Drawing.Point(129, 227);
            this._dataPathText.Name = "_dataPathText";
            this._dataPathText.Size = new System.Drawing.Size(300, 21);
            this._dataPathText.TabIndex = 6;
            // 
            // _browseButton
            // 
            this._browseButton.Location = new System.Drawing.Point(433, 225);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(68, 25);
            this._browseButton.TabIndex = 17;
            this._browseButton.Text = "浏览...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.Click += new System.EventHandler(this._browseButton_Click);
            // 
            // _dataPathLabel
            // 
            this._dataPathLabel.AutoSize = true;
            this._dataPathLabel.Location = new System.Drawing.Point(41, 227);
            this._dataPathLabel.Name = "_dataPathLabel";
            this._dataPathLabel.Size = new System.Drawing.Size(77, 12);
            this._dataPathLabel.TabIndex = 5;
            this._dataPathLabel.Text = "数据存储路径";
            // 
            // _saveButton
            // 
            this._saveButton.Location = new System.Drawing.Point(427, 321);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(75, 23);
            this._saveButton.TabIndex = 4;
            this._saveButton.Text = "保存&&应用";
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this._saveButton_Click);
            // 
            // _parityLabel
            // 
            this._parityLabel.AutoSize = true;
            this._parityLabel.Location = new System.Drawing.Point(39, 156);
            this._parityLabel.Name = "_parityLabel";
            this._parityLabel.Size = new System.Drawing.Size(65, 12);
            this._parityLabel.TabIndex = 3;
            this._parityLabel.Text = "奇偶校验位";
            // 
            // _stopBitsLabel
            // 
            this._stopBitsLabel.AutoSize = true;
            this._stopBitsLabel.Location = new System.Drawing.Point(39, 125);
            this._stopBitsLabel.Name = "_stopBitsLabel";
            this._stopBitsLabel.Size = new System.Drawing.Size(41, 12);
            this._stopBitsLabel.TabIndex = 2;
            this._stopBitsLabel.Text = "停止位";
            // 
            // _dataBitsLabel
            // 
            this._dataBitsLabel.AutoSize = true;
            this._dataBitsLabel.Location = new System.Drawing.Point(39, 94);
            this._dataBitsLabel.Name = "_dataBitsLabel";
            this._dataBitsLabel.Size = new System.Drawing.Size(41, 12);
            this._dataBitsLabel.TabIndex = 1;
            this._dataBitsLabel.Text = "数据位";
            // 
            // _baudRateLabel
            // 
            this._baudRateLabel.AutoSize = true;
            this._baudRateLabel.Location = new System.Drawing.Point(39, 63);
            this._baudRateLabel.Name = "_baudRateLabel";
            this._baudRateLabel.Size = new System.Drawing.Size(41, 12);
            this._baudRateLabel.TabIndex = 0;
            this._baudRateLabel.Text = "波特率";
            // 
            // SystemSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 356);
            this.Controls.Add(this._mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SystemSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "系统设置";
            this.Load += new System.EventHandler(this.SystemSettingsForm_Load);
            this._mainPanel.ResumeLayout(false);
            this._mainPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _mainPanel;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Label _parityLabel;
        private System.Windows.Forms.Label _stopBitsLabel;
        private System.Windows.Forms.Label _dataBitsLabel;
        private System.Windows.Forms.Label _baudRateLabel;
        private System.Windows.Forms.ComboBox _parityCombo;
        private System.Windows.Forms.ComboBox _stopBitsCombo;
        private System.Windows.Forms.ComboBox _dataBitsCombo;
        private System.Windows.Forms.ComboBox _baudRateCombo;
        private System.Windows.Forms.TextBox _dataPathText;
        private System.Windows.Forms.Button _browseButton;
        private System.Windows.Forms.Label _dataPathLabel;
        private System.Windows.Forms.ComboBox _portNameCombo;
        private System.Windows.Forms.Label _portNameLabel;
        private System.Windows.Forms.TextBox _logFileText;
        private System.Windows.Forms.Label _logFileLabel;
        private System.Windows.Forms.Label _energyMetersTypeLabel;
        private System.Windows.Forms.ComboBox _energyMetersTypeCombo;
    }
}