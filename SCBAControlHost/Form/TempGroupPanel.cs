using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SCBAControlHost.MyUtils;
using System.Windows.Forms;
using MyUtils;
using SCBAControlHost.SerialCommunication;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private int TempGrpOldSerialNO = 0;
		private int TempGrpNewSerialNO = 0;

		private void TempGroupInit()
		{
			btnTempGroupOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnTempGroupOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnTempGroupReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnTempGroupReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnTempGroupOK.Click += new EventHandler(btnTempGroupOK_Click);
			btnTempGroupReturn.Click += new EventHandler(btnTempGroupReturn_Click);
		}

		//确认临时编组按钮点击事件
		void btnTempGroupOK_Click(object sender, EventArgs e)
		{
			//先判断用户输入的格式对不对
			//检查原组号是否是数字
			if( RegexUtil.RegexCheckNumber(richTextTempOldGrpNO.Text))
			{
				//检查原组号是否超出范围
				if(int.Parse(richTextTempOldGrpNO.Text) < 0xFFFFFF)
				{
					//检查新组号是否是数字
					if( RegexUtil.RegexCheckNumber(richTextTempNewGrpNO.Text))
					{
						//检查新组号是否超出范围
						if(int.Parse(richTextTempNewGrpNO.Text) < 0xFFFFFF)
						{
							//检查原用户号是否是数字
							if( RegexUtil.RegexCheckNumber(richTextTempOldDevNO.Text))
							{
								//检查原用户号是否超出范围
								if(int.Parse(richTextTempOldDevNO.Text) < 33)
								{
									//检查新用户号是否是数字
									if( RegexUtil.RegexCheckNumber(richTextTempNewDevNO.Text))
									{
										//检查新用户号是否超出范围
										if(int.Parse(richTextTempNewDevNO.Text) < 33)
										{
											//全部检查完毕没有问题
											richTextTempGroupStatus.Text = "";		//状态栏清空
											TempGrpOldSerialNO = (int.Parse(richTextTempOldGrpNO.Text)<<8) | (int.Parse(richTextTempOldDevNO.Text));
											TempGrpNewSerialNO = (int.Parse(richTextTempNewGrpNO.Text)<<8) | (int.Parse(richTextTempNewDevNO.Text));

											//发送临时组队命令
											SerialSendMsg sendMsg = ProtocolCommand.BuildTeamCmdMsg(AppUtil.IntToBytes(TempGrpOldSerialNO),
																	AppUtil.IntToBytes(TempGrpNewSerialNO),
																	SysConfig.getSerialNOBytes());
											serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
											//写入按钮点击记录
											worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.TempGrpPanel, (int)BtnOfTempGrpPanel.StartTempGrp, TempGrpOldSerialNO.ToString("X8")+" "+TempGrpNewSerialNO.ToString("X8")));
										}
										else
											MessageBox.Show("新用户号必须小于33");
									}
									else
										MessageBox.Show("新用户号必须为数字形式");
								}
								else
									MessageBox.Show("原用户号必须小于33");
							}
							else
								MessageBox.Show("原用户号必须为数字形式");
						}
						else
							MessageBox.Show("新组号必须小于16777215");
					}
					else
						MessageBox.Show("新组号必须为数字形式");
				}
				else
					MessageBox.Show("原组号必须小于16777215");
			}
			else
				MessageBox.Show("原组号必须为数字形式");
		}

		//返回按钮点击事件
		void btnTempGroupReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelSysSetting);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.TempGrpPanel, (int)BtnOfTempGrpPanel.TempGrpReturn, null));
		}
	}
}
