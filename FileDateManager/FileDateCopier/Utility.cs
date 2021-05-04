using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FileDateCopier {
	public enum PathType {
		None, File, Directory
	};
	public static class Utility {
		public static EntryDateComparer Comparer = new EntryDateComparer();
		public static DateTimeGetter[] GetDateTime = {
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				return (bool)isFile ? File.GetLastAccessTime(path) : Directory.GetLastAccessTime(path);
			},
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				return (bool)isFile ? File.GetLastWriteTime(path) : Directory.GetLastWriteTime(path);
			},
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				return (bool)isFile ? File.GetCreationTime(path) : Directory.GetCreationTime(path);
			}
		};
		public static DateTimeSetter[] SetDateTime = {
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string, DateTime>)(File.SetLastAccessTime) : Directory.SetLastAccessTime)(path, value);
				if (attr.HasValue)
					File.SetAttributes(path, attr.Value);
			},
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string, DateTime>)(File.SetLastWriteTime) : Directory.SetLastWriteTime)(path, value);
				if (attr.HasValue)
					File.SetAttributes(path, attr.Value);
			},
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path, true) == PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string, DateTime>)(File.SetCreationTime) : Directory.SetCreationTime)(path, value);
				if (attr.HasValue)
					File.SetAttributes(path, attr.Value);
			}
		};
		public delegate DateTime DateTimeGetter(string path, bool? isFile = null);

		public delegate void DateTimeSetter(string path, DateTime value, bool? isFile = null);
		public static IList<int> DateTypeToIndices(DateType dateType) {
			var result = new List<int>(3);
			if (dateType.HasFlag(DateType.Accessed))
				result.Add(0);
			if (dateType.HasFlag(DateType.Modified))
				result.Add(1);
			if (dateType.HasFlag(DateType.Created))
				result.Add(2);
			return result;
		}
		public static PathType GetPathType(string path, bool throwIfNone = false) {
			if (File.Exists(path))
				return PathType.File;
			else if (Directory.Exists(path))
				return PathType.Directory;
			else if (throwIfNone)
				throw new FileNotFoundException("Path doesn't exist", path);
			else
				return PathType.None;
		}
		public static void Sort(FileDateInformation[] fileDates, bool recursive = false, bool checkSorted = true) {
			if (fileDates == null || fileDates.Length == 0 || checkSorted && IsSorted(fileDates))
				return;
			Array.Sort(fileDates, Comparer);
			if (recursive) {
				foreach (var fileDate in fileDates)
					if (fileDate.IncludesChildren) {
						Sort(fileDate.Files, recursive, checkSorted);
						Sort(fileDate.Directories, recursive, checkSorted);
					}
			}
		}
		public static bool IsSorted(FileDateInformation[] fileDates) {
			for (int i = 0; i < fileDates.Length - 1; ++i)
				if (Comparer.Compare(fileDates[i], fileDates[i + 1]) > 0)
					return false;
			return true;
		}
		private static FileAttributes? UnlockFile(string path) {
			var attr = File.GetAttributes(path);
			if (attr.HasFlag(FileAttributes.ReadOnly)) {
				File.SetAttributes(path, attr & ~FileAttributes.ReadOnly);
				return attr;
			}
			else
				return null;
		}

		public class EntryDateComparer : IComparer<FileDateInformation> {
			public int Compare(FileDateInformation x, FileDateInformation y) {
				if (x.General == y.General)
					return x.Path.CompareTo(y.Path);
				else
					return x.General ? -1 : 1;
			}
		}
	}
}
