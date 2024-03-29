﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EntryDateCopier.Properties;
using Ookii.Dialogs.Wpf;
using TrueMogician.Extensions.Enumerable;
using ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle;
using static EntryDateCopier.Utilities;

#nullable enable

namespace EntryDateCopier {
	internal static class MenuFactory {
		internal const int MAX_SYNC_FILES = 16;

		private static ToolStripSeparator Separator => new();

		private static ToolStripMenuItem GenerationFieldsConfigItem { get; } = CreateConfigMenuItem(
			"设置生成的文件日期字段",
			() => (EntryDateFields)Settings.Default.GenerationFields,
			fields => {
				Settings.Default.GenerationFields = (byte)fields;
				Settings.Default.Save();
			}
		);

		private static ToolStripMenuItem SyncFieldsConfigItem { get; } = CreateConfigMenuItem(
			"设置同步的文件日期字段",
			() => (EntryDateFields)Settings.Default.SyncFields,
			fields => {
				Settings.Default.SyncFields = (byte)fields;
				Settings.Default.Save();
			}
		);

		private static string GetFileName(IEnumerable<string> paths) => GetFileName(
			paths.SameOrDefault(Path.GetDirectoryName) ??
			throw new ArgumentException("Paths not in the same directory", nameof(paths))
		);

		private static string GetFileName(string path) {
			const string suffix = ".edi";
			string dir = Path.GetDirectoryName(path) ?? "";
			string name = dir == ""
				? DriveInfo.GetDrives().First(d => d.Name == path).VolumeLabel
				: Path.GetFileName(path);
			string fileName = name;
			var index = 1;
			while (File.Exists(Path.Combine(dir, fileName + suffix)))
				fileName = name + $"({++index})";
			return fileName + suffix;
		}

