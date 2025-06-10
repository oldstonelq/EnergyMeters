namespace ReadDataSoftware
{
    partial class Form_SystemSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_SystemSet));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmb_EnergyMetersType = new System.Windows.Forms.ComboBox();
            this.lab_EnergyMetersType = new System.Windows.Forms.Label();
            this.txt_LogFile = new System.Windows.Forms.TextBox();
            this.lab_LogFile = new System.Windows.Forms.Label();
            this.cmb_PortName = new System.Windows.Forms.ComboBox();
            this.lab_COM = new System.Windows.Forms.Label();
            this.cmb_Parity = new System.Windows.Forms.ComboBox();
            this.cmb_StopBits = new System.Windows.Forms.ComboBox();
            this.cmb_DataBits = new System.Windows.Forms.ComboBox();
            this.cmb_BaudRate = new System.Windows.Forms.ComboBox();
            this.txt_DataFile = new System.Windows.Forms.TextBox();
            this.lab_DataFile = new System.Windows.Forms.Label();
            this.btn_Save = new System.Windows.Forms.Button();
            this.lab_Parity = new System.Windows.Forms.Label();
            this.lab_StopBits = new System.Windows.Forms.Label();
            this.lab_DataBits = new System.Windows.Forms.Label();
            this.lab_BaudRate = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmb_EnergyMetersType);
            this.panel1.Controls.Add(this.lab_EnergyMetersType);
            this.panel1.Controls.Add(this.txt_LogFile);
            this.panel1.Controls.Add(this.lab_LogFile);
            this.panel1.Controls.Add(this.cmb_PortName);
            this.panel1.Controls.Add(this.lab_COM);
            this.panel1.Controls.Add(this.cmb_Parity);
            this.panel1.Controls.Add(this.cmb_StopBits);
            this.panel1.Controls.Add(this.cmb_DataBits);
            this.panel1.Controls.Add(this.cmb_BaudRate);
            this.panel1.Controls.Add(this.txt_DataFile);
            this.panel1.Controls.Add(this.lab_DataFile);
            this.panel1.Controls.Add(this.btn_Save);
            this.panel1.Controls.Add(this.lab_Parity);
            this.panel1.Controls.Add(this.lab_StopBits);
            this.panel1.Controls.Add(this.lab_DataBits);
            this.panel1.Controls.Add(this.lab_BaudRate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(523, 356);
            this.panel1.TabIndex = 0;
            // 
            // cmb_EnergyMetersType
            // 
            this.cmb_EnergyMetersType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_EnergyMetersType.FormattingEnabled = true;
            this.cmb_EnergyMetersType.Location = new System.Drawing.Point(129, 27);
            this.cmb_EnergyMetersType.Name = "cmb_EnergyMetersType";
            this.cmb_EnergyMetersType.Size = new System.Drawing.Size(121, 20);
            this.cmb_EnergyMetersType.TabIndex = 16;
            // 
            // lab_EnergyMetersType
            // 
            this.lab_EnergyMetersType.AutoSize = true;
            this.lab_EnergyMetersType.Location = new System.Drawing.Point(41, 27);
            this.lab_EnergyMetersType.Name = "lab_EnergyMetersType";
            this.lab_EnergyMetersType.Size = new System.Drawing.Size(53, 12);
            this.lab_EnergyMetersType.TabIndex = 15;
            this.lab_EnergyMetersType.Text = "电表型号";
            // 
            // txt_LogFile
            // 
            this.txt_LogFile.Location = new System.Drawing.Point(129, 259);
            this.txt_LogFile.Name = "txt_LogFile";
            this.txt_LogFile.Size = new System.Drawing.Size(373, 21);
            this.txt_LogFile.TabIndex = 14;
            // 
            // lab_LogFile
            // 
            this.lab_LogFile.AutoSize = true;
            this.lab_LogFile.Location = new System.Drawing.Point(45, 259);
            this.lab_LogFile.Name = "lab_LogFile";
            this.lab_LogFile.Size = new System.Drawing.Size(53, 12);
            this.lab_LogFile.TabIndex = 13;
            this.lab_LogFile.Text = "日志路径";
            // 
            // cmb_PortName
            // 
            this.cmb_PortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_PortName.FormattingEnabled = true;
            this.cmb_PortName.Location = new System.Drawing.Point(129, 187);
            this.cmb_PortName.Name = "cmb_PortName";
            this.cmb_PortName.Size = new System.Drawing.Size(121, 20);
            this.cmb_PortName.TabIndex = 12;
            // 
            // lab_COM
            // 
            this.lab_COM.AutoSize = true;
            this.lab_COM.Location = new System.Drawing.Point(43, 187);
            this.lab_COM.Name = "lab_COM";
            this.lab_COM.Size = new System.Drawing.Size(41, 12);
            this.lab_COM.TabIndex = 11;
            this.lab_COM.Text = "串口号";
            // 
            // cmb_Parity
            // 
            this.cmb_Parity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Parity.FormattingEnabled = true;
            this.cmb_Parity.Location = new System.Drawing.Point(129, 156);
            this.cmb_Parity.Name = "cmb_Parity";
            this.cmb_Parity.Size = new System.Drawing.Size(121, 20);
            this.cmb_Parity.TabIndex = 10;
            // 
            // cmb_StopBits
            // 
            this.cmb_StopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_StopBits.FormattingEnabled = true;
            this.cmb_StopBits.Location = new System.Drawing.Point(129, 125);
            this.cmb_StopBits.Name = "cmb_StopBits";
            this.cmb_StopBits.Size = new System.Drawing.Size(121, 20);
            this.cmb_StopBits.TabIndex = 9;
            // 
            // cmb_DataBits
            // 
            this.cmb_DataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_DataBits.FormattingEnabled = true;
            this.cmb_DataBits.Location = new System.Drawing.Point(129, 94);
            this.cmb_DataBits.Name = "cmb_DataBits";
            this.cmb_DataBits.Size = new System.Drawing.Size(121, 20);
            this.cmb_DataBits.TabIndex = 8;
            // 
            // cmb_BaudRate
            // 
            this.cmb_BaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_BaudRate.FormattingEnabled = true;
            this.cmb_BaudRate.Location = new System.Drawing.Point(129, 63);
            this.cmb_BaudRate.Name = "cmb_BaudRate";
            this.cmb_BaudRate.Size = new System.Drawing.Size(121, 20);
            this.cmb_BaudRate.TabIndex = 7;
            // 
            // txt_DataFile
            // 
            this.txt_DataFile.Location = new System.Drawing.Point(129, 227);
            this.txt_DataFile.Name = "txt_DataFile";
            this.txt_DataFile.Size = new System.Drawing.Size(373, 21);
            this.txt_DataFile.TabIndex = 6;
            // 
            // lab_DataFile
            // 
            this.lab_DataFile.AutoSize = true;
            this.lab_DataFile.Location = new System.Drawing.Point(41, 227);
            this.lab_DataFile.Name = "lab_DataFile";
            this.lab_DataFile.Size = new System.Drawing.Size(77, 12);
            this.lab_DataFile.TabIndex = 5;
            this.lab_DataFile.Text = "数据存储路径";
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(427, 321);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 23);
            this.btn_Save.TabIndex = 4;
            this.btn_Save.Text = "保存&&应用";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // lab_Parity
            // 
            this.lab_Parity.AutoSize = true;
            this.lab_Parity.Location = new System.Drawing.Point(39, 156);
            this.lab_Parity.Name = "lab_Parity";
            this.lab_Parity.Size = new System.Drawing.Size(65, 12);
            this.lab_Parity.TabIndex = 3;
            this.lab_Parity.Text = "奇偶效验位";
            // 
            // lab_StopBits
            // 
            this.lab_StopBits.AutoSize = true;
            this.lab_StopBits.Location = new System.Drawing.Point(39, 125);
            this.lab_StopBits.Name = "lab_StopBits";
            this.lab_StopBits.Size = new System.Drawing.Size(41, 12);
            this.lab_StopBits.TabIndex = 2;
            this.lab_StopBits.Text = "停止位";
            // 
            // lab_DataBits
            // 
            this.lab_DataBits.AutoSize = true;
            this.lab_DataBits.Location = new System.Drawing.Point(39, 94);
            this.lab_DataBits.Name = "lab_DataBits";
            this.lab_DataBits.Size = new System.Drawing.Size(41, 12);
            this.lab_DataBits.TabIndex = 1;
            this.lab_DataBits.Text = "数据位";
            // 
            // lab_BaudRate
            // 
            this.lab_BaudRate.AutoSize = true;
            this.lab_BaudRate.Location = new System.Drawing.Point(39, 63);
            this.lab_BaudRate.Name = "lab_BaudRate";
            this.lab_BaudRate.Size = new System.Drawing.Size(41, 12);
            this.lab_BaudRate.TabIndex = 0;
            this.lab_BaudRate.Text = "波特率";
            // 
            // Form_SystemSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 356);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_SystemSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "系统设置";
            this.Load += new System.EventHandler(this.SystemSet_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Label lab_Parity;
        private System.Windows.Forms.Label lab_StopBits;
        private System.Windows.Forms.Label lab_DataBits;
        private System.Windows.Forms.Label lab_BaudRate;
        private System.Windows.Forms.ComboBox cmb_Parity;
        private System.Windows.Forms.ComboBox cmb_StopBits;
        private System.Windows.Forms.ComboBox cmb_DataBits;
        private System.Windows.Forms.ComboBox cmb_BaudRate;
        private System.Windows.Forms.TextBox txt_DataFile;
        private System.Windows.Forms.Label lab_DataFile;
        private System.Windows.Forms.ComboBox cmb_PortName;
        private System.Windows.Forms.Label lab_COM;
        private System.Windows.Forms.TextBox txt_LogFile;
        private System.Windows.Forms.Label lab_LogFile;
        private System.Windows.Forms.Label lab_EnergyMetersType;
        private System.Windows.Forms.ComboBox cmb_EnergyMetersType;
    }
}