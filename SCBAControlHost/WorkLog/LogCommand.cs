using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCBAControlHost.SysConfig;
using SCBAControlHost.SerialCommunication;
using SCBAControlHost.NetCommunication;
using MyUtils;

namespace SCBAControlHost
{
	public enum BTNPANEL : int　　//按钮面板编号
	{
		MainPanel = 1,			//主界面面板
		SysSettingPanel = 2,	//系统设置面板
		InfoSyncPanel = 3,		//信息同步面板
		TempGrpPanel = 4,		//临时编组面板
		UserChangeNOPanel = 5,	//用户改号面板
		KnowledgeBasePanel = 6,	//知识库面板
		DeviceBasePanel = 7,	//器件库面板
		CheckUserPanel = 8		//用户列表面板
	};

	public enum BtnOfMainPanel : int　　//主界面面板按钮的编号
	{
		UserEvacuate = 1,		//用户撤出
		AllUserEvacuate = 2,	//全部撤出
		StopAlarm = 3,			//停止报警
		UserUpdate = 4,			//用户更新
		AllUserUpdate = 5,		//全部更新
		LoginSuccess = 6,		//登录成功
		KnowledgeBase = 7,		//知识库
		DeviceBase = 8,			//器件库
		RTUpload = 9,			//实时上传
		NetLink = 10,			//网络连接
		UserSelect = 11,		//选中用户
		ProgramExit = 12		//退出程序
	};

	public enum BtnOfSysSettingPanel : int　　//系统设置面板按钮的编号
	{
		ChangeUnit = 1,			//修改单位
		ChangeServerIP = 2,		//修改服务器IP
		ChangeServerPort = 3,	//修改服务器端口
		ChangeAccount = 4,		//修改账户
		ChangePasswd = 5,		//修改密码
		ChangeThreshold = 6,	//修改报警点
		ChangeGrpNO = 7,		//修改组号
		ChangeSysPwd = 8,		//修改系统密码
		InfoSync = 9,			//信息同步
		ImportUsers = 10,		//导入用户
		ExportFiles = 11,		//导出文件
		TempGrp = 12,			//临时编组
		UserChangeNO = 13,		//用户改号
		SysSettingReturn = 14,	//系统设置返回
		CheckUser = 15			//查看用户列表
	};

	public enum BtnOfInfoSyncPanel : int		//信息同步面板按钮的编号
	{
		StartSync = 1,			//开始同步
		InfoSyncReturn = 2		//返回
	};

	public enum BtnOfTempGrpPanel : int			//临时编组面板按钮的编号
	{
		StartTempGrp = 1,		//开始临时编组
		TempGrpReturn = 2		//返回
	};

	public enum BtnOfUserChangeNOPanel : int	//用户改号面板按钮的编号
	{
		StartChangeNO = 1,		//开始临时编组
		ChangeNOReturn = 2		//返回
	};

	public enum BtnOfKnowledgeBase : int		//知识库面板按钮的编号
	{
		AddBtn = 1,				//添加
		DeleteBtn = 2,			//删除
		CheckBtn = 3,			//查看
		KnowledgeBaseReturn = 4	//返回
	};


	public enum BtnOfDeviceBasePanel : int		//器件库面板按钮的编号
	{
		AddBtn = 1,				//添加
		DeleteBtn = 2,			//删除
		CheckBtn = 3,			//查看
		DeviceBaseReturn = 4	//返回
	};

	public enum BtnOfCheckUserPanel : int		//器件库面板按钮的编号
	{
		CheckUserReturn = 1	//返回
	};

	public enum SerialRecordType : int			//串口记录类型
	{
		Connect = 1,			//连接
		Disconnect = 2,			//断开连接
		SerialSend = 3,			//发送数据
		SerialRecv = 4,			//接收数据
		SerialTimeOut = 5		//超时数据
	};

	public enum NetRecordType : int				//网络记录类型
	{
		Connect = 1,				//连接
		Disconnect = 2,				//断开连接
		NetSend = 3,				//发送数据
		NetRecv = 4,				//接收数据
		NetDownloadFile = 5,		//下载一个文件
		NetUploadFile = 6,			//上传一个文件
		NetDownloadFileFail = 7,	//下载一个文件失败
		NetUploadFileFail = 8,		//上传一个文件失败
		TcpConnect = 9,				//连接TCP服务器
		TcpDisconnect = 10,			//断开连接TCP服务器
		Login = 11,					//登录Web服务器
		Logout = 12,                //退出登录Web服务器
        NetSendViaHttp = 13,                //发送数据HTTP
    };

