using System.IO;
using NUnit.Framework;

namespace EntryDateCopier.Test {
	public class ApplierTests {
		[SetUp]
		public void Setup() { }

		[Test]
		[TestCase(@"R:\Models\Clarice\[HegreArt]Session In Bed-20170319", @"R:\Models\Clarice\Clarice.edi", true)]
		public void ApplySingleTest(string source, string ediFile, bool applyToChildren) {
			var applier = new Applier(source, ediFile);
			applier.Apply(applyToChildren);
		}

		[Test]
		[TestCase(new[] { @"R:\Models\Clarice\[HegreArt]Session In Bed-20170319" }, @"R:\Models\Clarice\Clarice.edi", false)]
		public void ApplyMultipleTest(string[] sources, string ediFile, bool applyToChildren) {
			var applier = new Applier(sources, ediFile);
			applier.Apply(applyToChildren);
		}

		[Test]
		[TestCase(@"R:\Models", @"R:\Models\Models.edi", true)]
		public void ApplyMultipleTest(string root, string ediFile, bool applyToChildren) {
			var folders = Directory.GetDirectories(root);
			var applier = new Applier(folders, ediFile);
			applier.Apply(applyToChildren);
		}
	}
}