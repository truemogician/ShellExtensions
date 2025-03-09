using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace QuickActionExtension;

[ComVisible(true)]
[COMServerAssociation(AssociationType.AllFilesAndFolders)]
public class FilesAndFoldersQuickActionExtension : SharpContextMenu {
	protected override bool CanShowMenu() => true;

	protected override ContextMenuStrip CreateMenu() {
		string[] selectedPaths = SelectedItemPaths.ToArray();
		var menu = new ContextMenuStrip();
		var copyPath = new ToolStripMenuItem {
			Text = "复制路径",
			Visible = selectedPaths.Length == 1
		};
		copyPath.Click += (sender, e) => Clipboard.SetText(selectedPaths[0]);
		var runCommand = new ToolStripMenuItem {
			Text = "在此处运行命令行"
		};

		var runCommandAsAdmin = new ToolStripMenuItem {
			Text = "在此处运行命令行（以管理员身份）"
		};

		menu.Items.Add(
			new ToolStripMenuItem {
				Text = "快速操作",
				DropDownItems = {
					copyPath,
					runCommand,
					runCommandAsAdmin
				}
			}
		);
		return menu;
	}
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.DirectoryBackground)]
public class DirectoryBackgroundQuickActionExtension : SharpContextMenu {
	protected string RestartExplorerPath = @"D:\Program Files\Shell Extensions\Context Menu\QuickActionExtension\RestartExplorer.exe";

	protected override bool CanShowMenu() => true;

	protected override ContextMenuStrip CreateMenu() {
		var menu = new ContextMenuStrip();
		var restartExplorer = new ToolStripMenuItem {
			Text = "重启资源管理器"
		};
		restartExplorer.Click += (sender, e) => Process.Start(RestartExplorerPath);

		menu.Items.Add(
			new ToolStripMenuItem {
				Text = "快速操作",
				DropDownItems = {
					restartExplorer
				}
			}
		);
		return menu;
	}
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.DesktopBackground)]
public class DesktopBackgroundActionExtension : SharpContextMenu {
	protected string RestartExplorerPath = @"D:\Program Files\Shell Extensions\Context Menu\QuickActionExtension\RestartExplorer.exe";

	protected override bool CanShowMenu() => true;

	protected override ContextMenuStrip CreateMenu() {
		var menu = new ContextMenuStrip();
		var restartExplorer = new ToolStripMenuItem {
			Text = "重启资源管理器"
		};
		restartExplorer.Click += (sender, e) => Process.Start(RestartExplorerPath);

		menu.Items.Add(
			new ToolStripMenuItem {
				Text = "快速操作",
				DropDownItems = {
					restartExplorer
				}
			}
		);
		return menu;
	}
}