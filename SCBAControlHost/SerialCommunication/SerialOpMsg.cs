using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCBAControlHost.SerialCommunication
{
	//enum SerialOpCodEnum
	//{
	//    SerialDevNoExist,			//串口不存在
	//    ParaSetup1,					//参数设置1
	//    ParaSetup2,					//参数设置2
	//    ServerQuery,				//主机查询
	//    TerminalPowerOn,			//终端开机
	//    TerminalSwitch,				//终端切换
	//    RemotePlaySound,			//远程播报
	//    TerminalUpload,				//终端上传
	//    ServerQueryTerminal,		//主机查询终端
	//    ServerSwitch,				//主机切换
	//    BuildTeam,					//组队
	//    TerminalShutdown			//终端关机
	//};

	//enum SerialOpResEnum
	//{
	//    SUCCESS,
	//    FAILED,
	//    UNKNOWN
	//};

	class SerialOpMsg
	{
		private int operationId;					//操作id
		public int OperationId
		{
			get { return operationId; }
			set { operationId = value; }
		}

		//private SerialOpCodEnum serialOpCod;		//操作码
		//internal SerialOpCodEnum SerialOpCod
		//{
		//    get { return serialOpCod; }
		//    set { serialOpCod = value; }
		//}

		private SerialOpResEnum serialOpRes;		//操作结果
		internal SerialOpResEnum SerialOpRes
		{
			get { return serialOpRes; }
			set { serialOpRes = value; }
		}

		#region 发送参数
		/*-----------------------------------------发送参数-------------------------------------------*/
		private int sendRetryNum;					//发送重试次数
		public int SendRetryNum
		{
			get { return sendRetryNum; }
			set { sendRetryNum = value; }
		}

		private int sendWaitTime;					//发送重试等待时间
		public int SendWaitTime
		{
			get { return sendWaitTime; }
			set { sendWaitTime = value; }
		}

		private DateTime sendTime;					//发送时间
		public DateTime SendTime
		{
			get { return sendTime; }
			set { sendTime = value; }
		}
		/*********************************************************************************************/
		#endregion


		#region 接收参数
		/*-----------------------------------------接收参数-------------------------------------------*/
		private DateTime recvTime;					//接收接收时间
		public DateTime RecvTime
		{
			get { return recvTime; }
			set { recvTime = value; }
		}
		/*********************************************************************************************/
		#endregion
		
	}
}
