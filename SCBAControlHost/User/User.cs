using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MyUtils;
using System.IO;

namespace SCBAControlHost
{
	public enum USERSTATUS : int　　//显示指定枚举的底层数据类型
	{
		NoExistStatus = 0,		//用户不存在, 当用户移除时, 会变为"不存在状态"
		PowerOffStatus,			//用户关机
		SafeStatus,				//安全状态
		MildDangerousStatus,	//轻度危险
		DangerousStatus,		//危险状态
		LoseContactStatus,		//失去联系
		RetreatingStatus,		//正在撤出
		RetreatFailStatus		//撤出失败
	};

	public class UserBasicInfo
	{
		public string userNO = "";				//用户编号
		public string name = "";				//用户姓名
		public string birthDate = "";			//出生年月
		public string uAffiliatedUnit = "";		//用户所属单位
		public string userPhoto = "";			//照片
		public string duty = "";				//职务
		public int terminalGrpNO = 0;			//空呼组号
		public int terminalNO = 0;				//空呼终端号
		public string terminalCapSpec = "";		//终端设备的气瓶容量规格
		public string BlueToothMac = "";		//蓝牙MAC地址
		public string WirelessSN = "";			//无线SN
		public string Sex = "";					//性别
		public string Age = "";					//年龄

	}

	public class TerminalRTInfo
	{
		public double Pressure;				//终端当前气压
		public double Voltage;				//终端当前电压
		public int Temperature;				//终端当前温度
		public int PowerONTime;				//终端开机时间
		public byte TerminalStatus;			//终端状态
		public int RemainTime;				//终端当前剩余时间
		public double PressDropDownIn30s;	//30秒内的气压下降值

		private double pressPre;			//30秒之前的气压值
		public TerminalRTInfo()
		{
			//开启30秒气压检测定时器
			PressDropDownIn30s = 0;
			Pressure = 0;
			pressPre = 0;
			Timer tim = new Timer();
			tim.Interval = 30000;
			tim.Tick += new EventHandler(Timer_Tick);
			tim.Start();
		}

		//定时器
		private void Timer_Tick(object sender, EventArgs e)
		{
			PressDropDownIn30s = pressPre - Pressure;
			pressPre = Pressure;
		}
	}

	public class User
	{
		#region 静态常量区
		/*********************静态常量区*****************************************************/
		//颜色值
		public static Color RedColorBK = Color.FromArgb(0xFF, 0x00, 0x00);			//红色
		public static Color GreenColorBK = Color.FromArgb(0x70, 0xDB, 0x93);		//绿色
		public static Color YellowColorBK = Color.FromArgb(0xFF, 0x7F, 0x00);		//黄色
		public static Color GrayColorBK = Color.FromArgb(0x69, 0x69, 0x69);		//灰色
		public static Color ControlPrimaryColor = Color.FromArgb(63, 71, 82);		//控件原来的颜色

		// 倒计时栏背景色
		public static Color PowerOffStatusColor = GrayColorBK;			//关机状态的颜色--灰色
		public static Color SafeStatusColor = GreenColorBK;				//安全状态的颜色--绿色
		public static Color MildDangerousStatusColor = YellowColorBK;	//轻度危险的颜色--黄色
		public static Color DangerousStatusColor = RedColorBK;			//危险状态的颜色--红色
		public static Color LoseContactStatusColor = RedColorBK;		//失去联系的颜色--红色
		public  Color RetreatingStatusColor = YellowColorBK;			//正在撤出的颜色--黄色
		public static Color RetreatFailStatusColor = RedColorBK;		//撤出失败的颜色--红色
		

		/************************************************************************************/
		#endregion

		#region 静态方法区
		/*********************静态方法区*****************************************************/
		public static byte[] GetPressBytesByDouble(double Pressure)
		{
			byte[] PressureBytes = new byte[2];
			Pressure = Pressure * 100;
			PressureBytes[0] = (byte)(((int)Pressure) >> 8);
			PressureBytes[1] = (byte)(((int)Pressure) & 0x00FF);
			return PressureBytes;
		}

		public static byte[] GetVoltageBytesByDouble(double Voltage)
		{
			byte[] VoltageBytes = new byte[2];
			Voltage = Voltage * 100;
			VoltageBytes[0] = (byte)(((int)Voltage) >> 8);
			VoltageBytes[1] = (byte)(((int)Voltage) & 0x00FF);
			return VoltageBytes;
		}

