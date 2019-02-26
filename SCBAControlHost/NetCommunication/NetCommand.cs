using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using MyUtils;

namespace SCBAControlHost.NetCommunication
{
	public class NetCommand
	{
		private static byte[] padChars = new byte[16] { 0x55, 0x55, 0x55, 0x55, 
												 0x55, 0x55, 0x55, 0x55,
												 0x55, 0x55, 0x55, 0x55,
												 0x55, 0x55, 0x55, 0x55 };

		//验证包
		public static NetPacket NetAuthPacket(byte[] rand, string account, string pwd, string ip)
		{
			NetPacket np = new NetPacket();

			//byte[] byteAccount = System.Text.Encoding.UTF8.GetBytes(account);
			//byte[] bytePwd = System.Text.Encoding.UTF8.GetBytes(pwd);
			//np.DataLength = 4 + byteAccount.Length;

			//np.PacketType = 0x01;
			//np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			//np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			//np.datafield = new byte[np.DataLength];

			//byte[] hashPwd = new byte[4 + bytePwd.Length];
			//Array.Copy(rand, 0, hashPwd, 0, 4);
			//Array.Copy(bytePwd, 0, hashPwd, 4, bytePwd.Length);
			//MD5 md5 = new MD5CryptoServiceProvider();
			//byte[] targetData = md5.ComputeHash(hashPwd);
			//Array.Copy(targetData, 12, np.datafield, 0, 4);
			//Array.Copy(byteAccount, 0, np.datafield, 4, byteAccount.Length);

			byte[] byteAccount = System.Text.Encoding.UTF8.GetBytes(account);
			byte[] bytePwd = System.Text.Encoding.UTF8.GetBytes(pwd);
			np.DataLength = bytePwd.Length + byteAccount.Length + 1;

			np.PacketType = 0x01;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];

			DateTime dt = DateTime.Now;
			string tmp = null;
			tmp = AppUtil.GetNetDateTime(ip);
			if(tmp != null)
				dt = Convert.ToDateTime(tmp);
			byte addNum = (byte)dt.Hour;
			for (int i = 0; i < bytePwd.Length; i++)
				bytePwd[i] += addNum;

			Array.Copy(bytePwd, 0, np.datafield, 0, bytePwd.Length);
			np.datafield[bytePwd.Length] = 0x2B;
			Array.Copy(byteAccount, 0, np.datafield, bytePwd.Length + 1, byteAccount.Length);

			return np;
		}

		//用户信息上传包--单个用户
		public static NetPacket NetUploadUserPacket(User user)
		{
			NetPacket np = new NetPacket();
			np.DataLength = 18;
			np.PacketType = 0x05;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];

