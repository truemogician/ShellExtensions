using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CommandLine;

#nullable enable
namespace RegistrationHandler;

internal static class Program {
	private static void Main(string[] args) {
		new Parser().ParseArguments<Options>(args)
			.WithParsed(
				options => {
					string? errorMessage = null;
					if (!new Regex(@"^r(egister)?|u(nregister)?$", RegexOptions.IgnoreCase).IsMatch(options.Action))
						errorMessage = "action参数错误：应为r(register)或u(unregister)";
					else if (!File.Exists(options.DllPath))
						errorMessage = $"dll文件\"{options.DllPath}\"不存在";
					else if (!new Regex(@"^[xX]?(86|64)$").IsMatch(options.Architecture))
						errorMessage = "arch参数错误：应为x64或x86";
					if (errorMessage is not null) {
						MessageBox.Show(errorMessage, @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
					if (options.Action[0] is 'r' or 'R')
						RegAsm.Register(options.DllPath, is64Bit: options.Architecture.EndsWith("64"));
					else
						RegAsm.Unregister(options.DllPath, options.Architecture.EndsWith("64"));
				}
			)
			.WithNotParsed(
				errors => {
					foreach (var error in errors)
						Console.WriteLine(Enum.GetName(typeof(Error), error.Tag));
				}
			);
	}
}

public class Options {
	[Value(0, Required = true)]
	public string DllPath { get; set; } = null!;

	[Option('a', "action", Default = "r")]
	public string Action { get; set; } = null!;

	[Option("arch", Default = "x64")]
	public string Architecture { get; set; } = null!;
}