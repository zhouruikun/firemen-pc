﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using log4net;

namespace SCBAControlHost
{
    static class Program
    {
		private static ILog log = LogManager.GetLogger("ErrorNoCatch.Logging");//获取一个日志记录器

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
			try
			{
				//处理未捕获的异常
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				//处理UI线程异常
				Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
				//处理非UI线程异常
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
				log4net.Config.XmlConfigurator.Configure();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FormMain());

			}
			catch (Exception ex)
			{
				string str = "";
				string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";

				if (ex != null)
				{
					str = string.Format("出现未处理的异常：\r\n" + "异常类型:{0}\r\n异常信息:{1}\r\n堆栈信息:{2}\r\n",
					ex.GetType(), ex.Message, ex.StackTrace);
				}
				else
				{
					str = string.Format("应用程序线程错误:{0}", ex);
				}

				//MessageBox.Show(str, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//LogManager.WriteLog(str);
				log.Error(str);
			}
        }

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			string str = "";
			Exception error = e.Exception as Exception;
			if (error != null)
			{
				str = string.Format("出现未处理的异常：\r\n" + "异常类型:{0}\r\n异常信息:{1}\r\n堆栈信息:{2}\r\n",
				error.GetType(), error.Message, error.StackTrace);
			}
			else
			{
				str = string.Format("应用程序线程错误:{0}", e);
			}

			log.Error(str);
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			string str = "";
			Exception error = e.ExceptionObject as Exception;
			if (error != null)
			{
				str = string.Format("出现未处理的异常：\r\n" + "异常类型:{0}\r\n异常信息:{1}\r\n堆栈信息:{2}\r\n",
				error.GetType(), error.Message, error.StackTrace);
			}
			else
			{
				str = string.Format("Application UnhandledError:{0}", e);
			}

			log.Error(str);
		}
    }
}
