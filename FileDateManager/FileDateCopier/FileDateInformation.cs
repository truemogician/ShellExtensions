using Newtonsoft.Json;
using System;

namespace FileDateCopier {
	[Flags]
	public enum DateType {
		None = 0,
		Accessed = 1,
		Modified = 2,
		Created = 4,
		Written = 6,
		All = 7
	};

	public class FileDateInformation {
		public FileDateInformation() { }
		public FileDateInformation(DateTime[] times, DateType dateType = DateType.Written, string path = default, FileDateInformation[] files = default, FileDateInformation[] directories = default) {
			Path = string.IsNullOrEmpty(path) ? null : path;
			TimeType = dateType;
			Times = times;
			Files = files;
			Directories = directories;
			if (!IncludesChildren)
				IsDirectory = true;
		}
		[JsonIgnore]
		public bool General { get => Path == null; }
		[JsonIgnore]
		public bool IncludesChildren { get => Files != null || Directories != null; }
		public string Path { get; set; } = null;
		public bool? IsDirectory { get; private set; } = null;
		public DateType TimeType { get; set; } = DateType.None;
		public DateTime[] Times { get; set; } = null;
		public FileDateInformation[] Files { get; set; } = null;
		public FileDateInformation[] Directories { get; set; } = null;
	}
}