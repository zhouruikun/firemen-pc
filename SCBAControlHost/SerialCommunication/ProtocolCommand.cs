using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyUtils;
using SerialComPack;

namespace SCBAControlHost.SerialCommunication
{
	//串口波特率
	public enum SerialBaud : byte
	{
		baud1200 = 0x01,
		baud2400,
		baud4800,
		baud9600,
		baud19200,
		baud38400,
		baud57600,
		baud115200
	}
	//空中速率
	public enum AirRate : byte
	{
		rate300 = 0x00,
		rate1200,
		rate2400,
		rate4800,
		rate9600,
		rate19200
	}
	//FEC开关
	public enum FECSwitch : byte
	{
		FECClose = 0x00,
		FECOpen,
	}
	//发射功率
	public enum WorkPower : byte
	{
		Pow27dbm = 0x00,
		Pow30dbm,
	}

	//public enum SerialOpCodEnum : byte
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

	public partial class ProtocolCommand
	{
		private static byte[] padChars = new byte[16] { 0x55, 0x55, 0x55, 0x55, 
												 0x55, 0x55, 0x55, 0x55,
												 0x55, 0x55, 0x55, 0x55,
												 0x55, 0x55, 0x55, 0x55 };

		/*
		 * 参数设置1命令
		 */
		public static SerialSendMsg ParaSetup1CmdMsg(byte[] oldSerialNum, byte[] newSerialNum)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x80;
			sdp.Cmd = 0x41;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(oldSerialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(newSerialNum, 0, sdp.DataFiled, 5, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 9, 7);		//填充7字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 3;		//每次命令连续发送3个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 无重发
			sendMsg.IsWaitAck = true;		//等待响应
			sendMsg.SendWaitTime = 1000;		//等待时间为1秒
			return sendMsg;
		}

		/*
		 * 参数设置2命令
		 * 串口速率：1=1200, 2=2400, 3=4800, 4=9600, 5=19200, 6=38400, 7=57600, 8=115200
		 * 空中速率：0=300,  1=1200, 2=2400, 3=4800, 4=9600, 5=19200；
		 * FEC开关：0=关，  1=开
		 * 发射功率：0=27dbm ,  1=30dbm;
		 */
		public static SerialSendMsg ParaSetup2CmdMsg(byte[] serialNum, SerialBaud serialBaud, AirRate airRate, FECSwitch fecSwitch, WorkPower workPower)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x80;
			sdp.Cmd = 0x43;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(serialNum, 0, sdp.DataFiled, 1, 4);
			sdp.DataFiled[5] = (byte)serialBaud;
			sdp.DataFiled[6] = (byte)airRate;
			sdp.DataFiled[7] = (byte)fecSwitch;
			sdp.DataFiled[8] = (byte)workPower;

			sdp.DataFiled[9] = (byte)serialBaud;
			sdp.DataFiled[10] = (byte)airRate;
			sdp.DataFiled[11] = (byte)fecSwitch;
			sdp.DataFiled[12] = (byte)workPower;
			Array.Copy(padChars, 0, sdp.DataFiled, 13, 3);		//填充7字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 3;		//每次命令连续发送3个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 无重发
			sendMsg.IsWaitAck = true;		//等待响应
			sendMsg.SendWaitTime = 1000;	//等待时间为1秒
			return sendMsg;
		}

