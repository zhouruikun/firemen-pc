using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SCBAControlHost;
using System.Runtime.InteropServices;
using SCBAControlHost.SysConfig;
using MyUtils;
using SCBAControlHost.SerialCommunication;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using log4net;
using SCBAControlHost.MyUtils;
using System.Net;
using SCBAControlHost.AppFuction;
using System.Globalization;
using System.Net.Sockets;
using SCBAControlHost.NetCommunication;
using System.Security.Cryptography;
using System.Media;
using System.Diagnostics;
using CoreAudioApi;

namespace SCBAControlHost
{
	public partial class FormMain : Form
	{
		#region 变量区

		#region 系统功能变量
		//屏幕分辨率
		private int ScreenActualWidth = Screen.PrimaryScreen.Bounds.Width;
		private int ScreenActualHeight = Screen.PrimaryScreen.Bounds.Height;
		CtrlAutoSize autosize;							//自适应窗口大小工具类对象
		AppUtil appUtil = new AppUtil();				//工具类
		DetailsForm detailsForm;						//详情窗口
		EnterPwdForm enterPwdForm;						//输入密码窗口
		LogMaintain logMaintain = new LogMaintain();	//系统日志维护
		bool isFormLoadDone = false;					//窗口加载完毕
		private static System.Diagnostics.Process TouchKeyboardProcess;		//软键盘应用资源
		#endregion 

		#region 用户相关变量
		List<User> users = new List<User>();					//用户列表
		List<User> usersEmpty = new List<User>();				//空用户列表
		UserRW userRW = new UserRW();							//用户配置文件
		#endregion

		#region 串口相关变量
		SerialCommunicate serialCom = new SerialCommunicate();	//串口设备
		//string[] ComKeyStr = new string[] { "Silicon Labs", "USB to UART" };		//串口关键字
		string[] ComKeyStr = new string[] { "ELTIMA Virtual Serial Port" };			//串口关键字
		Thread recvMsgThread;						//接收消息的线程
		
		SerialComDebug comDebugForm = new SerialComDebug();		//调试窗口资源
		int[] SerialBaudLUT = new int[8] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };	//串口波特率查找表
		bool isSerialShouldOpen = true;
		#endregion

		#region 系统配置相关变量
		private SystemConfig sysConfig = new SystemConfig();	//系统配置类
		public SystemConfig SysConfig
		{
			get { SystemConfig sysCfgTmp; lock (sysConfig) { sysCfgTmp = sysConfig; } return sysCfgTmp; }
			set { lock (sysConfig) { sysConfig = value; } }
		}
		private bool isSystemSetting = false;			//是否处于系统设置状态
		private int ServerRole = 0;						//主机角色, 0-未确定, 1-控制主机, 2-普通主机
		#endregion

		#region 网络相关变量
		NetCommunicate netcom =null;
		public bool isInternetAvailiable = false;	//网络是否有效标志

		public bool isStartingRealUpload = false;   //是否正在开启实时上传
        public bool isAuthPass = false;				//是否验证通过
		public bool isRealTimeUploading = false;	//是否正在实时上传
		public bool isInfoSyncing = false;			//当前是否正在信息同步中
		string LatestLogName = null;				//服务器上最新的日志文件名称
		string LatestPlayLogName = null;			//服务器上最新的回放日志文件名称

		string LoginURL = "";						//登录页面
		string KnowledgeDownloadURL = "";			//知识库和设备库页面
		string UserInfoDownloadURL = "";			//用户信息页面
		string LogUploadURL = "";					//上传页面

		#endregion

		#region 工作日志相关变量
		WorkLog worklog;
		WorkLogPlay worklogplay;
		#endregion

		#region 回放模式相关变量

		bool isPlayBackMode = false;					//当前是否正处于回放模式
		bool isPlaying = false;							//是否正在播放
		ToolTip tt_PlayBackPlay = new ToolTip();
		PlayBackLog playBack = new PlayBackLog();
		List<List<string>> LogList = new List<List<string>>();
		int RecordCounter = 0;							//当前记录的计数器
		DateTime PlayBackBaseTime = new DateTime();		//回放系统基准时间
		DateTime PlayBackStartTime = new DateTime();	//起始时间
		DateTime PlayBackEndTime = new DateTime();		//结束时间
		string TotalPlayTime = "00:00:00";
		string TimeHasBeenRunning = "00:00:00";
		List<string> PlayBackRecord = new List<string>();	//一条记录
		bool isTrackBarDown = false;						//进度条是否鼠标按下
		List<int> UserChangeLogIndexList = new List<int>();	//用户改变日志的下标列表, 其中记录了一个日志文件中, 所有与用户改变相关的记录的下标
		bool isAdjustingPBPos = false;					//是否正在调整回放的位置
		Thread pb_th = null;							//回放线程
		#endregion

		#region 用户列表相关变量
		DataTable CheckUserDT = new DataTable();
		#endregion

		#region 其他变量
		SynchronizationContext m_SyncContext = null;//用于多线程操作UI的对象
		private bool isAllUserUpdating = false;		//是否正在执行全部刷新操作
		private Queue<SerialRecvMsg> AllUserUpdateQueue = new Queue<SerialRecvMsg>(); 
		private bool isAllUserEvacuating = false;	//是否正在执行全部撤出操作
		private Queue<SerialRecvMsg> AllUserEvacuateQueue = new Queue<SerialRecvMsg>();

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");	//获取一个日志记录器
		#endregion

		#endregion


		#region 主窗口
		public FormMain()
		{
            #region 外部程序调用
            /*try
			{
				if (TouchKeyboardProcess == null)
				{
					TouchKeyboardProcess = new System.Diagnostics.Process();
					if (Environment.Is64BitOperatingSystem)		//判断系统位数
						TouchKeyboardProcess.StartInfo.FileName = @".\Extern\AdjustTouchKeyboardOpacity\x64\AdjustTouchKeyboardOpacity.exe";
					else
						TouchKeyboardProcess.StartInfo.FileName = @".\Extern\AdjustTouchKeyboardOpacity\x32\AdjustTouchKeyboardOpacity.exe";
					TouchKeyboardProcess.Start();
				}
				else
				{
					if (TouchKeyboardProcess.HasExited) //是否已经退出
					{
						TouchKeyboardProcess.Start();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}*/
            #endregion

            #region 音量调整到最大
            try
            {
			CoreAudioApi.MMDeviceEnumerator devices = new MMDeviceEnumerator();
			MMDevice device = devices.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
			device.AudioEndpointVolume.Mute = false;
			device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)1;
            }
            catch
            {
                MessageBox.Show("请检查声卡设备");

            }

			#endregion

			#region 界面初始化
			InitializeComponent();		//主界面初始化
			CreateResDirectory();		//创建资源文件夹
			InfoSyncInit();				//信息同步界面初始化
			SystemSettingInit();		//系统设置界面初始化
			UserChangeNOInit();			//用户改号界面初始化
			TempGroupInit();			//临时组队界面初始化
			KnowledgeBaseInit();		//知识库界面初始化
			DeviceBaseInit();			//设备库界面初始化
			CheckUserInit();			//用户列表界面初始化

			//用户面板初始化
			panelUsers.BringToFront();
			panelUsers.Paint += new PaintEventHandler(panelUsers_Paint);	//panelUsers重绘事件----始终隐藏水平滚动条
			panelUsers.Click += new EventHandler(panelUsers_Click);			//panelUsers点击事件----获取焦点

			PanelSwitch(CurPanel.EpanelContentMain);

			//回放相关控件初始化
			labelPlayBack.Visible = false;
			trackBarPlayBack.Visible = false;
			btnPlayBackPlay.Visible = false;
			btnPlayBackOpen.Visible = false;

