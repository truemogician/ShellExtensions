using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#nullable enable
namespace EntryDateCopier {
	public class EntryDateInfo {
		public EntryDateInfo(EntryDate value, string? path = null, IList<EntryDateInfo>? entries = null) {
			Value = value;
			Path = string.IsNullOrEmpty(path) ? null : path;
			Entries = entries;
			if (!IncludesChildren)
				IsDirectory = true;
		}

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

		public string? Path { get; set; }

		public bool? IsDirectory { get; }

		public EntryDate Value { get; }

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Files => Entries?.Where(e => e.IsDirectory != true);

		[JsonIgnore]
		public IEnumerable<EntryDateInfo>? Directories => Entries?.Where(e => e.IsDirectory == true);

		public IList<EntryDateInfo>? Entries { get; set; }

		public bool Match(string path) {
			if (!System.IO.Path.IsPathRooted(path))
				throw new ArgumentException(@"Not an absolute path", nameof(path));
			if (Path is null)
				return true;
			path = System.IO.Path.GetFullPath(path);
			if (!path.EndsWith(Path))
				return false;
			return path.LastIndexOf(Path, StringComparison.Ordinal) is var idx && (idx == 0 || path[idx - 1] == System.IO.Path.DirectorySeparatorChar || path[idx - 1] == System.IO.Path.AltDirectorySeparatorChar);
		}
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
			if (Creation is not null)
				File.SetCreationTime(path, Creation.Value);
			if (Modification is not null)
				File.SetLastWriteTime(path, Modification.Value);
			if (Access is not null)
				File.SetLastAccessTime(path, Access.Value);
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

namespace System.Runtime.CompilerServices {
	internal class IsExternalInit { }
}