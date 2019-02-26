using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace SerialComPack
{

	//数据包类, 和具体应用相关
	public class SerialDataPacket
	{
		private byte[] header = new byte[2];		//包头5AA5--2bytes
		private byte direct;						//方向--1byte
		private byte path;							//路径--1byte
		public byte cmd;							//命令--1byte
		private byte[] dataFiled = new byte[16];	//数据域--16bytes
		private byte checkSum;						//校验

		private byte[] dataBytes = new byte[22];	//整个数据包的字节数组

		private string sendUType;
		public string SendUType
		{
			get { return sendUType; }
			set { sendUType = value; }
		}

		public void generateSendUType()
		{
			switch (cmd)
			{
				//远程播报命令1
				case 0x01:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x02:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//远程播报命令2
				case 0x03:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x04:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//远程播报命令3
				case 0x05:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x06:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//远程播报命令4
				case 0x07:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x08:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//远程播报命令5
				case 0x09:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x10:
					sendUType = "1-" + "09" + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//主机查询终端
				case 0x15:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x16:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//主机查询
				case 0x31:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x32:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//主机切换(无)

				//终端切换(未知, 无响应格式)
				case 0x35:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x36:
					sendUType = null;
					break;
				//临时组队
				case 0x37:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x38:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//设置1
				case 0x41:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x42:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				//设置2
				case 0x43:
					sendUType = "1-" + cmd.ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;
				case 0x44:
					sendUType = "1-" + (cmd - 1).ToString("X2") + ",2-" + dataFiled[0].ToString("X2") + ",3-" + dataFiled[1].ToString("X2") + dataFiled[2].ToString("X2") + dataFiled[3].ToString("X2") + dataFiled[4].ToString("X2");
					break;

				default: sendUType = null; break;
			}

		}

		public byte[] Header
		{
			get { return header; }
			set { header = value; }
		}

		public byte Direct
		{
			get { return direct; }
			set { direct = value; }
		}
		public byte Path
		{
			get { return path; }
			set { path = value; }
		}
		public byte Cmd
		{
			get { return cmd; }
			set { cmd = value; }
		}
		public byte[] DataFiled
		{
			get { return dataFiled; }
			set { dataFiled = value; }
		}
		public byte CheckSum
		{
			get { return checkSum; }
			set { checkSum = value; }
		}

		private int count = 0;
		public byte[] DataBytes
		{
			get
			{		//发送时调用

				Stream s = new MemoryStream();
				s.WriteByte(this.Header[0]);
				s.WriteByte(this.Header[1]);
				s.WriteByte(this.Direct);
				s.WriteByte(this.Path);
				s.WriteByte(this.Cmd);
				s.Write(this.DataFiled, 0, this.DataFiled.Length);
				s.WriteByte(this.CheckSum);
				s.Seek(0, SeekOrigin.Begin);	//将位置设置为流的起始位置
				s.Read(dataBytes, 0, (int)(s.Length));
				return dataBytes;
			}
			set
			{		//接受时调用
				dataBytes = value;
			}
		}
		public void updateDataBytesToField()
		{
			this.direct = dataBytes[2];
			this.path = dataBytes[3];
			this.cmd = dataBytes[4];
			for (int i = 0; i < 16; i++)
				this.dataFiled[i] = dataBytes[i + 5];
			this.checkSum = dataBytes[21];
		}

		//判断包是否是响应
		public bool PacketIsAck()
		{
			if (this.cmd % 2 == 0)
				return true;
			else
				return false;
		}

		//构造函数
		public SerialDataPacket()
		{
			Header[0] = 0x5A;
			Header[1] = 0xA5;
			sendUType = null;
		}

	}

}
