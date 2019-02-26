using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Specialized;

namespace SCBAControlHost.MyUtils
{
	class HttpHelper
	{
		private static readonly Encoding DEFAULTENCODE = Encoding.UTF8;
		public static string ResponseText = "";
		private static byte[] responseBytesRead = null;
		public static string _token = "";		//令牌
		public static CookieContainer cookieContainer = new CookieContainer();

		//发送http请求
		private static HttpWebResponse Request(string PageURI, NameValueCollection httpHeader, byte[] postBytes, string protocol)
		{
			HttpWebRequest request = null;
			HttpWebResponse response = null;

			try
			{
				request = (HttpWebRequest)HttpWebRequest.Create(PageURI);	//http://106.14.226.150/login

				//http请求头
				//request.AllowAutoRedirect = true;
				request.KeepAlive = true;			//建立持久性连接
				request.CookieContainer = cookieContainer;

				if (httpHeader != null)
				{
					foreach (string key in httpHeader.Keys)
					{
						switch (key)
						{
							case "UserAgent":
								request.UserAgent = httpHeader.Get(key);
								break;

							case "Method":
								request.Method = httpHeader.Get(key);
								break;

							case "ContentType":
								request.ContentType = httpHeader.Get(key);
								break;

							default:
								break;
						}
					}
				}

				//http请求体
				if (request.Method == "POST")
				{
					request.ContentLength = postBytes.Length;
					//发送数据  using结束代码段释放
					using (Stream requestStm = request.GetRequestStream())
					{
						requestStm.Write(postBytes, 0, postBytes.Length);
					}
				}

				//响应
				response = (HttpWebResponse)request.GetResponse();

				ResponseText = string.Empty;
				using (Stream responseStm = response.GetResponseStream())
				{
					StreamReader redStm = new StreamReader(responseStm, Encoding.UTF8);
					ResponseText = redStm.ReadToEnd();
					responseBytesRead = Encoding.UTF8.GetBytes(ResponseText);
				}

				//获取令牌
				int index = ResponseText.IndexOf("input type=\"hidden\" name=\"_token\" value=\"");
				if (index > 0)
				{
					_token = ResponseText.Substring(index + 41, 40);
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return response;
		}

		private static HttpWebResponse RequestWithDownload(string PageURI, NameValueCollection httpHeader, string filePath, string protocol)
		{
			HttpWebRequest request = null;
			HttpWebResponse response = null;

			try
			{
				request = (HttpWebRequest)HttpWebRequest.Create(PageURI);	//http://106.14.226.150/login

				//http请求头
				request.AllowAutoRedirect = true;
				request.KeepAlive = true;			//建立持久性连接
				request.CookieContainer = cookieContainer;

				if (httpHeader != null)
				{
					foreach (string key in httpHeader.Keys)
					{
						switch (key)
						{
							case "UserAgent":
								request.UserAgent = httpHeader.Get(key);
								break;

							case "Method":
								request.Method = httpHeader.Get(key);
								break;

							case "ContentType":
								request.ContentType = httpHeader.Get(key);
								break;

							default:
								break;
						}
					}
				}

				//响应
				response = (HttpWebResponse)request.GetResponse();
				using (Stream responseStm = response.GetResponseStream())
				{
					FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
					byte[] buffer = new byte[512];
					long contentLength = response.ContentLength;

					int length = responseStm.Read(buffer, 0, buffer.Length);
					while (length > 0)
					{
						fs.Write(buffer, 0, length);
						buffer = new byte[512];
						length = responseStm.Read(buffer, 0, buffer.Length);
					}
					fs.Close();
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return response;
		}

		//登录服务器
		public static int LoginServer(string PageURI, string uName, string uPasswd)
		{
			int res = 0;	//登录结果  0-网络问题  1-登录成功  2-密码错误
			HttpWebResponse response = null;

			try
			{
				//1. 获取登录页面
				NameValueCollection header = new NameValueCollection();
				header["Method"] = "GET";
				response = Request(PageURI, header, null, "HTTP");
				if (response != null)
				{
					if ((int)response.StatusCode < 400)
					{
						//2. 登录
						header.Clear();
						header["Method"] = "POST";
						header["ContentType"] = "application/x-www-form-urlencoded";

						string postString = string.Format("_token={0}&username={1}&password={2}", _token, uName, uPasswd);
						response = Request(PageURI, header, Encoding.UTF8.GetBytes(postString), "HTTP");
						if ((int)response.StatusCode < 400)
						{
							if (response.ResponseUri.AbsolutePath == "/home")		//如果响应是由home页面发来的, 则代表登录成功
								res = 2;
							else
								res = 1;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				res = 0;
			}

			return res;
		}

		//上传文件
		public static bool UploadFile(string PageURI, string file)
		{
			bool res = false;
			HttpWebResponse response = null;

			try
			{
				//1. 获取上传页面
				NameValueCollection header = new NameValueCollection();
				header["Method"] = "GET";
				response = Request(PageURI, header, null, "HTTP");
				if (response != null)
				{
					if ((int)response.StatusCode < 400)
					{
						//2. 上传
						string boundary = "----" + DateTime.Now.Ticks.ToString("x");
						byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
						byte[] tmpBytes;
						string tmpString;

						header.Clear();
						header["Method"] = "POST";
						header["ContentType"] = "multipart/form-data; boundary=" + boundary;
						//构造上传的请求体
						MemoryStream ms = new MemoryStream();			//开辟一个内存缓冲区

						//(1) 填入文件数据
						//(1.1) 填入分隔符
						ms.Write(boundaryBytes, 0, boundaryBytes.Length);
						//(1.2) 填入头信息
						tmpString = string.Format("Content-Disposition: form-data; name=\"fileName\"; filename=\"{0}\"\r\nContent-Type: application/vnd.ms-excel\r\n\r\n",
														 Path.GetFileName(file));
						tmpBytes = DEFAULTENCODE.GetBytes(tmpString);
						ms.Write(tmpBytes, 0, tmpBytes.Length);
						//(1.3) 填入文件数据
						int bytesRead = 0;
						byte[] buffer = new byte[4096];
						using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
						{
							while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
							{
								ms.Write(buffer, 0, bytesRead);
							}
						}

						//(2) 填入 _token
						//(2.1) 填入分隔符
						ms.Write(boundaryBytes, 0, boundaryBytes.Length);
						//(2.2) 填入头信息
						tmpString = "Content-Disposition: form-data; name=\"_token\"\r\n\r\n";
						tmpBytes = DEFAULTENCODE.GetBytes(tmpString);
						ms.Write(tmpBytes, 0, tmpBytes.Length);
						//(2.3) 填入_token
						tmpBytes = DEFAULTENCODE.GetBytes(_token);
						ms.Write(tmpBytes, 0, tmpBytes.Length);
						//(2.4) 填入分隔符
						ms.Write(boundaryBytes, 0, boundaryBytes.Length);

						response = Request(PageURI, header, ms.GetBuffer(), "HTTP");
						if ((int)response.StatusCode < 400)
						{
							res = true;
						}
						else
							res = false;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				res = false;
			}

			return res;
		}

		//下载文件
		public static bool DownloadFile(string PageURI, string filePath)
		{
			bool res = false;
			HttpWebResponse response = null;

			try
			{
				//1. 若文件存在, 则先删除文件
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}

				//2. 若目录不存在, 则创建目录
				if (!Directory.Exists(Path.GetDirectoryName(filePath)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				}

				//2. 以Get方式下载文件
				NameValueCollection header = new NameValueCollection();
				header["Method"] = "GET";
				response = RequestWithDownload(PageURI, header, filePath, "HTTP");
				if (response != null)
				{
					res = true;
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

		//删除文件
		public static bool DeleteFile(string PageURI, int fileIndex)
		{
			bool res = false;
			HttpWebResponse response = null;

			try
			{
				//1. 获取上传页面
				NameValueCollection header = new NameValueCollection();
				header["Method"] = "GET";
				response = Request(PageURI + "/" + fileIndex, header, null, "HTTP");
				if (response != null)
				{
					if ((int)response.StatusCode < 400)
						res = true;
					else
						res = false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				res = false;
			}

			return res;
		}

		public static List<Cookie> GetAllCookies(CookieContainer cc)
		{
			List<Cookie> lstCookies = new List<Cookie>();
			Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
				System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

			foreach (object pathList in table.Values)
			{
				SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
					| System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
				foreach (CookieCollection colCookies in lstCookieCol.Values)
					foreach (Cookie c in colCookies) lstCookies.Add(c);
			}
			return lstCookies;
		}

	}
}
