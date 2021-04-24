﻿using ServerManager.ServerDetails;

namespace ServerManager
{
    partial class ServerManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerManagerForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listViewServers = new System.Windows.Forms.ListView();
            this.columnHeaderServerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderServerType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.serverDetailsView1 = new ServerManager.ServerDetails.ServerDetailsView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installServerx86ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installServerx64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallServerx86ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallServerx64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.registerServerx86ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registerServerx64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unregisterServerx86ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unregisterServerx64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.testServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.explorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysUnloadDLLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.desktopProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableSharpShellLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMostRecentlyUsedServersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sharpShellProjectHomePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.reportABugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.requestAFeatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonTestServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpenTestShell = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(872, 439);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(872, 510);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStripMain);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(872, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(307, 17);
            this.toolStripStatusLabel1.Text = "Drop SharpShell Server DLLs onto the list to analyse them";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listViewServers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.serverDetailsView1);
            this.splitContainer1.Size = new System.Drawing.Size(872, 439);
            this.splitContainer1.SplitterDistance = 492;
            this.splitContainer1.TabIndex = 1;
            // 
            // listViewServers
            // 
            this.listViewServers.AllowDrop = true;
            this.listViewServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderServerName,
            this.columnHeaderServerType,
            this.columnHeader});
            this.listViewServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewServers.FullRowSelect = true;
            this.listViewServers.Location = new System.Drawing.Point(0, 0);
            this.listViewServers.Name = "listViewServers";
            this.listViewServers.Size = new System.Drawing.Size(492, 439);
            this.listViewServers.SmallImageList = this.imageList1;
            this.listViewServers.TabIndex = 0;
            this.listViewServers.UseCompatibleStateImageBehavior = false;
            this.listViewServers.View = System.Windows.Forms.View.Details;
            this.listViewServers.SelectedIndexChanged += new System.EventHandler(this.listViewServers_SelectedIndexChanged);
            this.listViewServers.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewServers_DragDrop);
            this.listViewServers.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewServers_DragEnter);
            this.listViewServers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewServers_KeyDown);
            // 
            // columnHeaderServerName
            // 
            this.columnHeaderServerName.Text = "Server Name";
            this.columnHeaderServerName.Width = 200;
            // 
            // columnHeaderServerType
            // 
            this.columnHeaderServerType.Text = "Type";
            this.columnHeaderServerType.Width = 120;
            // 
            // columnHeader
            // 
            this.columnHeader.Text = "CLSID";
            this.columnHeader.Width = 200;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ContextMenu.png");
            this.imageList1.Images.SetKeyName(1, "Icon.png");
            this.imageList1.Images.SetKeyName(2, "PropertySheet.png");
            this.imageList1.Images.SetKeyName(3, "InfoTip.png");
            this.imageList1.Images.SetKeyName(4, "IconOverlayHandler.png");
            // 
            // serverDetailsView1
            // 
            this.serverDetailsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverDetailsView1.Location = new System.Drawing.Point(0, 0);
            this.serverDetailsView1.Name = "serverDetailsView1";
            this.serverDetailsView1.Size = new System.Drawing.Size(376, 439);
            this.serverDetailsView1.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.serverToolStripMenuItem,
            this.explorerToolStripMenuItem,
            this.diagnosticsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(872, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadServerToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadServerToolStripMenuItem
            // 
            this.loadServerToolStripMenuItem.Name = "loadServerToolStripMenuItem";
            this.loadServerToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.loadServerToolStripMenuItem.Text = "&Load Server...";
            this.loadServerToolStripMenuItem.Click += new System.EventHandler(this.loadServerToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(141, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installServerx86ToolStripMenuItem,
            this.installServerx64ToolStripMenuItem,
            this.uninstallServerx86ToolStripMenuItem,
            this.uninstallServerx64ToolStripMenuItem,
            this.toolStripSeparator3,
            this.registerServerx86ToolStripMenuItem,
            this.registerServerx64ToolStripMenuItem,
            this.unregisterServerx86ToolStripMenuItem,
            this.unregisterServerx64ToolStripMenuItem,
            this.toolStripSeparator1,
            this.testServerToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.serverToolStripMenuItem.Text = "&Server";
            // 
            // installServerx86ToolStripMenuItem
            // 
            this.installServerx86ToolStripMenuItem.Name = "installServerx86ToolStripMenuItem";
            this.installServerx86ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.installServerx86ToolStripMenuItem.Text = "&Install Server (x86)";
            this.installServerx86ToolStripMenuItem.Click += new System.EventHandler(this.installServerx86ToolStripMenuItem_Click);
            // 
            // installServerx64ToolStripMenuItem
            // 
            this.installServerx64ToolStripMenuItem.Name = "installServerx64ToolStripMenuItem";
            this.installServerx64ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.installServerx64ToolStripMenuItem.Text = "I&nstall Server (x64)";
            this.installServerx64ToolStripMenuItem.Click += new System.EventHandler(this.installServerx64ToolStripMenuItem_Click);
            // 
            // uninstallServerx86ToolStripMenuItem
            // 
            this.uninstallServerx86ToolStripMenuItem.Name = "uninstallServerx86ToolStripMenuItem";
            this.uninstallServerx86ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.uninstallServerx86ToolStripMenuItem.Text = "&Uninstall Server (x86)";
            this.uninstallServerx86ToolStripMenuItem.Click += new System.EventHandler(this.uninstallServerx86ToolStripMenuItem_Click);
            // 
            // uninstallServerx64ToolStripMenuItem
            // 
            this.uninstallServerx64ToolStripMenuItem.Name = "uninstallServerx64ToolStripMenuItem";
            this.uninstallServerx64ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.uninstallServerx64ToolStripMenuItem.Text = "Unin&stall Server (x64)";
            this.uninstallServerx64ToolStripMenuItem.Click += new System.EventHandler(this.uninstallServerx64ToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(188, 6);
            // 
            // registerServerx86ToolStripMenuItem
            // 
            this.registerServerx86ToolStripMenuItem.Name = "registerServerx86ToolStripMenuItem";
            this.registerServerx86ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.registerServerx86ToolStripMenuItem.Text = "Register Server (x86)";
            this.registerServerx86ToolStripMenuItem.Click += new System.EventHandler(this.registerServerx86ToolStripMenuItem_Click);
            // 
            // registerServerx64ToolStripMenuItem
            // 
            this.registerServerx64ToolStripMenuItem.Name = "registerServerx64ToolStripMenuItem";
            this.registerServerx64ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.registerServerx64ToolStripMenuItem.Text = "Register Server (x64)";
            this.registerServerx64ToolStripMenuItem.Click += new System.EventHandler(this.registerServerx64ToolStripMenuItem_Click);
            // 
            // unregisterServerx86ToolStripMenuItem
            // 
            this.unregisterServerx86ToolStripMenuItem.Name = "unregisterServerx86ToolStripMenuItem";
            this.unregisterServerx86ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.unregisterServerx86ToolStripMenuItem.Text = "Unregister Server (x86)";
            this.unregisterServerx86ToolStripMenuItem.Click += new System.EventHandler(this.unregisterServerx86ToolStripMenuItem_Click);
            // 
            // unregisterServerx64ToolStripMenuItem
            // 
            this.unregisterServerx64ToolStripMenuItem.Name = "unregisterServerx64ToolStripMenuItem";
            this.unregisterServerx64ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.unregisterServerx64ToolStripMenuItem.Text = "Unregister Server (x64)";
            this.unregisterServerx64ToolStripMenuItem.Click += new System.EventHandler(this.unregisterServerx64ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // testServerToolStripMenuItem
            // 
            this.testServerToolStripMenuItem.Name = "testServerToolStripMenuItem";
            this.testServerToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.testServerToolStripMenuItem.Text = "&Test Server...";
            this.testServerToolStripMenuItem.Click += new System.EventHandler(this.testServerToolStripMenuItem_Click);
            // 
            // explorerToolStripMenuItem
            // 
            this.explorerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysUnloadDLLToolStripMenuItem,
            this.desktopProcessToolStripMenuItem,
            this.restartExplorerToolStripMenuItem});
            this.explorerToolStripMenuItem.Name = "explorerToolStripMenuItem";
            this.explorerToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.explorerToolStripMenuItem.Text = "&Explorer";
            // 
            // alwaysUnloadDLLToolStripMenuItem
            // 
            this.alwaysUnloadDLLToolStripMenuItem.CheckOnClick = true;
            this.alwaysUnloadDLLToolStripMenuItem.Name = "alwaysUnloadDLLToolStripMenuItem";
            this.alwaysUnloadDLLToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.alwaysUnloadDLLToolStripMenuItem.Text = "&Always Unload DLL";
            this.alwaysUnloadDLLToolStripMenuItem.Click += new System.EventHandler(this.alwaysUnloadDLLToolStripMenuItem_Click);
            // 
            // desktopProcessToolStripMenuItem
            // 
            this.desktopProcessToolStripMenuItem.CheckOnClick = true;
            this.desktopProcessToolStripMenuItem.Name = "desktopProcessToolStripMenuItem";
            this.desktopProcessToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.desktopProcessToolStripMenuItem.Text = "&Desktop Process";
            this.desktopProcessToolStripMenuItem.Click += new System.EventHandler(this.desktopProcessToolStripMenuItem_Click);
            // 
            // restartExplorerToolStripMenuItem
            // 
            this.restartExplorerToolStripMenuItem.Name = "restartExplorerToolStripMenuItem";
            this.restartExplorerToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.restartExplorerToolStripMenuItem.Text = "&Restart Explorer";
            this.restartExplorerToolStripMenuItem.Click += new System.EventHandler(this.restartExplorerToolStripMenuItem_Click);
            // 
            // diagnosticsToolStripMenuItem
            // 
            this.diagnosticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableSharpShellLogToolStripMenuItem,
            this.showLogToolStripMenuItem});
            this.diagnosticsToolStripMenuItem.Name = "diagnosticsToolStripMenuItem";
            this.diagnosticsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.diagnosticsToolStripMenuItem.Text = "&Diagnostics";
            // 
            // enableSharpShellLogToolStripMenuItem
            // 
            this.enableSharpShellLogToolStripMenuItem.Name = "enableSharpShellLogToolStripMenuItem";
            this.enableSharpShellLogToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.enableSharpShellLogToolStripMenuItem.Text = "&Enable SharpShell Log";
            this.enableSharpShellLogToolStripMenuItem.Click += new System.EventHandler(this.enableSharpShellLogToolStripMenuItem_Click);
            // 
            // showLogToolStripMenuItem
            // 
            this.showLogToolStripMenuItem.Name = "showLogToolStripMenuItem";
            this.showLogToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.showLogToolStripMenuItem.Text = "&Show Log";
            this.showLogToolStripMenuItem.Click += new System.EventHandler(this.showLogToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearMostRecentlyUsedServersToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // clearMostRecentlyUsedServersToolStripMenuItem
            // 
            this.clearMostRecentlyUsedServersToolStripMenuItem.Name = "clearMostRecentlyUsedServersToolStripMenuItem";
            this.clearMostRecentlyUsedServersToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.clearMostRecentlyUsedServersToolStripMenuItem.Text = "&Clear most recently used servers";
            this.clearMostRecentlyUsedServersToolStripMenuItem.Click += new System.EventHandler(this.clearMostRecentlyUsedServersToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sharpShellProjectHomePageToolStripMenuItem,
            this.toolStripSeparator4,
            this.reportABugToolStripMenuItem,
            this.requestAFeatureToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // sharpShellProjectHomePageToolStripMenuItem
            // 
            this.sharpShellProjectHomePageToolStripMenuItem.Name = "sharpShellProjectHomePageToolStripMenuItem";
            this.sharpShellProjectHomePageToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.sharpShellProjectHomePageToolStripMenuItem.Text = "&SharpShell Project Home Page";
            this.sharpShellProjectHomePageToolStripMenuItem.Click += new System.EventHandler(this.sharpShellProjectHomePageToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(231, 6);
            // 
            // reportABugToolStripMenuItem
            // 
            this.reportABugToolStripMenuItem.Name = "reportABugToolStripMenuItem";
            this.reportABugToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.reportABugToolStripMenuItem.Text = "&Report a Bug";
            this.reportABugToolStripMenuItem.Click += new System.EventHandler(this.reportABugToolStripMenuItem_Click);
            // 
            // requestAFeatureToolStripMenuItem
            // 
            this.requestAFeatureToolStripMenuItem.Name = "requestAFeatureToolStripMenuItem";
            this.requestAFeatureToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.requestAFeatureToolStripMenuItem.Text = "Request a &Feature";
            this.requestAFeatureToolStripMenuItem.Click += new System.EventHandler(this.requestAFeatureToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(231, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStripMain
            // 
            this.toolStripMain.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpenTestShell,
            this.toolStripButtonTestServer});
            this.toolStripMain.Location = new System.Drawing.Point(3, 24);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Size = new System.Drawing.Size(302, 25);
            this.toolStripMain.TabIndex = 3;
            // 
            // toolStripButtonTestServer
            // 
            this.toolStripButtonTestServer.Image = global::ServerManager.Properties.Resources.PlayHS;
            this.toolStripButtonTestServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonTestServer.Name = "toolStripButtonTestServer";
            this.toolStripButtonTestServer.Size = new System.Drawing.Size(150, 22);
            this.toolStripButtonTestServer.Text = "Test Server in Test Shell";
            this.toolStripButtonTestServer.Click += new System.EventHandler(this.toolStripButtonTestServer_Click);
            // 
            // toolStripButtonOpenTestShell
            // 
            this.toolStripButtonOpenTestShell.Image = global::ServerManager.Properties.Resources.PlayHS;
            this.toolStripButtonOpenTestShell.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpenTestShell.Name = "toolStripButtonOpenTestShell";
            this.toolStripButtonOpenTestShell.Size = new System.Drawing.Size(109, 22);
            this.toolStripButtonOpenTestShell.Text = "Open Test Shell";
            this.toolStripButtonOpenTestShell.Click += new System.EventHandler(this.toolStripButtonOpenTestShell_Click);
            // 
            // ServerManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 510);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ServerManagerForm";
            this.Text = "Server Manager";
            this.Load += new System.EventHandler(this.ServerManagerForm_Load);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ListView listViewServers;
        private System.Windows.Forms.ColumnHeader columnHeaderServerName;
        private System.Windows.Forms.ColumnHeader columnHeader;
        private System.Windows.Forms.ColumnHeader columnHeaderServerType;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem explorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysUnloadDLLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem desktopProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installServerx86ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallServerx86ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem loadServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ServerDetailsView serverDetailsView1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearMostRecentlyUsedServersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installServerx64ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallServerx64ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem registerServerx86ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerServerx64ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unregisterServerx86ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unregisterServerx64ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testServerToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonTestServer;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpShellProjectHomePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem reportABugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem requestAFeatureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diagnosticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableSharpShellLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenTestShell;
    }
}

