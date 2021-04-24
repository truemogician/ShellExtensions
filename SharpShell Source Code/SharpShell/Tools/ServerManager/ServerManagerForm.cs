﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Apex.WinForms.Interop;
using Apex.WinForms.Shell;
using ServerManager.TestShell;
using SharpShell;
using SharpShell.Attributes;
using SharpShell.Diagnostics;
using SharpShell.ServerRegistration;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using SharpShell.SharpInfoTipHandler;
using SharpShell.SharpPreviewHandler;
using SharpShell.SharpPropertySheet;

namespace ServerManager
{
    /// <summary>
    /// The main class 
    /// </summary>
    public partial class ServerManagerForm : Form
    {
        public ServerManagerForm()
        {
            InitializeComponent();

            if (Properties.Settings.Default.RecentlyUsedFiles == null)
                Properties.Settings.Default.RecentlyUsedFiles = new StringCollection();

            //  Set initial UI command state.
            UpdateUserInterfaceCommands();
        }

        public IEnumerable<ServerEntry> ServerEntries
        {
            get { return listViewServers.Items.OfType<ListViewItem>().Select(lvi => lvi.Tag).OfType<ServerEntry>(); }
        }

        public void AddServer(string path, bool addToMostRecentlyUsedFiles)
        {
            if (ServerEntries.Any(se => se.ServerPath == path))
                return;

            //  Create a server entry for the server.
            var serverEntry = ServerManagerApi.LoadServer(path);
            if (serverEntry != null)
            {
                AddServerEntryToList(serverEntry);

                if (addToMostRecentlyUsedFiles && Properties.Settings.Default.RecentlyUsedFiles.Contains(path) == false)
                {
                    //  We've successfully added the server - so add the path of the server to our recent files.
                    Properties.Settings.Default.RecentlyUsedFiles.Insert(0, path);
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void AddServerEntryToList(ServerEntry serverEntry)
        {
            var listItem = new ListViewItem(
                new[]
                    {
                        serverEntry.ServerName,
                        serverEntry.ServerType.ToString(),
                        serverEntry.ClassId.ToString()
                    }) {Tag = serverEntry};

            switch (serverEntry.ServerType)
            {
                case ServerType.ShellContextMenu:
                    listItem.ImageIndex = 0;
                    break;
                case ServerType.ShellPropertySheet:
                    listItem.ImageIndex = 2;
                    break;
                case ServerType.ShellIconHandler:
                    listItem.ImageIndex = 1;
                    break;
                    case ServerType.ShellInfoTipHandler:
                    listItem.ImageIndex = 3;
                    break;
                case ServerType.ShellIconOverlayHandler:
                    listItem.ImageIndex = 4;
                    break;
                default:
                    listItem.ImageIndex = 0;
                    break;
            }
            listViewServers.Items.Add(listItem);

        }

        public ServerEntry SelectedServerEntry
        {
            get { return listViewServers.SelectedItems.Count > 0 ? (ServerEntry)listViewServers.SelectedItems[0].Tag : null; }
        }

        private void ServerManagerForm_Load(object sender, EventArgs e)
        {
            //  Set the settings.
            desktopProcessToolStripMenuItem.Checked = explorerConfigurationManager.DesktopProcess;
            alwaysUnloadDLLToolStripMenuItem.Checked = explorerConfigurationManager.AlwaysUnloadDll;

            //  Add the recently used servers.
            if (Properties.Settings.Default.RecentlyUsedFiles != null)
            {
                foreach(var path in Properties.Settings.Default.RecentlyUsedFiles)
                    AddServer(path, false);
            }

            //  Check for any servers added via the command line.
            var arguments = Environment.GetCommandLineArgs();
            for(int i = 1; i < arguments.Count(); i++)
            {
                var arg = arguments[i];
                if(File.Exists(arg))
                    AddServer(arg, true);
            }

            //  Set the log availability.
            enableSharpShellLogToolStripMenuItem.Checked = Logging.IsLoggingEnabled();
        }

        private void alwaysUnloadDLLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            explorerConfigurationManager.AlwaysUnloadDll = alwaysUnloadDLLToolStripMenuItem.Checked;
        }

        private void desktopProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            explorerConfigurationManager.DesktopProcess = desktopProcessToolStripMenuItem.Checked;
        }

        /// <summary>
        /// The explorer configuration manager.
        /// </summary>
        private readonly ExplorerConfigurationManager explorerConfigurationManager = new ExplorerConfigurationManager();

        private void installServerx86ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedServerEntry == null)
                return;

            ServerRegistrationManager.InstallServer(SelectedServerEntry.Server, RegistrationType.OS32Bit, true);
            serverDetailsView1.Initialise(SelectedServerEntry);
        }
        
