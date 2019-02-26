using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Collections;

namespace SCBAControlHost.MyUtils
{
	/// <summary>   
	/// 适用与ZIP压缩   
	/// </summary>   
	public class ZipHelper
	{
		#region 压缩

		/// <summary>
		/// 压缩目录
		/// </summary>
		/// <param name="DirToZip">被压缩的路径(包含文件路径)</param>
		/// <param name="ZipedFile">压缩后的文件名称(包含文件路径)</param>
		/// <param name="CompressionLevel">压缩率0（无压缩）-9（压缩率最高）</param>
		public static void ZipDir(string DirToZip, string ZipedFile, int CompressionLevel)
		{
			//如果目录不存在就返回
			if (!Directory.Exists(DirToZip))
				throw new System.IO.FileNotFoundException("目录:" + DirToZip + "没有找到!");

			//压缩文件为空时默认与压缩文件夹同一级目录
			if (ZipedFile == string.Empty)
			{
				ZipedFile = DirToZip.Substring(DirToZip.LastIndexOf("//") + 1);
				ZipedFile = DirToZip.Substring(0, DirToZip.LastIndexOf("//")) + "//" + ZipedFile + ".zip";
			}

			//若后缀不为zip, 则给其补充zip后缀
			if (Path.GetExtension(ZipedFile) != ".zip")
			{
				ZipedFile = ZipedFile + ".zip";
			}

			//开始压缩
			using (ZipOutputStream zipoutputstream = new ZipOutputStream(File.Create(ZipedFile)))
			{
				zipoutputstream.SetLevel(CompressionLevel);
				ZipDirRecur(zipoutputstream, DirToZip, DirToZip);
			}
		}

		/// <summary>
		/// 递归压缩目录
		/// </summary>
		/// <param name="zipoutputstream">Zip输出流</param>
		/// <param name="ZipedFile">被压缩的路径(包含文件路径)</param>
		private static void ZipDirRecur(ZipOutputStream zipoutputstream, string Dir, string BaseDir)
		{
			Crc32 crc = new Crc32();
			FileStream fs = null;
			byte[] buffer = null;

			string[] SubDir = Directory.GetDirectories(Dir);
			if (SubDir != null)
			{
				foreach (string dirPath in SubDir)
				{
					ZipEntry entry = new ZipEntry(dirPath.Substring(BaseDir.Length) + "\\");
					zipoutputstream.PutNextEntry(entry);

					//对文件夹不断递归
					ZipDirRecur(zipoutputstream, dirPath, BaseDir);
				}
			}

			string[] SubFile = Directory.GetFiles(Dir);
			if (SubFile != null)
			{
				foreach (string filePath in SubFile)
				{
					fs = File.OpenRead(filePath);
					buffer = new byte[fs.Length];
					fs.Read(buffer, 0, buffer.Length);
					ZipEntry entry = new ZipEntry(filePath.Substring(BaseDir.Length));
					entry.Size = fs.Length;
					fs.Close();
					crc.Reset();
					crc.Update(buffer);
					entry.Crc = crc.Value;
					zipoutputstream.PutNextEntry(entry);
					zipoutputstream.Write(buffer, 0, buffer.Length);
				}
			}
		}

		#endregion


		#region 解压缩

		/// <summary>
		/// 功能：解压zip格式的文件。
		/// </summary>
		/// <param name="zipFilePath">压缩文件路径</param>
		/// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>
		/// <param name="err">出错信息</param>
		/// <returns>解压是否成功</returns>
		public static void UnZip(string zipFilePath, string unZipDir)
		{
			if (zipFilePath == string.Empty)
			{
				throw new Exception("压缩文件不能为空！");
			}
			if (!File.Exists(zipFilePath))
			{
				throw new System.IO.FileNotFoundException("压缩文件不存在！");
			}
			//解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
			if (unZipDir == string.Empty)
				unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
			if (!unZipDir.EndsWith("//"))
				unZipDir += "//";
			//解压文件夹不存在时, 创建该文件夹
			if (!Directory.Exists(unZipDir))
				Directory.CreateDirectory(unZipDir);

			//开始解压
			using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
			{

				ZipEntry theEntry;
				while ((theEntry = s.GetNextEntry()) != null)
				{
					string directoryName = Path.GetDirectoryName(theEntry.Name);
					string fileName = Path.GetFileName(theEntry.Name);
					if (directoryName.Length > 0)
					{
						Directory.CreateDirectory(unZipDir + directoryName);
					}
					if (!directoryName.EndsWith("//"))
						directoryName += "//";
					if (fileName != String.Empty)
					{
						using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
						{

							int size = 2048;
							byte[] data = new byte[2048];
							while (true)
							{
								size = s.Read(data, 0, data.Length);
								if (size > 0)
								{
									streamWriter.Write(data, 0, size);
								}
								else
								{
									break;
								}
							}
						}
					}
				}
			}
		}

		#endregion

	}
}