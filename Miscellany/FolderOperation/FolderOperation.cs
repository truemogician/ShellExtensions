using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrueMogician.Extensions.Enumerable;

namespace FolderOperation;

public class FolderOperation(IEnumerable<string> folders) {
	public string[] Folders { get; } = folders.Select(Path.GetFullPath).ToArray();

	internal Dictionary<string, FolderFlag> Flags { get; } = new();

	internal static bool IsDirectory(string path) {
		var info = new DirectoryInfo(path);
		return info.Exists &&
			info.Attributes.HasFlag(FileAttributes.Directory) &&
			!string.Equals(info.FullName, info.Root.FullName, StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsParentDirectory(string path, string dir) {
		if (path.Length <= dir.Length)
			return false;
		if (dir[^1] == Path.DirectorySeparatorChar || dir[^1] == Path.AltDirectorySeparatorChar)
			dir = dir[..^1];
		return path.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
	}

	public void Wrap() {
		var wrapperFolders = Folders.ToDictionaryWith(
			p => Path.Combine(
				Path.GetDirectoryName(p)!,
				Path.GetFileNameWithoutExtension(p)
			)
		);
		if (!wrapperFolders.Values.Unique())
			throw new FolderOperationException(Locale.FolderOperation.GetString("DupFileNameErr"));
		if (wrapperFolders.Values.FirstOrDefault(Directory.Exists) is { } existing)
			throw new FolderOperationException(string.Format(Locale.FolderOperation.GetString("FolderExistErr")!, existing));
		foreach (var pair in wrapperFolders) {
			Directory.CreateDirectory(pair.Value);
			Directory.Move(pair.Key, Path.Combine(pair.Value, Path.GetFileName(pair.Key)));
		}
	}

	public void Unwrap(UnwrapOption option = UnwrapOption.Unwrap) {
		if (Flags.Count < Folders.Length)
			Check();
		var deletion = new HashSet<string>();
		var rename = new Dictionary<string, string>();
		var allFolders = Flags.Keys.ToArray();
		Array.Sort(allFolders);
		var deepOpt = option.HasFlag(UnwrapOption.Deep);
		var unwrapOpt = option.HasFlag(UnwrapOption.Unwrap);

		foreach (var folder in Folders) {
			var flag = Flags[folder];
			var dstFolder = Path.GetDirectoryName(folder)!;
			switch (flag) {
				case FolderFlag.Empty when option.HasFlag(UnwrapOption.DeleteEmpty):
				case FolderFlag.Empty | FolderFlag.HasOneDirectory when option.HasFlag(UnwrapOption.DeleteEmpty | UnwrapOption.Deep): deletion.Add(folder); break;
				case FolderFlag.HasOneFile when unwrapOpt:
				case FolderFlag.HasMultipleEntries when unwrapOpt: {
					foreach (var entry in Directory.EnumerateFileSystemEntries(folder)) {
						var filename = Path.GetFileName(entry);
						SetRename(entry, Path.Combine(dstFolder, filename));
					}
					deletion.Add(folder);// Delete the empty folder after moving files
					break;
				}
				default: {
					if (!unwrapOpt || !flag.HasFlag(FolderFlag.HasOneDirectory))
						break;
					string[]? targets = null;
					if (!deepOpt)
						targets = [Directory.EnumerateFileSystemEntries(folder).Single()];
					else {
						var idx = Array.BinarySearch(allFolders, folder) + 1;
						while (idx < allFolders.Length && allFolders[idx].StartsWith(folder + Path.DirectorySeparatorChar))
							++idx;
						var targetFolder = allFolders[idx - 1];
						if (flag.HasFlag(FolderFlag.HasOneFile))
							targets = [Directory.EnumerateFileSystemEntries(targetFolder).Single()];
						else if (flag.HasFlag(FolderFlag.HasMultipleEntries)) {
							targets = option.HasFlag(UnwrapOption.UnwrapMultipleDeep)
								? Directory.EnumerateFileSystemEntries(targetFolder).ToArray()
								: [targetFolder];
						}
					}
					if (targets is not null) {
						foreach (var target in targets)
							SetRename(target, Path.Combine(dstFolder, Path.GetFileName(target)));
						deletion.Add(folder);
					}
					break;
				}
			}
		}
		// Remove duplicate entries in deletion list
		var deletionList = deletion.ToList();
		deletionList.Sort();
		for (var i = 1; i < deletionList.Count; ++i) {
			var prev = deletionList[i - 1];
			while (IsParentDirectory(deletionList[i], prev))
				deletion.Remove(deletionList[i++]);
		}
		// Ensure no conflicts in rename list
		var renameReverse = new Dictionary<string, string>();
		foreach (var pair in rename) {
			var dst = pair.Value;
			if (renameReverse.TryGetValue(dst, out var existing)) {
				var template = Locale.FolderOperation.GetString("SameNameFilesInFoldersErr")!;
				throw new FolderOperationException(string.Format(template, existing, pair.Key));
			}
			var isDir = IsDirectory(dst);
			if ((isDir && !deletion.Contains(dst) && Directory.Exists(dst)) || (!isDir && File.Exists(dst)))
				throw new FolderOperationException(string.Format(Locale.FolderOperation.GetString("FileExistErr")!, dst));
			renameReverse[dst] = pair.Key;
		}
		// Execute deletion and rename
		var tempRename = new Dictionary<string, string>();
		foreach (var pair in rename) {
			var dst = pair.Value;
			if (deletion.Contains(dst)) {
				var dir = Path.GetDirectoryName(dst)!;
				var tempFilename = $"_{Path.GetFileName(dst)}";
				while (Directory.Exists(Path.Combine(dir!, tempFilename)))
					tempFilename = $"_{tempFilename}";
				dst = Path.Combine(dir, tempFilename);
				tempRename[pair.Value] = dst;
            }
			Directory.Move(pair.Key, dst);
		}
		foreach (var folder in deletion)
			Directory.Delete(folder, deepOpt);
		foreach (var pair in tempRename)
			Directory.Move(pair.Value, pair.Key);

		return;
		void SetRename(string src, string dst) {
			if (!rename.TryGetValue(src, out var existing))
				rename[src] = dst;
			else if (existing != dst) {
				if (IsParentDirectory(existing, dst))
					rename[src] = dst;
				else if (!IsParentDirectory(dst, existing)) {
					var template = Locale.FolderOperation.GetString("ConflictingMoveDestinations")!;
					throw new FolderOperationException(string.Format(template, existing, dst));
				}
			}
		}
	}

	internal void Check() {
		foreach (var folder in Folders)
			SetFolderFlag(folder);
	}

	private FolderFlag SetFolderFlag(string path) {
		if (Flags.TryGetValue(path, out var flag))
			return flag;
		using var e = Directory.EnumerateFileSystemEntries(path).GetEnumerator();

		if (!e.MoveNext())
			return Flags[path] = FolderFlag.Empty;
		var cur = e.Current!;
		if (e.MoveNext())
			return Flags[path] = FolderFlag.HasMultipleEntries;
		if (!IsDirectory(cur))
			return Flags[path] = FolderFlag.HasOneFile;
		return Flags[path] = FolderFlag.HasOneDirectory | SetFolderFlag(cur);
	}
}

[Flags]
public enum UnwrapOption : byte {
	Unwrap = 1 << 0,

	DeleteEmpty = 1 << 1,

	Deep = 1 << 2,

	UnwrapMultipleDeep = 1 << 3,

	UnwrapAndDeleteEmpty = Unwrap | DeleteEmpty
}

[Flags]
internal enum FolderFlag : byte {
	Empty = 1 << 0,

	HasOneFile = 1 << 1,

	HasOneDirectory = 1 << 2,

	HasMultipleEntries = 1 << 3
}