using System.Reflection;
using System.Resources;

namespace FolderOperation;

internal static class Locale {
	internal static readonly ResourceManager ContextMenu = new("FolderOperation.Locales.ContextMenu", Assembly.GetExecutingAssembly());

	internal static readonly ResourceManager FolderOperation = new("FolderOperation.Locales.FolderOperation", Assembly.GetExecutingAssembly());
}