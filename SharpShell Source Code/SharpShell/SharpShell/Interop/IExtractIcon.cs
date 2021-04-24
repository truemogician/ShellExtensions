using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpShell.Interop
{
    /// <summary>
    /// Exposes methods that allow a client to retrieve the icon that is associated with one of the objects in a folder.
    /// </summary>
    [ComImport]
    [Guid("000214fa-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IExtractIcon
    {
        /// <summary>
        /// Gets the location and index of an icon.
        /// </summary>
        /// <param name="uFlags">One or more of the following values. This parameter can also be NULL.</param>
        /// <param name="szIconFile">A pointer to a buffer that receives the icon location. The icon location is a null-terminated string that identifies the file that contains the icon.</param>
        /// <param name="cchMax">The size of the buffer, in characters, pointed to by pszIconFile.</param>
        /// <param name="piIndex">A pointer to an int that receives the index of the icon in the file pointed to by pszIconFile.</param>
        /// <param name="pwFlags">A pointer to a UINT value that receives zero or a combination of the following value</param>
        /// <returns></returns>
        [PreserveSig]
        int GetIconLocation(GILInFlags uFlags, StringBuilder szIconFile, int cchMax, out int piIndex, out GILOutFlags pwFlags);

        /// <summary>
        /// Extracts an icon image from the specified location.
        /// </summary>
        /// <param name="pszFile">A pointer to a null-terminated string that specifies the icon location.</param>
        /// <param name="nIconIndex">The index of the icon in the file pointed to by pszFile.</param>
        /// <param name="phiconLarge">A pointer to an HICON value that receives the handle to the large icon. This parameter may be NULL.</param>
        /// <param name="phiconSmall">A pointer to an HICON value that receives the handle to the small icon. This parameter may be NULL.</param>
        /// <param name="nIconSize">The desired size of the icon, in pixels. The low word contains the size of the large icon, and the high word contains the size of the small icon. The size specified can be the width or height. The width of an icon always equals its height.</param>
        /// <returns>
        /// Returns S_OK if the function extracted the icon, or S_FALSE if the calling application should extract the icon.
        /// </returns>
        [PreserveSig]
        int Extract(string pszFile, uint nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);
    }

    /// <summary>
    /// Input GIL flags.
    /// </summary>
    [Flags]
    public enum GILInFlags : uint
    {
        /// <summary>
        /// Set this flag to determine whether the icon should be extracted asynchronously. If the icon can be extracted rapidly, this flag is usually ignored. If extraction will take more time, GetIconLocation should return E_PENDING
        /// </summary>
        GIL_ASYNC = 0x0020,
        /// <summary>
        /// Retrieve information about the fallback icon. Fallback icons are usually used while the desired icon is extracted and added to the cache.
        /// </summary>
        GIL_DEFAULTICON = 0x0040,
        /// <summary>
        /// The icon is displayed in a Shell folder.
        /// </summary>
        GIL_FORSHELL = 0x0002,
        /// <summary>
        /// The icon indicates a shortcut. However, the icon extractor should not apply the shortcut overlay; that will be done later. Shortcut icons are state-independent.
        /// </summary>
        GIL_FORSHORTCUT = 0x0080,
        /// <summary>
        /// The icon is in the open state if both open-state and closed-state images are available. If this flag is not specified, the icon is in the normal or closed state. This flag is typically used for folder objects.
        /// </summary>
        GIL_OPENICON = 0x0001,
        /// <summary>
        /// Explicitly return either GIL_SHIELD or GIL_FORCENOSHIELD in pwFlags. Do not block if GIL_ASYNC is set.
        /// </summary>
        GIL_CHECKSHIELD = 0x0200
    }

    /// <summary>
    /// Output GIL flags.
    /// </summary>
    [Flags]
    public enum GILOutFlags : uint
    {
        /// <summary>
        /// The physical image bits for this icon are not cached by the calling application.
        /// </summary>
        GIL_DONTCACHE = 0x0010,
        /// <summary>
        /// The location is not a file name/index pair. The values in pszIconFile and piIndex cannot be passed to ExtractIcon or ExtractIconEx.
        /// </summary>
        GIL_NOTFILENAME = 0x0008,
        /// <summary>
        /// All objects of this class have the same icon. This flag is used internally by the Shell. Typical implementations of IExtractIcon do not require this flag because the flag implies that an icon handler is not required to resolve the icon on a per-object basis. The recommended method for implementing per-class icons is to register a DefaultIcon for the class.
        /// </summary>
        GIL_PERCLASS = 0x0004,
        /// <summary>
        /// Each object of this class has its own icon. This flag is used internally by the Shell to handle cases like Setup.exe, where objects with identical names can have different icons. Typical implementations of IExtractIcon do not require this flag.
        /// </summary>
        GIL_PERINSTANCE = 0x0002,
        /// <summary>
        /// The calling application should create a document icon using the specified icon.
        /// </summary>
        GIL_SIMULATEDOC = 0x0001,
        /// <summary>
        /// Windows Vista only. The calling application must stamp the icon with the UAC shield.
        /// </summary>
        GIL_SHIELD = 0x0200,//Windows Vista only
        /// <summary>
        /// Windows Vista only. The calling application must not stamp the icon with the UAC shield.
        /// </summary>
        GIL_FORCENOSHIELD = 0x0400//Windows Vista only
    }
}