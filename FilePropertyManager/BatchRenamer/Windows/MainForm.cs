using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BatchRenamer.Windows {
	public partial class MainForm : Form {
		/// <summary>
		/// </summary>
		/// <param name="entries">Absolute paths</param>
		public MainForm(List<string> entries) {
			InitializeComponent();
			Entries = entries;
		}

		public List<string> Entries { get; }

		public Regex Pattern {
			get {
				try {
					return new Regex(SearchTextBox.Text);
				}
				catch (ArgumentException) {
					return null;
				}
			}
		}

		private void SearchTextBoxTextChanged(object sender, EventArgs e) {
			if (SearchTextBox.BackColor != SystemColors.Window)
				SearchTextBox.BackColor = SystemColors.Window;
		}

		private void ReplaceTextBoxKeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '\n' && Pattern is not null) { }
		}
	}
}