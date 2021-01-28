using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SHDocVw;
using PInvoke;

namespace TestShellExtension {
    using static Environment;
    using Win32Exception = System.ComponentModel.Win32Exception;

    public static class ProcessUtilities {
        public static string GetCurrentDirectory(int processId) {
            return GetProcessParametersString(processId, Environment.Is64BitOperatingSystem ? 0x38 : 0x24);
        }

        public static string GetCurrentDirectory(this Process process) {
            if (process == null)
                throw new ArgumentNullException("process");

            return GetCurrentDirectory(process.Id);
        }

        public static string GetCommandLine(int processId) {
            return GetProcessParametersString(processId, Environment.Is64BitOperatingSystem ? 0x70 : 0x40);
        }

        public static string GetCommandLine(this Process process) {
            if (process == null)
                throw new ArgumentNullException("process");

            return GetCommandLine(process.Id);
        }

        private static string GetProcessParametersString(int processId, int offset) {
            IntPtr handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
            if (handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            int processParametersOffset = Environment.Is64BitOperatingSystem ? 0x20 : 0x10;
            try {
                if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) // are we running in WOW?
                {
                    PROCESS_BASIC_INFORMATION_WOW64 pbi = new PROCESS_BASIC_INFORMATION_WOW64();
                    int hr = NtWow64QueryInformationProcess64(handle, 0, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    long pp = 0;
                    hr = NtWow64ReadVirtualMemory64(handle, pbi.PebBaseAddress + processParametersOffset, ref pp, Marshal.SizeOf(pp), IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    UNICODE_STRING_WOW64 us = new UNICODE_STRING_WOW64();
                    hr = NtWow64ReadVirtualMemory64(handle, pp + offset, ref us, Marshal.SizeOf(us), IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    if ((us.Buffer == 0) || (us.Length == 0))
                        return null;

                    string s = new string('\0', us.Length / 2);
                    hr = NtWow64ReadVirtualMemory64(handle, us.Buffer, s, us.Length, IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    return s;
                }
                else // we are running with the same bitness as the OS, 32 or 64
                {
                    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                    int hr = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    IntPtr pp = new IntPtr();
                    if (!ReadProcessMemory(handle, pbi.PebBaseAddress + processParametersOffset, ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    UNICODE_STRING us = new UNICODE_STRING();
                    if (!ReadProcessMemory(handle, pp + offset, ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    if ((us.Buffer == IntPtr.Zero) || (us.Length == 0))
                        return null;

                    string s = new string('\0', us.Length / 2);
                    if (!ReadProcessMemory(handle, us.Buffer, s, new IntPtr(us.Length), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    return s;
                }
            }
            finally {
                CloseHandle(handle);
            }
        }

        private const int PROCESS_QUERY_INFORMATION = 0x400;
        private const int PROCESS_VM_READ = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        // for 32-bit process in a 64-bit OS only
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION_WOW64 {
            public long Reserved1;
            public long PebBaseAddress;
            public long Reserved2_0;
            public long Reserved2_1;
            public long UniqueProcessId;
            public long Reserved3;
        }

        // for 32-bit process in a 64-bit OS only
        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING_WOW64 {
            public short Length;
            public short MaximumLength;
            public long Buffer;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref IntPtr lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        // for 32-bit process in a 64-bit OS only
        [DllImport("ntdll.dll")]
        private static extern int NtWow64QueryInformationProcess64(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION_WOW64 ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, ref long lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, ref UNICODE_STRING_WOW64 lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);
    }
    class Program {
        public static IEnumerable<string> SelectedItemPaths;
        public static string Platform = "x64";
        public static string VSCommandPromptDirectory = Path.Combine(GetFolderPath(SpecialFolder.CommonPrograms), @"Visual Studio 2019\Visual Studio Tools\VC");
        public static string AnotherArchitecture(string arch) {
            if (arch == "x64")
                return "x86";
            else if (arch == "x86")
                return "x64";
            throw new ArgumentException("Wrong architecture name");
        }
        public static bool Register(string action, string architecture, out string errorMessage) {
            errorMessage = null;
            string[] links = Directory.GetFiles(VSCommandPromptDirectory);
            string target = null;
            foreach (string link in links) {
                string linkName = Path.GetFileName(link);
                if (linkName.Substring(0, 3) == Platform) {
                    if (architecture == Platform && linkName.IndexOf(AnotherArchitecture(architecture)) == -1) {
                        target = link;
                        break;
                    }
                    else if (architecture == AnotherArchitecture(Platform) && linkName.IndexOf(architecture) != -1) {
                        target = link;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(target))
                return false;
            string message = null;
            using (var process = Process.Start(new ProcessStartInfo {
                FileName = "cmd.exe",
                Arguments = $"/c \"{target}\"",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            })) {
                process.ErrorDataReceived += (sender, args) => message = args.Data;
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine($"Regasm.exe {(action == "Register" ? "/codebase" : "/u")} {SelectedItemPaths.First()}");
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }
            errorMessage = message;
            return message == null;
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        const int WM_USER = 0x0400; //http://msdn.microsoft.com/en-us/library/windows/desktop/ms644931(v=vs.85).aspx
        class ExplorerInfo {
            public string URL { get; }
            public User32.WINDOWPLACEMENT Placement { get; }
            public bool IsUsed { get; set; }
            public ExplorerInfo(InternetExplorer ie) {
                IntPtr hWnd = new IntPtr(ie.HWND);
                URL = ie.LocationURL;
                Placement = User32.GetWindowPlacement(hWnd);
                IsUsed = false;
            }
        }
        static void Main(string[] args) {
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
                    if (ptr.ToInt32() == 0) {
                        Console.WriteLine("Success.");
                        break;
                    }
                    Thread.Sleep(1000);
                } while (true);
            }
            catch (Exception ex) {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
            Console.WriteLine("Restarting the shell.");
            Process.Start(new ProcessStartInfo {
                FileName = Path.Combine(GetFolderPath(SpecialFolder.Windows),"explorer.exe"),
                UseShellExecute = true
            });
            foreach (var info in explorersInfo) {
                Process.Start(new ProcessStartInfo {
                    FileName = "explorer.exe",
                    Arguments = info.URL,
                    WindowStyle = ProcessWindowStyle.Minimized
                }).WaitForExit();
            }
            while (true) {
                int count = 0;
                shells = new ShellWindows().Cast<object>().ToArray();
                for (int i = 0; i < shells.Length; ++i) {
                    InternetExplorer ie = (InternetExplorer)shells[i];
                    if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower()=="explorer")
                        ++count;
                }
                if (count == explorersInfo.Count)
                    break;
                Thread.Sleep(1000);
            }
            Console.WriteLine("Resizing and moving the shells");
            for (int i = 0; i < shells.Length; ++i) {
                InternetExplorer ie = (InternetExplorer)shells[i];
                if (Path.GetFileNameWithoutExtension(ie.FullName).ToLower() == "explorer") {
                    try {
                        ExplorerInfo info = explorersInfo.First((ExplorerInfo i) => !i.IsUsed && ie.LocationURL == i.URL);
                        info.IsUsed = true;
                        IntPtr hWnd = new IntPtr(ie.HWND);
                        User32.SetWindowPlacement(hWnd, info.Placement);
                    }
                    catch { }
                }
            }
            Console.WriteLine("Process accomplished, press any key to exit");
            Console.ReadKey();
        }
    }
}
