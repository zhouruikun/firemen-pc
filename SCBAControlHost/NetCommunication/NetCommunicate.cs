using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using MyUtils;
using log4net;
using SCBAControlHost.SysConfig;

namespace SCBAControlHost.NetCommunication
{
	public class NetPacket
	{
		public byte PacketType;				//包类型
		public byte DataLength_HighByte;	//长度高字节
		public byte DataLength_LowByte;		//长度低字节
		public byte[] datafield;			//数据域

		private int dataLength;
		public int DataLength
		{
			get { return dataLength; }
			set { dataLength = value; dataBytes = new byte[dataLength + 6]; }
		}

		public byte[] dataBytes;			//用于网络发送的所有数据字节
		public byte[] DataBytes
		{
			get
			{		//发送时调用
				Stream s = new MemoryStream();
				s.WriteByte(0x5A);
				s.WriteByte(0xA5);
				s.WriteByte(this.PacketType);
				s.WriteByte(this.DataLength_HighByte);
				s.WriteByte(this.DataLength_LowByte);
				s.Write(this.datafield, 0, this.DataLength);
				if (DataLength != 0)
					s.WriteByte(AppUtil.GetChecksum(new byte[4]{ PacketType, 
															 DataLength_HighByte, 
															 DataLength_LowByte, 
															 AppUtil.GetChecksum(datafield, 0, DataLength)}, 0, 4));
				else
					s.WriteByte(AppUtil.GetChecksum(new byte[3]{ PacketType, DataLength_HighByte, DataLength_LowByte}, 0, 3));
				s.Seek(0, SeekOrigin.Begin);	//将位置设置为流的起始位置
				s.Read(dataBytes, 0, (int)(s.Length));
				return dataBytes;
			}
			set
			{		//接受时调用
				dataBytes = value;
			}
		}
	}

	class NetCommunicate
	{
		enum RECVPACKSTATE
		{
			IdleState,
			StartFieldState,
			TypeFieldState,
			LengthFieldState,
			DataPacketState,
			CheckFieldState
		};

		public bool isConnected = false;		//当前是否连接上服务器

		//工作日志资源
		public WorkLog worklog;

		public TcpClient client = new TcpClient();
		public NetworkStream networkStream;
		public BinaryReader br;
		public BinaryWriter bw;

		public Queue<NetPacket> netSendQueue = new Queue<NetPacket>();
		public AutoResetEvent NetSendQueueWaitHandle = new AutoResetEvent(false);	//网络发送队列等待标志
		public Queue<NetPacket> netRecvQueue = new Queue<NetPacket>();
		public AutoResetEvent NetRecvQueueWaitHandle = new AutoResetEvent(false);	//网络接收队列等待标志

		public DateTime LatestSendTime = DateTime.Now;		//最近一次发送数据包的时间
		public NetDelegate netDelegate;

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		//构造函数
		public NetCommunicate()
		{
			Thread recvth = new Thread(RecvThread);	//接收数据的线程
			recvth.Name = "网络接收数据线程";
			recvth.IsBackground = true;				//线程随主线程的退出而退出
			recvth.Start();
			Thread sendth = new Thread(SendThread);	//发送数据的线程
			sendth.Name = "网络发送数据线程";
			sendth.IsBackground = true;				//线程随主线程的退出而退出
			sendth.Start();

			netDelegate = new NetDelegate();
		}


