using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrueMogician.Exceptions;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.Enumerator;

#nullable enable
namespace EntryDateCopier {
	using static Utilities;

	public class EntryEventArgs : EventArgs {
		public EntryEventArgs(string path, EntryDate? dates = null) {
			Path = path;
			EntryDate = dates;
		}

		public string Path { get; }

		public EntryDate? EntryDate { get; }
	}

	public class ReadyEventArgs : EventArgs {
		public ReadyEventArgs(Dictionary<string, EntryDate> map) => Map = map;

		public Dictionary<string, EntryDate> Map { get; init; }
	}

	public class Applier {
		public Applier(string path, string ediPath, bool appliesToChildren = true) : this(new[] { path }, ediPath, appliesToChildren) { }

		public Applier(string[] paths, string ediPath, bool appliesToChildren = true) : this(paths, GetEntryDates(ediPath), appliesToChildren) { }

		public Applier(string[] paths, EntryDateInfo[] entryDates, bool appliesToChildren) {
			Paths = paths;
			Array.Sort(Paths, (p1, p2) => string.CompareOrdinal(Path.GetFileName(p1), Path.GetFileName(p2)));
			EntryDates = entryDates;
			Sort(EntryDates, true);
			AppliesToChildren = appliesToChildren;
		}

		public event EventHandler? Start;

		public event EventHandler<ReadyEventArgs>? Ready;

		public event EventHandler<EntryEventArgs>? SetEntryDates;

		public event EventHandler? Complete;

		public string[] Paths { get; set; }

		public EntryDateInfo[] EntryDates { get; set; }

		public bool AppliesToChildren { get; }

		public async Task Apply() {
			var newDates = new Dictionary<string, EntryDate>();
			void Search(IEnumerable<string> paths, IEnumerable<EntryDateInfo> infos, EntryDateInfo? @default = null) {
				using var e = infos.GetEnumerator().ToExtended();
				if (!e.MoveNext())
					throw new EmptyCollectionException();
				@default = e.Current!.General ? e.GetAndMoveNext() : @default;
				if (e.Success && e.Current!.General)
					throw new InvalidOperationException(@"More than one default entry date found");
				foreach (string path in paths) {
					string? fileName = Path.GetFileName(path);
					while (e.Success && string.CompareOrdinal(fileName, e.Current!.Path) < 0)
						e.MoveNext();
					if (!e.Success && @default is null)
						break;
					if (fileName == e.Current!.Path && (e.Current.IsDirectory ?? false) == path.IsDirectory()) {
						newDates[path] = e.Current.Value;
						if (AppliesToChildren && e.Current.IncludesChildren && path.IsDirectory()) {
							string[] entries = Directory.EnumerateFileSystemEntries(path).ToArray();
							Array.Sort(entries);
							Search(entries, e.Current.Entries!, @default);
						}
					}
					else if (@default is not null) {
						newDates[path] = @default.Value;
						if (AppliesToChildren && path.IsDirectory())
							foreach (string? entry in EnumerateFileSystemEntriesRecursively(path))
								newDates[entry] = @default.Value;
					}
				}
			}
			OnStart();
			Search(Paths, EntryDates);
			OnReady(new ReadyEventArgs(newDates));
			await Task.WhenAll(
				newDates.Select(
						pair => Task.Run(
							() => {
								pair.Value.ApplyToEntry(pair.Key);
								OnSetEntryDates(new EntryEventArgs(pair.Key, pair.Value));
							}
						)
					)
					.ToArray()
			);
			OnComplete();
		}

		protected virtual void OnStart(EventArgs? e = null) => Start?.Invoke(this, e ?? EventArgs.Empty);

		protected virtual void OnReady(ReadyEventArgs e) => Ready?.Invoke(this, e);

		protected virtual void OnSetEntryDates(EntryEventArgs e) => SetEntryDates?.Invoke(this, e);

		protected virtual void OnComplete(EventArgs? e = null) => Complete?.Invoke(this, e ?? EventArgs.Empty);

		private static EntryDateInfo[] GetEntryDates(string ediPath) {
			try {
				return EdiFile.Load(ediPath).Data;
			}
			catch (Exception ex) {
				throw new InvalidDataException("edi file corrupted", ex);
			}
		}
	}

	public class Generator {
		public Generator(IList<string> paths) => Paths = paths.AsArray();

		public Generator(string path) : this(new[] { path }) { }

		public event EventHandler? Complete;

		public event EventHandler<EntryEventArgs>? ReadEntryDate;

		public event EventHandler? Start;

		public string[] Paths { get; }

		public EntryDateInfo[]? EntryDates { get; private set; }

		public void OnStart(EventArgs? e = null) => Start?.Invoke(this, e ?? EventArgs.Empty);

		public void OnReadEntryDate(EntryEventArgs e) => ReadEntryDate?.Invoke(this, e);

		public void OnComplete(EventArgs? e = null) => Complete?.Invoke(this, e ?? EventArgs.Empty);

		public static void SaveToFile(string path, EntryDateInfo src) => SaveToFile(path, new[] { src });

		public static void SaveToFile(string path, EntryDateInfo[] srcs) => new EdiFile(srcs).Save(path);

		public void SaveToFile(string path) => SaveToFile(path, EntryDates ?? throw new InvalidOperationException("Generator not started yet."));

		public EntryDateInfo[] Generate(bool matchBasePath = true, bool includesChildren = false, EntryDateFields fields = EntryDateFields.All) {
			OnStart();
			EntryDates = Paths.Select(path => Generate(path, matchBasePath, includesChildren, fields)).ToArray();
			OnComplete();
			return EntryDates;
		}

		private EntryDateInfo Generate(string path, bool matchBasePath = false, bool includesChildren = false, EntryDateFields fields = EntryDateFields.All) {
			var dates = EntryDate.FromEntry(path, fields);
			OnReadEntryDate(new EntryEventArgs(path, dates));
			var result = new EntryDateInfo(dates, matchBasePath ? Path.GetFileName(path) : null);
			if (path.IsDirectory() && includesChildren) {
				var dirInfo = new DirectoryInfo(path);
				result.Entries = dirInfo.EnumerateFileSystemInfos().Select(info => Generate(info.FullName, true, true, fields)).ToList();
				Sort(result.Entries, true);
			}
			return result;
		}
	}

	public class Synchronizer {
		public Synchronizer(IList<string> dsts, string src) {
			DestinationPaths = dsts.AsArray();
			SourcePath = src;
		}

		public Synchronizer(string dst, string src) : this(new[] { dst }, src) { }

		public string SourcePath { get; set; }

		public string[] DestinationPaths { get; set; }

		public async Task Synchronize(bool includeChildren = true, bool appliesToChildren = false, EntryDateFields fields = EntryDateFields.All) {
			var infos = new Generator(SourcePath).Generate(false, includeChildren, fields);
			await new Applier(DestinationPaths, infos, appliesToChildren).Apply();
		}
	}
}