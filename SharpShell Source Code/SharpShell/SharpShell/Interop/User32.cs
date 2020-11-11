using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpShell.Interop
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        internal static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition,
           [In] ref MENUITEMINFO lpmii);

        [DllImport("user32.dll")]
        internal static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", SetLastError = true)] // SETLAST by us
        internal static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose parent window handle is to be retrieved.</param>
        /// <returns>If the window is a child window, the return value is a handle to the parent window. If the window is a top-level window with the WS_POPUP style, the return value is a handle to the owner window. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetParent(IntPtr hWnd);
        
        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; 


        public static int HighWord(int number)
        {
            return ((number & 0x80000000) == 0x80000000) ?
                (number >> 16) : ((number >> 16) & 0xffff);
        }

        public static int LowWord(int number)
        {
            return number & 0xffff;
        }
    }
}
