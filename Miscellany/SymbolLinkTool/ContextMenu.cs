using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;
using PathSelector;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SymbolLinkTool.Utilities;
using SymbolLinkTool.Windows;
using TrueMogician.Extensions.Enumerable;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SymbolLinkTool;

internal static class Locale {
	private static readonly ResourceManager _localizer = new("SymbolLinkTool.Locales.ContextMenu", Assembly.GetExecutingAssembly());

	internal static string Get(string key) =>
		_localizer.GetString(key) ?? throw new ResourceReferenceKeyNotFoundException($"Resource key ${key} not found", key);
}

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
					Locale.Get("CreateHardLink"),
					Resource.Icon,
					(_, _) => Create(paths)
				)
			}
		};
		if (paths.Count == 1 && HardLink.GetFileLinkCount(paths[0]) > 1) {
			var manageItem = new ToolStripMenuItem(Locale.Get("ManageRefs"), Resource.Icon);
			string path = paths[0];
			string[] otherLinks = HardLink.GetFileSiblingHardLinks(path).Where(p => p != path).ToArray();
			var jumpToItem = otherLinks.Length == 1
				? new ToolStripMenuItem(
					string.Format(Locale.Get("OpenRef"), otherLinks[0]),
					null,
					(_, _) => ExplorerSelector.FileOrFolder(otherLinks[0])
				)
				: new ToolStripMenuItem(
					Locale.Get("OpenRefLocation"),
					null,
					otherLinks.Select(
							ToolStripItem (link) => new ToolStripMenuItem(
								link,
								null,
								(_, _) => ExplorerSelector.FileOrFolder(link)
							)
						)
						.ToArray()
				);
			manageItem.DropDownItems.Add(jumpToItem);
			string? fileName = Path.GetFileName(path);
			if (otherLinks.Select(Path.GetFileName).All(name => name == fileName))
				manageItem.DropDownItems.Add(
					Locale.Get("RenameAll"),
					null,
					(_, _) => {
						var files = new List<string>(otherLinks);
						files.Insert(0, path);
						RenameWindow.RenameHardLinks(files);
					}
				);
			manageItem.DropDownItems.Add(
				Locale.Get("RemoveAll"),
				null,
				(_, _) => Delete(otherLinks.Append(path), true)
			);
			manageItem.DropDownItems.Add(
				Locale.Get("DeleteAll"),
				null,
				(_, _) => Delete(otherLinks.Append(path), false)
			);
			menu.Items.Add(manageItem);
		}
		return menu;
	}

	private static void Create(IReadOnlyList<string> paths) {
		if (paths.Count == 1) {
			string src = paths[0];
			string? ext = Path.GetExtension(src);
			var dialog = new SaveFileDialog {
				Title = Locale.Get("ChooseNewHardLinkFile"),
				FileName = Path.GetFileName(src),
				RestoreDirectory = false,
				InitialDirectory = Path.GetDirectoryName(src),
				Filter = string.IsNullOrEmpty(ext)
					? string.Format(Locale.Get("AllFilesFilter"), "*.*")
					: string.Format($"{Locale.Get("HardLinkSaveFilter")}|${Locale.Get("AllFilesFilter")}", "*" + ext, "*.*")
			};
			var result = dialog.ShowDialog();
			if (result != DialogResult.OK)
				return;
		CreateHardLink:
			try {
				HardLink.Create(dialog.FileName, src);
			}
			catch (Exception ex) {
				if (MessageBox.Show(ex.Message, Locale.Get("HardLinkCreationFailure"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
					goto CreateHardLink;
			}
		}
		else {
			if (!paths.Unique(Path.GetFileName)) {
				MessageBox.Show(Locale.Get("DupNameErrorMsg"), Locale.Get("MsgBoxErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			var dialog = new CommonOpenFileDialog {
				IsFolderPicker = true,
				Multiselect = false,
				EnsurePathExists = true,
				AllowNonFileSystemItems = false,
				Title = Locale.Get("ChooseSaveDirForHardLinks"),
				InitialDirectory = Path.GetDirectoryName(paths[0])
			};
			if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
				return;
		CreateHardLinks:
			try {
				string root = dialog.FileName;
				foreach (string path in paths)
					HardLink.Create(Path.Combine(root, Path.GetFileName(path)), path);
			}
			catch (Exception ex) {
				if (MessageBox.Show(ex.Message, Locale.Get("HardLinkCreationFailure"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
					goto CreateHardLinks;
			}
		}
	}

	private static void Delete(IEnumerable<string> paths, bool recycleBin) =>
		paths.ForEach(
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
[COMServerAssociation(AssociationType.DirectoryBackground)]
public class FolderContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => !SelectedItemPaths.Skip(1).Any();

	protected override ContextMenuStrip CreateMenu() {
		string src = SelectedItemPaths.FirstOrDefault() ?? FolderPath;
		var strip = new ContextMenuStrip {
			Items = {
				new ToolStripMenuItem(
					Locale.Get("CreateJunctionPoint"),
					Resource.Icon,
					(_, _) => {
						var dialog = new CommonOpenFileDialog {
							IsFolderPicker = true,
							Multiselect = false,
							EnsurePathExists = false,
							EnsureFileExists = false,
							EnsureValidNames = true,
							Title = Locale.Get("ChooseEmptyDir"),
							DefaultFileName = Path.GetFileName(src)
						};
						string dst;
						while (true) {
							if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
								return;
							dst = dialog.FileName;
							DialogResult result;
							if (dst == src)
								result = MessageBox.Show(Locale.Get("SameDirErrorMsg"), Locale.Get("MsgBoxErrorCaption"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
							else if (Directory.EnumerateFileSystemEntries(dst).Any())
								result = MessageBox.Show(Locale.Get("NotEmptyDirErrorMsg"), Locale.Get("MsgBoxErrorCaption"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
							else
								break;
							if (result != DialogResult.Retry)
								return;
						}
					Create:
						try {
							JunctionPoint.Create(src, dst, true);
						}
						catch (IOException ex) {
							string message = ex.InnerException?.Message ?? ex.Message;
							if (MessageBox.Show(message, Locale.Get("JunctionPointCreationFailure"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
								goto Create;
						}
					}
				)
			}
		};
		if (JunctionPoint.GetTarget(src) is { } target)
			strip.Items.Add(
				new ToolStripMenuItem(
					Locale.Get("OpenJunctionPointTarget"),
					Resource.Icon,
					(_, _) => ExplorerSelector.FileOrFolder(target)
				)
			);
		return strip;
	}
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