using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BatchRenamer.Windows;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace BatchRenamer {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	public class FilesAndFoldersMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu()
			=> new() {
				Items = {
					new ToolStripMenuItem(
						"批量重命名",
						null,
						(_, _) => new MainWindow(SelectedItemPaths).ShowDialog()
					)
				}
			};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class DirectoryBackgroundMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu()
			=> new() {
				Items = {
					new ToolStripMenuItem(
						"批量重命名",
						null,
						(_, _) => new MainWindow(Directory.GetFileSystemEntries(FolderPath)).ShowDialog()
					)
				}
			};
	}
}