using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SCBAControlHost.MyUtils;
using SCBAControlHost.SerialCommunication;
using MyUtils;
using System.Threading;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private int ChangeNOOldSerialNO = 0;
		private int ChangeNONewSerialNO = 0;
        private int ChangeNONewSerialNO_forChannal = 0;
        private int ChangeNONewChannal = 0;
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
                                            //检查信道号是否是数字
                                            if (RegexUtil.RegexCheckNumber(richTextNewChannal.Text))
                                            {
                                                ChangeNONewChannal = int.Parse(richTextNewChannal.Text);
                                                //检查信道号是否超出范围
                                                if (ChangeNONewChannal < 31 && (ChangeNONewChannal<23|| ChangeNONewChannal >25))
                                                {
                                                    //全部检查完毕没有问题
                                                    richTextUserChangeNOStatus.Text = "";       //状态栏清空
                                                    ChangeNOOldSerialNO = (int.Parse(richTextOldGrpNO.Text) << 8) | (int.Parse(richTextOldDevNO.Text));
                                                    ChangeNONewSerialNO = (int.Parse(richTextNewGrpNO.Text) << 8) | (int.Parse(richTextNewDevNO.Text));
                                                    ChangeNONewSerialNO_forChannal = ChangeNONewSerialNO;                                                 //发送临时组队命令
                                                   SerialSendMsg sendMsg = ProtocolCommand.ParaSetup1CmdMsg(AppUtil.IntToBytes(ChangeNOOldSerialNO), AppUtil.IntToBytes(ChangeNONewSerialNO));
                                                    //发送切换信道命令
                                                    serialCom.SendQueue_Enqueue(sendMsg);   //发送出去
                                                    ChangeNONewChannal = int.Parse(richTextNewChannal.Text);
                                                    Thread th = new Thread(new ThreadStart(ThreadSwitch)); //创建线程                     
                                                    th.Start(); //启动线程
                                                    //写入按钮点击记录
                                                    worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.UserChangeNOPanel, (int)BtnOfUserChangeNOPanel.StartChangeNO, ChangeNOOldSerialNO.ToString("X8") + " " + ChangeNONewSerialNO.ToString("X8")));
                                                }
                                                else
                                                {
                                                    MessageBox.Show("信道必须小于30,切不能为23 24 25");
                                                }
                                            }
                                            else
                                                MessageBox.Show("信道必须是数字");
                                           
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
        //切换信道
        void ThreadSwitch()
        {
            Thread.Sleep(3000);//如果不延时，将占用CPU过高  
            //设定延时任务 目的是避免在设置连续三次命令的时候插进去
            SerialSendMsg sendMsg = ProtocolCommand.TerminalSwitchCmdMsg(AppUtil.IntToBytes(ChangeNONewSerialNO_forChannal), AppUtil.IntToBytes(ChangeNONewSerialNO_forChannal), (byte)ChangeNONewChannal);//发送终端切换命令
            serialCom.SendQueue_Enqueue(sendMsg);   //发送出去
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
