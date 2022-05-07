﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TrueMogician.Extensions.List;

#nullable enable
namespace EntryDateCopier {
	public enum EntryType {
		None,

		File,

		Directory
	}

	public static class Utilities {
		public static EntryType GetEntryType(string path, bool ensureExistence = false) {
			FileAttributes attributes;
			try {
				attributes = File.GetAttributes(path);
			}
			catch (FileNotFoundException) {
				if (ensureExistence)
					throw;
				return EntryType.None;
			}
			return attributes.HasFlag(FileAttributes.Directory) ? EntryType.Directory : EntryType.File;
		}

		public static bool IsFile(this string path) => GetEntryType(path, true) == EntryType.File;

		public static bool IsDirectory(this string path) => GetEntryType(path, true) == EntryType.Directory;

		public static void Sort(IList<EntryDateInfo>? entryDates, bool recursive = false, bool checkOrder = true) {
			if (entryDates is null || entryDates.Count == 0 || checkOrder && IsSorted(entryDates))
				return;
			entryDates.Sort(EntryDateInfoComparer.Default);
			if (recursive)
				foreach (var entryDate in entryDates.Where(info => info.IncludesChildren))
					Sort(entryDate.Entries, true, checkOrder);
		}

		public static bool IsSorted(IList<EntryDateInfo> entryDates) {
			for (var i = 0; i < entryDates.Count - 1; ++i)
				if (EntryDateInfoComparer.Default.Compare(entryDates[i], entryDates[i + 1]) > 0)
					return false;
			return true;
		}

		public static byte[] Compress(string str, Encoding? encoding = null) {
			encoding ??= Encoding.UTF8;
			byte[] bytes = encoding.GetBytes(str);
			using var msi = new MemoryStream(bytes);
			using var mso = new MemoryStream();
			using (var gs = new GZipStream(mso, CompressionMode.Compress))
				CopyStream(msi, gs);
			return mso.ToArray();
		}

		public static string Decompress(byte[] bytes, Encoding? encoding = null) {
			encoding ??= Encoding.UTF8;
			using var msi = new MemoryStream(bytes);
			using var mso = new MemoryStream();
			using (var gs = new GZipStream(msi, CompressionMode.Decompress))//gs.CopyTo(mso);
				CopyStream(gs, mso);
			return encoding.GetString(mso.ToArray());
		}

		public static IEnumerable<string> EnumerateFileSystemEntriesRecursively(string path) {
			foreach (string? entry in Directory.EnumerateFiles(path))
				yield return entry;
			foreach (string? entry in Directory.EnumerateDirectories(path)) {
				yield return entry;
				foreach (string? e in EnumerateFileSystemEntriesRecursively(entry))
					yield return e;
			}
		}

		public static void HandleException(Action action) {
			try {
				action();
			}
			catch (Exception ex) {
				MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static FileAttributes? UnlockFile(string path) {
			var attr = File.GetAttributes(path);
			if (attr.HasFlag(FileAttributes.ReadOnly)) {
				File.SetAttributes(path, attr & ~FileAttributes.ReadOnly);
				return attr;
			}
			return null;
		}

		private static void CopyStream(Stream src, Stream dest) {
			var bytes = new byte[4096];
			int cnt;
			while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
				dest.Write(bytes, 0, cnt);
		}
	}

	public class EntryDateInfoComparer : IComparer<EntryDateInfo> {
		public static EntryDateInfoComparer Default { get; } = new();

		public int Compare(EntryDateInfo x, EntryDateInfo y) {
			if (x is null || y is null)
				throw new ArgumentNullException();
			if (x.General == y.General)
				return string.Compare(x.Path, y.Path);
			return x.General ? -1 : 1;
		}
	}
}