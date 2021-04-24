using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FileDateCopier {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FileHeader {

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
		public static T FromByteArray(ref byte[] array) {
			int size = Size();
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(array, 0, ptr, size);
			T result = (T)Marshal.PtrToStructure(ptr, typeof(T));
			Marshal.FreeHGlobal(ptr);
			return result;
		}
	}
}
