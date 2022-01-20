using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;

namespace EntryDateSetter {
	[ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFilesAndFolders)]
    public class PropertySheet : SharpPropertySheet {
        protected override bool CanShowSheet() => true;
        protected override IEnumerable<SharpPropertyPage> CreatePages() => new[] { new FileDateInformationPropertyPage(string.IsNullOrEmpty(FolderPath) ? SelectedItemPaths.ToArray() : new[] { FolderPath }) };
    }
}
