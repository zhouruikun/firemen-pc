namespace SCBAControlHost
{
	partial class SerialComDebug
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
			this.listViewDebug = new System.Windows.Forms.ListView();
			this.columnTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDirect = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDirPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnCmd = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDataField = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnClearDbgData = new System.Windows.Forms.Button();
			this.btnPauseDbg = new System.Windows.Forms.Button();
			this.columnChecksum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// listViewDebug
			// 
			this.listViewDebug.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTime,
            this.columnDirect,
            this.columnType,
            this.columnDirPath,
            this.columnCmd,
            this.columnDataField,
            this.columnChecksum,
            this.columnDescription});
			this.listViewDebug.FullRowSelect = true;
			this.listViewDebug.Location = new System.Drawing.Point(3, 7);
			this.listViewDebug.Name = "listViewDebug";
			this.listViewDebug.Size = new System.Drawing.Size(1023, 472);
			this.listViewDebug.TabIndex = 4;
			this.listViewDebug.UseCompatibleStateImageBehavior = false;
			this.listViewDebug.View = System.Windows.Forms.View.Details;
			// 
			// columnTime
			// 
			this.columnTime.Text = "时间";
			this.columnTime.Width = 95;
			// 
			// columnDirect
			// 
			this.columnDirect.Text = "方向";
			this.columnDirect.Width = 70;
			// 
			// columnType
			// 
			this.columnType.Text = "包类型";
			this.columnType.Width = 130;
			// 
			// columnDirPath
			// 
			this.columnDirPath.Text = "Dir+Path字节";
			this.columnDirPath.Width = 85;
			// 
			// columnCmd
			// 
			this.columnCmd.Text = "命令字节";
			// 
			// columnDataField
			// 
			this.columnDataField.Text = "数据字段";
			this.columnDataField.Width = 300;
			// 
			// columnDescription
			// 
			this.columnDescription.Text = "说明";
			this.columnDescription.Width = 260;
			// 
			// btnClearDbgData
			// 
			this.btnClearDbgData.Location = new System.Drawing.Point(941, 485);
			this.btnClearDbgData.Name = "btnClearDbgData";
			this.btnClearDbgData.Size = new System.Drawing.Size(75, 23);
			this.btnClearDbgData.TabIndex = 5;
			this.btnClearDbgData.Text = "清空数据";
			this.btnClearDbgData.UseVisualStyleBackColor = true;
			// 
			// btnPauseDbg
			// 
			this.btnPauseDbg.Location = new System.Drawing.Point(860, 485);
			this.btnPauseDbg.Name = "btnPauseDbg";
			this.btnPauseDbg.Size = new System.Drawing.Size(75, 23);
			this.btnPauseDbg.TabIndex = 6;
			this.btnPauseDbg.Text = "暂停";
			this.btnPauseDbg.UseVisualStyleBackColor = true;
			// 
			// columnChecksum
			// 
			this.columnChecksum.Text = "校验";
			this.columnChecksum.Width = 40;
			// 
			// SerialComDebug
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1028, 512);
			this.Controls.Add(this.btnPauseDbg);
			this.Controls.Add(this.btnClearDbgData);
			this.Controls.Add(this.listViewDebug);
			this.KeyPreview = true;
			this.Location = new System.Drawing.Point(20, 20);
			this.Name = "SerialComDebug";
			this.Text = "调试窗口";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listViewDebug;
		private System.Windows.Forms.ColumnHeader columnTime;
		private System.Windows.Forms.ColumnHeader columnDirect;
		private System.Windows.Forms.ColumnHeader columnType;
		private System.Windows.Forms.ColumnHeader columnDirPath;
		private System.Windows.Forms.ColumnHeader columnCmd;
		private System.Windows.Forms.ColumnHeader columnDataField;
		private System.Windows.Forms.ColumnHeader columnDescription;
		private System.Windows.Forms.Button btnClearDbgData;
		private System.Windows.Forms.Button btnPauseDbg;
		private System.Windows.Forms.ColumnHeader columnChecksum;

	}
}