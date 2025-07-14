using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntryDateCopier.Properties;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.Enumerator;

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
		public ReadyEventArgs(IReadOnlyDictionary<string, EntryDate> map) => Map = map;

		public IReadOnlyDictionary<string, EntryDate> Map { get; }
	}

	public class CancelEventArgs : EventArgs {
		public CancelEventArgs() { }

		public CancelEventArgs(Exception? exception = null, Task? task = null) {
			Task = task;
			Exception = exception;
		}

		public Task? Task { get; }

		public Exception? Exception { get; }
	}

	public class Applier {
		public Applier(string path, string ediPath) : this(new[] { path }, ediPath) { }

		public Applier(IEnumerable<string> paths, string ediPath) : this(paths, GetEntryDates(ediPath)) { }

		public Applier(IEnumerable<string> paths, EntryDateInfo[] entryDates) {
			Paths = paths.OrderBy(Path.GetFileName).ToArray();
			EntryDates = entryDates;
			Sort(EntryDates, true);
		}

		public event EventHandler? Start;

		public event EventHandler<ReadyEventArgs>? Ready;

		public event EventHandler<EntryEventArgs>? SetEntryDate;

		public event EventHandler? Complete;

		public event EventHandler<CancelEventArgs>? Cancel;

		public string[] Paths { get; set; }

		public EntryDateInfo[] EntryDates { get; set; }

		public void Apply(bool appliesToChildren = true) {
			OnStart();
			var newDates = Search(Paths, EntryDates, appliesToChildren).ToDictionary(p => p.Path, p => p.NewDate);
			OnReady(new ReadyEventArgs(newDates));
			foreach (var pair in newDates) {
				pair.Value.ApplyToEntry(pair.Key);
				OnSetEntryDate(new EntryEventArgs(pair.Key, pair.Value));
			}
			OnComplete();
		}

		public async Task ApplyAsync(bool appliesToChildren = true, CancellationToken? cancellationToken = null) {
			cancellationToken ??= CancellationToken.None;
			OnStart();
			var newDates = Search(Paths, EntryDates, appliesToChildren).ToDictionary(p => p.Path, p => p.NewDate);
			OnReady(new ReadyEventArgs(newDates));
			try {
				await Task.WhenAll(
					newDates.Select(
							pair => Task.Run(
								() => {
									pair.Value.ApplyToEntry(pair.Key);
									OnSetEntryDate(new EntryEventArgs(pair.Key, pair.Value));
								},
								cancellationToken.Value
							)
						)
						.ToArray()
				);
				OnComplete();
			}
			catch (TaskCanceledException ex) {
				OnCancel(new CancelEventArgs(ex));
			}
			catch (AggregateException ex) {
				if (ex.InnerExceptions.Any(e => e is TaskCanceledException))
					OnCancel(new CancelEventArgs(ex));
				else
					throw;
			}
		}

		protected virtual void OnStart(EventArgs? e = null) => Start?.Invoke(this, e ?? EventArgs.Empty);

		protected virtual void OnReady(ReadyEventArgs e) => Ready?.Invoke(this, e);

		protected virtual void OnSetEntryDate(EntryEventArgs e) => SetEntryDate?.Invoke(this, e);

		protected virtual void OnCancel(CancelEventArgs e) => Cancel?.Invoke(this, e);

		protected virtual void OnComplete(EventArgs? e = null) => Complete?.Invoke(this, e ?? EventArgs.Empty);

		private static EntryDateInfo[] GetEntryDates(string ediPath) {
			try {
				return EdiFile.Load(ediPath).Data;
			}
			catch (Exception ex) {
				throw new InvalidDataException("Corrupted edi file", ex);
			}
		}

		private IEnumerable<(string Path, EntryDate NewDate)> Search(IEnumerable<string> paths, IEnumerable<EntryDateInfo> infos, bool appliesToChildren) {
			using var enumerator = infos.GetEnumerator();
			using var e = enumerator.ToExtended();
			if (!e.MoveNext())
				yield break;
			var general = e.Current!.General ? e.GetAndMoveNext() : null;
			if (e.Success && e.Current!.General)
				throw new InvalidOperationException(@"More than one general EntryDateInfo found");
			foreach (var path in paths) {
				var fileName = Path.GetFileName(path);
				while (e.Success && string.Compare(fileName, e.Current!.Path) > 0)
					e.MoveNext();
				if (!e.Success && general is null)
					break;
				var isDirectory = path.IsDirectory();
				EntryDateInfo? srcInfo = null;
				if (e.Success && fileName == e.Current!.Path && e.Current.IsDirectory == isDirectory)
					srcInfo = e.GetAndMoveNext();
				else if (general is not null) {
					if (general.IsDirectory is null || general.IsDirectory == isDirectory)
						srcInfo = general;
				}
				if (srcInfo is not null) {
					yield return (path, srcInfo.Value);
					if (appliesToChildren && isDirectory && srcInfo.IncludesChildren) {
						string[] entries = Directory.EnumerateFileSystemEntries(path).ToArray();
						if (entries.Length > 0) {
							Array.Sort(entries);
							foreach (var entry in Search(entries, srcInfo.Entries!, appliesToChildren))
								yield return entry;
						}
					}
				}
			}
		}
	}

	public class Generator {
		[Flags]
		public enum Flag : byte {
			None = 0,

			MatchBasePath = 1 << 0,

			DirectoryOnly = 1 << 1
		}

		public Generator(IEnumerable<string> paths) => Paths = paths.AsArray();

		public Generator(string path) : this(new[] { path }) { }

		public event EventHandler? Start;

		public event EventHandler<EntryEventArgs>? ReadEntryDate;

		public event EventHandler<CancelEventArgs>? Cancel;

		public event EventHandler? Complete;

		public string[] Paths { get; }

		public EntryDateInfo[]? EntryDates { get; private set; }

		public EntryDateFields Fields { get; set; } = EntryDateFields.All;

		public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

		public static void SaveToFile(string path, EntryDateInfo src) => SaveToFile(path, new[] { src });

		public static void SaveToFile(string path, EntryDateInfo[] srcs) => new EdiFile(srcs).Save(path);

		public void SaveToFile(string path) => SaveToFile(path, EntryDates ?? throw new InvalidOperationException("Generator not started yet."));

		public async Task<EntryDateInfo[]?> Generate(int maxDepth, Flag flag = Flag.MatchBasePath) {
			if (Paths.Length > 1)
				flag |= Flag.MatchBasePath;
			OnStart();
			try {
				EntryDates = await Task.WhenAll(
					Paths.Select(path => Generate(path, maxDepth, flag))
				);
				OnComplete();
				return EntryDates;
			}
			catch (TaskCanceledException ex) {
				OnCancel(new CancelEventArgs(ex));
				return null;
			}
			catch (AggregateException ex) {
				if (ex.InnerExceptions.Any(e => e is TaskCanceledException)) {
					OnCancel(new CancelEventArgs(ex));
					return null;
				}
				throw;
			}
		}

		public void OnStart(EventArgs? e = null) => Start?.Invoke(this, e ?? EventArgs.Empty);

		public void OnReadEntryDate(EntryEventArgs e) => ReadEntryDate?.Invoke(this, e);

		public void OnCancel(CancelEventArgs e) => Cancel?.Invoke(this, e);

		public void OnComplete(EventArgs? e = null) => Complete?.Invoke(this, e ?? EventArgs.Empty);

		private async Task<EntryDateInfo> Generate(string path, int maxDepth, Flag flag, uint depth = 0u) {
			var dates = await Task.Run(
				() => {
					var result = EntryDate.FromEntry(path, Fields);
					OnReadEntryDate(new EntryEventArgs(path, result));
					return result;
				},
				CancellationToken
			);
			IList<EntryDateInfo>? entries = null;
			if ((maxDepth < 0 || depth < maxDepth) && path.IsDirectory()) {
				entries = await Task.WhenAll(
					(flag.HasFlag(Flag.DirectoryOnly) ? Directory.EnumerateDirectories(path) : Directory.EnumerateFileSystemEntries(path))
					.Select(p => Generate(p, maxDepth, flag, depth + 1u))
				);
				Sort(entries, true);
			}
			return new EntryDateInfo(
				dates,
				flag.HasFlag(Flag.MatchBasePath) ? Path.GetFileName(path) : null,
				entries,
				flag.HasFlag(Flag.MatchBasePath) ? path.IsDirectory() : null
			);
		}
	}

	public class Synchronizer {
		public Synchronizer(IEnumerable<string> dsts, string src) {
			DestinationPaths = dsts.AsArray();
			SourcePath = src;
		}

		public Synchronizer(string dst, string src) : this(new[] { dst }, src) { }

		public event EventHandler? Start;

		public event EventHandler? ApplicationStart;

		public event EventHandler<ReadyEventArgs>? ApplicationReady;

		public event EventHandler<EntryEventArgs>? ApplicationSetEntryDate;

		public event EventHandler? Complete;

		public event EventHandler<CancelEventArgs>? Cancel;

		public string SourcePath { get; set; }

		public string[] DestinationPaths { get; set; }

		public EntryDateFields Fields { get; set; } = EntryDateFields.All;

		public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

		public async Task Synchronize(bool includesChildren = true, bool appliesToChildren = false) {
			var generator = new Generator(SourcePath) {
				Fields = Fields,
				CancellationToken = CancellationToken
			};
			generator.Start += Start;
			generator.Cancel += Cancel;
			var infos = await generator.Generate(
				includesChildren ? -1 : 0,
				Settings.Default.DirectoryOnly ? Generator.Flag.DirectoryOnly : Generator.Flag.None
			);
			if (infos is null)
				return;
			var applier = new Applier(DestinationPaths, infos);
			applier.Start += ApplicationStart;
			applier.Ready += ApplicationReady;
			applier.SetEntryDate += ApplicationSetEntryDate;
			applier.Complete += Complete;
			applier.Cancel += Cancel;
			await applier.ApplyAsync(appliesToChildren, CancellationToken);
		}
	}
}