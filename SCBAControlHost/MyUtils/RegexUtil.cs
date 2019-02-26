using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SCBAControlHost.MyUtils
{
	public class RegexUtil
	{
		//验证是否是纯数字
		public static bool RegexCheckNumber(string s)
		{
			string reg = @"^\d+$";
			return Regex.IsMatch(s, reg);
		}

		//验证6位数字密码
		public static bool RegexCheck6Pwd(string s)
		{
			string reg = @"^\d{6}$";
			return Regex.IsMatch(s, reg);
		}

		//匹配包含大小写字母, 数字, 下划线的密码
		public static bool RegexCheckPwd(string s)
		{
			string reg = "^[\\w]+$";
			return Regex.IsMatch(s, reg);
		}

		//验证IP地址
		public static bool RegexCheckIP(string s)
		{
			string reg = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
			return Regex.IsMatch(s, reg);
		}

		//验证串口号
		public static bool RegexCheckCOM(string s)
		{
			string reg = @"^COM\d{1,2}$";
			return Regex.IsMatch(s, reg);
		}
	}
}
