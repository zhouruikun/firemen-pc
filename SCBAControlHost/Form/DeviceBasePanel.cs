using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MyUtils;

namespace SCBAControlHost
{
	public partial class FormMain
	{
		private string DeviceBaseRootPath = @".\res\DeviceBase\SrcFiles";		//知识库根目录
		TreeNode rootNodeDeviceBase = new TreeNode("设备库");

		private void DeviceBaseInit()
		{
			btnDeviceBaseAdd.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnDeviceBaseAdd.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnDeviceBaseDel.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnDeviceBaseDel.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnDeviceBaseCheck.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnDeviceBaseCheck.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnDeviceBaseReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnDeviceBaseReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnDeviceBaseReturn.Click += new EventHandler(btnDeviceBaseReturn_Click);
			btnDeviceBaseAdd.Click += new EventHandler(btnDeviceBaseAdd_Click);
			btnDeviceBaseDel.Click += new EventHandler(btnDeviceBaseDel_Click);
			btnDeviceBaseCheck.Click += new EventHandler(btnDeviceBaseCheck_Click);

			treeViewDeviceBase.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeViewDeviceBase_NodeMouseDoubleClick);

			LoadDeviceBase(null);
		}

		//加载知识库到TreeView中
		public void LoadDeviceBase(object obj)
		{
			//先清空所有的节点
			rootNodeDeviceBase.Nodes.Clear();
			treeViewDeviceBase.Nodes.Clear();

			//在依次加入节点
			try
			{
				//父节点
				if (Directory.Exists(DeviceBaseRootPath))		//若目录存在
				{
					rootNodeDeviceBase.Tag = GetTagPath(Path.GetFullPath(DeviceBaseRootPath));		//将目录的全路径存到Tag中
					rootNodeDeviceBase.ImageIndex = 0;
					rootNodeDeviceBase.SelectedImageIndex = 0;
					//给treeview添加节点
					this.treeViewDeviceBase.Nodes.Add(rootNodeDeviceBase);
					//调用方法递归出所有文件，并将父节点和路径传入
					AddAllValidFiles(DeviceBaseRootPath, rootNodeDeviceBase);
					rootNodeDeviceBase.Expand();		//展开父级目录
				}

			}
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
		}