		/*
		 * 主机查询命令
		 */
		public static SerialSendMsg ServerQueryCmdMsg(byte[] serialNum)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x80;
			sdp.Path = 0xC0;
			sdp.Cmd = 0x31;
			sdp.DataFiled[0] = 0x12;
			Array.Copy(serialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 5, 11);		//填充11字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			//发送消息的重发次数为2次, 每次间隔1000ms
			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为2, 最多重发1次
			sendMsg.IsWaitAck = true;		//等待响应
			sendMsg.SendWaitTime = 2000;	//等待时间为1秒
			return sendMsg;
		}

		/*
		 * 响应终端开机注册, serialNum为终端序列号
		 */
		public static SerialSendMsg TerminalPowerOnAckMsg(byte[] serialNum)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x00;
			sdp.Cmd = 0x22;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(serialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 5, 11);		//填充11字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			//发送消息的重发次数为2次, 每次间隔1000ms
			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为2, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0秒
			return sendMsg;
		}

		/*
		 * 终端切换命令
		 */
		public static SerialSendMsg TerminalSwitchCmdMsg(byte[] TargetSerialNum, byte[] controlHostSerialNum, byte channel)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x80;
			sdp.Path = 0x80;
			sdp.Cmd = 0x35;
			sdp.DataFiled[0] = 0x12;
			Array.Copy(TargetSerialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(controlHostSerialNum, 0, sdp.DataFiled, 5, 4);
			sdp.DataFiled[9] = channel;
			Array.Copy(padChars, 0, sdp.DataFiled, 10, 6);		//填充6字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0
			return sendMsg;
		}

		/*
		 * 远程播报命令  cmdIndex-播报语音的命令号01, 03, 05, 07, 09
		 */
		public static SerialSendMsg RemotePlaySoundCmdMsg(byte cmdIndex, byte[] TerminalSerialNO, int sendNum, int waitTim)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x80;
			sdp.Cmd = cmdIndex;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(TerminalSerialNO, 0, sdp.DataFiled, 1, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 5, 11);		//填充11字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			sdp.generateSendUType();

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送2个相同的包
			sendMsg.SendNumMax = sendNum;	//最大发送次数为1, 不重发
			if (waitTim == 0) sendMsg.IsWaitAck = false;//是否等待响应
			else sendMsg.IsWaitAck = true;
			sendMsg.SendWaitTime = waitTim;	//等待时间为0
			return sendMsg;
		}

		/*
		 * 响应终端主动上传数据
		 */
		public static SerialSendMsg TerminalUploadCmdAckMsg(byte[] dataField)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x00;
			sdp.Cmd = 0x14;
			Array.Copy(dataField, 0, sdp.DataFiled, 0, 16);
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0
			return sendMsg;
		}

		/*
		 * 主机查询终端命令
		 */
		public static SerialSendMsg ServerQueryTerminalCmdMsg(byte[] TerminalSerialNO, int sendNum, int waitTim)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x80;
			sdp.Cmd = 0x15;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(TerminalSerialNO, 0, sdp.DataFiled, 1, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 5, 11);		//填充11字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = sendNum;	//最大发送次数
			if (waitTim == 0) sendMsg.IsWaitAck = false;//是否等待响应
			else sendMsg.IsWaitAck = true;
			sendMsg.SendWaitTime = waitTim;		//等待时间由参数决定(单个用户查询间隔为2秒, 全部查询间隔为10秒)
			return sendMsg;
		}

		/*
		 * 主机切换命令
		 */
		public static SerialSendMsg ServerSwitchCmdMsg()
		{
			SerialDataPacket sdp = new SerialDataPacket();

			SerialSendMsg sendMsg = new SerialSendMsg();

			return sendMsg;
		}

		/*
		 * 临时组队命令
		 */
		public static SerialSendMsg BuildTeamCmdMsg(byte[] TargetSerialNum, byte[] newDevNum, byte[] controlHostSerialNum)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x80;
			sdp.Path = 0x80;
			sdp.Cmd = 0x37;
			sdp.DataFiled[0] = 0x12;
			Array.Copy(TargetSerialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(newDevNum, 0, sdp.DataFiled, 5, 4);
			Array.Copy(controlHostSerialNum, 0, sdp.DataFiled, 9, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 13, 3);		//填充3字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = true;		//不等待响应
			sendMsg.SendWaitTime = 1000;	//等待时间为1000ms
			return sendMsg;
		}

		/*
		 * 响应终端关机
		 */
		public static SerialSendMsg TerminalShutdownAckMsg(byte[] TerminalSerialNum)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x10;
			sdp.Path = 0x00;
			sdp.Cmd = 0x24;
			sdp.DataFiled[0] = 0x11;
			Array.Copy(TerminalSerialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(padChars, 0, sdp.DataFiled, 5, 11);		//填充6字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0
			return sendMsg;
		}

		/*
		 * 回复"主机查询"命令
		 */
		public static SerialSendMsg ServerQueryAckMsg(byte[] TargetSerialNum, byte[] ControlSerialNum, byte channel)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x80;
			sdp.Path = 0x40;
			sdp.Cmd = 0x32;
			sdp.DataFiled[0] = 0x12;
			Array.Copy(TargetSerialNum, 0, sdp.DataFiled, 1, 4);
			Array.Copy(ControlSerialNum, 0, sdp.DataFiled, 5, 4);
			sdp.DataFiled[9] = channel;
			Array.Copy(padChars, 0, sdp.DataFiled, 10, 6);		//填充7字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0
			return sendMsg;
		}

		/*
		 * 主机切换波特率或信道
		 */
		public static SerialSendMsg SwitchChannelMsg(byte channel)
		{
			SerialDataPacket sdp = new SerialDataPacket();

			sdp.Cmd = 0xFE;

			sdp.DataFiled[0] = 0xC2;
			sdp.DataFiled[1] = 0x00;
			sdp.DataFiled[2] = 0x00;
			sdp.DataFiled[3] = 0x1B;
			sdp.DataFiled[4] = channel;
			sdp.DataFiled[5] = 0x44;

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为1000
			return sendMsg;
		}

		/*
		 * 主机配置无线模块
		 */
		public static SerialSendMsg WirelessConfigMsg(byte channel)
		{
			SerialDataPacket sdp = new SerialDataPacket();
			sdp.Direct = 0x00;
			sdp.Path = 0x00;
			sdp.Cmd = 0xFE;
			sdp.DataFiled[0] = 0x11;
			sdp.DataFiled[1] = channel;
			Array.Copy(padChars, 0, sdp.DataFiled, 2, 14);		//填充14字节的0x55
			sdp.CheckSum = AppUtil.GetChecksum(sdp.DataBytes, 2, 19);

			SerialSendMsg sendMsg = new SerialSendMsg();
			sendMsg.PacketData = sdp;
			sendMsg.PackNumPerCmd = 1;		//每次命令连续发送1个相同的包
			sendMsg.SendNumMax = 1;			//最大发送次数为1, 不重发
			sendMsg.IsWaitAck = false;		//不等待响应
			sendMsg.SendWaitTime = 0;		//等待时间为0
			return sendMsg;
		}
	}
}
