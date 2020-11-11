using System.IO;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, ".dll")]
    public class RegisterShellExtension : SharpContextMenu {
        protected string Platform="x64";
        protected string VSCommandPromptDirectory = Path.Combine(GetFolderPath(SpecialFolder.CommonPrograms), @"Visual Studio 2019\Visual Studio Tools\VC");
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
                    if (Register(arguments[0], arguments[1], out string errorMessage))
                        MessageBox.Show($"{arguments[0]}ed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(errorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            }
            menu.Items.Add(item);
            return menu;
        }
        protected string AnotherArchitecture(string arch) {
            if (arch == "x64")
                return "x86";
            else if (arch == "x86")
                return "x64";
            throw new ArgumentException("Wrong architecture name");
        }
        protected bool Register(string action, string architecture, out string errorMessage) {
            errorMessage = null;
            string[] links = Directory.GetFiles(VSCommandPromptDirectory);
            string target = null;
            foreach (string link in links) {
                string linkName = Path.GetFileName(link);
                if (linkName.Substring(0, 3) == Platform) {
                    if (architecture == Platform && linkName.IndexOf(AnotherArchitecture(architecture)) == -1) {
                        target = link;
                        break;
                    }
                    else if (architecture == AnotherArchitecture(Platform) && linkName.IndexOf(architecture) != -1) {
                        target = link;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(target))
                return false;
            string message = null;
            using (var process = Process.Start(new ProcessStartInfo {
                FileName = "cmd.exe",
                Arguments = $"/c \"{target}\"",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            })) {
                process.ErrorDataReceived += (sender, args) => message = args.Data;
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine($"Regasm.exe {(action == "Register" ? "/codebase" : "/u")} \"{SelectedItemPaths.First()}\"");
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }
            errorMessage = message;
            return message == null;
        }
    }
}
