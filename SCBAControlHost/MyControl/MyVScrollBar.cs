using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MyControl
{
	struct VerticalInfo
	{
		public int yMin;
		public int yMax;
		public int height;
	}

	class MyVScrollBar
	{
		VScrollBar vScrollBar;
		public VScrollBar VScrollBar
		{
			get { return vScrollBar; }
			set { vScrollBar = value; }
		}

		VerticalInfo verInfo1;

		Control relaControl;
		public Control RelaControl
		{
			get { return RelaControl; }
			set {
				relaControl = value;
				relaControl.ControlAdded += new ControlEventHandler(RelaControlAddedOrRemoved);
				relaControl.ControlRemoved += new ControlEventHandler(RelaControlAddedOrRemoved);
				relaControl.MouseWheel += new MouseEventHandler(relaControl_MouseWheel);
				relaControl.MouseClick += new MouseEventHandler(relaControl_MouseClick);
				vScrollBar.Parent = relaControl;
				vScrollBar.Size = new Size(15, relaControl.Size.Height);
				vScrollBar.Location = new Point(relaControl.Size.Width - vScrollBar.Size.Width, 0);
				if (getRelaConContentInfo().height <= relaControl.Size.Height)
					vScrollBar.Visible = false;
				else
					vScrollBar.Visible = true;
			}
		}

		void relaControl_MouseClick(object sender, MouseEventArgs e)
		{
			relaControl.Focus();
		}


		public MyVScrollBar()
		{
			vScrollBar = new VScrollBar();
			vScrollBar.Name = "vScrollBar";
			vScrollBar.ValueChanged += new EventHandler(vScrollBar_ValueChanged);
		}


		//获取控件内容的Y轴的最大, 最小值
		public VerticalInfo getRelaConContentInfo()
		{
			VerticalInfo verInfo;
			verInfo.yMin = 0;
			verInfo.yMax = relaControl.Size.Height;
			int max_y = relaControl.Size.Height;
			foreach (Control con in relaControl.Controls)
			{
				int y = con.Location.Y + con.Size.Height;
				if (y > verInfo.yMax)
					verInfo.yMax = y;
				if (verInfo.yMin > con.Location.Y)
					verInfo.yMin = con.Location.Y;
			}
			verInfo.height = verInfo.yMax - verInfo.yMin;
			return verInfo;
		}

		//将滚动条拉到最上
		private void ResumeRelaCon(VerticalInfo verInfo)
		{
			foreach (Control con in relaControl.Controls)
			{
				if(con != this.vScrollBar)
				con.Location = new Point(con.Location.X, con.Location.Y - verInfo.yMin);
			}
			vScrollBar.Value = 0;
		}

		private void AdjustVScrollBar()
		{
			VerticalInfo verInfo = getRelaConContentInfo();
			ResumeRelaCon(verInfo);
			if (verInfo.height <= relaControl.Size.Height)
			{
				vScrollBar.Visible = false;
			}
			else
			{
				vScrollBar.SmallChange = relaControl.Size.Height / 16;
				vScrollBar.LargeChange = relaControl.Size.Height;
				vScrollBar.Maximum = verInfo.height;
				vScrollBar.Visible = true;
			}
		}

		void relaControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (vScrollBar.Visible == true)
			{
				if (e.Delta > 0)
				{
					if ((vScrollBar.Value - vScrollBar.SmallChange) >= 0)
						vScrollBar.Value -= vScrollBar.SmallChange;
					else
						vScrollBar.Value = 0;
				}
				else
				{
					if ((vScrollBar.Value + vScrollBar.SmallChange) <= (vScrollBar.Maximum - vScrollBar.LargeChange + 1))
						vScrollBar.Value += vScrollBar.SmallChange;
					else
						vScrollBar.Value = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
				}
			}
		}

		//当panel增加或删减控件时自动调用
		private void  RelaControlAddedOrRemoved(object sender, ControlEventArgs e)
		{
			Control con = (Control)sender;
			AdjustVScrollBar();
			vScrollBar.BringToFront();
		}

		void vScrollBar_ValueChanged(object sender, EventArgs e)
		{
			VerticalInfo verInfo = getRelaConContentInfo();

			foreach (Control con in relaControl.Controls)
			{
				if (con.Name != "vScrollBar")
				{
					con.Location = new Point(con.Location.X, con.Location.Y - verInfo.yMin - vScrollBar.Value);
				}
			}
		}


	}
}
