using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Windows.Forms;
using System.Net;
using SCBAControlHost.MyUtils;
using MyUtils;
using SCBAControlHost.SerialCommunication;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private void SystemSettingInit()
		{
			#region 改变背景图片
			//圆形按键的按下与抬起----改变背景图片
			btnSysSetComChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetComChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetComOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetComOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetBaudChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetBaudChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetBaudOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetBaudOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetUnitChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetUnitChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetUnitOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetUnitOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetServerAddChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetServerAddChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetServerAddOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetServerAddOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetServerPortChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetServerPortChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetServerPortOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetServerPortOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetAccountChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetAccountChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetAccountOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetAccountOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetPwdChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetPwdChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetPwdOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetPwdOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetThresChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetThresChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetThresOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetThresOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetGrpNOChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetGrpNOChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetGrpNOOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetGrpNOOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetSysPwdChange.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetSysPwdChange.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetSysPwdOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnSysSetSysPwdOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnSysSetReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnSysSetReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			//6个按键的按下与抬起----改变背景图片
			btnSysSetInfoSync.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetInfoSync.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);
			btnSysSetImport.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetImport.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);
			btnSysSetExport.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetExport.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);
			btnSysSetTempGroup.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetTempGroup.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);
			btnSysSetChangeNO.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetChangeNO.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);
			btnSysSetCheckUser.MouseDown += new System.Windows.Forms.MouseEventHandler(btnRecPress);
			btnSysSetCheckUser.MouseUp += new System.Windows.Forms.MouseEventHandler(btnRecPop);

			//下拉按钮的按下与抬起----改变背景图片
			btnThresList.MouseDown += new System.Windows.Forms.MouseEventHandler(btnThresList_MouseDown);
			btnThresList.MouseUp += new System.Windows.Forms.MouseEventHandler(btnThresList_MouseUp);
			#endregion

			#region 按钮点击事件订阅
			//圆形按键点击事件
			btnSysSetComChange.Click += new EventHandler(btnSysSetComChange_Click);
			btnSysSetComOK.Click += new EventHandler(btnSysSetComOK_Click);
			btnSysSetBaudChange.Click += new EventHandler(btnSysSetBaudChange_Click);
			btnSysSetBaudOK.Click += new EventHandler(btnSysSetBaudOK_Click);
			btnSysSetUnitChange.Click += new EventHandler(btnSysSetUnitChange_Click);
			btnSysSetUnitOK.Click += new EventHandler(btnSysSetUnitOK_Click);
			btnSysSetServerAddChange.Click += new EventHandler(btnSysSetServerAddChange_Click);
			btnSysSetServerAddOK.Click += new EventHandler(btnSysSetServerAddOK_Click);
			btnSysSetServerPortChange.Click += new EventHandler(btnSysSetServerPortChange_Click);
			btnSysSetServerPortOK.Click += new EventHandler(btnSysSetServerPortOK_Click);
			btnSysSetAccountChange.Click += new EventHandler(btnSysSetAccountChange_Click);
			btnSysSetAccountOK.Click += new EventHandler(btnSysSetAccountOK_Click);
			btnSysSetPwdChange.Click += new EventHandler(btnSysSetPwdChange_Click);
			btnSysSetPwdOK.Click += new EventHandler(btnSysSetPwdOK_Click);
			btnSysSetThresChange.Click += new EventHandler(btnSysSetThresChange_Click);
			btnSysSetThresOK.Click += new EventHandler(btnSysSetThresOK_Click);
			btnSysSetGrpNOChange.Click += new EventHandler(btnSysSetGrpNOChange_Click);
			btnSysSetGrpNOOK.Click += new EventHandler(btnSysSetGrpNOOK_Click);
			btnSysSetSysPwdChange.Click += new EventHandler(btnSysSetSysPwdChange_Click);
			btnSysSetSysPwdOK.Click += new EventHandler(btnSysSetSysPwdOK_Click);
			btnSysSetReturn.Click+=new EventHandler(btnSysSetReturn_Click);
			//返回按键点击事件
			btnSysSetReturn.Click += new EventHandler(btnSysSetReturn_Click);
			//6个功能按键的点击事件
			btnSysSetInfoSync.Click += new EventHandler(btnSysSetInfoSync_Click);
			btnSysSetImport.Click += new EventHandler(btnSysSetImport_Click);
			btnSysSetExport.Click += new EventHandler(btnSysSetExport_Click);
			btnSysSetTempGroup.Click += new EventHandler(btnSysSetTempGroup_Click);
			btnSysSetChangeNO.Click += new EventHandler(btnSysSetChangeNO_Click);
			btnSysSetCheckUser.Click += new EventHandler(btnSysSetCheckUser_Click);
			//下拉按钮点击事件
			btnThresList.Click += new EventHandler(btnThresList_Click);
			#endregion

			comboBoxSysSetThres.Items.Add("50%");
			comboBoxSysSetThres.Items.Add("10MPa");
			comboBoxSysSetThres.Items.Add("6MPa");

			comboBoxSysSetBaud.Items.Add("1200");
			comboBoxSysSetBaud.Items.Add("2400");
			comboBoxSysSetBaud.Items.Add("4800");
			comboBoxSysSetBaud.Items.Add("9600");
			comboBoxSysSetBaud.Items.Add("19200");
			comboBoxSysSetBaud.Items.Add("38400");
			comboBoxSysSetBaud.Items.Add("57600");
			comboBoxSysSetBaud.Items.Add("115200");


		}

		#region 16个功能按钮+1个下拉按钮 的 点击事件

		//修改串口号
		void btnSysSetComOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetCom.Enabled == true)
			{
				if (RegexUtil.RegexCheckCOM(richTextSysSetCom.Text))		//若输入的串口号格式正确
				{
					//若串口是打开着的, 则先将其关闭
					if (serialCom.SerialPortIsOpen)
					{
						try
						{
							serialCom.ComDevice.Close();
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}

					richTextSysSetCom.Enabled = false;
					SysConfig.Setting.serialCom = richTextSysSetCom.Text;
					SysConfig.SaveSystemSetting(SysConfig.Setting);
					if (serialCom.ComOpen(SysConfig.Setting.serialCom, SerialBaudLUT[SysConfig.Setting.serialBaud]))		//若打开串口成功
					{
						SerialSendMsg sendMsg = ProtocolCommand.ServerQueryCmdMsg(SysConfig.getSerialNOBytes());	//发送主机查询命令
						serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
					}
					else
						MessageBox.Show("打开串口失败!");
				}
				else
					MessageBox.Show("请输入正确格式的串口号!");
			}
		}

		void btnSysSetComChange_Click(object sender, EventArgs e)
		{ richTextSysSetCom.Enabled = true; }

		//修改波特率
		void btnSysSetBaudOK_Click(object sender, EventArgs e)
		{
			//发送修改波特率命令
			//if (serialCom.SerialPortIsOpen)
			//{
			//    SerialSendMsg sendMsg = ProtocolCommand.SwitchChannelMsg(0x01, (byte)(comboBoxSysSetBaud.SelectedIndex + 1), 0);
			//    serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
			//    comboBoxSysSetBaud.Enabled = false;
			//    SysConfig.Setting.serialBaud = comboBoxSysSetBaud.SelectedIndex;
			//    SysConfig.SaveSystemSetting(SysConfig.Setting);
			//}
			//else
			//{
			//    comboBoxSysSetBaud.Enabled = false;
			//    comboBoxSysSetBaud.SelectedIndex = SysConfig.Setting.serialBaud;
			//    MessageBox.Show("修改失败, 串口未打开!");
			//}
			if (comboBoxSysSetBaud.Enabled == true)
			{
				//若串口是打开着的, 则先将其关闭
				if (serialCom.SerialPortIsOpen)
				{
					try
					{
						serialCom.ComDevice.Close();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}

				//再修改波特率
				comboBoxSysSetBaud.Enabled = false;
				SysConfig.Setting.serialBaud = comboBoxSysSetBaud.SelectedIndex;
				SysConfig.SaveSystemSetting(SysConfig.Setting);
				try
				{
					if (isSerialShouldOpen)
					{
						if (serialCom.ComOpen(SysConfig.Setting.serialCom, SerialBaudLUT[SysConfig.Setting.serialBaud]))		//若打开串口成功
						{
							SerialSendMsg sendMsg = ProtocolCommand.ServerQueryCmdMsg(SysConfig.getSerialNOBytes());	//发送主机查询命令
							serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
						}
						else
							MessageBox.Show("打开串口失败!");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
		void btnSysSetBaudChange_Click(object sender, EventArgs e)
		{ comboBoxSysSetBaud.Enabled = true; }

		//修改单位
		void btnSysSetUnitOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetUnit.Enabled == true)
			{
				richTextSysSetUnit.Enabled = false;
				SysConfig.Setting.unitName = richTextSysSetUnit.Text;
				SysConfig.SaveSystemSetting(SysConfig.Setting);
				labelUnit.Text = "单位：" + SysConfig.Setting.unitName;
				//写入单位修改记录
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeUnit, richTextSysSetUnit.Text));
			}
		}
		void btnSysSetUnitChange_Click(object sender, EventArgs e)
		{ richTextSysSetUnit.Enabled = true; }

		//修改服务器地址
		void btnSysSetServerAddOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetServerAdd.Enabled == true)		//若处于可以修改的状态
			{
				//判断IP是否正确, 这里直接用IPAddress.Parse来尝试将IP地址进行解析, 若报异常则代表是无效的IP地址
				bool isIpValid = false;
				try{IPAddress ipAddress = IPAddress.Parse(richTextSysSetServerAdd.Text); isIpValid = true;}
				catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); isIpValid = false; }
				//若IP地址有效
				if (isIpValid)
				{
					richTextSysSetServerAdd.Enabled = false;
					SysConfig.Setting.serverIP = richTextSysSetServerAdd.Text;
					SysConfig.SaveSystemSetting(SysConfig.Setting);				//将系统设置序列化到磁盘
					//写入服务器地址修改记录
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeServerIP, richTextSysSetServerAdd.Text));
				}
				else
					MessageBox.Show("请输入有效的IP地址");
			}
		}
		void btnSysSetServerAddChange_Click(object sender, EventArgs e)
		{ richTextSysSetServerAdd.Enabled = true; }

		//修改服务器端口号
		void btnSysSetServerPortOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetServerPort.Enabled == true)
			{
				//验证端口号格式是否正确(即是否是小于65535的纯数字)
				if (RegexUtil.RegexCheckNumber(richTextSysSetServerPort.Text))
				{
					int port = int.Parse(richTextSysSetServerPort.Text);
					if ((port > 0) && (port < 65535))
					{
						richTextSysSetServerPort.Enabled = false;
						SysConfig.Setting.serverPort = port;
						SysConfig.SaveSystemSetting(SysConfig.Setting);				//将系统设置序列化到磁盘
						//写入服务器端口号修改记录
						worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeServerPort, richTextSysSetServerPort.Text));
					}
					else MessageBox.Show("请输入小于65535的端口号");
				}
				else MessageBox.Show("请输入有效的端口号");
			}
		}
		void btnSysSetServerPortChange_Click(object sender, EventArgs e)
		{ richTextSysSetServerPort.Enabled = true; }

		//修改服务器账号
		void btnSysSetAccountOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetAccount.Enabled == true)
			{
				richTextSysSetAccount.Enabled = false;
				SysConfig.Setting.accessAccount = richTextSysSetAccount.Text;
				SysConfig.SaveSystemSetting(SysConfig.Setting);					//将系统设置序列化到磁盘
				//写入服务器账号修改记录
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeAccount, richTextSysSetAccount.Text));
			}
		}
		void btnSysSetAccountChange_Click(object sender, EventArgs e)
		{ richTextSysSetAccount.Enabled = true; }

		//修改服务器密码
		void btnSysSetPwdOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetPwd.Enabled == true)
			{
				richTextSysSetPwd.Enabled = false;
				SysConfig.Setting.accessPassword = richTextSysSetPwd.Text;
				SysConfig.SaveSystemSetting(SysConfig.Setting);					//将系统设置序列化到磁盘
				//写入服务器密码修改记录
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangePasswd, richTextSysSetPwd.Text));
			}
		}
		void btnSysSetPwdChange_Click(object sender, EventArgs e)
		{ richTextSysSetPwd.Enabled = true; btnThresList.Enabled = true; }

		//修改阈值
		void btnSysSetThresOK_Click(object sender, EventArgs e)
		{
			if (comboBoxSysSetThres.Enabled == true)
			{
				comboBoxSysSetThres.Enabled = false;
				btnThresList.Enabled = false;
				SysConfig.Setting.alarmThreshold = comboBoxSysSetThres.SelectedIndex;
				SysConfig.SaveSystemSetting(SysConfig.Setting);
				//写入阈值修改记录
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeThreshold, comboBoxSysSetThres.SelectedIndex.ToString()));
			}
		}

		void btnSysSetThresChange_Click(object sender, EventArgs e)
		{ comboBoxSysSetThres.Enabled = true; }

		//修改组号
		void btnSysSetGrpNOOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetGrpNO.Enabled == true)
			{
				//验证组号格式是否正确(即是否是小于16777215的纯数字)
				if (RegexUtil.RegexCheckNumber(richTextSysSetGrpNO.Text))
				{
					int GrpNO = int.Parse(richTextSysSetGrpNO.Text);
					if ((GrpNO > 0) && (GrpNO < 16777215))
					{
						richTextSysSetGrpNO.Text = GrpNO.ToString("D8");
						richTextSysSetGrpNO.Enabled = false;
						SysConfig.Setting.groupNumber = GrpNO;
						labelGrpNumber.Text = "组号：" + SysConfig.Setting.groupNumber.ToString("D8");
						SysConfig.SaveSystemSetting(SysConfig.Setting);
						//写入组号修改记录
						worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeGrpNO, richTextSysSetGrpNO.Text));
						isSerialShouldOpen = true;
					}
					else MessageBox.Show("请输入小于16777215的组号");
				}
				else MessageBox.Show("请输入正确的组号格式");
			}
		}
		void btnSysSetGrpNOChange_Click(object sender, EventArgs e)
		{ richTextSysSetGrpNO.Enabled = true; }

		//修改系统密码
		void btnSysSetSysPwdOK_Click(object sender, EventArgs e)
		{
			if (richTextSysSetSysPwd.Enabled == true)
			{
				richTextSysSetSysPwd.Enabled = false;
				SysConfig.Setting.systemPassword = richTextSysSetSysPwd.Text;
				SysConfig.SaveSystemSetting(SysConfig.Setting);
				//写入系统密码修改记录
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ChangeSysPwd, richTextSysSetSysPwd.Text));
			}
		}
		void btnSysSetSysPwdChange_Click(object sender, EventArgs e)
		{ richTextSysSetSysPwd.Enabled = true; }

		//下拉按钮
		void btnThresList_Click(object sender, EventArgs e)
		{
			comboBoxSysSetThres.DroppedDown = true;
		}

		#endregion



		#region 5个底部按钮 的 点击事件

		//信息同步 按钮
		void btnSysSetInfoSync_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelInfoSync);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.InfoSync, null));
		}

		//导入用户 按钮
		void btnSysSetImport_Click(object sender, EventArgs e)
		{
			List<UserBasicInfo> userList = null;
			int SuccessCnt = 0;					//导入成功的文件数目
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Excel,JPG,PNG|*.xlsx;*.xls;*.jpg;*.jpeg;*.png";
			openFileDialog.FilterIndex = 0;
			openFileDialog.RestoreDirectory = true;				//保存对话框是否记忆上次打开的目录
			openFileDialog.Multiselect = true;
			openFileDialog.InitialDirectory = Application.StartupPath + @"\res\UserTable";
			openFileDialog.Title = "导入用户配置文件";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if(openFileDialog.FileNames != null)
				{
					string[] filesPath = openFileDialog.FileNames;
					foreach (string filePath in filesPath)
					{
						if (Path.GetExtension(filePath).Contains("xlsx") || Path.GetExtension(filePath).Contains("xls"))	//若是excel文件
						{
							userList = userRW.ReadUserInfoFile(openFileDialog.FileName);	//读取导入文件中的用户
							if (userList != null)
							{
								//删除当前所有用户
								for (int i = users.Count - 1; i >= 0; i--)
									RemoveUserAt(i);
								if (userList.Count > 0)
								{
									foreach (UserBasicInfo userInfo in userList)
									{
										AddUser(userInfo);
									}
								}
								//更新默认用户文件
								userRW.SaveUserInfoFile(userRW.DefaultUserFileName, users);

								//写入按钮点击记录
								worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.ImportUsers, openFileDialog.FileName));
								//写入用户全部更新记录
								worklog.LogQueue_Enqueue(LogCommand.getUserUpdateRecord(1, users, null));
								SuccessCnt++;
							}
						}
						else	//若是图像文件
						{
							if (AppUtil.CopyFileTo(filePath, ".\\res\\UserTable"))		// 直接将其拷贝到用户目录下
								SuccessCnt++;
						}
					}
					if (SuccessCnt == 0)
						MessageBox.Show("导入失败");
					else if (SuccessCnt < filesPath.Length)
						MessageBox.Show("部分导入成功");
					else
						MessageBox.Show("导入成功");
				}
			}
		}

		//导出文件 按钮
		void btnSysSetExport_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.Description = "请选择导出的路径";
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if (string.IsNullOrEmpty(dialog.SelectedPath))
				{
					MessageBox.Show(this, "文件夹路径不能为空", "提示");
					return;
				}
				else
				{
					string dst = Path.GetFullPath(dialog.SelectedPath) + "\\导出文件" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
					Directory.CreateDirectory(dst);
					if (FolderHelper.Copy(@".\res\UserTable", dst) && FolderHelper.Copy(@".\res\WorkLog", dst))
						MessageBox.Show("导出成功");
					else
						MessageBox.Show("导出失败");
				}
			}

		}

		//临时编组 按钮
		void btnSysSetTempGroup_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelTempGroup);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.TempGrp, null));
		}

		//用户改号 按钮
		void btnSysSetChangeNO_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelUserChangeNO);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.UserChangeNO, null));
		}

		//用户列表 按钮
		void btnSysSetCheckUser_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelCheckUser);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.CheckUser, null));
		}

		#endregion

		//返回按键
		void btnSysSetReturn_Click(object sender, EventArgs e)
		{
			isSystemSetting = false;
			PanelSwitch(CurPanel.EpanelContentMain);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.SysSettingPanel, (int)BtnOfSysSettingPanel.SysSettingReturn, null));
		}

	}
}
