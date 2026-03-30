using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ShellExtensionManager {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.DirectoryBackground)]
	public class DirectoryBackgroundContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => true;

		protected override ContextMenuStrip CreateMenu() => throw new NotImplementedException();
	}
}
