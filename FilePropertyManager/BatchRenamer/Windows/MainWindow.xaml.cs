using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrueMogician.Extensions.Enumerable;

#nullable enable
namespace BatchRenamer.Windows {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public enum EntityType : byte {
			File,

			Directory
		}

		private Regex? _pattern;

		private double _windowHeight;

		public MainWindow(IEnumerable<string> entities) {
			Entities = entities.Select(Path.GetFullPath).ToList();
			EntitiesTypes = Entities.Select(
					e =>
						File.Exists(e) ? EntityType.File :
						Directory.Exists(e) ? EntityType.Directory : throw new FileNotFoundException(null, e)
				)
				.ToList();
			var tmp = Entities.IndexJoin(EntitiesTypes).ToList();
			tmp.Sort((a, b) => (int)b.Second - (int)a.Second);
			Entities = tmp.Select(x => x.First).ToList();
			EntitiesTypes = tmp.Select(x => x.Second).ToList();
			FileNames = Entities.Select(
					(e, i) =>
						EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e)
				)
				.ToList();
			InitializeComponent();
			Icon = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "MainWindow.png")));
			EntitiesBox.Document = new FlowDocument(EntitiesParagraph);
			ResultsBox.Document = new FlowDocument(ResultsParagraph);
		}

		public List<string> Entities { get; }

		public List<EntityType> EntitiesTypes { get; }

		public Regex? Pattern {
			get => _pattern;
			set {
				_pattern = value;
				PatternChanged(null, EventArgs.Empty);
			}
		}

		private Paragraph EntitiesParagraph { get; } = new();

		private Paragraph ResultsParagraph { get; } = new();

		private List<string> FileNames { get; set; }

		private List<MatchCollection>? MatchCollections { get; set; }

		private event EventHandler PatternChanged = delegate { };

		private string? GetNewEntity(int index) {
			if (MatchCollections![index].Count == 0)
				return null;
			string entity = Entities[index];
			var type = EntitiesTypes[index];
			string? source = FileNames[index];
			string replaced = MatchAll.IsChecked == true
				? Pattern!.Replace(source, ReplaceBox.Text)
				: Pattern!.Replace(source, ReplaceBox.Text, 1);
			return (IncludePath.IsChecked, IncludeExtension.IsChecked) switch {
				(true, true)   => replaced,
				(true, false)  => replaced + (type == EntityType.File ? Path.GetExtension(entity) : string.Empty),
				(false, true)  => Path.GetDirectoryName(entity) + Path.DirectorySeparatorChar + replaced,
				(false, false) => Path.GetDirectoryName(entity) + Path.DirectorySeparatorChar + replaced + (type == EntityType.File ? Path.GetExtension(entity) : string.Empty),
				_              => throw new ArgumentOutOfRangeException()
			};
		}

		private void Display(Paragraph target, bool highlightMatch = true) {
			target.Inlines.Clear();
			if (!highlightMatch) {
				foreach ((string? fileName, var type) in FileNames.IndexJoin(EntitiesTypes)) {
					var style = type == EntityType.Directory ? FontStyles.Italic : FontStyles.Normal;
					target.Inlines.Add(new Run(fileName) {FontStyle = style});
					target.Inlines.Add(new LineBreak());
				}
				return;
			}
			foreach ((var matches, int index) in MatchCollections.ToIndexed()) {
				if (matches.Count == 0)
					continue;
				string name = FileNames[index]!;
				var style = EntitiesTypes[index] == EntityType.Directory ? FontStyles.Italic : FontStyles.Normal;
				bool? valid = Equals(target, ResultsParagraph) ? PathExtensions.IsFullPathValid(GetNewEntity(index)!) : null;
				var baseColor = valid != false ? Brushes.Black : Brushes.Tomato;
				var highlightColor = valid != false ? Brushes.Aquamarine : Brushes.Gold;
				if (MatchAll.IsChecked == true) {
					var cursor = 0;
					for (var i = 0; i < matches.Count; ++i) {
						var match = matches[i];
						if (match.Index > cursor)
							target.Inlines.Add(new Run(name[cursor..match.Index]) {Foreground = baseColor, FontStyle = style});
						target.Inlines.Add(
							new Run(
									Equals(target, EntitiesParagraph)
										? match.Value
										: Pattern!.Replace(match.Value, ReplaceBox.Text)
								)
								{Foreground = highlightColor, FontStyle = style}
						);
						cursor = match.Index + match.Length;
					}

					if (cursor < name.Length)
						target.Inlines.Add(new Run(name[cursor..]) {Foreground = baseColor, FontStyle = style});
				}
				else {
					var match = matches[0];
					if (match.Index > 0)
						target.Inlines.Add(new Run(name[..match.Index]) {Foreground = baseColor, FontStyle = style});
					target.Inlines.Add(
						new Run(
								Equals(target, EntitiesParagraph)
									? match.Value
									: Pattern!.Replace(match.Value, ReplaceBox.Text)
							)
							{Foreground = highlightColor, FontStyle = style}
					);
					if (match.Index + match.Length < name.Length)
						target.Inlines.Add(new Run(name[(match.Index + match.Length)..]) {Foreground = baseColor, FontStyle = style});
				}
				target.Inlines.Add(new LineBreak() {Background = Brushes.Aqua});
			}
			foreach ((var matches, int index) in MatchCollections.ToIndexed()) {
				if (matches.Count > 0)
					continue;
				target.Inlines.Add(
					new Run(FileNames[index]!) {
						Foreground = Brushes.DarkGray,
						FontStyle = EntitiesTypes[index] == EntityType.Directory ? FontStyles.Italic : FontStyles.Normal
					}
				);
				target.Inlines.Add(new LineBreak() {Background = Brushes.Aqua});
			}
		}

		private void Apply() {
			for (var i = 0; i < Entities.Count; ++i) {
				string? entity = Entities[i];
				var type = EntitiesTypes[i];
				string? newEntity = GetNewEntity(i);
				if (newEntity is null || !PathExtensions.IsFullPathValid(newEntity))
					continue;
				Directory.CreateDirectory(Path.GetDirectoryName(newEntity)!);
				(type == EntityType.File ? (Action<string, string>)File.Move : Directory.Move)(entity, newEntity);
			}
		}

		private void RefreshPreview() {
			if (IsLoaded)
				EntitiesBox.Background = Pattern is null ? Brushes.Tomato : SystemColors.WindowBrush;
			if (Pattern is null) {
				Display(EntitiesParagraph, false);
				ResultsParagraph.Inlines.Clear();
			}
			else {
				Display(EntitiesParagraph);
				if (ApplyButton.IsEnabled)
					Display(ResultsParagraph);
				else
					ResultsParagraph.Inlines.Clear();
			}
		}

		private void UpdatePattern() {
			try {
				Pattern = new Regex(SearchBox.Text, IgnoreCase.IsChecked == true ? RegexOptions.IgnoreCase : RegexOptions.None);
			}
			catch (ArgumentException) {
				Pattern = null;
			}
		}

		private void OnPatternChanged(object sender, EventArgs args) {
			MatchCollections = Pattern is null ? null : FileNames.Select(e => Pattern!.Matches(e)).ToList();
			ReplaceBox.IsEnabled = ApplyButton.IsEnabled = Pattern is not null &&
				!string.IsNullOrEmpty(SearchBox.Text) &&
				MatchCollections?.Any(c => c.Count > 0) == true;
			SearchBox.Background = Pattern is null ? Brushes.Tomato : SystemColors.WindowBrush;
			if (ShowPreview.IsChecked == true && AutoPreview.IsChecked == true)
				RefreshPreview();
		}

		private void WindowLoaded(object sender, EventArgs e) {
			this.HideMinimizeAndMaximizeButtons();
			PatternChanged += OnPatternChanged;
		}

		private void IgnoreCaseChanged(object sender, RoutedEventArgs args) => UpdatePattern();

		private void TargetChanged(object sender, RoutedEventArgs args) {
			FileNames = (IncludePath.IsChecked, IncludeExtension.IsChecked) switch {
				(true, true) => Entities,
				(true, false) => Entities.Select(
						(e, i) => Path.Combine(
							Path.GetDirectoryName(e)!,
							EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e)
						)
					)
					.ToList(),
				(false, true) => Entities.Select(Path.GetFileName).ToList(),
				(false, false) => Entities.Select(
						(e, i) =>
							EntitiesTypes[i] == EntityType.File ? Path.GetFileNameWithoutExtension(e) : Path.GetFileName(e)
					)
					.ToList(),
				_ => throw new ArgumentOutOfRangeException()
			};
			UpdatePattern();
			if (Equals(sender, IncludePath)) {
				EntitiesBox.HorizontalScrollBarVisibility = ResultsBox.HorizontalScrollBarVisibility =
					IncludePath.IsChecked == true ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
				EntitiesBox.Document.PageWidth = ResultsBox.Document.PageWidth = IncludePath.IsChecked == true ? 1000 : double.NaN;
			}
		}

		private void MatchAllChanged(object sender, RoutedEventArgs args) {
			if (ShowPreview.IsChecked == true && AutoPreview.IsChecked == true)
				RefreshPreview();
		}

		private void ShowPreviewChecked(object sender, RoutedEventArgs args) {
			if (!IsLoaded)
				return;
			ResizeMode = ResizeMode.CanResize;
			Height = _windowHeight;
			PreviewContainer.Visibility = Visibility.Visible;
			PreviewRow.Height = new GridLength(1, GridUnitType.Star);
			AutoPreview.IsEnabled = true;
			PreviewButton.IsEnabled = AutoPreview.IsChecked != true;
		}

		private void ShowPreviewUnchecked(object sender, RoutedEventArgs args) {
			if (!IsLoaded)
				return;
			ResizeMode = ResizeMode.NoResize;
			_windowHeight = ActualHeight;
			Height = 210.6;
			PreviewContainer.Visibility = Visibility.Collapsed;
			PreviewRow.Height = GridLength.Auto;
			AutoPreview.IsEnabled = false;
			PreviewButton.IsEnabled = false;
		}

		private void AutoPreviewChecked(object sender, RoutedEventArgs e) {
			PreviewButton.IsEnabled = false;
			RefreshPreview();
		}

		private void AutoPreviewUnchecked(object sender, RoutedEventArgs args) {
			PreviewButton.IsEnabled = true;
		}

		private void SearchBoxTextChanged(object sender, TextChangedEventArgs e) => UpdatePattern();

		private void SearchBoxKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter && ApplyButton.IsEnabled)
				PreviewButtonClick(sender, args);
		}

		private void ReplaceBoxTextChanged(object sender, TextChangedEventArgs args) {
			if (ShowPreview.IsChecked == true && AutoPreview.IsChecked == true && Pattern is not null)
				Display(ResultsParagraph);
		}

		private void ReplaceBoxKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter && ApplyButton.IsEnabled)
				ApplyButtonClick(sender, args);
		}

		private void SyncScroll(object sender, ScrollChangedEventArgs args) {
			var target = Equals(sender, EntitiesBox) ? ResultsBox : EntitiesBox;
			target.ScrollToVerticalOffset(args.VerticalOffset);
			target.ScrollToHorizontalOffset(args.HorizontalOffset);
		}

		private void PreviewButtonClick(object sender, RoutedEventArgs e) => RefreshPreview();

		private void ApplyButtonClick(object sender, RoutedEventArgs e) {
			Apply();
			Close();
		}
	}

	public static class PathExtensions {
		private static readonly Regex FullPathPattern = new(@$"^[a-z]:(?:[\\/][^{string.Join("", Path.GetInvalidFileNameChars()).Replace("\\", "\\\\")}]+)+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static bool IsFullPathValid(string path) => FullPathPattern.IsMatch(path);
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
			var hWnd = new WindowInteropHelper(window).Handle;
			int currentStyle = GetWindowLong(hWnd, GWL_STYLE);

			SetWindowLong(hWnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
		}
	}
}