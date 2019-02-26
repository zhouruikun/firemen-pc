namespace SCBAControlHost
{
	partial class MessageBoxEX
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
			this.btnCanel = new System.Windows.Forms.Button();
			this.btnNo = new System.Windows.Forms.Button();
			this.btnYes = new System.Windows.Forms.Button();
			this.LabelMessage = new System.Windows.Forms.Label();
			this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// btnCanel
			// 
			this.btnCanel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnCanel.Location = new System.Drawing.Point(286, 99);
			this.btnCanel.Name = "btnCanel";
			this.btnCanel.Size = new System.Drawing.Size(100, 50);
			this.btnCanel.TabIndex = 9;
			this.btnCanel.Text = "取消";
			this.btnCanel.UseVisualStyleBackColor = true;
			// 
			// btnNo
			// 
			this.btnNo.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnNo.Location = new System.Drawing.Point(152, 99);
			this.btnNo.Name = "btnNo";
			this.btnNo.Size = new System.Drawing.Size(100, 50);
			this.btnNo.TabIndex = 8;
			this.btnNo.Text = "否";
			this.btnNo.UseVisualStyleBackColor = true;
			// 
			// btnYes
			// 
			this.btnYes.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnYes.Location = new System.Drawing.Point(18, 99);
			this.btnYes.Name = "btnYes";
			this.btnYes.Size = new System.Drawing.Size(100, 50);
			this.btnYes.TabIndex = 7;
			this.btnYes.Text = "是";
			this.btnYes.UseVisualStyleBackColor = true;
			// 
			// LabelMessage
			// 
			this.LabelMessage.AutoSize = true;
			this.LabelMessage.Font = new System.Drawing.Font("宋体", 12F);
			this.LabelMessage.Location = new System.Drawing.Point(74, 44);
			this.LabelMessage.Name = "LabelMessage";
			this.LabelMessage.Size = new System.Drawing.Size(56, 16);
			this.LabelMessage.TabIndex = 6;
			this.LabelMessage.Text = "label1";
			// 
			// pictureBoxIcon
			// 
			this.pictureBoxIcon.Location = new System.Drawing.Point(18, 28);
			this.pictureBoxIcon.Name = "pictureBoxIcon";
			this.pictureBoxIcon.Size = new System.Drawing.Size(50, 50);
			this.pictureBoxIcon.TabIndex = 5;
			this.pictureBoxIcon.TabStop = false;
			// 
			// MessageBoxEX
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(408, 153);
			this.Controls.Add(this.btnCanel);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.btnYes);
			this.Controls.Add(this.LabelMessage);
			this.Controls.Add(this.pictureBoxIcon);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageBoxEX";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "MessageBoxEX";
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnCanel;
		private System.Windows.Forms.Button btnNo;
		private System.Windows.Forms.Button btnYes;
		private System.Windows.Forms.Label LabelMessage;
		private System.Windows.Forms.PictureBox pictureBoxIcon;
	}
}