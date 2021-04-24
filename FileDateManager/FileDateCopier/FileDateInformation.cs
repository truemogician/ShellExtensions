using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FileDateCopier {
	public enum PathType {
		None, File, Directory
	};
	[Flags]
	public enum DateType {
		None = 0,
		Accessed = 1,
		Modified = 2,
		Created = 4,
		Written = 6,
		All = 7
	};
	public class FileDateInformationConfiguration {
		public bool ApplyToChildren { get; set; }
		public FileDateInformationConfiguration(bool applyToChildren) {
			ApplyToChildren = applyToChildren;
		}
	}
	public class FileDateInformation {
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
		public static int[] DateTypeToIndices(DateType dateType) {
			var result = new List<int>();
			if (dateType.HasFlag(DateType.Accessed))
				result.Add(0);
			if (dateType.HasFlag(DateType.Modified))
				result.Add(1);
			if (dateType.HasFlag(DateType.Created))
				result.Add(2);
			return result.ToArray();
		}
		public delegate DateTime DateTimeGetter(string path, bool? isFile = null);
		public static DateTimeGetter[] GetDateTime = {
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				return (bool)isFile ? File.GetLastAccessTime(path) : Directory.GetLastAccessTime(path);
			},
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				return (bool)isFile ? File.GetLastWriteTime(path) : Directory.GetLastWriteTime(path);
			},
			(path, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				return (bool)isFile ? File.GetCreationTime(path) : Directory.GetCreationTime(path);
			}
		};
		public static FileAttributes UnlockFile(string path) {
			var attr = File.GetAttributes(path);
			File.SetAttributes(path, FileAttributes.Normal);
			return attr;
		}
		public delegate void DateTimeSetter(string path, DateTime value, bool? isFile = null);
		public static DateTimeSetter[] SetDateTime = {
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string,DateTime>)(File.SetLastAccessTime) : Directory.SetLastAccessTime)(path,value);
				File.SetAttributes(path,attr);
			},
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string,DateTime>)(File.SetLastWriteTime) : Directory.SetLastWriteTime)(path,value);
				File.SetAttributes(path,attr);
			},
			(path, value, isFile) => {
				if (!isFile.HasValue)
					isFile = GetPathType(path,true)==PathType.File;
				var attr = UnlockFile(path);
				((bool)isFile ? (Action<string,DateTime>)(File.SetCreationTime) : Directory.SetCreationTime)(path,value);
				File.SetAttributes(path,attr);
			}
		};
		public string Path { get; set; } = null;
		public bool? IsDirectory { get; set; } = null;
		[JsonIgnore]
		public bool General { get => Path == null; }
		[JsonIgnore]
		public bool IncludesChildren { get => Files != null || Directories != null; }
		public DateType TimeType { get; set; } = DateType.None;
		public DateTime[] Times { get; set; } = null;
		public FileDateInformation[] Files { get; set; } = null;
		public FileDateInformation[] Directories { get; set; } = null;

		public FileDateInformation() { }
		public FileDateInformation(string path, string pathToMatch = null, bool includesChildren = false, DateType dateType = DateType.Written) {
			Path = string.IsNullOrEmpty(pathToMatch) ? null : pathToMatch;
			TimeType = dateType;
			if (GetPathType(path, true) == PathType.Directory && includesChildren) {
				string[] files = Directory.GetFiles(path);
				if (files?.Length != 0) {
					var subs = new List<FileDateInformation>();
					foreach (string file in files)
						subs.Add(new FileDateInformation(file, System.IO.Path.GetFileName(file), false, dateType));
					Files = subs.ToArray();
				}
				string[] directories = Directory.GetDirectories(path);
				if (directories?.Length != 0) {
					var subs = new List<FileDateInformation>();
					foreach (string directory in directories)
						subs.Add(new FileDateInformation(directory, System.IO.Path.GetFileName(directory), true, dateType));
					Directories = subs.ToArray();
				}
				if (!IncludesChildren)
					IsDirectory = true;
			}
			var times = new List<DateTime>();
			foreach (int i in DateTypeToIndices(dateType))
				times.Add(GetDateTime[i](path, IsDirectory == false));
			if (times.Count > 0)
				Times = times.ToArray();
		}

		public string ToJson(bool ignoreNull = true) {
			JsonSerializerSettings settings = new JsonSerializerSettings {
				NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
			};
			return JsonConvert.SerializeObject(this, settings);
		}
	}
}
