using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ServerManager;
using SharpShell.Attributes;
using SharpShell.ServerRegistration;
using SharpShell.SharpContextMenu;

namespace DllRegistrator;

[Flags]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum RegistrationStatus {
	Unknown = 0,

	RegisteredOnOS32Bit = 1,

	UnregisteredOnOS32Bit = 2,

	PartialOnOS32Bit = 3,

	RegisteredOnOS64Bit = 4,

	UnregisteredOnOS64Bit = 8,

	PartialOnOS64Bit = 12
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.ClassOfExtension, ".dll")]
public class ContextMenu : SharpContextMenu {
	protected override bool CanShowMenu() => SelectedItemPaths.Count() == 1;

	protected override ContextMenuStrip CreateMenu() {
		string path = SelectedItemPaths.First();
		List<ServerEntry> entries;
		try {
			entries = ServerManagerApi.LoadServers(path).ToList();
		}
		catch (Exception) {
			return null;
		}
		var menu = new ContextMenuStrip();
		var item = new ToolStripMenuItem("注册类库");
		var status = RegistrationStatus.Unknown;
		if (entries.Count != 0)
			status = entries.Select(entry => ServerRegistrationManager.GetServerRegistrationInfo(entry.Server.ServerClsid, RegistrationType.OS32Bit)).Aggregate(status, (current, info) => current | (info == null ? RegistrationStatus.UnregisteredOnOS32Bit : RegistrationStatus.RegisteredOnOS32Bit));
		else
			status |= RegistrationStatus.UnregisteredOnOS32Bit;
		if (Environment.Is64BitOperatingSystem) {
			if (entries.Count != 0)
				status = entries.Select(entry => ServerRegistrationManager.GetServerRegistrationInfo(entry.Server.ServerClsid, RegistrationType.OS64Bit)).Aggregate(status, (current, info) => current | (info == null ? RegistrationStatus.UnregisteredOnOS64Bit : RegistrationStatus.RegisteredOnOS64Bit));
			else
				status |= RegistrationStatus.UnregisteredOnOS64Bit;
			if (!status.HasFlag(RegistrationStatus.UnregisteredOnOS32Bit) && !status.HasFlag(RegistrationStatus.UnregisteredOnOS64Bit))
				item.Text = "注销类库";
			if (status.HasFlag(RegistrationStatus.UnregisteredOnOS64Bit))
				item.DropDownItems.Add(new ToolStripMenuItem("注册", null, (_, _) => Register("r", "x64")));
			if (status.HasFlag(RegistrationStatus.RegisteredOnOS64Bit))
				item.DropDownItems.Add(new ToolStripMenuItem("注销", null, (_, _) => Register("u", "x64")));
			if (status.HasFlag(RegistrationStatus.UnregisteredOnOS32Bit))
				item.DropDownItems.Add(new ToolStripMenuItem("注册到32位子系统", null, (_, _) => Register("r", "x86")));
			if (status.HasFlag(RegistrationStatus.RegisteredOnOS32Bit))
				item.DropDownItems.Add(new ToolStripMenuItem("从32位子系统注销", null, (_, _) => Register("u", "x86")));
		}
		else {
			if (status == RegistrationStatus.PartialOnOS32Bit) {
				item.DropDownItems.Add(new ToolStripMenuItem("注册", null, (_, _) => Register("r", "x86")));
				item.DropDownItems.Add(new ToolStripMenuItem("注销", null, (_, _) => Register("u", "x86")));
			}
			else {
				item.Click += (_, _) => Register(status == RegistrationStatus.UnregisteredOnOS32Bit ? "r" : "u", "x86");
				if (status == RegistrationStatus.RegisteredOnOS32Bit)
					item.Text = "注销类库";
			}
		}
		menu.Items.Add(item);
		return menu;
	}

	protected void Register(string action, string targetArch) {
		string registrationHandler = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "RegistrationHandler.exe");
		Process.Start(
			new ProcessStartInfo {
				FileName = registrationHandler,
				Arguments = $"\"{SelectedItemPaths.First()}\" --action {action} --arch {targetArch}",
				Verb = "runas"
			}
		);
	}
}