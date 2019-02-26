using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
//using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using Aspose.Words;
using System.Net;
using System.Drawing;
using SCBAControlHost;
using log4net;

namespace MyUtils
{
	class AppUtil
	{
		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		//根据控件名 来 提取uid号
		public static int getUidByControlName(string controlName)
		{
			string strSplit1 = Regex.Replace(controlName, "[a-z]", "", RegexOptions.IgnoreCase);
			int uid = Convert.ToInt32(strSplit1);
			return uid;
		}
		
		//获取校验和
		public static byte GetChecksum(byte[] data, int offset, int Length)
		{
			int i = offset + 1;
			byte res = data[offset];
			Length--;
			while (Length > 0)
			{
				res ^= data[i];
				i++;
				Length--;
			}
			return res;
		}

		//递归地设置控件及其子控件的Tag, 在创建一个控件时调用
		public static void setControlsTag(Control parent)
		{
			parent.Tag = parent.Width + ":" + parent.Height + ":" + parent.Left + ":" + parent.Top + ":" + parent.Font.Size;
			foreach (Control con in parent.Controls)
			{
				setControlsTag(con);
			}
		}

		//获取异常相关的信息
		public static string getExceptionInfo(Exception error)
		{
			string str = "";
			if (error != null)
			{
				str = string.Format("出现已捕获的异常：\r\n" + "异常类型:{0}\r\n异常信息:{1}\r\n堆栈信息:{2}\r\n",
				error.GetType(), error.Message, error.StackTrace);
			}
			else
			{
				str = string.Format("应用程序线程错误:{0}", error);
			}

			return str;
		}

		/*
		 * 查找包含多个关键字符串的第一个串口
		 * 例如要找出CH340串口号是多少: 其串口名称为"USB-SERIAL CH340 (COM8)", 关键字符串为"SERIAL"和"CH340", 则函数返回"COM8"
		 */
		public static string FindComByKeyStr(string[] keyStr)
		{
			string comName = null;

			//串口枚举, 获取每个串口名称的字符串数组
			if (keyStr != null)
			{
				string[] CurSerialPort = SerialEnumPort.enumSerialPortGetName();
				if (CurSerialPort != null)
				{
					foreach (string str in CurSerialPort)
					{
						bool isContanKey = true;
						foreach (string ketstr in keyStr)
						{
							if (!str.Contains(ketstr))
							{
								isContanKey = false;
								break;
							}

						}
						if (isContanKey)		//若名称包含关键字符串
						{
							int leftBracketIndex = 0;
							try { leftBracketIndex = str.LastIndexOf("("); }
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
							if (leftBracketIndex != 0)
							{
								int tmp = 0;
								if (int.TryParse(str.Substring(leftBracketIndex + 5, 1), out tmp))
									comName = str.Substring(leftBracketIndex + 1, 5);
								else
									comName = str.Substring(leftBracketIndex + 1, 4);
								break;
							}
						}
					}
				}
			}
			
			return comName;
		}

