﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#nullable enable
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
		public DateTime? CreationDate => Value.Creation;

		[JsonIgnore]
		public DateTime? ModificationDate => Value.Modification;

		[JsonIgnore]
		public DateTime? AccessDate => Value.Access;

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Files => Entries?.Where(e => e.IsDirectory != true);

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Directories => Entries?.Where(e => e.IsDirectory == true);
	}

	public record EntryDate(DateTime? Creation = null, DateTime? Modification = null, DateTime? Access = null) {
		public DateTime? Creation { get; set; } = Creation;

		public DateTime? Modification { get; set; } = Modification;

		public DateTime? Access { get; set; } = Access;

		public static EntryDate FromEntry(string path, EntryDateFields fields = EntryDateFields.All) {
			var info = new FileInfo(path);
			DateTime? creation = null, modification = null, access = null;
			if (fields.HasFlag(EntryDateFields.Creation))
				creation = info.CreationTime;
			if (fields.HasFlag(EntryDateFields.Modification))
				modification = info.LastWriteTime;
			if (fields.HasFlag(EntryDateFields.Access))
				access = info.LastAccessTime;
			return new EntryDate(creation, modification, access);
		}

		public void ApplyToEntry(string path) {
			if (path.IsFile()) {
				if (Creation is not null)
					File.SetCreationTime(path, Creation.Value);
				if (Modification is not null)
					File.SetLastWriteTime(path, Modification.Value);
				if (Access is not null)
					File.SetLastAccessTime(path, Access.Value);
			}
			else {
				if (Creation is not null)
					Directory.SetCreationTime(path, Creation.Value);
				if (Modification is not null)
					Directory.SetLastWriteTime(path, Modification.Value);
				if (Access is not null)
					Directory.SetLastAccessTime(path, Access.Value);
			}
		}
	}

	[Flags]
	public enum EntryDateFields : byte {
		Creation = 1 << 0,

		Modification = 1 << 1,

		Access = 1 << 2,

		All = Creation | Modification | Access
	}
}