		public static byte GetTemeratureByteByInt(int Temerature)
		{
			byte TemeratureByte;
			if (Temerature > 0)
				TemeratureByte = (byte)Temerature;
			else
				TemeratureByte = (byte)((Temerature & 0x7F) | 0x80);
			return TemeratureByte;
		}

		public static byte[] GetTimeBytesByInt(int PowerTim)
		{
			byte[] PowerTimBytes = new byte[2];
			PowerTimBytes[0] = (byte)(PowerTim >> 8);
			PowerTimBytes[1] = (byte)(PowerTim & 0x00FF);
			return PowerTimBytes;
		}


		public static double GetPressDoubleByBytes(byte[] PressureBytes, int offset)
		{
			double Press = 0;

			Press = (double)(AppUtil.bytesToInt(PressureBytes, offset, 2)) / 100;
			
			return Press;
		}

		public static double GetVoltageDoubleByBytes(byte[] VoltageBytes, int offset)
		{
			double Voltage = 0;

			Voltage = (double)(AppUtil.bytesToInt(VoltageBytes, offset, 2)) / 100;
			
			return Voltage;
		}

		public static byte GetTemeratureIntByByte(byte TemeratureByte)
		{
			int Temerature = 0;

			if ((TemeratureByte & 0x80) > 0)
				Temerature = (TemeratureByte & 0x7F) * -1;
			else
				Temerature = (TemeratureByte & 0x7F);

			return (byte)Temerature;
		}

		public static int GetTimeIntByBytes(byte[] TimeBytes, int offset)
		{
			int PowerTime = 0;

			PowerTime = AppUtil.bytesToInt(TimeBytes, offset, 2);

			return PowerTime;
		}

		/************************************************************************************/
		#endregion


		#region 成员变量
		/*********************成员变量******************************************************/
		private int uid = -1;				//对象唯一标识
		public int Uid
		{
			get { return uid; }
			set {
				uid = value;
				if (uid != -1)
				{
					pUserView.RearrangeUserView(uid);	//改变uid需要重新排布位置, 并且需要重新设置tag
					AppUtil.setControlsTag(pUserView.UserPanel);		//重新设置tag
				}
			}
		}

		private UserView pUserView;			//用户视图类对象
		internal UserView PUserView
		{
			get { return pUserView; }
			set { pUserView = value; }
		}

