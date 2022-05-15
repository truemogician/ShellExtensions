using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntryDateCopier.Properties;
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

		public async Task Apply(bool appliesToChildren = true, CancellationToken? cancellationToken = null) {
			cancellationToken ??= CancellationToken.None;
			var newDates = new Dictionary<string, EntryDate>();
			void Search(IEnumerable<string> paths, IEnumerable<EntryDateInfo> infos) {
				using var e = infos.GetEnumerator().ToExtended();
				if (!e.MoveNext())
					return;
				var general = e.Current!.General ? e.GetAndMoveNext() : null;
				if (e.Success && e.Current!.General)
					throw new InvalidOperationException(@"More than one general EntryDateInfo found");
				foreach (string path in paths) {
					string? fileName = Path.GetFileName(path);
					while (e.Success && string.Compare(fileName, e.Current!.Path) < 0)
						e.MoveNext();
					if (!e.Success && general is null)
						break;
					bool isDirectory = path.IsDirectory();
					EntryDateInfo? srcInfo = null;
					if (e.Success && fileName == e.Current!.Path && e.Current.IsDirectory == isDirectory)
						srcInfo = e.GetAndMoveNext();
					else if (general is not null)
						if (general.IsDirectory is null || general.IsDirectory == isDirectory)
							srcInfo = general;
					if (srcInfo is not null) {
						newDates[path] = srcInfo.Value;
						if (appliesToChildren && isDirectory && srcInfo.IncludesChildren) {
							string[] entries = Directory.EnumerateFileSystemEntries(path).ToArray();
							if (entries.Length > 0) {
								Array.Sort(entries);
								Search(entries, srcInfo.Entries!);
							}
						}
					}
				}
			}
			OnStart();
			Search(Paths, EntryDates);
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
	}

	public class Generator {
		public Generator(IEnumerable<string> paths) => Paths = paths.AsArray();

		public Generator(string path) : this(new[] { path }) { }

		public event EventHandler? Start;

		public event EventHandler<EntryEventArgs>? ReadEntryDate;

		public event EventHandler<CancelEventArgs>? Cancel;

		public event EventHandler? Complete;

		public string[] Paths { get; }

        public EntryDateInfo[]? EntryDates { get; private set; }

        public static void SaveToFile(string path, EntryDateInfo src) => SaveToFile(path, new[] { src });

		public static void SaveToFile(string path, EntryDateInfo[] srcs) => new EdiFile(srcs).Save(path);

		public void SaveToFile(string path) => SaveToFile(path, EntryDates ?? throw new InvalidOperationException("Generator not started yet."));

		public async Task<EntryDateInfo[]?> Generate(
			bool matchBasePath = true,
			bool includesChildren = false,
			bool directoryOnly = false,
			EntryDateFields fields = EntryDateFields.All,
			CancellationToken? cancellationToken = null
		) {
			cancellationToken ??= CancellationToken.None;
			if (Paths.Length > 1)
				matchBasePath = true;
			OnStart();
			try {
				EntryDates = await Task.WhenAll(
					Paths.Select(path => Generate(path, matchBasePath, directoryOnly, includesChildren, fields, cancellationToken.Value))
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

		private async Task<EntryDateInfo> Generate(
			string path,
			bool matchBasePath,
			bool includesChildren,
			bool directoryOnly,
			EntryDateFields fields,
			CancellationToken cancellationToken
		) {
			var dates = await Task.Run(
				() => {
					var result = EntryDate.FromEntry(path, fields);
					OnReadEntryDate(new EntryEventArgs(path, result));
					return result;
				},
				cancellationToken
			);
			IList<EntryDateInfo>? entries = null;
			if (includesChildren && path.IsDirectory()) {
				var dirInfo = new DirectoryInfo(path);
                entries = await Task.WhenAll(
                    (directoryOnly ? dirInfo.EnumerateDirectories() : dirInfo.EnumerateFileSystemInfos())
						.Select(info => Generate(info.FullName, true, directoryOnly, true, fields, cancellationToken))
				);
				Sort(entries, true);
			}
			return new EntryDateInfo(dates, matchBasePath ? Path.GetFileName(path) : null, entries, matchBasePath ? path.IsDirectory() : null);
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

		public async Task Synchronize(
			bool includesChildren = true,
			bool appliesToChildren = false,
			EntryDateFields fields = EntryDateFields.All,
			CancellationToken? cancellationToken = null
		) {
			var generator = new Generator(SourcePath);
			generator.Start += Start;
			generator.Cancel += Cancel;
			var infos = await generator.Generate(
                false,
                includesChildren,
                Settings.Default.DirectoryOnly,
                fields,
                cancellationToken
            );
			if (infos is null)
				return;
			var applier = new Applier(DestinationPaths, infos);
			applier.Start += ApplicationStart;
			applier.Ready += ApplicationReady;
			applier.SetEntryDate += ApplicationSetEntryDate;
			applier.Complete += Complete;
			applier.Cancel += Cancel;
			await applier.Apply(appliesToChildren, cancellationToken);
		}
	}
}