using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EntryDateCopier.Properties;
using Ookii.Dialogs.Wpf;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.String;
using ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle;


namespace EntryDateCopier {
	using static Locale;
	using static Utilities;

	internal static class MenuFactory {
		internal const int MAX_SYNC_FILES = 16;

		private static ToolStripSeparator Separator => new();

		private static ToolStripMenuItem GenerationFieldsConfigItem { get; } = CreateConfigMenuItem(
			Text.GetString("ConfigGenFields")!,
			() => (EntryDateFields)Settings.Default.GenerationFields,
			fields => {
				Settings.Default.GenerationFields = (byte)fields;
				Settings.Default.Save();
			}
		);

		private static ToolStripMenuItem SyncFieldsConfigItem { get; } = CreateConfigMenuItem(
			Text.GetString("ConfigSyncFields")!,
			() => (EntryDateFields)Settings.Default.SyncFields,
			fields => {
				Settings.Default.SyncFields = (byte)fields;
				Settings.Default.Save();
			}
		);

		private static string GetFileName(IEnumerable<string> paths) => GetFileName(
			paths.SameOrDefault(Path.GetDirectoryName) ??
			throw new ArgumentException(Text.GetString("PathNotInSameDir"), nameof(paths))
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
			var format = Text.GetString("GenMenuFormat")!;
            var result = new ToolStripMenuItem {
				Text = Text.GetString("GenerateDateFile"),
				Image = Resources.Image.Generate,
				DropDownItems = {
					new ToolStripMenuItem(
						pathType == EntryType.Directory 
							? string.Format(format, Text.GetString("MatchPath"), Text.GetString("DirOnly"))
							: Text.GetString("MatchPath"),
						Resources.Image.Aim,
						(_, _) => Generate(true, false)
					)
				}
			};
			if (pathType == EntryType.Directory)
				result.DropDownItems.Add(
					new ToolStripMenuItem(
						string.Format(format, Text.GetString("MatchPath"), Text.GetString("IncludeSubStructure")),
						Resources.Image.Aim,
						(_, _) => Generate(true, true)
					)
				);
			result.DropDownItems.AddRange(
				new ToolStripItem[] {
					new ToolStripMenuItem(
						string.Format(format, Text.GetString("Universal"), Text.GetString("DraggingContentOnly")),
						Resources.Image.Globe,
						(_, _) => Generate(false, false)
					),
					new ToolStripMenuItem(
						string.Format(format, Text.GetString("Universal"), Text.GetString("IncludeSubStructure")),
						Resources.Image.Globe,
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
			bool hasDirectory = paths.Any(Directory.Exists);
			var format = Text.GetString("GenMenuFormat")!;
            var menu = new ToolStripMenuItem(Text.GetString("GenerateDateFile"), Resources.Image.Generate);
			menu.DropDownItems.Add(
				new ToolStripMenuItem(
					hasDirectory ? 
						string.Format(format, Text.GetString("MatchPath"), Text.GetString("DirOnly"))
						: Text.GetString("MatchPath"),
					Resources.Image.Aim,
					(_, _) => RunGeneration(paths, Path.Combine(root, GetFileName(paths)), true, false)
                )
			);
			if (hasDirectory)
				menu.DropDownItems.Add(
					new ToolStripMenuItem(
						string.Format(format, Text.GetString("MatchPath"), Text.GetString("IncludeSubStructure")),
						Resources.Image.Aim,
						(_, _) => RunGeneration(paths, Path.Combine(root, GetFileName(paths)), true, true)
                    )
				);
			menu.DropDownItems.Add(Separator);
			menu.DropDownItems.Add(GenerationFieldsConfigItem);
			yield return menu;
			if (isBackground || paths.Count > MAX_SYNC_FILES)
				yield break;
			menu = new ToolStripMenuItem(Text.GetString("SyncEntryDates"), Resources.Image.Sync);
			var items = menu.DropDownItems;
			var entryTypes = paths.ToDictionaryWith(p => GetEntryType(p, true));
			int folderCount = entryTypes.Values.Count(t => t == EntryType.Directory);
			foreach (string path in paths) {
				var dsts = paths.ToList();
				dsts.Remove(path);
				var text = string.Format(Text.GetString("SyncToFormat")!, Path.GetFileName(path));
                items.Add(entryTypes[path] == EntryType.File || folderCount <= 1
					? CreateSyncMenuItem(text, dsts, path, false, false)
					: new ToolStripMenuItem(text, Resources.Image.Sync) {
						DropDownItems = {
							CreateSyncMenuItem(Text.GetString("SelectedOnly")!, dsts, path, false, false),
							CreateSyncMenuItem(Text.GetString("IncludeSubStructure")!, dsts, path, true, true)
						}
					}
				);
			}
			items.Add(Separator);
			items.Add(SyncFieldsConfigItem);
			yield return menu;
		}

		private static ToolStripMenuItem CreateConfigMenuItem(string text, Func<EntryDateFields> getter, Action<EntryDateFields> setter) {
			ToolStripMenuItem CreateFieldItem(string? itemText, EntryDateFields field) {
				var item = new ToolStripMenuItem(itemText, null) {
					CheckOnClick = true,
					Checked = getter().HasFlag(field)
				};
				item.CheckedChanged += (_, _) => setter(getter() ^ field);
				return item;
			}
            var directoryOnlyItem = new ToolStripMenuItem(Text.GetString("DirOnly").Capitalize(true), null) {
                CheckOnClick = true,
                Checked = Settings.Default.DirectoryOnly
            };
            directoryOnlyItem.CheckedChanged +=
                (_, _) => Settings.Default.DirectoryOnly = !Settings.Default.DirectoryOnly;
			return new ToolStripMenuItem(text, Resources.Image.Configuration) {
				DropDownItems = {
					CreateFieldItem(Text.GetString("CreationDate"), EntryDateFields.Creation),
					CreateFieldItem(Text.GetString("ModificationDate"), EntryDateFields.Modification),
					CreateFieldItem(Text.GetString("AccessDate"), EntryDateFields.Access),
					Separator,
					directoryOnlyItem
				}
			};
		}

		private static ToolStripMenuItem CreateSyncMenuItem(string text, IEnumerable<string> dsts, string src, bool includesChildren, bool appliesToChildren) =>
			new(
				text,
				Resources.Image.Sync,
				(_, _) => HandleException(
					() => {
						var synchronizer = new Synchronizer(dsts, src);
						var dialog = new ProgressDialog {
							MinimizeBox = true,
							ShowCancelButton = false,
							ShowTimeRemaining = true,
							UseCompactPathsForText = true,
							UseCompactPathsForDescription = true,
							WindowTitle =Text.GetString("SyncProgressTitle"),
							ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
						};
						int total = 0, current = 0;
						var source = new CancellationTokenSource();
						synchronizer.Start += (_, _) => dialog.ReportProgress(0, Text.GetString("CollectingEntryDateData"), null);
						synchronizer.ApplicationStart += (_, _) => dialog.ReportProgress(0, Text.GetString("CountingEntries"), null);
						synchronizer.ApplicationReady += (_, args) => {
							total = args.Map.Count;
							dialog.ProgressBarStyle = ProgressBarStyle.ProgressBar;
						};
						synchronizer.ApplicationSetEntryDate += (_, args) => {
							++current;
							dialog.ReportProgress(
								(int)Math.Round((double)current / total),
								string.Format(Text.GetString("ProgressFormat")!, current, total),
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
				WindowTitle = Text.GetString("GenerateDateFile"),
				ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar
			};
			generator.ReadEntryDate += (_, args) => dialog.ReportProgress(0, Text.GetString("ReadingEntryDates"), args.Path);
			generator.Complete += (_, _) => dialog.ReportProgress(100, Text.GetString("SavingDateFile"), saveFile);
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