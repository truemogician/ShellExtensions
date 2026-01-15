using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TrueMogician.Extensions.IO;

namespace EntryDateCopier {
	public class EntryDateInfo {
		public EntryDateInfo(EntryDate value, string? path, IList<EntryDateInfo>? entries, bool? isDirectory) {
			Value = value;
			Path = string.IsNullOrEmpty(path) ? null : path;
			Entries = entries;
			if (isDirectory is null && Path is not null)
				throw new ArgumentException("Null directory flag is not allowed in non-general EntryDateInfo");
			IsDirectory = isDirectory;
		}

		public EntryDate Value { get; }

		public string? Path { get; }

		public IList<EntryDateInfo>? Entries { get; }

		public bool? IsDirectory { get; }

		[JsonIgnore]
		public bool General => Path is null;

		[JsonIgnore]
		public bool IncludesChildren => Entries is not null;

		[JsonIgnore]
		public DateTime? CreationTime => Value.Creation;

		[JsonIgnore]
		public DateTime? LastWriteTime => Value.LastWrite;

		[JsonIgnore]
		public DateTime? LastAccessTime => Value.LastAccess;

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Files => Entries?.Where(e => e.IsDirectory != true);

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Directories => Entries?.Where(e => e.IsDirectory == true);
	}

	public record EntryDate(DateTime? Creation = null, DateTime? LastWrite = null, DateTime? LastAccess = null) {
		public DateTime? Creation { get; set; } = Creation;

		[JsonProperty("Modification")]
		public DateTime? LastWrite { get; set; } = LastWrite;

		[JsonProperty("Access")]
        public DateTime? LastAccess { get; set; } = LastAccess;

		public static EntryDate FromEntry(string path, EntryDateFields fields = EntryDateFields.All) {
			var metadata = new EntryMetadata(path).BasicInfo;
			return new EntryDate(
				fields.HasFlag(EntryDateFields.Creation) ? metadata.CreationTime : null,
				fields.HasFlag(EntryDateFields.LastWrite) ? metadata.LastWriteTime : null,
				fields.HasFlag(EntryDateFields.LastAccess) ? metadata.LastAccessTime : null
			);
		}

		public void ApplyToEntry(string path) {
			var metadata = new EntryMetadata(path);
			var attr = metadata.Attributes;
			if (attr.HasFlag(FileAttributes.ReadOnly))
				metadata.Attributes = attr & ~FileAttributes.ReadOnly;
			try {
				metadata.SetTimes(Creation, LastAccess, LastWrite);
			}
			finally {
				if (attr.HasFlag(FileAttributes.ReadOnly))
					metadata.Attributes = attr;
			}
		}
	}

	[Flags]
	public enum EntryDateFields : byte {
		Creation = 1 << 0,

		LastWrite = 1 << 1,

		LastAccess = 1 << 2,

		All = Creation | LastWrite | LastAccess
	}
}