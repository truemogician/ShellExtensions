using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.Interop;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using SharpShell.SharpIconOverlayHandler;
using Extension;
using FileDateCopier.Dialog;

namespace FileDateCopier {
	static internal class ContextMenuStripItems {
		static readonly ToolStripSeparator Seperator = new ToolStripSeparator();
		static readonly ToolStripMenuItem Help = new ToolStripMenuItem("帮助文档", Resource.Help, (sender, e) => {
			throw new NotImplementedException();
		});
		static ToolStripMenuItem Advanced(string[] paths)
			=> new ToolStripMenuItem("高级设置", Resource.Advanced, (sender, e) => new AdvancedGenerator(paths).ShowDialog());
		static ToolStripMenuItem Manual()
			=> new ToolStripMenuItem("手动配置", Resource.Manual, (sender, e) => new ManualGenerator().ShowDialog());

		static internal ToolStripMenuItem SingleMenu(string path, bool manulCreation = false) {
			var pathType = FileDateInformation.GetPathType(path, true);
			var result = new ToolStripMenuItem {
				Text = "生成日期文件",
				Image = Resource.Generate,
				DropDownItems = {
					new ToolStripMenuItem("通用（仅拖放内容）", Resource.Globe, (sender, e) => {
						var content = Generator.Generate(path);
						Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path), "universal.fdi"), content);
					}),
					new ToolStripMenuItem("通用（包括子结构）", Resource.Globe, (sender, e) => {
						var content = Generator.Generate(path, includesChildren:true);
						Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path), "universal.fdi"), content, true);
					}),
					new ToolStripMenuItem(pathType == PathType.Directory ? "专用（仅文件夹）":"专用（仅文件）", Resource.Aim, (sender, e) => {
						var content = Generator.Generate(path, true);
						Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path),"special.fdi"), content);
					})
				}
			};
			if (pathType == PathType.Directory)
				result.DropDownItems.Add(new ToolStripMenuItem("专用（包括子结构）", Resource.Aim, (sender, e) => {
					var content = Generator.Generate(path, true, true);
					Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path), "special.fdi"), content, true);
				}));
			result.DropDownItems.Add(Seperator);
			if (manulCreation)
				result.DropDownItems.Add(Manual());
			result.DropDownItems.Add(Advanced(new string[] { path }));
			result.DropDownItems.Add(Help);
			return result;
		}
		static internal ToolStripMenuItem[] MultipleMenu(string[] paths) {
			bool hasDirectory = paths.Any(path => Directory.Exists(path));
			var geneMenu = new ToolStripMenuItem("生成日期文件", Resource.Generate);
			var syncMenu = new ToolStripMenuItem {
				Text = "同步文件日期",
				Image = Resource.Sync,
				DropDownItems = {
					new ToolStripMenuItem(hasDirectory ? "专用（仅文件夹本身）":"专用", Resource.Aim, (sender, e) => new Synchronizer().ShowDialog())
				}
			};
			if (hasDirectory) {
				geneMenu.DropDownItems.AddRange(new ToolStripMenuItem[]{
					new ToolStripMenuItem("专用（仅文件夹本身）", Resource.Aim, (sender, e) => {
						var content = Generator.Generate(paths);
						Generator.WriteFile(Path.Combine(PathTool.GetCommonPath(paths), "special.fdi"), content);
					}),
					new ToolStripMenuItem("专用（包括子结构）", Resource.Aim, (sender, e) => {
						var content = Generator.Generate(paths, true);
						Generator.WriteFile(Path.Combine(PathTool.GetCommonPath(paths), "special.fdi"), content, true);
					})
				});
			}
			else {
				geneMenu.Click += (sender, e) => {
					var content = Generator.Generate(paths);
					Generator.WriteFile(Path.Combine(PathTool.GetCommonPath(paths), "special.fdi"), content);
				};
			}
			geneMenu.DropDownItems.Add(Seperator);
			geneMenu.DropDownItems.Add(Advanced(paths));
			geneMenu.DropDownItems.Add(Help);
			return new ToolStripMenuItem[] { geneMenu, syncMenu };
		}
		static internal ToolStripMenuItem FileDateInformationMenu(string path) =>
			new ToolStripMenuItem("更改日期配置", Resource.Configuration, (sender, e) => new FdiModifier(path).ShowDialog());
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	class FileFolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;
		protected override ContextMenuStrip CreateMenu() {
			string[] selectedPaths = SelectedItemPaths.ToArray();
			var menu = new ContextMenuStrip();
			if (selectedPaths.Length == 1) {
				menu.Items.Add(ContextMenuStripItems.SingleMenu(selectedPaths[0]));
				if (Path.GetExtension(selectedPaths[0]) == ".fdi") {
					menu.Items.Add(new ToolStripSeparator());
					menu.Items.Add(ContextMenuStripItems.FileDateInformationMenu(selectedPaths[0]));
				}
			}
			else
				menu.Items.AddRange(ContextMenuStripItems.MultipleMenu(selectedPaths));
			return menu;
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	class BackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;
		protected override ContextMenuStrip CreateMenu() => new ContextMenuStrip {
			Items = { ContextMenuStripItems.SingleMenu(FolderPath, true) }
		};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".fdi")]
	class DropHandler : SharpDropHandler {
		protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;
		protected override void Drop(DragEventArgs dragEventArgs) => Applier.Apply(DragItems.ToArray(), SelectedItemPath);
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".fdi")]
	class IconHandler : SharpIconHandler {
		protected override Icon GetIcon(bool smallIcon, uint iconSize) => Resource.Main;
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".fdi")]
	class IconOverlayHandler : SharpIconOverlayHandler {
		protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes) {
			using var reader = new StreamReader(path);
			char applyToChildren = (char)reader.Read();
			reader.Close();
			return applyToChildren == '1';
		}
		protected override Icon GetOverlayIcon() => Resource.Hierarchy;
		protected override int GetPriority() => 0;
	}
}