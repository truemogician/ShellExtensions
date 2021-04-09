using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DllRegistrator {

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".dll")]
	public class DllRegistrator : SharpContextMenu {
		protected override bool CanShowMenu() => SelectedItemPaths.Count() == 1;

		protected override ContextMenuStrip CreateMenu() {
			var menu = new ContextMenuStrip();
			var item = new ToolStripMenuItem("注册类库");
			if (Environment.Is64BitOperatingSystem) {
				item.DropDownItems.Add(new ToolStripMenuItem("注册", null, (sender, e) => Register("r", "x64")));
				item.DropDownItems.Add(new ToolStripMenuItem("注销", null, (sender, e) => Register("u", "x64")));
				item.DropDownItems.Add(new ToolStripMenuItem("注册到32位子系统", null, (sender, e) => Register("r", "x86")));
				item.DropDownItems.Add(new ToolStripMenuItem("从32位子系统注销", null, (sender, e) => Register("u", "x86")));
			}
			else {
				item.DropDownItems.Add(new ToolStripMenuItem("注册", null, (sender, e) => Register("r", "x86")));
				item.DropDownItems.Add(new ToolStripMenuItem("注销", null, (sender, e) => Register("u", "x86")));
			}
			menu.Items.Add(item);
			return menu;
		}

		protected void Register(string action, string targetArch) {
			string registrationHandler = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RegistrationHandler.exe");
			Process.Start(new ProcessStartInfo {
				FileName = registrationHandler,
				Arguments = $"{action} \"{SelectedItemPaths.First()}\" {targetArch}",
				Verb = "runas"
			});
		}
	}
}