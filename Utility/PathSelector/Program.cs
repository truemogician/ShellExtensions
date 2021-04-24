using System.IO;
using System.Windows.Forms;

namespace PathSelector {

	public class Program {
		public static void Main(string[] args) {
			if (args == null || args.Length == 0)
				return;
			foreach (string arg in args)
				if (!File.Exists(arg) && !Directory.Exists(arg)) {
					MessageBox.Show($"Path {arg} doesn't exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			ExplorerSelector.FilesOrFolders(args);
		}
	}
}