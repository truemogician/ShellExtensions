using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EntryDateUtility;

public sealed class EntryMetadata : IEquatable<EntryMetadata> {
	private const uint FILE_READ_ATTRIBUTES = 0x0080;
	private const uint FILE_WRITE_ATTRIBUTES = 0x0100;
	private const uint FILE_SHARE_READ = 1;
	private const uint FILE_SHARE_WRITE = 2;
	private const uint FILE_SHARE_DELETE = 4;
	private const uint OPEN_EXISTING = 3;
	private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

	public EntryMetadata(string path) {
		if (path is null)
			throw new ArgumentNullException(nameof(path));
		FullName = Path.GetFullPath(path);
	}

	public EntryMetadata(FileSystemInfo info) {
		FullName = info.FullName;
	}

	public string FullName { get; }

	public FileAttributes Attributes {
		get {
			using var handle = OpenHandle(FILE_READ_ATTRIBUTES);
			if (!GetFileInformationByHandle(handle, out var info))
				ThrowLastWin32Error();
			return (FileAttributes)info.dwFileAttributes;
		}
		set {
			// SetFileAttributes is path-based. 
			// If you need to set attributes via handle to respect specific sharing 
			// modes, you would need SetFileInformationByHandle (Vista+).
			if (!SetFileAttributes(FullName, (uint)value))
				ThrowLastWin32Error();
		}
	}

	public DateTime CreationTime {
		get => CreationTimeUtc.ToLocalTime();
		set => CreationTimeUtc = value.ToUniversalTime();
	}

	public DateTime LastAccessTime {
		get => LastAccessTimeUtc.ToLocalTime();
		set => LastAccessTimeUtc = value.ToUniversalTime();
	}

	public DateTime LastWriteTime {
		get => LastWriteTimeUtc.ToLocalTime();
		set => LastWriteTimeUtc = value.ToUniversalTime();
	}

	public DateTime CreationTimeUtc {
		get => GetTimeUtc(EntryTimestamp.Creation);
		set => SetTimeUtc(EntryTimestamp.Creation, value);
	}

	public DateTime LastAccessTimeUtc {
		get => GetTimeUtc(EntryTimestamp.LastAccess);
		set => SetTimeUtc(EntryTimestamp.LastAccess, value);
	}

	public DateTime LastWriteTimeUtc {
		get => GetTimeUtc(EntryTimestamp.LastWrite);
		set => SetTimeUtc(EntryTimestamp.LastWrite, value);
	}

	public DateTime GetTime(EntryTimestamp t) => GetTimeUtc(t).ToLocalTime();

	public DateTime GetTimeUtc(EntryTimestamp t) {
		GetTimesUtc(out var c, out var a, out var w);
		return t switch {
			EntryTimestamp.Creation   => c,
			EntryTimestamp.LastAccess => a,
			EntryTimestamp.LastWrite  => w,
			_                         => throw new ArgumentOutOfRangeException(nameof(t))
		};
	}

	public void GetTimes(out DateTime creation, out DateTime lastAccess, out DateTime lastWrite) {
		GetTimesUtc(out var c, out var a, out var w);
		creation = c.ToLocalTime();
		lastAccess = a.ToLocalTime();
		lastWrite = w.ToLocalTime();
	}

	/// <summary>
	///     Gets all timestamps using a single handle and a single kernel call.
	/// </summary>
	public void GetTimesUtc(out DateTime creationUtc, out DateTime lastAccessUtc, out DateTime lastWriteUtc) {
		using var handle = OpenHandle(FILE_READ_ATTRIBUTES);

		// Use GetFileInformationByHandle instead of GetFileTime to allow 
		// future expansion (like FileIndex, VolumeSerialNumber, etc.)
		if (!GetFileInformationByHandle(handle, out var info))
			ThrowLastWin32Error();

		creationUtc = DateTime.FromFileTimeUtc(info.ftCreationTime);
		lastAccessUtc = DateTime.FromFileTimeUtc(info.ftLastAccessTime);
		lastWriteUtc = DateTime.FromFileTimeUtc(info.ftLastWriteTime);
	}

	public void SetTime(EntryTimestamp t, DateTime value) => SetTimeUtc(t, value.ToUniversalTime());

	public void SetTimeUtc(EntryTimestamp t, DateTime utcValue) {
		long fileTime = utcValue.ToFileTimeUtc();
		unsafe {
			long* c = t == EntryTimestamp.Creation ? &fileTime : null;
			long* a = t == EntryTimestamp.LastAccess ? &fileTime : null;
			long* w = t == EntryTimestamp.LastWrite ? &fileTime : null;

			using var handle = OpenHandle(FILE_WRITE_ATTRIBUTES);
			if (!SetFileTime(handle, c, a, w))
				ThrowLastWin32Error();
		}
	}

	public void SetTimes(DateTime? creation, DateTime? access, DateTime? write)
		=> SetTimesUtc(creation?.ToUniversalTime(), access?.ToUniversalTime(), write?.ToUniversalTime());

	public void SetTimesUtc(DateTime? creationUtc, DateTime? accessUtc, DateTime? writeUtc) {
		unsafe {
			long cVal = creationUtc?.ToFileTimeUtc() ?? 0;
			long aVal = accessUtc?.ToFileTimeUtc() ?? 0;
			long wVal = writeUtc?.ToFileTimeUtc() ?? 0;

			long* cPtr = creationUtc.HasValue ? &cVal : null;
			long* aPtr = accessUtc.HasValue ? &aVal : null;
			long* wPtr = writeUtc.HasValue ? &wVal : null;

			using var handle = OpenHandle(FILE_WRITE_ATTRIBUTES);
			if (!SetFileTime(handle, cPtr, aPtr, wPtr))
				ThrowLastWin32Error();
		}
	}

	private SafeFileHandle OpenHandle(uint desiredAccess) {
		var handle = CreateFile(
			FullName,
			desiredAccess,
			FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
			IntPtr.Zero,
			OPEN_EXISTING,
			FILE_FLAG_BACKUP_SEMANTICS,
			IntPtr.Zero
		);

		if (handle.IsInvalid)
			ThrowLastWin32Error();

		return handle;
	}

	private static void ThrowLastWin32Error()
		=> throw new Win32Exception(Marshal.GetLastWin32Error());

	#region P/Invoke
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct BY_HANDLE_FILE_INFORMATION {
		public uint dwFileAttributes;
		public long ftCreationTime;
		public long ftLastAccessTime;
		public long ftLastWriteTime;
		public uint dwVolumeSerialNumber;
		public uint nFileSizeHigh;
		public uint nFileSizeLow;
		public uint nNumberOfLinks;
		public uint nFileIndexHigh;
		public uint nFileIndexLow;
	}

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern SafeFileHandle CreateFile(
		string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetFileInformationByHandle(
		SafeFileHandle hFile,
		out BY_HANDLE_FILE_INFORMATION lpFileInformation
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern unsafe bool SetFileTime(
		SafeFileHandle hFile,
		long* lpCreationTime,
		long* lpLastAccessTime,
		long* lpLastWriteTime
	);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);
	#endregion

	#region Boilerplate
	public override string ToString() => FullName;

	public bool Equals(EntryMetadata? other) => other is not null && (ReferenceEquals(this, other) || FullName == other.FullName);

	public override bool Equals(object? obj) => obj is EntryMetadata other && Equals(other);

	public override int GetHashCode() => FullName.GetHashCode();

	public static implicit operator EntryMetadata(FileSystemInfo info) => new(info);

	public static explicit operator string(EntryMetadata info) => info.FullName;

	public static explicit operator EntryMetadata(string path) => new(path);
	#endregion
}