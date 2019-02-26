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
	public class LogPlayCommand
	{

		//获取用户状态记录--一组用户
		public static string getUserStatusRecord(List<User> users)
		{
			string res = "";
			int count = users.Count;
			res = "5AA505" + (count * 18).ToString("X4");
			
			int i = 0;
			foreach (User user in users)
			{
				res = res + i.ToString("X2") + GenTerminalInfo(user);
				i++;
			}

			return res;
		}
		//获取用户状态记录--单个用户
		public static string getUserStatusRecord(User user)
		{
			string res = "";
			res = "5AA505" + "0012" + "00" + GenTerminalInfo(user);

			return res;
		}

		//获取修改地点记录
		public static string getChangeAddRecord(string address)
		{
			string res = "";
			byte[] tmpBytes = Encoding.UTF8.GetBytes(address);
			res = "5AA507" + tmpBytes.Length.ToString("X4") + AppUtil.hexByteToString(tmpBytes);

			return res;
		}

		//获取修改任务记录
		public static string getChangeTaskRecord(string task)
		{
			string res = "";
			byte[] tmpBytes = Encoding.UTF8.GetBytes(task);
			res = "5AA509" + tmpBytes.Length.ToString("X4") + AppUtil.hexByteToString(tmpBytes);

			return res;
		}

		//获取修改账号记录
		public static string getChangeAccountRecord(string account)
		{
			string res = "";
			byte[] tmpBytes = Encoding.UTF8.GetBytes(account);
			res = "5AA510" + tmpBytes.Length.ToString("X4") + AppUtil.hexByteToString(tmpBytes);

			return res;
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
