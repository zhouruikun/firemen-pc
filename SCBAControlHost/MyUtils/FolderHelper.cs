using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;

namespace MyUtils
{
	/// <summary>
	/// 文件夹操作类
	/// </summary>
	public static class FolderHelper
	{
		private static ILog log = LogManager.GetLogger("ErrorCatched.Logging");//获取一个日志记录器

		/// <summary>
		/// 复制文件夹
		/// </summary>
		/// <param name="sourceFolderName">源文件夹目录</param>
		/// <param name="destFolderName">目标文件夹目录(可以不存在)</param>
		public static bool Copy(string sourceFolderName, string destFolderName)
		{
			return Copy(sourceFolderName, destFolderName, false);
		}

		/// <summary>
		/// 复制文件夹
		/// </summary>
		/// <param name="sourceFolderName">源文件夹目录</param>
		/// <param name="destFolderName">目标文件夹目录</param>
		/// <param name="overwrite">允许覆盖文件</param>
		public static bool Copy(string sourceFolderName, string destFolderName, bool overwrite)
		{
			if (Directory.Exists(sourceFolderName))
			{
				try
				{
					var sourceFilesPath = Directory.GetFileSystemEntries(sourceFolderName);

					for (int i = 0; i < sourceFilesPath.Length; i++)
					{
						var sourceFilePath = sourceFilesPath[i];
						var directoryName = Path.GetDirectoryName(sourceFilePath);
						var forlders = directoryName.Split('\\');
						var lastDirectory = forlders[forlders.Length - 1];
						var dest = Path.Combine(destFolderName, lastDirectory);

						if (File.Exists(sourceFilePath))
						{
							var sourceFileName = Path.GetFileName(sourceFilePath);
							if (!Directory.Exists(dest))
							{
								Directory.CreateDirectory(dest);
							}
							File.Copy(sourceFilePath, Path.Combine(dest, sourceFileName), overwrite);
						}
						else
						{
							if (!Copy(sourceFilePath, dest, overwrite))
								return false;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					log.Info(AppUtil.getExceptionInfo(ex));
					return false;
				}
			}
			else
				return false;

			return true;
		}

		/// <summary>
		/// 删除文件夹下所有内容
		/// </summary>
		/// <param name="path">文件夹路径</param>
		public static void DeleteAllInDir(string path)
		{
			try
			{
				//如果目录存在就删除
				if (Directory.Exists(path))
					Directory.Delete(path, true);

				//在创建
				Directory.CreateDirectory(path);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
