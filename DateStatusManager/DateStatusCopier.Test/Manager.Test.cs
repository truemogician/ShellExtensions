using System;
using DateStatusCopier;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DateStatusCopier.Test {
	internal static class TestPath {
		internal static readonly string BasePath = @"D:\Code\Visual Studio\Solutions\Shell Extensions\DateStatusManager\DateStatusCopier.Test\test";
		internal static readonly string DateStatusFile = Path.Combine(BasePath, "test.date-status");
		internal static readonly string SrcFile = Path.Combine(BasePath, "src.txt");
		internal static readonly string DstFile = Path.Combine(BasePath, "dst.txt");
		internal static readonly string SrcFolder = Path.Combine(BasePath, "src");
		internal static readonly string DstFolder = Path.Combine(BasePath, "dst");
	}
	[TestClass]
	public class GeneratorTest {
		[TestMethod]
		public void SingleUniversal() {
			var result = Generator.Generate(TestPath.SrcFile);
			Generator.WriteFile(TestPath.DateStatusFile, result);
			Assert.IsTrue(File.Exists(TestPath.DateStatusFile));
		}
		[TestMethod]
		public void SingleRecursive() {
			var result = Generator.Generate(TestPath.SrcFolder, includesChildren:true);
			Generator.WriteFile(TestPath.DateStatusFile, result);
			Assert.IsTrue(File.Exists(TestPath.DateStatusFile));
		}
	}

	[TestClass]
	public class ApplierTest {
		static void AssertDateEquality(string src, string dst, DateType dateType) {
			var srcType = DateStatus.GetPathType(src);
			var dstType = DateStatus.GetPathType(src);
			if (srcType == PathType.None || dstType == PathType.None)
				Assert.Fail();
			foreach (int i in DateStatus.DateTypeToIndices(dateType))
				Assert.AreEqual(DateStatus.GetDateTime[i](src, srcType == PathType.File), DateStatus.GetDateTime[i](dst, dstType == PathType.File));
		}
		static void AssertEquality(string src, string dst, DateType dateType, bool includesChildren = false) {
			AssertDateEquality(src, dst, dateType);
			var srcType = DateStatus.GetPathType(src);
			var dstType = DateStatus.GetPathType(src);
			if (includesChildren && srcType == PathType.Directory && dstType == PathType.Directory) {
				var srcSubfiles = Directory.GetFiles(src);
				var dstSubfiles = Directory.GetFiles(dst);
				foreach (string srcSubfile in srcSubfiles) {
					try {
						string dstSubfile = dstSubfiles.First(file => Path.GetFileName(file) == Path.GetFileName(srcSubfile));
						if (!string.IsNullOrEmpty(dstSubfile))
							AssertDateEquality(srcSubfile, dstSubfile, dateType);
					}
					catch (InvalidOperationException) { }
				}
				var srcSubdirectories = Directory.GetDirectories(src);
				var dstSubdirectories = Directory.GetDirectories(dst);
				foreach (string srcSubdirectory in srcSubdirectories) {
					try {
						string dstSubdirectory = dstSubdirectories.First(file => Path.GetFileName(file) == Path.GetFileName(srcSubdirectory));
						if (!string.IsNullOrEmpty(dstSubdirectory))
							AssertEquality(srcSubdirectory, dstSubdirectory, dateType, true);
					}
					catch (InvalidOperationException) { }
				}
			}
		}
		[TestMethod]
		public void Universal() {
			Applier.Apply(new string[] { TestPath.DstFile }, TestPath.DateStatusFile);
			AssertDateEquality(TestPath.SrcFile, TestPath.DstFile, DateType.Written);
		}
		[TestMethod]
		public void Recursive() {
			Applier.Apply(TestPath.DstFolder, TestPath.DateStatusFile, true);
			AssertEquality(TestPath.SrcFolder, TestPath.DstFolder, DateType.Written, true);
		}
	}
}
