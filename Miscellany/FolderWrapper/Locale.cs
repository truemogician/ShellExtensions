using System.Reflection;
using System.Resources;

namespace FolderWrapper;

internal static class Locale {
	internal static readonly ResourceManager ContextMenu = new ResourceManager("FolderWrapper.Locales.ContextMenu", Assembly.GetExecutingAssembly());

	internal static readonly ResourceManager FolderWrapper = new ResourceManager("FolderWrapper.Locales.FolderWrapper", Assembly.GetExecutingAssembly());
}