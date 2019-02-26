using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MyUtils;

/*用户视图类*/
namespace SCBAControlHost
{
	class UserView
	{
		#region 成员变量
		/*********************成员变量*********************/
		private Panel userPanel;
		public Panel UserPanel
		{
			get { return userPanel; }
			set { userPanel = value; }
		}

		//姓名相关的控件
		private Button btnName;
		public Button BtnName
		{
			get { return btnName; }
			set { btnName = value; }
		}
		private PictureBox pictureBoxUser;
		public PictureBox PictureBoxUser
		{
			get { return pictureBoxUser; }
			set { pictureBoxUser = value; }
		}

		//气压相关的控件
		private Panel panelPressure;
		public Panel PanelPressure
		{
			get { return panelPressure; }
			set { panelPressure = value; }
		}
		private Label labelPressure;
		public Label LabelPressure
		{
			get { return labelPressure; }
			set { labelPressure = value; }
		}
		private Label labelPressureUnit;
		public Label LabelPressureUnit
		{
			get { return labelPressureUnit; }
			set { labelPressureUnit = value; }
		}
		private Label labelAlarm;
		public Label LabelAlarm
		{
			get { return labelAlarm; }
			set { labelAlarm = value; }
		}

		//倒计时相关的控件
		private Panel panelCountDown;
		public Panel PanelCountDown
		{
			get { return panelCountDown; }
			set { panelCountDown = value; }
		}
		private Label labelCountDown;
		public Label LabelCountDown
		{
			get { return labelCountDown; }
			set { labelCountDown = value; }
		}
		private Label labelCountDownUnit;
		public Label LabelCountDownUnit
		{
			get { return labelCountDownUnit; }
			set { labelCountDownUnit = value; }
		}

		private PictureBox pictureBoxStatus;
		public PictureBox PictureBoxStatus
		{
			get { return pictureBoxStatus; }
			set { pictureBoxStatus = value; }
		}
		private Label labelStatus;
		public Label LabelStatus
		{
			get { return labelStatus; }
			set { labelStatus = value; }
		}

		//详情相关的控件
		private Button btnDetails;
		public Button BtnDetails
		{
			get { return btnDetails; }
			set { btnDetails = value; }
		}
		private PictureBox pictureBoxDetails;
		public PictureBox PictureBoxDetails
		{
			get { return pictureBoxDetails; }
			set { pictureBoxDetails = value; }
		}
		/*************************************************/
		#endregion

		#region 参数区
		/*********************参数区*********************/
		private const int columnHeight = 98;			//高度
		private const int btnNameWidth = 158, panelPressureWidth = 158, panelCountDownWidth = 158, btnDetailsWidth = 150;	//各宽度
		private Color panelBackColor = Color.FromArgb(53, 121, 170), backColor = Color.FromArgb(63, 71, 82);		//背景颜色
		private Color foreColor = Color.FromArgb(255, 255, 255), btnDetailsForeColor = Color.FromArgb(249, 127, 32);	//字体颜色
		private const String font = "微软雅黑";								//字体
		private const float btnNameFontSize = (float)10.5, panelPressureFontSize = (float)24, panelPressureUnitFontSize = (float)9,
							labelCountDownFontSize = (float)25, labelCountDownUnitFontSize = (float)25, btnDetailsFontSize = (float)15.75;	//字体大小
		/*************************************************/
		#endregion

		#region 构造函数
		/*********************构造函数*********************/
		public UserView(int uid, Control con)
        {
			//创建空间
			userPanel = new Panel();
            btnName = new Button();
			pictureBoxUser = new PictureBox();
			panelPressure = new Panel();
			labelPressure = new Label();
			labelPressureUnit = new Label();
			labelAlarm = new Label();
			panelCountDown = new Panel();
			labelCountDown = new Label();
			labelCountDownUnit = new Label();
			pictureBoxStatus = new PictureBox();
			labelStatus = new Label();
            btnDetails = new Button();
			pictureBoxDetails = new PictureBox();

			UserViewInit(uid);
			userPanel.Parent = con;
			userPanel.BringToFront();		//设置在最顶层
        }
		/*************************************************/
		#endregion

