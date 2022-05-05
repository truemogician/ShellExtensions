using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SymbolLinkTool.Utilities;
using TrueMogician.Extensions.Enumerable;

namespace SymbolLinkTool {
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFiles)]
	public class FileContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() =>
			SelectedItemPaths.All(p => !File.GetAttributes(p).HasFlag(FileAttributes.Directory));

		protected override ContextMenuStrip CreateMenu() =>
			new() {
				Items = {
					new ToolStripMenuItem(
						"创建Hard Link",
						Resource.Icon,
						(_, _) => {
							var paths = SelectedItemPaths.ToList();
							if (paths.Count == 1) {
								string src = paths[0];
								var dialog = new SaveFileDialog {
									Title = "选择要创建的Hard Link文件",
									FileName = Path.GetFileName(src)
								};
								var result = dialog.ShowDialog();
								if (result != DialogResult.OK)
									return;
								while (true) {
									try {
										HardLink.Create(dialog.FileName, src);
									}
									catch (Exception ex) {
										if (MessageBox.Show(ex.Message, "Hard Link创建失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
											continue;
									}
									break;
								}
							}
							else {
								if (!paths.Unique(Path.GetFileName)) {
									MessageBox.Show("所选文件中存在重名", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
									return;
								}
								var dialog = new CommonOpenFileDialog {
									IsFolderPicker = true,
									Multiselect = false,
									EnsurePathExists = true,
									AllowNonFileSystemItems = false,
									Title = "选择Hard Link文件的保存目录",
									InitialDirectory = Path.GetDirectoryName(paths[0])
								};
								if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
									return;
								while (true) {
									try {
										string root = dialog.FileName;
										foreach (string path in paths)
											HardLink.Create(Path.Combine(root, Path.GetFileName(path)), path);
									}
									catch (Exception ex) {
										if (MessageBox.Show(ex.Message, "Hard Link创建失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
											continue;
									}
									break;
								}
							}
						}
					)
				}
			};
	}

	[ComVisible(true)]
	[COMServerAssociation(AssociationType.Folder)]
	public class FolderContextMenu : SharpContextMenu {
		protected override bool CanShowMenu() => !SelectedItemPaths.Skip(1).Any();

		protected override ContextMenuStrip CreateMenu() =>
			new() {
				Items = {
					new ToolStripMenuItem(
						"创建Junction Point",
						null,
						(_, _) => {
							string src = SelectedItemPaths.First();
							var dialog = new CommonOpenFileDialog {
								IsFolderPicker = true,
								Multiselect = false,
								EnsureFileExists = false,
								EnsurePathExists = false,
								Title = "选择要创建的Junction Point的空文件夹",
								DefaultFileName = Path.GetFileName(src),
								AllowNonFileSystemItems = false
							};
							string dst;
							while (true) {
								if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
									return;
								dst = dialog.FileName;
								DialogResult result;
								if (dst == src)
									result = MessageBox.Show("目标文件夹不可与源文件夹相同", "错误", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								else if (Directory.EnumerateFileSystemEntries(dst).Any())
									result = MessageBox.Show("目标文件夹非空", "错误", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								else
									break;
								if (result != DialogResult.Retry)
									return;
							}
							while (true) {
								try {
									JunctionPoint.Create(src, dst, true);
								}
								catch (IOException ex) {
									string message = ex.InnerException?.Message ?? ex.Message;
									if (MessageBox.Show(message, "Junction Point创建失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
										continue;
								}
								break;
							}
						}
					)
				}
			};
	}

	internal static class Resource {
		internal static Image Icon = Resize(new Bitmap(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Icon.png")), 24, 24);

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
