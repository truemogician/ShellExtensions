using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using TrueMogician.Extensions.Enumerable;
using static EntryDateCopier.Utilities;

namespace EntryDateCopier {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	public class FileFolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => SelectedItemPaths.Same(Path.GetDirectoryName);

		protected override ContextMenuStrip CreateMenu() {
			string[] selectedPaths = SelectedItemPaths.ToArray();
			var menu = new ContextMenuStrip();
			if (selectedPaths.Length == 1)
				menu.Items.Add(ContextMenu.CreateSingleMenu(selectedPaths[0], false));
			else
				menu.Items.AddRange(ContextMenu.CreateMultipleMenu(selectedPaths).OfType<ToolStripItem>().ToArray());
			return menu;
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class BackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() => new() {
			Items = { ContextMenu.CreateSingleMenu(FolderPath, true) }
		};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".edi")]
	public class DropHandler : SharpDropHandler {
		protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;

		protected override void Drop(DragEventArgs dragEventArgs) {
			Task.Run(
				async () => {
					await new Applier(DragItems.ToArray(), SelectedItemPath).Apply();
					MessageBox.Show("日期导入完毕");
				}
			);
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".edi")]
	public class IconHandler : SharpIconHandler {
		protected override Icon GetIcon(bool smallIcon, uint iconSize) => Resource.Main;
	}

	internal static class ContextMenu {
		internal const int MAX_SYNC_FILES = 16;

		private static ToolStripSeparator Separator { get; } = new();

		private static string GetFileName(string path, bool general) {
			var suffix = $"{(general ? ".general" : ".special")}.edi";
			string dir = Path.GetDirectoryName(path)!;
			string name = Path.GetFileNameWithoutExtension(path);
			string fileName = name;
			int index = 1;
			while (File.Exists(Path.Combine(dir, fileName + suffix)))
				fileName = name + $"({++index})";
			return fileName + suffix;
		}

		private static string GetFileName(IReadOnlyList<string> paths) {
			const string suffix = ".special.edi";
			string dir = Path.GetDirectoryName(paths[0])!;
			string name = Path.GetFileName(dir);
			string fileName = name;
			int index = 1;
			while (File.Exists(Path.Combine(dir, fileName + suffix)))
				fileName = name + $"({++index})";
			return fileName + suffix;
		}

		internal static ToolStripMenuItem CreateSingleMenu(string path, bool isBackground) {
			string dstDirectory = isBackground ? path : Path.GetDirectoryName(path)!;
			void Generate(bool general, bool includesChildren) {
				var infos = new Generator(path).Generate(general, includesChildren);
				Generator.SaveToFile(Path.Combine(dstDirectory, GetFileName(path, general)), infos);
			}
			var pathType = GetEntryType(path, true);
			var result = new ToolStripMenuItem {
				Text = "生成日期文件",
				Image = Resource.Generate,
				DropDownItems = {
					new ToolStripMenuItem(
						pathType == EntryType.Directory ? "匹配路径（仅文件夹）" : "匹配路径",
						Resource.Aim,
						(_, _) => Generate(false, false)
					)
				}
			};
			if (pathType == EntryType.Directory)
				result.DropDownItems.Add(
					new ToolStripMenuItem(
						"匹配路径（含子结构）",
						Resource.Aim,
						(_, _) => Generate(false, true)
					)
				);
			result.DropDownItems.AddRange(
				new ToolStripItem[] {
					Separator,
					new ToolStripMenuItem(
						"通用（仅拖放内容）",
						Resource.Globe,
						(_, _) => Generate(true, false)
					),
					new ToolStripMenuItem(
						"通用（含子结构）",
						Resource.Globe,
						(_, _) => Generate(true, true)
					)
				}
			);
			return result;
		}

		internal static IEnumerable<ToolStripMenuItem> CreateMultipleMenu(string[] paths) {
			string root = Path.GetDirectoryName(paths[0])!;
			void Generate(bool includesChildren) {
				var infos = new Generator(paths).Generate(includesChildren: includesChildren);
				Generator.SaveToFile(Path.Combine(root, GetFileName(paths)), infos);
			}
			bool hasDirectory = paths.Any(Directory.Exists);
			var menu = new ToolStripMenuItem("生成日期文件", Resource.Generate);
			menu.DropDownItems.Add(
				new ToolStripMenuItem(
					hasDirectory ? "匹配路径（仅文件夹）" : "匹配路径",
					Resource.Aim,
					(_, _) => Generate(false)
				)
			);
			if (hasDirectory)
				menu.DropDownItems.Add(
					new ToolStripMenuItem(
						"匹配路径（含子结构）",
						Resource.Aim,
						(_, _) => Generate(true)
					)
				);
			yield return menu;
			if (paths.Length > MAX_SYNC_FILES)
				yield break;
			static ToolStripMenuItem CreateSyncMenuItem(string text, IList<string> dsts, string src, bool includesChildren, bool appliesToChildren) =>
				new(
					text,
					Resource.Sync,
					(_, _) => Task.Run(() => new Synchronizer(dsts, src).Synchronize(includesChildren, appliesToChildren))
				);
			menu = new ToolStripMenuItem("同步文件日期", Resource.Sync);
			var items = menu.DropDownItems;
			var entryTypes = paths.ToDictionaryWith(p => GetEntryType(p, true));
			int folderCount = entryTypes.Values.Count(t => t == EntryType.Directory);
			foreach (string path in paths) {
				var dsts = paths.ToList();
				dsts.Remove(path);
				items.Add(
					folderCount > (entryTypes[path] == EntryType.Directory ? 1 : 0)
						? new ToolStripMenuItem($"同步到{Path.GetFileName(path)}", Resource.Sync) {
							DropDownItems = {
								CreateSyncMenuItem("仅所选内容", dsts, path, false, false),
								CreateSyncMenuItem("包含子结构", dsts, path, true, true)
							}
						}
						: CreateSyncMenuItem($"同步到{Path.GetFileName(path)}", dsts, path, false, false)
				);
			}
			yield return menu;
		}
	}
}