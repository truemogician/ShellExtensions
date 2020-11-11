using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpShell.Extensions;
using SharpShell.Interop;

namespace SharpShell
{
    /// <summary>
    /// The ShellExtInitServer is the base class for Shell Extension Servers
    /// that implement IShellExtInit.
    /// </summary>
    public abstract class ShellExtInitServer : SharpShellServer, IShellExtInit
    {
        #region Implementation of IShellExtInit

        /// <summary>
        /// Initializes the shell extension with the parent folder and data object.
        /// </summary>
        /// <param name="pidlFolder">The pidl of the parent folder.</param>
        /// <param name="pDataObj">The data object pointer.</param>
        /// <param name="hKeyProgID">The handle to the key prog id.</param>
        void IShellExtInit.Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgID)
        {
            //  Create the IDataObject from the provided pDataObj.
            var dataObject = (System.Runtime.InteropServices.ComTypes.IDataObject)Marshal.GetObjectForIUnknown(pDataObj);

            //  Add the set of files to the selected file paths.
            selectedItemPaths = dataObject.GetFileList();
        }

        #endregion
        
        /// <summary>
        /// The selected item paths.
        /// </summary>
        private List<string> selectedItemPaths = new List<string>();
            
        /// <summary>
        /// Gets the selected item paths.
        /// </summary>
        public IEnumerable<string> SelectedItemPaths
        {
            get { return selectedItemPaths; }
        }
    }
}
