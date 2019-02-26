using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

/// 使用方法：
/// 1.MyUtil.CtrlAutoSize autosize;          //申明本类对象为全局变量
/// 2.private void Form1_Load(object sender, EventArgs e)
///    {
///        this.Resize+=new EventHandler(Form1_Resize);
///        autosize = new CMyClass.CtrlAutoSize(this);		//实例化对象并传递"this"
///        autosize.setControlsTag(this);					//递归地设置控件及其子控件的Tag, 在窗口初始化的最后之后调用
///     }
///  3.public void Form1_Resize(object sender,EventArgs e)
///    {
///        autosize.setControls(this);                    //在resize消息里调用此函数以自动设置窗口控件大小和位置
///    }
/// 
namespace MyUtils
{
	class CtrlAutoSize
	{
		private float x, y;         //父窗口的大小
		float ratioX, ratioY;       //缩放比例因子

		//构造函数中记录了父窗口的大小
		public CtrlAutoSize(System.Windows.Forms.Form form)
		{
			x = form.Width;
			y = form.Height;
		}

		//递归地设置控件及其子控件的Tag, 在窗口初始化的最后之后调用, 在程序中动态创建的控件最后也要调用此函数
		public void setControlsTag(Control parent)
		{
			parent.Tag = parent.Width + ":" + parent.Height + ":" + parent.Left + ":" + parent.Top + ":" + parent.Font.Size;
			foreach (Control con in parent.Controls)
			{
				setControlsTag(con);
			}
		}

		//设置单个控件的Tag
		private void setControlTag(Control con)
		{
			con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
		}

		public void resizeControl(Control parent)
		{
			ratioX = parent.Width / x;
			ratioY = parent.Height / y;
			adjustControls(parent);
		}
		private void adjustControls(Control parent)
		{
			foreach (Control con in parent.Controls)
			{
				string[] mytag = con.Tag.ToString().Split(new char[] { (':') });    //取出Tag中的内容
				con.Width = (int)(Convert.ToSingle(mytag[0]) * ratioX);
				con.Height = (int)(Convert.ToSingle(mytag[1]) * ratioY);
				con.Left = (int)(Convert.ToSingle(mytag[2]) * ratioX);
				con.Top = (int)(Convert.ToSingle(mytag[3]) * ratioY);
				Single currentSize = Convert.ToSingle(mytag[4]) * ratioY;
				con.Font = new System.Drawing.Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
				if (con.Controls.Count > 0)
				{
					adjustControls(con);
				}
			}
		}

		public static Size GetSizeByTag(Control con)
		{
			Size size = new Size(0, 0);
			string[] mytag = con.Tag.ToString().Split(new char[] { (':') });    //取出Tag中的内容
			if (mytag[0] != null)
			{
				size.Width = (int)Convert.ToSingle(mytag[0]);
				size.Height = (int)Convert.ToSingle(mytag[1]);
			}
			return size;
		}
		public static Point GetLocationByTag(Control con)
		{
			Point point = new Point(0, 0);
			string[] mytag = con.Tag.ToString().Split(new char[] { (':') });    //取出Tag中的内容
			if (mytag[0] != null)
			{
				point.X = (int)Convert.ToSingle(mytag[2]);
				point.Y = (int)Convert.ToSingle(mytag[3]);
			}
			return point;
		}
	}
}
