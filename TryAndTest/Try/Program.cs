using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Try {
    using static PInvoke.User32;
    class Program {

        static void Main() {
            Process.Start(new ProcessStartInfo {
                FileName = "explorer",
                Arguments = @"D:\Code",
                UseShellExecute = true
            });
            Process.Start(new ProcessStartInfo {
                FileName = "explorer",
                Arguments = @"C:\",
                UseShellExecute = true
            });
        }
    }
}