	public class LogCommand
	{
		//获取初始状态记录
		public static List<string> getInitStatusRecord(List<User> users, SystemConfig sysConfig, string address, string task, bool SerialStatus, bool NetStatus)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("1");					//类型
			rowStr.Add(""+users.Count);			//用户个数
			foreach (User user in users)
			{
				rowStr.Add("" + user.BasicInfo.userNO);				//编号
				rowStr.Add("" + user.BasicInfo.name);				//姓名
				rowStr.Add("" + user.BasicInfo.birthDate);			//出生年月
				rowStr.Add("" + user.BasicInfo.uAffiliatedUnit);	//所属单位
				rowStr.Add("" + user.BasicInfo.userPhoto);			//照片路径
				rowStr.Add("" + user.BasicInfo.duty);				//职务
				rowStr.Add("" + user.BasicInfo.terminalGrpNO);		//组号
				rowStr.Add("" + user.BasicInfo.terminalNO);			//终端号
				rowStr.Add("" + user.BasicInfo.terminalCapSpec);	//气瓶容量
			}

			rowStr.Add("" + sysConfig.Setting.unitName);			//单位名称
			rowStr.Add("" + sysConfig.Setting.serverIP);			//服务器IP
			rowStr.Add("" + sysConfig.Setting.serverPort);			//服务器端口
			rowStr.Add("" + sysConfig.Setting.accessAccount);		//用户账号
			rowStr.Add("" + sysConfig.Setting.accessPassword);		//用户密码
			rowStr.Add("" + sysConfig.Setting.alarmThreshold);		//报警点
			rowStr.Add("" + sysConfig.Setting.groupNumber);			//组号
			rowStr.Add("" + sysConfig.Setting.systemPassword);		//系统密码

			rowStr.Add(address);									//地址
			rowStr.Add(task);										//任务

			if (SerialStatus) rowStr.Add("1");						//串口状态
			else rowStr.Add("2");
			if (NetStatus) rowStr.Add("1");							//网络状态
			else rowStr.Add("2");

			return rowStr;
		}

		//获取用户更新记录
		public static List<string> getUserUpdateRecord(int type, List<User> users, User m_user)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("2");					//类型
			rowStr.Add(""+type);				//更新类型
			if (type == 1)
			{
				rowStr.Add("" + users.Count);			//用户个数
				foreach (User user in users)
				{
					rowStr.Add("" + user.BasicInfo.userNO);				//编号
					rowStr.Add("" + user.BasicInfo.name);				//姓名
					rowStr.Add("" + user.BasicInfo.birthDate);			//出生年月
					rowStr.Add("" + user.BasicInfo.uAffiliatedUnit);	//所属单位
					rowStr.Add("" + user.BasicInfo.userPhoto);			//照片路径
					rowStr.Add("" + user.BasicInfo.duty);				//职务
					rowStr.Add("" + user.BasicInfo.terminalGrpNO);		//组号
					rowStr.Add("" + user.BasicInfo.terminalNO);			//终端号
					rowStr.Add("" + user.BasicInfo.terminalCapSpec);	//气瓶容量
				}
			}
			else if (type == 2)
			{
				rowStr.Add("" + m_user.BasicInfo.userNO);				//编号
				rowStr.Add("" + m_user.BasicInfo.name);				//姓名
				rowStr.Add("" + m_user.BasicInfo.birthDate);			//出生年月
				rowStr.Add("" + m_user.BasicInfo.uAffiliatedUnit);	//所属单位
				rowStr.Add("" + m_user.BasicInfo.userPhoto);			//照片路径
				rowStr.Add("" + m_user.BasicInfo.duty);				//职务
				rowStr.Add("" + m_user.BasicInfo.terminalGrpNO);		//组号
				rowStr.Add("" + m_user.BasicInfo.terminalNO);			//终端号
				rowStr.Add("" + m_user.BasicInfo.terminalCapSpec);	//气瓶容量
			}
			else if (type == 3)
			{

			}

