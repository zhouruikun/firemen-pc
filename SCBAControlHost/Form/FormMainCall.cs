using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using MyUtils;
using System.IO;
using SCBAControlHost.NetCommunication;

namespace SCBAControlHost
{
	public enum CurPanel
	{
		EpanelContentMain,
		EpanelSysSetting,
		EpanelTempGroup,
		EpanelInfoSync,
		EpanelUserChangeNO,
		EpanelKnowledgeBase,
		EpanelDeviceBase,
		EpanelCheckUser
	};

	public struct UserStatusPara
	{
		public byte teriminalNO;
		public USERSTATUS status;
	}

	//消息提示框类
	public class MessageBoxInfo
	{
		public string text = "";
		public string caption = "";
		public MessageBoxButtons mbb = MessageBoxButtons.OK;
		public MessageBoxIcon mbi = MessageBoxIcon.Error;

		public MessageBoxInfo(string text, string caption, MessageBoxButtons mbb, MessageBoxIcon mbi)
		{
			this.text = text;
			this.caption = caption;
			this.mbb = mbb;
			this.mbi = mbi;
		}
	}

	public partial class FormMain
	{
		//创建资源目录 和 文件
		private void CreateResDirectory()
		{
			List<string> ResDirectory = new List<string>();
			ResDirectory.Add(".\\res\\UserTable");
			ResDirectory.Add(".\\res\\SytemConfig");
			//ResDirectory.Add(".\\res\\KnowledgeBase\\SrcFiles");
			//ResDirectory.Add(".\\res\\KnowledgeBase\\HtmlFiles");
			//ResDirectory.Add(".\\res\\DeviceBase\\SrcFiles");
			//ResDirectory.Add(".\\res\\DeviceBase\\HtmlFiles");
			ResDirectory.Add(".\\res\\KnowledgeBase\\SrcFiles\\本地库");
			ResDirectory.Add(".\\res\\KnowledgeBase\\SrcFiles\\同步库");
			ResDirectory.Add(".\\res\\KnowledgeBase\\HtmlFiles\\本地库");
			ResDirectory.Add(".\\res\\KnowledgeBase\\HtmlFiles\\同步库");
			ResDirectory.Add(".\\res\\DeviceBase\\SrcFiles\\本地库");
			ResDirectory.Add(".\\res\\DeviceBase\\SrcFiles\\同步库");
			ResDirectory.Add(".\\res\\DeviceBase\\HtmlFiles\\本地库");
			ResDirectory.Add(".\\res\\DeviceBase\\HtmlFiles\\同步库");

			ResDirectory.Add(".\\res\\WorkLog");
			ResDirectory.Add(".\\res\\WorkLogPlay");

			foreach(string path in ResDirectory)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			//若最新日志名称文件不存在, 则创建
			if (!File.Exists(@".\res\LatestUploadLogName.txt"))
			{
				FileStream fs = new FileStream(@".\res\LatestUploadLogName.txt", FileMode.CreateNew);
				StreamWriter sw = new StreamWriter(fs);
				sw.WriteLine("123456@qq.com+LLL+20000101-011000");			//这里是写入的工作日志名称
				sw.WriteLine("123456@qq.com+LLL+20000101-011000+Play");		//这里是写入的回放日志名称
				sw.Flush();
				sw.Close();
				fs.Close();
			}
		}

