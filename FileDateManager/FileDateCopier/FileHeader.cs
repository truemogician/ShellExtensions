using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FileDateCopier {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FileHeader {
		public HeaderFlag Flags;
		public FileHeader(bool appliesToChildren, FileDateInformation[] infos = null) {
			Flags = HeaderFlag.None;
			if (appliesToChildren)
				Flags |= HeaderFlag.AppliesToChildren;
			if (infos != null) {
				if (infos.Length > 1)
					Flags |= HeaderFlag.Multiple;
				if (infos.Any(info => info.General))
					Flags |= HeaderFlag.General;
				if (infos.Any(info => !info.General))
					Flags |= HeaderFlag.Special;
				if (infos.Any(info => info.IncludesChildren))
					Flags |= HeaderFlag.IncludesChildren;
			}
		}
		[Flags]
		public enum HeaderFlag : int {
			None = 0,
			AppliesToChildren = 1,
			Multiple = 2,
			General = 4,
			Special = 8,
			IncludesChildren = 16
		}
	}
	public static class StructMarshalling<T> where T : unmanaged {
		public static int Size() => Marshal.SizeOf(typeof(T));
		public static byte[] ToByteArray(ref T obj) {
			int size = Size();
			byte[] result = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, result, 0, size);
			Marshal.FreeHGlobal(ptr);
			return result;
		}
		public static T FromByteArray(ref byte[] array, bool autoExtend = true) {
			int size = Size();
			IntPtr ptr = Marshal.AllocHGlobal(size);
			if (autoExtend && array.Length < size) {
				var temp = new byte[size];
				Array.Copy(array, temp, array.Length);
				Marshal.Copy(temp, 0, ptr, size);
			}
			else
				Marshal.Copy(array, 0, ptr, size);
			T result = (T)Marshal.PtrToStructure(ptr, typeof(T));
			Marshal.FreeHGlobal(ptr);
			return result;
		}
	}
}
