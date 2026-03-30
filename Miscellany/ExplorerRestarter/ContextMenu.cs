using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
						var exe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "RestartExplorer.exe");
						Process.Start(new ProcessStartInfo {
							FileName = exe,
							Verb = "runas"
						});
					}
					catch (Exception ex) {
						MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			)
		}
	};
}