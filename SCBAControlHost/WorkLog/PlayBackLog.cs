using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;
using MyUtils;
using log4net;

namespace SCBAControlHost
{
	public class PlayBackLog
	{
		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		StreamReader sr = null;
		
		//打开一个csv记录文件
		public bool OpenFile(string filePath)
		{
			if (File.Exists(filePath))		//若文件存在
			{
				if (Path.GetExtension(filePath) == ".csv")	//若文件的后缀名为csv
				{
					try
					{
						sr = new StreamReader(filePath, Encoding.Default);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
						return false;
					}
				}
				else
					return false;
			}
			else
				return false;
			return true;
		}

		//读取下一行记录
		public List<string> ReadNextLine()
		{
			if (sr != null)
			{
				string str = null;
				str = sr.ReadLine();
				List<string> list = new List<string>();
				MatchCollection mcs = Regex.Matches(str, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
				foreach (Match mc in mcs)
				{
					list.Add(mc.Value);
				}
				return list;
			}
			return null;
		}

		
	}
}
