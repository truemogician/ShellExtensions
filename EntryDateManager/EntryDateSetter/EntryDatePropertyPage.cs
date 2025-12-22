using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using EntryDateUtility;
using Extension.Forms;
using SharpShell.SharpPropertySheet;

namespace EntryDateSetter;

public partial class EntryDateInformationPropertyPage : SharpPropertyPage {
	#region Constant Fields
	protected static readonly Dictionary<DateTimeComponent, Func<DateTime, int>> GetDateTimeComponent = new() {
		{ DateTimeComponent.Year, datetime => datetime.Year },
		{ DateTimeComponent.Month, datetime => datetime.Month },
		{ DateTimeComponent.Day, datetime => datetime.Day },
		{ DateTimeComponent.Hour, datetime => datetime.Hour },
		{ DateTimeComponent.Minute, datetime => datetime.Minute },
		{ DateTimeComponent.Second, datetime => datetime.Second }
	};

	protected static readonly Dictionary<DateTimeComponent, Func<DateTime, int, DateTime>> SetDateTimeComponent = new() {
		{ DateTimeComponent.Year, (datetime, value) => datetime.AddYears(value - datetime.Year) },
		{ DateTimeComponent.Month, (datetime, value) => datetime.AddMonths(value - datetime.Month) },
		{ DateTimeComponent.Day, (datetime, value) => datetime.AddDays(value - datetime.Day) },
		{ DateTimeComponent.Hour, (datetime, value) => datetime.AddHours(value - datetime.Hour) },
		{ DateTimeComponent.Minute, (datetime, value) => datetime.AddMinutes(value - datetime.Minute) },
		{ DateTimeComponent.Second, (datetime, value) => datetime.AddSeconds(value - datetime.Second) }
	};

	protected static readonly ResourceManager Locale = new("EntryDateSetter.Locales.Text", Assembly.GetExecutingAssembly());

	protected static readonly ResourceManager Style = new("EntryDateSetter.Locales.Style", Assembly.GetExecutingAssembly());
	#endregion

	#region Fields
	protected string[] Paths;

	protected DateTimeExtended[]? Current;

	protected Label[] Labels;

	protected DateTimePickerExtended[] Pickers;

	protected CheckBox[] CheckBoxes;

	protected Button[] Buttons;

	protected int[] MappedIndex = [0, 1, 2];
	#endregion

	#region Constructors
	public EntryDateInformationPropertyPage(string path) : this([path]) { }

	public EntryDateInformationPropertyPage(IEnumerable<string> paths) {
		InitializeComponent();
		Paths = paths.ToArray();
		if (Paths.Length == 0)
			throw new ArgumentNullException(nameof(paths), Locale.GetString("EmptyPathErr"));
		Labels = [lastAccessDateLabel, lastWriteDateLabel, creationDateLabel];
		Pickers = [lastAccessDatePicker, lastWriteDatePicker, creationDatePicker];
		CheckBoxes = [lastAccessDateCheckBox, lastWriteDateCheckBox, creationDateCheckBox];
		Buttons = [setToNowButton, exchangeDateButton, restoreDateButton];
		ApplyLocale();
	}
	#endregion

	#region Events
	public event DateTimePickedEventHandler Pick = delegate { };
	#endregion

	#region Methods
	private void ApplyLocale() {
		PageTitle = Locale.GetString("Title");
		string? colon = Locale.GetString("Colon");
		lastAccessDateLabel.Text = Locale.GetString("LastAccessDate") + colon;
		lastWriteDateLabel.Text = Locale.GetString("LastWriteDate") + colon;
		creationDateLabel.Text = Locale.GetString("CreationDate") + colon;
		lastAccessDateCheckBox.Text = Locale.GetString("LastAccessDate");
		lastWriteDateCheckBox.Text = Locale.GetString("LastWriteDate");
		creationDateCheckBox.Text = Locale.GetString("CreationDate");
		setToNowButton.Text = Locale.GetString("SetToNow");
		exchangeDateButton.Text = Locale.GetString("ExchangeDate");
		restoreDateButton.Text = Locale.GetString("RestoreDate");

		string? family = Style.GetString("FontFamily");
		foreach (var label in Labels)
			label.Font = new Font(family, float.Parse(Style.GetString("Label.FontSize")!));
		foreach (var checkbox in CheckBoxes)
			checkbox.Font = new Font(family, float.Parse(Style.GetString("CheckBox.FontSize")!));
		foreach (var button in Buttons)
			button.Font = new Font(family, float.Parse(Style.GetString("Button.FontSize")!));
	}

	protected static void UpdateUnknownComponent(DateTime original, DateTime @new, ref DateTimeComponent unknown) {
		var comp = DateTimeComponent.Year;
		do {
			if (!unknown.HasFlag(comp) && GetDateTimeComponent[comp](original) != GetDateTimeComponent[comp](@new))
				unknown |= comp;
			comp = (DateTimeComponent)((int)comp << 1);
		} while (comp <= DateTimeComponent.Second);
	}

	protected static DateTime[] GetDateTimes(string path) {
		var info = new EntryDateInfo(path);
		info.GetTimes(out var c, out var a, out var w);
		return [a, w, c];
	}

	protected static void SetDateTimes(string path, DateTime?[] times) {
		if (times.Length != 3)
			throw new ArgumentException();
		var info = new EntryDateInfo(path);
		info.SetTimes(times[2], times[0], times[1]);
	}

