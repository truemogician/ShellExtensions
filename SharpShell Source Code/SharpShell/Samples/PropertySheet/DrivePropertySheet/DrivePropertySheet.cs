using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;

namespace DrivePropertySheet
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Drive)]
    public class DrivePropertySheet : SharpPropertySheet
    {
        protected override bool CanShowSheet()
        {
            return true;
        }

        protected override IEnumerable<ISharpPropertyPage> CreatePages()
        {
            yield return new DrivePropertyPage();
        }
    }
}
