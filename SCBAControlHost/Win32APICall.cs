using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace SCBAControlHost
{
	public class Win32APICall
	{
		//获取鼠标位置的API
		[DllImport("User32.dll")]
		public extern static bool GetCursorPos(ref Point pot);
		//设置鼠标位置的API
		[DllImport("User32.dll")]
		public extern static void SetCursorPos(int x, int y);
		//鼠标事件的API
		[DllImport("user32", CharSet = CharSet.Unicode)]
		public static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
		//移动鼠标 
		const int MOUSEEVENTF_MOVE = 0x0001;
		//模拟鼠标左键按下 
		const int MOUSEEVENTF_LEFTDOWN = 0x0002;
		//模拟鼠标左键抬起 
		const int MOUSEEVENTF_LEFTUP = 0x0004;
		//模拟鼠标右键按下 
		const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
		//模拟鼠标右键抬起 
		const int MOUSEEVENTF_RIGHTUP = 0x0010;
		//模拟鼠标中键按下 
		const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
		//模拟鼠标中键抬起 
		const int MOUSEEVENTF_MIDDLEUP = 0x0040;
		//标示是否采用绝对坐标 
		const int MOUSEEVENTF_ABSOLUTE = 0x8000;

		//在指定的决定位置 模拟鼠标进行一次点击
		public static void SimulateMouseClick(Point absPot)
		{
			//保存鼠标移动之前的坐标位置
			Point posPrev = new Point();
			GetCursorPos(ref posPrev);

			//将鼠标移动到指定位置, 并点击
			mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, absPot.X * 65535 / Screen.PrimaryScreen.Bounds.Width, absPot.Y * 65535 / Screen.PrimaryScreen.Bounds.Height, 0, 0);
			
			//将鼠标移回原来的位置
			mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, posPrev.X * 65535 / Screen.PrimaryScreen.Bounds.Width, posPrev.Y * 65535 / Screen.PrimaryScreen.Bounds.Height, 0, 0);
		}

		//设置某窗体控件的滚动条的可见性
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int ShowScrollBar(IntPtr hWnd, int bar, int show);

		//API声明：获取当前焦点控件句柄
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		internal static extern IntPtr GetFocus();
		///获取 当前拥有焦点的控件
		public static Control GetFocusedControl()
		{
			Control focusedControl = null;
			// To get hold of the focused control:
			IntPtr focusedHandle = GetFocus();
			if (focusedHandle != IntPtr.Zero)
				//focusedControl = Control.FromHandle(focusedHandle);
				focusedControl = Control.FromChildHandle(focusedHandle);
			return focusedControl;
		}

		// 系统音量相关
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
		public const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		public const int APPCOMMAND_VOLUME_UP = 0x0a0000;
		public const int APPCOMMAND_VOLUME_DOWN = 0x090000;
		public const int WM_APPCOMMAND = 0x319;
	}

}
