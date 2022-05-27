﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
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

	RegisteredOnOS64Bit = 4,

	UnregisteredOnOS64Bit = 8
}

[ComVisible(true)]
[COMServerAssociation(AssociationType.ClassOfExtension, ".dll")]
public class ContextMenu : SharpContextMenu {
	private static Image Icon { get; } = new Bitmap(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Icon.png"));

	protected override bool CanShowMenu() => SelectedItemPaths.Count() == 1;

	protected override ContextMenuStrip? CreateMenu() {
		string path = SelectedItemPaths.First();
		List<ServerEntry> entries;
		try {
			entries = ServerManagerApi.LoadServers(path).ToList();
		}
		catch (Exception) {
			return null;
		}
		var menu = new ContextMenuStrip();
		var status = entries.Count == 0
			? RegistrationStatus.UnregisteredOnOS32Bit
			: entries.Select(
					entry => ServerRegistrationManager.GetServerRegistrationInfo(
						entry.Server.ServerClsid,
						RegistrationType.OS32Bit
					)
				)
				.Aggregate(RegistrationStatus.Unknown, (current, info) => current | (info is null ? RegistrationStatus.UnregisteredOnOS32Bit : RegistrationStatus.RegisteredOnOS32Bit));
		if (Environment.Is64BitOperatingSystem)
			status |= entries.Count == 0
				? RegistrationStatus.UnregisteredOnOS64Bit
				: entries.Select(
						entry => ServerRegistrationManager.GetServerRegistrationInfo(
							entry.Server.ServerClsid,
							RegistrationType.OS64Bit
						)
					)
					.Aggregate(status, (current, info) => current | (info == null ? RegistrationStatus.UnregisteredOnOS64Bit : RegistrationStatus.RegisteredOnOS64Bit));
		bool status32 = status.HasFlag(RegistrationStatus.UnregisteredOnOS32Bit);
		bool status64 = status.HasFlag(RegistrationStatus.UnregisteredOnOS64Bit);
		if (status32 || status64) {
			if (status32 && status64)
				menu.Items.Add(
					new ToolStripMenuItem("注册类库", Icon) {
						DropDownItems = {
							new ToolStripMenuItem("到当前系统", null, (_, _) => Register("x64")),
							new ToolStripMenuItem("到32位子系统", null, (_, _) => Register("x86"))
						}
					}
				);
			else if (status64 || !Environment.Is64BitOperatingSystem)
				menu.Items.Add(new ToolStripMenuItem("注册类库", Icon, (_, _) => Register()));
			else
				menu.Items.Add(new ToolStripMenuItem("注册类库（到32位子系统）", Icon, (_, _) => Register("x86")));
		}
		status32 = status.HasFlag(RegistrationStatus.RegisteredOnOS32Bit);
		status64 = status.HasFlag(RegistrationStatus.RegisteredOnOS64Bit);
		if (status32 || status64) {
			if (status32 && status64)
				menu.Items.Add(
					new ToolStripMenuItem("注销类库", Icon) {
						DropDownItems = {
							new ToolStripMenuItem("从当前系统", null, (_, _) => Unregister("x64")),
							new ToolStripMenuItem("从32位子系统", null, (_, _) => Unregister("x86"))
						}
					}
				);
			else if (status64 || !Environment.Is64BitOperatingSystem)
				menu.Items.Add(new ToolStripMenuItem("注销类库", Icon, (_, _) => Unregister()));
			else
				menu.Items.Add(new ToolStripMenuItem("注销类库（从32位子系统）", Icon, (_, _) => Unregister("x86")));
		}
		return menu;
	}

	private void Register(string? targetArch = null) => Operate("r", targetArch);

	private void Unregister(string? targetArch = null) => Operate("u", targetArch);

	private void Operate(string action, string? targetArch = null) {
		targetArch ??= Environment.Is64BitOperatingSystem ? "x64" : "x86";
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