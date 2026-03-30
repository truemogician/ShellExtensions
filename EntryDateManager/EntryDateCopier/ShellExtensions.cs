using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using TrueMogician.Extensions.Enumerable;
using Image = EntryDateCopier.Resources.Image;

namespace EntryDateCopier {
	using static Utilities;

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFilesAndFolders)]
	public class FileFolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() {
			var paths = SelectedItemPaths.ToArray();
			var menu = new ContextMenuStrip {
				Items = { MenuFactory.CreateApplicationMenu(paths) }
			};
			if (SelectedItemPaths.Same(Path.GetDirectoryName)) {
				if (paths.Length == 1)
					menu.Items.Add(MenuFactory.CreateGenerationSingleMenu(paths[0]));
				else
					menu.Items.AddRange(MenuFactory.CreateGenerationMultipleMenus(paths, false).OfType<ToolStripItem>().ToArray());
			}
			return menu;
		}
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class BackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() => new() {
			Items = { MenuFactory.CreateGenerationMultipleMenus(Directory.GetFileSystemEntries(FolderPath), true).Single() }
		};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, EdiFile.EXT)]
	public class DropHandler : SharpDropHandler {
		public const int SHOW_DIALOG_COUNT = 100;

		protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;

		protected override void Drop(DragEventArgs dragEventArgs)
			=> HandleException(() => MenuFactory.RunApplication(DragItems, SelectedItemPath));
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, EdiFile.EXT)]
	public class IconHandler : SharpIconHandler {
		protected override Icon GetIcon(bool smallIcon, uint iconSize) => Image.Main;
	}
}