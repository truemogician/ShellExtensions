using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace FolderWrapper;

[ComVisible(true)]
[COMServerAssociation(AssociationType.AllFiles)]
public class FilesContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => true;

	protected override ContextMenuStrip CreateMenu() => new() {
		Items = {
			new ToolStripMenuItem(
				Locale.ContextMenu.GetString("WrapWithSameNameFolder"),
				Resource.Wrap,
				(_, _) => {
					try {
						FolderWrapper.Wrap(SelectedItemPaths);
					}
					catch (FolderWrapperException ex) {
						MessageBox.Show(ex.Message, Locale.ContextMenu.GetString("MsgBoxErrCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			)
		}
	};
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.Folder)]
public class FoldersContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => SelectedItemPaths.All(
		p => FolderWrapper.IsDirectory(p) && Directory.EnumerateFileSystemEntries(p).Any()
	);

	protected override ContextMenuStrip CreateMenu() {
		string[] paths = SelectedItemPaths.ToArray();
		var unwrapMenuItem = new ToolStripMenuItem(
			Locale.ContextMenu.GetString("UnwrapFolder"),
			Resource.Unwrap,
			(_, _) => {
				try {
					FolderWrapper.Unwrap(paths, false);
				}
				catch (FolderWrapperException ex) {
					MessageBox.Show(ex.Message, Locale.ContextMenu.GetString("MsgBoxErrCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		);
		return new ContextMenuStrip {
			Items = {
				paths.Any(
					p => {
						using var e = Directory.EnumerateFileSystemEntries(p).GetEnumerator();
						if (!e.MoveNext())
							return false;
						string target = e.Current!;
						if (e.MoveNext() || !FolderWrapper.IsDirectory(target))
							return false;
						return Directory.EnumerateFileSystemEntries(target).Any();
					}
				)
					? new ToolStripMenuItem(Locale.ContextMenu.GetString("FolderOperations"), Resource.Folder) {
						DropDownItems = {
							unwrapMenuItem,
							new ToolStripMenuItem(
								Locale.ContextMenu.GetString("UnwrapFolderDeep"),
								Resource.Unwrap,
								(_, _) => {
									try {
										FolderWrapper.Unwrap(paths, true);
									}
									catch (FolderWrapperException ex) {
										MessageBox.Show(ex.Message, Locale.ContextMenu.GetString("MsgBoxErrCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
									}
								}
							)
						}
					}
					: unwrapMenuItem
			}
		};
	}
}

internal static class Resource {
	internal static string Root { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Assets");

	internal static Image Folder { get; } = Resize(new Bitmap(Path.Combine(Root, "Folder.png")), 24, 24);

	internal static Image Wrap { get; } = Resize(new Bitmap(Path.Combine(Root, "Wrap.png")), 24, 24);

	internal static Image Unwrap { get; } = Resize(new Bitmap(Path.Combine(Root, "Unwrap.png")), 24, 24);

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