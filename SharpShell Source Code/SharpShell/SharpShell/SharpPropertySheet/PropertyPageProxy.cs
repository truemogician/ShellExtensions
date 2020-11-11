using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SharpShell.Diagnostics;
using SharpShell.Extensions;
using SharpShell.Interop;

namespace SharpShell.SharpPropertySheet
{
    /// <summary>
    /// The PropertyPageProxy is the object used to pass data between the 
    /// shell and the SharpPropertyPage.
    /// </summary>
    internal class PropertyPageProxy
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="PropertyPageProxy"/> class from being created.
        /// </summary>
        private PropertyPageProxy()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPageProxy"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyPage">The target property page.</param>
        internal PropertyPageProxy(SharpPropertySheet parent, SharpPropertyPage propertyPage)
        {
            //  Set the target.
            Parent = parent;
            Target = propertyPage;

            //  set the proxy reference in the property page.
            propertyPage.PropertyPageProxy = this;

            //  Create the dialog proc delegate (as a class member so it won't be garbage collected).
            dialogProc = new DialogProc(WindowProc);
            callbackProc = new PropSheetCallback(CallbackProc);
        }

        /// <summary>
        /// The WindowProc. Called by the shell and must delegate messages via the proxy to the user control.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="uMessage">The u message.</param>
        /// <param name="wParam">The w param.</param>
        /// <param name="lParam">The l param.</param>
        /// <returns></returns>
        private bool WindowProc(IntPtr hWnd, uint uMessage, IntPtr wParam, IntPtr lParam)
        {
            switch (uMessage)
            {
                case WindowsMessages.WM_INITDIALOG:

                    Logging.Log("WM_INITDIALOG Start");

                    try
                    {
                        //  Set the parent of the property page to the host.
                        User32.SetParent(Target.Handle, hWnd);

                        //  Get the handle to the property sheet.
                        propertySheetHandle = User32.GetParent(hWnd);

                        //  Update the page.
                        Target.OnPageInitialised(Parent);
                    }
                    catch (Exception exception)
                    {
                        Logging.Error("Failed to set the parent to the host.", exception);
                    }

                    Logging.Log("WM_INITDIALOG End");

                    break;

                case WindowsMessages.WM_NOTIFY:

                    //  Get the NMHDR.
                    var nmhdr = (NMHDR)Marshal.PtrToStructure(lParam, typeof (NMHDR));

                    const uint PSN_APPLY = 0xffffff36;

                    //  Is it PSN_APPLY?
                    if (nmhdr.code == PSN_APPLY)
                    {
                        
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// The CallbackProc. Called by the shell to inform of key property page events.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="uMsg">The u MSG.</param>
        /// <param name="ppsp">The PPSP.</param>
        /// <returns></returns>
        private uint CallbackProc(IntPtr hWnd, PSPCB uMsg, ref PROPSHEETPAGE ppsp)
        {
            switch (uMsg)
            {
                case PSPCB.PSPCB_ADDREF:

                    break;

                case PSPCB.PSPCB_RELEASE:

                    break;

                case PSPCB.PSPCB_CREATE:

                    //  Allow the sheet to be created.
                    return 1;
            }
            return 0;
        }
        
        /// <summary>
        /// Creates the property page handle.
        /// </summary>
        public void CreatePropertyPageHandle(NativeBridge.NativeBridge nativeBridge)
        {
            Logging.Log("Creating property page handle via bridge.");

            //  Create a prop sheet page structure.
            var psp = new PROPSHEETPAGE();

            //  Set the key properties.
            psp.dwSize = (uint)Marshal.SizeOf(psp);
            //psp.dwFlags = PSP.USETITLE | PSP.USECALLBACK/* | PSP.DEFAULT |*/| PSP.DLGINDIRECT;
            //psp.dwFlags = PSP.DEFAULT | PSP.USETITLE | PSP.DLGINDIRECT;
            //psp.hInstance = nativeBridge.GetInstanceHandle();

            psp.hInstance = nativeBridge.GetInstanceHandle();
            psp.dwFlags = PSP.PSP_DEFAULT | PSP.PSP_USETITLE | PSP.PSP_USECALLBACK;
            Logging.Log("Getting proxy host...");
            psp.pTemplate = nativeBridge.GetProxyHostTemplate();
            psp.pfnDlgProc = dialogProc;
            psp.pcRefParent = 0;
            psp.pfnCallback = callbackProc;
            psp.lParam = IntPtr.Zero;

            //  If we have a title, set it.
            if (!string.IsNullOrEmpty(Target.PageTitle))
            {
                psp.dwFlags |= PSP.PSP_USETITLE;
                psp.pszTitle = Target.PageTitle;
            }

            //  If we have an icon, set it.
            if (Target.PageIcon != null && Target.PageIcon.Handle != IntPtr.Zero)
            {
                psp.dwFlags |= PSP.PSP_USEHICON;
                psp.hIcon = Target.PageIcon.Handle;
            }

            //  Create a the property sheet page.
            HostWindowHandle = Comctl32.CreatePropertySheetPage(ref psp);

            //  Log the host window handle.
            Logging.Log("Created Proxy Host: " + HostWindowHandle.ToString("X8"));
        }

        /// <summary>
        /// Sets the data changed state of the parent property sheet, enabling (or disabling) the apply button.
        /// </summary>
        /// <param name="changed">if set to <c>true</c> data has changed.</param>
        internal void SetDataChanged(bool changed)
        {
            //  Send the appropriate message to the property sheet.
            User32.SendMessage(propertySheetHandle, 
                changed ? WindowsMessages.PSM_CHANGED : WindowsMessages.PSM_UNCHANGED, HostWindowHandle, IntPtr.Zero);
        }

        /// <summary>
        /// The dialog proc.
        /// </summary>
        private readonly DialogProc dialogProc;

        /// <summary>
        /// The callback proc.
        /// </summary>
        private readonly PropSheetCallback callbackProc;

        /// <summary>
        /// The property sheet handle.
        /// </summary>
        private IntPtr propertySheetHandle;

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public SharpPropertySheet Parent { get; set; }

        /// <summary>
        /// Gets the property page.
        /// </summary>
        /// <value>
        /// The property page.
        /// </value>
        public SharpPropertyPage Target { get; private set; }

        /// <summary>
        /// Gets the host window handle.
        /// </summary>
        /// <value>
        /// The host window handle.
        /// </value>
        public IntPtr HostWindowHandle { get; private set; }
    }
}
