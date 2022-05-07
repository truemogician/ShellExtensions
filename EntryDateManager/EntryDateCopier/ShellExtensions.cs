using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using TrueMogician.Extensions.Enumerable;
using ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle;
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
				menu.Items.AddRange(ContextMenu.CreateMultipleMenus(selectedPaths).OfType<ToolStripItem>().ToArray());
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
			var applier = new Applier(DragItems.ToArray(), SelectedItemPath);
			var dialog = new ProgressDialog {
				MinimizeBox = true,
				ShowCancelButton = false,
				ShowTimeRemaining = true,
				UseCompactPathsForText = true,
				UseCompactPathsForDescription = true,
				WindowTitle = "文件日期设置进度",
				ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
			};
			int total = 0, current = 0;
			applier.Start += (_, _) => dialog.ReportProgress(0, "正在统计文件数量...", "");
			applier.Ready += (_, args) => {
				total = args.Map.Count;
				dialog.ProgressBarStyle = ProgressBarStyle.ProgressBar;
			};
			applier.SetEntryDate += (_, args) => {
				++current;
				dialog.ReportProgress(
					(int)Math.Round((double)current / total),
					$"进度：{current} / ${total}",
					$"正在设置 {args.Path}"
				);
			};
			dialog.DoWork += (_, _) => {
				var task = applier.Apply();
				task.Wait();
			};
			dialog.Show();
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

		private static ToolStripMenuItem GenerationFieldsConfigItem { get; } = CreateFieldsConfigMenuItem(
			"设置生成的文件日期字段",
			() => (EntryDateFields)Properties.Settings.Default.GenerationFields,
			fields => {
				Properties.Settings.Default.GenerationFields = (byte)fields;
				Properties.Settings.Default.Save();
			}
		);

		private static ToolStripMenuItem SyncFieldsConfigItem { get; } = CreateFieldsConfigMenuItem(
			"设置同步的文件日期字段",
			() => (EntryDateFields)Properties.Settings.Default.SyncFields,
			fields => {
				Properties.Settings.Default.SyncFields = (byte)fields;
				Properties.Settings.Default.Save();
			}
		);

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
			void Generate(bool matchBasePath, bool includesChildren) =>
				RunGeneration(new[] { path }, Path.Combine(dstDirectory, GetFileName(path, !matchBasePath)), matchBasePath, includesChildren);
			var pathType = GetEntryType(path, true);
			var result = new ToolStripMenuItem {
				Text = "生成日期文件",
				Image = Resource.Generate,
				DropDownItems = {
					new ToolStripMenuItem(
						pathType == EntryType.Directory ? "匹配路径（仅文件夹）" : "匹配路径",
						Resource.Aim,
						(_, _) => Generate(true, false)
					)
				}
			};
			if (pathType == EntryType.Directory)
				result.DropDownItems.Add(
					new ToolStripMenuItem(
						"匹配路径（含子结构）",
						Resource.Aim,
						(_, _) => Generate(true, true)
					)
				);
			result.DropDownItems.AddRange(
				new ToolStripItem[] {
					new ToolStripMenuItem(
						"通用（仅拖放内容）",
						Resource.Globe,
						(_, _) => Generate(false, false)
					),
					new ToolStripMenuItem(
						"通用（含子结构）",
						Resource.Globe,
						(_, _) => Generate(false, true)
					),
					Separator,
					GenerationFieldsConfigItem
				}
			);
			return result;
		}

		internal static IEnumerable<ToolStripMenuItem> CreateMultipleMenus(string[] paths) {
			string root = Path.GetDirectoryName(paths[0])!;
			void Generate(bool includesChildren) =>
				RunGeneration(paths, Path.Combine(root, GetFileName(paths)), true, includesChildren);
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
			menu.DropDownItems.Add(Separator);
			menu.DropDownItems.Add(GenerationFieldsConfigItem);
			yield return menu;
			if (paths.Length > MAX_SYNC_FILES)
				yield break;
			static ToolStripMenuItem CreateSyncMenuItem(string text, IEnumerable<string> dsts, string src, bool includesChildren, bool appliesToChildren) =>
				new(
					text,
					Resource.Sync,
					(_, _) => {
						var synchronizer = new Synchronizer(dsts, src);
						var dialog = new ProgressDialog {
							MinimizeBox = true,
							ShowCancelButton = false,
							ShowTimeRemaining = true,
							UseCompactPathsForText = true,
							UseCompactPathsForDescription = true,
							WindowTitle = "文件日期同步进度",
							ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
						};
						int total = 0, current = 0;
						synchronizer.Start += (_, _) => dialog.ReportProgress(0, "正在收集文件日期数据...", "");
						synchronizer.ApplicationStart += (_, _) => dialog.ReportProgress(0, "正在统计文件数量...", "");
						synchronizer.ApplicationReady += (_, args) => {
							total = args.Map.Count;
							dialog.ProgressBarStyle = ProgressBarStyle.ProgressBar;
						};
						synchronizer.ApplicationSetEntryDate += (_, args) => {
							++current;
							dialog.ReportProgress(
								(int)Math.Round((double)current / total),
								$"当前进度：{current} / ${total}",
								$"正在设置{args.Path}"
							);
						};
						dialog.DoWork += (_, _) => {
							var task = synchronizer.Synchronize(
								includesChildren,
								appliesToChildren,
								(EntryDateFields)Properties.Settings.Default.SyncFields
							);
							task.Wait();
						};
						dialog.Show();
					}
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
			items.Add(Separator);
			items.Add(SyncFieldsConfigItem);
			yield return menu;
		}

		private static ToolStripMenuItem CreateFieldsConfigMenuItem(string text, Func<EntryDateFields> getter, Action<EntryDateFields> setter) {
			ToolStripMenuItem CreateItem(string itemText, EntryDateFields field) {
				var item = new ToolStripMenuItem(itemText, null) {
					CheckOnClick = true,
					Checked = getter().HasFlag(field)
				};
				item.CheckedChanged += (_, _) => setter(getter() & ~field);
				return item;
			}
			return new ToolStripMenuItem(text, Resource.Configuration) {
				DropDownItems = {
					CreateItem("创建日期", EntryDateFields.Creation),
					CreateItem("修改日期", EntryDateFields.Modification),
					CreateItem("访问日期", EntryDateFields.Access)
				}
			};
		}

		private static void RunGeneration(IEnumerable<string> paths, string saveFile, bool matchBasePath, bool includesChildren) {
			var generator = new Generator(paths);
			var dialog = new ProgressDialog {
				MinimizeBox = true,
				ShowCancelButton = false,
				ShowTimeRemaining = false,
				UseCompactPathsForText = true,
				UseCompactPathsForDescription = true,
				WindowTitle = "生成日期文件进度",
				ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
			};
			generator.ReadEntryDate += (_, args) => dialog.ReportProgress(0, "正在读取文件日期...", $"文件：{args.Path}");
			generator.Complete += (_, _) => dialog.ReportProgress(100, "正在保存日期文件...", $"保存到{saveFile}");
			dialog.DoWork += (_, _) => {
				generator.Generate(matchBasePath, includesChildren, (EntryDateFields)Properties.Settings.Default.GenerationFields);
				generator.SaveToFile(saveFile);
			};
			dialog.Show();
		}
	}
}