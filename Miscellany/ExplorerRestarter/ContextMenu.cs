using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ExplorerRestarter;

[ComVisible(true)]
[COMServerAssociation(AssociationType.DesktopBackground)]
[COMServerAssociation(AssociationType.DirectoryBackground)]
public class ContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => true;

	protected override ContextMenuStrip CreateMenu() => new() {
		Items = {
			new ToolStripMenuItem(
				"重启文件资源管理器",
				Resource.Restart,
				(_, _) => {
					try {
						Explorer.Restart();
					}
					catch (Exception ex) {
						MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			)
		}
	};
}