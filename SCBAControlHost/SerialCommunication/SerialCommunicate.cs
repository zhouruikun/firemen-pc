using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using MyUtils;
using log4net;
using SerialComPack;

namespace SCBAControlHost.SerialCommunication
{
	enum RECVPACKSTATE
	{
		IdleState,
		StartFieldState,
		DataPacketState
	};

	public partial class SerialCommunicate
	{
		#region 成员变量
		/*-----------------------------------------成员变量-------------------------------------------*/

		//发送消息队列部分
		private Queue<SerialSendMsg> serialSendQueue;							//发送消息队列
		public Queue<SerialSendMsg> SerialSendQueue
		{
			get { return serialSendQueue; }
			set { serialSendQueue = value; }
		}
		public AutoResetEvent SendQueueWaitHandle = new AutoResetEvent(false);	//发送消息队列等待标志

		//重发消息字典部分
		private Dictionary<string, SerialSendMsg> resendDic;					//重发消息字典, 泛型为:<包类型唯一标识, 发送消息>

		//发送消息队列部分
		private Queue<SerialRecvMsg> serialRecvQueue;							//接受消息队列
		public AutoResetEvent RecvQueueWaitHandle = new AutoResetEvent(false);	//发送消息队列等待标志
		public Queue<SerialRecvMsg> SerialRecvQueue
		{
			get { return serialRecvQueue; }
			set { serialRecvQueue = value; }
		}

		//调试队列部分
		public Queue<object> DebugMsgQueue;										//调试所用的消息队列
		public AutoResetEvent DebugQueueWaitHandle = new AutoResetEvent(false);	//调试消息队列等待标志

		//工作日志资源
		public WorkLog worklog;

		private SerialPort comDevice;						//串口资源
		public SerialPort ComDevice
		{
			get { return comDevice; }
			set { comDevice = value; }
		}
		private bool serialPortIsOpen = false;				//串口开启标志
		public bool SerialPortIsOpen
		{
			get { return serialPortIsOpen; }
			set { serialPortIsOpen = value; }
		}
		private RECVPACKSTATE CurrentRecvState = RECVPACKSTATE.IdleState;

		private Thread SerialSendThread;
		private Thread SerialResendThread;

		// 模块配置数据部分
		//public bool isStartRecvConfData = true;
		//public bool isRecvConfData = false;
		//public byte[] ConfData = new byte[6];

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		/*********************************************************************************************/
		#endregion


		#region 构造函数 和 析构函数
		/*----------------------------------------构造函数 和 析构函数--------------------------------*/
		//构造函数
		public SerialCommunicate()
		{
			serialSendQueue = new Queue<SerialSendMsg>();
			resendDic = new Dictionary<string, SerialSendMsg>();
			serialRecvQueue = new Queue<SerialRecvMsg>();
			DebugMsgQueue = new Queue<object>();
			comDevice = new SerialPort();
		}

		public bool ComOpen(string comNum, int Baud)
		{
			if (comNum != null)
			{
				//若串口正是打开着的, 则先关闭
				try
				{
					if (comDevice.IsOpen)
						comDevice.Close();
				}
				catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

				//打开串口
				try
				{
					comDevice.PortName = comNum;
					this.comDevice.BaudRate = Baud;
					this.comDevice.Parity = Parity.None;
					this.comDevice.DataBits = 8;
					this.comDevice.StopBits = StopBits.One;
					this.comDevice.ReadBufferSize = 512;
					this.comDevice.WriteBufferSize = 512;

					//打开串口
					comDevice.Open();
					comDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);	//绑定事件
					SerialSendThread = new Thread(new ThreadStart(SerialSend));		//开启发送消息的线程
					SerialSendThread.Name = "串口发送数据线程";
					SerialSendThread.IsBackground = true;
					SerialResendThread = new Thread(new ThreadStart(SerialResend));	//开启重发消息的线程
					SerialResendThread.Name = "串口重发线程";
					SerialResendThread.IsBackground = true;

					SerialSendThread.Start();
					SerialResendThread.Start();
					serialPortIsOpen = true;
					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					//log.Info(AppUtil.getExceptionInfo(ex));
					serialPortIsOpen = false;
					return false;
				}
			}
			else
				return false;
		}

		/*********************************************************************************************/
		#endregion


		#region  发送部分函数
		/*-----------------------------------------发送部分函数---------------------------------------*/

