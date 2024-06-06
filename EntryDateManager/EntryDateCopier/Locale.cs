using System.Reflection;
using System.Resources;

namespace EntryDateCopier {
	internal static class Locale {
		internal static readonly ResourceManager Text = new ResourceManager("EntryDateCopier.Resources.Text", Assembly.GetExecutingAssembly());
    }
}