		internal static ToolStripMenuItem CreateSingleMenu(string path) {
			string dstDirectory = Path.GetDirectoryName(path)!;
			void Generate(bool matchBasePath, bool includesChildren) => HandleException(
				() => RunGeneration(
					new[] { path },
					Path.Combine(dstDirectory, GetFileName(path)),
					matchBasePath,
					includesChildren
				)
			);
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

		internal static IEnumerable<ToolStripMenuItem> CreateMultipleMenus(string[] paths) =>
			CreateMultipleMenus(paths, false);

		internal static ToolStripMenuItem CreateBackgroundMenu(string directory) =>
			CreateMultipleMenus(Directory.GetFileSystemEntries(directory), true).Single();

		private static IEnumerable<ToolStripMenuItem> CreateMultipleMenus(IReadOnlyList<string> paths, bool isBackground) {
			string root = Path.GetDirectoryName(paths[0])!;
			void Generate(bool includesChildren) => HandleException(
				() => RunGeneration(paths, Path.Combine(root, GetFileName(paths)), true, includesChildren)
			);
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
			if (isBackground || paths.Count > MAX_SYNC_FILES)
				yield break;
			menu = new ToolStripMenuItem("同步文件日期", Resource.Sync);
			var items = menu.DropDownItems;
			var entryTypes = paths.ToDictionaryWith(p => GetEntryType(p, true));
			int folderCount = entryTypes.Values.Count(t => t == EntryType.Directory);
			foreach (string path in paths) {
				var dsts = paths.ToList();
				dsts.Remove(path);
				items.Add(
					entryTypes[path] == EntryType.File || folderCount <= 1
						? CreateSyncMenuItem($"同步到{Path.GetFileName(path)}", dsts, path, false, false)
						: new ToolStripMenuItem($"同步到{Path.GetFileName(path)}", Resource.Sync) {
							DropDownItems = {
								CreateSyncMenuItem("仅所选内容", dsts, path, false, false),
								CreateSyncMenuItem("包含子结构", dsts, path, true, true)
							}
						}
				);
			}
			items.Add(Separator);
			items.Add(SyncFieldsConfigItem);
			yield return menu;
		}

		private static ToolStripMenuItem CreateConfigMenuItem(string text, Func<EntryDateFields> getter, Action<EntryDateFields> setter) {
			ToolStripMenuItem CreateFieldItem(string itemText, EntryDateFields field) {
				var item = new ToolStripMenuItem(itemText, null) {
					CheckOnClick = true,
					Checked = getter().HasFlag(field)
				};
				item.CheckedChanged += (_, _) => setter(getter() ^ field);
				return item;
			}
            var directoryOnlyItem = new ToolStripMenuItem("仅文件夹", null) {
                CheckOnClick = true,
                Checked = Settings.Default.DirectoryOnly
            };
            directoryOnlyItem.CheckedChanged +=
                (_, _) => Settings.Default.DirectoryOnly = !Settings.Default.DirectoryOnly;
			return new ToolStripMenuItem(text, Resource.Configuration) {
				DropDownItems = {
					CreateFieldItem("创建日期", EntryDateFields.Creation),
					CreateFieldItem("修改日期", EntryDateFields.Modification),
					CreateFieldItem("访问日期", EntryDateFields.Access),
					Separator,
					directoryOnlyItem
				}
			};
		}

		private static ToolStripMenuItem CreateSyncMenuItem(string text, IEnumerable<string> dsts, string src, bool includesChildren, bool appliesToChildren) =>
			new(
				text,
				Resource.Sync,
				(_, _) => HandleException(
					() => {
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
						var source = new CancellationTokenSource();
						synchronizer.Start += (_, _) => dialog.ReportProgress(0, "正在收集文件日期数据...", null);
						synchronizer.ApplicationStart += (_, _) => dialog.ReportProgress(0, "正在统计文件数量...", null);
						synchronizer.ApplicationReady += (_, args) => {
							total = args.Map.Count;
							dialog.ProgressBarStyle = ProgressBarStyle.ProgressBar;
						};
						synchronizer.ApplicationSetEntryDate += (_, args) => {
							++current;
							dialog.ReportProgress(
								(int)Math.Round((double)current / total),
								$"当前进度：{current} / ${total}",
								args.Path
							);
						};
						dialog.DoWork += (_, args) => HandleException(
							() => {
								CreateCancellationTimer(args, source, dialog, 500).Start();
								synchronizer.Synchronize(
										includesChildren,
										appliesToChildren,
										(EntryDateFields)Settings.Default.SyncFields,
										source.Token
									)
									.Wait(source.Token);
							},
							dialog
						);
						dialog.Show(source.Token);
					}
				)
			);

		private static void RunGeneration(IEnumerable<string> paths, string saveFile, bool matchBasePath, bool includesChildren) {
			var generator = new Generator(paths);
			var dialog = new ProgressDialog {
				MinimizeBox = true,
				ShowTimeRemaining = false,
				UseCompactPathsForText = true,
				UseCompactPathsForDescription = true,
				WindowTitle = "生成日期文件",
				ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
			};
			generator.ReadEntryDate += (_, args) => dialog.ReportProgress(0, "正在读取文件日期...", args.Path);
			generator.Complete += (_, _) => dialog.ReportProgress(100, "正在保存日期文件...", saveFile);
			var source = new CancellationTokenSource();
			dialog.DoWork += (_, args) => HandleException(
				() => {
					CreateCancellationTimer(args, source, dialog, 500).Start();
					generator.Generate(
							matchBasePath,
							includesChildren,
							Settings.Default.DirectoryOnly,
							(EntryDateFields)Settings.Default.GenerationFields,
							source.Token
						)
						.ContinueWith(
							task => {
								if (task.IsFaulted)
									throw task.Exception!;
								generator.SaveToFile(saveFile);
							},
							source.Token
						)
						.Wait(source.Token);
				},
				dialog
			);
			dialog.Show(source.Token);
		}
	}
}