		//从发送队列中取出消息, 然后发送
		private void SerialSend()
		{
			SerialSendMsg sendMsg;
			int SendQueueItemCount = 0;
			while (true)
			{
				SendQueueWaitHandle.WaitOne();
				SendQueueItemCount = serialSendQueue.Count;				//提取队列中消息的个数
				if (serialPortIsOpen && (SendQueueItemCount > 0))		//若串口已打开, 且队列中有数据
				{
					for (int i = 0; i < SendQueueItemCount; i++)
					{
						lock (serialSendQueue) { sendMsg = serialSendQueue.Dequeue(); }	//从发送队列中取出消息
						bool isSendSuccess = false;
						//发送出去
						try
						{
							for (int n = 0; n < sendMsg.PackNumPerCmd; n++)
							{
								if (sendMsg.PacketData.Cmd == 0xFE)		// 先提取修改信道命令
								{
									comDevice.Write(sendMsg.PacketData.DataFiled, 0, 6);
								}
								else									// 直接发送的命令
									comDevice.Write(sendMsg.PacketData.DataBytes, 0, sendMsg.PacketDataLength);//发送数据
								#if DEBUG	//将发送消息投入调试发送队列中
								if (SerialComDebug.isPauseDebug == false)	//若当前不是暂停状态
								{
									sendMsg.SendTime = DateTime.Now;	//记录时间
									this.DebugQueue_Enqueue(sendMsg);
								}
								#endif
								//写入串口发送记录到日志文件中
								worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialSend, sendMsg));

								Thread.Sleep(55);		//底层串口每2个包发送的间隔不能小于50ms
							}
							isSendSuccess = true;
						}
						catch (Exception ex)
						{
							isSendSuccess = false;
							serialPortIsOpen = false;		//串口未开启
							Console.WriteLine(ex.Message);
							log.Info(AppUtil.getExceptionInfo(ex));
							//写入串口断开记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.Disconnect, null));
						}
						if (isSendSuccess)				//若发送成功
						{
							if (sendMsg.IsWaitAck)		//若需要重发, 则记录本次发送时间, 并投入重发字典中
							{
								sendMsg.SendTime = DateTime.Now;	//记录时间
								if (sendMsg.SendNumMax > 0)			//将发送次数减1
								{
									sendMsg.SendNumMax--;
								}
								sendMsg.PacketData.generateSendUType();
								lock (resendDic)
								{
									try
									{
										resendDic.Add(sendMsg.PacketData.SendUType, sendMsg);		//存入重发字典中
									}
									catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
								}
							}
						}
					}
				}
			}
		}

		//从重发字典中取出消息, 判断是否超时, 然后投入"底层发送队列"
		private void SerialResend()
		{
			DateTime dtNow;
			List<string> list = new List<string>();
			while (true)
			{
				if (serialPortIsOpen && (resendDic.Count > 0))
				{
					lock (resendDic)
					{
						dtNow = DateTime.Now;
						list.Clear();
						//遍历重发字典, 查找超时的消息, 并进行响应操作
						foreach (var sendMsg in resendDic)
						{
							if ((int)((dtNow - sendMsg.Value.SendTime).TotalMilliseconds) > sendMsg.Value.SendWaitTime)		//若超时
							{
								list.Add(sendMsg.Key);					//将超时消息的键存入list中, 为后面的移除工作做准备
								if (sendMsg.Value.SendNumMax > 0)		//若重发次数大于0, 则继续投入发送队列中
								{
									this.SendQueue_Enqueue(sendMsg.Value);
								}
								else									//若不能再重发了, 则投入接收处理的队列中
								{
									SerialRecvMsg recvMsg = new SerialRecvMsg();
									recvMsg.PacketData = sendMsg.Value.PacketData;
									//将发送命令变为响应
									if (recvMsg.PacketData.Cmd != 0x09)
										recvMsg.PacketData.Cmd += 1;
									else
										recvMsg.PacketData.Cmd = 0x10;
									recvMsg.IsFromExtern = false;		//内部消息(由于多次重发还是未收到响应导致)
									this.RecvQueue_Enqueue(recvMsg);	//投入接收消息队列中
									#if DEBUG	//将发送消息投入调试队列中
									if (SerialComDebug.isPauseDebug == false)	//若当前不是暂停状态
									{
										recvMsg.RecvTime = DateTime.Now;	//记录时间
										this.DebugQueue_Enqueue(recvMsg);
									}
									#endif
								}
							}
						}
						//将超时消息从重发字典中移除
						if (list.Count > 0)
						{
							foreach (string key in list)
								resendDic.Remove(key);
						}
					}
				}
				Thread.Sleep(5);
			}
		}
		/*********************************************************************************************/
		#endregion


		#region  接收部分函数
		/*-----------------------------------------接收部分函数---------------------------------------*/
		//串口数据接收
		private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			byte[] ReDatas = new byte[comDevice.BytesToRead];
			comDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据
			for (int i = 0; i < ReDatas.Length; i++)
			{
				RecvPacketStateMachine(ReDatas[i]);
			}
		}
		private int DataCnt = 0;
		private byte[] RecvDataField = new byte[20];
		//串口数据接收状态机
		private void RecvPacketStateMachine(byte RecvByte)
		{
			switch (CurrentRecvState)
			{
				case RECVPACKSTATE.IdleState:			//空闲态
					if (RecvByte == (byte)0x5A)
						CurrentRecvState = RECVPACKSTATE.StartFieldState;
					else
						CurrentRecvState = RECVPACKSTATE.IdleState;
					break;
				case RECVPACKSTATE.StartFieldState:		//接收起始符
					if (RecvByte == (byte)0xA5)
					{
						CurrentRecvState = RECVPACKSTATE.DataPacketState;
						DataCnt = 0;
					}
					else
						CurrentRecvState = RECVPACKSTATE.IdleState;
					break;
				case RECVPACKSTATE.DataPacketState:		//接收数据
					if (DataCnt < 19)
					{
						RecvDataField[DataCnt] = RecvByte;
						DataCnt++;
						CurrentRecvState = RECVPACKSTATE.DataPacketState;
					}
					else
					{
						RecvDataField[DataCnt] = RecvByte;
						//若校验通过, 则解析数据
						if (AppUtil.GetChecksum(RecvDataField, 0, 19) == RecvDataField[19])
						{
							byte DIRECT = (byte)(RecvDataField[0] >> 4);	//指令方向, 先过滤一些数据
							if (DIRECT == 0x02 || DIRECT == 0x04 || DIRECT == 0x08)	//2:(终端-主机), 4:(中继-主机), 8:(非本组设备之间的通信)
							{
								SerialRecvMsg recvMsg = new SerialRecvMsg();
								Array.Copy(RecvDataField, 0, recvMsg.PacketData.DataBytes, 2, RecvDataField.Length);
								recvMsg.PacketData.updateDataBytesToField();	//将接收到的字节更新到数据包的各个字段(如dir, path, cmd等)
								PacketHandler(recvMsg);
							}
						}
							
						CurrentRecvState = RECVPACKSTATE.IdleState;
					}
					break;

				default:
					CurrentRecvState = RECVPACKSTATE.IdleState;
					break;
			}
		}

		//数据包处理函数
		private void PacketHandler(SerialRecvMsg recvMsg)
		{
			if (recvMsg.PacketData.PacketIsAck())		//判断是否是响应数据包, 若是响应, 则将其从重发字典中移除
			{
				recvMsg.PacketData.generateSendUType();
				string key = recvMsg.PacketData.SendUType;
				if (key != null)
				{
					lock (resendDic)
					{
						if (resendDic.ContainsKey(key))
							resendDic.Remove(key);
					}
				}
			}

			#if DEBUG	//将发送消息投入调试接收队列中
			if (SerialComDebug.isPauseDebug == false)	//若当前不是暂停状态
			{
				recvMsg.RecvTime = DateTime.Now;	//记录时间
				this.DebugQueue_Enqueue(recvMsg);
			}
			#endif

			recvMsg.IsFromExtern = true;		//外部消息
			this.RecvQueue_Enqueue(recvMsg);	//投入接收消息队列中

		}

		/*********************************************************************************************/
		#endregion


		#region  队列出入栈相关函数
		//向发送队列中投递消息----外部调用
		public bool SendQueue_Enqueue(SerialSendMsg sendMsg)
		{
			if (serialPortIsOpen)
			{
				lock (this.serialSendQueue) { serialSendQueue.Enqueue(sendMsg); }
				SendQueueWaitHandle.Set();
				return true;
			}
			else
				return false;
		}
		//从发送队列中取走消息, 返回null代表无消息
		public SerialSendMsg SendQueue_Dequeue()
		{
			SerialSendMsg sendMsg = null;
			if (serialSendQueue.Count > 0)
			{
				lock (serialSendQueue) { sendMsg = serialSendQueue.Dequeue(); }
			}
			return sendMsg;
		}

		//向接收队列中投递消息
		public bool RecvQueue_Enqueue(SerialRecvMsg recvMsg)
		{
			lock (this.serialRecvQueue) { serialRecvQueue.Enqueue(recvMsg); }
			RecvQueueWaitHandle.Set();
			return true;
		}
		//从接收队列中取走消息, 返回null代表无消息
		public SerialRecvMsg RecvQueue_Dequeue()
		{
			SerialRecvMsg recvMsg = null;
			if (serialRecvQueue.Count > 0)
			{
				lock (this.serialRecvQueue) { recvMsg = serialRecvQueue.Dequeue(); }
			}
			return recvMsg;
		}

		//向调试队列中投递消息
		public void DebugQueue_Enqueue(object Msg)
		{
			lock (this.DebugMsgQueue) { DebugMsgQueue.Enqueue(Msg); }
			DebugQueueWaitHandle.Set();
		}
		/*********************************************************************************************/
		#endregion

	}
}
