using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using PInvoke;
using SHDocVw;

namespace ExplorerRestarter {
	public static class Explorer {
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint msg, IntPtr wParam, IntPtr lParam);

		public static void Restart() {
			var explorerInfos = new List<WindowInfo>();
			var shells = new ShellWindows().Cast<InternetExplorer>().ToArray();
			foreach (var ie in shells) {
				if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower() != "explorer")
					continue;
				var hWnd = new IntPtr(ie.HWND);
				explorerInfos.Add(new WindowInfo(ie));
				User32.SendMessage(hWnd, User32.WindowMessage.WM_CLOSE, (IntPtr)0, (IntPtr)0);
			}
			try {
				var ptr = User32.FindWindow("Shell_TrayWnd", null);
				PostMessage(ptr, (int)User32.WindowMessage.WM_USER + 436, (IntPtr)0, (IntPtr)0);
				while (true) {
					ptr = User32.FindWindow("Shell_TrayWnd", null);
					if (ptr.ToInt32() == 0)
						break;
					Thread.Sleep(1000);
				}
			}
			catch (Exception) {
				//ignore
			}
			var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
			Process.Start(
				new ProcessStartInfo {
					FileName = explorerPath,
					UseShellExecute = false
				}
			);
			if (explorerInfos.Count > 0) {
				foreach (var info in explorerInfos) {
					Process.Start(
						new ProcessStartInfo {
							FileName = explorerPath,
							Arguments = $"\"{info.Location}\"",
							UseShellExecute = true,
							WindowStyle = ProcessWindowStyle.Minimized
						}
					);
				}
				shells = new ShellWindows().Cast<InternetExplorer>().ToArray();
				while (shells.Length < explorerInfos.Count) {
					Thread.Sleep(1000);
					shells = new ShellWindows().Cast<InternetExplorer>().ToArray();
				}
				foreach (var ie in shells) {
					if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower() != "explorer")
						continue;
					var current = explorerInfos.FirstOrDefault(info => ie.LocationURL == info.Location);
					if (current is null)
						continue;
					explorerInfos.Remove(current);
					var hWnd = new IntPtr(ie.HWND);
					User32.SetWindowPlacement(hWnd, current.Placement);
				}
			}
		}

		internal class WindowInfo {
			public string Location { get; }

			public User32.WINDOWPLACEMENT Placement { get; }

			public WindowInfo(InternetExplorer ie) {
				var hWnd = new IntPtr(ie.HWND);
				Location = ie.LocationURL;
				Placement = User32.GetWindowPlacement(hWnd);
				if (Placement.showCmd == User32.WindowShowStyle.SW_SHOWNORMAL) {
					User32.GetWindowRect(hWnd, out var actual);
					Placement = new User32.WINDOWPLACEMENT {
						length = Placement.length,
						flags = Placement.flags,
						showCmd = Placement.showCmd,
						ptMaxPosition = Placement.ptMaxPosition,
						ptMinPosition = Placement.ptMinPosition,
						rcNormalPosition = actual
					};
				}
			}
		}
	}
}