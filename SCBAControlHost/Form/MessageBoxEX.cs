using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SCBAControlHost
{
	public partial class MessageBoxEX : Form
	{
		public MessageBoxEX(string title, string text, MessageBoxIcon icon, MessageBoxButtons buttons)
		{
			InitializeComponent();
			btnYes.Click += new EventHandler(btnYes_Click);
			btnNo.Click += new EventHandler(btnNo_Click);
			btnCanel.Click += new EventHandler(btnCanel_Click);

			this.Text = title;
			LabelMessage.Text = text;
			pictureBoxIcon.Image = (System.Drawing.Image)(System.Drawing.SystemIcons.Question.ToBitmap());
			if (buttons == MessageBoxButtons.YesNoCancel)
			{
				btnYes.Visible = true;
				btnNo.Visible = true;
				btnCanel.Visible = true;
			}
			else if (buttons == MessageBoxButtons.OKCancel)
			{
				btnYes.Visible = true;
				btnNo.Visible = false;
				btnCanel.Visible = true;
			}

		}

		void btnCanel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		void btnNo_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.No;
		}

		void btnYes_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Yes;
		}
	}
}
