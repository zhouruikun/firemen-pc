using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCBAControlHost.NetCommunication
{
	//定义一个委托
	public delegate void MyDelegate(object obj);

	class NetDelegate
	{
		public event MyDelegate myEvent;	//定义一个事件委托实例

		//触发事件函数
		internal void TriggerEvent(object obj)
		{
			if (myEvent != null)
			{
				myEvent(obj);		//调用委托函数
			}
		}

	}
}
