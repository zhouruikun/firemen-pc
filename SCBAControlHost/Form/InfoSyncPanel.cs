using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SCBAControlHost.NetCommunication;
using MyUtils;
using System.IO;
using System.Globalization;
using SCBAControlHost.MyUtils;
using System.Security.Cryptography;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		//string LoginURL = "http://106.14.226.150/login";
		//string KnowledgeDownloadURL = "http://106.14.226.150/database/download";
		//string UserInfoDownloadURL = "http://106.14.226.150/firemen/download";
		//string LogUploadURL = "http://106.14.226.150/event/record";

		Thread infoSyncTh = null;		//同步线程

		private void InfoSyncInit()
		{
			btnInfoSyncStart.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnInfoSyncStart.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnInfoSyncCancel.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnInfoSyncCancel.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnInfoSyncReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnInfoSyncReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnInfoSyncReturn.Click += new EventHandler(btnInfoSyncReturn_Click);
			btnInfoSyncStart.Click += new EventHandler(btnInfoSyncStart_Click);
			btnInfoSyncCancel.Click += new EventHandler(btnInfoSyncCancel_Click);
		}


		//开始同步按钮点击事件
		void btnInfoSyncStart_Click(object sender, EventArgs e)
		{
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.InfoSyncPanel, (int)BtnOfInfoSyncPanel.StartSync, null));
			if (!isInfoSyncing)	//若没有正在信息同步
			{
				//if (netcom.isConnected)		//若网络连接正常
				if(true)
				{
					isInfoSyncing = true;
					pictureInfoSyncWait.Visible = true;		//"等待图片"显示出来
					//将所有URI清空
					LatestLogName = null;
					//开启信息同步线程
					infoSyncTh = new Thread(InfoSyncThread);
					infoSyncTh.Name = "信息同步线程";
					infoSyncTh.IsBackground = true;
					infoSyncTh.Start();
				}
				else
					MessageBox.Show("当前网络不可用", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
				MessageBox.Show("正在信息同步中, 请不要重复点击");
		}

		//信息同步线程
		bool UpdateBaseResStartFlag = true;
		bool UpdateUserResStartFlag = true;
		bool UploadResStartFlag = true;

		void InfoSyncThread()
		{
			bool UpdateBaseRes = false;
			bool UpdateUserRes = false;
			bool UploadRes = false;

			//设置各个访问页面
			LoginURL = "http://" + SysConfig.Setting.serverIP + "/login";
			KnowledgeDownloadURL = "http://" + SysConfig.Setting.serverIP + "/database/download";
			UserInfoDownloadURL = "http://" + SysConfig.Setting.serverIP + "/firemen/download";
			LogUploadURL = "http://" + SysConfig.Setting.serverIP + "/event/record";

			//先登录服务器
			int LoginRes = HttpHelper.LoginServer(LoginURL, SysConfig.Setting.accessAccount, SysConfig.Setting.accessPassword);
			if (LoginRes == 2)	//若登录服务器成功
			{
				//写入网络登录Web记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Login, "1"));

				#region 下载知识库和设备库
				try
				{
					if (UpdateBaseResStartFlag)		//若开启了 下载知识库和设备库 功能
					{
						//1. 先下载知识库和设备库 压缩包
						if (HttpHelper.DownloadFile(KnowledgeDownloadURL, @"./res/tmp/KnowledgeFile.zip"))	//若下载成功
						{
							//2. 写入网络下载文件记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetDownloadFile, "KnowledgeFile.zip"));

							//3. 解压压缩包
							ZipHelper.UnZip(@"./res/tmp/KnowledgeFile.zip", @"./res/tmp");

							/*  处理救援知识库  */
							//4. 删除原有的目录
							if (Directory.Exists(@"./res/KnowledgeBase/SrcFiles/同步库"))
								Directory.Delete(@"./res/KnowledgeBase/SrcFiles/同步库", true);
							if (Directory.Exists(@"./res/KnowledgeBase/HtmlFiles/同步库"))
								Directory.Delete(@"./res/KnowledgeBase/HtmlFiles/同步库", true);

							if (Directory.Exists(@"./res/tmp/KnowledgeFile/救援知识库"))		//若下载下来的文件中存在知识库
							{
								//5. 移动目录
								Directory.Move(@"./res/tmp/KnowledgeFile/救援知识库", @"./res/KnowledgeBase/SrcFiles/同步库");
								//6. 重新加载知识库
								lock (m_SyncContext) { m_SyncContext.Send(LoadKnowledgeBase, null); }
							}
							else
							{
								//5. 重新创建目录
								Directory.CreateDirectory(@"./res/KnowledgeBase/SrcFiles/同步库");
								//6. 重新加载知识库
								lock (m_SyncContext) { m_SyncContext.Send(LoadKnowledgeBase, null); }
							}

							/*  处理设备器械库  */
							//4. 删除原有的目录
							if (Directory.Exists(@"./res/DeviceBase/SrcFiles/同步库"))
								Directory.Delete(@"./res/DeviceBase/SrcFiles/同步库", true);
							if (Directory.Exists(@"./res/DeviceBase/HtmlFiles/同步库"))
								Directory.Delete(@"./res/DeviceBase/HtmlFiles/同步库", true);

							if (Directory.Exists(@"./res/tmp/KnowledgeFile/设备器械库"))		//若下载下来的文件中存在设备库
							{
								//5. 移动目录
								Directory.Move(@"./res/tmp/KnowledgeFile/设备器械库", @"./res/DeviceBase/SrcFiles/同步库");
								//6. 重新加载设备库
								lock (m_SyncContext) { m_SyncContext.Send(LoadDeviceBase, null); }
							}
							else
							{
								//5. 重新创建目录
								Directory.CreateDirectory(@"./res/DeviceBase/SrcFiles/同步库");
								//6. 重新加载设备库
								lock (m_SyncContext) { m_SyncContext.Send(LoadDeviceBase, null); }
							}


							//7. 删除缓存文件
							File.Delete(@"./res/tmp/KnowledgeFile.zip");
							Directory.Delete(@"./res/tmp/KnowledgeFile", true);

							UpdateBaseRes = true;
						}
						else
						{
							UpdateBaseRes = false;
							//写入网络下载文件失败记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetDownloadFileFail, "KnowledgeFile.zip,下载失败"));
							m_SyncContext.Send(MessageBoxShow, new MessageBoxInfo("知识库下载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
							//MessageBox.Show("知识库下载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					else
						UpdateBaseRes = true;
				}
				catch (Exception ex)
				{
					UpdateBaseRes = false;
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
				}
				#endregion

				#region 下载用户Excel和头像
				try
				{
					if (UpdateUserResStartFlag)			//若开启了 下载用户Excel和头像 功能
					{
						//1. 先下载知识库和设备库 压缩包
						if (HttpHelper.DownloadFile(UserInfoDownloadURL, @"./res/tmp/UserTable.zip"))	//若下载成功
						{
							//2. 写入网络下载文件记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetDownloadFile, "UserTable.zip"));

							//3. 删除原有的目录
							if (Directory.Exists(@"./res/UserTable"))
								Directory.Delete(@"./res/UserTable", true);

							//4. 解压压缩包
							ZipHelper.UnZip(@"./res/tmp/UserTable.zip", @"./res/tmp");

							//5. 移动文件
							Directory.Move(@"./res/tmp/UserInfo", @"./res/UserTable");


							//6. 删除缓存文件
							File.Delete(@"./res/tmp/UserTable.zip");				//删除压缩包

							//7. 重新加载用户信息表
							lock (m_SyncContext) { m_SyncContext.Send(ReImportUserFromDefaultFile, null); }

							UpdateUserRes = true;
						}
						else
						{
							UpdateUserRes = false;
							//写入网络下载文件失败记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetDownloadFileFail, "UserTable.zip,下载失败"));
							m_SyncContext.Send(MessageBoxShow, new MessageBoxInfo("用户信息下载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
							//MessageBox.Show("用户信息下载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					else
						UpdateUserRes = true;
				}
				catch (Exception ex)
				{
					UpdateUserRes = false;
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
				}
				#endregion

				#region 上传日志部分
				if (UploadResStartFlag)
				{
					GetLatestLogName();		//获取最新日志名

					// 开始上传
					if (UploadLogFiles() && UploadPlayLogFiles())		//上传日志文件
						UploadRes = true;
					else
					{
						UploadRes = false;
						m_SyncContext.Send(MessageBoxShow, new MessageBoxInfo("日志上传失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
						//MessageBox.Show("日志上传失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
					UploadRes = true;
				#endregion

			}
			else if (LoginRes == 1)	//若登录服务器失败
			{
				//写入网络登录记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Login, "2"));
				m_SyncContext.Send(MessageBoxShow, new MessageBoxInfo("登录服务器失败, 账号或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
				//MessageBox.Show("登录服务器失败, 账号或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else	//若登录服务器失败
			{
				//写入网络登录记录到日志文件中
				worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Login, "2"));
				m_SyncContext.Send(MessageBoxShow, new MessageBoxInfo("登录服务器失败, 网络错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error));
				//MessageBox.Show("登录服务器失败, 网络错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			#region 更新信息同步结果显示
			//更新信息同步结果显示
			if (UpdateBaseRes && UpdateUserRes && UploadRes)
				InfoSyncResDisplay("成 功");
			else
				InfoSyncResDisplay("失 败");
			#endregion

			isInfoSyncing = false;			//信息同步线程结束了
		}

		//信息同步界面结果显示
		void InfoSyncResDisplay(string res)
		{
			pictureInfoSyncWait.Invoke(new Action(() => { pictureInfoSyncWait.Visible = false; }));			//隐藏正在同步图片
			richTextInfoSyncStatus.Invoke(new Action(() => { richTextInfoSyncStatus.Text = res; }));		//信息同步结果显示"失败"
		}

		//上传日志文件的函数
		private bool UploadLogFiles()
		{
			try
			{
				//解析最新文件的时间
				DateTime LatestTime = DateTime.ParseExact(LatestLogName.Split('+')[2].Replace(".csv", ""), "yyyyMMdd-HHmmss", CultureInfo.CurrentCulture);	//最新文件的时间
				DateTime LatestTimeLevelOne = new DateTime(LatestTime.Year, LatestTime.Month, 1);					//最新文件一级目录的时间  年-月
				DateTime LatestTimeLevelTwo = new DateTime(LatestTime.Year, LatestTime.Month, LatestTime.Day);		//最新文件二级目录的时间  年-月-日

				if (Directory.Exists(".\\res\\WorkLog"))
				{
					/*------ 处理一级目录 ------*/
					string[] AllLevelOneDirs = Directory.GetDirectories(".\\res\\WorkLog");	//获取所有一级目录 -- 年月
					if (AllLevelOneDirs != null)	//若一级目录不为空
					{
						foreach (string LevelOneDir in AllLevelOneDirs)	//遍历还没上传的一级目录
						{
							DateTime LevelOneDir_Date = DateTime.ParseExact(Path.GetFileName(LevelOneDir), "yyyy-MM", CultureInfo.CurrentCulture);
							if (DateTime.Compare(LatestTimeLevelOne, LevelOneDir_Date) <= 0)	//若最新文件比较早, 则需要上传
							{
								/*------ 处理二级目录 ------*/
								string[] AllLevelTwoDirs = Directory.GetDirectories(LevelOneDir);	//获取所有二级目录 -- 年-月-日
								if (AllLevelTwoDirs != null)	//若二级目录不为空
								{
									foreach (string LevelTwoDir in AllLevelTwoDirs)	//遍历还没上传的二级目录
									{
										DateTime LevelTwoDir_Date = DateTime.ParseExact(LevelOneDir_Date.Year.ToString() + "-" + Path.GetFileName(LevelTwoDir), "yyyy-MM-dd", CultureInfo.CurrentCulture);
										if (DateTime.Compare(LatestTimeLevelTwo, LevelTwoDir_Date) <= 0)	//若最新文件比较早, 则需要上传
										{
											/*------ 处理二级目录下所有的Log文件 ------*/
											string[] LogFilesPath = Directory.GetFiles(LevelTwoDir);	//获取所有Log文件
											if (LogFilesPath != null)
											{
												foreach (string filePath in LogFilesPath)
												{
													string aa = Path.GetFileName(filePath).Replace("Log", "").Replace(".csv", "");
													//解析文件时间
													DateTime LogFileDate = DateTime.ParseExact(Path.GetFileName(filePath).Split('+')[2].Replace(".csv", ""), "yyyyMMdd-HHmmss", CultureInfo.CurrentCulture);
													if (DateTime.Compare(LatestTime, LogFileDate) < 0)		//当前Log文件为未上传的
													{
														if (filePath != worklog.filePath)		//不上传正在写入的文件
														{
															//上传该文件
															Console.WriteLine("上传文件:" + filePath);

															if (HttpHelper.UploadFile(LogUploadURL, filePath))		//若上传成功, 则更新最新文件名
															{
																SetLatestLogName(0, Path.GetFileNameWithoutExtension(filePath));
																//写入工作日志
																worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFile, Path.GetFileName(filePath)));
															}
															else													//否则重传一次
															{
																if (HttpHelper.UploadFile(LogUploadURL, filePath))		//若重传上传成功, 则更新最新文件名
																{
																	SetLatestLogName(0, Path.GetFileNameWithoutExtension(filePath));
																	//写入工作日志
																	worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFile, Path.GetFileName(filePath)));
																}
																else	//重传失败
																{
																	//写入工作日志
																	worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFileFail, Path.GetFileName(filePath) + ",上传失败"));
																	return false;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				return false;
			}

			return true;
		}

		//上传回放日志文件的函数
		private bool UploadPlayLogFiles()
		{
			try
			{
				//解析最新文件的时间
				DateTime LatestTime = DateTime.ParseExact(LatestPlayLogName.Split('+')[2].Replace(".csv", ""), "yyyyMMdd-HHmmss", CultureInfo.CurrentCulture);	//最新文件的时间
				DateTime LatestTimeLevelOne = new DateTime(LatestTime.Year, LatestTime.Month, 1);					//最新文件一级目录的时间  年-月
				DateTime LatestTimeLevelTwo = new DateTime(LatestTime.Year, LatestTime.Month, LatestTime.Day);		//最新文件二级目录的时间  年-月-日

				if (Directory.Exists(".\\res\\WorkLogPlay"))
				{
					/*------ 处理一级目录 ------*/
					string[] AllLevelOneDirs = Directory.GetDirectories(".\\res\\WorkLogPlay");	//获取所有一级目录 -- 年月
					if (AllLevelOneDirs != null)	//若一级目录不为空
					{
						foreach (string LevelOneDir in AllLevelOneDirs)	//遍历还没上传的一级目录
						{
							DateTime LevelOneDir_Date = DateTime.ParseExact(Path.GetFileName(LevelOneDir), "yyyy-MM", CultureInfo.CurrentCulture);
							if (DateTime.Compare(LatestTimeLevelOne, LevelOneDir_Date) <= 0)	//若最新文件比较早, 则需要上传
							{
								/*------ 处理二级目录 ------*/
								string[] AllLevelTwoDirs = Directory.GetDirectories(LevelOneDir);	//获取所有二级目录 -- 年-月-日
								if (AllLevelTwoDirs != null)	//若二级目录不为空
								{
									foreach (string LevelTwoDir in AllLevelTwoDirs)	//遍历还没上传的二级目录
									{
										DateTime LevelTwoDir_Date = DateTime.ParseExact(LevelOneDir_Date.Year.ToString() + "-" + Path.GetFileName(LevelTwoDir), "yyyy-MM-dd", CultureInfo.CurrentCulture);
										if (DateTime.Compare(LatestTimeLevelTwo, LevelTwoDir_Date) <= 0)	//若最新文件比较早, 则需要上传
										{
											/*------ 处理二级目录下所有的Log文件 ------*/
											string[] LogFilesPath = Directory.GetFiles(LevelTwoDir);	//获取所有Log文件
											if (LogFilesPath != null)
											{
												foreach (string filePath in LogFilesPath)
												{
													string aa = Path.GetFileName(filePath).Replace("Log", "").Replace(".csv", "");
													//解析文件时间
													DateTime LogFileDate = DateTime.ParseExact(Path.GetFileName(filePath).Split('+')[2].Replace(".csv", ""), "yyyyMMdd-HHmmss", CultureInfo.CurrentCulture);
													if (DateTime.Compare(LatestTime, LogFileDate) < 0)		//当前Log文件为未上传的
													{
														if (filePath != worklogplay.filePath)		//不上传正在写入的文件
														{
															//上传该文件
															Console.WriteLine("上传文件:" + filePath);

															if (HttpHelper.UploadFile(LogUploadURL, filePath))		//若上传成功, 则更新最新文件名
															{
																SetLatestLogName(1, Path.GetFileNameWithoutExtension(filePath));
																//写入工作日志
																worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFile, Path.GetFileName(filePath)));
															}
															else													//否则重传一次
															{
																if (HttpHelper.UploadFile(LogUploadURL, filePath))		//若重传上传成功, 则更新最新文件名
																{
																	SetLatestLogName(1, Path.GetFileNameWithoutExtension(filePath));
																	//写入工作日志
																	worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFile, Path.GetFileName(filePath)));
																}
																else
																{
																	//写入工作日志
																	worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetUploadFileFail, Path.GetFileName(filePath)+",上传失败"));
																	return false;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				return false;
			}

			return true;
		}

		//取消同步按钮响应事件
		void btnInfoSyncCancel_Click(object sender, EventArgs e)
		{
			if (isInfoSyncing)
			{
				try
				{
					infoSyncTh.Abort();
				}
				catch (Exception ex) { Console.WriteLine(ex.Message); }
				InfoSyncResDisplay("失 败");
				isInfoSyncing = false;
			}
		}

		//返回按钮响应事件
		void btnInfoSyncReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelSysSetting);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.InfoSyncPanel, (int)BtnOfInfoSyncPanel.InfoSyncReturn, null));
		}
	}
}
