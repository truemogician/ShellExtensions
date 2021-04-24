using System.Runtime.InteropServices;

namespace SharpShell.Interop
{
    /// <summary>
    /// Exposes methods for applying color and font information to preview handlers.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("8327b13c-b63f-4b24-9b8a-d010dcc3f599")]
    public interface IPreviewHandlerVisuals
    {
        /// <summary>
        /// Sets the background color of the preview handler.
        /// </summary>
        /// <param name="color">A value of type COLORREF to use for the preview handler background.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [PreserveSig]
        int SetBackgroundColor(COLORREF color);

        /// <summary>
        /// Sets the font attributes to be used for text within the preview handler.
        /// </summary>
        /// <param name="plf">A pointer to a LOGFONTW Structure containing the necessary attributes for the font to use.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [PreserveSig]
        int SetFont(ref LOGFONT plf);

        /// <summary>
        /// Sets the color of the text within the preview handler.
        /// </summary>
        /// <param name="color">A value of type COLORREF to use for the preview handler text color.</param>
        /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error cod</returns>
        [PreserveSig]
        int SetTextColor(COLORREF color);
    };
}