			int userNO;
			if(int.TryParse(user.BasicInfo.userNO, out userNO))
				np.datafield[0] = (byte)userNO;
			else
				np.datafield[0] = 1;
			Array.Copy(AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO), 0, np.datafield, 1, 4);	//填充序列号
			Array.Copy(User.GetPressBytesByDouble(user.TerminalInfo.Pressure), 0, np.datafield, 5, 2);		//气压值
			Array.Copy(User.GetVoltageBytesByDouble(user.TerminalInfo.Voltage), 0, np.datafield, 7, 2);		//电压值
			np.datafield[9] = User.GetTemeratureByteByInt(user.TerminalInfo.Temperature);					//温度值
			Array.Copy(User.GetTimeBytesByInt(user.TerminalInfo.PowerONTime), 0, np.datafield, 10, 2);		//上电时间值
			Array.Copy(padChars, 0, np.datafield, 12, 3);													//填充值
			np.datafield[15] = (byte)(user.UStatus);														//终端状态
			Array.Copy(User.GetTimeBytesByInt(user.TerminalInfo.RemainTime), 0, np.datafield, 16, 2);		//剩余时间

			return np;
		}

		//用户信息上传包--多个用户
		public static NetPacket NetUploadUsersPacket(List<User> users)
		{
			NetPacket np = new NetPacket();
			np.DataLength = users.Count * 18;
			np.PacketType = 0x05;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];

			int i = 0;
			foreach (User user in users)
			{
				int userNO;
				if (int.TryParse(user.BasicInfo.userNO, out userNO))
					np.datafield[16 * i] = (byte)userNO;
				else
					np.datafield[16 * i] = 1;
				Array.Copy(AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO), 0, np.datafield, 18 * i + 1, 4);	//填充序列号
				Array.Copy(User.GetPressBytesByDouble(user.TerminalInfo.Pressure), 0, np.datafield, 18 * i + 5, 2);		//气压值
				Array.Copy(User.GetVoltageBytesByDouble(user.TerminalInfo.Voltage), 0, np.datafield, 18 * i + 7, 2);	//电压值
				np.datafield[18 * i + 9] = User.GetTemeratureByteByInt(user.TerminalInfo.Temperature);					//温度值
				Array.Copy(User.GetTimeBytesByInt(user.TerminalInfo.PowerONTime), 0, np.datafield, 18 * i + 10, 2);		//上电时间值
				Array.Copy(padChars, 0, np.datafield, 18 * i + 12, 3);													//填充值
				np.datafield[18 * i + 15] = (byte)(user.UStatus);														//终端状态
				Array.Copy(User.GetTimeBytesByInt(user.TerminalInfo.RemainTime), 0, np.datafield, 18 * i + 16, 2);		//剩余时间
				i++;
			}

			return np;
		}

		/*
		 * 地点改变数据包
		 * address--改变之后的地址
		 */
		public static NetPacket NetAddChangePacket(string address)
		{
			byte[] tmpBytes = Encoding.UTF8.GetBytes(address);

			NetPacket np = new NetPacket();
			np.DataLength = tmpBytes.Length;
			np.PacketType = 0x07;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];
			Array.Copy(tmpBytes, np.datafield, tmpBytes.Length);

			return np;
		}

		/*
		 * 任务改变数据包
		 * tesk--改变之后的任务
		 */
		public static NetPacket NetTaskChangePacket(string tesk)
		{
			byte[] tmpBytes = Encoding.UTF8.GetBytes(tesk);

			NetPacket np = new NetPacket();
			np.DataLength = tmpBytes.Length;
			np.PacketType = 0x09;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];
			Array.Copy(tmpBytes, np.datafield, tmpBytes.Length);

			return np;
		}

		/*
		 * 获取文件URI包
		 * fileType--文件类型  0x01-知识库和设备库URI列表文件
		 *					   0x02-用户Excel和用户头像URI列表文件
		 */
		//public static NetPacket NetGetFileURIPacket(byte fileType)
		//{
		//    NetPacket np = new NetPacket();
		//    np.DataLength = 1;
		//    np.PacketType = 0x07;
		//    np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
		//    np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
		//    np.datafield = new byte[np.DataLength];
		//    np.datafield[0] = fileType;

		//    return np;
		//}

		/*
		 * 请求上传日志包
		 * reqype--请求类型     0x01-请求最新日志文件名
		 *					   0x02-请求post的处理页面URI
		 */
		//public static NetPacket NetUploadLogPacket(byte reqype)
		//{
		//    NetPacket np = new NetPacket();
		//    np.DataLength = 1;
		//    np.PacketType = 0x09;
		//    np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
		//    np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
		//    np.datafield = new byte[np.DataLength];
		//    np.datafield[0] = reqype;

		//    return np;
		//}

		/*
		 * 心跳包
		 */
		public static NetPacket NetHeartBeatPacket()
		{
			NetPacket np = new NetPacket();
			np.DataLength = 1;
			np.PacketType = 0x30;
			np.DataLength_HighByte = (byte)((np.DataLength & 0xFF00) >> 8);
			np.DataLength_LowByte = (byte)(np.DataLength & 0x00FF);
			np.datafield = new byte[np.DataLength];
			np.datafield[0] = 0x00;

			return np;
		}
	}
}
