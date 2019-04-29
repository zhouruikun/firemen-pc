using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using MyUtils;
using log4net;

namespace SCBAControlHost.SysConfig
{
	[Serializable]
	public class SystemSetting
	{
		public string serialCom = null;
		public int serialBaud = 9600;
		public String unitName;			//单位名称
		public String serverIP;			//服务器IP地址
		public int serverPort;		//服务器端口号
		public String accessAccount;	//服务器访问账号
		public String accessPassword;	//服务器访问密码
		public int alarmThreshold;		//报警点		0--50%  1--10MPa  2--6MPa
		public int groupNumber;     //组号
        public int channal;     //组号
        public String systemPassword = "888888";	//系统密码
		public Dictionary<byte, int> channelDic = new Dictionary<byte, int>();	//信道字典key为byte型的信道号, value为int型的组号(不包含终端号)
		public int ChannelIndex = 0;
	}

	public class SystemConfig
	{
		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		public byte[] ChannelList = new byte[15] { 20, 22, 26, 18, 28, 16, 21, 27, 19, 17, 29, 15, 14, 12, 11 };

		private SystemSetting setting = null;
		public SystemSetting Setting
		{
			get { return setting; }
			set { setting = value; }
		}

		private byte workChannel;		//当前工作信道
		public byte WorkChannel
		{
			get { return workChannel; }
			set { workChannel = value; }
		}

		public SystemConfig()
		{
			//赋予默认值
			setting = new SystemSetting();
			setting.serialCom = "COM5";
			setting.serialBaud = 3;
			setting.unitName = "xxxxxxxx";
			setting.serverIP = "192.168.1.3";
			setting.serverPort = 9000;
			setting.accessAccount = "abcdefg";
			setting.accessPassword = "123456";
			setting.alarmThreshold = 0;
			setting.groupNumber = 3;
            setting.channal = 20;
            setting.systemPassword = "888888";
		}

		//读取系统配置
		public bool ReadSystemSetting()
		{
			bool res = false;
			//用于序列化和反序列化的对象
			IFormatter serializer = new BinaryFormatter();
			FileStream configFile = null;
			try
			{
				//开始反序列化
				configFile = new FileStream("SysConfig.cfg", FileMode.Open, FileAccess.Read);
				setting = serializer.Deserialize(configFile) as SystemSetting;
				configFile.Close();
				res = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				res = false;
				//MessageBox.Show("缺少系统配置文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (configFile != null)
					configFile.Close();
			}
			return res;
		}

		//保存系统配置到磁盘上
		public void SaveSystemSetting(SystemSetting sysSetting)
		{
			//用于序列化和反序列化的对象
			IFormatter serializer = new BinaryFormatter();
			FileStream saveFile = null;
			try
			{
				//开始序列化
				saveFile = new FileStream("SysConfig.cfg", FileMode.Create, FileAccess.Write);
				serializer.Serialize(saveFile, sysSetting);
				saveFile.Close();
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
			finally
			{
				if (saveFile != null)
					saveFile.Close();
			}
		}

		//创建系统配置, 用于软件第一次使用时调用
		public void CreateConfigFile()
		{
			setting = new SystemSetting();
			setting.serialCom = "COM5";
			setting.serialBaud = 3;
			setting.unitName = "xxxxxx";
			setting.serverIP = "192.168.1.3";
			setting.serverPort = 9000;
			setting.accessAccount = "zhangsan";
			setting.accessPassword = "123456";
			setting.alarmThreshold = 0;
			setting.groupNumber = 3;
            setting.channal = 20;

            setting.systemPassword = "888888";

			//创建信道列表
			for (int i = 0; i < 15; i++ )
				setting.channelDic.Add(ChannelList[i], -1);
			setting.ChannelIndex = 1;

			SaveSystemSetting(setting);
		}

		//获取4字节的byte数组形式的组号+终端号
		public byte[] getSerialNOBytes()
		{
			byte[] serialNO = new byte[4];

			int tmp = this.setting.groupNumber;
			serialNO[0] = (byte)((tmp & 0x00FFFFFF) >> 16);
			serialNO[1] = (byte)((tmp & 0x0000FFFF) >> 8);
			serialNO[2] = (byte)(tmp & 0x000000FF);
			serialNO[3] = 0x01;

			return serialNO;
		}
	}
}
