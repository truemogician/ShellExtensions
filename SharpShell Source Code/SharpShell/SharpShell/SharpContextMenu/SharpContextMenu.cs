using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.Interop;

namespace SharpShell.SharpContextMenu
{
    /// <summary>
    /// SharpContextMenu is the base class for Shell Context Menu Extensions supported
    /// by SharpShell. By providing the implementation of 'CanShowMenu' and 'CreateMenu',
    /// derived classes can provide all of the functionality required to create a fully
    /// functional shell context menu.
    /// </summary>
    [ServerType(ServerType.ShellContextMenu)]
    public abstract class SharpContextMenu : ShellExtInitServer, IContextMenu
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharpContextMenu"/> class.
        /// </summary>
        protected SharpContextMenu()
        {
            //  The abstract CreateMenu function will provide the value for the lazy.
            contextMenuStrip = new Lazy<ContextMenuStrip>(CreateMenu);
        }

        #region Implementation of IContextMenu

        /// <summary>
        /// Called to query the context menu.
        /// </summary>
        /// <param name="hMenu">The handle to the parent menu.</param>
        /// <param name="indexMenu">The index of the menu.</param>
        /// <param name="idCmdFirst">The first command ID.</param>
        /// <param name="idCmdLast">The last command ID.</param>
        /// <param name="uFlags">The flags.</param>
        /// <returns>An HRESULT indicating success.</returns>
        int IContextMenu.QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst, int idCmdLast, CMF uFlags)
        {
            //  Log this key event.
            Log(string.Format("Query Context Menu for items {0}", string.Join(", ", SelectedItemPaths)));
            
            //  If we've got the defaultonly flag, we're done.
            if (uFlags.HasFlag(CMF.CMF_DEFAULTONLY))
                return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 0);

            //  Set the first item id.
            var firstItemId = (uint)idCmdFirst;
            
            //  Use the native context menu wrapper to build the context menu.
            uint lastItemId = 0;
            try
            {
                nativeContextMenuWrapper.ResetNativeContextMenu();
                lastItemId = nativeContextMenuWrapper.BuildNativeContextMenu(hMenu, firstItemId, contextMenuStrip.Value.Items);
            }
            catch (Exception exception)
            {
                //  Log the exception.
                LogError("An exception occured building the context menu.", exception);

                //  Return the failure.
                return WinError.E_FAIL;
            }

            //  Return success, passing the the last item ID plus one (which will be the next command id).
            //  MSDN documentation is flakey here - to be explicit we need to return the count of the items added plus one.
            return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, (lastItemId - firstItemId) + 1);
        }

        /// <summary>
        /// Called to invoke the comamand.
        /// </summary>
        /// <param name="pici">The command info pointer.</param>
        int IContextMenu.InvokeCommand(IntPtr pici)
        {
            //  We'll work out whether the commandis unicode or not...
            var isUnicode = false;

            //  We could have been provided with a CMINVOKECOMMANDINFO or a 
            //  CMINVOKECOMMANDINFOEX - cast to the small and then check the size.
            var ici = (CMINVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typeof(CMINVOKECOMMANDINFO));
            var iciex = new CMINVOKECOMMANDINFOEX();

            //  Is it a CMINVOKECOMMANDINFOEX?
            if (ici.cbSize == Marshal.SizeOf(typeof(CMINVOKECOMMANDINFOEX)))
            {
                //  Check the unicode flag, get the extended command info.
                if ((ici.fMask & CMIC.CMIC_MASK_UNICODE) != 0)
                {
                    isUnicode = true;
                    iciex = (CMINVOKECOMMANDINFOEX)Marshal.PtrToStructure(pici,
                        typeof(CMINVOKECOMMANDINFOEX));
                }
            }

            //  If we're not unicode and the verb hiword is not zero,
            //  we've got an ANSI verb string.
            if (!isUnicode && User32.HighWord(ici.verb.ToInt32()) != 0)
            {
                //  Get the verb.
                var verb = Marshal.PtrToStringAnsi(ici.verb);

                //  Log this key event.
                Log(string.Format("Invoke ANSI verb {0}", verb));
                
                //  Try and invoke the command. If we don't invoke it, throw
                //  E_FAIL so that other handlers can try.
                if (!nativeContextMenuWrapper.TryInvokeCommand(verb))
                    Marshal.ThrowExceptionForHR(WinError.E_FAIL);
            }
            //  If we're unicode, and the verb hiword is not zero,
            //  we've got a unicode command string.
            else if (isUnicode && User32.HighWord(iciex.verbW.ToInt32()) != 0)
            {
                //  Get the verb.
                var verb = Marshal.PtrToStringAnsi(ici.verb);

                //  Log this key event.
                Log(string.Format("Invoke Unicode verb {0}", verb));
                
                //  Try and invoke the command. If we don't invoke it, throw
                //  E_FAIL so that other handlers can try.
                if (!nativeContextMenuWrapper.TryInvokeCommand(verb))
                    Marshal.ThrowExceptionForHR(WinError.E_FAIL);
            }
            //  The verb pointer isn't a string at all, it's an index.
            else
            {
                //  Get the command index. Logically, we don't actually need to
                //  loword it, as the hiword is zero, but we're following the
                //  documentation rigourously.
                var index = User32.LowWord(ici.verb.ToInt32());
                
                //  Log this key event.
                Log(string.Format("Invoke command index {0}", index));
                
                //  Try and invoke the command. If we don't invoke it, throw
                //  E_FAIL so that other handlers can try.
                if (!nativeContextMenuWrapper.TryInvokeCommand(index))
                    Marshal.ThrowExceptionForHR(WinError.E_FAIL);
            }

            //  Return success.
            return WinError.S_OK;
        }

        /// <summary>
        /// Gets the command string.
        /// </summary>
        /// <param name="idcmd">The idcmd.</param>
        /// <param name="uflags">The uflags.</param>
        /// <param name="reserved">The reserved.</param>
        /// <param name="commandstring">The commandstring.</param>
        /// <param name="cch">The CCH.</param>
        int IContextMenu.GetCommandString(int idcmd, GCS uflags, int reserved, StringBuilder commandstring, int cch)
        {
            //  Get the item.
            if (idcmd >= contextMenuStrip.Value.Items.Count)
                return WinError.E_FAIL;
            var item = contextMenuStrip.Value.Items[idcmd];
            
            //  Based on the flags, choose a string to set.
            var stringData = string.Empty;
            switch (uflags)
            {
                case GCS.GCS_VERBW:
                    //  We need to provide a verb. Use the item name, as the Context Menu Builder will 
                    //  make sure it is unique.
                    stringData = item.Name;
                    break;

                case GCS.GCS_HELPTEXTW:
                    //  We need to provide the tooltip text.
                    stringData = item.ToolTipText;
                    break;
            }

            //  If we have not been given sufficient space for the string, throw an insufficient buffer exception.
            if(stringData.Length > cch - 1)
            {
                Marshal.ThrowExceptionForHR(WinError.STRSAFE_E_INSUFFICIENT_BUFFER);
                return WinError.E_FAIL;
            }

            //  Append the string data.
            commandstring.Clear();
            commandstring.Append(stringData);

            //  Return success.
            return WinError.S_OK;
        }

        #endregion
        
        /// <summary>
        /// Determines whether this instance can a shell context show menu, given the specified selected file list.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance should show a shell context menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanShowMenu();

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// </summary>
        /// <returns>The context menu for the shell context menu.</returns>
        protected abstract ContextMenuStrip CreateMenu();

        /// <summary>
        /// The lazy context menu strip, only created when we actually need it.
        /// </summary>
        private readonly Lazy<ContextMenuStrip> contextMenuStrip;

        /// <summary>
        /// The native context menu wrapper.
        /// </summary>
        private readonly NativeContextMenuWrapper nativeContextMenuWrapper = new NativeContextMenuWrapper();
    }
}
