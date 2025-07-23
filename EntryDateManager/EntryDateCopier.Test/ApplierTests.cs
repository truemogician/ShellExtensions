using NUnit.Framework;

namespace EntryDateCopier.Test {
    public class ApplierTests {
        [SetUp]
        public void Setup() {
        }

		[Test]
		public void TestApply(string source, string ediFile, bool applyToChildren) {
			var applier = new Applier(source, ediFile);
			applier.Apply(applyToChildren);
		}
    }
}