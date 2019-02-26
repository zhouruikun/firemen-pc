using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SerialComPack;

namespace SCBAControlHost.SerialCommunication
{
	enum SerialOpResEnum
	{
		SUCCESS,
		FAILED,
		UNKNOWN
	};

	enum SerialMsgType
	{
		Send,
		Recv
	};

	//串口发送消息
	public class SerialSendMsg
	{
		private SerialDataPacket packetData;		//数据包内容
		public SerialDataPacket PacketData
		{
			get { return packetData; }
			set { packetData = value; }
		}

		private int packetDataLength;				//数据包有效长度
		public int PacketDataLength
		{
			get { return packetDataLength; }
			set { packetDataLength = value; }
		}

		private int packNumPerCmd;					//一次命令发送的包的个数
		public int PackNumPerCmd
		{
			get { return packNumPerCmd; }
			set { packNumPerCmd = value; }
		}

		private bool isWaitAck;						//该发送消息是否等待响应
		public bool IsWaitAck
		{
			get { return isWaitAck; }
			set {
				if (value == false) 
					this.sendNumMax = 1; 
				isWaitAck = value;
			}
		}

		private int sendNumMax;						//最大发送次数(等待响应时有效)
		public int SendNumMax
		{
			get { return sendNumMax; }
			set { sendNumMax = value; }
		}

		private int sendWaitTime;					//发送重试等待时间(等待响应时有效)
		public int SendWaitTime
		{
			get { return sendWaitTime; }
			set { sendWaitTime = value; }
		}

		private DateTime sendTime;					//发送时间(等待响应时有效)
		public DateTime SendTime
		{
			get { return sendTime; }
			set { sendTime = value; }
		}

		//构造函数
		public SerialSendMsg()
		{
			packetData = new SerialDataPacket();
			packNumPerCmd = 1;
			packetDataLength = 22;
		}

	}

	//串口接收消息
	public class SerialRecvMsg
	{
		private SerialDataPacket packetData;		//数据包内容
		public SerialDataPacket PacketData
		{
			get { return packetData; }
			set { packetData = value; }
		}

		private int packetDataLength;				//数据包有效长度
		public int PacketDataLength
		{
			get { return packetDataLength; }
			set { packetDataLength = value; }
		}

		private bool isFromExtern;					//从外部接收得到
		public bool IsFromExtern
		{
			get { return isFromExtern; }
			set { isFromExtern = value; }
		}

		private DateTime recvTime;					//接收时间
		public DateTime RecvTime
		{
			get { return recvTime; }
			set { recvTime = value; }
		}

		//构造函数
		public SerialRecvMsg()
		{
			packetData = new SerialDataPacket();
			packetDataLength = 22;
		}
	}
}
