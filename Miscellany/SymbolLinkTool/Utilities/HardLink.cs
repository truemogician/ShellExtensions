using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace SymbolLinkTool.Utilities {
	public static class HardLink {
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool CreateHardLink(
			string lpFileName,
			string lpExistingFileName,
			IntPtr lpSecurityAttributes
		);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern SafeFileHandle CreateFile(
			string lpFileName,
			[MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
			[MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
			IntPtr lpSecurityAttributes,
			[MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
			[MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
			IntPtr hTemplateFile
		);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation lpFileInformation);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(SafeHandle hObject);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindFirstFileNameW(
			string lpFileName,
			uint dwFlags,
			ref uint stringLength,
			StringBuilder fileName
		);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool FindNextFileNameW(
			IntPtr hFindStream,
			ref uint stringLength,
			StringBuilder fileName
		);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FindClose(IntPtr fFindHandle);

		[DllImport(@"shlwapi.dll", CharSet = CharSet.Auto)]
		private static extern bool PathAppend([In] [Out] StringBuilder pszPath, string pszMore);

		public static void Create(string hardLinkPath, string sourcePath) {
			if (!CreateHardLink(hardLinkPath, sourcePath, IntPtr.Zero))
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
		}

		public static int GetFileLinkCount(string filepath) {
			var result = 0;
			var handle = CreateFile(filepath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);
			if (GetFileInformationByHandle(handle, out var fileInfo))
				result = (int)fileInfo.NumberOfLinks;
			CloseHandle(handle);
			return result;
		}

		public static IEnumerable<string> GetFileSiblingHardLinks(string filePath) {
			string volume = Path.GetPathRoot(filePath);
			var sb = new StringBuilder(256);
			var stringLength = 256u;
			var findHandle = FindFirstFileNameW(filePath, 0, ref stringLength, sb);
			if (findHandle.ToInt64() != -1) {
				do {
					var pathSb = new StringBuilder(volume, 256);
					PathAppend(pathSb, sb.ToString());
					yield return pathSb.ToString();
					sb.Length = 0;
					stringLength = 256;
				} while (FindNextFileNameW(findHandle, ref stringLength, sb));
				FindClose(findHandle);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ByHandleFileInformation {
			public uint FileAttributes;

			public FileTime CreationTime;

			public FileTime LastAccessTime;

			public FileTime LastWriteTime;

			public uint VolumeSerialNumber;

			public uint FileSizeHigh;

			public uint FileSizeLow;

			public uint NumberOfLinks;

			public uint FileIndexHigh;

			public uint FileIndexLow;
		}
	}
}