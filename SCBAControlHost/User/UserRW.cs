using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using MyUtils;
using log4net;
using System.Globalization;

namespace SCBAControlHost
{
	//enum USER_RW_ERROR_TYPE		//显示指定枚举的底层数据类型
	//{
	//    UserFileNonExist,
	//    ReadFileFailed,
	//    ReadSuccess
	//};

	class UserRW
	{
		public string DefaultUserFileName = "./res/UserTable/FireMen.xlsx";

		private List<UserBasicInfo> userInfoList;
		public List<UserBasicInfo> UserInfoList
		{
			get { return userInfoList; }
			set { userInfoList = value; }
		}

		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		//读取Excel文件中的数据, 返回集合的形式 -- 底层调用
		private IEnumerator ReadExcelFile(string filePath)
		{
			IEnumerator rows = null;
			FileStream file = null;

			//先打开文件
			try
			{
				file = new FileStream(filePath, FileMode.Open);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				//MessageBox.Show("用户配置文件打开失败!");
				if (file != null)
					file.Close();
			}

			if (file != null)
			{
				try
				{
					//根据现有的Excel文档创建工作簿
					XSSFWorkbook workbook = new XSSFWorkbook(file);
					//获得工作表0
					ISheet sheet0 = workbook.GetSheetAt(0);
					//获得所有行的集合
					rows = sheet0.GetRowEnumerator();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
					rows = null;
				}
			}

			return rows;
		}

		private string getCellString(XSSFRow row, int cellNum)
		{
			string tmp = "";
			try
			{
				ICell icell = row.GetCell(cellNum);
				if (icell != null)
					tmp = icell.ToString();
			}
			catch (Exception ex) { }

			return tmp;
		}