			return rowStr;
		}

		//获取用户状态记录--一组用户
		public static List<string> getUserStatusRecord(List<User> users)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("3");					//类型
			int i = 0;
			foreach (User user in users)
			{
				rowStr.Add(i.ToString("X2") + GenTerminalInfo(user));	//序列号
				i++;
			}

			return rowStr;
		}
		//获取用户状态记录--单个用户
		public static List<string> getUserStatusRecord(User user)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("3");					//类型
			rowStr.Add("00" + GenTerminalInfo(user));

			return rowStr;
		}

		//获取按钮点击记录
		public static List<string> getButtonClickRecord(BTNPANEL BtnPanel, int BtnNo, string addionData)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("4");					//类型
			rowStr.Add("" + (int)BtnPanel);		//面板编号
			rowStr.Add("" + BtnNo);				//按钮编号
			if (addionData != null)
			{
				rowStr.Add(addionData);			//附加数据
			}

			return rowStr;
		}

		//获取串口记录
		public static List<string> getSerialRecord(SerialRecordType type, object objMsg)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("5");					//类型
			rowStr.Add(""+(int)type);			//串口记录类型
			string tmp = null;

			if (type == SerialRecordType.SerialSend)
			{
				if (objMsg is SerialSendMsg)		//若是发送消息
				{
					SerialSendMsg sendMsg = objMsg as SerialSendMsg;
					tmp = BitConverter.ToString(sendMsg.PacketData.DataBytes, 0).Replace("-", " ");
					rowStr.Add(tmp);				//附加数据
				}
				
			}
			else if (type == SerialRecordType.SerialRecv || type == SerialRecordType.SerialTimeOut)
			{
				if (objMsg is SerialRecvMsg)	//若是接收消息
				{
					SerialRecvMsg recvMsg = objMsg as SerialRecvMsg;
					tmp = BitConverter.ToString(recvMsg.PacketData.DataBytes, 0).Replace("-", " ");
					rowStr.Add(tmp);				//附加数据
				}
			}

			return rowStr;
		}

		//获取网络记录
		public static List<string> getNetRecord(NetRecordType type, object addtionData)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("6");					//类型
			rowStr.Add("" + (int)type);				//系统设置类型
			string tmp = null;
			if (type == NetRecordType.NetSend || type == NetRecordType.NetRecv)		//若为发送或接收数据, 则附加数据为数据包
			{
				if (addtionData is NetPacket)
				{
					NetPacket packet = addtionData as NetPacket;
					tmp = BitConverter.ToString(packet.DataBytes, 0).Replace("-", " ");
					rowStr.Add(tmp);
				}
				
			}
			else if (type == NetRecordType.NetDownloadFile || type == NetRecordType.NetUploadFile)	//若为下载或上传文件, 则附加数据为文件路径
			{
				if (addtionData is string)
				{
					tmp = addtionData as string;
					rowStr.Add(tmp);
				}
			}
			else if (type == NetRecordType.TcpConnect || type == NetRecordType.Login)	//若为连接TCP或登录Web, 则附加数据为字符串
			{
				if (addtionData is string)
				{
					tmp = addtionData as string;
					rowStr.Add(tmp);
				}
			}

			return rowStr;
		}

		//获取修改地点记录
		public static List<string> getChangeAddRecord(string addr)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("7");					//类型
			rowStr.Add(addr);					//地点

			return rowStr;
		}

		//获取修改任务记录
		public static List<string> getChangeTaskRecord(string task)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("8");					//类型
			rowStr.Add(task);					//任务

			return rowStr;
		}

		//获取所有用户状态记录--可用于回放时的定位
		public static List<string> getAllUserStatusRecord(List<User> users)
		{
			List<string> rowStr = new List<string>();
			rowStr.Add(" ");					//时间
			rowStr.Add("9");					//类型
			int i = 0;
			foreach (User user in users)
			{
				rowStr.Add(i.ToString("X2") + GenTerminalInfo(user));
				i++;
			}

			return rowStr;
		}

		//根据特定用户生成终端信息
		public static string GenTerminalInfo(User user)
		{
			StringBuilder info = new StringBuilder("");

			//终端序列号
			info.Append(user.BasicInfo.terminalGrpNO.ToString("X6") + user.BasicInfo.terminalNO.ToString("X2"));

			//气压
			info.Append(AppUtil.hexByteToString(User.GetPressBytesByDouble(user.TerminalInfo.Pressure)));

			//电压
			info.Append(AppUtil.hexByteToString(User.GetVoltageBytesByDouble(user.TerminalInfo.Voltage)));

			//温度
			info.Append(User.GetTemeratureByteByInt(user.TerminalInfo.Temperature).ToString("X2"));

			//开机时间
			info.Append(AppUtil.hexByteToString(User.GetTimeBytesByInt(user.TerminalInfo.PowerONTime)));

			//预留
			info.Append("000000");

			//状态
			info.Append(((byte)(user.UStatus)).ToString("X2"));

			//剩余时间
			info.Append(AppUtil.hexByteToString(User.GetTimeBytesByInt(user.TerminalInfo.RemainTime)));

			return info.ToString();
		}
	}
}
