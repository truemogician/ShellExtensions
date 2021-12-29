using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace NcmConverter {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFiles)]
	public class ContextMenu : SharpContextMenu {
		private const string ConverterPath = @"D:\Program Files\NcmDump\ncmdump.exe";

		protected override bool CanShowMenu() => SelectedItemPaths.All(path => Path.GetExtension(path) == ".ncm");

		protected override ContextMenuStrip CreateMenu() => new() {
			Items = {
				new ToolStripMenuItem(
					"转换为mp3",
					null,
					(_, _) => Process.Start(
						new ProcessStartInfo(ConverterPath, string.Join(" ", SelectedItemPaths.Select(p => $"\"{p}\""))) {
							UseShellExecute = false,
							CreateNoWindow = true
						}
					)
				)
			}
		};
	}
}