		#region 私有函数
		/*********************私有函数*********************/
		//设置个各控件的名称, 大小, 位置, 字体
		private void setProperties(int id)
		{
			int x = id % 2, y = id / 2;

			userPanel.Location = new Point(x * 650, y * (columnHeight + 2));
			userPanel.Name = "userPanel" + id;
			userPanel.Size = new Size(630, columnHeight + 2);
			
			btnName.Location = new Point(0, 0);
			btnName.Name = "btnName" + id;
			btnName.Font = new Font(font, btnNameFontSize);
			btnName.Size = new Size(btnNameWidth, columnHeight);

			pictureBoxUser.Location = new Point(54, 5);
			pictureBoxUser.Name = "pictureBoxUser" + id;
			pictureBoxUser.Size = new Size(50, 70);

			panelPressure.Location = new Point(btnNameWidth + 2, 0);
			panelPressure.Name = "panelPressure" + id;
			panelPressure.Size = new Size(panelPressureWidth, columnHeight);

			labelPressure.Font = new Font(font, panelPressureFontSize);
			labelPressure.Name = "labelPressure" + id;
			labelPressure.Location = new Point(0, 25);
			labelPressure.Size = new Size(110, 40);

			labelPressureUnit.Font = new Font(font, panelPressureUnitFontSize);
			labelPressureUnit.Name = "labelPressureUnit" + id;
			labelPressureUnit.Location = new Point(100, 45);

			labelAlarm.Font = new Font(font, panelPressureUnitFontSize);
			labelAlarm.Name = "labelAlarm" + id;
			labelAlarm.Location = new Point(60, 70);
			labelAlarm.Visible = false;

			panelCountDown.Location = new Point(btnNameWidth + 2 + panelPressureWidth + 2, 0);
			panelCountDown.Name = "panelCountDown" + id;
			panelCountDown.Size = new Size(panelCountDownWidth, columnHeight);

			labelCountDown.Name = "labelCountDown" + id;
			labelCountDown.Font = new Font(font, panelPressureFontSize);
			labelCountDown.Location = new Point(0, 25);
			labelCountDown.Size = new Size(100, 40);

			labelCountDownUnit.Name = "labelCountDownUnit" + id;
			labelCountDownUnit.Font = new Font(font, panelPressureUnitFontSize);
			labelCountDownUnit.Location = new Point(90, 45);

			pictureBoxStatus.Name = "pictureBoxStatus" + id;
			pictureBoxStatus.Size = new Size(40, 40);
			pictureBoxStatus.Location = new Point(59, 20);

			labelStatus.Font = new Font(font, 10);
			labelStatus.Location = new Point(49, 61);
			labelStatus.Name = "labelStatus" + id;

			btnDetails.Location = new Point(btnNameWidth + 2 + panelPressureWidth + 2 + panelCountDownWidth + 2, 0);
			btnDetails.Name = "btnDetails" + id;
			btnDetails.Size = new Size(btnDetailsWidth, columnHeight);
			btnDetails.Font = new Font(font, btnDetailsFontSize);

			pictureBoxDetails.Location = new Point(50, 5);
			pictureBoxDetails.Name = "pictureBoxDetails" + id;
			pictureBoxDetails.Size = new Size(50, 60);
		}

