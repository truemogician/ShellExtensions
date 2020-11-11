using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;

namespace ResourcesPropertySheet
{
    /// <summary>
    /// The ResourcesPropertySheet is a shell extension to show the managed 
    /// and unmanaged resources containing in binary files.
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".dll", ".exe", "*.ocx")]
    public class ResourcesPropertySheet : SharpPropertySheet
    {
        /// <summary>
        /// Determines whether this instance can show a shell property sheet, given the specified selected file list.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance should show a shell property sheet for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowSheet()
        {
            return true;
        }

        /// <summary>
        /// Creates the pages.
        /// </summary>
        /// <returns>
        /// The property sheet pages.
        /// </returns>
        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            return new[] {new UnmanagedResourcesPropertyPage()};
        }
    }
}
