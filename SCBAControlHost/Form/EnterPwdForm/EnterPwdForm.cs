using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using MyUtils;

namespace SCBAControlHost
{
	public partial class EnterPwdForm : Form
	{
		private Form fBack = new Form();
		CtrlAutoSize autosize;					//自适应窗口大小工具类对象
		public FormMain formMain = null;		//主窗口

		private bool formVisible;
		public bool FormVisible
		{
			get { return formVisible; }
			set
			{
				//清除密码窗口
				textBoxEnterPwdFormPwd.Text = "";

				formVisible = value;
				fBack.Visible = value;
				this.Visible = value;
				textBoxEnterPwdFormPwd.Focus();
				//fBack.BringToFront();
			}
		}

		public EnterPwdForm()
		{
			InitializeComponent();

			this.TransparencyKey = Color.Silver;
			this.FormClosing += new FormClosingEventHandler(EnterPwdForm_FormClosing);	//窗口关闭事件
			this.Move += new EventHandler(EnterPwdForm_Move);							//窗口移动事件
			this.LocationChanged += new EventHandler(EnterPwdForm_LocationChanged);		//窗口位置改变事件
			btnEnterPwdFormExit.Click += new EventHandler(btnExit_Click);				//退出按钮事件
			this.LostFocus += new EventHandler(EnterPwdForm_LostFocus);
			textBoxEnterPwdFormPwd.LostFocus += new EventHandler(textBoxEnterPwdFormPwd_LostFocus);

			fBack.GotFocus += new EventHandler(fBack_GotFocus);
			fBack.FormBorderStyle = FormBorderStyle.None;
			fBack.Size = this.Size;
			fBack.Location = this.Location;
			fBack.Opacity = 0.7;
			fBack.Show();

			this.BringToFront();

			this.Resize += new EventHandler(EnterPwdForm_Resize);
			autosize = new CtrlAutoSize(this);
			autosize.setControlsTag(this);
		}

		void textBoxEnterPwdFormPwd_LostFocus(object sender, EventArgs e)
		{
			if (!btnEnterPwdFormOK.Focused)
				btnExit_Click(null, null);
		}

		void EnterPwdForm_LostFocus(object sender, EventArgs e)
		{
			if (!textBoxEnterPwdFormPwd.Focused)
				btnExit_Click(null, null);
		}

		//重载 预处理键 按下事件
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Enter:	//Enter按键, 停止报警
					btnEnterPwdFormOK.PerformClick();
					break;
				default:
					break;
			}
			return false;
		}

		#region 系统功能函数

		//背景窗口获取焦点之后, 又将前景窗口切换到最上面
		void fBack_GotFocus(object sender, EventArgs e)
		{
			this.BringToFront();
		}

		//位置改变时, 背景窗口跟着变
		void EnterPwdForm_LocationChanged(object sender, EventArgs e)
		{
			fBack.Size = this.Size;
			fBack.Location = this.Location;
		}

		//大小改变时, 背景窗口跟着变, 且自适应改变
		void EnterPwdForm_Resize(object sender, EventArgs e)
		{
			fBack.Size = this.Size;
			fBack.Location = this.Location;
			autosize.resizeControl(this);	//在resize消息里调用此函数以自动设置窗口控件大小和位置
		}

		//前景窗口关闭时, 背景窗口也关闭
		void EnterPwdForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			fBack.Close();
			fBack.Dispose();
		}

		//退出时, 隐藏窗口
		void btnExit_Click(object sender, EventArgs e)
		{
			this.FormVisible = false;
		}

		void EnterPwdForm_Move(object sender, EventArgs e)
		{
			fBack.Location = this.Location;
		}

		#endregion
	}
}
