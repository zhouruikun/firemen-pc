using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SCBAControlHost.MyUtils;
using SCBAControlHost.SerialCommunication;
using MyUtils;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private int ChangeNOOldSerialNO = 0;
		private int ChangeNONewSerialNO = 0;

		private void UserChangeNOInit()
		{
			btnUserChangeNOOK.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnUserChangeNOOK.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnUserChangeNOReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnUserChangeNOReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnUserChangeNOOK.Click += new EventHandler(btnUserChangeNOOK_Click);
			btnUserChangeNOReturn.Click += new EventHandler(btnUserChangeNOReturn_Click);
		}

		//确认改号按钮点击事件
		void btnUserChangeNOOK_Click(object sender, EventArgs e)
		{
			//先判断用户输入的格式对不对
			//检查原组号是否是数字
			if (RegexUtil.RegexCheckNumber(richTextOldGrpNO.Text))
			{
				//检查原组号是否超出范围
				if (int.Parse(richTextOldGrpNO.Text) < 0xFFFFFF)
				{
					//检查新组号是否是数字
					if (RegexUtil.RegexCheckNumber(richTextNewGrpNO.Text))
					{
						//检查新组号是否超出范围
						if (int.Parse(richTextNewGrpNO.Text) < 0xFFFFFF)
						{
							//检查原用户号是否是数字
							if (RegexUtil.RegexCheckNumber(richTextOldDevNO.Text))
							{
								//检查原用户号是否超出范围
								if (int.Parse(richTextOldDevNO.Text) < 33)
								{
									//检查新用户号是否是数字
									if (RegexUtil.RegexCheckNumber(richTextNewDevNO.Text))
									{
										//检查新用户号是否超出范围
										if (int.Parse(richTextNewDevNO.Text) < 33)
										{
											//全部检查完毕没有问题
											richTextUserChangeNOStatus.Text = "";		//状态栏清空
											ChangeNOOldSerialNO = (int.Parse(richTextOldGrpNO.Text) << 8) | (int.Parse(richTextOldDevNO.Text));
											ChangeNONewSerialNO = (int.Parse(richTextNewGrpNO.Text) << 8) | (int.Parse(richTextNewDevNO.Text));

											//发送临时组队命令
											SerialSendMsg sendMsg = ProtocolCommand.ParaSetup1CmdMsg(AppUtil.IntToBytes(ChangeNOOldSerialNO), AppUtil.IntToBytes(ChangeNONewSerialNO));
											serialCom.SendQueue_Enqueue(sendMsg);	//发送出去
											//写入按钮点击记录
											worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.UserChangeNOPanel, (int)BtnOfUserChangeNOPanel.StartChangeNO, ChangeNOOldSerialNO.ToString("X8") + " " + ChangeNONewSerialNO.ToString("X8")));
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
		void btnUserChangeNOReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelSysSetting);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.UserChangeNOPanel, (int)BtnOfUserChangeNOPanel.ChangeNOReturn, null));
		}

		

	}
}
