using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EntryDateUtility;

public sealed class EntryDateInfo : IEquatable<EntryDateInfo> {
	private const uint FILE_READ_ATTRIBUTES = 0x0080;
	private const uint FILE_WRITE_ATTRIBUTES = 0x0100;
	private const uint FILE_SHARE_READ = 1;
	private const uint FILE_SHARE_WRITE = 2;
	private const uint FILE_SHARE_DELETE = 4;
	private const uint OPEN_EXISTING = 3;
	private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

	public EntryDateInfo(string path) {
		if (path is null)
			throw new ArgumentNullException(nameof(path));
		FullName = Path.GetFullPath(path);
	}

	public EntryDateInfo(FileSystemInfo info) {
		FullName = info.FullName;
	}

	public bool Equals(EntryDateInfo? other) {
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return FullName == other.FullName;
	}

	public string FullName { get; }

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

	public override string ToString() => FullName;

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

	public void GetTimesUtc(out DateTime creationUtc, out DateTime lastAccessUtc, out DateTime lastWriteUtc) {
		using var handle = OpenHandle(FILE_READ_ATTRIBUTES);
		if (!GetFileTime(handle, out long c, out long a, out long w))
			ThrowLastWin32Error();
		creationUtc = DateTime.FromFileTimeUtc(c);
		lastAccessUtc = DateTime.FromFileTimeUtc(a);
		lastWriteUtc = DateTime.FromFileTimeUtc(w);
	}

	public void SetTime(EntryTimestamp t, DateTime value) => SetTimeUtc(t, value.ToUniversalTime());

	public void SetTimeUtc(EntryTimestamp t, DateTime utcValue) {
		long fileTime = utcValue.ToFileTimeUtc();

		// We only pass the specific timestamp we want to change.
		// By passing null (IntPtr.Zero) to the others, Windows leaves them untouched.
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
		// Note: For production, consider prepending @"\\?\" to FullName to support paths longer than 260 characters.
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
	private static extern bool GetFileTime(
		SafeFileHandle hFile,
		out long lpCreationTime,
		out long lpLastAccessTime,
		out long lpLastWriteTime
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern unsafe bool SetFileTime(
		SafeFileHandle hFile,
		long* lpCreationTime,
		long* lpLastAccessTime,
		long* lpLastWriteTime
	);

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is EntryDateInfo other && Equals(other);

	public override int GetHashCode() => FullName.GetHashCode();

	public static implicit operator EntryDateInfo(FileSystemInfo info) => new(info);

	public static explicit operator string(EntryDateInfo info) => info.FullName;

	public static explicit operator EntryDateInfo(string path) => new(path);
}