using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MyUtils;
using log4net;

namespace SCBAControlHost.AppFuction
{
	/*
	 * 日志维护类, 删除超期的日志文件
	 */
	class LogMaintain
	{
		private const int logExpireDays = 15;		//日志过期期限
		private const string systemLogPath = "./Logs";

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		/*
		 * 删除过期的日志文件
		 */
		public void DeleteExpiredLogFiles()
		{
			DateTime dtNow = DateTime.Now;

			if (Directory.Exists(systemLogPath))	//若路径存在
			{
				DeleteFiles(systemLogPath);
			}
		}

		private void DeleteFiles(string path)
		{
			//获取所有子目录
			string[] SubDir = Directory.GetDirectories(path);
			foreach (string dirPath in SubDir)
			{
				DeleteFiles(dirPath);
			}

			//获取所有子文件
			string[] SubFile = Directory.GetFiles(path);
			foreach (string filePath in SubFile)
			{
				try
				{
					string fileName = Path.GetFileNameWithoutExtension(filePath);	//由文件的全路径获取文件名
					string dateStr = fileName.Substring(fileName.Length - 10, 10);	//获取文件名中的日期字符串
					DateTime dt = DateTime.ParseExact(dateStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
					if ((DateTime.Now - dt).Days > logExpireDays)	//若文件超期, 则删除
					{
						File.Delete(filePath);
					}
				}
				catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			}
		}
	}
}
