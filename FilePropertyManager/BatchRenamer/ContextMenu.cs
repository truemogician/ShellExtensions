using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
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
						Resource.Icon,
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
						Resource.Icon,
						(_, _) => new MainWindow(FolderPath).ShowDialog()
					)
				}
			};
	}

	internal static class Resource {
		internal static Image Icon = Resize(new Bitmap(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "MainWindow.png")), 24, 24);

		private static Image Resize(Image src, int width, int height) {
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(src.HorizontalResolution, src.VerticalResolution);

			using var graphics = Graphics.FromImage(destImage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			using var wrapMode = new ImageAttributes();
			wrapMode.SetWrapMode(WrapMode.TileFlipXY);
			graphics.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);

			return destImage;
		}
	}
}