using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrueMogician.Extensions.Enumerable;


namespace FolderWrapper;

public static class FolderWrapper {
	public static void Wrap(IEnumerable<string> paths) {
		var wrapperFolders = paths.ToDictionaryWith(
			p => Path.Combine(
				Path.GetDirectoryName(p)!,
				Path.GetFileNameWithoutExtension(p)
			)
		);
		if (!wrapperFolders.Values.Unique())
			throw new FolderWrapperException(Locale.FolderWrapper.GetString("DupFileNameErr"));
		if (wrapperFolders.Values.FirstOrDefault(Directory.Exists) is { } existing)
			throw new FolderWrapperException(string.Format(Locale.FolderWrapper.GetString("FolderExistErr")!, existing));
		foreach (var pair in wrapperFolders) {
			Directory.CreateDirectory(pair.Value);
			Directory.Move(pair.Key, Path.Combine(pair.Value, Path.GetFileName(pair.Key)));
		}
	}

	public static void Unwrap(IEnumerable<string> paths, bool deep) {
		var subEntries = new Dictionary<string, (IList<string> Entries, Dictionary<string, string?> SameNameEntries)>();
		foreach (var group in paths.GroupBy(Path.GetDirectoryName)) {
			var entries = new List<string>();
			foreach (string folder in group) {
				string[] subs = Directory.GetFileSystemEntries(folder);
				if (deep)
					while (subs.Length == 1 && IsDirectory(subs[0])) {
						string[] @new = Directory.GetFileSystemEntries(subs[0]);
						if (@new.Length > 0)
							subs = @new;
						else
							break;
					}
				entries.AddRange(subs);
			}
			if (!entries.Select(Path.GetFileName).Unique())
				throw new FolderWrapperException(Locale.FolderWrapper.GetString("SameNameFilesInFoldersErr"));
			var selected = new HashSet<string>(group);
			var sameNameEntries = group.ToDictionary(p => p, _ => null as string);
			if (entries.FirstOrDefault(
					p => {
						string path = Path.Combine(group.Key, Path.GetFileName(p));
						if (selected.Contains(path) && IsDirectory(path)) {
							sameNameEntries[path] = p;
							return false;
						}
						return Exists(path, IsDirectory(p));
					}
				) is { } existing)
				throw new FolderWrapperException(string.Format(Locale.FolderWrapper.GetString("FileExistErr")!, existing));
			sameNameEntries.Values.Where(p => p is not null).ForEach(p => entries.Remove(p!));
			subEntries[group.Key] = (entries, sameNameEntries);
		}
		foreach (var pair in subEntries)
			foreach (string src in pair.Value.Entries)
				Directory.Move(src, Path.Combine(pair.Key, Path.GetFileName(src)));
		foreach (var pair in subEntries) {
			var sameNameEntries = pair.Value.SameNameEntries;
			var sameNameMapping = new Dictionary<string, string>();
			foreach (string? entry in sameNameEntries.Values) {
				if (entry is null)
					continue;
				var fileName = $"_{Path.GetFileName(entry)}";
				while (Path.Combine(pair.Key, fileName) is var @new && Exists(@new, IsDirectory(entry)))
					fileName = "_" + fileName;
				sameNameMapping[fileName] = Path.GetFileName(entry);
				Directory.Move(entry, Path.Combine(pair.Key, fileName));
			}
			foreach (string? folder in sameNameEntries.Keys) {
				using var enumerator = Directory.EnumerateFileSystemEntries(folder).GetEnumerator();
				if (!enumerator.MoveNext() || deep && !enumerator.MoveNext())
					Directory.Delete(folder, deep);
				else
					throw new FolderWrapperException(Locale.FolderWrapper.GetString("FolderNotEmptyAfterWrap"));
			}
			foreach (var mapping in sameNameMapping)
				Directory.Move(Path.Combine(pair.Key, mapping.Key), Path.Combine(pair.Key, mapping.Value));
		}
	}

	internal static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);

	internal static bool Exists(string path, bool? isDirectory = null) => isDirectory is null
		? File.Exists(path) || Directory.Exists(path)
		: isDirectory.Value ? Directory.Exists(path) : File.Exists(path);
}