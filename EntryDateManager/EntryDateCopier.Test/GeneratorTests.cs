using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EntryDateCopier.Test {
	public class GeneratorTests {
		[Test]
		public async Task GenerateSingleTest(string root, int depth, string output) {
			var generator = new Generator(root) {
				Fields = EntryDateFields.Creation | EntryDateFields.Modification
			};
			await generator.Generate(depth);
			generator.SaveToFile(output);
		}

		[Test]
        public async Task GenerateMultipleTest(string root, int depth, string output) {
			var folders = Directory.GetDirectories(root);
			var generator = new Generator(folders) {
				Fields = EntryDateFields.Creation
			};
			await generator.Generate(depth);
			generator.SaveToFile(output);
        }
    }
}