using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RegistrationHandler;

public static class RegAsm {
	/// <summary>
	///     Registers the given assembly, as 32 bit.
	/// </summary>
	/// <param name="assemblyPath">The assembly path.</param>
	/// <param name="codebase">if set to <c>true</c> set the codebase flag.</param>
	/// <returns><c>true</c> if registration succeeded, <c>false</c> otherwise.</returns>
	public static bool Register32(string assemblyPath, bool codebase = true) {
		string flags = codebase ? "/codebase" : string.Empty;
		var args = $@"{flags} ""{assemblyPath}""";
		return Execute(FindRegAsmPath32(), args);
	}

	/// <summary>
	///     Registers the given assembly, as 64 bit.
	/// </summary>
	/// <param name="assemblyPath">The assembly path.</param>
	/// <param name="codebase">if set to <c>true</c> set the codebase flag.</param>
	/// <returns><c>true</c> if registration succeeded, <c>false</c> otherwise.</returns>
	public static bool Register64(string assemblyPath, bool codebase = true) {
		string flags = codebase ? "/codebase" : string.Empty;
		var args = $"{flags} \"{assemblyPath}\"";
		return Execute(FindRegAsmPath64(), args);
	}

	public static bool Register(string assemblyPath, bool codebase = true, bool? is64Bit = null) {
		is64Bit ??= Environment.Is64BitOperatingSystem;
		return (bool)is64Bit ? Register64(assemblyPath, codebase) : Register32(assemblyPath, codebase);
	}

	/// <summary>
	///     Unregisters the given assembly, as 32 bit.
	/// </summary>
	/// <param name="assemblyPath">The assembly path.</param>
	/// <returns><c>true</c> if unregistration succeeded, <c>false</c> otherwise.</returns>
	public static bool Unregister32(string assemblyPath) {
		var args = $"/u \"{assemblyPath}\"";
		return Execute(FindRegAsmPath32(), args);
	}

	/// <summary>
	///     Unregisters the given assembly, as 64 bit.
	/// </summary>
	/// <param name="assemblyPath">The assembly path.</param>
	/// <returns><c>true</c> if unregistration succeeded, <c>false</c> otherwise.</returns>
	public static bool Unregister64(string assemblyPath) {
		var args = $"/u \"{assemblyPath}\"";
		return Execute(FindRegAsmPath64(), args);
	}

	public static bool Unregister(string assemblyPath, bool? is64Bit = null) {
		is64Bit ??= Environment.Is64BitOperatingSystem;
		return (bool)is64Bit ? Unregister64(assemblyPath) : Unregister32(assemblyPath);
	}

	private static bool Execute(string regasmPath, string arguments) {
		var regasm = new Process {
			StartInfo = {
				FileName = regasmPath,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			}
		};
		regasm.Start();
		regasm.WaitForExit();
		string stdout = regasm.StandardOutput.ReadToEnd();
		string stderr = regasm.StandardError.ReadToEnd();
		if (string.IsNullOrEmpty(stderr))
			MessageBox.Show(stdout, @"注册成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
		else
			MessageBox.Show(stderr, @"注册失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
		return regasm.ExitCode == 0;
	}

	private static string FindRegAsmPath32() => FindRegAsmPath("Framework");

	private static string FindRegAsmPath64() => FindRegAsmPath("Framework64");

	/// <summary>
	///     Finds the 'regasm.exe' path, from the given framework folder.
	/// </summary>
	/// <param name="frameworkFolder">
	///     The framework folder, which would normally be <code>%WINDIR%\Microsoft.NET\Framework</code> or
	///     <code>%WINDIR%\Microsoft.NET\Framework64</code>.
	/// </param>
	/// <returns>The path to the regasm.exe executable, from the most recent .NET Framework installation.</returns>
	/// <exception cref="InvalidOperationException">Thrown if a valid regasm path cannot be found.</exception>
	private static string FindRegAsmPath(string frameworkFolder) {
		//  This function essentially will set for folders inside the 'Framework' folder, then
		//  build a path to a hypothetical 'regasm.exe' in the folder:
		//
		//  C:\WINDOWS\Microsoft.Net\Framework\v1.0.3705\regasm.exe
		//  C:\WINDOWS\Microsoft.Net\Framework\v1.1.4322\regasm.exe
		//  C:\WINDOWS\Microsoft.Net\Framework\v2.0.50727\regasm.exe
		//  C:\WINDOWS\Microsoft.Net\Framework\v4.0.30319\regasm.exe
		//
		//  It will then sort descending, and pick the first regasm which actually exists, or return null.

		//  Build an array of candidate paths - these are paths which *might* point to a valid regasm executable.
		string searchRoot = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"Microsoft.Net", frameworkFolder);
		string[] frameworkDirectories = Directory.GetDirectories(searchRoot, "v*", SearchOption.TopDirectoryOnly);
		string[] candidates = frameworkDirectories.Select(c => Path.Combine(c, @"regasm.exe")).ToArray();

		//  Sort descending, i.e. we're shooting for the latest framework available.
		var sorted = candidates.OrderByDescending(s => s);

		//  Return the first element which exists, or null.
		string path = sorted.Where(File.Exists).FirstOrDefault();

		//  If we failed to find the path, boot an exception.
		if (path == null)
			throw new InvalidOperationException($"Failed to find regasm in '{searchRoot}'. Checked: {Environment.NewLine + string.Join(Environment.NewLine, candidates)}");

		return path;
	}
}