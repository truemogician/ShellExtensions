using FileDateCopier.Dialog;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FileDateCopier {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	public class FileFolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() {
			string[] selectedPaths = SelectedItemPaths.ToArray();
			var menu = new ContextMenuStrip();
			if (selectedPaths.Length == 1) {
				menu.Items.Add(ContextMenu.SingleMenu(selectedPaths[0]));
				if (Path.GetExtension(selectedPaths[0]) == ".fdi") {
					menu.Items.Add(new ToolStripSeparator());
					menu.Items.Add(ContextMenu.FileDateInformationMenu(selectedPaths[0]));
				}
			}
			else
				menu.Items.AddRange(ContextMenu.MultipleMenu(selectedPaths));
			return menu;
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class BackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() => new ContextMenuStrip {
			Items = { ContextMenu.SingleMenu(FolderPath, true) }
		};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".fdi")]
	public class DropHandler : SharpDropHandler {
		protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;

		protected override void Drop(DragEventArgs dragEventArgs) {
			Applier.Apply(DragItems.ToArray(), SelectedItemPath);
			MessageBox.Show("日期导入完毕");
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".fdi")]
	public class IconHandler : SharpIconHandler {
		protected override Icon GetIcon(bool smallIcon, uint iconSize) => Resource.Main;
	}

	//[ComVisible(true)]
	//public class IconOverlayHandler : SharpIconOverlayHandler {
	//	protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes) {
	//		if (Path.GetExtension(path) != ".fdi")
	//			return false;
	//		using var reader = new StreamReader(path);
	//		char applyToChildren = (char)reader.Read();
	//		reader.Close();
	//		return applyToChildren == '1';
	//	}

	//	protected override Icon GetOverlayIcon() => Resource.Hierarchy;

	//	protected override int GetPriority() => 0;
	//}

	static internal class ContextMenu {
		private static ToolStripSeparator Separator() => new ToolStripSeparator();

		private static ToolStripMenuItem Help()
			=> new ToolStripMenuItem("帮助文档", Resource.Help, (sender, e) => new Progress().ShowDialog());

		private static ToolStripMenuItem Advanced(string[] paths)
			=> new ToolStripMenuItem("高级设置", Resource.Advanced, (sender, e) => new AdvancedGenerator(paths).ShowDialog());

		private static ToolStripMenuItem Manual()
			=> new ToolStripMenuItem("手动配置", Resource.Manual, (sender, e) => new ManualGenerator().ShowDialog());

		private static string GenerateFileName(string path, bool isGeneral) {
			string suffix = (isGeneral ? ".general" : ".special") + ".fdi";
			string directory = Path.GetDirectoryName(path);
			string srcname = Path.GetFileNameWithoutExtension(path);
			string filename = srcname;
			int index = 1;
			while (File.Exists(Path.Combine(directory, filename + suffix)))
				filename = srcname + $"({++index})";
			return filename + suffix;
		}
		private static string GenerateFileName(string[] paths) {
			string suffix = ".special.fdi";
			string directory = Path.GetDirectoryName(paths[0]);
			string srcname = Path.GetFileName(directory);
			string filename = srcname;
			int index = 1;
			while (File.Exists(Path.Combine(directory, filename + suffix)))
				filename = srcname + $"({++index})";
			return filename + suffix;
		}

		internal static ToolStripMenuItem SingleMenu(string path, bool dirBackground = false) {
			string dstDirectory = dirBackground ? path : Path.GetDirectoryName(path);
			var pathType = FileDateInformation.GetPathType(path, true);
			var result = new ToolStripMenuItem {
				Text = "生成日期文件",
				Image = Resource.Generate,
				DropDownItems = {
					new ToolStripMenuItem("通用（仅拖放内容）", Resource.Globe, (sender, e) => {
						var content = Generator.Generate(path);
						Generator.WriteFile(Path.Combine(dstDirectory, GenerateFileName(path,true)), content);
					}),
					new ToolStripMenuItem("通用（含子结构）", Resource.Globe, (sender, e) => {
						var content = Generator.Generate(path, includesChildren:true);
						Generator.WriteFile(Path.Combine(dstDirectory, GenerateFileName(path,true)), content, true);
					}),
					new ToolStripMenuItem(pathType == PathType.Directory ? "匹配路径（仅文件夹）":"匹配路径", Resource.Aim, (sender, e) => {
						var content = Generator.Generate(path, true);
						Generator.WriteFile(Path.Combine(dstDirectory, GenerateFileName(path,false)), content);
					})
				}
			};
			if (pathType == PathType.Directory)
				result.DropDownItems.Add(new ToolStripMenuItem("匹配路径（含子结构）", Resource.Aim, (sender, e) => {
					var content = Generator.Generate(path, true, true);
					Generator.WriteFile(Path.Combine(dstDirectory, GenerateFileName(path, false)), content, true);
				}));
			result.DropDownItems.Add(Separator());
			if (dirBackground)
				result.DropDownItems.Add(Manual());
			result.DropDownItems.Add(Advanced(new string[] { path }));
			result.DropDownItems.Add(Help());
			return result;
		}

		internal static ToolStripMenuItem[] MultipleMenu(string[] paths) {
			bool hasDirectory = paths.Any(path => Directory.Exists(path));
			var geneMenu = new ToolStripMenuItem("生成日期文件", Resource.Generate);
			geneMenu.DropDownItems.Add(new ToolStripMenuItem(
				hasDirectory ? "匹配路径（仅文件夹）" : "匹配路径",
				Resource.Aim,
				(sender, e) => {
					var content = Generator.Generate(paths);
					Generator.WriteFile(Path.Combine(Path.GetDirectoryName(paths[0]), GenerateFileName(paths)), content);
				}
			));
			if (hasDirectory)
				geneMenu.DropDownItems.Add(new ToolStripMenuItem(
					"匹配路径（含子结构）",
					Resource.Aim,
					(sender, e) => {
						var content = Generator.Generate(paths, true);
						Generator.WriteFile(Path.Combine(Path.GetDirectoryName(paths[0]), GenerateFileName(paths)), content, true);
					}
				));
			var syncMenu = new ToolStripMenuItem("同步文件日期", Resource.Sync);
			if (paths.Length <= 16) {
				foreach (string path in paths) {
					var dsts = paths.ToList();
					dsts.Remove(path);
					syncMenu.DropDownItems.Add(new ToolStripMenuItem(
						$"同步到{Path.GetFileName(path)}",
						Resource.Sync,
						(sender, e) => Synchronizor.Synchronize(dsts.ToArray(), path, applyToChildren: true)
					));
				}
				syncMenu.DropDownItems.Add(new ToolStripMenuItem(
					"高级设置",
					Resource.Advanced,
					(sender, e) => new DetailedSynchronizer(paths).ShowDialog()
				));
			}
			else
				syncMenu.Click += (sender, e) => new DetailedSynchronizer(paths).ShowDialog();
			geneMenu.DropDownItems.Add(Separator());
			geneMenu.DropDownItems.Add(Advanced(paths));
			geneMenu.DropDownItems.Add(Help());
			return new ToolStripMenuItem[] { geneMenu, syncMenu };
		}

		internal static ToolStripMenuItem FileDateInformationMenu(string path) =>
			new ToolStripMenuItem("更改配置", Resource.Configuration, (sender, e) => new FdiModifier(path).ShowDialog());
	}
}