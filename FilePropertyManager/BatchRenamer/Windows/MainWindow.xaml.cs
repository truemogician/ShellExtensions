using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TrueMogician.Extensions.Enumerable;

#nullable enable
namespace BatchRenamer.Windows {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		private IEnumerable<MatchCollection>? _matchCollections;

		private (string?, Regex?) _patternCache;

		public MainWindow(IEnumerable<string> entities) {
			Entities = entities.AsList();
			EntitiesTypes = Entities.Select(e => File.Exists(e) ? EntityType.File : Directory.Exists(e) ? EntityType.Directory : throw new FileNotFoundException(null, e)).ToList();
			var tmp = Entities.IndexJoin(EntitiesTypes).ToList();
			tmp.Sort((a, b) => (int)b.Second - (int)a.Second);
			Entities = tmp.Select(x => x.First).ToList();
			EntitiesTypes = tmp.Select(x => x.Second).ToList();
			FileNames = Entities.Select((e, i) => EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e)).ToList();
			InitializeComponent();
			EntitiesBox.Document = new FlowDocument(EntitiesParagraph);
			ResultsBox.Document = new FlowDocument(ResultsParagraph);
		}

		public List<string> Entities { get; }

		public List<EntityType> EntitiesTypes { get; }

		public Regex? Pattern {
			get {
				if (SearchBox.Text != _patternCache.Item1)
					try {
						_patternCache = (SearchBox.Text, new Regex(SearchBox.Text));
					}
					catch (ArgumentException) {
						_patternCache = (SearchBox.Text, null);
					}

				return _patternCache.Item2;
			}
		}

		public bool PatternValid => Pattern is not null;

		public bool PreviewButtonEnabled => AutoPreview?.IsEnabled == false || AutoPreview?.IsChecked == false;

		private Paragraph EntitiesParagraph { get; } = new();

		private Paragraph ResultsParagraph { get; } = new();

		private List<string> FileNames { get; set; }

		public IEnumerable<MatchCollection> MatchCollections
			=> SearchBox.Text == _patternCache.Item1 && _matchCollections is not null
				? _matchCollections
				: _matchCollections = FileNames.Select(e => Pattern!.Matches(e));

		private void Preview(Paragraph target, IEnumerable<MatchCollection> matchCollections) {
			target.Inlines.Clear();
			var collections = matchCollections.AsList();
			foreach ((MatchCollection matches, int index) in collections.ToIndexed()) {
				if (matches.Count == 0)
					continue;
				string name = FileNames[index]!;
				var style = EntitiesTypes[index] == EntityType.Directory ? FontStyles.Italic : FontStyles.Normal;
				if (MatchAll.IsChecked == true) {
					var cursor = 0;
					for (var i = 0; i < matches.Count; ++i) {
						var match = matches[i];
						if (match.Index > cursor)
							target.Inlines.Add(new Run(name[cursor..match.Index]) {FontStyle = style});
						target.Inlines.Add(new Run(Equals(target, EntitiesParagraph) ? match.Value : Pattern!.Replace(match.Value, ReplaceBox.Text)) {Foreground = Brushes.Aquamarine, FontStyle = style});
						cursor = match.Index + match.Length;
					}
					if (cursor < name.Length)
						target.Inlines.Add(new Run(name[cursor..]) {FontStyle = style});
				}
				else {
					var match = matches[0];
					if (match.Index > 0)
						target.Inlines.Add(new Run(name[..match.Index]) {FontStyle = style});
					target.Inlines.Add(new Run(Equals(target, EntitiesParagraph) ? match.Value : Pattern!.Replace(match.Value, ReplaceBox.Text)) {Foreground = Brushes.Aquamarine, FontStyle = style});
					if (match.Index + match.Length < name.Length)
						target.Inlines.Add(new Run(name[(match.Index + match.Length)..]) {FontStyle = style});
				}
				target.Inlines.Add(new LineBreak());
			}
			foreach ((MatchCollection matches, int index) in collections.ToIndexed()) {
				if (matches.Count > 0)
					continue;
				target.Inlines.Add(new Run(FileNames[index]!) {Foreground = Brushes.Tomato, FontStyle = EntitiesTypes[index] == EntityType.Directory ? FontStyles.Italic : FontStyles.Normal});
				target.Inlines.Add(new LineBreak());
			}
		}

		private void Apply() {
			foreach ((_, int index) in MatchCollections.ToIndexed().Where(pair => pair.Value.Count > 0)) {
				var entity = Entities[index];
				var type = EntitiesTypes[index];
				var source = FileNames[index];
				var replaced = MatchAll.IsChecked == true
					? Pattern!.Replace(source, ReplaceBox.Text, 1)
					: Pattern!.Replace(source, ReplaceBox.Text);
				var target = (IncludePath.IsChecked, IncludeExtension.IsChecked) switch {
					(true, true)   => replaced,
					(true, false)  => replaced + (type == EntityType.File ? Path.GetExtension(entity) : string.Empty),
					(false, true)  => Path.Combine(Path.GetDirectoryName(entity)!, replaced),
					(false, false) => Path.Combine(Path.GetDirectoryName(entity)!, replaced + (type == EntityType.File ? Path.GetExtension(entity) : string.Empty)),
					_              => throw new ArgumentOutOfRangeException()
				};
				if (IncludeExtension.IsChecked == true)
					Directory.CreateDirectory(Path.GetDirectoryName(target)!);
				(type == EntityType.File ? (Action<string, string>)File.Move : Directory.Move)(entity, target);
			}
		}

		private void RefreshPreview() {
			if (Pattern is null) {
				EntitiesParagraph.Inlines.Clear();
				ResultsParagraph.Inlines.Clear();
			}
			else {
				Preview(EntitiesParagraph, MatchCollections);
				if (!string.IsNullOrEmpty(ReplaceBox.Text))
					Preview(ResultsParagraph, MatchCollections);
			}
		}

		private void WindowLoaded(object sender, EventArgs e) => this.HideMinimizeAndMaximizeButtons();

		private void SearchBoxTextChanged(object sender, TextChangedEventArgs args) {
			if (RegexCheck.IsChecked == true)
				SearchBox.Background = Pattern is null ? Brushes.Tomato : SystemColors.WindowBrush;
			if (ShowPreview.IsChecked == true && AutoPreview.IsChecked == true)
				RefreshPreview();
		}

		private void ReplaceBoxTextChanged(object sender, TextChangedEventArgs args) {
			if (ShowPreview.IsChecked == true && AutoPreview.IsChecked == true && Pattern is not null)
				Preview(ResultsParagraph, MatchCollections);
		}

		private void SyncScroll(object sender, ScrollChangedEventArgs args) {
			var target = Equals(sender, EntitiesBox) ? ResultsBox : EntitiesBox;
			target.ScrollToVerticalOffset(args.VerticalOffset);
			target.ScrollToHorizontalOffset(args.HorizontalOffset);
		}

		private void SearchBoxKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter && ApplyButton.IsEnabled)
				PreviewButtonClick(sender, args);
		}

		private void ReplaceBoxKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter && ApplyButton.IsEnabled)
				ApplyButtonClick(sender, args);
		}

		private void TargetChanged(object sender, RoutedEventArgs args) {
			FileNames = (IncludePath.IsChecked, IncludeExtension.IsChecked) switch {
				(true, true)   => Entities,
				(true, false)  => Entities.Select((e, i) => Path.Combine(Path.GetDirectoryName(e)!, EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e))).ToList(),
				(false, true)  => Entities.Select(Path.GetFileName).ToList(),
				(false, false) => Entities.Select((e, i) => EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e)).ToList(),
				_              => throw new ArgumentOutOfRangeException()
			};
			if (ShowPreview.IsChecked == true)
				RefreshPreview();
		}

		private void MatchAllChanged(object sender, RoutedEventArgs args) {
			if (ShowPreview.IsChecked == true)
				RefreshPreview();
		}

		private void RegexCheckChecked(object sender, RoutedEventArgs e) => SearchBox.Background = Pattern is null ? Brushes.Tomato : SystemColors.WindowBrush;

		private void RegexCheckUnchecked(object sender, RoutedEventArgs e) => SearchBox.Background = SystemColors.WindowBrush;

		private void ShowPreviewChecked(object sender, RoutedEventArgs args) {
			ResizeMode = ResizeMode.CanResize;
			PreviewContainer.Visibility = Visibility.Visible;
			PreviewRow.Height = new GridLength(1, GridUnitType.Star);
			AutoPreview.IsEnabled = true;
		}

		private void ShowPreviewUnchecked(object sender, RoutedEventArgs args) {
			ResizeMode = ResizeMode.NoResize;
			PreviewContainer.Visibility = Visibility.Collapsed;
			PreviewRow.Height = GridLength.Auto;
			AutoPreview.IsEnabled = false;
		}

		private void AutoPreviewChecked(object sender, RoutedEventArgs e) {
			RefreshPreview();
		}

		private void PreviewButtonClick(object sender, RoutedEventArgs e) => RefreshPreview();

		private void ApplyButtonClick(object sender, RoutedEventArgs e) => Apply();

		public enum EntityType : byte {
			File,

			Directory
		}
	}

	internal static class WindowExtensions {
		private const int GWL_STYLE = -16,
			WS_MAXIMIZEBOX = 0x10000,
			WS_MINIMIZEBOX = 0x20000;

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int index);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int index, int value);

		internal static void HideMinimizeAndMaximizeButtons(this Window window) {
			var hWnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int currentStyle = GetWindowLong(hWnd, GWL_STYLE);

			SetWindowLong(hWnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
		}
	}
}