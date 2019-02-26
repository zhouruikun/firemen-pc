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
		private string KnowledgeBaseRootPath = @".\res\KnowledgeBase\SrcFiles";		//知识库根目录
		TreeNode rootNodeKnowledgeBase = new TreeNode("救援知识库");

		private void KnowledgeBaseInit()
		{
			btnKnowledgeBaseAdd.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnKnowledgeBaseAdd.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnKnowledgeBaseDel.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnKnowledgeBaseDel.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnKnowledgeBaseCheck.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_y);
			btnKnowledgeBaseCheck.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_y);
			btnKnowledgeBaseReturn.MouseDown += new System.Windows.Forms.MouseEventHandler(btnCircularPress_g);
			btnKnowledgeBaseReturn.MouseUp += new System.Windows.Forms.MouseEventHandler(btnCircularPop_g);

			btnKnowledgeBaseReturn.Click += new EventHandler(btnKnowledgeBaseReturn_Click);
			btnKnowledgeBaseAdd.Click += new EventHandler(btnKnowledgeBaseAdd_Click);
			btnKnowledgeBaseDel.Click += new EventHandler(btnKnowledgeBaseDel_Click);
			btnKnowledgeBaseCheck.Click += new EventHandler(btnKnowledgeBaseCheck_Click);

			treeViewKnowledgeBase.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeViewKnowledgeBase_NodeMouseDoubleClick);

			LoadKnowledgeBase(null);
		}

		//加载知识库到TreeView中
		public void LoadKnowledgeBase(object obj)
		{
			//先清空所有的节点
			rootNodeKnowledgeBase.Nodes.Clear();
			treeViewKnowledgeBase.Nodes.Clear();

			//在依次加入节点
			try
			{
				//父节点
				if (Directory.Exists(KnowledgeBaseRootPath))		//若目录存在
				{
					rootNodeKnowledgeBase.Tag = GetTagPath(Path.GetFullPath(KnowledgeBaseRootPath));		//将目录的全路径存到Tag中
					rootNodeKnowledgeBase.ImageIndex = 0;
					rootNodeKnowledgeBase.SelectedImageIndex = 0;
					//给treeview添加节点
					this.treeViewKnowledgeBase.Nodes.Add(rootNodeKnowledgeBase);
					//调用方法递归出所有文件，并将父节点和路径传入
					AddAllValidFiles(KnowledgeBaseRootPath, rootNodeKnowledgeBase);
					rootNodeKnowledgeBase.Expand();		//展开父级目录
				}

			}
			catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
		}

		//添加按钮点击事件
		void btnKnowledgeBaseAdd_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = treeViewKnowledgeBase.SelectedNode;
			TreeNode ParentNode = null;
			string savePath = null;
			if (selectedNode != null)
			{
				string path = ((string[])selectedNode.Tag)[0];		//源文件全路径
				// 若选中的节点在同步库中, 则将其改为本地库
				if (path.Contains("\\res\\KnowledgeBase\\SrcFiles\\同步库"))
				{
					foreach (TreeNode node in rootNodeKnowledgeBase.Nodes)
					{
						if (node.Text == "本地库")
							ParentNode = node;
					}
					savePath = ((string[])rootNodeKnowledgeBase.Tag)[0] + "\\本地库";
				}
				else
				{
					if (Directory.Exists(path)) { savePath = path; ParentNode = selectedNode; }						//如果选中的是目录, 则保存路径就是当前目录
					else { savePath = ((string[])selectedNode.Parent.Tag)[0]; ParentNode = selectedNode.Parent; }	//如果选中的是文件, 则保存路径就是当前文件的目录
				}
			}
			else													//如果没有选中, 则保存路径就是本地库
			{
				foreach (TreeNode node in rootNodeKnowledgeBase.Nodes)
				{
					if (node.Text == "本地库")
						ParentNode = node;
				}
				savePath = ((string[])rootNodeKnowledgeBase.Tag)[0] + "\\本地库";
			}

			if (ParentNode == null)
				return;

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
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.KnowledgeBasePanel, (int)BtnOfKnowledgeBase.AddBtn, openFileDialog.FileName));
				}
			}
		}

		//删除按钮点击事件
		void btnKnowledgeBaseDel_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = treeViewKnowledgeBase.SelectedNode;
			if ((selectedNode != null) && (selectedNode != rootNodeKnowledgeBase))		//不能删除空节点
			{
				if ((selectedNode != rootNodeKnowledgeBase) && (selectedNode.Parent != rootNodeKnowledgeBase))		//不能删除根节点 和 二级子节点
				{
					string path = ((string[])selectedNode.Tag)[0];
					if (Directory.Exists(path))		//如果选中的是目录, 则删除路径就是当前目录
					{
						if (MessageBox.Show("您确定要删除“" + selectedNode.Text + "”目录吗?", "删除文件提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
						{
							try
							{
								//写入按钮点击记录到日志文件中
								worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.KnowledgeBasePanel, (int)BtnOfKnowledgeBase.DeleteBtn, ((string[])selectedNode.Tag)[0]));
								Directory.Delete(((string[])selectedNode.Tag)[0], true);
								Directory.Delete(((string[])selectedNode.Tag)[1], true);
								treeViewKnowledgeBase.Nodes.Remove(selectedNode);
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
								worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.KnowledgeBasePanel, (int)BtnOfKnowledgeBase.DeleteBtn, ((string[])selectedNode.Tag)[0]));
								File.Delete(((string[])selectedNode.Tag)[0]);
								Directory.Delete(((string[])selectedNode.Tag)[1], true);
								treeViewKnowledgeBase.Nodes.Remove(selectedNode);
							}
							catch (Exception ex) { Console.WriteLine(ex.Message); log.Info(AppUtil.getExceptionInfo(ex)); }
						}
					}
				}
			}
		}

		//查看按钮点击事件
		void btnKnowledgeBaseCheck_Click(object sender, EventArgs e)
		{
			TreeNode fileNode = treeViewKnowledgeBase.SelectedNode;
			if (fileNode != null)	//若选中了节点
			{
				string srcFilePath = ((string[])fileNode.Tag)[0];		//源文件全路径
				string htmlDirPath = ((string[])fileNode.Tag)[1];		//Html文档所在文件夹路径
				string htmlFilePath = ((string[])fileNode.Tag)[2];		//Html文档全路径
				if (File.Exists(srcFilePath))		//若所选中的文件存在
				{
					//写入按钮点击记录到日志文件中
					worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.KnowledgeBasePanel, (int)BtnOfKnowledgeBase.CheckBtn, srcFilePath));
					if (File.Exists(htmlFilePath))	//若Html文件存在, 则显示出来
					{
						webBrowserKnowledgeBase.Navigate(htmlFilePath);		//显示到WebBrowser中
					}
					else											//若Html文件不存在, 则创建
					{
						if (KnowledgeBaseGenerateHtmlByWord(srcFilePath))	//若创建Html文档成功
						{
							webBrowserKnowledgeBase.Navigate(htmlFilePath);	//则显示到WebBrowser中
						}
					}
				}
			}
		}

		//根据word文档生成对应的html文档, 并存储到指定位置
		private bool KnowledgeBaseGenerateHtmlByWord(string wordFullPath)
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
						webBrowserKnowledgeBase.Navigate(htmlFilePath);		//显示到WebBrowser中
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

		//更新知识库, 即从服务器上下载知识库文件
		public void UpdateKnowledgeBase()
		{

		}

		//双击节点事件--执行查看动作
		void treeViewKnowledgeBase_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			btnKnowledgeBaseCheck.PerformClick();
		}

		//返回按钮点击事件
		void btnKnowledgeBaseReturn_Click(object sender, EventArgs e)
		{
			PanelSwitch(CurPanel.EpanelContentMain);
			//写入按钮点击记录到日志文件中
			worklog.LogQueue_Enqueue(LogCommand.getButtonClickRecord(BTNPANEL.KnowledgeBasePanel, (int)BtnOfKnowledgeBase.KnowledgeBaseReturn, null));
		}
		
	}
}