		private void UserViewInit(int id)
		{
			int x = id % 2, y = id / 2;
			userPanel.BackColor = panelBackColor;
			userPanel.BringToFront();		//设置在最顶层

			//设置姓名控件 相关属性设置
			btnName.Parent = userPanel;
			
			btnName.BackColor = backColor;
			btnName.Cursor = Cursors.Hand;
			btnName.FlatStyle = FlatStyle.Flat;
			btnName.FlatAppearance.BorderSize = 0;
			btnName.FlatAppearance.BorderColor = Color.White;
			btnName.ForeColor = foreColor;
			btnName.Padding = new Padding(0, 0, 0, 0);
			
			btnName.TextAlign = ContentAlignment.BottomCenter;
			btnName.Text = "姓名:";
			//btnName.Enabled = false;

			pictureBoxUser.Parent = btnName;
			pictureBoxUser.BackColor = Color.Transparent;
			pictureBoxUser.Image = Properties.Resources.UserImageNew;
			pictureBoxUser.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBoxUser.Enabled = false;

			//设置压力控件 相关属性设置
			panelPressure.Parent = userPanel;
			panelPressure.BackColor = backColor;
			labelPressure.Parent = panelPressure;
			labelPressure.AutoSize = false;
			labelPressure.BackColor = Color.Transparent;
			labelPressure.ForeColor = Color.White;
			labelPressure.TextAlign = ContentAlignment.MiddleRight;
			labelPressure.Text = "28.5";

			labelPressureUnit.Parent = panelPressure;
			labelPressureUnit.BringToFront();
			labelPressureUnit.AutoSize = true;
			labelPressureUnit.BackColor = Color.Transparent;
			labelPressureUnit.ForeColor = Color.White;
			labelPressureUnit.Text = "Mpa";

			labelAlarm.Parent = panelPressure;
			labelAlarm.BringToFront();
			labelAlarm.AutoSize = true;
			labelAlarm.BackColor = Color.Transparent;
			labelAlarm.ForeColor = Color.White;
			labelAlarm.Text = "报警中";

			//设置倒计时控件 相关属性设置
			panelCountDown.Parent = userPanel;
			panelCountDown.BackColor = backColor;
			labelCountDown.Parent = panelCountDown;
			labelCountDown.AutoSize = false;
			labelCountDown.BackColor = Color.Transparent;
			labelCountDown.ForeColor = Color.White;
			labelCountDown.TextAlign = ContentAlignment.MiddleRight;
			labelCountDown.Text = "118";
			
			labelCountDownUnit.Parent = panelCountDown;
			labelCountDownUnit.BringToFront();
			labelCountDownUnit.AutoSize = true;
			labelCountDownUnit.BackColor = Color.Transparent;
			labelCountDownUnit.ForeColor = Color.White;
			labelCountDownUnit.Text = "分钟";

			pictureBoxStatus.Parent = panelCountDown;
			pictureBoxStatus.Image = Properties.Resources.Shutdown;
			pictureBoxStatus.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBoxStatus.Enabled = false;
			pictureBoxStatus.Visible = false;

			labelStatus.Parent = panelCountDown;
			labelStatus.AutoSize = true;
			labelStatus.BackColor = Color.Transparent;
			labelStatus.ForeColor = Color.White;
			labelStatus.Visible = false;

			//设置详情控件 相关属性设置
			btnDetails.Parent = userPanel;
			btnDetails.BackColor = backColor;
			btnDetails.Cursor = Cursors.Hand;
			btnDetails.FlatStyle = FlatStyle.Flat;
			btnDetails.FlatAppearance.BorderSize = 0;
			btnDetails.FlatAppearance.BorderColor = Color.White;
			btnDetails.ForeColor = btnDetailsForeColor;
			btnDetails.Text = "查  看";
			btnDetails.TextAlign = ContentAlignment.BottomCenter;

			pictureBoxDetails.Parent = btnDetails;
			pictureBoxDetails.BackColor = Color.Transparent;
			pictureBoxDetails.Image = Properties.Resources.DetailsImageNew;
			pictureBoxDetails.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBoxDetails.Enabled = false;

			//设置位置
			setProperties(id);
		}
		/*************************************************/
		#endregion


		#region 外部调用函数
		/*********************外部调用函数*********************/
		//重新设置位置
		public void RearrangeUserView(int id)
		{
			int x = id % 2, y = id / 2;
			userPanel.Location = new Point(x * 650, y * (columnHeight + 2));

			setProperties(id);
		}
		/*************************************************/
		#endregion
	}
}