		#region  连接部分函数
		/*-----------------------------------------连接部分函数---------------------------------------*/
		//连接服务器
		public bool NetConnect(string ip, int port)
		{
			if (!isConnected)	//若当前还未连接, 则进行连接
			{
				try
				{
					client = new TcpClientWithTimeout(ip, port, 3000).Connect();	//带超时检测的连接
					//client = new TcpClient(ip, port);
					networkStream = client.GetStream();
					br = new BinaryReader(networkStream);
					bw = new BinaryWriter(networkStream);
					isConnected = true;			//标志已连接上服务器
					//写入网络连接记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.TcpConnect, "1"));
					Console.WriteLine("连接成功");

					return true;
				}
				catch (Exception ex)
				{
					//写入网络连接记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.TcpConnect, "2"));
					Console.WriteLine("LinkThread:" + ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
					isConnected = false;
				}
			}
			Thread.Sleep(10);
			return false;
		}

		//关闭连接
		public void NetClose()
		{
			try
			{
				if (bw != null)
					bw.Close();
				if (br != null)
					br.Close();
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }
			try
			{
				if (networkStream != null)
					networkStream.Close();
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }
			try
			{
				if (client != null)
					client.Close();
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }

			isConnected = false;
		}

		/*********************************************************************************************/
		#endregion


		#region  发送部分函数
		/*-----------------------------------------发送部分函数---------------------------------------*/
		//发送数据包的线程
		private void SendThread()
		{
			NetPacket sendPacket;
			int SendQueueItemCount = 0;
			while (true)
			{
				NetSendQueueWaitHandle.WaitOne();
				SendQueueItemCount = netSendQueue.Count;
				if (SendQueueItemCount > 0)
				{
					for (int i = 0; i < SendQueueItemCount; i++)
					{
						lock (netSendQueue) { sendPacket = netSendQueue.Dequeue(); }	//取出数据包
						if (client != null)
						{
							DataPackSend(sendPacket);		//发送出去
						}
					}
				}

			}
		}

		//发送数据包的函数
		public void DataPackSend(byte PackType, int DataLength, byte[] data)
		{
			byte[] DataSend = new byte[DataLength + 6];

			DataSend[0] = (byte)0x5A; DataSend[1] = (byte)0xA5;
			DataSend[2] = PackType;
			DataSend[3] = (byte)((DataLength & 0xFF00) >> 8); DataSend[4] = (byte)(DataLength & 0x00FF);
			for (int i = 0; i < DataLength; i++)
				DataSend[5 + i] = data[i];
			DataSend[DataLength + 5] = AppUtil.GetChecksum(DataSend, 2, DataLength + 3);
			if (isConnected)
			{
				try
				{
					bw.Write(DataSend, 0, DataLength + 6);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
					isConnected = false;
				}
			}
		}

		//发送数据包的函数, 发送一个数据包
		public void DataPackSend(NetPacket packetSend)
		{
			if (packetSend != null)
			{
				byte[] sendBuf = new byte[packetSend.DataLength + 6];
				sendBuf[0] = 0x5A;								//起始符0
				sendBuf[1] = 0xA5;								//起始符1
				sendBuf[2] = packetSend.PacketType;				//报类型
				sendBuf[3] = packetSend.DataLength_HighByte;	//长度高字节
				sendBuf[4] = packetSend.DataLength_LowByte;		//长度低字节
				for (int i = 0; i < packetSend.DataLength; i++)	//数据域
					sendBuf[5 + i] = packetSend.datafield[i];
				//校验字节
				if (packetSend.DataLength != 0)
					sendBuf[packetSend.DataLength + 5] = AppUtil.GetChecksum(new byte[4]{ packetSend.PacketType, 
															 packetSend.DataLength_HighByte, 
															 packetSend.DataLength_LowByte, 
															 AppUtil.GetChecksum(packetSend.datafield, 0, packetSend.DataLength)}, 0, 4);
				else
					sendBuf[packetSend.DataLength + 5] = AppUtil.GetChecksum(new byte[3]{ packetSend.PacketType, packetSend.DataLength_HighByte, packetSend.DataLength_LowByte}, 0, 3);
				
				//发送出去
				if (isConnected)
				{
					try
					{
						bw.Write(sendBuf, 0, sendBuf.Length);
						//写入网络发送记录到日志文件中
						worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetSend, packetSend));

						LatestSendTime = DateTime.Now;	//更新最近一次发送数据包的时间
					}
					catch (Exception ex)	//发送数据异常, 判断是与主机断开连接
					{
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
						isConnected = false;

						netDelegate.TriggerEvent(null);		//网络意外断开调用委托
					}
				}
			}
		}

		/*********************************************************************************************/
		#endregion


