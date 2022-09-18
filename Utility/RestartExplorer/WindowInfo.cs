using System;
using PInvoke;
using SHDocVw;

namespace RestartExplorer {
	static partial class Explorer {
		internal class WindowInfo {
			public string Location { get; }

			public User32.WINDOWPLACEMENT Placement { get; set; }

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