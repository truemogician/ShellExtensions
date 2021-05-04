using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace FileDateCopier {
	using static FileHeader;
	using static Utility;
	public class EntryEventArg : EventArgs {
		public EntryEventArg(string path = default) : base()
			=> Path = path;
		public string Path { get; }
	}
	public delegate void EntryEventHandler(object sender, EntryEventArg e);
	public class Applier {
		public Applier(string[] paths, string fileDatePath) {
			var reader = new StreamReader(fileDatePath);
			var byteArray = new byte[4];
			reader.BaseStream.Read(byteArray, 0, 4);
			int size = BitConverter.ToInt32(byteArray, 0);
			byteArray = new byte[size];
			reader.BaseStream.Read(byteArray, 0, size);
			try {
				Paths = paths;
				Header = StructMarshalling<FileHeader>.FromByteArray(ref byteArray);
				FileDates = JsonConvert.DeserializeObject<FileDateInformation[]>(reader.ReadToEnd());
				reader.Close();
			}
			catch (Exception ex) {
				throw new InvalidDataException("File date information file corrupted", ex);
			}
		}
		public Applier(string path, string fileDatePath) : this(new string[] { path }, fileDatePath) { }
		public Applier(string[] paths, FileDateInformation[] fileDates, bool appliesToChildren) {
			Paths = paths;
			FileDates = fileDates;
			Header = new FileHeader(appliesToChildren, fileDates);
		}
		public event EntryEventHandler EntryDateWritten = delegate { };
		public string[] Paths { get; set; }
		public FileHeader Header { get; set; }
		public FileDateInformation[] FileDates { get; set; }
		public void Apply() {
			if (Paths.Length == 0)
				return;
			if (!Header.Flags.HasFlag(HeaderFlag.Special) && !Header.Flags.HasFlag(HeaderFlag.General))
				throw new ArgumentException("Incomplete file header", nameof(Header));
			if (Header.Flags.HasFlag(HeaderFlag.General)) {
				if (Header.Flags.HasFlag(HeaderFlag.Special | HeaderFlag.Multiple)) {
					if (FileDates.Count(fileDate => fileDate.General) != 1)
						throw new ArgumentException("One layer can only cantain 1 general FileDateInformation at most", nameof(FileDates));
					Utility.Sort(FileDates, true);
					Array.Sort(Paths);
					ApplySpecialWithDefault(Paths, FileDates, Header.Flags.HasFlag(HeaderFlag.AppliesToChildren));
				}
				else if (!Header.Flags.HasFlag(HeaderFlag.Special) && !Header.Flags.HasFlag(HeaderFlag.Multiple))
					ApplyGeneral(Paths, FileDates[0], Header.Flags.HasFlag(HeaderFlag.AppliesToChildren));
				else
					throw new ArgumentException("Invalid file header", nameof(Header));
			}
			else {
				Utility.Sort(FileDates, true);
				Array.Sort(Paths);
				ApplySpecial(Paths, FileDates, Header.Flags.HasFlag(HeaderFlag.AppliesToChildren));
			}
		}
		protected virtual void OnEntryDateWritten(EntryEventArg e) => EntryDateWritten(this, e);
		private void ApplyFileDateInformation(string path, FileDateInformation fileDate) {
			bool isFile = GetPathType(path, true) == PathType.File;
			var indices = DateTypeToIndices(fileDate.TimeType);
			for (int i = 0; i < indices.Count; ++i)
				SetDateTime[indices[i]](path, fileDate.Times[i], isFile);
			OnEntryDateWritten(new EntryEventArg(path));
		}
		private void ApplyToChildren(string path, FileDateInformation fileDate) {
			if (!Directory.Exists(path))
				return;
			if (fileDate.IncludesChildren) {
				if (fileDate.Files != null) {
					string[] files = Directory.GetFiles(path);
					if (files.Length > 0) {
						Array.Sort(files);
						if (fileDate.Files[fileDate.Files.Length - 1].General)
							ApplySpecialWithDefault(files, fileDate.Files, true);
						else
							ApplySpecial(files, fileDate.Files, true);
					}
				}
				if (fileDate.Directories != null) {
					string[] folders = Directory.GetDirectories(path);
					if (folders.Length > 0) {
						Array.Sort(folders);
						if (fileDate.Directories[fileDate.Directories.Length - 1].General)
							ApplySpecialWithDefault(folders, fileDate.Directories, true);
						else
							ApplySpecial(folders, fileDate.Directories, true);
					}
				}
			}
			else if (fileDate.General) {
				string[] files = Directory.GetFiles(path);
				string[] folders = Directory.GetDirectories(path);
				ApplyGeneral(files.Concat(folders).ToArray(), fileDate, true);
			}
		}
		private void ApplyGeneral(string[] paths, FileDateInformation fileDate, bool appliesToChildren) {
			foreach (string path in paths) {
				if (appliesToChildren)
					ApplyToChildren(path, fileDate);
				ApplyFileDateInformation(path, fileDate);
			}
		}
		private void ApplySpecial(string[] paths, FileDateInformation[] fileDates, bool appliesToChildren) {
			int i = 0;
			foreach (string path in paths) {
				if (Path.GetFileName(path) == fileDates[i].Path) {
					if (appliesToChildren)
						ApplyToChildren(path, fileDates[i]);
					ApplyFileDateInformation(path, fileDates[i]);
					++i;
				}
				if (i == fileDates.Length)
					break;
			}
		}
		private void ApplySpecialWithDefault(string[] paths, FileDateInformation[] fileDates, bool appliesToChildren) {
			var defaultFileDate = fileDates[fileDates.Length - 1];
			int i = 0;
			foreach (string path in paths) {
				if (i < fileDates.Length - 1 && Path.GetFileName(path) == fileDates[i].Path) {
					if (appliesToChildren)
						ApplyToChildren(path, fileDates[i]);
					ApplyFileDateInformation(path, fileDates[i]);
					++i;
				}
				else {
					if (appliesToChildren)
						ApplyToChildren(path, defaultFileDate);
					ApplyFileDateInformation(path, defaultFileDate);
				}
			}
		}
	}
	public class Generator {
		public Generator(string[] paths)
			=> Paths = paths;
		public Generator(string path) : this(new string[] { path }) { }
		public event EntryEventHandler EntryDateRead;
		public string[] Paths { get; set; }
		public FileDateInformation[] FileDates { get; private set; }
		public void OnEntryDateRead(EntryEventArg e) => EntryDateRead(this, e);
		public FileDateInformation Generate(string path, bool matchBasePath = false, bool includesChildren = false, DateType dateType = DateType.Written) {
			var isDirectory = GetPathType(path, true) == PathType.Directory;
			var times = new List<DateTime>(3);
			foreach (int i in DateTypeToIndices(dateType))
				times.Add(GetDateTime[i](path, !isDirectory));
			OnEntryDateRead(new EntryEventArg(path));
			FileDateInformation result = new FileDateInformation(times.ToArray(), dateType, matchBasePath ? Path.GetFileName(path) : null);
			if (isDirectory && includesChildren) {
				var dirInfo = new DirectoryInfo(path);
				var files = dirInfo.EnumerateFiles();
				var subs = new List<FileDateInformation>();
				foreach (var file in files)
					subs.Add(Generate(file.FullName, true, false, dateType));
				result.Files = subs.ToArray();
				var directories = new DirectoryInfo(path).EnumerateDirectories();
				subs.Clear();
				foreach (var directory in directories)
					subs.Add(Generate(directory.FullName, true, true, dateType));
				result.Directories = subs.ToArray();
			}
			return result;
		}
		public FileDateInformation[] Generate(bool includesChildren = false, DateType dateType = DateType.Written) {
			var result = new List<FileDateInformation>();
			foreach (string path in Paths)
				result.Add(Generate(path, true, includesChildren, dateType));
			return FileDates = result.ToArray();
		}
		public static void SaveToFile(string path, FileDateInformation src, bool appliesToChildren = false)
			=> SaveToFile(path, new FileDateInformation[] { src }, appliesToChildren);
		public static void SaveToFile(string path, FileDateInformation[] srcs, bool appliesToChildren = false) {
			if (!File.Exists(path))
				File.Create(path).Close();
			var writer = new StreamWriter(path, false);
			var header = new FileHeader(appliesToChildren, srcs);
			writer.Write(StructMarshalling<FileHeader>.Size());
			var byteArray = StructMarshalling<FileHeader>.ToByteArray(ref header);
			writer.BaseStream.Write(byteArray, 0, byteArray.Length);
			writer.Write(JsonConvert.SerializeObject(srcs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
			writer.Close();
		}
		public void SaveToFile(string path, bool appliedToChildren = false)
			=> SaveToFile(path, FileDates, appliedToChildren);
	}
	public static class Synchronizor {
		public static void Synchronize(string[] dsts, string src, bool matchPath = false, bool includeChildren = true, bool appliesToChildren = false, DateType dateType = DateType.All) {
			var info = Generator.Generate(src, matchPath, includeChildren, dateType);
			Applier.Apply(dsts, new FileDateInformation[] { info }, appliesToChildren);
		}
		public static void Synchronize(string dst, string src, bool matchPath = false, bool includeChildren = true, bool appliesToChildren = false, DateType dateType = DateType.All)
			=> Synchronize(new string[] { dst }, src, matchPath, includeChildren, appliesToChildren, dateType);
	}
}