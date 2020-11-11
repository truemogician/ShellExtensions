﻿using System.IO;
using System.Windows.Forms;
using SharpShell.SharpContextMenu;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using System.Linq;
using System.Diagnostics;
using System;

namespace RegisterShellExtension {
    using static System.Environment;

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension,".dll")]
    public class RegisterShellExtension : SharpContextMenu {
        protected string HostArch = "x64";
        protected override bool CanShowMenu() {
            if (SelectedItemPaths.Count() > 1)
                return false;
            else if (Path.GetFileName(SelectedItemPaths.First()) == "SharpShell.dll")
                return false;
            string directory = Path.GetDirectoryName(SelectedItemPaths.First());
            if (!File.Exists(Path.Combine(directory, "SharpShell.dll")))
                return false;
            return true;
        }
        protected override ContextMenuStrip CreateMenu() {
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem {
                Text = "Shell Extension",
                DropDownItems = {
                    new ToolStripMenuItem("Register x64"),
                    new ToolStripMenuItem("Unregister x64"),
                    new ToolStripMenuItem("Register x86"),
                    new ToolStripMenuItem("Unregister x86")
                }
            };
            foreach (ToolStripMenuItem dropDownItem in item.DropDownItems) {
                dropDownItem.Click += (sender, args) => {
                    string[] arguments = dropDownItem.Text.Split(' ');
                    Register(arguments[0], arguments[1]);
                };
            }
            menu.Items.Add(item);
            return menu;
        }
        protected void Register(string action, string targetArch) {
            string registrationHandler = @"D:\Program Files\Shell Extensions\Context Menu\RegisterShellExtension\RegistrationHandler.exe";
            Process.Start(new ProcessStartInfo {
                FileName = registrationHandler,
                Arguments = $"{action} \"{SelectedItemPaths.First()}\" {HostArch} {targetArch}",
                Verb="runas"
            });
        }
    }
}