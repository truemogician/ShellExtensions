using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace PathSelector {
	public static class ExplorerSelector {
		private static Guid IShellFolderGuid = typeof(IShellFolder).GUID;

		[Flags]
		internal enum SHCONT : ushort {
			SHCONTF_CHECKING_FOR_CHILDREN = 0x0010,

			SHCONTF_FOLDERS = 0x0020,

			SHCONTF_NONFOLDERS = 0x0040,

			SHCONTF_INCLUDEHIDDEN = 0x0080,

			SHCONTF_INIT_ON_FIRST_NEXT = 0x0100,

			SHCONTF_NETPRINTERSRCH = 0x0200,

			SHCONTF_SHAREABLE = 0x0400,

			SHCONTF_STORAGE = 0x0800,

			SHCONTF_NAVIGATION_ENUM = 0x1000,

			SHCONTF_FASTITEMS = 0x2000,

			SHCONTF_FLATLIST = 0x4000,

			SHCONTF_ENABLE_ASYNC = 0x8000
		}

		[ComImport]
		[Guid("000214E6-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComConversionLoss]
		internal interface IShellFolder {
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void ParseDisplayName(IntPtr hwnd, [In] [MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, [Out] out uint pchEaten, [Out] out IntPtr ppidl, [In] [Out] ref uint pdwAttributes);

			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int EnumObjects([In] IntPtr hwnd, [In] SHCONT grfFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenumIDList);

			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int BindToObject([In] IntPtr pidl, [In] [MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] ref Guid riid, [Out] [MarshalAs(UnmanagedType.Interface)] out IShellFolder ppv);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void BindToStorage([In] ref IntPtr pidl, [In] [MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] ref Guid riid, out IntPtr ppv);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void CompareIDs([In] IntPtr lParam, [In] ref IntPtr pidl1, [In] ref IntPtr pidl2);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void GetAttributesOf([In] uint cidl, [In] IntPtr apidl, [In] [Out] ref uint rgfInOut);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void GetUIObjectOf([In] IntPtr hwndOwner, [In] uint cidl, [In] IntPtr apidl, [In] ref Guid riid, [In] [Out] ref uint rgfReserved, out IntPtr ppv);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void GetDisplayNameOf([In] ref IntPtr pidl, [In] uint uFlags, out IntPtr pName);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void SetNameOf([In] IntPtr hwnd, [In] ref IntPtr pidl, [In] [MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] uint uFlags, [Out] IntPtr ppidlOut);
		}

		[ComImport]
		[Guid("000214F2-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IEnumIDList {
			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int Next(uint celt, IntPtr rgelt, out uint pceltFetched);

			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int Skip([In] uint celt);

			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int Reset();

			[PreserveSig]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
		}

		public static void FileOrFolder(string path, bool edit = false) {
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			var pidl = PathToAbsolutePidl(path);
			try {
				SHOpenFolderAndSelectItems(pidl, null, edit);
			}
			finally {
				NativeMethods.ILFree(pidl);
			}
		}

		public static void FilesOrFolders(string parentDirectory, ICollection<string> fileNames) {
			if (fileNames == null)
				throw new ArgumentNullException(nameof(fileNames));
			if (fileNames.Count == 0)
				return;

			var parentPidl = PathToAbsolutePidl(parentDirectory);
			try {
				var parent = PidlToShellFolder(parentPidl);

				var filesPidl = new List<IntPtr>(fileNames.Count);
				foreach (string filename in fileNames)
					filesPidl.Add(GetShellFolderChildrenRelativePidl(parent, filename));

				try {
					SHOpenFolderAndSelectItems(parentPidl, filesPidl.ToArray(), false);
				}
				finally {
					foreach (var pidl in filesPidl)
						NativeMethods.ILFree(pidl);
				}
			}
			finally {
				NativeMethods.ILFree(parentPidl);
			}
		}

		public static void FilesOrFolders(params string[] paths) => FilesOrFolders((IEnumerable<string>)paths);

		public static void FilesOrFolders(IEnumerable<string> paths) => FilesOrFolders(PathToFileSystemInfo(paths));

		public static void FilesOrFolders(IEnumerable<FileSystemInfo> paths) {
			if (paths == null)
				throw new ArgumentNullException(nameof(paths));
			var explorerWindows = paths.GroupBy(p => Path.GetDirectoryName(p.FullName));
			foreach (var explorerWindowPaths in explorerWindows) {
				string parentDirectory = Path.GetDirectoryName(explorerWindowPaths.First().FullName);
				FilesOrFolders(parentDirectory, explorerWindowPaths.Select(fsi => fsi.Name).ToList());
			}
		}

		private static IntPtr GetShellFolderChildrenRelativePidl(IShellFolder parentFolder, string displayName) {
			uint pdwAttributes = 0;
			parentFolder.ParseDisplayName(IntPtr.Zero, null, displayName, out _, out var ppidl, ref pdwAttributes);
			return ppidl;
		}

		private static IntPtr PathToAbsolutePidl(string path) {
			var desktopFolder = NativeMethods.SHGetDesktopFolder();
			return GetShellFolderChildrenRelativePidl(desktopFolder, path);
		}

		private static IShellFolder PidlToShellFolder(IShellFolder parent, IntPtr pidl) {
			int result = parent.BindToObject(pidl, null, ref IShellFolderGuid, out var folder);
			Marshal.ThrowExceptionForHR(result);
			return folder;
		}

		private static IShellFolder PidlToShellFolder(IntPtr pidl)
			=> PidlToShellFolder(NativeMethods.SHGetDesktopFolder(), pidl);

		private static void SHOpenFolderAndSelectItems(IntPtr pidlFolder, IntPtr[] apidl, bool edit)
			=> NativeMethods.SHOpenFolderAndSelectItems(pidlFolder, apidl, edit ? 1 : 0);

		private static IEnumerable<FileSystemInfo> PathToFileSystemInfo(IEnumerable<string> paths) {
			foreach (string path in paths) {
				string fixedPath = path;
				if (fixedPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || fixedPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
					fixedPath = fixedPath.Remove(fixedPath.Length - 1);
				if (Directory.Exists(fixedPath))
					yield return new DirectoryInfo(fixedPath);
				else if (File.Exists(fixedPath))
					yield return new FileInfo(fixedPath);
				else
					throw new FileNotFoundException($"The specified file or folder doesn't exists : {fixedPath}", fixedPath);
			}
		}

		private static class NativeMethods {
			[DllImport("ole32.dll")]
			private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

			public static IBindCtx CreateBindCtx() {
				Marshal.ThrowExceptionForHR(CreateBindCtx(0, out var result));
				return result;
			}

			[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			private static extern int SHGetDesktopFolder([MarshalAs(UnmanagedType.Interface)] out IShellFolder ppshf);

			public static IShellFolder SHGetDesktopFolder() {
				Marshal.ThrowExceptionForHR(SHGetDesktopFolder(out var result));
				return result;
			}

			[DllImport("shell32.dll")]
			private static extern int SHOpenFolderAndSelectItems(
				[In] IntPtr pidlFolder,
				uint cidl,
				[In] [Optional] [MarshalAs(UnmanagedType.LPArray)]
				IntPtr[] apidl,
				int dwFlags
			);

			public static void SHOpenFolderAndSelectItems(IntPtr pidlFolder, IntPtr[] apidl, int dwFlags) {
				uint cidl = apidl != null ? (uint)apidl.Length : 0U;
				int result = SHOpenFolderAndSelectItems(pidlFolder, cidl, apidl, dwFlags);
				Marshal.ThrowExceptionForHR(result);
			}

			[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
			public static extern IntPtr ILCreateFromPath([In] [MarshalAs(UnmanagedType.LPWStr)] string pszPath);

			[DllImport("shell32.dll")]
			public static extern void ILFree([In] IntPtr pidl);
		}
	}
}