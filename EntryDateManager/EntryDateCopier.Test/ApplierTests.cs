using System.IO;
using NUnit.Framework;

namespace EntryDateCopier.Test {
    public class ApplierTests {
        [SetUp]
        public void Setup() {
        }

		[Test]
        public void ApplySingleTest(string source, string ediFile, bool applyToChildren) {
			var applier = new Applier(source, ediFile);
			applier.Apply(applyToChildren);
		}

        [Test]
        public void ApplyMultipleTest(string root, string output, bool applyToChildren) {
			var folders = Directory.GetDirectories(root);
			var applier = new Applier(folders, output);
			applier.Apply(applyToChildren);
		}
    }
}