		//将Word文档转为HTML网页形式
		public static void ConvertDocToHtml(string SourcePath, string TargetPath)
		{
			try
			{
				Aspose.Words.Document d = new Aspose.Words.Document(SourcePath);
				d.Save(TargetPath, SaveFormat.Html);
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
		}

		//拷贝文件到特定的目录下, SourceFilePath-源文件的全路径名, TargetDirPath-目标目录路径
		public static bool CopyFileTo(string SourceFilePath, string TargetDirPath)
		{
			if (File.Exists(SourceFilePath) && Directory.Exists(TargetDirPath))		//若源文件 和 目标路径都存在
			{
				string TargetFilePath = TargetDirPath + "\\" + Path.GetFileName(SourceFilePath);	//目标文件的全路径名
				if (!File.Exists(TargetFilePath))	//若目标目录下没有同名的文件
				{
					FileInfo file = new FileInfo(SourceFilePath);
					try
					{
						file.CopyTo(TargetFilePath, true);	// true is overwrite
						return true;
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
				}
				else
					MessageBox.Show("已存在一个名为 " +Path.GetFileName(SourceFilePath) + " 的文件");
			}
			return false;
		}

		/// 从一个目录将其内容移动到另一目录  
		/// </summary>
		/// <param name="directorySource">源目录</param>
		/// <param name="directoryTarget">目标目录</param>
		public static void MoveFolderTo(string directorySource, string directoryTarget)
		{
			//检查是否存在目的目录
			if (!Directory.Exists(directoryTarget))
			{
				Directory.CreateDirectory(directoryTarget);
			}
			//先来移动文件  
			DirectoryInfo directoryInfo = new DirectoryInfo(directorySource);
			FileInfo[] files = directoryInfo.GetFiles();
			//移动所有文件  
			foreach (FileInfo file in files)
			{
				//如果自身文件在运行，不能直接覆盖，需要重命名之后再移动  
				if (File.Exists(Path.Combine(directoryTarget, file.Name)))
				{
					if (File.Exists(Path.Combine(directoryTarget, file.Name + ".bak")))
					{
						File.Delete(Path.Combine(directoryTarget, file.Name + ".bak"));
					}
					File.Move(Path.Combine(directoryTarget, file.Name), Path.Combine(directoryTarget, file.Name + ".bak"));

				}
				file.MoveTo(Path.Combine(directoryTarget, file.Name));

			}
			//最后移动目录  
			DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories();
			foreach (DirectoryInfo dir in directoryInfoArray)
			{
				MoveFolderTo(Path.Combine(directorySource, dir.Name), Path.Combine(directoryTarget, dir.Name));
			}
		}

		//比较两个byte数组其中某些元素是否相等
		public static bool IsBytesEqual(byte[] array1, int offset1, byte[] array2, int offset2, int length)
		{
			if ((array1 == null) || (array2 == null))
				return false;
			if ((array1.Length < (offset1 + length)) || (array2.Length < (offset2 + length)))
				return false;
			for (int i = 0; i < length; i++)
			{
				if (array1[offset1 + i] != array2[offset2 + i])
					return false;
			}
			return true;
		}

		//将bytes数组中连续的几个字节转为int型
		public static int bytesToInt(byte[] array1, int offset, int length)
		{
			int tmp = 0;
			if (array1.Length > (offset + length))
			{
				for (int i = 0; i < length; i++)
				{
					tmp = tmp * 256 + (int)(array1[offset + i]);
				}
			}
			return tmp;
		}

		//int转4字节的byte数组
		public static byte[] IntToBytes(int src)
		{
			byte[] res = new byte[4];
			res[0] = (byte)(src >> 24);
			res[1] = (byte)((src & 0x00FFFFFF) >> 16);
			res[2] = (byte)((src & 0x0000FFFF) >> 8);
			res[3] = (byte)((src & 0x000000FF));

			return res;
		}


		//将byte数组中连续的几个字节转为int型, 再转为字符串
		//public static string bytesToIntString(byte[] array1, int offset, int length)
		//{
		//    string res = null;

		//    int tmp = 0;
		//    if(array1.Length > (offset+length))
		//    {
		//        for (int i = 0; i < length; i++)
		//        {
		//            tmp = tmp*256 + (int)(array1[offset + i]);
		//        }
		//        res = tmp.ToString();
		//    }
		//    return res;
		//}

		//从byte数组中提取出连续的几个字节, 并作为新的byte数组返回
		public static byte[] ExtractBytes(byte[] srcArray, int offset, int length)
		{
			byte[] res = new byte[length];

			Array.Copy(srcArray, offset, res, 0, length);

			return res;
		}

		//将组号和终端号转为4字节数组
		public static byte[] IntSerialToBytes(int GrpNO, int terminalNO)
		{
			byte[] res = new byte[4];

			res[0] = (byte)((GrpNO & 0x00FF0000) >> 16);
			res[1] = (byte)((GrpNO & 0x0000FF00) >> 8);
			res[2] = (byte)((GrpNO & 0x000000FF));
			res[3] = (byte)(terminalNO);

			return res;
		}

		//根据随机数和密码获取响应字节
		public static byte[] GetMD5FromRandPwd(byte[] rand, string pwd)
		{
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			List<byte> byteSource = new List<byte>();
			byteSource.AddRange(rand);
			byteSource.AddRange(System.Text.Encoding.ASCII.GetBytes(pwd));

			byte[] inputBytes = byteSource.ToArray();
			byte[] hashBytes = md5.ComputeHash(inputBytes);
			//StringBuilder sb = new StringBuilder();
			//for (int i = 0; i < hashBytes.Length; i++)
			//{
			//    sb.Append(hashBytes[i].ToString("X2"));
			//}
			//string md5Value = sb.ToString();

			return (new byte[4] { hashBytes[12], hashBytes[13], hashBytes[14], hashBytes[15] });
		}

		//ping主机以检测主机是否在线
		public static bool PingServerAlive(string ip, int timeout)
		{
			if (SystemInformation.Network)		//已经联网，但是不确定是互联网还是局域网
			{
				Ping ping = new Ping();
				try
				{
					PingReply pr = ping.Send(ip, timeout);	//超时时间为1000ms
					if (pr.Status == IPStatus.Success)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex) {
					Console.WriteLine(ex.Message);
					//log.Info(AppUtil.getExceptionInfo(ex));
					return false;
				}
			}
			else
				return false;
		}

		//Http下载文件, retryNum为重试次数
		public static bool HttpDownloadFile(string URLAddress, string filePath, int retryNum)
		{
			bool res = false;
			if (URLAddress != null && filePath != null)
			{
				for (int i = 0; i < retryNum; i++)
				{
					try
					{
						WebClient wc = new WebClient();
						//若文件存在则删除
						if (File.Exists(filePath))
							File.Delete(filePath);
						//若目录不存在则创建目录
						if (!Directory.Exists(Path.GetDirectoryName(filePath)))
							Directory.CreateDirectory(Path.GetDirectoryName(filePath));
						wc.DownloadFile(URLAddress, filePath);
						res = true;
						break;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
						res = false;
					}
				}
			}

			return res;
		}

		//获取文件的MD5值
		public static string GetMD5HashFromFile(string fileName)
		{
			try
			{
				FileStream file = new FileStream(fileName, FileMode.Open);
				System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(file);
				file.Close();
 
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < retVal.Length; i++)
				{
					sb.Append(retVal[i].ToString("x2"));
				}
				return sb.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
			}
		}

		//删除不需要的文件
		public static void FileClean(string dirToClean, List<string> fileExist)
		{
			if (Directory.Exists(dirToClean))	//若目录存在
			{
				//先清理所有不应该存在的文件
				string[] SubFile = Directory.GetFiles(dirToClean);
				foreach (string filePath in SubFile)
				{
					if (!fileExist.Contains(Path.GetFullPath(filePath)))	//若列表中不包含, 则删除
						File.Delete(filePath);
				}
				//再清理所有不应该存在的子目录及其内部文件
				string[] SubDir = Directory.GetDirectories(dirToClean);
				foreach (string dirPath in SubDir)
				{
					FileClean(dirPath, fileExist);
				}

				//删除空目录
				if ((Directory.GetDirectories(dirToClean).Length + Directory.GetFiles(dirToClean).Length) == 0)
					Directory.Delete(dirToClean);
			}
		}

		public static string GetNetDateTime(string ip)
		{
			WebRequest request = null;
			WebResponse response = null;
			WebHeaderCollection headerCollection = null;
			string datetimeStr = null;
			try
			{
				request = WebRequest.Create("http://" + ip + "/");
				request.Timeout = 3000;
				request.Credentials = CredentialCache.DefaultCredentials;
				response = (WebResponse)request.GetResponse();
				headerCollection = response.Headers;
				foreach (var h in headerCollection.AllKeys)
				{ if (h == "Date") { datetimeStr = headerCollection[h]; } }
				return datetimeStr;
			}
			catch (Exception) { return datetimeStr; }
			finally
			{
				if (request != null)
				{ request.Abort(); }
				if (response != null)
				{ response.Close(); }
				if (headerCollection != null)
				{ headerCollection.Clear(); }
			}
		} 

		//public static string int2String8Char(int num)
		//{
		//    string res = "";

		//    if(num)

		//    return res;
		//}

		/// <summary> 
		/// 字符串转16进制字节数组 
		/// </summary> 
		/// <param name="hexString"></param> 
		/// <returns></returns> 
		public static byte[] strToHexByte(string hexString)
		{
			hexString = hexString.Replace(" ", "");
			if ((hexString.Length % 2) != 0)
				hexString += " ";
			byte[] returnBytes = new byte[hexString.Length / 2];
			for (int i = 0; i < returnBytes.Length; i++)
				returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			return returnBytes;
		}

		/// <summary> 
		/// 16进制字节数组转字符串
		/// </summary> 
		/// <returns></returns> 
		public static string hexByteToString(byte[] inByte)
		{
			string res = "";
			for(int i = 0; i < inByte.Length; i++)
				res = res + inByte[i].ToString("X2");

			return res;
		}

		//检测日志文件的文件名是否符合规范
		public static bool CheckCSVLogFileName(string name)
		{
			bool res = false;
			try
			{
				if (name != null)
				{
					if (Path.GetExtension(name) == ".CSV" || Path.GetExtension(name) == ".csv")
					{
						string[] tmp = name.Split('+');
						if (tmp.Length == 3)
						{
							res = true;
						}
						else
							res = false;
					}
					else
						res = false;
				}
				else
					res = false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				res = false;
			}

			return res;
		}

		//使用程序来移动鼠标点击控件
		public static void ClickControl(Control con)
		{
			Point controlPosInScreen;
			controlPosInScreen = con.PointToScreen(new Point(con.Size.Width / 2, con.Size.Height / 2));
			Win32APICall.SimulateMouseClick(controlPosInScreen);
		}
		
		//使用程序来移动鼠标点击控件
		public static void SetControlPos(Control conRef, Control conContainer, Control conMove)
		{
			Point conRefPosInScreen;
			conRefPosInScreen = conRef.PointToScreen(new Point(0, 0));
			Point conContainerPosInScreen;
			conContainerPosInScreen = conContainer.PointToScreen(new Point(0, 0));
			

			conMove.Location = new Point(conContainerPosInScreen.X - conRefPosInScreen.X + conContainer.Width/2, conContainerPosInScreen.Y - conRefPosInScreen.Y + conContainer.Height/2);
			conMove.BringToFront();
		}

		////使用程序来移动鼠标点击控件
		//public static void SetControlPosCentre(Control conContainer, Control conMove)
		//{
		//    conMove.Parent = conContainer;
		//    conMove.Location = new Point(conContainer.Width / 2, conContainer.Height / 2);
		//    conMove.BackColor = Color.Transparent;
		//    conMove.BringToFront();
		//}

	}
}