	protected void OnPick(DateTimePickedEventArgs e) {
		int i = e.PickerIndex;
		bool different = Pickers[i].Value != Current?[i].Value || Pickers[i].UnknownComponent != Current[i].Unknown;
		SetPageDataChanged(different);
		restoreDateButton.Enabled = different;
		Pick(this, e);
	}

	protected override void OnPropertyPageInitialised(SharpPropertySheet parent) {
		DateTimeComponent[] unknowns = [0, 0, 0];
		string path = Paths[0];
		if (!File.Exists(path) && !Directory.Exists(path))
			throw new FileNotFoundException(Locale.GetString("PathNotFoundErr"), path);
		var times = GetDateTimes(path);
		for (int i = 0; i < 3; ++i)
			Pickers[i].Value = times[i];
		for (var i = 1; i < Paths.Length; ++i) {
			if (!File.Exists(path) && !Directory.Exists(path))
				throw new FileNotFoundException(Locale.GetString("PathNotFoundErr"), path);
			times = GetDateTimes(path);
			for (var j = 0; j < 3; ++j)
				UpdateUnknownComponent(Pickers[j].Value, times[i], ref unknowns[j]);
		}
		Current = new DateTimeExtended[3];
		for (var i = 0; i < 3; ++i) {
			Pickers[i].UnknownComponent = unknowns[i];
			Current[i] = new DateTimeExtended(Pickers[i].Value, unknowns[i]);
		}
	}

	protected override void OnPropertySheetApply() {
		try {
			var originals = new DateTime[3];
			foreach (string path in Paths) {
				var times = GetDateTimes(path);
				for (var i = 0; i < 3; ++i) {
					if (!File.Exists(path) && !Directory.Exists(path))
						throw new FileNotFoundException(Locale.GetString("PathNotFoundErr"), path);
					originals[i] = times[MappedIndex[i]];
				}
				var newTimes = new DateTime?[3];
				for (var i = 0; i < 3; ++i) {
					if (!Pickers[i].HasUnknown)
						newTimes[i] = Pickers[i].Value;
					else {
						var @new = Pickers[i].Value;
						var comp = DateTimeComponent.Year;
						do {
							if (Pickers[i].UnknownComponent.HasFlag(comp))
								@new = SetDateTimeComponent[comp](@new, GetDateTimeComponent[comp](originals[i]));
							comp = (DateTimeComponent)((int)comp << 1);
						} while (comp <= DateTimeComponent.Second);
						newTimes[i] = @new;
					}
				}
				SetDateTimes(path, newTimes);
			}
			for (var i = 0; i < 3; ++i) {
				Current![i] = new DateTimeExtended(Pickers[i].Value, Pickers[i].UnknownComponent);
				MappedIndex[i] = i;
			}
			restoreDateButton.Enabled = false;
		}
		catch (Exception ex) {
			MessageBox.Show(ex.Message);
		}
	}

	protected override void OnPropertySheetOK() {
		if (restoreDateButton.Enabled)
			OnPropertySheetApply();
	}

	private void DateTimePickerOnValueChanged(object sender, EventArgs e) {
		if (Current is null)
			return;
		for (var i = 0; i < 3; ++i) {
			if (sender as DateTimePickerExtended == Pickers[i])
				OnPick(new DateTimePickedEventArgs(i));
		}
	}

	private void CheckBoxOnCheckedChanged(object sender, EventArgs e) {
		exchangeDateButton.Enabled = CheckBoxes.Sum(checkBox => checkBox.Checked ? 1 : 0) == 2;
	}

	private void SetToNowButtonOnClick(object sender, EventArgs e) {
		for (var i = 0; i < 3; ++i) {
			if (CheckBoxes[i].Checked) {
				Pickers[i].Value = DateTime.Now;
				Pickers[i].UnknownComponent = DateTimeComponent.None;
			}
		}
	}

	private void ExchangeDateButtonOnClick(object sender, EventArgs e) {
		int i, j;
		for (i = -1, j = 0; j < 3; ++j) {
			if (CheckBoxes[j].Checked) {
				if (i == -1)
					i = j;
				else
					break;
			}
		}
		var temp = new DateTimeExtended(Pickers[i].Value, Pickers[i].UnknownComponent);
		Pickers[i].Value = Pickers[j].Value;
		Pickers[i].UnknownComponent = Pickers[j].UnknownComponent;
		Pickers[j].Value = temp.Value;
		Pickers[j].UnknownComponent = temp.Unknown;
		int tmp = MappedIndex[i];
		MappedIndex[i] = MappedIndex[j];
		MappedIndex[j] = tmp;
	}

	private void RecoverDateButtonOnClick(object sender, EventArgs e) {
		for (var i = 0; i < 3; ++i) {
			if (CheckBoxes[i].Checked) {
				Pickers[i].Value = Current![i].Value;
				Pickers[i].UnknownComponent = Current[i].Unknown;
			}
		}
	}
	#endregion
}

public struct DateTimeExtended(DateTime value, DateTimeComponent unknown) {
	public DateTime Value = value;

	public DateTimeComponent Unknown = unknown;
}

public sealed class DateTimePickedEventArgs : EventArgs {
	internal DateTimePickedEventArgs(int index) => PickerIndex = index;

	[Range(0, 2)]
	internal int PickerIndex { get; }
}

public delegate void DateTimePickedEventHandler(object sender, DateTimePickedEventArgs e);