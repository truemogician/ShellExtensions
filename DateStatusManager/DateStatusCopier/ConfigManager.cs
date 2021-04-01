using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DateStatusCopier {
	public static class Applier {
		static void ApplyDateStatus(string path, DateStatus dateStatus) {
			bool isFile = DateStatus.GetPathType(path, true) == PathType.File;
			var indices = DateStatus.DateTypeToIndices(dateStatus.TimeType);
			for (int i = 0; i < indices.Length; ++i)
				DateStatus.SetDateTime[indices[i]](path, dateStatus.Times[i], isFile);
		}
		static void ApplyToChildren(string path, DateStatus dateStatus) {
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
			else if (dateStatus.Universal) {
				string[] files = Directory.GetFiles(path);
				string[] folders = Directory.GetDirectories(path);
				Apply(files.Concat(folders).ToArray(), new DateStatus[] { dateStatus }, true);
			}
		}
		static void Apply(string[] paths, DateStatus[] dateStatuses, bool applyToChildren = false) {
			if (paths.Length == 0)
				return;
			if (dateStatuses.Length == 1 && dateStatuses[0].Universal) {
				DateStatus dateStatus = dateStatuses[0];
				foreach (string path in paths) {
					if (applyToChildren && Directory.Exists(path))
						ApplyToChildren(path, dateStatus);
					ApplyDateStatus(path, dateStatus);
				}
			}
			else {
				var appliedFiles = new HashSet<string>();
				foreach (DateStatus dateStatus in dateStatuses)
					if (!dateStatus.Universal) {
						foreach (string path in paths)
							if (Path.GetFileName(path) == dateStatus.Path) {
								if (applyToChildren && Directory.Exists(path))
									ApplyToChildren(path, dateStatus);
								ApplyDateStatus(path, dateStatus);
								appliedFiles.Add(path);
							}
					}
				try {
					DateStatus defaultDateStatus = dateStatuses.First(dateStatus => dateStatus.Universal);
					foreach (string path in paths)
						if (!appliedFiles.Contains(path)) {
							if (applyToChildren && Directory.Exists(path))
								ApplyToChildren(path, defaultDateStatus);
							ApplyDateStatus(path, defaultDateStatus);
						}
				}
				catch (InvalidOperationException) { }
			}
		}
		public static void Apply(string[] paths, string dateStatusFilePath, bool applyToChildren = false) {
			var reader = new StreamReader(dateStatusFilePath);
			DateStatus[] dateStatuses = JsonConvert.DeserializeObject<DateStatus[]>(reader.ReadToEnd());
			Apply(paths, dateStatuses, applyToChildren);
		}
		public static void Apply(string path, string dateStatusFilePath, bool applyToChildren = false)
			=> Apply(new string[] { path }, dateStatusFilePath, applyToChildren);
	}
	public static class Generator {
		public static DateStatus Generate(string path, bool matchBasePath = false, bool includesChildren = false, DateType dateType = DateType.Written) {
			if (matchBasePath)
				return new DateStatus(path, Path.GetFileName(path), includesChildren, dateType);
			else
				return new DateStatus(path, null, includesChildren, dateType);
		}
		public static DateStatus[] Generate(string[] paths, bool includesChildren = false, DateType dateType = DateType.Written) {
			var result = new List<DateStatus>();
			foreach (string path in paths)
				result.Add(Generate(path, true, includesChildren, dateType));
			return result.ToArray();
		}
		public static void WriteFile(string path, DateStatus src, bool hidden = false) => WriteFile(path, new DateStatus[] { src }, hidden);
		public static void WriteFile(string path, DateStatus[] srcs, bool hidden = false) {
			if (!File.Exists(path))
				File.Create(path).Close();
			if (hidden)
				File.SetAttributes(path, FileAttributes.Hidden);
			var writer = new StreamWriter(path);
			writer.Write(JsonConvert.SerializeObject(srcs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
			writer.Close();
		}
	}
}