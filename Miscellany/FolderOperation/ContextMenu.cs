using System;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FolderOperation;

internal static class ExceptionHandler {
	internal static void Handle(Exception ex) {
		var caption = ex is FolderOperationException
			? Locale.ContextMenu.GetString("ExpectedErrCaption")
			: Locale.ContextMenu.GetString("UnexpectedErrCaption");
		MessageBox.Show(ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

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
						new FolderOperation(SelectedItemPaths).Wrap();
					}
					catch (Exception ex) {
						ExceptionHandler.Handle(ex);
					}
				}
			)
		}
	};
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.Folder)]
public class FoldersContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => SelectedItemPaths.All(FolderOperation.IsDirectory);

	protected override ContextMenuStrip CreateMenu() {
		var op = new FolderOperation(SelectedItemPaths);
		op.Check();

		var menus = new List<ToolStripItem>();

		bool hasNonEmptyDir = op.Flags.Values.Any(f => f != FolderFlag.Empty);
		bool hasEmptyDir = op.Flags.Values.Any(f => f == FolderFlag.Empty);
        if (hasNonEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapFolder"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.Unwrap)
			));
		}
		if (hasEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("DeleteEmptyFolder"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.DeleteEmpty)
			));
		}
		if (hasNonEmptyDir && hasEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapAndDeleteEmptyFolder"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.UnwrapAndDeleteEmpty)
			));
        }

		bool hasDeepNonEmptyDir = op.Folders.Any(f => op.Flags.TryGetValue(f, out var flag) && flag.HasFlag(FolderFlag.HasOneDirectory) && !flag.HasFlag(FolderFlag.Empty));
		bool hasDeepEmptyDir = op.Folders.Any(f => op.Flags[f] == (FolderFlag.HasOneDirectory | FolderFlag.Empty));
        if (hasDeepNonEmptyDir || hasDeepEmptyDir)
			menus.Add(new ToolStripSeparator());
		if (hasDeepNonEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapFolderDeep"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.Unwrap | UnwrapOption.Deep)
			));
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapFolderDeepMultiple"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.Unwrap | UnwrapOption.Deep | UnwrapOption.UnwrapMultipleDeep)
			));
        }
		if (hasDeepEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("DeleteEmptyFolderDeep"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.DeleteEmpty | UnwrapOption.Deep)
			));
        }
		if (hasDeepNonEmptyDir && hasDeepEmptyDir) {
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapAndDeleteEmptyFolderDeep"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.UnwrapAndDeleteEmpty | UnwrapOption.Deep)
			));
			menus.Add(new ToolStripMenuItem(
				Locale.ContextMenu.GetString("UnwrapAndDeleteEmptyFolderDeepMultiple"),
				Resource.Unwrap,
				CreateUnwrapHandler(UnwrapOption.UnwrapAndDeleteEmpty | UnwrapOption.Deep | UnwrapOption.UnwrapMultipleDeep)
			));
        }

		var contextMenu = new ContextMenuStrip();
		if (menus.Count == 1)
			contextMenu.Items.Add(menus[0]);
		else {
			var item = new ToolStripMenuItem(Locale.ContextMenu.GetString("FolderOperations"), Resource.Folder);
			item.DropDownItems.AddRange(menus.ToArray());
			contextMenu.Items.Add(item);
        }
		return contextMenu;

		EventHandler CreateUnwrapHandler(UnwrapOption option) {
			return (_, _) => {
				try {
					op.Unwrap(option);
				}
				catch (Exception ex) {
					ExceptionHandler.Handle(ex);
				}
			};
		}
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