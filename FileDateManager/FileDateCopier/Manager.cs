using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace FileDateCopier {
	
	public static class Applier {
		private static void ApplyFileDateInformation(string path, FileDateInformation dateStatus) {
			bool isFile = FileDateInformation.GetPathType(path, true) == PathType.File;
			var indices = FileDateInformation.DateTypeToIndices(dateStatus.TimeType);
			for (int i = 0; i < indices.Length; ++i)
				FileDateInformation.SetDateTime[indices[i]](path, dateStatus.Times[i], isFile);
		}
		private static void ApplyToChildren(string path, FileDateInformation dateStatus) {
			if (dateStatus.IncludesChildren) {
				if (dateStatus.Files != null) {
					string[] files = Directory.GetFiles(path);
					if (files.Length > 0)
						Apply(files, dateStatus.Files, true);
				}
				if (dateStatus.Directories != null) {
					string[] folders = Directory.GetDirectories(path);
					if (folders.Length > 0)
						Apply(folders, dateStatus.Directories, true);
				}
			}
			else if (dateStatus.General) {
				string[] files = Directory.GetFiles(path);
				string[] folders = Directory.GetDirectories(path);
				Apply(files.Concat(folders).ToArray(), new FileDateInformation[] { dateStatus }, true);
			}
		}
		internal static void Apply(string[] paths, FileDateInformation[] dateStatuses, bool applyToChildren = false) {
			if (paths.Length == 0)
				return;
			if (dateStatuses.Length == 1 && dateStatuses[0].General) {
				FileDateInformation dateStatus = dateStatuses[0];
				foreach (string path in paths) {
					if (applyToChildren && Directory.Exists(path))
						ApplyToChildren(path, dateStatus);
					ApplyFileDateInformation(path, dateStatus);
				}
			}
			else {
				var appliedFiles = new HashSet<string>();
				foreach (FileDateInformation dateStatus in dateStatuses)
					if (!dateStatus.General) {
						foreach (string path in paths)
							if (Path.GetFileName(path) == dateStatus.Path) {
								if (applyToChildren && Directory.Exists(path))
									ApplyToChildren(path, dateStatus);
								ApplyFileDateInformation(path, dateStatus);
								appliedFiles.Add(path);
							}
					}
				var defaultFileDateInformation = dateStatuses.FirstOrDefault(dateStatus => dateStatus.General);
				if (defaultFileDateInformation != default) {
					foreach (string path in paths)
						if (!appliedFiles.Contains(path)) {
							if (applyToChildren && Directory.Exists(path))
								ApplyToChildren(path, defaultFileDateInformation);
							ApplyFileDateInformation(path, defaultFileDateInformation);
						}
				}
			}
		}
		public static void Apply(string[] paths, string dateStatusFilePath) {
			var reader = new StreamReader(dateStatusFilePath);
			bool applyToChildren = (char)reader.Read() == '1';
			FileDateInformation[] dateStatuses = JsonConvert.DeserializeObject<FileDateInformation[]>(reader.ReadToEnd());
			reader.Close();
			Apply(paths, dateStatuses, applyToChildren);
		}
		public static void Apply(string path, string dateStatusFilePath)
			=> Apply(new string[] { path }, dateStatusFilePath);
	}
	public static class Generator {
		public static FileDateInformation Generate(string path, bool matchBasePath = false, bool includesChildren = false, DateType dateType = DateType.Written)
			=> new FileDateInformation(path, matchBasePath ? Path.GetFileName(path) : null, includesChildren, dateType);
		public static FileDateInformation[] Generate(string[] paths, bool includesChildren = false, DateType dateType = DateType.Written) {
			var result = new List<FileDateInformation>();
			foreach (string path in paths)
				result.Add(Generate(path, true, includesChildren, dateType));
			return result.ToArray();
		}
		public static void WriteFile(string path, FileDateInformation src, bool applyToChildren = false, bool hidden = false) => WriteFile(path, new FileDateInformation[] { src }, applyToChildren, hidden);
		public static void WriteFile(string path, FileDateInformation[] srcs, bool applyToChildren = false, bool hidden = false) {
			if (!File.Exists(path))
				File.Create(path).Close();
			if (hidden)
				File.SetAttributes(path, FileAttributes.Hidden);
			var writer = new StreamWriter(path, false);
			writer.Write(applyToChildren ? '1' : '0');
			writer.Write(JsonConvert.SerializeObject(srcs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
			writer.Close();
		}
	}
	public static class Synchronizor {
		public static void Synchronize(string[] dsts, string src, bool matchPath = false, bool includeChildren = true, bool applyToChildren = false, DateType dateType = DateType.All) {
			var info = Generator.Generate(src, matchPath, includeChildren, dateType);
			Applier.Apply(dsts, new FileDateInformation[] { info }, applyToChildren);
		}
		public static void Synchronize(string dst, string src, bool matchPath = false, bool includeChildren = true, bool applyToChildren = false, DateType dateType = DateType.All)
			=> Synchronize(new string[] { dst }, src, matchPath, includeChildren, applyToChildren, dateType);
	}
}