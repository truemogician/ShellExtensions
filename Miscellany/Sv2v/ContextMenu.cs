using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Sv2v {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFiles)]
	public class ContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => SelectedItemPaths.All(path => Path.GetExtension(path) == ".sv");
		protected override ContextMenuStrip CreateMenu() {
			var menu = new ContextMenuStrip();
			menu.Items.Add(new ToolStripMenuItem("转换为Verilog", null, (sender, e) => {
				var converterPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Converter\sv2v.exe");
				var filesArgument = new StringBuilder();
				foreach (string path in SelectedItemPaths)
					filesArgument.Append($"\"{path}\" ");
				var process = Process.Start(new ProcessStartInfo() {
					FileName = converterPath,
					Arguments = $"--write=adjacent {filesArgument.ToString().Trim()}",
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardError=true
				});
				process.WaitForExit();
				string error = process.StandardError.ReadToEnd();
				if (!string.IsNullOrEmpty(error))
					MessageBox.Show(error, "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}));
			return menu;
		}
	}
}