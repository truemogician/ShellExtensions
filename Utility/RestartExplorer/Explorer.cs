using PInvoke;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RestartExplorer {
	public static partial class Explorer {
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint msg, IntPtr wParam, IntPtr lParam);

		internal const int WM_USER = 0x0400;

		public static async Task Restart() {
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
				PostMessage(ptr, WM_USER + 436, (IntPtr)0, (IntPtr)0);
				while (true) {
					ptr = User32.FindWindow("Shell_TrayWnd", null);
					if (ptr.ToInt32() == 0)
						break;
					await Task.Delay(TimeSpan.FromSeconds(1));
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
	}
}