using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace RegistrationHandler; 

public partial class RegisterStatus : Form {
	internal readonly Process regasm;

	public RegisterStatus(string regasmPath, string arguments) {
		InitializeComponent();
		regasm = new Process {
			StartInfo = {
				FileName = regasmPath,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			}
		};
		regasm.Exited += (sender, e) => {
			bar.MarqueeAnimationSpeed = 0;
			string stdout = regasm.StandardOutput.ReadToEnd();
			string stderr = regasm.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(stdout) || !string.IsNullOrEmpty(stderr)) {
				Width = 500;
				Height = 320;
				bar.Width = 446;

				label1.Visible = true;
				label2.Visible = true;
				stdoutBox.Visible = true;
				stdoutBox.Text = stdout;
				stderrBox.Visible = true;
				stderrBox.Text = stderr;
			}
		};
	}

	private void RegisterStatusLoad(object sender, EventArgs e) {
		regasm.Start();
	}
}