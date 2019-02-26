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
using SCBAControlHost.SerialCommunication;
using System.Threading;

namespace SCBAControlHost
{
	public partial class SerialComDebug : Form
	{
		public Queue<object> DebugMsgQueue;
		AutoResetEvent DebugQueueWaitHandle;
		public static bool isPauseDebug = false;				//是否暂停调试

		//构造函数, 传入主窗口的大小和用户
		public SerialComDebug()
		{
			InitializeComponent();
			this.ControlBox = false;
			btnClearDbgData.Click += new EventHandler(btnClearDbgData_Click);
			btnPauseDbg.Click += new EventHandler(btnPauseDbg_Click);
		}

		//暂停调试
		void btnPauseDbg_Click(object sender, EventArgs e)
		{
			if (isPauseDebug)	//若当前正是暂停调试状态
			{
				isPauseDebug = false;
				btnPauseDbg.Text = "暂停";
			}
			else
			{
				isPauseDebug = true;
				btnPauseDbg.Text = "启动";
			}
		}

		//清空调试数据
		void btnClearDbgData_Click(object sender, EventArgs e)
		{
			listViewDebug.Items.Clear();
		}

		//开启调试线程
		public void StartDebugThread(Queue<object> debugMsgQueue, AutoResetEvent debugQueueWaitHandle)
		{
			DebugMsgQueue = debugMsgQueue;
			DebugQueueWaitHandle = debugQueueWaitHandle;

			Thread debugThread = new Thread(DebugDisplayMsg);
			debugThread.Name = "串口调试线程";
			debugThread.IsBackground = true;
			#if DEBUG
			debugThread.Start();
			#endif
		}

		private int PacketType = 0;			//指示包类型 0-发送, 1-接收, 2-超时错误
		private string DirectString;		//方向字符串
		private string PacketTypeString;	//包类型字符串
		private string DirPathString;		//Dir+Path字符串
		private string CmdByteString;		//cmd字节字符串
		private string DataFieldString;		//数据字段字符串
		private string ChecksumString;
		private string DescriptionString;	//说明字符串


		void DebugDisplayMsg()
		{
			SerialSendMsg sendMsg;
			SerialRecvMsg recvMsg;
			object objMsg;
			int DebugQueueItemCount = 0;
			Thread.Sleep(500);
			while (true)
			{
				DebugQueueWaitHandle.WaitOne();
				DebugQueueItemCount = DebugMsgQueue.Count;

				if (DebugQueueItemCount > 0)
				{
					for (int n = 0; n < DebugQueueItemCount; n++)
					{
						lock (DebugMsgQueue) { objMsg = DebugMsgQueue.Dequeue(); }
						ListViewItem lvi = new ListViewItem();
						if (objMsg is SerialSendMsg)		//若是发送消息
						{
							sendMsg = objMsg as SerialSendMsg;
							getPacketStr(sendMsg.PacketData.DataBytes, false);
							if (sendMsg.PacketData.Cmd == 0x31)
							{
								PacketType = 0;
								DirectString = "主机发送";
								PacketTypeString = "主机查询信道";
							}
							else if (sendMsg.PacketData.Cmd == 0x32)
							{
								PacketType = 0;
								DirectString = "主机发送";
								PacketTypeString = "主机响应其他主机的信道查询";
							}
							lvi.Text = sendMsg.SendTime.ToString("HH:mm:ss.fff");	//时间
						}
						else if (objMsg is SerialRecvMsg)	//若是接收消息
						{
							recvMsg = objMsg as SerialRecvMsg;
							getPacketStr(recvMsg.PacketData.DataBytes, recvMsg.IsFromExtern);
							if (recvMsg.PacketData.Cmd == 0x31)
							{
								PacketType = 0;
								DirectString = "主机接收";
								PacketTypeString = "主机接收到其他主机的查询命令";
							}
							else if (recvMsg.PacketData.Cmd == 0x32)
							{
								if (recvMsg.IsFromExtern)
								{
									PacketType = 1;
									DirectString = "主机接收";
									PacketTypeString = "主机接收到其他主机的查询响应";
									DescriptionString = "主机即将切换信道";
								}
								else
								{
									PacketType = 2;
									DirectString = "-";
									PacketTypeString = "主机查询响应超时";
									DescriptionString = "主机查询响应超时, 主机即将作为区域主机";
									CmdByteString = (0x31).ToString("x2");
								}
							}
							lvi.Text = recvMsg.RecvTime.ToString("HH:mm:ss.fff");	//时间
						}

						lvi.SubItems.Add(DirectString);							//方向
						lvi.SubItems.Add(PacketTypeString);						//包类型
						lvi.SubItems.Add(DirPathString);						//Dir+Path字节
						lvi.SubItems.Add(CmdByteString);						//命令字节
						lvi.SubItems.Add(DataFieldString);						//数据字段
						lvi.SubItems.Add(ChecksumString);						//校验字节
						lvi.SubItems.Add(DescriptionString);					//说明
						if (PacketType == 0)
							lvi.BackColor = Color.FromArgb(199, 237, 204);
						else if (PacketType == 1)
							lvi.BackColor = Color.FromArgb(219, 229, 241);
						else if (PacketType == 2)
							lvi.BackColor = Color.FromArgb(229, 184, 184);

						listViewDebug.Invoke(new Action(() => { listViewDebug.Items.Add(lvi); }));
					}
				}
			}
		}