		//获取最新的日志文件名
		public void GetLatestLogName()
		{
			try
			{
				FileStream fs = new FileStream(@".\res\LatestUploadLogName.txt", FileMode.Open);
				StreamReader sr = new StreamReader(fs);
				LatestLogName = sr.ReadLine();
				LatestPlayLogName = sr.ReadLine();
				sr.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		//设置最新的日志文件名 no:0-工作日志 1-回放日志
		public void SetLatestLogName(int no, string name)
		{
			try
			{
				string LatestLogTmp = "", LatestPlayLogTmp = "";
				//FileStream fs = new FileStream(@".\res\LatestUploadLogName.txt", FileMode.Open, FileAccess.ReadWrite);

				StreamReader sr = new StreamReader(@".\res\LatestUploadLogName.txt");
				LatestLogTmp = sr.ReadLine();
				LatestPlayLogTmp = sr.ReadLine();
				sr.Close();

				StreamWriter sw = new StreamWriter(@".\res\LatestUploadLogName.txt");
				if (no == 0)
					LatestLogTmp = name;
				else
					LatestPlayLogTmp = name;
				sw.WriteLine(LatestLogTmp);			//这里是写入的工作日志名称
				sw.WriteLine(LatestPlayLogTmp);		//这里是写入的回放日志名称
				sw.Flush();
				
				sw.Close();
				//fs.Close();

				//更新最新文件名
				if (no == 0)
					LatestLogName = name;
				else
					LatestPlayLogName = name;

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		//系统设置面板初始化, 主要将所有输入框控件的enabled置为false
		public void PanelSysSettingInit()
		{
			richTextSysSetCom.Enabled = false;				//串口号 enabled置为false
			comboBoxSysSetBaud.Enabled = false;				//波特率 enabled置为false
			richTextSysSetUnit.Enabled = false;				//单位设置 enabled置为false
			richTextSysSetServerAdd.Enabled = false;		//服务器地址 enabled置为false
			richTextSysSetServerPort.Enabled = false;		//服务器端口号 enabled置为false
			richTextSysSetAccount.Enabled = false;			//服务器账号 enabled置为false
			richTextSysSetPwd.Enabled = false;				//服务器密码 enabled置为false
			comboBoxSysSetThres.Enabled = false;			//报警阈值下拉框 enabled置为false
			btnThresList.Enabled = false;					//报警阈值下拉按钮 enabled置为false
			richTextSysSetGrpNO.Enabled = false;            //组号 enabled置为false
            richTextSysSetChannal.Enabled = false;
            richTextSysSetSysPwd.Enabled = false;			//系统密码 enabled置为false
		}

		//显示面板切换
		public void PanelSwitch(CurPanel curPanel)
		{
			ChangeNONewSerialNO = 0;	//变量清零
			TempGrpNewSerialNO = 0;		//变量清零

			switch (curPanel)
			{
				case CurPanel.EpanelContentMain:		//主内容面板
					panelContentMain.Visible = true;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					panelInfo.BackColor = Color.FromArgb(63, 71, 82);
					break;

				case CurPanel.EpanelInfoSync:			//信息同步面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = true;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					richTextInfoSyncStatus.Text = "";

					panelInfo.BackColor = Color.FromArgb(34, 34, 34);
					break;

				case CurPanel.EpanelSysSetting:			//系统设置面板
					PanelSysSettingInit();		//系统设置面板初始化, 主要将所有输入框控件的enabled置为false
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = true;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					panelInfo.BackColor = Color.FromArgb(34, 34, 34);
					break;

				case CurPanel.EpanelTempGroup:			//临时编组面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = true;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					richTextTempGroupStatus.Text = "";
					richTextTempOldGrpNO.Text = "";
					richTextTempOldDevNO.Text = "";
					richTextTempNewGrpNO.Text = "";
					richTextTempNewDevNO.Text = "";

					panelInfo.BackColor = Color.FromArgb(34, 34, 34);
					break;

				case CurPanel.EpanelUserChangeNO:		//用户改号面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = true;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					richTextUserChangeNOStatus.Text = "";
					richTextOldGrpNO.Text = "";
					richTextOldDevNO.Text = "";
					richTextNewGrpNO.Text = "";
					richTextNewDevNO.Text = "";

					panelInfo.BackColor = Color.FromArgb(34, 34, 34);
					break;

				case CurPanel.EpanelKnowledgeBase:		//知识库面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = true;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = false;

					panelInfo.BackColor = Color.FromArgb(34, 34, 34);
					break;

				case CurPanel.EpanelDeviceBase:			//设备库面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = true;
					panelCheckUser.Visible = false;
					break;

				case CurPanel.EpanelCheckUser:			//用户列表面板
					panelContentMain.Visible = false;
					panelInfoSync.Visible = false;
					panelSysSetting.Visible = false;
					panelTempGroup.Visible = false;
					panelUserChangeNO.Visible = false;
					panelKnowledgeBase.Visible = false;
					panelDeviceBase.Visible = false;
					panelCheckUser.Visible = true;
					break;

				default:
					break;
			}
		}

		/*
		 * 根据字节数组来检验该终端是否属于本组, 其中arrayIn[offset]为4字节序列号的第0个字节
		 */
		public bool MatchGrpNO(byte[] arrayIn, int offset)
		{
			AppUtil m_appUtil = new AppUtil();
			if (AppUtil.bytesToInt(arrayIn, offset, 3) == SysConfig.Setting.groupNumber)		//若组号匹配
			{
				return true;
			}
			return false;
		}

		/*
		 * 根据字节数组来检验该终端是否存在, 其中arrayIn[offset]为4字节序列号的第0个字节
		 */
		public bool MatchSerialNO(byte[] arrayIn, int offset)
		{
			if (AppUtil.bytesToInt(arrayIn, offset, 3) == SysConfig.Setting.groupNumber)		//若组号匹配
			{
				int index = 255;
				for (int i = 0; i < users.Count; i++)
				{
					if (users[i].BasicInfo.terminalNO == (int)arrayIn[offset + 3])
					{
						index = i;
						break;
					}
				}
				if (index != 255)		//找到匹配的终端
					return true;
			}
			return false;
		}

		/*
		 * 根据终端号返回用户在用户列表中的下表位置
		 */
		public int GetIndexBySerialNO(byte termNO)
		{
			int index = 255;
			for (int i = 0; i < users.Count; i++)
			{
				if (users[i].BasicInfo.terminalNO == (int)termNO)
				{
					index = i;
					break;
				}
			}
			return index;
		}

		/*
		 * 改变用户状态, terminalNO为终端号
		 * 与主线程同步执行
		 */
		public void ChangeUserState(object statusPara1)
		{
			UserStatusPara statusPara = (UserStatusPara)statusPara1;
			int index = 255;
			if (!isPlayBackMode)	//若不是回放模式
			{
				for (int i = 0; i < users.Count; i++)
				{
					if (users[i].BasicInfo.terminalNO == (int)(statusPara.teriminalNO))
					{
						index = i;
						break;
					}
				}
				if (index != 255)		//找到匹配的终端
				{
					users[index].UStatus = statusPara.status;
					//改变标志
					users[index].isChanged = true;
					
					//若当前详情窗口显示的是本用户, 则更新详情窗口的显示
					if (users[index] == detailsForm.CurUser)
					{
						detailsForm.Update();
					}

					//判断用户是否需要报警
					if (users[index].UStatus == USERSTATUS.LoseContactStatus)		//失去联系需要报警
					{
						users[index].AlarmFlagForLost = true;
						users[index].isRecvPack = false;
					}
					else if (users[index].UStatus == USERSTATUS.RetreatFailStatus)	//撤出失败需要报警
					{
						users[index].AlarmFlagForRetreat = true;
						users[index].isRecvPack = false;
					}
					else if (users[index].UStatus == USERSTATUS.PowerOffStatus)		//撤出失败需要清除报警标志和超出阈值标志
					{
						users[index].IsExceedThreshold = false;
						users[index].AlarmFlagForExceedTh = false;
						users[index].AlarmFlagForLost = false;
						users[index].AlarmFlagForRetreat = false;
						users[index].isRecvPack = true;
					}
					else															//否则将失去联系与撤出失败报警标志清除
					{
						users[index].AlarmFlagForLost = false;
						users[index].AlarmFlagForRetreat = false;
						users[index].isRecvPack = true;
					}

					//写入用户状态记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getUserStatusRecord(users[index]));
					//写入用户状态记录到回放日志文件中
					//worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getUserStatusRecord(users[index]));
					//上传到服务器
					if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
					{
						//netcom.NetSendQueue_Enqueue(NetCommand.NetUploadUserPacket(users[index]));
					}
				}
			}
		}

		/*
		 * 根据接收的byte数组更新终端的实时信息, array1[]为16字节的参数(此函数在主线程中执行)
		 * 返回true表示更新成功, 否则更新失败(未找到匹配的终端)
		 * 与主线程同步执行
		 */
		void UpdateTerInfoByBytes(object array)
		{
			if (!isPlayBackMode)	//若不是回放模式
			{
				byte[] array1 = (byte[])array;
				if (AppUtil.bytesToInt(array1, 1, 3) == SysConfig.Setting.groupNumber)		//若组号匹配
				{
					int index = 255;
					for (int i = 0; i < users.Count; i++)
					{
						if (users[i].BasicInfo.terminalNO == (byte)array1[4])
						{
							index = i;
							break;
						}
					}

					if (index != 255)		//若找到了匹配的终端
					{
						//改变标志
						users[index].isChanged = true;
						users[index].isRecvPack = true;
						//压力, 单位为MPa
						users[index].TerminalInfo.Pressure = User.GetPressDoubleByBytes(array1, 5);
						//电压, 单位为V
						users[index].TerminalInfo.Voltage = User.GetVoltageDoubleByBytes(array1, 7);
						//温度
						users[index].TerminalInfo.Temperature = User.GetTemeratureIntByByte(array1[9]);
						//开机时间
						users[index].TerminalInfo.PowerONTime = User.GetTimeIntByBytes(array1, 10);
						//状态
						users[index].TerminalInfo.TerminalStatus = array1[15];

						//计算剩余时间
						double A = 0;	//由30s下降值计算出来的时间
						double B = 0;	//由气压值计算出来的时间
						bool isAvalid = false;	//计算的A值是否有效标志: 若30s内无下降, 则计算出来的置为A无效
						if (users[index].TerminalInfo.Pressure <= 2.0)		//若当前气压小于2, 则计算的结果肯定是负数, 则将A和B都置0, 剩余时间当做0
						{
							A = 0;
							B = 0;
						}
						else												//若当前气压大于2, 则需要计算A 和 B, 取小的那一个
						{
							//计算A  ----  由30s下降值计算出来的时间
							if (users[index].TerminalInfo.PressDropDownIn30s > 0)	//若30s内气压值是下降的, 则可以计算A
							{
								isAvalid = true;
								A = (users[index].TerminalInfo.Pressure - 2) / (users[index].TerminalInfo.PressDropDownIn30s * 2);	//由30s下降值计算出来的时间
							}
							else
							{
								A = 0;
								isAvalid = false;
							}

							//计算B  ----  由气压值计算出来的时间
							if (users[index].BasicInfo.terminalCapSpec == "6.8")		//6.8L的气瓶
								B = (users[index].TerminalInfo.Pressure - 2) * 6.8 * 10 / 50;
							else if (users[index].BasicInfo.terminalCapSpec == "9")		//9L的气瓶
								B = (users[index].TerminalInfo.Pressure - 2) * 9 * 10 / 50;
						}
						//更新剩余时间, 取A、B中小的一方
						if(A < B)
						{
							if(isAvalid)	//若A有效, 则取A, 否则取B
								users[index].TerminalInfo.RemainTime = (int)A;
							else
								users[index].TerminalInfo.RemainTime = (int)B;
						}
						else
							users[index].TerminalInfo.RemainTime = (int)B;

						//若用户状态不是撤出中, 则更改用户状态
						if (users[index].UStatus != USERSTATUS.RetreatingStatus)
						{
							if (users[index].TerminalInfo.Pressure > 10)				//安全状态
								users[index].UStatus = USERSTATUS.SafeStatus;
							else if (users[index].TerminalInfo.Pressure > 6)			//轻度危险
								users[index].UStatus = USERSTATUS.MildDangerousStatus;
							else
								users[index].UStatus = USERSTATUS.DangerousStatus;		//危险状态
						}
						else		//否则更新气压
							users[index].UStatus = USERSTATUS.RetreatingStatus;

						//若当前详情窗口显示的是本用户, 则更新详情窗口的显示
						if (users[index] == detailsForm.CurUser)
						{
							detailsForm.UpdateDetailForm();
						}

						//判断是否报警
						//若终端不是关机、不存在状态, 则需要设置其报警状态
						if (users[index].UStatus != USERSTATUS.NoExistStatus && users[index].UStatus != USERSTATUS.PowerOffStatus)
						{
							try
							{
                                users[index].AlarmFlagForLost = false;
                                bool IsUserExceedTh = false;	//用户是否超出阈值标识

								//1. 分析用户是否超阈值, 存储在标志IsUserExceedTh中
								if (SysConfig.Setting.alarmThreshold == 0)			//若设定阈值为气瓶容量的50%
								{
									double terminalCapSpec = double.Parse(users[index].BasicInfo.terminalCapSpec);	//获取气瓶容量
									if (users[index].TerminalInfo.Pressure < (terminalCapSpec / 2.0))	//若小于气瓶容量的50%--超出阈值
									{
										IsUserExceedTh = true;	//用户超阈值
									}
									else
									{
										IsUserExceedTh = false;	//没有超出阈值
									}
								}
								else if (SysConfig.Setting.alarmThreshold == 1)		//若设定阈值为10MPa
								{
									if (users[index].TerminalInfo.Pressure < 10.0)		//若小于10MPa--超出阈值
									{
										IsUserExceedTh = true;	//用户超阈值
									}
									else	//没有超出阈值
									{
										IsUserExceedTh = false;	//没有超出阈值
									}
								}
								else if (SysConfig.Setting.alarmThreshold == 2)		//若设定阈值为6MPa
								{
									if (users[index].TerminalInfo.Pressure < 6.0)		//若小于6MPa--超出阈值
									{
										IsUserExceedTh = true;	//用户超阈值
									}
									else	//没有超出阈值
									{
										IsUserExceedTh = false;	//没有超出阈值
									}
								}

								//2. 更新当前用户的"超阈值报警标志"
								if (IsUserExceedTh)		//若用户超阈值
								{
									if (users[index].IsExceedThreshold == false)	//若上一次没有超过阈值, 但本次超出阈值, 则开启"超阈值报警"
										users[index].AlarmFlagForExceedTh = true;
									else											//若上一次超过阈值, 且本次继续超出阈值, 则不去处理
									{ }
								}
								else					//若用户没有超阈值
								{
									if (users[index].IsExceedThreshold == true)		//若上一次超过阈值, 但本次没有超出阈值, 则停止"超阈值报警"
										users[index].AlarmFlagForExceedTh = false;
									else											//若上一次没有超过阈值, 且本次继续没有超出阈值, 则不去处理
									{ }
								}

								//3. 更新用户超阈值标志
								users[index].IsExceedThreshold = IsUserExceedTh;
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); }
						}

						//写入用户状态记录到日志文件中
						worklog.LogQueue_Enqueue(LogCommand.getUserStatusRecord(users[index]));
						//写入用户状态记录到回放日志文件中
						//worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getUserStatusRecord(users[index]));
						//若开启了实时上传 且 服务器已连接, 则上传到服务器
						if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	//若网络连接正常 且 验证通过 且 服务器在线
						{
							//netcom.NetSendQueue_Enqueue(NetCommand.NetUploadUserPacket(users[index]));
						}
					}
				}
			}
		}

		/*
		 * 由传入的路径获取节点需要用到的所有路径
		 * 1. 传入F:\\res\\SrcFiles\\aa目录  输出(1)F:\\res\\HtmlFiles\\aa目录 (2)F:\\res\\HtmlFiles\\aa目录 (3)F:\\res\\HtmlFiles\\aa目录
		 * 2. 传入F:\\res\\SrcFiles\\aa.docx文档  输出(1)F:\\res\\SrcFiles\\aa.docx文档 (1)F:\\res\\HtmlFiles\\aa-html目录 (1)F:\\res\\HtmlFiles\\aa-html\\aa.html文档
		 */
		private string[] GetTagPath(string path)
		{
			string[] TagPath = new string[3];
			TagPath[0] = path;

			string htmlDirPath = Path.GetDirectoryName(path).Replace("SrcFiles", "HtmlFiles");
			TagPath[1] = path.Replace("SrcFiles", "HtmlFiles");
			TagPath[2] = path.Replace("SrcFiles", "HtmlFiles");
			if (path.Substring(path.Length - 5, 5).Contains(".doc"))			//若传入的路径中包含.doc, 即路径为word文件
			{
				TagPath[1] = htmlDirPath + "\\" + Path.GetFileNameWithoutExtension(path) + "-html";
				TagPath[2] = TagPath[1] + "\\" + Path.GetFileNameWithoutExtension(path) + ".html";
			}
			return TagPath;
		}

		//递归的添加所有word文档到TreeView中
		private void AddAllValidFiles(string path, TreeNode tn)
		{
			try
			{
				///获取父节点目录的子目录
				string[] SubDir = Directory.GetDirectories(path);
				///子节点
				TreeNode subDirNode = new TreeNode();
				///通过遍历给传进来的父节点添加子节点
				foreach (string dirPath in SubDir)
				{
					subDirNode = new TreeNode(Path.GetFileNameWithoutExtension(dirPath));
					subDirNode.Tag = GetTagPath(Path.GetFullPath(dirPath));		//将目录的全路径存到Tag中
					subDirNode.ImageIndex = 0;
					subDirNode.SelectedImageIndex = 0;
					tn.Nodes.Add(subDirNode);
					///对文件夹不断递归， 得到所有文件
					AddAllValidFiles(dirPath, subDirNode);
				}

				string[] SubFile = Directory.GetFiles(path, "*.doc?");	//匹配文件后缀名为doc或docx的word文档
				TreeNode subFileNode = new TreeNode();
				foreach (string filePath in SubFile)
				{
					subFileNode = new TreeNode(Path.GetFileNameWithoutExtension(filePath));
					subFileNode.Tag = GetTagPath(Path.GetFullPath(filePath));		//将目录的全路径存到Tag中
					subFileNode.ImageIndex = 1;
					subFileNode.SelectedImageIndex = 1;
					tn.Nodes.Add(subFileNode);
				}
			}
			catch { }
		}

		//删除过期的工作日志文件
		void WorkLogMaintain()
		{
			int month = 3;						//工作日志保留的月数
			string path = @".\res\WorkLog";
			//获取所有子目录
			try
			{
				string[] SubDir = Directory.GetDirectories(path);
				foreach (string dirPath in SubDir)
				{
					DateTime pathDate = DateTime.ParseExact(Path.GetFileName(dirPath), "yyyy-MM", null);
					if ((DateTime.Now - pathDate).TotalDays > (month * 30))		//若超过期限
						Directory.Delete(dirPath, true);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		//弹出消息框
		void MessageBoxShow(object obj)
		{
			if (obj is MessageBoxInfo)
			{
				MessageBoxInfo mbInfo = obj as MessageBoxInfo;
				MessageBox.Show(mbInfo.text, mbInfo.caption, mbInfo.mbb, mbInfo.mbi);

			}
		}

		#region 添加和删除用户操作

		//添加空用户
		private void AddEmptyUser(int uid)
		{
			User user;
			user = new User(uid, panelUsers);
			user.UStatus = USERSTATUS.NoExistStatus;
			autosize.setControlsTag(user.PUserView.UserPanel);	//将控件加入 自动调节大小的控制类中
			autosize.resizeControl(this);						//在resize消息里调用此函数以自动设置窗口控件大小和位置
			usersEmpty.Add(user);
		}

		//调整用户
		private void AdjustUser()
		{
			User user;

			//重新调整"空用户"
			if (users.Count < 8)		//若有效用户数小于8, 则增加空用户使得总用户数为8
			{
				if (usersEmpty.Count < (8 - users.Count))	//若空用户数不足
				{
					int tmp = 8 - users.Count - usersEmpty.Count;	//计算需要加入的空用户数
					for (int i = 0; i < tmp; i++)
					{
						user = new User(users.Count, panelUsers);
						user.UStatus = USERSTATUS.NoExistStatus;
						autosize.setControlsTag(user.PUserView.UserPanel);	//将控件加入 自动调节大小的控制类中
						autosize.resizeControl(this);						//在resize消息里调用此函数以自动设置窗口控件大小和位置
						usersEmpty.Add(user);
					}
				}
				else if (usersEmpty.Count > (8 - users.Count))	//若空用户数超过
				{
					int tmp = usersEmpty.Count;
					for (int i = 0; i < (users.Count + tmp - 8); i++)
					{
						usersEmpty[usersEmpty.Count - 1].PUserView.UserPanel.Dispose();
						usersEmpty.RemoveAt(usersEmpty.Count - 1);	//则移除
					}
				}

				//重新调整位置
				for (int i = 0; i < usersEmpty.Count; i++)
				{
					usersEmpty[i].Uid = users.Count + i;
				}
				autosize.resizeControl(this);
			}
			else			//若有效用户数大于等于8, 空用户最多为1个
			{
				if (usersEmpty.Count > 1)	//若空用户数大于1, 则需要删减空用户数到1个
				{
					int tmp = usersEmpty.Count;
					for (int i = 0; i < (tmp - 1); i++)
					{
						usersEmpty[usersEmpty.Count - 1].PUserView.UserPanel.Dispose();
						usersEmpty.RemoveAt(usersEmpty.Count - 1);	//则移除
					}
				}
				else    //usersEmpty.Count=0 or 1
				{
					if (users.Count % 2 == 0)		//若有效用户数为偶数个
					{
						if (usersEmpty.Count > 0)	//usersEmpty.Count=1
						{
							usersEmpty[usersEmpty.Count - 1].PUserView.UserPanel.Dispose();
							usersEmpty.RemoveAt(0);	//则移除
						}
					}
					else							//若有效用户数为奇数个
					{
						if (usersEmpty.Count == 0)	//若空用户数为0的话, 则需要增加一个空用户
						{
							user = new User(users.Count, panelUsers);
							user.UStatus = USERSTATUS.NoExistStatus;
							autosize.setControlsTag(user.PUserView.UserPanel);	//将控件加入 自动调节大小的控制类中
							autosize.resizeControl(this);						//在resize消息里调用此函数以自动设置窗口控件大小和位置
							usersEmpty.Add(user);
						}
					}
				}

				if (usersEmpty.Count > 0)
				{
					usersEmpty[0].Uid = users.Count;	//重新调整位置
				}
				autosize.resizeControl(this);
			}
		}

		//新增一个用户
		//void AddUser(UserBasicInfo basicInfo)
		void AddUser(object basicInfo1)
		{
			User user;
			panelUsers.VerticalScroll.Value = 0;

			UserBasicInfo basicInfo = (UserBasicInfo)basicInfo1;

			//先加入一个用户
			user = new User(users.Count, panelUsers);
			autosize.setControlsTag(user.PUserView.UserPanel);	//将控件加入 自动调节大小的控制类中
			autosize.resizeControl(this);						//在resize消息里调用此函数以自动设置窗口控件大小和位置

			user.PUserView.BtnName.Click += new EventHandler(UserNameButtons_Click);			//设置姓名按钮的点击事件
			user.PUserView.BtnName.MouseEnter += new EventHandler(UserNameButtons_MouseEnter);
			user.PUserView.BtnName.MouseLeave += new EventHandler(UserNameButtons_MouseLeave);
			user.PUserView.BtnDetails.Click += new EventHandler(DetailsButtons_Click);			//设置姓名按钮的点击事件
			user.PUserView.BtnDetails.MouseEnter += new EventHandler(UserDetailsButtons_MouseEnter);
			user.PUserView.BtnDetails.MouseLeave += new EventHandler(UserDetailsButtons_MouseLeave);
			user.BasicInfo = basicInfo;
			users.Add(user);

			//再重新调整"空用户"
			AdjustUser();

			UserTableAddUser(user);		//向用户信息列表中添加一个用户
		}

		//移除一个用户, 需要重新调整所有用户的位置
		void RemoveUserAt(object index1)
		{
			int index = (int)index1;
			Console.WriteLine("" + index);
			if (users != null)
			{
				if (users.Count > 0 && index < users.Count)
				{

					panelUsers.VerticalScroll.Value = 0;
					UserTableDelUser(users[index]);				//先在用户信息列表中删除用户
					users[index].PUserView.UserPanel.Dispose();	//再删除用户
					users.RemoveAt(index);
					if (users.Count > 0)
					{
						for (int i = 0; i < users.Count; i++)
						{
							if (users[i].Uid != i)
							{
								users[i].Uid = i;
								autosize.setControlsTag(users[i].PUserView.UserPanel);		//将控件加入 自动调节大小的控制类中
							}
						}
					}
					AdjustUser();

					autosize.resizeControl(this);	//在resize消息里调用此函数以自动设置窗口控件大小和位置
				}
			}
		}

		//清空当前所有用户
		void ClearUsers()
		{
			if (users.Count > 0)
			{
				//删除当前所有用户
				for (int i = users.Count - 1; i >= 0; i--)
				{
					UserTableDelUser(users[i]);				//先在用户信息列表中删除用户
					users[i].PUserView.UserPanel.Dispose();	//再删除用户
					users.RemoveAt(i);
				}

				//删除当前所有空用户
				for (int i = usersEmpty.Count - 1; i >= 0; i--)
				{
					usersEmpty[i].PUserView.UserPanel.Dispose();
					usersEmpty.RemoveAt(i);
				}
			}
		}

		//重新导入用户
		public void ReImportUserFromDefaultFile(object obj)
		{
			//删除当前所有用户
			for (int i = users.Count - 1; i >= 0; i--)
			{
				RemoveUserAt(i);
			}
				

			//读取用户配置文件, 并添加用户
			if (userRW.ReadDefaultUserFile())
			{
				if (userRW.UserInfoList.Count > 0)
				{
					foreach (UserBasicInfo userInfo in userRW.UserInfoList)
					{
						if (userInfo.terminalGrpNO == SysConfig.Setting.groupNumber)	//若组号相等, 则将用户加入
							AddUser(userInfo);
					}
				}
			}

			//写入用户全部更新记录
			worklog.LogQueue_Enqueue(LogCommand.getUserUpdateRecord(1, users, null));
		}

		//根据组号和终端号查找users列表中的用户
		//User FindUserBySerialNO(int grpNO, int terminalNO)
		//{
		//    User m_user = null;
		//    foreach (User user in users)
		//    {
		//        if (((user.BasicInfo.terminalGrpNO == grpNO)) && ((user.BasicInfo.terminalNO == terminalNO)))
		//        {
		//            m_user = user;
		//            break;
		//        }
		//    }
		//    return m_user;
		//}

		#endregion


		#region 图片按钮的背景图片切换
		//图片按钮的背景图片切换
		private void btnCircularPress_y(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.circular_border_btn_press_y;
		}
		private void btnCircularPop_y(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.circular_border_btn_pop_y;
		}
		private void btnCircularPress_g(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.circular_border_btn_press_g;
		}
		private void btnCircularPop_g(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.circular_border_btn_pop_g;
		}
		void btnRecPress(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.rec_btn_press;
		}
		private void btnRecPop(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.rec_btn_pop;
		}
		void btnThresList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.DropDownBtn_pop;
		}
		void btnThresList_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Button btn = (Button)sender;
			btn.BackgroundImage = Properties.Resources.DropDownBtn_press;
		}
		#endregion

		#region 回放相关按钮的背景图片切换
		//播放按钮按下事件
		void btnPlayBackPlay_MouseDown(object sender, MouseEventArgs e)
		{
			if (!isPlaying)
			{
				btnPlayBackPlay.BackgroundImage = Properties.Resources.Pause_32x32_down;
			}
			else
			{
				btnPlayBackPlay.BackgroundImage = Properties.Resources.Play_32x32_down;
			}
		}
		//打开文件按钮的按下和抬起事件
		void btnPlayBackOpen_MouseUp(object sender, MouseEventArgs e)
		{
			btnPlayBackOpen.BackgroundImage = Properties.Resources.Open_32x32_up;
		}
		void btnPlayBackOpen_MouseDown(object sender, MouseEventArgs e)
		{
			btnPlayBackOpen.BackgroundImage = Properties.Resources.Open_32x32_down;
		}
		#endregion

	}
}