		#region  接收部分函数
		/*-----------------------------------------接收部分函数---------------------------------------*/
		//接收数据的线程
		private void RecvThread()
		{
			while (true)
			{
				if ((client != null) && (isConnected))
				{
					try
					{
						if (networkStream != null)
						{
							if (networkStream.DataAvailable)
							{
								byte[] buffer = new byte[1024];
								int length = networkStream.Read(buffer, 0, 1024);
								if (length > 0)
									RecvPacketStateMachine(buffer, length);
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("RecvThread:" + ex.Message + ex.StackTrace);
						log.Info(AppUtil.getExceptionInfo(ex));
					}
				}

				Thread.Sleep(1);
			}
		}


		//解码状态机
		private RECVPACKSTATE CurrentRecvState = RECVPACKSTATE.IdleState;
		private NetPacket NetPacketRecv = new NetPacket();
		int DataCnt = 0;
		private void RecvPacketStateMachine(byte[] RecvBytes, int length)
		{
			byte RecvByte;
			for (int i = 0; i < length; i++)
			{
				RecvByte = RecvBytes[i];

				switch (CurrentRecvState)
				{
					case RECVPACKSTATE.IdleState:
						if (RecvByte == (byte)0x5A)
							CurrentRecvState = RECVPACKSTATE.StartFieldState;
						else
							CurrentRecvState = RECVPACKSTATE.IdleState;
						break;

					case RECVPACKSTATE.StartFieldState:
						if (RecvByte == (byte)0xA5)
							CurrentRecvState = RECVPACKSTATE.TypeFieldState;
						else
							CurrentRecvState = RECVPACKSTATE.IdleState;
						break;

					case RECVPACKSTATE.TypeFieldState:
						NetPacketRecv.PacketType = RecvByte;
						NetPacketRecv.DataLength = 0;
						CurrentRecvState = RECVPACKSTATE.LengthFieldState;
						break;

					case RECVPACKSTATE.LengthFieldState:
						if (NetPacketRecv.DataLength < 1)
						{
							NetPacketRecv.DataLength_HighByte = RecvByte;
							NetPacketRecv.DataLength++;
							CurrentRecvState = RECVPACKSTATE.LengthFieldState;
						}
						else
						{
							NetPacketRecv.DataLength_LowByte = RecvByte;
							NetPacketRecv.DataLength = (int)(((int)NetPacketRecv.DataLength_HighByte) << 8) + (int)RecvByte;
							NetPacketRecv.datafield = new byte[NetPacketRecv.DataLength];
							DataCnt = 0;
							CurrentRecvState = RECVPACKSTATE.DataPacketState;
						}
						break;

					case RECVPACKSTATE.DataPacketState:
						NetPacketRecv.datafield[DataCnt] = RecvByte;
						DataCnt++;
						if (DataCnt < NetPacketRecv.DataLength)
						{
							CurrentRecvState = RECVPACKSTATE.DataPacketState;
						}
						else
						{
							CurrentRecvState = RECVPACKSTATE.CheckFieldState;
						}
						break;

					case RECVPACKSTATE.CheckFieldState:
						if (AppUtil.GetChecksum(new byte[4]{ NetPacketRecv.PacketType, 
														 NetPacketRecv.DataLength_HighByte, 
														 NetPacketRecv.DataLength_LowByte, 
														 AppUtil.GetChecksum(NetPacketRecv.datafield, 0, NetPacketRecv.DataLength)}, 0, 4) == RecvByte)
						{
							PacketParse(NetPacketRecv);		//接收到了一个完整的数据包, 然后对数据包进行解码
						}
						CurrentRecvState = RECVPACKSTATE.IdleState;
						break;

					default:
						CurrentRecvState = RECVPACKSTATE.IdleState;
						break;
				}
			}
		}

		//数据包解析函数
		private void PacketParse(NetPacket netPacketRecv)
		{
			this.NetRecvQueue_Enqueue(netPacketRecv);
			//Console.WriteLine("接收到了一条数据");
		}

		/*********************************************************************************************/
		#endregion


		#region  队列出入栈相关函数

		//将一个数据包投入发送队列中
		public void NetSendQueue_Enqueue(NetPacket packetSend)
		{
			if (isConnected)
			{
				lock (netSendQueue) { netSendQueue.Enqueue(packetSend); }
				NetSendQueueWaitHandle.Set();
			}
		}

		//将一个数据包投入接收队列中
		public void NetRecvQueue_Enqueue(NetPacket packetRecv)
		{
			lock (netRecvQueue) { netRecvQueue.Enqueue(packetRecv); }
			NetRecvQueueWaitHandle.Set();
		}

		/*********************************************************************************************/
		#endregion
	}
}