		private void getPacketStr(byte[] dataPacket, bool isFromExtern)
		{
			DirPathString = dataPacket[2].ToString("x2") + " " + dataPacket[3].ToString("x2");
			CmdByteString = dataPacket[4].ToString("x2");
			ChecksumString = dataPacket[21].ToString("x2");
			DataFieldString = "";
			for (int i = 0; i < 16; i++)
			{
				DataFieldString += dataPacket[5 + i].ToString("x2");
				DataFieldString += " ";
			}
			DescriptionString = "-";

			switch (dataPacket[4])
			{
				#region 远程播报命令1 0x01, 0x02
				//远程播报命令1
				case 0x01:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "远程播放1命令";
					break;
				case 0x02:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "远程播放1响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "远程播放1响应超时";
						DescriptionString = "远程播放1响应超时";
						CmdByteString = (0x01).ToString("x2");
					}
					break;
				#endregion

				#region 远程播报命令2 0x03, 0x04
				//远程播报命令2
				case 0x03:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "远程播放2命令";
					break;
				case 0x04:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "远程播放2响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "远程播放2响应超时";
						DescriptionString = "远程播放2响应超时";
						CmdByteString = (0x03).ToString("x2");
					}
					break;
				#endregion

				#region 远程播报命令3 0x05, 0x06
				//远程播报命令3
				case 0x05:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "远程播放3命令";
					break;
				case 0x06:
					if (isFromExtern)
					{
						PacketType =1;
						DirectString = "主机接收";
						PacketTypeString = "远程播放3响应";
					}
					else		//若是超时响应
					{
						PacketType =2;
						DirectString = "-";
						PacketTypeString = "远程播放3响应超时";
						DescriptionString = "远程播放3响应超时";
						CmdByteString = (0x05).ToString("x2");
					}
					break;
				#endregion

				#region 远程播报命令4 0x07, 0x08
				//远程播报命令4
				case 0x07:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "远程播放4命令";
					break;
				case 0x08:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "远程播放4响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "远程播放4响应超时";
						DescriptionString = "远程播放4响应超时";
						CmdByteString = (0x07).ToString("x2");
					}
					break;
				#endregion

				#region 远程播报命令5 0x09, 0x10 (撤出操作)
				//远程播报命令5--撤出
				case 0x09:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "远程播放5命令";
					break;
				case 0x10:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "远程播放5响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "远程播放5响应超时";
						DescriptionString = "远程播放5响应超时";
						CmdByteString = (0x09).ToString("x2");
					}
					break;
				#endregion

				#region 终端上传主机 0x13, 0x14
				//终端上传主机
				case 0x13:
					PacketType = 1;
					DirectString = "主机接收";
					PacketTypeString = "终端上传主机命令";
					break;
				case 0x14:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "终端上传主机响应";
					break;
				#endregion

				#region 主机查询终端 0x15, 0x16	(刷新操作)
				//主机查询终端
				case 0x15:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "主机查询终端命令";
					break;
				case 0x16:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "主机查询终端响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "主机查询终端响应超时";
						DescriptionString = "主机查询终端响应超时";
						CmdByteString = (0x15).ToString("x2");
					}
					break;
				#endregion

				#region 终端开机注册 0x21, 0x22
				//终端开机注册
				case 0x21:
					PacketType = 1;
					DirectString = "主机接收";
					PacketTypeString = "终端开机注册命令";
					break;
				case 0x22:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "主机响应终端开机";
					break;
				#endregion

				#region 终端关机声明 0x23, 0x24
				//终端关机声明
				case 0x23:
					PacketType = 1;
					DirectString = "主机接收";
					PacketTypeString = "终端关机声明命令";
					break;
				case 0x24:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "主机响应终端关机";
					break;
				#endregion

				#region 主机查询 0x31, 0x32
				//主机查询
				case 0x31:
					DirectString = "主机发送";
					PacketTypeString = "主机查询命令";
					break;
				case 0x32:
					DirectString = "主机接收";
					PacketTypeString = "主机查询响应";
					break;
				#endregion

				#region 主机切换 0x33, 0x34
				//主机切换(无)
				#endregion

				#region 终端切换 0x35, 0x36
				//终端切换(未知, 无响应格式)
				case 0x35:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "终端切换命令";
					break;
				case 0x36:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "主机响应终端切换";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "终端切换响应超时";
						DescriptionString = "终端切换响应超时";
						CmdByteString = (0x35).ToString("x2");
					}
					break;
				#endregion

				#region 临时组队 0x37, 0x38
				//临时组队
				case 0x37:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "临时组队命令";
					break;
				case 0x38:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "临时组队响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "临时组队响应超时";
						DescriptionString = "临时组队响应超时";
						CmdByteString = (0x37).ToString("x2");
					}
					break;
				#endregion

				#region 设置1 0x41, 0x42
				//设置1
				case 0x41:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "设置1命令";
					break;
				case 0x42:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "设置1响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "设置1响应超时";
						DescriptionString = "设置1响应超时";
						CmdByteString = (0x41).ToString("x2");
					}
					break;
				#endregion

				#region 设置2 0x43, 0x44
				//设置2
				case 0x43:
					PacketType = 0;
					DirectString = "主机发送";
					PacketTypeString = "设置2命令";
					break;
				case 0x44:
					if (isFromExtern)
					{
						PacketType = 1;
						DirectString = "主机接收";
						PacketTypeString = "设置2响应";
					}
					else		//若是超时响应
					{
						PacketType = 2;
						DirectString = "-";
						PacketTypeString = "设置2响应超时";
						DescriptionString = "设置2响应超时";
						CmdByteString = (0x43).ToString("x2");
					}
					break;
				#endregion

				default:
					break;
			}
		}
	}
}
