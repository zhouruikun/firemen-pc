using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MyUtils;
using System.Windows.Forms;
using System.Drawing;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private void CheckUserInit()
		{
			//绑定事件
			btnCheckUserReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnCheckUserReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnCheckUserReturn.Click += new EventHandler(btnCheckUserReturn_Click);
			dataGridViewCheckUser.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(dataGridViewCheckUser_RowsAdded);
			dataGridViewCheckUser.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(dataGridViewCheckUser_RowsRemoved);

			//设置数据源
			CheckUserDT.Columns.Add("UserName", typeof(string));
			CheckUserDT.Columns.Add("UserSex", typeof(string));
			CheckUserDT.Columns.Add("UserAge", typeof(string));
			CheckUserDT.Columns.Add("UserSerialNO", typeof(string));

			//绑定数据源
			dataGridViewCheckUser.DataSource = CheckUserDT;

			//设置标题字体
			Font ft = new Font(dataGridViewCheckUser.ColumnHeadersDefaultCellStyle.Font.FontFamily, dataGridViewCheckUser.ColumnHeadersDefaultCellStyle.Font.Size + 1);
			for (int i = 0; i < dataGridViewCheckUser.Columns.Count; i++)
				dataGridViewCheckUser.Columns[i].HeaderCell.Style.Font = ft;
		}

		//添加行时, 自动更新行号
		void dataGridViewCheckUser_RowsAdded(object sender, System.Windows.Forms.DataGridViewRowsAddedEventArgs e)
		{
			if (dataGridViewCheckUser != null)
			{
				if (dataGridViewCheckUser.Rows != null)
				{
					for (int i = 0; i < dataGridViewCheckUser.Rows.Count; i++)
						dataGridViewCheckUser.Rows[i].HeaderCell.Value = (i + 1).ToString();
				}
			}
			
		}

		//删除行时, 自动更新行号
		void dataGridViewCheckUser_RowsRemoved(object sender, System.Windows.Forms.DataGridViewRowsRemovedEventArgs e)
		{
			if (dataGridViewCheckUser != null)
			{
				if (dataGridViewCheckUser.Rows != null)
				{
					for (int i = 0; i < dataGridViewCheckUser.Rows.Count; i++)
						dataGridViewCheckUser.Rows[i].HeaderCell.Value = (i + 1).ToString();
				}
			}
		}

		//返回按钮响应事件
		void btnCheckUserReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelSysSetting);
			//写入按钮点击记录
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.CheckUserPanel, (int)BtnOfCheckUserPanel.CheckUserReturn, null));
		}

		//添加一个用户
		public void UserTableAddUser(User user)
		{
			if (user != null)
			{
				if (user.BasicInfo != null)
				{
					try
					{
						string serialno = "";
						serialno = user.BasicInfo.terminalGrpNO.ToString("D8") + "-" + user.BasicInfo.terminalNO.ToString("D2");

						//添加到数据表
						CheckUserDT.Rows.Add(new object[] {
													user.BasicInfo.name,
													user.BasicInfo.Sex,
													user.BasicInfo.Age,
													serialno });
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
				}
			}
		}

		//删除一个用户
		public void UserTableDelUser(User user)
		{
			if (user != null)
			{
			    if (user.BasicInfo != null)
			    {
					try
					{
						string serialno = "";
						serialno = user.BasicInfo.terminalGrpNO.ToString("D8") + "-" + user.BasicInfo.terminalNO.ToString("D2");

						if(CheckUserDT.Rows != null)
						{
							if (CheckUserDT.Rows.Count > 0)
							{
								DataRow[] drArr = CheckUserDT.Select("UserSerialNO='" + serialno + "'");	//从数据表中查找要删除的用户
								if (drArr != null)														//若找到
								{
									if (drArr.Length > 0)
										CheckUserDT.Rows.Remove(drArr[0]);									//则将其删除
								}
							}
						}
						
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
				}
			}
		}

	}
}

