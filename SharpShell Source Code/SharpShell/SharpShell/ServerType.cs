using System.ComponentModel;

namespace SharpShell
{
    /// <summary>
    /// The Server Type.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// No Server Type.
        /// </summary>
        [Description("Not a SharpShell Server")]
        None,

        /// <summary>
        /// A Shell Context Menu.
        /// </summary>
        [Description("Shell Context Menu")]
        ShellContextMenu,

        /// <summary>
        /// A Shell Property Sheet.
        /// </summary>
        [Description("Shell Property Sheet")]
        ShellPropertySheet,

        /// <summary>
        /// A Shell Icon Handler.
        /// </summary>
        [Description("Shell Icon Handler")]
        ShellIconHandler,

        /// <summary>
        /// A Shell Info Tip Handler.
        /// </summary>
        [Description("Shell Info Tip Handler")]
        ShellInfoTipHandler,

        /// <summary>
        /// A Shell Drop Handler
        /// </summary>
        [Description("Shell Drop Handler")]
        ShellDropHandler,

        /// <summary>
        /// A Shell Icon Overlay Handler.
        /// </summary>
        [Description("Shell Icon OVerlay Handler")]
        ShellIconOverlayHandler,

        /// <summary>
        /// A Shell Preview Handler
        /// </summary>
        [Description("Shell Preview Handler")]
        ShellPreviewHander
    }
}