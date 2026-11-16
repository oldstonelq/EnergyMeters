namespace ReadDataSoftware
{
    partial class ChannelCardPanel
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

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // _layout
            // 
            this._layout.AutoScroll = true;
            this._layout.ColumnCount = 1;
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.Location = new System.Drawing.Point(0, 0);
            this._layout.Name = "_layout";
            this._layout.RowCount = 0;
            this._layout.Size = new System.Drawing.Size(640, 400);
            this._layout.TabIndex = 0;
            // 
            // ChannelCardPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._layout);
            this.Name = "ChannelCardPanel";
            this.Size = new System.Drawing.Size(640, 400);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
    }
}
