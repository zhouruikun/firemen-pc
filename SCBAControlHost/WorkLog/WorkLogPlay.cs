using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using MyUtils;
using log4net;

namespace SCBAControlHost
{
	public class WorkLogPlay
	{
		public string filePath;			//文件全路径名
		string DirLevelOne;				//一级目录-年-月
		string DirLevelTwo;				//二级目录-月-日

		FileStream fs;
		StreamWriter sw;
		private Queue<string> logQueue = new Queue<string>();		//日志队列
		public AutoResetEvent LogQueueWaitHandle = new AutoResetEvent(false);	//日志队列等待标志

		public string TaskAddress = "";
		public string UserName = "";

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		//构造函数
		public WorkLogPlay(string userName, string taskAddress)
		{
			UserName = userName;
			TaskAddress = taskAddress;

			DirLevelOne = ".\\res\\WorkLogPlay\\" + DateTime.Now.ToString("yyyy-MM");				//一级目录-年-月
			DirLevelTwo = DirLevelOne + "\\" + DateTime.Now.ToString("MM-dd");					//二级目录-月-日
			filePath = DirLevelTwo + "\\" + UserName + "+" + TaskAddress + "+" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "+Play.csv";		//文件--年月日-时分秒
			//filePath = DirLevelTwo + "\\" + UserName + "+" + "A" + "+" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "+Play.csv";		//文件--年月日-时分秒
			try
			{
				if (!Directory.Exists(DirLevelOne))
					Directory.CreateDirectory(DirLevelOne);
				if (!Directory.Exists(DirLevelTwo))
					Directory.CreateDirectory(DirLevelTwo);
				if (!File.Exists(filePath))
					fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				else
					fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.Default);
				//开启日志写入线程
				Thread logthread = new Thread(PlayLogWriteThread);
				logthread.Name = "写入播放日志线程";
				logthread.IsBackground = true;
				logthread.Start();
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); };
		}

		//修改文件名
		private void ModifyPlayLogFileName()
		{
			// 1. 关闭sw
			sw.Close();
			// 2. 关闭fs
			sw.Close();
			// 3. 修改文件名
			string time = filePath.Split('+')[2].Replace(".csv","");
			string newFilePath = DirLevelTwo + "\\" + UserName + "+" + TaskAddress + "+" + time + "+Play.csv";		//文件--年月日-时分秒
			//string newFilePath = DirLevelTwo + "\\" + UserName + "+" + "A" + "+" + time + "+Play.csv";		//文件--年月日-时分秒
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Move(filePath, newFilePath);
				filePath = newFilePath;
			}

			// 4. 以追加方式重新打开fs
			fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);

			// 5. 重新打开sw
			sw = new StreamWriter(fs, System.Text.Encoding.Default);
		}

		//播放日志写入线程
		private void PlayLogWriteThread()
		{
			int LogQueueItemCount = 0;
			while (true)
			{
				LogQueueWaitHandle.WaitOne();
				LogQueueItemCount = logQueue.Count;
				if (LogQueueItemCount > 0)
				{
					for (int i = 0; i < LogQueueItemCount; i++)
					{
						List<string> rowStr = new List<string>();
						rowStr.Add(" ");
						rowStr[0] = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");		//封装时间

						string resStr;
						lock (logQueue) { resStr = logQueue.Dequeue(); }
						if (resStr.Substring(4, 2) == "07")		//修改地点记录, 重新修改文件名
						{
							TaskAddress = Encoding.UTF8.GetString(AppUtil.strToHexByte(resStr.Substring(10, resStr.Length - 10)));
							ModifyPlayLogFileName();
						}
						else if (resStr.Substring(4, 2) == "10")		//修改用户记录, 重新修改文件名
						{
							UserName = Encoding.UTF8.GetString(AppUtil.strToHexByte(resStr.Substring(10, resStr.Length - 10)));
							ModifyPlayLogFileName();
						}

						rowStr.Add(resStr);

						LogWriteRow(rowStr);	//写入日志信息
					}
				}
			}
		}

		//写入一行日志
		private void LogWriteRow(List<string> rowStr)//table数据写入csv
		{
			string data = "";
			foreach (string tmp in rowStr)
			{
				string str = tmp;
				str = str.Replace("\"", "\"\"");//替换英文引号 英文引号需要换成两个引号
				if (str.Contains(',') || str.Contains('"') || str.Contains('\r') || str.Contains('\n')) //含逗号 引号 换行符的需要放到引号中
					str = string.Format("\"{0}\"", str);
				data += (str+",");
			}
			sw.WriteLine(data);		//这一步会先写到内存中, 需要调用Flush才会真正写到文件中
			sw.Flush();				//立即写入到文件
		}

		//往播放日志队列中投入一条日志
		public void LogPlayQueue_Enqueue(string rowStr)
		{
			lock (logQueue) { logQueue.Enqueue(rowStr); }
			LogQueueWaitHandle.Set();
		}
	}
}