			tt_PlayBackPlay.ShowAlways = true;
			tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "播放");
			ToolTip tt_PlayBackStop = new ToolTip();
			ToolTip tt_PlayBackOpen = new ToolTip();
			tt_PlayBackOpen.ShowAlways = true;
			tt_PlayBackOpen.SetToolTip(btnPlayBackOpen, "打开文件");

			pictureBoxPlayBackArrow.Parent = this;

			
			//自适应窗口大小工具类配置
			this.Resize += new EventHandler(Form_Resize);		//当窗口大小改变时, 使其中的所有控件自适应窗口大小
			autosize = new CtrlAutoSize(this);
			autosize.setControlsTag(this);
			#endregion

			#region 读取系统配置文件
			//反序列化系统配置  重新写入配置文件"systemConfig.CreateConfigFile();"
			//SysConfig.CreateConfigFile();
			if(SysConfig.ReadSystemSetting() == false)		//若读取配置文件失败, 则创建配置文件
				SysConfig.CreateConfigFile();
			if (SysConfig.Setting != null)
			{
				labelUnit.Text = "单位：" + SysConfig.Setting.unitName;
				labelGrpNumber.Text = "组号：" + SysConfig.Setting.groupNumber.ToString("D8");
			}
            netcom = new NetCommunicate(SysConfig);
            #endregion

            #region 为窗口和控件 订阅事件
            //为窗口和控件 订阅事件
            btnUserEvacuate.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);		//图片按钮的图片切换
			btnUserEvacuate.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);			//图片按钮的图片切换
			btnAllUserEvacuate.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);	//图片按钮的图片切换
			btnAllUserEvacuate.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);        //图片按钮的图片切换
            btnResetEvacuate.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);    //图片按钮的图片切换
            btnResetEvacuate.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);        //图片按钮的图片切换
            btnStopAlarm.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);			//图片按钮的图片切换
			btnStopAlarm.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);				//图片按钮的图片切换
			btnUserUpdate.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);			//图片按钮的图片切换
			btnUserUpdate.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);				//图片按钮的图片切换
			btnAllUserUpdate.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);		//图片按钮的图片切换
			btnAllUserUpdate.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);			//图片按钮的图片切换

			this.FormClosing += new FormClosingEventHandler(FormMain_FormClosing);		//退出 按钮点击事件
			btnUserEvacuate.Click += new EventHandler(btnUserEvacuate_Click);			//用户撤出 按钮点击事件
			btnAllUserEvacuate.Click += new EventHandler(btnAllUserEvacuate_Click);		//全部撤出 按钮点击事件
			btnUserUpdate.Click += new EventHandler(btnUserUpdate_Click);				//用户刷新 按钮点击事件
			btnAllUserUpdate.Click += new EventHandler(btnAllUserUpdate_Click);			//全部刷新 按钮点击事件
            btnResetEvacuate.Click += new EventHandler(btnResetEvacuate_Click);			//全部刷新 按钮点击事件

            btnStopAlarm.Click += new EventHandler(btnStopAlarm_Click);					//停止报警 按钮点击事件
			btnSysSetting.Click += new EventHandler(btnSysSetting_Click);				//系统设置 按钮点击事件
			btnUpLoad.Click += new EventHandler(btnUpLoad_Click);						//实时上传 按钮点击事件
			btnKnowledgeBase.Click += new EventHandler(btnKnowledgeBase_Click);			//知识库 按钮点击事件
			btnDeviceBase.Click += new EventHandler(btnDeviceBase_Click);				//设备库 按钮点击事件
			this.KeyDown += new KeyEventHandler(FormMain_KeyDown);						//键盘点击事件
			this.Load += new EventHandler(FormMain_Load);								//窗口加载完毕后, 再加载密码输入窗口和详情窗口
			this.Shown += new EventHandler(FormMain_Shown);

			richTextBoxAddress.TextChanged += new EventHandler(richTextBoxAddress_TextChanged);		//地址栏内容改变事件
			richTextBoxAddress.LostFocus += new EventHandler(richTextBoxAddress_LostFocus);			//地址栏失去焦点事件
			richTextBoxTask.TextChanged += new EventHandler(richTextBoxTask_TextChanged);			//任务栏内容改变事件
			richTextBoxTask.LostFocus += new EventHandler(richTextBoxTask_LostFocus);
			richTextBoxAddress_text = richTextBoxAddress.Text;										//地址栏初始内容记录
			richTextBoxTask_text = richTextBoxTask.Text;											//任务栏初始内容记录

			btnInternetState.Click += new EventHandler(btnInternetState_Click);						//联网按钮点击事件

			btnPlayBackEnter.Click += new EventHandler(btnPlayBackEnter_Click);
			btnPlayBackOpen.Click += new EventHandler(btnPlayBackOpen_Click);
			btnPlayBackPlay.MouseDown += new MouseEventHandler(btnPlayBackPlay_MouseDown);
			btnPlayBackPlay.MouseUp += new MouseEventHandler(btnPlayBackPlay_MouseUp);
			btnPlayBackOpen.MouseDown += new MouseEventHandler(btnPlayBackOpen_MouseDown);
			btnPlayBackOpen.MouseUp += new MouseEventHandler(btnPlayBackOpen_MouseUp);
			trackBarPlayBack.ValueChanged += new EventHandler(trackBarPlayBack_ValueChanged);
			trackBarPlayBack.MouseDown += new MouseEventHandler(trackBarPlayBack_MouseDown);
			trackBarPlayBack.MouseUp += new MouseEventHandler(trackBarPlayBack_MouseUp);

			//panelUsers.
			#endregion

			#region 定时器配置
			//定时器配置
			System.Windows.Forms.Timer tim = new System.Windows.Forms.Timer();
			tim.Interval = 1000;
			tim.Tick += new EventHandler(Timer_Tick); tim.Start();
			#endregion

			#region 测试代码

			#endregion
			
			#region 添加用户操作
			//添加空用户, 用于填充界面
			for (int i = 0; i < 8; i++)
				AddEmptyUser(i);

			//读取用户配置文件, 并添加用户
			if (userRW.ReadDefaultUserFile())
			{
				if (userRW.UserInfoList.Count > 0)
				{
					foreach (UserBasicInfo userInfo in userRW.UserInfoList)
					{
						if (userInfo.terminalGrpNO == SysConfig.Setting.groupNumber)	//若组号相等, 则将用户加入
							AddUser(userInfo);
					}
				}
			}
			else { MessageBox.Show("读取用户配置文件失败!"); }
			//AddUser(new UserBasicInfo());

			#endregion

			//设置全部控件的Tag
			autosize.setControlsTag(this);

        }

		//主窗口加载时执行的代码
		void FormMain_Load(object sender, EventArgs e)
		{
			#region 工作日志相关
			//创建工作日志实例
			worklog = new WorkLog(SysConfig.Setting.accessAccount, richTextBoxAddress.Text);
			//写入初始化状态到日志文件中
			worklog.LogQueue_Enqueue(LogCommand.getInitStatusRecord(users, SysConfig, richTextBoxAddress.Text, richTextBoxTask.Text, serialCom.SerialPortIsOpen, isInternetAvailiable));

			//创建播放日志实例
			worklogplay = new WorkLogPlay(SysConfig.Setting.accessAccount, richTextBoxAddress.Text);

			//服务器周期线程
			Thread PeriodServerth = new Thread(PeriodRecordServerThread);		//周期上传线程-周期为10s
			PeriodServerth.Name = "服务器周期线程";
			PeriodServerth.IsBackground = true;
			PeriodServerth.Start();

			#endregion

			#region 串口通信配置
			serialCom.worklog = this.worklog;
            labelChannel.Text = "信道：" + SysConfig.Setting.channal.ToString("D2");
            //串口通信配置
            //string comName = AppUtil.FindComByKeyStr(ComKeyStr);		//查找包含关键字的COM号
            if (SysConfig.Setting.serialCom != null)
			{
				if (serialCom.ComOpen(SysConfig.Setting.serialCom, SerialBaudLUT[SysConfig.Setting.serialBaud]))//若打开串口成功
				{
					//写入串口连接记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.Connect, null));
					serialCom.SendQueue_Enqueue(ProtocolCommand.SwitchChannelMsg((byte)(SysConfig.Setting.channal)));	//切换信道
					Thread.Sleep(2000);
                    ////发送主机查询命令
                    //serialsendmsg sendmsg = protocolcommand.serverquerycmdmsg(sysconfig.getserialnobytes());
                    //serialcom.sendqueue_enqueue(sendmsg);	//发送出去
                }
                else
                { MessageBox.Show("打开串口失败!"); }
               
            }
			else
				MessageBox.Show("打开串口失败!");
            //获取UI线程同步上下文
            m_SyncContext = SynchronizationContext.Current;
            recvMsgThread = new Thread(RecvMsgThread);		//开启接收消息的线程
			recvMsgThread.Name = "主界面处理串口数据线程";
			recvMsgThread.IsBackground = true;
			recvMsgThread.Start();

			//串口重连线程
			Thread serialRelinkThread = new Thread(SerialRelink);
			serialRelinkThread.Name = "串口重连线程";
			serialRelinkThread.IsBackground = true;
			serialRelinkThread.Start();

			Thread PeriodUploadth = new Thread(PeriodRecordThread);		//周期记录线程
			PeriodUploadth.Name = "周期记录线程";
			PeriodUploadth.IsBackground = true;
			PeriodUploadth.Start();

			#endregion

			#region 串口调试配置
			btnOpenDbg.Click += new EventHandler(btnOpenDbg_Click);
#if DEBUG
            btnOpenDbg.Visible = true;
            comDebugForm.StartDebugThread(serialCom.DebugMsgQueue, serialCom.DebugQueueWaitHandle);
            comDebugForm.Show();
            comDebugForm.Visible = false;
#else
			btnOpenDbg.Visible = false;
#endif
			#endregion

			#region 创建信道列表
			////创建信道列表
			//sysConfig.Setting.channelDic.Add(20, -1);
			//sysConfig.Setting.channelDic.Add(22, -1);
			//sysConfig.Setting.channelDic.Add(26, -1);
			//sysConfig.Setting.channelDic.Add(18, -1);
			//sysConfig.Setting.channelDic.Add(28, -1);
			//sysConfig.Setting.channelDic.Add(16, -1);
			//sysConfig.Setting.channelDic.Add(21, -1);
			//sysConfig.Setting.channelDic.Add(27, -1);
			//sysConfig.Setting.channelDic.Add(19, -1);
			//sysConfig.Setting.channelDic.Add(17, -1);
			//sysConfig.Setting.channelDic.Add(29, -1);
			//sysConfig.Setting.channelDic.Add(15, -1);
			//sysConfig.Setting.channelDic.Add(14, -1);
			//sysConfig.Setting.channelDic.Add(12, -1);
			//sysConfig.Setting.channelDic.Add(11, -1);
			#endregion

			#region 网络通信配置
			netcom.worklog = this.worklog;

			netcom.netDelegate.myEvent += new MyDelegate(netDelegate_myEvent);

			Thread NetRecvth = new Thread(NetPacketHandler);		//主界面处理网络接收数据线程
			NetRecvth.Name = "主界面处理网络接收数据线程";
			NetRecvth.IsBackground = true;
			NetRecvth.Start();

			Thread NetLinkTh = new Thread(NetLinkCheckHandler);		//网络检测线程
			NetLinkTh.Name = "网络检测线程";
			NetLinkTh.IsBackground = true;
			NetLinkTh.Start();

			System.Windows.Forms.Timer HeartBeatSnedTimer = new System.Windows.Forms.Timer();	//心跳包发送定时器, 定时2s
			HeartBeatSnedTimer.Interval = 2000;
			HeartBeatSnedTimer.Tick += new EventHandler(HeartBeatSnedTimer_Tick);
			HeartBeatSnedTimer.Start();
			#endregion

			#region 声音报警线程配置
			Thread alarmTh = new Thread(AlarmPlaySound);
			alarmTh.Name = "声音报警线程";
			alarmTh.IsBackground = true;
			alarmTh.Start();
			#endregion


			pb_th = new Thread(PlayBackThread);
			pb_th.Name = "回放线程";
			pb_th.IsBackground = true;
			pb_th.Start();

			logMaintain.DeleteExpiredLogFiles();		//删除过期的系统日志
			WorkLogMaintain();							//删除过期的工作日志




			//详情窗口初始化
			detailsForm = new DetailsForm();
			detailsForm.formMain = this;
			detailsForm.StartPosition = FormStartPosition.Manual;
			int x1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X;
			int y1 = panelContentMain.PointToScreen(new Point(0, 0)).Y;
			detailsForm.Location = new Point(x1 + btnCountDownColumn1.Width / 2, y1);
			Size sizeThis = CtrlAutoSize.GetSizeByTag(this);
			Size sizeDetails = CtrlAutoSize.GetSizeByTag(detailsForm);
			detailsForm.Size = new Size((int)((float)sizeDetails.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeDetails.Height / (float)sizeThis.Height * this.Height));
			detailsForm.Show();
			detailsForm.FormVisible = false;

			//输入密码窗口初始化
			enterPwdForm = new EnterPwdForm();
			enterPwdForm.formMain = this;
			enterPwdForm.StartPosition = FormStartPosition.Manual;
			int x1_1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X + btnCountDownColumn1.Width * 2 / 3;
			int y1_1 = panelUsers.PointToScreen(new Point(0, 0)).Y + panelColumnName.PointToScreen(new Point(0, 0)).Y;
			enterPwdForm.Location = new Point(x1_1, y1_1);
			Size sizeEnterPwd = CtrlAutoSize.GetSizeByTag(enterPwdForm);
			enterPwdForm.Size = new Size((int)((float)sizeEnterPwd.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeEnterPwd.Height / (float)sizeThis.Height * this.Height));
			enterPwdForm.Show();
			enterPwdForm.FormVisible = false;
			enterPwdForm.btnEnterPwdFormOK.Click += new EventHandler(btnEnterPwdFormOK_Click);
			isFormLoadDone = true;		//窗口加载完毕标志
		}
		// 窗口完全加载完毕的事件
		void FormMain_Shown(object sender, EventArgs e)
		{
//            #region 工作日志相关
//            //创建工作日志实例
//            worklog = new WorkLog(SysConfig.Setting.accessAccount, richTextBoxAddress.Text);
//            //写入初始化状态到日志文件中
//            worklog.LogQueue_Enqueue(LogCommand.getInitStatusRecord(users, SysConfig, richTextBoxAddress.Text, richTextBoxTask.Text, serialCom.SerialPortIsOpen, isInternetAvailiable));

//            //创建播放日志实例
//            worklogplay = new WorkLogPlay(SysConfig.Setting.accessAccount, richTextBoxAddress.Text);

//            //服务器周期线程
//            Thread PeriodServerth = new Thread(PeriodRecordServerThread);		//周期上传线程-周期为10s
//            PeriodServerth.Name = "服务器周期线程";
//            PeriodServerth.IsBackground = true;
//            PeriodServerth.Start();

//            #endregion

//            #region 串口通信配置
//            serialCom.worklog = this.worklog;
//            //串口通信配置
//            //string comName = AppUtil.FindComByKeyStr(ComKeyStr);		//查找包含关键字的COM号
//            if (SysConfig.Setting.serialCom != null)
//            {
//                if (serialCom.ComOpen(SysConfig.Setting.serialCom, SerialBaudLUT[SysConfig.Setting.serialBaud]))//若打开串口成功
//                {
//                    //写入串口连接记录到日志文件中
//                    worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.Connect, null));
//                    serialCom.SendQueue_Enqueue(ProtocolCommand.SwitchChannelMsg(20));	//切换信道
//                    Thread.Sleep(2000);
//                    //发送主机查询命令
//                    SerialSendMsg sendMsg = ProtocolCommand.ServerQueryCmdMsg(SysConfig.getSerialNOBytes());
//                    serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
//                }
//            }
//            else
//                MessageBox.Show("打开串口失败!");
//            recvMsgThread = new Thread(RecvMsgThread);		//开启接收消息的线程
//            recvMsgThread.Name = "主界面处理串口数据线程";
//            recvMsgThread.IsBackground = true;
//            recvMsgThread.Start();

//            //串口重连线程
//            Thread serialRelinkThread = new Thread(SerialRelink);
//            serialRelinkThread.Name = "串口重连线程";
//            serialRelinkThread.IsBackground = true;
//            serialRelinkThread.Start();

//            Thread PeriodUploadth = new Thread(PeriodRecordThread);		//周期记录线程
//            PeriodUploadth.Name = "周期记录线程";
//            PeriodUploadth.IsBackground = true;
//            PeriodUploadth.Start();

//            #endregion

//            #region 串口调试配置
//            btnOpenDbg.Click += new EventHandler(btnOpenDbg_Click);
//#if DEBUG
//            btnOpenDbg.Visible = true;
//            comDebugForm.StartDebugThread(serialCom.DebugMsgQueue, serialCom.DebugQueueWaitHandle);
//            comDebugForm.Show();
//            comDebugForm.Visible = false;
//#else
//            btnOpenDbg.Visible = false;
//#endif
//            #endregion

//            #region 创建信道列表
//            //创建信道列表
//            channelDic.Add(20, -1);
//            channelDic.Add(22, -1);
//            channelDic.Add(26, -1);
//            channelDic.Add(18, -1);
//            channelDic.Add(28, -1);
//            channelDic.Add(16, -1);
//            channelDic.Add(21, -1);
//            channelDic.Add(27, -1);
//            channelDic.Add(19, -1);
//            channelDic.Add(17, -1);
//            channelDic.Add(29, -1);
//            channelDic.Add(15, -1);
//            channelDic.Add(14, -1);
//            channelDic.Add(12, -1);
//            channelDic.Add(11, -1);
//            #endregion

//            #region 网络通信配置
//            netcom.worklog = this.worklog;

//            netcom.netDelegate.myEvent += new MyDelegate(netDelegate_myEvent);

//            Thread NetRecvth = new Thread(NetPacketHandler);		//主界面处理网络接收数据线程
//            NetRecvth.Name = "主界面处理网络接收数据线程";
//            NetRecvth.IsBackground = true;
//            NetRecvth.Start();

//            Thread NetLinkTh = new Thread(NetLinkCheckHandler);		//网络检测线程
//            NetLinkTh.Name = "网络检测线程";
//            NetLinkTh.IsBackground = true;
//            NetLinkTh.Start();

//            System.Windows.Forms.Timer HeartBeatSnedTimer = new System.Windows.Forms.Timer();	//心跳包发送定时器, 定时2s
//            HeartBeatSnedTimer.Interval = 2000;
//            HeartBeatSnedTimer.Tick += new EventHandler(HeartBeatSnedTimer_Tick);
//            HeartBeatSnedTimer.Start();
//            #endregion

//            #region 声音报警线程配置
//            Thread alarmTh = new Thread(AlarmPlaySound);
//            alarmTh.Name = "声音报警线程";
//            alarmTh.IsBackground = true;
//            alarmTh.Start();
//            #endregion


//            pb_th = new Thread(PlayBackThread);
//            pb_th.Name = "回放线程";
//            pb_th.IsBackground = true;
//            pb_th.Start();

//            logMaintain.DeleteExpiredLogFiles();		//删除过期的系统日志
//            WorkLogMaintain();							//删除过期的工作日志

//            //获取UI线程同步上下文
//            m_SyncContext = SynchronizationContext.Current;


//            //详情窗口初始化
//            detailsForm = new DetailsForm();
//            detailsForm.formMain = this;
//            detailsForm.StartPosition = FormStartPosition.Manual;
//            int x1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X;
//            int y1 = panelContentMain.PointToScreen(new Point(0, 0)).Y;
//            detailsForm.Location = new Point(x1 + btnCountDownColumn1.Width / 2, y1);
//            Size sizeThis = CtrlAutoSize.GetSizeByTag(this);
//            Size sizeDetails = CtrlAutoSize.GetSizeByTag(detailsForm);
//            detailsForm.Size = new Size((int)((float)sizeDetails.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeDetails.Height / (float)sizeThis.Height * this.Height));
//            detailsForm.Show();
//            detailsForm.FormVisible = false;

//            //输入密码窗口初始化
//            enterPwdForm = new EnterPwdForm();
//            enterPwdForm.formMain = this;
//            enterPwdForm.StartPosition = FormStartPosition.Manual;
//            int x1_1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X + btnCountDownColumn1.Width * 2 / 3;
//            int y1_1 = panelUsers.PointToScreen(new Point(0, 0)).Y + panelColumnName.PointToScreen(new Point(0, 0)).Y;
//            enterPwdForm.Location = new Point(x1_1, y1_1);
//            Size sizeEnterPwd = CtrlAutoSize.GetSizeByTag(enterPwdForm);
//            enterPwdForm.Size = new Size((int)((float)sizeEnterPwd.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeEnterPwd.Height / (float)sizeThis.Height * this.Height));
//            enterPwdForm.Show();
//            enterPwdForm.FormVisible = false;

//            enterPwdForm.btnEnterPwdFormOK.Click += new EventHandler(btnEnterPwdFormOK_Click);
//            isFormLoadDone = true;		//窗口加载完毕标志
		}

		//调试按键事件
		void btnOpenDbg_Click(object sender, EventArgs e)
		{
			if (comDebugForm.Visible == false)
			{
				comDebugForm.Visible = true;
				btnOpenDbg.Text = "关闭调试";
			}
			else
			{
				comDebugForm.Visible = false;
				btnOpenDbg.Text = "打开调试";
			}
		}

		#endregion


		#region 系统功能按键点击事件 - 系统设置, 确认密码, 实时上传, 知识库, 设备库, 回放模式
		//系统设置按钮点击事件
		void btnSysSetting_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isSystemSetting)
				{
					//设置位置
					int x1_1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X + btnCountDownColumn1.Width * 2 / 3;
					int y1_1 = panelUsers.PointToScreen(new Point(0, 0)).Y + panelColumnName.Height / 2;
					enterPwdForm.Location = new Point(x1_1, y1_1);
					//设置大小
					Size sizeThis = CtrlAutoSize.GetSizeByTag(this);
					Size sizeEnterPwd = CtrlAutoSize.GetSizeByTag(enterPwdForm);
					enterPwdForm.Size = new Size((int)((float)sizeEnterPwd.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeEnterPwd.Height / (float)sizeThis.Height * this.Height));
					//显示窗口
					enterPwdForm.FormVisible = true;
					enterPwdForm.BringToFront();
				}
			}
		}

		//确认密码按钮
		void btnEnterPwdFormOK_Click(object sender, EventArgs e)
		{
			if (enterPwdForm.textBoxEnterPwdFormPwd.Text == SysConfig.Setting.systemPassword)	//若密码正确
			{
				richTextSysSetCom.Text = SysConfig.Setting.serialCom;
				comboBoxSysSetBaud.SelectedIndex = SysConfig.Setting.serialBaud;
				richTextSysSetUnit.Text = SysConfig.Setting.unitName;
				richTextSysSetServerAdd.Text = SysConfig.Setting.serverIP;
				richTextSysSetServerPort.Text = SysConfig.Setting.serverPort.ToString();
				richTextSysSetAccount.Text = SysConfig.Setting.accessAccount;
				richTextSysSetPwd.Text = SysConfig.Setting.accessPassword;
				comboBoxSysSetThres.SelectedIndex = SysConfig.Setting.alarmThreshold;
				richTextSysSetGrpNO.Text = SysConfig.Setting.groupNumber.ToString("D8");
                richTextSysSetChannal.Text = SysConfig.Setting.channal.ToString("D8");
                richTextSysSetSysPwd.Text = SysConfig.Setting.systemPassword;

				PanelSwitch(CurPanel.EpanelSysSetting);
				enterPwdForm.FormVisible = false;

				//写入按钮点击记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.LoginSuccess, null));
			}
			else
			{
				MessageBox.Show("密码错误", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		//实时上传按钮点击事件
		void btnUpLoad_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isRealTimeUploading)	//若当前没有正在上传
				{
					if (!isStartingRealUpload)	//若当前没有正在开启实时上传
					{
						if (isInternetAvailiable)	//若服务器在线
						{
                            //Thread StartRealUploadth = new Thread(StartRealUpload);		//开启实时上传线程
                            Thread StartRealUploadth = new Thread(StartRealUploadViaHttp);		//开启实时上传线程
                            StartRealUploadth.Name = "实时上传线程";
							StartRealUploadth.IsBackground = true;
							StartRealUploadth.Start();
                            netcom.SetHttpSend(true);//http新增
                            isRealTimeUploading = true;
                        }
						else
							MessageBox.Show("服务器未在线");
					}
				}
				else				//若当前正在实时上传, 则取消上传
				{
					// 1. 切换为非实时上传
					isRealTimeUploading = false;
					pictureBoxUpload.Image = Properties.Resources.UploadImage;
					btnUpLoad.Text = "实时上传";
                    // 2. 断开网络连接
                    //netcom.NetClose();
                    netcom.SetHttpSend(false);//http新增
                    //停止上传记录
                    worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.RTUpload, "3"));
					//写入网络断开TCP连接记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.TcpDisconnect, null));
				}
			}
		}

		//知识库按钮点击事件
		void btnKnowledgeBase_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				PanelSwitch(CurPanel.EpanelKnowledgeBase);
				//写入按钮点击记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.KnowledgeBase, null));
			}
		}

		//设备库按钮点击事件
		void btnDeviceBase_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				PanelSwitch(CurPanel.EpanelDeviceBase);
				//写入按钮点击记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.DeviceBase, null));
			}
		}

		//联网按钮点击事件
		void btnInternetState_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				isInternetAvailiable = AppUtil.PingServerAlive(SysConfig.Setting.serverIP, 300);	//先ping一下主机

				if (isInternetAvailiable)
				{
					btnInternetState.BackgroundImage = Properties.Resources.wifi_connected_24px;
					//联网记录
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.NetLink, "1"));
				}
				else
				{
					btnInternetState.BackgroundImage = Properties.Resources.wifi_disconnected_24px;
					//联网记录
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.NetLink, "2"));
				}
			}
		}

		//进入和退出回放模式按钮点击事件
		void btnPlayBackEnter_Click(object sender, EventArgs e)
		{
			//若当前不是处于回放模式, 则处于回放模式
			if(!isPlayBackMode)
			{
				isPlayBackMode = true;
				//切换界面
				if (panelContentMain.Visible == false)
					PanelSwitch(CurPanel.EpanelContentMain);

				btnPlayBackEnter.Text = "退出回放";
				//删除当前所有用户
				ClearUsers();
				//for (int i = users.Count - 1; i >= 0; i--)
				//    RemoveUserAt(i);
				//初始化 及 显示回放相关控件
				labelPlayBack.Visible = true;
				labelPlayBack.Text = TimeHasBeenRunning + "/" + TotalPlayTime;
				trackBarPlayBack.Value = 0;
				trackBarPlayBack.Visible = true;
				btnPlayBackPlay.Visible = true;
				btnPlayBackOpen.Visible = true;
				pictureBoxPlayBackArrow.Visible = true;
			}
			//否则退出回放模式
			else
			{
				isPlayBackMode = false;
				btnPlayBackEnter.Text = "回放模式";
				//反序列化系统配置
				SysConfig.ReadSystemSetting();
				if (SysConfig.Setting != null)
				{
					labelUnit.Text = "单位：" + SysConfig.Setting.unitName;
					labelGrpNumber.Text = "组号：" + SysConfig.Setting.groupNumber.ToString("D8");
				}
				//删除当前所有用户
				ClearUsers();
				//for (int i = users.Count - 1; i >= 0; i--)
				//    RemoveUserAt(i);
				//读取用户配置文件, 并添加用户
				if (userRW.ReadDefaultUserFile())
				{
					if (userRW.UserInfoList.Count > 0)
					{
						foreach (UserBasicInfo userInfo in userRW.UserInfoList)
						{
							if (userInfo.terminalGrpNO == SysConfig.Setting.groupNumber)	//若组号相等, 则将用户加入
								AddUser(userInfo);
						}
					}
				}
				//将播放按钮置为"播放"
				btnPlayBackPlay.BackgroundImage = Properties.Resources.Play_32x32_up;
				tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "播放");
				//隐藏回放相关控件
				labelPlayBack.Visible = false;
				trackBarPlayBack.Visible = false;
				btnPlayBackPlay.Visible = false;
				btnPlayBackOpen.Visible = false;
				pictureBoxPlayBackArrow.Visible = false;
			}
		}
		
		//播放按钮抬起事件
		void btnPlayBackPlay_MouseUp(object sender, MouseEventArgs e)
		{
			//若当前为暂停, 则改为播放
			if (!isPlaying)
			{
				isPlaying = true;
				tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "暂停");
				btnPlayBackPlay.BackgroundImage = Properties.Resources.Pause_32x32_up;
			}
			else
			{
				isPlaying = false;
				tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "播放");
				btnPlayBackPlay.BackgroundImage = Properties.Resources.Play_32x32_up;
			}
		}

		//回放打开文件按钮点击事件
		void btnPlayBackOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "CSV|*.csv";
			openFileDialog.FilterIndex = 0;
			openFileDialog.RestoreDirectory = true;				//保存对话框是否记忆上次打开的目录
			openFileDialog.Title = "导出用户配置文件到";
			openFileDialog.InitialDirectory = Application.StartupPath + @"\res\WorkLog";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (openFileDialog.FileName != null)
				{
					try
					{
						if (AppUtil.CheckCSVLogFileName(openFileDialog.FileName))		//检测文件是否符合规范
						{
							//读取文件
							LogList = CSVHelper.ReadCSV(openFileDialog.FileName);
							//更新"用户改变记录"下标列表
							UserChangeLogIndexList.Clear();
							foreach (List<string> listTmp in LogList)
							{
								if (listTmp[1] == "1" || listTmp[1] == "2")		//若是初始状态记录 或 用户更新记录, 则存入列表中
									UserChangeLogIndexList.Add(LogList.IndexOf(listTmp));
							}
							//启动回放线程
							RecordCounter = 0;
							PlayBackRecord = LogList[0];
							PlayBackStartTime = DateTime.ParseExact(PlayBackRecord[0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);
							PlayBackBaseTime = PlayBackStartTime;
							PlayBackEndTime = DateTime.ParseExact(LogList[LogList.Count - 1][0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);
							labelCurrentTime.Text = "时间：" + PlayBackBaseTime.ToString("yyyy.MM.dd  HH:mm");

							//更新进度条的最大值, 为播放时间的总秒数
							TimeSpan ts = PlayBackEndTime - PlayBackBaseTime;
							trackBarPlayBack.Maximum = (int)(ts.TotalSeconds);
							//更新总播放时间, 单位为秒
							TotalPlayTime = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
							TimeHasBeenRunning = "00:00:00";
							//更新播放时间标签
							labelPlayBack.Text = TimeHasBeenRunning + "/" + TotalPlayTime;
							//开始播放
							isPlaying = true;
							trackBarPlayBack.Enabled = true;
							tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "暂停");
							btnPlayBackPlay.BackgroundImage = Properties.Resources.Pause_32x32_up;
						}
						else
						{
							MessageBox.Show("不是标准的日志文件");
						}
					}
					catch (Exception ex) {
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
						MessageBox.Show("打开文件失败");
					}
				}
			}
		}

		//进度条鼠标按下事件
		void trackBarPlayBack_MouseDown(object sender, MouseEventArgs e)
		{
			isTrackBarDown = true;
			trackBarPlayBack.Value = (int)((double)e.X * trackBarPlayBack.Maximum / trackBarPlayBack.Width) + 1;
		}

		//进度条鼠标抬起事件
		void trackBarPlayBack_MouseUp(object sender, MouseEventArgs e)
		{
			isAdjustingPBPos = true;		//开始调整
			lock (PlayBackRecord)
			{
				//记录当前播放的记录下标号
				int RecordCounter_Cur = RecordCounter;
				//更新下一条要播放的记录的下标
				if (trackBarPlayBack.Value == 0)
					RecordCounter = 0;
				else
					RecordCounter = findFirstGreater(trackBarPlayBack.Value);	//查找第一个大于定位点的记录, 该条记录定为下一条要播放的记录
				//更新下一个要播放的记录
				PlayBackRecord = LogList[RecordCounter];
				//更新当前播放时间
				PlayBackBaseTime = PlayBackStartTime.AddSeconds(trackBarPlayBack.Value);	//更新基准时间

				//用户状态改变处理, 需要重新导入用户状态
				bool isUserChanged = false;
				if (RecordCounter_Cur > RecordCounter)			//进度条向前拖动
				{
					foreach (int index in UserChangeLogIndexList)
					{
						if (index <= RecordCounter_Cur && index >= RecordCounter)
						{
							isUserChanged = true;
							break;
						}
					}
				}
				else											//进度条往后拖动
				{
					foreach (int index in UserChangeLogIndexList)
					{
						if (index >= RecordCounter_Cur && index <= RecordCounter)
						{
							isUserChanged = true;
							break;
						}
					}
				}
				//若进度条拖动的时间内有用户状态改变
				if (isUserChanged)
				{
					int updateIndex = 0;
					//找到最近一次不是单个用户状态更新的记录下标
					for (int i = UserChangeLogIndexList.Count - 1; i >= 0; i--)
					{
						if (UserChangeLogIndexList[i] < RecordCounter)
						{
							if (LogList[UserChangeLogIndexList[i]][1] != "2" && LogList[UserChangeLogIndexList[i]][2] != "2")
							{
								updateIndex = i;
								break;
							}
						}
					}
					//一次执行用户改变的记录
					for (int i = updateIndex; i < UserChangeLogIndexList.Count; i++)
					{
						if (UserChangeLogIndexList[i] <= RecordCounter)
							ParseLogRecord(LogList[UserChangeLogIndexList[i]]);
						else
							break;
					}
				}
				
				//找到最近一次全部用户状态更新记录
				int LastestAllUserRecord = -1;
				int indextmp = RecordCounter;
				while (indextmp > 0)
				{
					if(LogList[indextmp][1] == "9")		//记录类型是否为9
					{
						LastestAllUserRecord = indextmp;
						break;
					}
					indextmp--;
				}
				//若找到
				if (indextmp > 0)
				{
					UserStatusRecordParse(LogList[indextmp]);	//先更新所有用户的状态
					while (indextmp < RecordCounter)
					{
						if (LogList[indextmp][1] == "3")		//记录类型是否为3
						{
							UserStatusRecordParse(LogList[indextmp]);	//再解析这之间的所有状态改变记录
						}
						indextmp++;
					}
				}

				isTrackBarDown = false;
			}
			isAdjustingPBPos = false;		//调整完毕
		}

		//二分查找第一个大于定位点的记录
		int findFirstGreater(int key)
		{
			int left = 0;
			int right = LogList.Count - 1;
			int mid;
			key *= 1000;
			PlayBackStartTime = DateTime.ParseExact(LogList[0][0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);
			DateTime LogTime = new DateTime();		//记录的时间

			while (left <= right) {
				mid = (left + right) / 2;
				LogTime = DateTime.ParseExact(LogList[mid][0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);	//解析记录的时间
				int delta = (int)(LogTime - PlayBackStartTime).TotalMilliseconds;
				if (key < delta) {
					right = mid - 1;
				}
				else {
					left = mid + 1;
				}
			}
			return left;
		}

		//进度条值改变事件
		void trackBarPlayBack_ValueChanged(object sender, EventArgs e)
		{
			//进度条值改变之后, 要更新已播放的时间
			TimeSpan ts = TimeSpan.FromSeconds(trackBarPlayBack.Value);
			TimeHasBeenRunning = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
			labelPlayBack.Text = TimeHasBeenRunning + "/" + TotalPlayTime;
		}
		
		#endregion


		#region 所有的用户姓名按钮事件处理函数
		/************************所有的用户姓名按钮事件处理函数***************************/
		//点击事件
		public void UserNameButtons_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);
				if (!users[uid].IsSelected)		//若用户未被选中
				{
					for (int i = 0; i < users.Count; i++)
					{
						if (i == uid)
							users[i].IsSelected = true;
						else
							users[i].IsSelected = false;
					}
					//写入用户选中记录
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.UserSelect, BitConverter.ToString(AppUtil.IntSerialToBytes(users[uid].BasicInfo.terminalGrpNO, users[uid].BasicInfo.terminalNO), 0).Replace("-", string.Empty)));
				}
			}
		}
		//鼠标进入事件
		public void UserNameButtons_MouseEnter(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);
				if (!users[uid].IsSelected)
					btn.FlatAppearance.BorderSize = 1;
			}
		}
		//鼠标移出事件
		public void UserNameButtons_MouseLeave(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);
				if (!users[uid].IsSelected)
					btn.FlatAppearance.BorderSize = 0;
			}
		}
		#endregion


		#region 所有的用户详情按钮事件处理函数
		/************************所有的用户详情按钮事件处理函数***************************/
		//点击事件
		public void DetailsButtons_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);

				//设置位置
				int x1 = btnCountDownColumn1.PointToScreen(new Point(0, 0)).X;
				int y1 = panelContentMain.PointToScreen(new Point(0, 0)).Y;
				detailsForm.Location = new Point(x1 + btnCountDownColumn1.Width / 2, y1);
				//设置大小
				Size sizeThis = CtrlAutoSize.GetSizeByTag(this);
				Size sizeDetails = CtrlAutoSize.GetSizeByTag(detailsForm);
				detailsForm.Size = new Size((int)((float)sizeDetails.Width / (float)sizeThis.Width * this.Width), (int)((float)sizeDetails.Height / (float)sizeThis.Height * this.Height));
				//显示窗口
				if (detailsForm.CurUser != null)
				{
					if (!(detailsForm.CurUser.BasicInfo.terminalGrpNO == users[uid].BasicInfo.terminalGrpNO &&		//若显示的不是当前用户, 则调整用户
					detailsForm.CurUser.BasicInfo.terminalNO == users[uid].BasicInfo.terminalNO &&
					detailsForm.CurUser.BasicInfo.name == users[uid].BasicInfo.name))
						detailsForm.SetUser(users[uid]);
				}
				else detailsForm.SetUser(users[uid]);
				detailsForm.FormVisible = true;
				detailsForm.BringToFront();
			}
		}
		//鼠标进入事件
		public void UserDetailsButtons_MouseEnter(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);
				if (!users[uid].IsSelected)
					btn.FlatAppearance.BorderSize = 1;
			}
		}
		//鼠标移出事件
		public void UserDetailsButtons_MouseLeave(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				Button btn = (Button)sender;
				int uid = AppUtil.getUidByControlName(btn.Name);
				if (!users[uid].IsSelected)
					btn.FlatAppearance.BorderSize = 0;
			}
		}
		#endregion


		#region 5个功能按键点击事件
		//用户撤出按钮点击事件
		void btnUserEvacuate_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isAllUserEvacuating)	//若没有正在执行全部撤出操作
				{
					if (serialCom.SerialPortIsOpen)	//若串口是打开着的
					{
						byte[] serialNO = null;
						//遍历用户找出当前被选中的, 且不处于"关机"或"撤出中"的终端
						foreach (User user in users)
						{
							//if (user.IsSelected && (user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.RetreatingStatus))
							if (user.IsSelected && (user.UStatus != USERSTATUS.PowerOffStatus))
							{
								serialNO = AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO);
								break;
							}
						}
						//若找到终端, 则发送撤出命令
						if (serialNO != null)
						{
							SerialSendMsg sendMsg = ProtocolCommand.RemotePlaySoundCmdMsg(0x09, serialNO, 3, 1000);	//终端撤出命令, 发送3次, 最大等待时间为1000ms
							serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
							//写入按钮点击记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.UserEvacuate, BitConverter.ToString(serialNO, 0).Replace("-", string.Empty)));
						}
					}
				}
			}
		}
		//全部撤出按钮点击事件
		void btnAllUserEvacuate_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isAllUserEvacuating)	//若没有正在执行全部撤出操作
				{
					if (serialCom.SerialPortIsOpen)	//若串口是打开着的
					{
						Thread thAllEvacuate = new Thread(AllUserEvacuate_Thread);
						thAllEvacuate.Name = "全部撤出线程";
						thAllEvacuate.IsBackground = true;
						AllUserEvacuateQueue.Clear();	//清除队列
						isAllUserEvacuating = true;		//正在进行全部撤出操作
						thAllEvacuate.Start();
						//写入按钮点击记录到日志文件中
						worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.AllUserEvacuate, null));
					}
				}
			}
		}
		//停止报警按钮点击事件
		void btnStopAlarm_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				User userSelected = null;
				//遍历用户找出当前被选中的, 且不处于"关机"或"不存在"的终端
				foreach (User user in users)
				{
					if (user.IsSelected && (user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.NoExistStatus))
					{
						userSelected = user;
						break;
					}
				}
				//若找到用户
				if (userSelected != null)
				{
					if (userSelected.IsPlayingAlarm)	//若当前选中用户正在报警, 则清除正在报警标志
					{
						userSelected.AlarmFlagForExceedTh = false;
						userSelected.AlarmFlagForLost = false;
						userSelected.AlarmFlagForRetreat = false;
						userSelected.IsPlayingAlarm = false;
					}
				}

				//写入按钮点击记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.StopAlarm, null));
				//netcom.NetSendQueue_Enqueue(NetCommand.NetGetFileURIPacket(1));
			}
		}
		//用户刷新按钮点击事件
		void btnUserUpdate_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isAllUserUpdating)	//若没有正在执行全部刷新操作
				{
					if (serialCom.SerialPortIsOpen)	//若串口是打开着的
					{
						byte[] serialNO = null;
						//遍历用户找出当前被选中的, 且不处于"关机"或"撤出中"的终端
						foreach (User user in users)
						{
							if (user.IsSelected && (user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.RetreatingStatus))
							{
								serialNO = AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO);
								break;
							}
						}
						//若找到终端, 则发送刷新命令
						if (serialNO != null)
						{
							SerialSendMsg sendMsg = ProtocolCommand.ServerQueryTerminalCmdMsg(serialNO, 3, 1000);	//主机查询终端命令, 发送3次, 最大等待时间为1000ms
							serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
							//写入按钮点击记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.UserUpdate, BitConverter.ToString(serialNO, 0).Replace("-", string.Empty)));
						}
					}
				}
			}
		}
		//全部刷新按钮点击事件
		void btnAllUserUpdate_Click(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				if (!isAllUserUpdating)	//若没有正在执行全部刷新操作
				{
					if (serialCom.SerialPortIsOpen)	//若串口是打开着的
					{
						Thread thAllUpdate = new Thread(AllUserUpdate_Thread);
						thAllUpdate.Name = "全部更新线程";
						thAllUpdate.IsBackground = true;
						AllUserUpdateQueue.Clear();	//清除队列
						isAllUserUpdating = true;		//正在进行全部刷新操作
						thAllUpdate.Start();
						//写入按钮点击记录到日志文件中
						worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.AllUserUpdate, null));
					}
				}
			}
		}

        //用户撤出复位按钮点击事件
        void btnResetEvacuate_Click(object sender, EventArgs e)
        {
            if (!isPlayBackMode)    //若不是回放模式
            {
                if (!isAllUserEvacuating)   //若没有正在执行全部撤出操作
                {

                        byte[] serialNO = null;
                        //遍历用户找出当前被选中的, 且不处于"关机"或"撤出中"的终端
                        foreach (User user in users)
                        {
                            //if (user.IsSelected && (user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.RetreatingStatus))
                            if (user.IsSelected && (user.UStatus == USERSTATUS.RetreatingStatus|| user.UStatus == USERSTATUS.RetreatFailStatus))
                            {//若找到终端, 则修改状态
                            if (user.TerminalInfo.Pressure > 10)             //安全状态
                                user.UStatus = USERSTATUS.SafeStatus;
                            else if (user.TerminalInfo.Pressure > 6)         //轻度危险
                                user.UStatus = USERSTATUS.MildDangerousStatus;
                            else
                                user.UStatus = USERSTATUS.DangerousStatus;


                            serialNO = AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO);
                            worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.ResetEvacuate, BitConverter.ToString(serialNO, 0).Replace("-", string.Empty)));
                        }
                        }
                        
         
              
                }
            }
        }
        #endregion


        #region 键盘事件处理函数
        //键盘事件处理函数-普通按键
        void FormMain_KeyDown(object sender, KeyEventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				//Point controlPosInScreen;
				Keys key = e.KeyCode;
				switch (key)
				{
					case Keys.F1:		//F1按键, 用户撤出
						//controlPosInScreen = btnUserEvacuate.PointToScreen(new Point(btnUserEvacuate.Size.Width / 2, btnUserEvacuate.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						btnUserEvacuate.PerformClick();
						break;
					case Keys.F2:		//F2按键, 全部撤出
						//controlPosInScreen = btnAllUserEvacuate.PointToScreen(new Point(btnAllUserEvacuate.Size.Width / 2, btnAllUserEvacuate.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						btnAllUserEvacuate.PerformClick();
						break;
					case Keys.F3:		//F3按键, 用户刷新
						//controlPosInScreen = btnUserUpdate.PointToScreen(new Point(btnUserUpdate.Size.Width / 2, btnUserUpdate.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						btnUserUpdate.PerformClick();
						break;
					case Keys.F4:		//F4按键, 全部刷新
						//controlPosInScreen = btnAllUserUpdate.PointToScreen(new Point(btnAllUserUpdate.Size.Width / 2, btnAllUserUpdate.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						btnAllUserUpdate.PerformClick();
						break;
					case Keys.F5:		//F5按键, 查看详情
						//controlPosInScreen = btnStopAlarm.PointToScreen(new Point(btnStopAlarm.Size.Width / 2, btnStopAlarm.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						foreach (User user in users)
						{
							if (user.IsSelected)
							{
								user.PUserView.BtnDetails.PerformClick();
								break;
							}
						}
						break;
					case Keys.F6:		//F6按键, 退出查看详情
						//controlPosInScreen = btnStopAlarm.PointToScreen(new Point(btnStopAlarm.Size.Width / 2, btnStopAlarm.Size.Height / 2));
						//Win32APICall.SimulateMouseClick(controlPosInScreen);
						detailsForm.FormVisible = false;
						this.Focus();
						break;
                    case Keys.F12:       //F12按键, 撤出恢复
                        btnResetEvacuate.PerformClick();
                        break;
                     

                    case Keys.Escape:	//Esc按键, 停止报警
						btnStopAlarm.PerformClick();
						break;

					default:
						break;
				}
			}
		}
		//键盘事件处理函数-方向按键
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				switch (keyData)
				{
					case Keys.Up:		//方向键-上↑

						break;
					case Keys.Down:		//方向键-下↓

						break;
					case Keys.Left:		//方向键-左←

						break;
					case Keys.Right:	//方向键-右→

						break;
					case Keys.Escape:	//Esc按键, 停止报警
						break;
					case Keys.Enter:	//Enter按键
						Control con = Win32APICall.GetFocusedControl();
						if (con != null)
						{
							if (con is Button)
							{
								Button btnSelected = con as Button;
								btnSelected.PerformClick();
							}
						}
						break;
					default:
						break;
				}
			}
			return false;
		}
		#endregion


		#region 系统功能函数

		//心跳包发送函数
		void HeartBeatSnedTimer_Tick(object sender, EventArgs e)
		{
			if (isRealTimeUploading)	//若需要实时上传
			{
				if ((DateTime.Now - netcom.LatestSendTime).TotalSeconds > 20)			//若超过20s没有发送数据包给主机
					netcom.NetSendQueue_Enqueue(NetCommand.NetHeartBeatPacket());
			}
		}

		//地点栏内容改变事件
		static private string richTextBoxAddress_text;
		void richTextBoxAddress_LostFocus(object sender, EventArgs e)
		{
			//if (!isPlayBackMode)	//若不是回放模式
			//{
			//    //写入修改地点记录到日志文件中
			//    if (richTextBoxAddress_text != richTextBoxAddress.Text)		//若内容确实改变了
			//    {
			//        //上传到服务器
			//        if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
			//        {
			//            netcom.NetSendQueue_Enqueue(NetCommand.NetAddChangePacket(richTextBoxAddress.Text));
			//        }

			//        //写入工作日志
			//        worklog.LogQueue_Enqueue(LogCommand.getChangeAddRecord(richTextBoxAddress.Text));
			//        //写入回放工作日志
			//        worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getChangeAddRecord(richTextBoxAddress.Text));
			//        richTextBoxAddress_text = richTextBoxAddress.Text;
			//    }
			//}
		}
		void richTextBoxAddress_TextChanged(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				//写入修改地点记录到日志文件中
				if (richTextBoxAddress_text != richTextBoxAddress.Text)		//若内容确实改变了
				{
					//上传到服务器
					if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
					{
						netcom.NetSendQueue_Enqueue(NetCommand.NetAddChangePacket(richTextBoxAddress.Text));
					}

					//写入工作日志
					worklog.LogQueue_Enqueue(LogCommand.getChangeAddRecord(richTextBoxAddress.Text));
					//写入回放工作日志
					worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getChangeAddRecord(richTextBoxAddress.Text));
					richTextBoxAddress_text = richTextBoxAddress.Text;
				}
			}
		}
		//任务栏内容改变事件
		static private string richTextBoxTask_text;
		void richTextBoxTask_LostFocus(object sender, EventArgs e)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				//写入修改任务记录到日志文件中
				if (richTextBoxTask_text != richTextBoxTask.Text)			//若内容确实改变了
				{
					//上传到服务器
					if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
					{
						netcom.NetSendQueue_Enqueue(NetCommand.NetTaskChangePacket(richTextBoxTask.Text));
					}

					//写入工作日志
					worklog.LogQueue_Enqueue(LogCommand.getChangeTaskRecord(richTextBoxTask.Text));
					//写入回放工作日志
					worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getChangeTaskRecord(richTextBoxTask.Text));
					richTextBoxTask_text = richTextBoxTask.Text;
				}
			}
		}
		void richTextBoxTask_TextChanged(object sender, EventArgs e)
		{
			//if (!isPlayBackMode)	//若不是回放模式
			//{
			//    //写入修改任务记录到日志文件中
			//    if (richTextBoxTask_text != richTextBoxTask.Text)			//若内容确实改变了
			//    {
			//        //上传到服务器
			//        if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
			//        {
			//            netcom.NetSendQueue_Enqueue(NetCommand.NetTaskChangePacket(richTextBoxTask.Text));
			//        }

			//        //写入工作日志
			//        worklog.LogQueue_Enqueue(LogCommand.getChangeTaskRecord(richTextBoxTask.Text));
			//        //写入回放工作日志
			//        worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getChangeTaskRecord(richTextBoxTask.Text));
			//        richTextBoxTask_text = richTextBoxTask.Text;
			//    }
			//}
		}

		//自适应窗口大小函数
		public void Form_Resize(object sender, EventArgs e)
		{
			autosize.resizeControl(this);	//在resize消息里调用此函数以自动设置窗口控件大小和位置
		}

		//串口重连线程
		private void SerialRelink()
		{
			while (true)
			{
				if (!isPlayBackMode)	//若不是回放模式
				{
					if (isSerialShouldOpen)
					{
						if (!serialCom.ComDevice.IsOpen)		//若串口未打开
						{
							//string comName = AppUtil.FindComByKeyStr(ComKeyStr);		//查找包含关键字的COM号
							if (SysConfig.Setting.serialCom != null)
							{
								if (serialCom.ComOpen(SysConfig.Setting.serialCom, SerialBaudLUT[SysConfig.Setting.serialBaud]))		//若打开串口成功
								{
									//写入串口连接记录到日志文件中
									worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.Connect, null));
									if (false)			//若还没有确定角色ServerRole ==
									{
										SerialSendMsg sendMsg = ProtocolCommand.ServerQueryCmdMsg(SysConfig.getSerialNOBytes());	//发送主机查询命令
										serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
									}
								}
							}
						}
					}
					
				}
				Thread.Sleep(2000);
			}
		}

		//定时器
		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!isPlayBackMode)
				labelCurrentTime.Text = "时间：" + DateTime.Now.ToString("yyyy.MM.dd  HH:mm");
		}

		//panelUsers重绘事件----始终隐藏水平滚动条
		void panelUsers_Paint(object sender, PaintEventArgs e)
		{
			Win32APICall.ShowScrollBar(((Control)sender).Handle, 0, 0);
		}

		//panelUsers点击事件----获取焦点
		void panelUsers_Click(object sender, EventArgs e)
		{
			panelUsers.Focus();
		}

		//退出按钮响应
		private void btnExit_Click(object sender, EventArgs e)
		{
			//System.Environment.Exit(0); //退出程序
			//DialogResult dr = MessageBox.Show("是否在退出程序后自动关机?", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			DialogResult dr = (new MessageBoxEX("提示", "是否在退出程序后自动关机?", MessageBoxIcon.Question, MessageBoxButtons.YesNoCancel)).ShowDialog();
			if (dr == DialogResult.Yes)
			{
				//Process.Start("shutdown.exe", "-l");//注销
				Process.Start("shutdown.exe", "-s -t 0");		//关机
				FormMain_FormClosing(null, null);
			}
			else if (dr == DialogResult.No)
			{
				FormMain_FormClosing(null, null);
			}
			
		}

		//窗口关闭按钮响应
		private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			//写入按钮点击记录到日志文件中
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.ProgramExit, null));
			System.Environment.Exit(0); //退出程序
		}
        #endregion

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelSysSetGrpNO_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

}