		private bool isSelected = false;	//用户是否被选中
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				if (value)
					this.PUserView.BtnName.FlatAppearance.BorderSize = 2;
				else
					this.PUserView.BtnName.FlatAppearance.BorderSize = 0;
			}
		}

		private int powerupCount = 0;	//开机计数器
		public int PowerupCount
		{
			get { return powerupCount; }
			set { powerupCount = value; }
		}

		/***************************用户人员相关信息**************************************/
		private UserBasicInfo basicInfo;
		public UserBasicInfo BasicInfo
		{
			get { return basicInfo; }
			set {
				basicInfo = value;
				if (basicInfo.name != null)
				{
					if (basicInfo.name.Length > 6)
						pUserView.BtnName.Text = "姓名: " + basicInfo.name.Substring(0, 6);
					else
						pUserView.BtnName.Text = "姓名: " + basicInfo.name;
				}
				else
					pUserView.BtnName.Text = "姓名: ";
			}
		}

		/***************************终端设备实时相关信息**************************************/
		private TerminalRTInfo terminalInfo;
		public TerminalRTInfo TerminalInfo
		{
			get { return terminalInfo; }
			set {
				terminalInfo = value;
			}
		}

		private USERSTATUS uStatus = USERSTATUS.NoExistStatus;	//用户状态
		public USERSTATUS UStatus
		{
			get { return uStatus; }
			set {
				uStatus = value;
				if (value == USERSTATUS.NoExistStatus)				//用户不存在, 当用户移除时, 会变为"不存在状态"
				{
					this.uid = -1;
					HideUserView();
				}
				else if (value == USERSTATUS.PowerOffStatus)		//关机状态
				{
					//压力栏
					pUserView.PanelPressure.BackColor = PowerOffStatusColor;
					pUserView.LabelPressure.Text = "0.0";
					//隐藏倒计时
					pUserView.PanelCountDown.BackColor = PowerOffStatusColor;
					pUserView.LabelCountDown.Visible = false;
					pUserView.LabelCountDownUnit.Visible = false;
					//设置并显示状态标志
					pUserView.PictureBoxStatus.Image = Properties.Resources.Shutdown;
					pUserView.PictureBoxStatus.Visible = true;
					pUserView.LabelStatus.Text = "关      机";
					pUserView.LabelStatus.Visible = true;
				}
				else if (value == USERSTATUS.SafeStatus)			//安全状态
				{
					//压力栏
					pUserView.PanelPressure.BackColor = SafeStatusColor;
					pUserView.LabelPressure.Text = terminalInfo.Pressure.ToString("F1");	//更新气压显示
					//显示倒计时
					pUserView.PanelCountDown.BackColor = ControlPrimaryColor;
					pUserView.LabelCountDown.Visible = true;
					pUserView.LabelCountDownUnit.Visible = true;
					pUserView.LabelCountDown.Text = terminalInfo.RemainTime.ToString();		//更新剩余时间显示
					pUserView.LabelCountDownUnit.BringToFront();
					//隐藏状态标志
					pUserView.PictureBoxStatus.Visible = false;
					pUserView.LabelStatus.Visible = false;
				}
				else if (value == USERSTATUS.MildDangerousStatus)	//轻度危险状态
				{
					//压力栏
					pUserView.PanelPressure.BackColor = MildDangerousStatusColor;
					pUserView.LabelPressure.Text = terminalInfo.Pressure.ToString("F1");	//更新气压显示
					//显示倒计时
					pUserView.PanelCountDown.BackColor = ControlPrimaryColor;
					pUserView.LabelCountDown.Visible = true;
					pUserView.LabelCountDownUnit.Visible = true;
					pUserView.LabelCountDown.Text = terminalInfo.RemainTime.ToString();		//更新剩余时间显示
					pUserView.LabelCountDownUnit.BringToFront();
					//隐藏状态标志
					pUserView.PictureBoxStatus.Visible = false;
					pUserView.LabelStatus.Visible = false;
				}
				else if (value == USERSTATUS.DangerousStatus)		//危险状态
				{
					//设置压力
					pUserView.PanelPressure.BackColor = DangerousStatusColor;
					pUserView.LabelPressure.Text = terminalInfo.Pressure.ToString("F1");	//更新气压显示
					//显示倒计时
					pUserView.PanelCountDown.BackColor = ControlPrimaryColor;
					pUserView.LabelCountDown.Visible = true;
					pUserView.LabelCountDownUnit.Visible = true;
					pUserView.LabelCountDown.Text = terminalInfo.RemainTime.ToString();		//更新剩余时间显示
					pUserView.LabelCountDownUnit.BringToFront();
					//隐藏状态标志
					pUserView.PictureBoxStatus.Visible = false;
					pUserView.LabelStatus.Visible = false;
				}
				else if (value == USERSTATUS.LoseContactStatus)		//失去联系状态
				{
					//设置压力
					pUserView.PanelPressure.BackColor = LoseContactStatusColor;
					//隐藏倒计时
					pUserView.PanelCountDown.BackColor = LoseContactStatusColor;
					pUserView.LabelCountDownUnit.Visible = false;
					pUserView.LabelCountDown.Visible = false;
					//显示状态标志
					pUserView.PictureBoxStatus.Image = Properties.Resources.loseContact;
					pUserView.PictureBoxStatus.Visible = true;
					pUserView.LabelStatus.Text = "失去联系";
					pUserView.LabelStatus.Visible = true;
				}
				else if (value == USERSTATUS.RetreatingStatus)		//正在撤出状态
				{
					//确定背景颜色
					if (TerminalInfo.Pressure > 10)				//安全状态
						RetreatingStatusColor = SafeStatusColor;
					else if (TerminalInfo.Pressure > 6)			//轻度危险
						RetreatingStatusColor = MildDangerousStatusColor;
					else
						RetreatingStatusColor = DangerousStatusColor;

					//设置压力
					pUserView.PanelPressure.BackColor = RetreatingStatusColor;
					pUserView.LabelPressure.Text = terminalInfo.Pressure.ToString("F1");	//更新气压显示
					//隐藏倒计时
					pUserView.PanelCountDown.BackColor = RetreatingStatusColor;
					pUserView.LabelCountDownUnit.Visible = false;
					pUserView.LabelCountDown.Visible = false;
					//显示状态标志
					pUserView.PictureBoxStatus.Image = Properties.Resources.retreating1;
					pUserView.PictureBoxStatus.Visible = true;
					pUserView.LabelStatus.Text = "正在撤出";
					pUserView.LabelStatus.Visible = true;
				}
				else if (value == USERSTATUS.RetreatFailStatus)		//撤出失败状态
				{
					//设置压力
					pUserView.PanelPressure.BackColor = RetreatFailStatusColor;
					pUserView.LabelPressure.Text = terminalInfo.Pressure.ToString("F1");	//更新气压显示
					//隐藏倒计时
					pUserView.PanelCountDown.BackColor = RetreatFailStatusColor;
					pUserView.LabelCountDown.Visible = false;
					pUserView.LabelCountDownUnit.Visible = false;
					//显示状态标志
					pUserView.PictureBoxStatus.Image = Properties.Resources.retreatFail;
					pUserView.PictureBoxStatus.Visible = true;
					pUserView.LabelStatus.Text = "撤出失败";
					pUserView.LabelStatus.Visible = true;
				}
			}
		}

		//报警使能标志
		public bool IsExceedThreshold = false;		//当前是否超出阈值
		private bool alarmFlagForExceedTh = false;	//由超出阈值引起的报警标志
		public bool AlarmFlagForExceedTh
		{
			get { return alarmFlagForExceedTh; }
			set {
				alarmFlagForExceedTh = value;
				if (alarmFlagForExceedTh | alarmFlagForLost | alarmFlagForRetreat)	//若三个报警标志中有一个有效, 则开启本用户报警
					IsPlayingAlarm = true;
				else	//若三个报警标志都无效, 则停止本用户报警
					IsPlayingAlarm = false;
				}
		}
		private bool alarmFlagForLost = false;		//由失去联系引起的报警标志
		public bool AlarmFlagForLost
		{
			get { return alarmFlagForLost; }
			set {
				alarmFlagForLost = value;
				if (alarmFlagForExceedTh | alarmFlagForLost | alarmFlagForRetreat)	//若三个报警标志中有一个有效, 则开启本用户报警
					IsPlayingAlarm = true;
				else	//若三个报警标志都无效, 则停止本用户报警
					IsPlayingAlarm = false;
			}
		}
		private bool alarmFlagForRetreat = false;	//由撤出失败引起的报警标志
		public bool AlarmFlagForRetreat
		{
			get { return alarmFlagForRetreat; }
			set {
				alarmFlagForRetreat = value;
				if (alarmFlagForExceedTh | alarmFlagForLost | alarmFlagForRetreat)	//若三个报警标志中有一个有效, 则开启本用户报警
					IsPlayingAlarm = true;
				else	//若三个报警标志都无效, 则停止本用户报警
					IsPlayingAlarm = false;
			}
		}

		//当前用户是否正在处于报警状态
		private bool isPlayingAlarm = false;
		public bool IsPlayingAlarm
		{
			get { return isPlayingAlarm; }
			set {
				isPlayingAlarm = value;
				if (isPlayingAlarm)			//若正在报警, 则显示报警label
					pUserView.LabelAlarm.Visible = true;
				else						//否则隐藏报警label
					pUserView.LabelAlarm.Visible = false;
			}
		}

		//状态是否改变标志
		public bool isChanged = false;
		public bool isRecvPack = false;		//是否收到数据包

		/*************************************************/
		#endregion


		#region 参数区
		/*********************参数区*********************/
		private const float upperThreshold = 10;
		private const float lowerThreshold = 6;
		/*************************************************/
		#endregion


		#region 构造函数
		/*********************构造函数*********************/
		public User(int uid, Control con)
        {
			this.uid = uid;
			pUserView = new UserView(uid, con);
			terminalInfo = new TerminalRTInfo();
			this.UStatus = USERSTATUS.PowerOffStatus;	//初始时中断为关机状态
        }
		/*************************************************/
		#endregion


		#region 私有函数
		/*********************私有函数*********************/
		/*************************************************/
		#endregion


		#region 外部调用函数
		/*********************外部调用函数*********************/
		public void ShowUserView()
		{
			pUserView.UserPanel.Visible = true;
		}

		public void HideUserView()
		{
			RecursiveHide(pUserView.UserPanel);
			pUserView.UserPanel.Visible = true;

			pUserView.BtnName.Visible = true;
			pUserView.BtnName.Text = "";

			pUserView.PanelPressure.Visible = true;
			pUserView.PanelPressure.BackColor = ControlPrimaryColor;

			pUserView.PanelCountDown.Visible = true;
			pUserView.PanelCountDown.BackColor = ControlPrimaryColor;

			pUserView.BtnDetails.Visible = true;
			pUserView.BtnDetails.Text = "";

		}

		public void RecursiveShow(Control parent)
		{
			parent.Visible = true;
			foreach (Control con in parent.Controls)
			{
				RecursiveHide(con);
			}
		}

		public void RecursiveHide(Control parent)
		{
			parent.Visible = false;
			foreach (Control con in parent.Controls)
			{
				RecursiveHide(con);
			}
		}

		/*************************************************/
		#endregion
	}
}