		//添加按钮点击事件
		void btnDeviceBaseAdd_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = treeViewDeviceBase.SelectedNode;
			TreeNode ParentNode = null;
			string savePath = null;
			if (selectedNode != null)		//选中了节点
			{
				string path = ((string[])selectedNode.Tag)[0];		//源文件全路径
				// 若选中的节点在同步库中, 则将其改为本地库
				if (path.Contains("\\res\\DeviceBase\\SrcFiles\\同步库"))
				{
					foreach (TreeNode node in rootNodeDeviceBase.Nodes)
					{
						if (node.Text == "本地库")
							ParentNode = node;
					}
					savePath = ((string[])rootNodeDeviceBase.Tag)[0] + "\\本地库";
				}
				else
				{
					if (Directory.Exists(path)) { savePath = path; ParentNode = selectedNode; }						//如果选中的是目录, 则保存路径就是当前目录
					else { savePath = ((string[])selectedNode.Parent.Tag)[0]; ParentNode = selectedNode.Parent; }	//如果选中的是文件, 则保存路径就是当前文件的目录
				}
			}
			else							//如果没有选中, 则保存路径就是本地库目录
			{
				foreach (TreeNode node in rootNodeDeviceBase.Nodes)
				{
					if (node.Text == "本地库")
						ParentNode = node;
				}
				savePath = ((string[])rootNodeDeviceBase.Tag)[0] + "\\本地库";
			}

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Word|*.docx|Word|*.doc";
			openFileDialog.FilterIndex = 0;
			openFileDialog.RestoreDirectory = true;				//保存对话框是否记忆上次打开的目录
			openFileDialog.Title = "添加文件";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (AppUtil.CopyFileTo(openFileDialog.FileName, savePath))	//若拷贝文件成功
				{
					string filePath = savePath + "\\" + Path.GetFileName(openFileDialog.FileName);
					TreeNode subFileNode = new TreeNode(Path.GetFileNameWithoutExtension(filePath));
					subFileNode.Tag = GetTagPath(Path.GetFullPath(filePath));
					subFileNode.ImageIndex = 1;
					subFileNode.SelectedImageIndex = 1;
					ParentNode.Nodes.Add(subFileNode);
					//写入按钮点击记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.DeviceBasePanel, (int)BtnOfDeviceBasePanel.AddBtn, openFileDialog.FileName));
				}
			}
		}

		//删除按钮点击事件
		void btnDeviceBaseDel_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = treeViewDeviceBase.SelectedNode;
			if ((selectedNode != null) && (selectedNode != rootNodeDeviceBase))		//不能删除空节点
			{
				if ((selectedNode != rootNodeDeviceBase) && (selectedNode != rootNodeDeviceBase))		//不能删除根节点 和 二级子节点
				{
					string path = ((string[])selectedNode.Tag)[0];
					if (Directory.Exists(path))		//如果选中的是目录, 则删除路径就是当前目录
					{
						if (MessageBox.Show("您确定要删除“" + selectedNode.Text + "”目录吗?", "删除文件提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
						{
							try
							{
								//写入按钮点击记录到日志文件中
								worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.DeviceBasePanel, (int)BtnOfDeviceBasePanel.DeleteBtn, ((string[])selectedNode.Tag)[0]));
								Directory.Delete(((string[])selectedNode.Tag)[0], true);
								Directory.Delete(((string[])selectedNode.Tag)[1], true);
								treeViewDeviceBase.Nodes.Remove(selectedNode);
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
						}
					}
					else							//如果选中的是文件, 则删除路径就是当前文件的目录
					{
						if (MessageBox.Show("您确定要删除“" + selectedNode.Text + "”文件吗?", "删除文件提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
						{
							try
							{
								//写入按钮点击记录到日志文件中
								worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.DeviceBasePanel, (int)BtnOfDeviceBasePanel.DeleteBtn, ((string[])selectedNode.Tag)[0]));
								File.Delete(((string[])selectedNode.Tag)[0]);
								Directory.Delete(((string[])selectedNode.Tag)[1], true);
								treeViewDeviceBase.Nodes.Remove(selectedNode);
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
						}
					}
				}
			}
		}

		//查看按钮点击事件
		void btnDeviceBaseCheck_Click(object sender, EventArgs e)
		{
			TreeNode fileNode = treeViewDeviceBase.SelectedNode;
			if (fileNode != null)	//若选中了节点
			{
				string srcFilePath = ((string[])fileNode.Tag)[0];		//源文件全路径
				string htmlDirPath = ((string[])fileNode.Tag)[1];		//Html文档所在文件夹路径
				string htmlFilePath = ((string[])fileNode.Tag)[2];		//Html文档全路径
				if (File.Exists(srcFilePath))		//若所选中的文件存在
				{
					//写入按钮点击记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.DeviceBasePanel, (int)BtnOfDeviceBasePanel.CheckBtn, srcFilePath));
					if (File.Exists(htmlFilePath))	//若Html文件存在, 则显示出来
					{
						webBrowserDeviceBase.Navigate(htmlFilePath);		//显示到WebBrowser中
					}
					else											//若Html文件不存在, 则创建
					{
						if (DeviceBaseGenerateHtmlByWord(srcFilePath))	//若创建Html文档成功
						{
							webBrowserDeviceBase.Navigate(htmlFilePath);	//则显示到WebBrowser中
						}
					}
				}
			}
		}

		//根据word文档生成对应的html文档, 并存储到指定位置
		private bool DeviceBaseGenerateHtmlByWord(string wordFullPath)
		{
			if (File.Exists(wordFullPath))		//若所选中的文件存在
			{
				//获取文件对应的HTML所在目录----目录名+\\+不带后缀的文件名+"-html"
				string htmlDirPath = Path.GetDirectoryName(wordFullPath).Replace("SrcFiles", "HtmlFiles") + "\\" + Path.GetFileNameWithoutExtension(wordFullPath) + "-html";
				//获取对应的html文件的文件名
				string htmlFilePath = htmlDirPath + "\\" + Path.GetFileNameWithoutExtension(wordFullPath) + ".html";

				if (Directory.Exists(htmlDirPath))	//若存放HTML文件的目录存在
				{
					if (File.Exists(htmlFilePath))		//若Html文件存在, 则显示出来
					{
						webBrowserDeviceBase.Navigate(htmlFilePath);		//显示到WebBrowser中
					}
					else								//若Html文件不存在, 则创建
					{
						try
						{
							AppUtil.ConvertDocToHtml(wordFullPath, htmlFilePath);	//创建Html文件
							return true;
						}
						catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
					}
				}
				else								//若存放HTML文件的目录不存在, 则创建目录
				{
					try
					{
						Directory.CreateDirectory(htmlDirPath);
						AppUtil.ConvertDocToHtml(wordFullPath, htmlFilePath);		//创建Html文件
						return true;
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }

				}
			}
			return false;
		}

		//双击节点事件--执行查看动作
		void treeViewDeviceBase_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			btnDeviceBaseCheck.PerformClick();
		}

		//返回按钮点击事件
		void btnDeviceBaseReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelContentMain);
			//写入按钮点击记录到日志文件中
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.DeviceBasePanel, (int)BtnOfDeviceBasePanel.DeviceBaseReturn, null));
		}
		
	}
}
