using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using TrueMogician.Extensions.Enumerable;
using Image = EntryDateCopier.Resources.Image;
using ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle;

namespace EntryDateCopier {
	using static Locale;
	using static Utilities;

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	public class FileFolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => SelectedItemPaths.Same(Path.GetDirectoryName);

		protected override ContextMenuStrip CreateMenu() {
			string[] selectedPaths = SelectedItemPaths.ToArray();
			var menu = new ContextMenuStrip();
			if (selectedPaths.Length == 1)
				menu.Items.Add(MenuFactory.CreateSingleMenu(selectedPaths[0]));
			else
				menu.Items.AddRange(MenuFactory.CreateMultipleMenus(selectedPaths).OfType<ToolStripItem>().ToArray());
			return menu;
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class BackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() => new() {
			Items = { MenuFactory.CreateBackgroundMenu(FolderPath) }
		};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".edi")]
	public class DropHandler : SharpDropHandler {
		public const int SHOW_DIALOG_COUNT = 100;

		protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;

		protected override void Drop(DragEventArgs dragEventArgs) => HandleException(() => {
			var applier = new Applier(DragItems.ToArray(), SelectedItemPath);
			applier.Ready += (_, e) => {
				int total = e.Map.Count;
				if (total < SHOW_DIALOG_COUNT)
					return;
				var dialog = new ProgressDialog {
					MinimizeBox = true,
					ShowTimeRemaining = false,
					UseCompactPathsForText = true,
					UseCompactPathsForDescription = true,
					WindowTitle = Text.GetString("DragDropProgressTitle"),
					ProgressBarStyle = ProgressBarStyle.ProgressBar
				};
				int current = 0;
				applier.SetEntryDate += (_, args) => {
					Interlocked.Increment(ref current);
					dialog.ReportProgress(
						(int)Math.Round((double)current / total),
						string.Format(Text.GetString("ProgressFormat")!, current, total),
						args.Path
					);
				};
			};
			_ = applier.ApplyAsync().ContinueWith(task => {
				if (task.IsFaulted)
					HandleException(task.Exception!.InnerException!);
			});
			/*try {
				applier.Apply();
				MessageBox.Show(Text.GetString("EdiApplied"), Text.GetString("Success"), MessageBoxButtons.OK);
			}
			catch (Exception ex) {
				HandleException(ex);
			}*/
		});
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".edi")]
	public class IconHandler : SharpIconHandler {
		protected override Icon GetIcon(bool smallIcon, uint iconSize) => Image.Main;
	}
}