		//读取默认的用户配置文件
		public bool ReadDefaultUserFile()
		{
			userInfoList = new List<UserBasicInfo>();

			IEnumerator rows = ReadExcelFile(DefaultUserFileName);

			try
			{
				if (rows != null)
				{
					rows.MoveNext();	//跳过第一行
					//向下移动
					while (rows.MoveNext())
					{
						//获得当前行
						XSSFRow row = rows.Current as XSSFRow;
						if (row.Cells.Count >= 11)		//每行至少有11列
						{
							UserBasicInfo userInfo = new UserBasicInfo();
							userInfo.userNO = getCellString(row, 0);					//读取用户编号
							userInfo.name = getCellString(row, 1);					//读取姓名
							userInfo.birthDate = getCellString(row, 2);				//读取出生年月
							userInfo.uAffiliatedUnit = getCellString(row, 3);			//读取单位
							userInfo.userPhoto = getCellString(row, 4);				//读取照片
							userInfo.duty = getCellString(row, 5);					//读取职务
							int result;
							string[] SerialNo = getCellString(row, 6).Split('-');
							if (int.TryParse(SerialNo[0], out result))					//读取组号
								userInfo.terminalGrpNO = result;
							else return false;
							if (int.TryParse(SerialNo[1], out result))					//读取终端号
								userInfo.terminalNO = result;
							else return false;
							userInfo.terminalCapSpec = getCellString(row, 7);			//读取气瓶容量
							userInfo.BlueToothMac = getCellString(row, 8);			//读取蓝牙MAC地址
							userInfo.WirelessSN = getCellString(row, 9);				//读取无线SN号
							userInfo.Sex = getCellString(row, 10);					//性别

							//解析年龄
							try
							{
								DateTime birthDate = DateTime.ParseExact(userInfo.birthDate, "yyyy-MM-dd", CultureInfo.CurrentCulture);	//获取记录的时间
								userInfo.Age = ((DateTime.Now.Year - birthDate.Year) >= 0) ? ("" + (DateTime.Now.Year - birthDate.Year)) : "";
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

							userInfoList.Add(userInfo);
						}
					}
				}
				else
					return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				MessageBox.Show("读取用户配置文件失败");
			}
			

			return true;
		}

		//读取用户配置文件, 参数传入文件的路径, 返回用户基本信息表
		public List<UserBasicInfo> ReadUserInfoFile(string filePath)
		{
			List<UserBasicInfo> userList = null;
			IEnumerator rows = ReadExcelFile(filePath);

			try
			{
				if (rows != null)
				{
					userList = new List<UserBasicInfo>();
					rows.MoveNext();
					//向下移动
					while (rows.MoveNext())
					{
						//获得当前行
						XSSFRow row = rows.Current as XSSFRow;
						if (row.Cells.Count >= 11)				//每行至少有11列
						{
							UserBasicInfo userInfo = new UserBasicInfo();
							userInfo.userNO = getCellString(row, 0);					//读取用户编号
							userInfo.name = getCellString(row, 1);					//读取姓名
							userInfo.birthDate = getCellString(row, 2);				//读取出生年月
							userInfo.uAffiliatedUnit = getCellString(row, 3);			//读取单位
							userInfo.userPhoto = getCellString(row, 4);				//读取照片
							userInfo.duty = getCellString(row, 5);					//读取职务

							int result;
							string[] SerialNo = getCellString(row, 6).Split('-');
							if (int.TryParse(SerialNo[0], out result))					//读取组号
								userInfo.terminalGrpNO = result;
							if (int.TryParse(SerialNo[1], out result))					//读取终端号
								userInfo.terminalNO = result;
							userInfo.terminalCapSpec = getCellString(row, 7);			//读取气瓶容量
							userInfo.BlueToothMac = getCellString(row, 8);			//读取蓝牙MAC地址
							userInfo.WirelessSN = getCellString(row, 9);				//读取无线SN号
							userInfo.Sex = getCellString(row, 10);					//性别

							//解析年龄
							try
							{
								DateTime birthDate = DateTime.ParseExact(userInfo.birthDate, "yyyy-MM-dd", CultureInfo.CurrentCulture);	//获取记录的时间
								userInfo.Age = ((DateTime.Now.Year - birthDate.Year) >= 0) ? ("" + (DateTime.Now.Year - birthDate.Year)) : "";
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

							//加入到列表中
							userList.Add(userInfo);
						}
					}
				}
				else
					return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				log.Info(AppUtil.getExceptionInfo(ex));
				MessageBox.Show("读取用户配置文件失败");
				return null;
			}

			return userList;
		}

		//保存用户配置文件到外部, filePath为文件的全路径
		public bool SaveUserInfoFile(string filePath, List<User> userList)
		{
			XSSFWorkbook workbook2007 = null;
			bool isSuccess = false;

			//若当前有可用用户, 则可以执行导出操作
			if (userList.Count > 0)
			{
				try
				{
					workbook2007 = new XSSFWorkbook();		//新建xlsx工作簿
					workbook2007.CreateSheet("Sheet1");					//新建1个Sheet工作表
					XSSFSheet SheetOne = (XSSFSheet)workbook2007.GetSheet("Sheet1"); //获取名称为Sheet1的工作表

					SheetOne.CreateRow(0);
					XSSFRow SheetRowTitle = (XSSFRow)SheetOne.GetRow(0);	//获取Sheet1工作表的第0行 
					XSSFCell[] SheetCellTitle = new XSSFCell[11];			//每行有8个单元格
					for (int j = 0; j < 11; j++)								//为第0行创建8个单元格
					{
						SheetCellTitle[j] = (XSSFCell)SheetRowTitle.CreateCell(j);
					}

					//为每个单元格赋值
					SheetCellTitle[0].SetCellValue("用户编号");
					SheetCellTitle[1].SetCellValue("姓名");
					SheetCellTitle[2].SetCellValue("出生年月");
					SheetCellTitle[3].SetCellValue("所属单位");
					SheetCellTitle[4].SetCellValue("照片名称");
					SheetCellTitle[5].SetCellValue("职务");
					SheetCellTitle[6].SetCellValue("终端ID");
					SheetCellTitle[7].SetCellValue("气瓶容量");
					SheetCellTitle[8].SetCellValue("蓝牙MAC地址");
					SheetCellTitle[9].SetCellValue("无线SN号");
					SheetCellTitle[10].SetCellValue("性别");

					for (int i = 0, k = 0; i < userList.Count; i++)
					{
						if (userList[i].BasicInfo.name != null)		//若用户名字不为空, 代表不是临时编组进来的
						{
							SheetOne.CreateRow(k + 1);   //为Sheet1工作表创建第(k+1)行
							XSSFRow SheetRow = (XSSFRow)SheetOne.GetRow(k + 1);	//获取Sheet1工作表的第(k+1)行 
							XSSFCell[] SheetCell = new XSSFCell[11];				//每行有11个单元格
							for (int j = 0; j < 11; j++)
							{
								SheetCell[j] = (XSSFCell)SheetRow.CreateCell(j);  //为每一行创建1个单元格
							}

							//为每个单元格赋值
							SheetCell[0].SetCellValue(userList[i].BasicInfo.userNO);
							SheetCell[1].SetCellValue(userList[i].BasicInfo.name);
							SheetCell[2].SetCellValue(userList[i].BasicInfo.birthDate);
							SheetCell[3].SetCellValue(userList[i].BasicInfo.uAffiliatedUnit);
							SheetCell[4].SetCellValue(userList[i].BasicInfo.userPhoto);
							SheetCell[5].SetCellValue(userList[i].BasicInfo.duty);
							SheetCell[6].SetCellValue(userList[i].BasicInfo.terminalGrpNO.ToString("D8") + "-" + userList[i].BasicInfo.terminalNO.ToString("D2"));
							SheetCell[7].SetCellValue(userList[i].BasicInfo.terminalCapSpec);
							SheetCell[8].SetCellValue(userList[i].BasicInfo.BlueToothMac);
							SheetCell[9].SetCellValue(userList[i].BasicInfo.WirelessSN);
							SheetCell[10].SetCellValue(userList[i].BasicInfo.Sex);
							k++;
						}
					}
				}
				catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
				

				//写入磁盘文件中
				FileStream file2007 = null;
				if (workbook2007 != null)
				{
					try
					{
						file2007 = new FileStream(@"" + filePath, FileMode.Create);
						workbook2007.Write(file2007);
						isSuccess = true;
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
					finally		//最后还要关闭文件
					{
						if (file2007 != null)
							file2007.Close();
						if (workbook2007 != null)
							workbook2007.Close();
					}
				}
			}

			return isSuccess;
		}
	}
}
