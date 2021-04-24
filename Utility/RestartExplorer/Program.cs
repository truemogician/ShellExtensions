using PInvoke;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace RestartExplorer {
	class Program {
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);
		const int WM_USER = 0x0400;
		class ExplorerInfo {
			public string Location { get; }
			public User32.WINDOWPLACEMENT Placement { get; set; }
			public bool IsUsed { get; set; }
			public ExplorerInfo(InternetExplorer ie) {
				IntPtr hWnd = new IntPtr(ie.HWND);
				Location = ie.LocationURL;
				Placement = User32.GetWindowPlacement(hWnd);
				if (Placement.showCmd == User32.WindowShowStyle.SW_SHOWNORMAL) {
					User32.GetWindowRect(hWnd, out RECT actual);
					Placement = new User32.WINDOWPLACEMENT {
						length = Placement.length,
						flags = Placement.flags,
						showCmd = Placement.showCmd,
						ptMaxPosition = Placement.ptMaxPosition,
						ptMinPosition = Placement.ptMinPosition,
						rcNormalPosition = actual
					};
				}
				IsUsed = false;
			}
		}
		static void Main() {
			List<ExplorerInfo> explorersInfo = new List<ExplorerInfo>();
			var shells = new ShellWindows().Cast<object>().ToArray();
			for (int i = 0; i < shells.Length; ++i) {
				InternetExplorer ie = (InternetExplorer)shells[i];
				if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower() == "explorer") {
					var hWnd = new IntPtr(ie.HWND);
					explorersInfo.Add(new ExplorerInfo(ie));
					User32.SendMessage(hWnd, User32.WindowMessage.WM_CLOSE, (IntPtr)0, (IntPtr)0);
				}
			}
			try {
				var ptr = User32.FindWindow("Shell_TrayWnd", null);
				PostMessage(ptr, WM_USER + 436, (IntPtr)0, (IntPtr)0);
				do {
					ptr = User32.FindWindow("Shell_TrayWnd", null);
					if (ptr.ToInt32() == 0)
						break;
					Thread.Sleep(1000);
				} while (true);
			}
			catch (Exception ex) {
				Console.WriteLine($"{ex.Message} {ex.StackTrace}");
			}
			Process.Start(new ProcessStartInfo {
				FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
				UseShellExecute = true
			});
			foreach (var info in explorersInfo) {
				var cmd = Process.Start(new ProcessStartInfo {
					FileName = "cmd",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardInput = true
				});

				cmd.StandardInput.WriteLine(string.Format("start /min \"\" \"{0}\"", string.IsNullOrEmpty(info.Location) ? "userinit" : info.Location));
				cmd.StandardInput.WriteLine("exit");
				cmd.WaitForExit();
			}
			shells = new ShellWindows().Cast<object>().ToArray();
			while (shells.Length < explorersInfo.Count) {
				Thread.Sleep(1000);
				shells = new ShellWindows().Cast<object>().ToArray();
			}
			for (int i = 0; i < shells.Length; ++i) {
				InternetExplorer ie = (InternetExplorer)shells[i];
				if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower() == "explorer") {
					try {
						ExplorerInfo info = explorersInfo.First((ExplorerInfo expInfo) => !expInfo.IsUsed && ie.LocationURL == expInfo.Location);
						info.IsUsed = true;
						IntPtr hWnd = new IntPtr(ie.HWND);
						User32.SetWindowPlacement(hWnd, info.Placement);
					}
					catch { }
				}
			}
		}
	}
}
