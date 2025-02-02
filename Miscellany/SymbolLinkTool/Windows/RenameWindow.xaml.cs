using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrueMogician.Extensions.Enumerable;

namespace SymbolLinkTool.Windows;


/// <summary>
/// Interaction logic for RenameWindow.xaml
/// </summary>
public partial class RenameWindow {
	public class FileStatus: INotifyPropertyChanged {
		private string _fileName;

		private bool _valid;

		public FileStatus(string fullPath) {
			OriginalPath = FullPath = fullPath;
			_fileName = Path.GetFileName(fullPath);
			DirectoryName = Path.GetDirectoryName(fullPath)!;
		}

		public string OriginalPath { get; }

		public string FullPath { get; private set; }

		public string DirectoryName { get; }

		public string FileName {
			get => _fileName;
			set {
				if (_fileName == value)
					return;
				_fileName = value;
				OnPropertyChanged();
				FullPath = Path.Combine(DirectoryName, value);
				OnPropertyChanged(nameof(FullPath));
				Valid = !File.Exists(FullPath);
			}
		}

		public bool Valid {
			get => _valid;
			private set {
				if (_valid == value)
					return;
				_valid = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public RenameWindow(IEnumerable<string> files) {
        FileStatusList = new ObservableCollection<FileStatus>(files.Select(f => new FileStatus(f)));
		var fileName = Files.SameOrDefault(Path.GetFileName)!;
		InitializeComponent();
		Title = Locale.Get("RenameWindowTitle");
		ApplyButton.Content = Locale.Get("Apply");
        FileNameTextBox.Text = fileName;
		var idx = fileName.LastIndexOf('.');
		FileNameTextBox.Focus();
		FileNameTextBox.Select(0, idx == -1 ? fileName.Length : idx);
		FileNameTextBox.TextChanged += FileNameBoxTextChanged;
	}

	public ObservableCollection<FileStatus> FileStatusList { get; }

	protected IEnumerable<string> Files => FileStatusList.Select(fs => fs.FullPath);

	public static void RenameHardLinks(IEnumerable<string> files) {
		try {
			var window = new RenameWindow(files);
			window.Show();
		}
		catch (Exception ex) {
			MessageBox.Show(ex.Message, "Error");
		}
	}

	private void FileNameBoxTextChanged(object sender, TextChangedEventArgs e) {
		if (FileNameTextBox.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
			FileNameTextBox.Foreground = System.Windows.Media.Brushes.Red;
			ApplyButton.IsEnabled = false;
		}
		else {
			FileNameTextBox.Foreground = SystemColors.WindowTextBrush;
			FileStatusList.ForEach(f => f.FileName = FileNameTextBox.Text);
			ApplyButton.IsEnabled = FileStatusList.All(f => f.Valid);
		}
	}

	private void ApplyButtonClicked(object sender, RoutedEventArgs e) {
		foreach (var f in FileStatusList)
			File.Move(f.OriginalPath, Path.Combine(f.DirectoryName, f.FileName));
		Close();
	}

	private void FileNameTextBoxKeyDown(object sender, KeyEventArgs e) {
		if (e.Key == Key.Enter && ApplyButton.IsEnabled)
			ApplyButtonClicked(sender, e);
	}
}