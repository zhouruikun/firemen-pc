using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCBAControlHost.SerialCommunication;
using MyUtils;
using System.Threading;
using SCBAControlHost.NetCommunication;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;
using System.Media;

//主窗口的其他线程代码
namespace SCBAControlHost
{
	public partial class FormMain
	{
		#region 串口相关线程
		//接收消息的线程
		//int InitStage = 0;
		void RecvMsgThread()
		{
			SerialRecvMsg recvMsg = null;
			int RecvQueueItemCount = 0;
			int grpNo = 0;
			int serialNO = 0;
			while (true)
			{
				serialCom.RecvQueueWaitHandle.WaitOne();				//等待接受队列中有数据
				RecvQueueItemCount = serialCom.SerialRecvQueue.Count;	//查询接收队列中有几个数据1
				if (RecvQueueItemCount > 0)								//若接收队列中有数据
				{
					for (int n = 0; n < RecvQueueItemCount; n++)		//则依次将数据取出来, 进行处理
					{
						lock (serialCom) { recvMsg = serialCom.SerialRecvQueue.Dequeue(); }
						if (recvMsg != null)
						{
							// 初始化第0阶段, 确定区域主机 或 控制主机角色
							if (false)	//若主机角色还没定下来ServerRole ==
							{
								if (recvMsg.PacketData.Cmd == 0x32)		//若为主机查询消息
								{
									if (recvMsg.IsFromExtern)		//若为外部接收到的消息
									{
										if (SysConfig.Setting.groupNumber == (AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 3)))	//若组号匹配
										{
											ServerRole = 2;		//为普通主机
											Console.WriteLine("主机角色: 普通主机");
											if (recvMsg.PacketData.DataFiled[9] > 0 && recvMsg.PacketData.DataFiled[9] < 30)		//信道号有效
											{
												SerialSendMsg sendMsg = ProtocolCommand.SwitchChannelMsg(recvMsg.PacketData.DataFiled[9]);	//切换信道
												serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
												//写入串口接收记录到日志文件中
												worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
												//richTextBoxAddress.Invoke(new Action(() => { richTextBoxAddress.Text = "" + recvMsg.PacketData.DataFiled[9]; }));
												Thread.Sleep(100);
												labelChannel.Invoke(new Action(() => { labelChannel.Text = "信道: " + recvMsg.PacketData.DataFiled[9]; }));
											}
											else if (recvMsg.PacketData.DataFiled[9] == (byte)255)		//信道号为255, 代表已经有一个相同组号的主机
											{
												Thread.Sleep(100);
												labelGrpNumber.Invoke(new Action(() => { labelGrpNumber.BackColor = Color.Red; }));
												// 关闭串口
												isSerialShouldOpen = false;
												if (serialCom.ComDevice.IsOpen)		//若串口已打开
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
											}
										}
									}
									else							//若为内部消息
									{
										ServerRole = 1;			//确定主机角色为控制主机
										Console.WriteLine("主机角色: 区域主机");
										sysConfig.Setting.channelDic[20] = SysConfig.Setting.groupNumber;
										//写入串口超时记录到日志文件中
										worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
									}
								}
							}
							else					//主机角色已经定下来了
							{
								switch (recvMsg.PacketData.Cmd)
								{
									#region 远程播报命令1 0x01, 0x02
									//远程播报命令1
									case 0x01:
										break;
									case 0x02:
										break;
									#endregion

									#region 远程播报命令2 0x03, 0x04
									//远程播报命令2
									case 0x03:
										break;
									case 0x04:
										break;
									#endregion

									#region 远程播报命令3 0x05, 0x06
									//远程播报命令3
									case 0x05:
										break;
									case 0x06:
										break;
									#endregion

									#region 远程播报命令4 0x07, 0x08
									//远程播报命令4
									case 0x07:
										break;
									case 0x08:
										break;
									#endregion

									#region 远程播报命令5 0x09, 0x10
									//远程播报命令5--撤出
									case 0x09:
										break;
									case 0x10:
										if (MatchSerialNO(recvMsg.PacketData.DataFiled, 1))	//若序列号匹配
										{
											if (isAllUserEvacuating)	//若正在全部用户撤出中, 则将返回的消息投入全部撤出队列中
											{
												lock (AllUserEvacuateQueue) { AllUserEvacuateQueue.Enqueue(recvMsg); }
											}
											else						//否则是单个撤出操作, 则根据响应来改变用户状态
											{
												UserStatusPara st = new UserStatusPara();
												st.teriminalNO = recvMsg.PacketData.DataFiled[4];
												if (recvMsg.IsFromExtern)	//若是从外部发来的, 则将用户状态改为"撤出中"
												{
													st.status = USERSTATUS.RetreatingStatus;
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
												}
												else						//若是内部消息, 表示超时, 则将用户状态改为"撤出失败"
												{
													st.status = USERSTATUS.RetreatFailStatus;
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
												}
												lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			//改变用户状态
											}
										}
										break;
                                    #endregion

                                    #region 终端上传主机 0x13, 0x14
                                    //终端上传主机
                                    case 0x13:
                                        if (MatchGrpNO(recvMsg.PacketData.DataFiled, 1))    //若组号匹配
                                        {
                                            if (MatchSerialNO(recvMsg.PacketData.DataFiled, 1)) //若终端号在当前用户列表中存在, 则更新信息, 同时返回响应
                                            {
                                                lock (m_SyncContext) { m_SyncContext.Send(UpdateTerInfoByBytes, recvMsg.PacketData.DataFiled); }        //更新信息
                                                SerialSendMsg sendMsg = ProtocolCommand.TerminalUploadCmdAckMsg(recvMsg.PacketData.DataFiled);      //返回响应
                                                serialCom.SendQueue_Enqueue(sendMsg);   //发送出去
                                                                                        //写入串口接收记录到日志文件中
                                                worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
                                            }
                                            else                                                // 若终端号在当前用户列表中不存在, 则添加用户
                                            {
                                                UserBasicInfo basicInfo = new UserBasicInfo();
                                                basicInfo.terminalGrpNO = SysConfig.Setting.groupNumber;
                                                basicInfo.terminalNO = recvMsg.PacketData.DataFiled[4];
                                                basicInfo.terminalCapSpec = "6.8";  //默认的气瓶容量
                                                lock (m_SyncContext) { m_SyncContext.Send(AddUser, basicInfo); }            //新增用户
                                                int index = GetIndexBySerialNO(recvMsg.PacketData.DataFiled[4]);    //获取终端在用户列表中的下标位置
                                                users[index].PowerupCount++;
                                                //写入日志文件中
                                                worklog.LogQueue_Enqueue(LogCommand.getUserUpdateRecord(2, null, users[index]));    //单个用户加入记录

                                                lock (m_SyncContext) { m_SyncContext.Send(UpdateTerInfoByBytes, recvMsg.PacketData.DataFiled); }        //更新信息
                                                SerialSendMsg sendMsg = ProtocolCommand.TerminalUploadCmdAckMsg(recvMsg.PacketData.DataFiled);      //返回响应
                                                serialCom.SendQueue_Enqueue(sendMsg);   //发送出去
                                                                                        //写入串口接收记录到日志文件中
                                                worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
                                            }
                                        }

                                        break;
                                    case 0x14:
                                        break;
                                    #endregion

                                    #region 主机查询终端 0x15, 0x16	(刷新操作)
                                    //主机查询终端
                                    case 0x15:
										break;
									case 0x16:
										if (MatchSerialNO(recvMsg.PacketData.DataFiled, 1))	//若序列号匹配
										{
											if (isAllUserUpdating)		//若正在全部用户刷新中
											{
												lock (AllUserUpdateQueue) { AllUserUpdateQueue.Enqueue(recvMsg); }
											}
											else						//否则是单个刷新操作
											{
												if (recvMsg.IsFromExtern)	//若是从外部发来的, 则更新用户信息
												{
													lock (m_SyncContext) { m_SyncContext.Send(UpdateTerInfoByBytes, recvMsg.PacketData.DataFiled); }
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
												}
												else						//若是内部消息, 表示超时, 则将用户状态改为"失去联系"
												{
													UserStatusPara st = new UserStatusPara();
													st.teriminalNO = recvMsg.PacketData.DataFiled[4];
													st.status = USERSTATUS.LoseContactStatus;
													lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			//改变用户状态
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
												}
											}
										}
										break;
									#endregion

									#region 终端开机注册 0x21, 0x22
									//终端开机注册
									case 0x21:
										grpNo = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 3);	//获取组号
										if (MatchGrpNO(recvMsg.PacketData.DataFiled, 1))	//若组号匹配
										{
											if (!MatchSerialNO(recvMsg.PacketData.DataFiled, 1))	//若终端号在当前用户列表中不存在, 则新增用户
											{
												UserBasicInfo basicInfo = new UserBasicInfo();
												basicInfo.terminalGrpNO = SysConfig.Setting.groupNumber;
												basicInfo.terminalNO = recvMsg.PacketData.DataFiled[4];
												basicInfo.terminalCapSpec = "6.8";	//默认的气瓶容量
												lock (m_SyncContext) { m_SyncContext.Send(AddUser, basicInfo); }			//新增用户
												int index = GetIndexBySerialNO(recvMsg.PacketData.DataFiled[4]);	//获取终端在用户列表中的下标位置
												users[index].PowerupCount++;
												//写入日志文件中
												worklog.LogQueue_Enqueue(LogCommand.getUserUpdateRecord(2, null, users[index]));	//单个用户加入记录
											}
											else
											{
												int index = GetIndexBySerialNO(recvMsg.PacketData.DataFiled[4]);	//获取终端在用户列表中的下标位置
												if (false)//users[index].PowerupCount == 0) //若是第一次发送开机注册
                                                {
                                                    users[index].PowerupCount++;
                                                }
                                                else                                //修正逻辑 只要是开机注册就返回
                                                {
													//发送响应
													SerialSendMsg sendMsg = ProtocolCommand.TerminalPowerOnAckMsg(AppUtil.ExtractBytes(recvMsg.PacketData.DataFiled, 1, 4));	//先回复"开机注册"的响应
													serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
													users[index].PowerupCount = 0;			//计数器清零
													//最后还要更新信息
													lock (m_SyncContext) { m_SyncContext.Send(UpdateTerInfoByBytes, recvMsg.PacketData.DataFiled); }
												}
											}
											//写入串口接收记录到日志文件中
											worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
										}
										else												//若组号不匹配
										{
											if (ServerRole == 1)	//自己是控制主机
											{
												foreach (var channel in sysConfig.Setting.channelDic)	//查找字典中是否存在已分配信道的主机
												{
													if (channel.Value == grpNo)		//若存在已分配信道的主机
													{
														byte[] tmp = new byte[4];				//目标主机序列号
														Array.Copy(recvMsg.PacketData.DataFiled, 1, tmp, 0, 4);		//复制目标主机序列号
														SerialSendMsg sendMsg = ProtocolCommand.TerminalSwitchCmdMsg(tmp, SysConfig.getSerialNOBytes(), channel.Key);//发送终端切换命令
														serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
														break;
													}
												}
												//写入串口接收记录到日志文件中
												worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
											}
										}
										break;
									case 0x22:
										break;
									#endregion

									#region 终端关机声明 0x23, 0x24
									//终端关机声明
									case 0x23:
										if (MatchSerialNO(recvMsg.PacketData.DataFiled, 1))	//若序列号匹配, 则回复响应, 并将对应用户的状态置为"关机"
										{
											SerialSendMsg sendMsg = ProtocolCommand.TerminalShutdownAckMsg(AppUtil.ExtractBytes(recvMsg.PacketData.DataFiled, 1, 4));
											serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
											UserStatusPara st = new UserStatusPara();
											st.teriminalNO = recvMsg.PacketData.DataFiled[4];
											st.status = USERSTATUS.PowerOffStatus;
											lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			//改变用户状态
											//写入串口接收记录到日志文件中
											worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
										}
										break;
									case 0x24:
										break;
									#endregion

									#region 主机查询 0x31, 0x32
									//主机查询
									case 0x31:
										grpNo = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 3);
										if (ServerRole == 1)		//若当前是控制主机, 则分配信道, 否则不动作
										{
											byte tmpKey = 255;
											//查询当前已存在的信道下标号
											foreach (var channel in sysConfig.Setting.channelDic)
											{
												if (channel.Value == grpNo)
												{
													tmpKey = channel.Key;
													break;
												}
											}

											// 当不存在同号的主机, 则查询空闲的信道
											if (tmpKey == 255)
											{
												foreach (var channel in sysConfig.Setting.channelDic)
												{
													if (channel.Value == -1)
													{
														tmpKey = channel.Key;
														break;
													}
												}
												//为其他主机分配信道
												if (tmpKey > 0 && tmpKey < 30)	//当前有可用信道
												{
													sysConfig.Setting.channelDic[tmpKey] = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 3);	//获取其他主机的组号转为int型
													sysConfig.Setting.ChannelIndex++;
													if (sysConfig.Setting.ChannelIndex >= 15) sysConfig.Setting.ChannelIndex = 0;
													SysConfig.SaveSystemSetting(SysConfig.Setting);
													byte[] tarGrpNo = new byte[4];
													Array.Copy(recvMsg.PacketData.DataFiled, 1, tarGrpNo, 0, 4);	//复制目标主机序列号
													SerialSendMsg sendMsg = ProtocolCommand.ServerQueryAckMsg(tarGrpNo, SysConfig.getSerialNOBytes(), tmpKey);	//返回响应
													serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
												}
												else							// 当前无可用信道
												{
													sysConfig.Setting.channelDic[sysConfig.ChannelList[sysConfig.Setting.ChannelIndex]] = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 3);	//获取其他主机的组号转为int型
													sysConfig.Setting.ChannelIndex++;
													if (sysConfig.Setting.ChannelIndex >= 15) sysConfig.Setting.ChannelIndex = 0;
													SysConfig.SaveSystemSetting(SysConfig.Setting);
												}
											}
											else		// 组号已存在信道列表中
											{
												byte[] tarGrpNo = new byte[4];
												Array.Copy(recvMsg.PacketData.DataFiled, 1, tarGrpNo, 0, 4);	//复制目标主机序列号
												SerialSendMsg sendMsg = ProtocolCommand.ServerQueryAckMsg(tarGrpNo, SysConfig.getSerialNOBytes(), tmpKey);	//返回响应
												serialCom.SendQueue_Enqueue(sendMsg);	// 发送出去
											}

											// 写入串口接收记录到日志文件中
											worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
										}
										break;
									case 0x32:
										break;
									#endregion

									#region 主机切换 0x33, 0x34
									//主机切换(无)
									#endregion

									#region 终端切换 0x35, 0x36
									//终端切换(未知, 无响应格式)
									case 0x35:
										break;
									case 0x36:
										break;
									#endregion

									#region 临时组队 0x37, 0x38
									//临时组队
									case 0x37:
										break;
									case 0x38:
										if (TempGrpNewSerialNO != 0)		//正在等待临时组队响应
										{
											if (recvMsg.IsFromExtern)		//若是从外部发来的, 则再继续判断序列号是否匹配
											{
												serialNO = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 4);	//获取目标终端序列号
												if (serialNO == TempGrpOldSerialNO)
												{
													TempGrpOldSerialNO = 0;
													TempGrpNewSerialNO = 0;	//新用户号清零, 代表临时组队操作完成
													richTextTempGroupStatus.Invoke(new Action(() => { richTextTempGroupStatus.Text = "成 功"; }));
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
												}
											}
											else		//若是内部发来的, 则代表超时了
											{
												TempGrpOldSerialNO = 0;
												TempGrpNewSerialNO = 0;	//新用户号清零, 代表临时组队操作完成
												richTextTempGroupStatus.Invoke(new Action(() => { richTextTempGroupStatus.Text = "失 败"; }));
												//写入串口超时记录到日志文件中
												worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
											}
										}
										break;
									#endregion

									#region 设置1 0x41, 0x42
									//设置1
									case 0x41:
										break;
									case 0x42:
										if (ChangeNONewSerialNO != 0)		//正在等待用户改号响应
										{
											if (recvMsg.IsFromExtern)		//若是从外部发来的, 则再继续判断序列号是否匹配
											{
												serialNO = AppUtil.bytesToInt(recvMsg.PacketData.DataFiled, 1, 4);
												if (serialNO == ChangeNOOldSerialNO)
												{
													ChangeNOOldSerialNO = 0;
													ChangeNONewSerialNO = 0;	//新用户号清零, 代表用户改号操作完成
													richTextUserChangeNOStatus.Invoke(new Action(() => { richTextUserChangeNOStatus.Text = "成 功"; }));
													//写入串口接收记录到日志文件中
													worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
												}
											}
											else		//若是内部发来的, 则代表超时了
											{
												ChangeNOOldSerialNO = 0;
												ChangeNONewSerialNO = 0;	//新用户号清零, 代表用户改号操作完成
												richTextUserChangeNOStatus.Invoke(new Action(() => { richTextUserChangeNOStatus.Text = "失 败"; }));
												//写入串口接收记录到日志文件中
												worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
											}
										}
										break;
									#endregion

									#region 设置2 0x43, 0x44
									//设置2
									case 0x43:
										break;
									case 0x44:
										break;
									#endregion

									default:
										break;
								}
							}
						}
						
					}
				}
			}
		}

		// 周期记录线程
		void PeriodRecordThread()
		{
			int wrklogCnt = 0;
			int LostConnectCnt = 0;

			while (true)
			{
				// 周期30s写入worklog
				if (wrklogCnt < 30)
				{
					wrklogCnt++;
				}
				else
				{
					wrklogCnt = 0;
					worklog.LogQueue_Enqueue(LogCommand.getAllUserStatusRecord(users));
				}

				// 周期25s检测终端掉线
                //修改為95s
				if (LostConnectCnt < 95)
				{
					LostConnectCnt++;
				}
				else
				{
					LostConnectCnt = 0;
					for (int i = 0; i < users.Count; i++)
					{
						if (users[i].isRecvPack)		// 若收到数据包
						{
							users[i].isRecvPack = false;
						}
						else
						{
							// 若用户状态不是 不存在 关机 失去联系
							if (users[i].UStatus != USERSTATUS.NoExistStatus && users[i].UStatus != USERSTATUS.PowerOffStatus && users[i].UStatus != USERSTATUS.LoseContactStatus)
							{
								UserStatusPara st = new UserStatusPara();
								st.teriminalNO = (byte)users[i].BasicInfo.terminalNO;
								st.status = USERSTATUS.LoseContactStatus;
								lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			// 改变用户状态为失去联系
							}
						}
					}
				}

				Thread.Sleep(1000);
			}
		}
		
		#endregion

		#region 功能相关线程
		//报警播放线程
		public void AlarmPlaySound()
		{
			bool isShouldPlaySound = false;		// 是否应该播放声音
			SoundPlayer AlarmSound = null;		// 报警音频
			try { AlarmSound = new SoundPlayer(@"./res/Sound/Alarm.wav"); }
			catch(Exception ex) {Console.WriteLine(ex.Message);}
			bool isPlayingAlarmSound = false;										// 是否正在播放声音
			while (true)
			{
				Thread.Sleep(100);
				isShouldPlaySound = false;
				if (users != null)
				{
					// 只要有一个用户处于正在播放状态, 则就应该播放报警声音
					foreach (User user in users)
					{
						if (user.IsPlayingAlarm)
						{
							isShouldPlaySound = true;
							break;
						}
					}
				}

				if (isShouldPlaySound)		// 若应该播放声音
				{
					if (isPlayingAlarmSound == false)	// 若当前没有在播放, 则播放
					{
						if (AlarmSound != null)
						{
							try { AlarmSound.PlayLooping(); }
							catch (Exception ex) { Console.WriteLine(ex.Message); }
						}
						isPlayingAlarmSound = true;
					}
				}
				else						// 若不应该播放声音
				{
					if (isPlayingAlarmSound == true)	// 若当前正在播放, 则停止播放
					{
						if (AlarmSound != null)
						{
							try { AlarmSound.Stop(); }
							catch (Exception ex) { Console.WriteLine(ex.Message); }
						}
						isPlayingAlarmSound = false;
					}
				}

			}
		}

		// 全部刷新线程
		public void AllUserUpdate_Thread()
		{
			Dictionary<byte[], bool> SerialNoDic = new Dictionary<byte[], bool>();		// 终端序列号字典, key存储终端序列号, value代表是否已收到终端的响应
			List<byte[]> SerialNoKey = new List<byte[]>();								// 终端序列号列表
			Dictionary<byte[], SerialRecvMsg> TerminalMsg = new Dictionary<byte[], SerialRecvMsg>();// 终端消息字典, key存储终端序列号, value存储终端返回的消息

			// 1. 首先找到当前不处于"关机"或"撤出中"状态的终端, 并将它们存放到列表中
			foreach (User user in users)
			{
				if ((user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.RetreatingStatus))	//若用户状态不为"关机"和"撤出中"状态
				{
					SerialNoDic.Add(AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO), false);
				}
			}
			SerialNoKey.AddRange(SerialNoDic.Keys);

			// 2. 开始一次对它们发送查询命令
			// 重发两次
			for (int i = 0; i < 2; i++)
			{
				foreach (byte[] serialNO in SerialNoKey)
				{
					if (SerialNoDic[serialNO] == false)	// 若终端还没有接到响应
					{
						SerialSendMsg sendMsg = ProtocolCommand.ServerQueryTerminalCmdMsg(serialNO, 1, 1000);	// 主机查询终端命令, 发送1次, 最大等待时间为600ms
						serialCom.SendQueue_Enqueue(sendMsg);	// 发送出去
						// 每个命令等待最多450ms的接收应答时间
						DateTime SendTime = DateTime.Now;
						while (((int)((DateTime.Now - SendTime).TotalMilliseconds) < 850) && !SerialNoDic[serialNO])		// 若还没到450ms且还没接到响应, 则一直接收响应
						{
							if (AllUserUpdateQueue.Count > 0)	// 若队列中有消息, 则取出消息, 并判断终端序列号是否匹配
							{
								SerialRecvMsg recvMsg = new SerialRecvMsg();
								lock (AllUserUpdateQueue) { recvMsg = AllUserUpdateQueue.Dequeue(); }		// 取出消息
								if (AppUtil.IsBytesEqual(serialNO, 0, recvMsg.PacketData.DataFiled, 1, 4))	// 若序列号匹配, 则记录该终端完成
								{
									if (recvMsg.IsFromExtern)	// 若是正确的响应, 则将标志位置true, 同时将消息推入队列
									{
										SerialNoDic[serialNO] = true;
										TerminalMsg.Add(serialNO, recvMsg);	// 将接收到的消息加入队列中, 方便第3步的处理
										// 写入串口接收记录到日志文件中
										worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
										break;
									}
									else						// 若是超时响应, 则判断是否是第二次发送命令了
									{
										if (i > 0)	//若是第二次发送了, 则将超时消息推入队列
											TerminalMsg.Add(serialNO, recvMsg);
										// 写入串口接收记录到日志文件中
										worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
									}
								}
							}
						}
					}
				}
			}

			// 3. 统一更新信息操作
			foreach (byte[] serialNO in SerialNoKey)
			{
				if (MatchSerialNO(serialNO, 0))	// 若序列号匹配
				{
					try
					{
						if (TerminalMsg[serialNO].IsFromExtern)	// 若是从外部发来的, 则更新用户信息
						{
							lock (m_SyncContext) { m_SyncContext.Send(UpdateTerInfoByBytes, TerminalMsg[serialNO].PacketData.DataFiled); }
						}
						else						// 若是内部消息, 表示超时, 则将用户状态改为"失去联系"
						{
							UserStatusPara st = new UserStatusPara();
							st.teriminalNO = TerminalMsg[serialNO].PacketData.DataFiled[4];
							st.status = USERSTATUS.LoseContactStatus;
							lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			// 改变用户状态
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
					}
				}
			}
			isAllUserUpdating = false;
		}

		// 全部撤出线程
		public void AllUserEvacuate_Thread()
		{
			Dictionary<byte[], bool> SerialNoDic = new Dictionary<byte[], bool>();
			Dictionary<byte[], SerialRecvMsg> TerminalMsg = new Dictionary<byte[], SerialRecvMsg>();
			List<byte[]> SerialNoKey = new List<byte[]>();

			// 1. 首先找到当前不处于"关机"状态的终端, 并将它们存放到列表中
			foreach (User user in users)
			{
				//if ((user.UStatus != USERSTATUS.PowerOffStatus) && (user.UStatus != USERSTATUS.RetreatingStatus))	//若用户状态不为"关机"和"撤出中"状态
				if ((user.UStatus != USERSTATUS.PowerOffStatus))	// 若用户状态不为"关机"
				{
					SerialNoDic.Add(AppUtil.IntSerialToBytes(user.BasicInfo.terminalGrpNO, user.BasicInfo.terminalNO), false);
				}
			}
			SerialNoKey.AddRange(SerialNoDic.Keys);

			// 2. 开始一次对它们发送远程播报5命令(0x09)
			// 重发两次
			for (int i = 0; i < 2; i++)
			{
				foreach (byte[] serialNO in SerialNoKey)
				{
					if (SerialNoDic[serialNO] == false)
					{
						SerialSendMsg sendMsg = ProtocolCommand.RemotePlaySoundCmdMsg(0x09, serialNO, 1, 1000);	//终端撤出命令, 发送1次, 最大等待时间为600ms
						serialCom.SendQueue_Enqueue(sendMsg);	// 发送出去
						DateTime SendTime = DateTime.Now;
						while (((int)((DateTime.Now - SendTime).TotalMilliseconds) < 850) && !SerialNoDic[serialNO])		//若还没到450ms且还没接到响应, 则一直接收响应
						{
							if (AllUserEvacuateQueue.Count > 0)	// 若队列中有消息, 则取出消息, 并判断终端序列号是否匹配
							{
								SerialRecvMsg recvMsg = new SerialRecvMsg();
								lock (AllUserEvacuateQueue) { recvMsg = AllUserEvacuateQueue.Dequeue(); }	//取出消息
								if (AppUtil.IsBytesEqual(serialNO, 0, recvMsg.PacketData.DataFiled, 1, 4))	//若序列号匹配, 则记录该终端完成
								{
									if (recvMsg.IsFromExtern)	// 若是正确的响应, 则将标志位置true, 同时将消息推入队列
									{
										SerialNoDic[serialNO] = true;
										if (!TerminalMsg.ContainsKey(serialNO))
											TerminalMsg.Add(serialNO, recvMsg);	// 将接收到的消息加入队列中, 方便第3步的处理
										//写入串口接收记录到日志文件中
										worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialRecv, recvMsg));
										break;
									}
									else						// 若是超时响应, 则判断是否是第二次发送命令了
									{
										if (i > 0)	// 若是第二次发送了, 则将超时消息推入队列
										{
											if (!TerminalMsg.ContainsKey(serialNO))
												TerminalMsg.Add(serialNO, recvMsg);
										}
										// 写入串口接收记录到日志文件中
										worklog.LogQueue_Enqueue(LogCommand.getSerialRecord(SerialRecordType.SerialTimeOut, recvMsg));
									}
								}
							}
						}
					}
				}
			}

			//3. 统一更新用户状态操作
			foreach (byte[] serialNO in SerialNoKey)
			{
				if (MatchSerialNO(serialNO, 0))	// 若序列号匹配
				{
					try
					{
						UserStatusPara st = new UserStatusPara();
						st.teriminalNO = TerminalMsg[serialNO].PacketData.DataFiled[4];
						if (TerminalMsg[serialNO].IsFromExtern)	// 若是从外部发来的, 则将用户状态改为"撤出中"
						{
							st.status = USERSTATUS.RetreatingStatus;
						}
						else						// 若是内部消息, 表示超时, 则将用户状态改为"撤出失败"
						{
							st.status = USERSTATUS.RetreatFailStatus;
						}
						lock (m_SyncContext) { m_SyncContext.Send(ChangeUserState, st); }			//改变用户状态
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						log.Info(AppUtil.getExceptionInfo(ex));
					}
				}
			}
			isAllUserEvacuating = false;
		}
		#endregion

		#region 网络相关线程
		// 处理网络接收数据包的线程
		public void NetPacketHandler()
		{
			NetPacket recvPacket;
			int RecvQueueItemCount = 0;
			while (true)
			{
				netcom.NetRecvQueueWaitHandle.WaitOne();
				RecvQueueItemCount = netcom.netRecvQueue.Count;

				// 若有数据包
				if (RecvQueueItemCount > 0)
				{
					for (int n = 0; n < RecvQueueItemCount; n++)
					{
						lock (netcom.netRecvQueue) { recvPacket = netcom.netRecvQueue.Dequeue(); }
						// 写入网络接收记录到日志文件中
						worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.NetRecv, recvPacket));
						switch (recvPacket.PacketType)
						{
							//验证数据包
							case 0x01:
								//if (recvPacket.DataLength == 4)		// 若验证包数据域长度为4才进行验证
								//    netcom.NetSendQueue_Enqueue(NetCommand.NetAuthPacket(recvPacket.datafield, SysConfig.Setting.accessAccount, SysConfig.Setting.accessPassword));
								break;
							//验证结果指示数据包
							case 0x03:
								if (recvPacket.datafield[0] == (byte)0x03)		// 若校验成功
									isAuthPass = true;
								else											// 若校验失败
								{
									isAuthPass = false;
									if (isRealTimeUploading)		// 若正在实时上传
									{
										isRealTimeUploading = false;	// 校验失败就不用继续实时上传了
										pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadImage; }));	//更换实时上传图片
									}
									//if (isInfoSyncing)				// 若正在信息同步
									//{
									//    isInfoSyncing = false;			//校验失败就不用继续信息同步了
									//    pictureInfoSyncWait.Invoke(new Action(() => { pictureInfoSyncWait.Visible = false; }));			//隐藏正在同步图片
									//    richTextInfoSyncStatus.Invoke(new Action(() => { richTextInfoSyncStatus.Text = "失 败"; }));		//信息同步结果显示"失败"
									//}
									MessageBox.Show("用户名或密码错误, 不能连接到服务器!!!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
								}
								break;

							// 实时上传数据包
							case 0x05:
								break;

							// 心跳包
							case 0x30:
								break;
							default:
								break;
						}
						// 心跳包定时线程复位
						Console.WriteLine("接收到数据包" + DateTime.Now.ToString("hh:mm:ss-fff"));
					}
				}
			}
		}

		//网络连接检测线程(ping服务器)
		void NetLinkCheckHandler()
		{
			while (true)
			{
				if (isFormLoadDone)
				{
                    // 先ping一下主机, 看主机是否在线
                    string ip = SysConfig.Setting.serverIP.Split(':')[0];
                    bool pingResult = AppUtil.PingServerAlive(ip, 1000);

					if (pingResult)		// 若主机在线
					{
						if (!isInternetAvailiable)	// 若之前不在线, 则需要更新网络状态图标为"连接"
						{
							btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_connected_24px; }));
							// 写入网络连接记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Connect, null));
						}
						isInternetAvailiable = pingResult;

						Thread.Sleep(10000);	// 若主机在线, 则10s之后再ping
					}
					else
					{
						if (isInternetAvailiable)	// 若之前在线, 则需要更新网络状态图标为"未连接"
						{
							btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_disconnected_24px; }));
							//写入网络断开连接记录到日志文件中
							worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Disconnect, null));
						}
						isInternetAvailiable = pingResult;

						Thread.Sleep(3000);	//若主机不在线, 则3s之后再ping
					}
				}
			}
		}

		//开启实时上传线程
		void StartRealUpload()
		{
			isStartingRealUpload = true;	//进入 开启实时上传 线程

            pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.Waiting; }));	// 更换实时上传图片为等待
			btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "正在连接"; }));
			// 1. 开始3次连接服务器
			for (int i = 0; i < 3; i++)
			{
				if (!netcom.isConnected)
				{
					netcom.NetConnect(SysConfig.Setting.serverIP, SysConfig.Setting.serverPort);
				}
				else
					break;		//连接上了服务器就退出循环
			}

			if (!netcom.isConnected)		//若未能连上服务器
			{
				pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadImage; }));	// 更换实时上传图片
				btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "实时上传"; }));
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.RTUpload, "2"));	// 记录实时上传失败
				MessageBox.Show("连接服务器失败");
				isStartingRealUpload = false;
				return;
			}

			//若连上了服务器, 就开始验证
			isAuthPass = false;
			pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.Waiting; }));	// 更换实时上传图片
			btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "正在验证"; }));
			netcom.NetSendQueue_Enqueue(NetCommand.NetAuthPacket(null, SysConfig.Setting.accessAccount, SysConfig.Setting.accessPassword, SysConfig.Setting.serverIP));	//发送验证数据包
			int cnt = 0;
			while ((cnt < 50) && (isAuthPass == false))
			{
				Thread.Sleep(100);
				cnt++;
			}

			//若验证失败
			if (isAuthPass == false)
			{
				isRealTimeUploading = false;
				pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadImage; }));	// 更换实时上传图片
				btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "实时上传"; }));
				worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.RTUpload, "2"));	// 记录实时上传失败
				MessageBox.Show("账号或密码错误, 连接服务器失败");
				isStartingRealUpload = false;
				return;
			}

			// 验证成功
			pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadingImage; }));		// 更换实时上传图片为正在上传
			btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "取消上传"; }));	//实时上传按钮文本改为"取消上传"
			isRealTimeUploading = true;
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.MainPanel, (int)BtnOfMainPanel.RTUpload, "1"));	// 记录实时上传成功
			isStartingRealUpload = false;
			//上传到服务器
			if (isRealTimeUploading && netcom.isConnected && isAuthPass && isInternetAvailiable)	// 若网络连接正常 且 验证通过 且 服务器在线
			{
				string strTmp = "";
				richTextBoxAddress.Invoke(new Action(() => { strTmp = richTextBoxAddress.Text; }));
				netcom.NetSendQueue_Enqueue(NetCommand.NetAddChangePacket(strTmp));						// 上传任务
				richTextBoxTask.Invoke(new Action(() => { strTmp = richTextBoxTask.Text; }));
				netcom.NetSendQueue_Enqueue(NetCommand.NetTaskChangePacket(strTmp));					// 上传地址
				richTextBoxAddress.Invoke(new Action(() => { strTmp = richTextBoxAddress.Text; }));
				netcom.NetSendQueue_Enqueue(NetCommand.NetUploadUsersPacket(users));					// 上传当前所有用户
			}
		}
        //开启实时上传线程
        void StartRealUploadViaHttp()
        {
            if (isRealTimeUploading && isInternetAvailiable)    // 若网络连接正常 且 服务器在线
            {
                netcom.SetHttpSend(true);
                pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadingImage; }));       // 更换实时上传图片为正在上传
                btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "取消上传"; }));	//实时上传按钮文本改为"取消上传"
                string strTmp = "";
                richTextBoxAddress.Invoke(new Action(() => { strTmp = richTextBoxAddress.Text; }));
                netcom.NetSendQueue_Enqueue(NetCommand.NetAddChangePacket(strTmp));                     // 上传任务
                richTextBoxTask.Invoke(new Action(() => { strTmp = richTextBoxTask.Text; }));
                netcom.NetSendQueue_Enqueue(NetCommand.NetTaskChangePacket(strTmp));                    // 上传地址
                richTextBoxAddress.Invoke(new Action(() => { strTmp = richTextBoxAddress.Text; }));
                netcom.NetSendQueue_Enqueue(NetCommand.NetUploadUsersPacket(users));                    // 上传当前所有用户
            }
        }
        //网络意外断开时调用的处理函数
        void netDelegate_myEvent(object obj)
		{
			if (isRealTimeUploading)	//若当前正在实时上传
			{
				isRealTimeUploading = false;
				pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadImage; }));
				btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "实时上传"; }));
			}
			isAuthPass = false;		//验证状态改不通过
			//写入网络断开记录到日志文件中
			worklog.LogQueue_Enqueue(LogCommand.getNetRecord(NetRecordType.Disconnect, null));

			MessageBox.Show("与服务器断开连接");
		}

		//服务器周期线程, 包括上传数据 以及 写入回放日志
		void PeriodRecordServerThread()
		{
			List<User> ChangedUsers;
			
			while (true)
			{
				//先查看用户状态是否改变
				ChangedUsers = new List<User>();
				for (int i = 0; i < users.Count; i++)
				{
					//if (users[i].isChanged)	// 判断状态改变了的用户
					if(true)					// 不管用户状态有没有改变, 都记录下来
					{
						users[i].isChanged = false;
						ChangedUsers.Add(users[i]);
					}
				}

				//上传到服务器
				if (isRealTimeUploading)	//若需要实时上传
				{
					netcom.NetSendQueue_Enqueue(NetCommand.NetUploadUsersPacket(ChangedUsers));		//上传实时数据包
				}
				//else
				//{
				//    pictureBoxUpload.Invoke(new Action(() => { pictureBoxUpload.Image = Properties.Resources.UploadImage; }));	//更换实时上传图片
				//    btnUpLoad.Invoke(new Action(() => { btnUpLoad.Text = "实时上传"; }));
				//}

				//写入用户状态记录到回放日志文件中
				worklogplay.LogPlayQueue_Enqueue(LogPlayCommand.getUserStatusRecord(ChangedUsers));

				Thread.Sleep(1000);
			}

		}
		#endregion

		#region 回放相关线程
		//回放线程
		void PlayBackThread()
		{
			DateTime recordTime;
			while (true)
			{
				try
				{
					if (isPlayBackMode)			//若处于回放模式
					{
						if (isPlaying && PlayBackRecord != null)			//若正在播放
						{
							if (PlayBackRecord.Count > 0)
							{
								recordTime = DateTime.ParseExact(PlayBackRecord[0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);	//获取记录的时间
								while ((int)(PlayBackBaseTime - recordTime).TotalSeconds >= 0)		//当前记录应该被处理
								{
									//根据记录更新界面
									while (isAdjustingPBPos) ;	//若当前正在调整位置, 则线程不再往下走
									lock (PlayBackRecord) { ParseLogRecord(PlayBackRecord); }		//解析记录
									if (RecordCounter < LogList.Count - 1)		//若还没有播放完
									{
										RecordCounter++;						//记录下标加1
										lock (PlayBackRecord) { PlayBackRecord = LogList[RecordCounter]; }											//读取记录
										recordTime = DateTime.ParseExact(PlayBackRecord[0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);		//解析记录的时间

									}
									else									//若已经播放完了
									{
										isPlaying = false;
										PlayBackBaseTime = DateTime.ParseExact(LogList[LogList.Count - 1][0], "yyyyMMdd-HHmmss-fff", CultureInfo.CurrentCulture);	//设置基准时间为最后
										//将播放按钮置为"播放"
										btnPlayBackPlay.Invoke(new Action(() =>
										{
											btnPlayBackPlay.BackgroundImage = Properties.Resources.Play_32x32_up;
											tt_PlayBackPlay.SetToolTip(btnPlayBackPlay, "播放");
										}));
										trackBarPlayBack.Invoke(new Action(() => { trackBarPlayBack.Enabled = false; }));
										break;
									}
								}
							}

							//播放基准时间加10ms
							PlayBackBaseTime = PlayBackBaseTime.AddMilliseconds(10);
							labelCurrentTime.Invoke(new Action(() => { labelCurrentTime.Text = "时间：" + PlayBackBaseTime.ToString("yyyy.MM.dd  HH:mm"); }));

							//更新"已播放时间"
							TimeSpan ts = PlayBackBaseTime - PlayBackStartTime;
							trackBarPlayBack.Invoke(new Action(() =>
							{
								if ((int)ts.TotalSeconds <= trackBarPlayBack.Maximum && !isTrackBarDown)
									trackBarPlayBack.Value = (int)ts.TotalSeconds;
							}));
						}
					}
					else
					{
						PlayBackRecord = null;
						RecordCounter = 0;
						TotalPlayTime = "00:00:00";
						TimeHasBeenRunning = "00:00:00";
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
				}
				
				Thread.Sleep(10);
			}
		}

		//解析Log记录
		void ParseLogRecord(List<string> record)
		{
			switch (int.Parse(record[1]))
			{
				//初始状态记录
				case 1:
					InitRecordParse(record);
					break;

				//用户更新记录
				case 2:
					UserUpdateRecordParse(record);
					break;

				//用户状态记录
				case 3:
					m_SyncContext.Send(UserStatusRecordParse, record);
					break;

				//按钮点击记录
				case 4:
					m_SyncContext.Send(ButtonClickRecordParse, record);
					break;

				//串口记录
				case 5:

					break;

				//网络记录
				case 6:
					NetRecordParse(record);
					break;

				//修改地点记录
				case 7:
					richTextBoxAddress.Invoke(new Action(() => { richTextBoxAddress.Text = record[2]; }));
					break;

				//修改任务记录
				case 8:
					richTextBoxTask.Invoke(new Action(() => { richTextBoxTask.Text = record[2]; }));
					break;

				default:
					break;
			}
		}

		//初始化状态记录解析
		void InitRecordParse(List<string> record)
		{
			int UserCount = 0;
			try { UserCount = int.Parse(record[2]); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			//删除所有用户
			for (int i = users.Count - 1; i >= 0; i--)
				m_SyncContext.Send(RemoveUserAt, i);
			//读取用户
			for (int i = 0; i < UserCount; i++)
			{
				Console.WriteLine("t2");
				UserBasicInfo userInfo = new UserBasicInfo();
				userInfo.userNO = record[3 + i * 9];
				userInfo.name = record[4 + i * 9];
				userInfo.birthDate = record[5 + i * 9];
				userInfo.uAffiliatedUnit = record[6 + i * 9];
				userInfo.userPhoto = record[7 + i * 9];
				userInfo.duty = record[8 + i * 9];
				userInfo.terminalGrpNO = int.Parse(record[9 + i * 9]);
				userInfo.terminalNO = int.Parse(record[10 + i * 9]);
				userInfo.terminalCapSpec = record[11 + i * 9];

				m_SyncContext.Send(AddUser, userInfo);
			}
			//读取系统配置
			SysConfig.Setting.unitName = record[3 + UserCount * 9];
			SysConfig.Setting.serverIP = record[4 + UserCount * 9];
			SysConfig.Setting.serverPort = int.Parse(record[5 + UserCount * 9]);
			SysConfig.Setting.accessAccount = record[6 + UserCount * 9];
			SysConfig.Setting.accessPassword = record[7 + UserCount * 9];
			SysConfig.Setting.alarmThreshold = int.Parse(record[8 + UserCount * 9]);
			SysConfig.Setting.groupNumber = int.Parse(record[9 + UserCount * 9]);
			SysConfig.Setting.systemPassword = record[10 + UserCount * 9];
			//读取地点
			richTextBoxAddress.Invoke(new Action(() => { richTextBoxAddress.Text = record[11 + UserCount * 9]; }));
			//读取任务
			richTextBoxTask.Invoke(new Action(() => { richTextBoxTask.Text = record[12 + UserCount * 9]; }));
			//读取串口状态
			int serialstatus;
			try { serialstatus = int.Parse(record[13 + UserCount * 9]); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			//读取网络状态
			int netstatus = 0;
			try { netstatus = int.Parse(record[14 + UserCount * 9]); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			if(netstatus == 1)
				btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_connected_24px; }));
			else
				btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_disconnected_24px; }));
		}

		//用户更新记录解析
		void UserUpdateRecordParse(List<string> record)
		{
			int UpdateType = 0;
			int UserCount = 0;
			try { UpdateType = int.Parse(record[2]); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			if(UpdateType == 1)
			{
				//删除所有用户
				for (int i = users.Count - 1; i >= 0; i--)
				{
					Console.WriteLine("t1");
					m_SyncContext.Send(RemoveUserAt, i);
				}
				try { UserCount = int.Parse(record[3]); }
				catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
				//读取用户
				for (int i = 0; i < UserCount; i++)
				{
					UserBasicInfo userInfo = new UserBasicInfo();
					userInfo.userNO = record[4 + i * 9];
					userInfo.name = record[5 + i * 9];
					userInfo.birthDate = record[6 + i * 9];
					userInfo.uAffiliatedUnit = record[7 + i * 9];
					userInfo.userPhoto = record[8 + i * 9];
					userInfo.duty = record[9 + i * 9];
					userInfo.terminalGrpNO = int.Parse(record[10 + i * 9]);
					userInfo.terminalNO = int.Parse(record[11 + i * 9]);
					userInfo.terminalCapSpec = record[12 + i * 9];

					m_SyncContext.Send(AddUser, userInfo);
				}
			}
			else if(UpdateType == 2)
			{
				UserBasicInfo userInfo = new UserBasicInfo();
				userInfo.userNO = record[3];
				userInfo.name = record[4];
				userInfo.birthDate = record[5];
				userInfo.uAffiliatedUnit = record[6];
				userInfo.userPhoto = record[7];
				userInfo.duty = record[8];
				userInfo.terminalGrpNO = int.Parse(record[9]);
				userInfo.terminalNO = int.Parse(record[10]);
				userInfo.terminalCapSpec = record[11];
				m_SyncContext.Send(AddUser, userInfo);
			}
			else
			{

			}
		}

		//用户状态记录解析
		void UserStatusRecordParse(object record1)
		{
			List<string> record = (List<string>)record1;

			int UserCount = 0;
			//try { UserCount = int.Parse(record[2]); }		//获取记录中的用户数目
			//catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

			UserCount = record.Count - 2;		//获取记录中的用户数目

			for (int i = 0; i < UserCount; i++)
			{
				int serialNO = int.Parse(record[2 + i].Substring(2, 8), System.Globalization.NumberStyles.AllowHexSpecifier);	//解析终端序列号
				int GrpNO = serialNO >> 8;				//获取组号
				int terminalNO = serialNO & 0x000000FF;	//获取终端号

				foreach (User user in users)
				{
					if (user.BasicInfo.terminalGrpNO == GrpNO && user.BasicInfo.terminalNO == terminalNO)	//若终端序列号匹配
					{
						byte[] array1 = null;
						try { array1 = AppUtil.strToHexByte(record[2 + i].Substring(10, 26)); }
						catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

						//气压
						user.TerminalInfo.Pressure = User.GetPressDoubleByBytes(array1, 0);
						//电压
						user.TerminalInfo.Voltage = User.GetVoltageDoubleByBytes(array1, 2);
						//温度
						user.TerminalInfo.Temperature = User.GetTemeratureIntByByte(array1[4]);

						//开机时间
						user.TerminalInfo.PowerONTime = User.GetTimeIntByBytes(array1, 5);

						//状态
						user.UStatus = (USERSTATUS)array1[10];

						//剩余时间
						user.TerminalInfo.RemainTime = User.GetTimeIntByBytes(array1, 11);
						
						break;		//更新完毕, 不用再继续寻找匹配的终端了
					}
				}
			}
		}

		//按钮点击记录解析
		void ButtonClickRecordParse(object record1)
		{
			List<string> record = (List<string>)record1;
			BTNPANEL BtnPanelNO = 0;
			int BtnNO = 0;
			try { BtnPanelNO = (BTNPANEL)(int.Parse(record[2])); BtnNO = int.Parse(record[3]); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			
			switch (BtnPanelNO)
			{
				//主界面面板按钮
				case BTNPANEL.MainPanel:
					BtnOfMainPanel BtnNOMain = (BtnOfMainPanel)BtnNO;
					switch (BtnNOMain)
					{
						//用户撤出按钮
						case BtnOfMainPanel.UserEvacuate:
							//AppUtil.SetControlPosCentre(btnUserEvacuate, pictureBoxPlayBackArrow);
							AppUtil.ClickControl(btnUserEvacuate);
							break;
						//全部撤出按钮
						case BtnOfMainPanel.AllUserEvacuate:
							//AppUtil.SetControlPosCentre(btnAllUserEvacuate, pictureBoxPlayBackArrow);
							AppUtil.ClickControl(btnAllUserEvacuate);
							break;
						//停止报警按钮
						case BtnOfMainPanel.StopAlarm:
							//AppUtil.SetControlPosCentre(btnStopAlarm, pictureBoxPlayBackArrow);
							AppUtil.ClickControl(btnStopAlarm);
							break;
						//用户更新按钮
						case BtnOfMainPanel.UserUpdate:
							//AppUtil.SetControlPosCentre(btnUserUpdate, pictureBoxPlayBackArrow);
							AppUtil.ClickControl(btnUserUpdate);
							break;
						//全部更新按钮
						case BtnOfMainPanel.AllUserUpdate:
							//AppUtil.SetControlPosCentre(btnAllUserUpdate, pictureBoxPlayBackArrow);
							AppUtil.ClickControl(btnAllUserUpdate);
							break;
						//登录成功按钮
						case BtnOfMainPanel.LoginSuccess:
							break;
						//知识库按钮
						case BtnOfMainPanel.KnowledgeBase:
							break;
						//设备库按钮
						case BtnOfMainPanel.DeviceBase:
							break;
						//实时上传按钮
						case BtnOfMainPanel.RTUpload:
							break;
						//网络连接按钮
						case BtnOfMainPanel.NetLink:
							break;
						//用户选中按钮
						case BtnOfMainPanel.UserSelect:
							int serialNO = int.Parse(record[4], System.Globalization.NumberStyles.AllowHexSpecifier);
							int GrpNO = serialNO >> 8;
							int terminalNO = serialNO & 0x000000FF;
							foreach (User user in users)
							{
								if (user.BasicInfo.terminalGrpNO == GrpNO && user.BasicInfo.terminalNO == terminalNO)
								{
									user.IsSelected = true;
								}
								else
									user.IsSelected = false;
							}
							break;
						default:
							break;
					}
					break;

				//系统设置面板按钮
				case BTNPANEL.SysSettingPanel:
					BtnOfSysSettingPanel BtnNOSysSetting = (BtnOfSysSettingPanel)BtnNO;
					break;

				//信息同步面板按钮
				case BTNPANEL.InfoSyncPanel:
					BtnOfInfoSyncPanel BtnNOInfoSync = (BtnOfInfoSyncPanel)BtnNO;

					break;

				//临时编组面板按钮
				case BTNPANEL.TempGrpPanel:
					BtnOfTempGrpPanel BtnNOTempGrp = (BtnOfTempGrpPanel)BtnNO;

					break;

				//用户改号面板按钮
				case BTNPANEL.UserChangeNOPanel:
					BtnOfUserChangeNOPanel BtnNOUserChangeNO = (BtnOfUserChangeNOPanel)BtnNO;

					break;

				//知识库按钮
				case BTNPANEL.KnowledgeBasePanel:
					BtnOfKnowledgeBase BtnNOKnowledgeBase = (BtnOfKnowledgeBase)BtnNO;

					break;

				//设备库按钮
				case BTNPANEL.DeviceBasePanel:
					BtnOfDeviceBasePanel BtnNODeviceBase = (BtnOfDeviceBasePanel)BtnNO;

					break;

				default:
					break;
			}
		}

		//网络记录解析解析
		void NetRecordParse(List<string> record)
		{
			NetRecordType NetActionType = 0;
			try { NetActionType = (NetRecordType)(int.Parse(record[2])); }
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			switch (NetActionType)
			{
				//网络连接
				case NetRecordType.Connect:
					btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_connected_24px; }));
					break;
				//网络断开
				case NetRecordType.Disconnect:
					btnInternetState.Invoke(new Action(() => { btnInternetState.BackgroundImage = Properties.Resources.wifi_disconnected_24px; }));
					break;
				//网络发送数据
				case NetRecordType.NetSend:

					break;
				//网络接收数据
				case NetRecordType.NetRecv:

					break;
				//网络下载一个文件
				case NetRecordType.NetDownloadFile:

					break;
				//网络上传一个文件
				case NetRecordType.NetUploadFile:

					break;
				//下载一个文件失败
				case NetRecordType.NetDownloadFileFail:

					break;
				//上传一个文件失败
				case NetRecordType.NetUploadFileFail:

					break;
				//连接TCP服务器
				case NetRecordType.TcpConnect:

					break;
				//断开连接TCP服务器
				case NetRecordType.TcpDisconnect:

					break;
				//登录Web服务器
				case NetRecordType.Login:

					break;
				//退出登录Web服务器
				case NetRecordType.Logout:

					break;
				default:
					break;
			}
		}
		#endregion

	}
}
