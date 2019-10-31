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
using System.IO;

namespace SCBAControlHost
{
	public partial class DetailsForm : Form
	{
		private Form fBack = new Form();
		CtrlAutoSize autosize;					//自适应窗口大小工具类对象
		public FormMain formMain = null;		//主窗口

		System.Drawing.Image picBmp;

		private bool formVisible;
		public bool FormVisible
		{
			get { return formVisible; }
			set {
				formVisible = value;
				fBack.Visible = value;
				this.Visible = value;
				//fBack.BringToFront();
			}
		}

		public User CurUser;

		//构造函数, 传入主窗口的大小和用户
		public DetailsForm()
		{
			InitializeComponent();

			//this.pictureBoxDetailsFormUserPhoto.Parent = this.pictureBoxDetailsFormUserBottom;		// 修改后注释掉的(2019-2-26)
			//pictureBoxDetailsFormUserPhoto.Location = new Point(30, 20);

			this.TransparencyKey = Color.Silver;	//此窗口设置为透明
			this.FormClosing += new FormClosingEventHandler(DetailsForm_FormClosing);	//窗口关闭事件
			this.Move += new EventHandler(DetailsForm_Move);							//窗口移动事件
			this.LocationChanged += new EventHandler(DetailsForm_LocationChanged);		//窗口位置改变事件
			btnDetailsFormExit.Click += new EventHandler(btnDetailsFormExit_Click);		//退出按钮事件
			this.KeyDown += new KeyEventHandler(DetailsForm_KeyDown);					//键盘事件
			this.LostFocus += new EventHandler(DetailsForm_LostFocus);
			
			fBack.GotFocus += new EventHandler(fBack_GotFocus);
			fBack.FormBorderStyle = FormBorderStyle.None;
			fBack.Size = this.Size;
			fBack.Location = this.Location;
			fBack.Opacity = 0.7;
			fBack.Enabled = false;
			fBack.Show();
			
			this.BringToFront();

			this.Resize += new EventHandler(DetailsForm_Resize);
			autosize = new CtrlAutoSize(this);
			autosize.setControlsTag(this);
		}

		//当前景窗口失去焦点时的事件
		void DetailsForm_LostFocus(object sender, EventArgs e)
		{
			btnDetailsFormExit_Click(null, null);
			//this.FormVisible = false;
			//if (formMain != null)
			//    formMain.Focus();
		}

		//设置用户
		public void SetUser(User user)
		{
			CurUser = user;
			UpdateDetailForm();
		}

		//刷新窗口显示
		public void UpdateDetailForm()
		{
			if (CurUser != null)
			{
				if (CurUser.BasicInfo.name.Length > 6)
					labelDetailsFormUserName.Text = "用户: " + CurUser.BasicInfo.name.Substring(0, 6);
				else
					labelDetailsFormUserName.Text = "用户: " + CurUser.BasicInfo.name;
				labelDetailsFormBirthDate.Text = "出生年月:  " + CurUser.BasicInfo.birthDate;
				labelDetailsFormDuty.Text = "职      务:  " + CurUser.BasicInfo.duty;
				switch (CurUser.UStatus)
				{
					case USERSTATUS.PowerOffStatus: labelDetailsFormStatus.Text = "状      态:  关机"; break;
					case USERSTATUS.SafeStatus: labelDetailsFormStatus.Text = "状      态:  安全"; break;
					case USERSTATUS.MildDangerousStatus: labelDetailsFormStatus.Text = "状      态:  轻度危险"; break;
					case USERSTATUS.DangerousStatus: labelDetailsFormStatus.Text = "状      态:  危险"; break;
					case USERSTATUS.LoseContactStatus: labelDetailsFormStatus.Text = "状      态:  失去联系"; break;
					case USERSTATUS.RetreatingStatus: labelDetailsFormStatus.Text = "状      态:  正在撤出"; break;
					case USERSTATUS.RetreatFailStatus: labelDetailsFormStatus.Text = "状      态:  撤出失败"; break;
					default: break;
				}
				labelDetailsFormTerminalNO.Text = "空呼编号:  " + CurUser.BasicInfo.terminalGrpNO.ToString("D8") + "-" + CurUser.BasicInfo.terminalNO.ToString("D2");
				labelDetailsFormTerminalCapSpec.Text = "气瓶容量:  " + CurUser.BasicInfo.terminalCapSpec + "L";
				labelDetailsFormTerminalPressure.Text = "当前气压:  " + CurUser.TerminalInfo.Pressure.ToString("F1") + "MPa";
				labelDetailsFormTerminalVolt.Text = "设备电压:  " + CurUser.TerminalInfo.Voltage.ToString("F2") + "V";
                if(CurUser.TerminalInfo.Temperature>0x80)
                {
                    labelDetailsFormTerminalTemperature.Text = "环境温度:  -" + (CurUser.TerminalInfo.Temperature&0x7f) + "℃";

                }
                else
                    labelDetailsFormTerminalTemperature.Text = "环境温度:  " + CurUser.TerminalInfo.Temperature + "℃";

                labelDetailsFormTerminalRemainTim.Text = "剩余时间:  " + CurUser.TerminalInfo.RemainTime + "分钟";
				labelDetailsFormBTMac.Text = "蓝牙MAC:  " + CurUser.BasicInfo.BlueToothMac;
				labelDetailsFormSNNo.Text = "无线SN号:  " + CurUser.BasicInfo.WirelessSN;

				if (File.Exists(".\\res\\UserTable\\" + CurUser.BasicInfo.userPhoto))
				{
					System.Drawing.Image img = System.Drawing.Image.FromFile(".\\res\\UserTable\\" + CurUser.BasicInfo.userPhoto);
					picBmp = new System.Drawing.Bitmap(img);
					img.Dispose();			// 释放图片资源
					if (picBmp != null)
						pictureBoxDetailsFormUserPhoto.Image = picBmp;
				}
				else
					pictureBoxDetailsFormUserPhoto.Image = Properties.Resources.UserImageNew;	//默认的用户图片
			}
		}

		#region 系统功能函数

		//键盘事件
		void DetailsForm_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.F6:	//F6按键, 退出查看详情
					btnDetailsFormExit.PerformClick();
					if (formMain != null)
						formMain.Focus();
					break;
				default:
					break;
			}
		}

		//背景窗口获取焦点之后, 又将前景窗口切换到最上面
		void fBack_GotFocus(object sender, EventArgs e)
		{
			this.BringToFront();
		}

		//位置改变时, 背景窗口跟着变
		void DetailsForm_LocationChanged(object sender, EventArgs e)
		{
			fBack.Size = this.Size;
			fBack.Location = this.Location;
		}

		//大小改变时, 背景窗口跟着变, 且自适应改变
		void DetailsForm_Resize(object sender, EventArgs e)
		{
			fBack.Size = this.Size;
			fBack.Location = this.Location;
			autosize.resizeControl(this);	//在resize消息里调用此函数以自动设置窗口控件大小和位置
		}

		//前景窗口关闭时, 背景窗口也关闭
		void DetailsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			fBack.Close();
			fBack.Dispose();
		}

		//退出时, 隐藏窗口
		void btnDetailsFormExit_Click(object sender, EventArgs e)
		{
			this.FormVisible = false;
			if (formMain != null)
				formMain.Focus();
		}

		void DetailsForm_Move(object sender, EventArgs e)
		{
			fBack.Location = this.Location;
		}

		#endregion
	}
}