        private void uninstallServerx86ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedServerEntry == null)
                return;

            //  Unregister the server.
            ServerRegistrationManager.UninstallServer(SelectedServerEntry.Server, RegistrationType.OS32Bit);
            serverDetailsView1.Initialise(SelectedServerEntry);
        }

        private void loadServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Create a file open dialog.
            var fileOpenDialog = new OpenFileDialog();
            fileOpenDialog.Filter = "COM Servers (*.dll)|*.dll|All Files (*.*)|*.*";
            if (fileOpenDialog.ShowDialog(this) == DialogResult.OK)
            {
                //  Try and add  the server.
                AddServer(fileOpenDialog.FileName, true);
            }
        }

        private void listViewServers_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = ((string[]) e.Data.GetData(DataFormats.FileDrop)).Any() ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void listViewServers_DragDrop(object sender, DragEventArgs e)
        {
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    foreach(var file in files)
                        AddServer(file, true);
                }
            }
        }

        private void listViewServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  Update the user interface commands.
            UpdateUserInterfaceCommands();

            //  Update the details view with the selected server entry.
            serverDetailsView1.Initialise(SelectedServerEntry);
        }

        private void listViewServers_KeyDown(object sender, KeyEventArgs e)
        {
            if (SelectedServerEntry == null)
                return;

            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                //  If we have a server selected, remove it.
                var item = listViewServers.Items.OfType<ListViewItem>().FirstOrDefault(li => li.Tag == SelectedServerEntry);
                if (item != null)
                {
                    listViewServers.Items.Remove(item);

                    //  Remove from the most recently used files.
                    if (Properties.Settings.Default.RecentlyUsedFiles != null)
                    {
                        Properties.Settings.Default.RecentlyUsedFiles.Remove(SelectedServerEntry.ServerPath);
                        Properties.Settings.Default.Save();
                    }
                }
            }

        }

        private void clearMostRecentlyUsedServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.RecentlyUsedFiles.Clear();
            Properties.Settings.Default.Save();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void installServerx64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Get the selected server.
            var selectedServer = SelectedServerEntry;
            if (selectedServer == null)
                return;

            ServerRegistrationManager.InstallServer(SelectedServerEntry.Server, RegistrationType.OS64Bit, true);
            serverDetailsView1.Initialise(SelectedServerEntry);
        }

        private void uninstallServerx64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Bail if we have no server selected.
            if (SelectedServerEntry == null)
                return;

            ServerRegistrationManager.UninstallServer(SelectedServerEntry.Server, RegistrationType.OS64Bit);
            serverDetailsView1.Initialise(SelectedServerEntry);
        }

        private void registerServerx86ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Bail if we have no server selected.
            if (SelectedServerEntry == null)
                return;

            //  Register the server, x86 mode.
            ServerRegistrationManager.RegisterServer(SelectedServerEntry.Server, RegistrationType.OS32Bit);
        }

        private void registerServerx64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Bail if we have no server selected.
            if (SelectedServerEntry == null)
                return;

            //  Register the server, x64 mode.
            ServerRegistrationManager.RegisterServer(SelectedServerEntry.Server, RegistrationType.OS64Bit);
        }

        private void unregisterServerx86ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Bail if we have no server selected.
            if (SelectedServerEntry == null)
                return;

            //  Unregister the server, x86 mode.
            ServerRegistrationManager.UnregisterServer(SelectedServerEntry.Server, RegistrationType.OS32Bit);
        }

        private void unregisterServerx64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Bail if we have no server selected.
            if (SelectedServerEntry == null)
                return;

            //  Unregister the server, x64 mode.
            ServerRegistrationManager.UnregisterServer(SelectedServerEntry.Server, RegistrationType.OS64Bit);
        }

        private void restartExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExplorerManager.RestartExplorer();
        }
        
        private void UpdateUserInterfaceCommands()
        {
            //  Install/Uninstall etc etc only available if we have a selection.
            installServerx86ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            installServerx64ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            registerServerx86ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            registerServerx64ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            unregisterServerx86ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            unregisterServerx64ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            uninstallServerx86ToolStripMenuItem.Enabled = SelectedServerEntry != null;
            uninstallServerx64ToolStripMenuItem.Enabled = SelectedServerEntry != null;

            //  Test functions only available for specific servers.
            testServerToolStripMenuItem.Enabled =
                (
                    SelectedServerEntry != null &&
                    (
                        SelectedServerEntry.Server is SharpContextMenu ||
                        SelectedServerEntry.Server is SharpIconHandler ||
                        SelectedServerEntry.Server is SharpInfoTipHandler ||
                        SelectedServerEntry.Server is SharpDropHandler ||
                        SelectedServerEntry.Server is SharpPreviewHandler
                    )
                );
            toolStripButtonTestServer.Enabled = testServerToolStripMenuItem.Enabled;
        }

        private void testServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Call the test server command.
            DoTestServer();
        }

        private void toolStripButtonTestServer_Click(object sender, EventArgs e)
        {
            //  Call the test server command.
            DoTestServer();
        }

        /// <summary>
        /// Tests the selected serer.
        /// </summary>
        private void DoTestServer()
        {
            //  If we don't have a server, bail.
            if (SelectedServerEntry == null)
                return;

            //  Create a test shell form.
            var testShellForm = new TestShellForm { TestServer = SelectedServerEntry.Server };

            //  Show the form.
            testShellForm.ShowDialog(this);
        }

        private void sharpShellProjectHomePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.UrlSharpShellProjectHomePage);
        }

        private void reportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.UrlReportABug);
        }

        private void requestAFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.URlSuggestAFeature);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutForm()).ShowDialog(this);
        }

        private void enableSharpShellLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableSharpShellLogToolStripMenuItem.Checked = !enableSharpShellLogToolStripMenuItem.Checked;
            Logging.EnableLogging(enableSharpShellLogToolStripMenuItem.Checked);
        }

        private void showLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Show a log view.
            (new LogView.LogViewForm()).Show(this);
        }

        private void toolStripButtonOpenTestShell_Click(object sender, EventArgs e)
        {
            (new TestShellForm()).ShowDialog(this);
        }
        
    }
}
