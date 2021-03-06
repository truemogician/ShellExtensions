using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;
using PathSelector;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SymbolLinkTool.Utilities;
using TrueMogician.Extensions.Enumerable;

namespace SymbolLinkTool; 

[ComVisible(true)]
[COMServerAssociation(AssociationType.AllFiles)]
public class FileContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() =>
		SelectedItemPaths.All(p => !File.GetAttributes(p).HasFlag(FileAttributes.Directory));

	protected override ContextMenuStrip CreateMenu() {
		var paths = SelectedItemPaths.ToList();
		var menu = new ContextMenuStrip {
			Items = {
				new ToolStripMenuItem(
					"创建硬链接",
					Resource.Icon,
					(_, _) => Create(paths)
				)
			}
		};
		if (paths.Count == 1 && HardLink.GetFileLinkCount(paths[0]) > 1) {
			string path = paths[0];
			string[] otherLinks = HardLink.GetFileSiblingHardLinks(path).Where(p => p != path).ToArray();
			var jumpToItem = new ToolStripMenuItem("打开引用所在位置");
			jumpToItem.DropDownItems.AddRange(
				otherLinks.Select(
						link => new ToolStripMenuItem(
							link,
							null,
							(_, _) => ExplorerSelector.FileOrFolder(link)
						) as ToolStripItem
					)
					.ToArray()
			);
			menu.Items.Add(
				new ToolStripMenuItem(
					"管理硬链接引用",
					Resource.Icon
				) {
					DropDownItems = {
						jumpToItem,
						new ToolStripMenuItem(
							"全部移入回收站",
							null,
							(_, _) => Delete(otherLinks.Append(path), true)
						),
						new ToolStripMenuItem(
							"全部删除",
							null,
							(_, _) => Delete(otherLinks.Append(path), false)
						)
					}
				}
			);
		}
		return menu;
	}

	private static void Create(IReadOnlyList<string> paths) {
		if (paths.Count == 1) {
			string src = paths[0];
			var dialog = new SaveFileDialog {
				Title = "选择要创建的硬链接文件",
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
					if (MessageBox.Show(ex.Message, "硬链接创建失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
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
				Title = "选择硬链接文件的保存目录",
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
					if (MessageBox.Show(ex.Message, "硬链接创建失败", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
						continue;
				}
				break;
			}
		}
	}

	private static void Delete(
		IEnumerable<string> paths,
		bool recycleBin
	)
		=> paths.ForEach(
			path => FileSystem.DeleteFile(
				path,
				UIOption.OnlyErrorDialogs,
				recycleBin
					? RecycleOption.SendToRecycleBin
					: RecycleOption.DeletePermanently
			)
		);
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