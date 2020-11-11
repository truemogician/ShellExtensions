using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;
using System.Linq;

namespace TextFilePropertySheet
{
    /// <summary>
    /// The TextFilePropertySheet is a property sheet extension for text files.
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".txt")]
    public class TextFilePropertySheet : SharpPropertySheet
    {
        /// <summary>
        /// Determines whether this instance can show a shell property sheet, given the specified selected file list.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance should show a shell property sheet for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowSheet()
        {
            //  We can show the properties only if we have a single text file selected.
            return SelectedItemPaths.Count() == 1;
        }

        /// <summary>
        /// Creates the pages.
        /// </summary>
        /// <returns>
        /// The property sheet pages.
        /// </returns>
        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            //  Return the text file property page.
            yield return new TextFilePropertyPage();
        }
    }
}
