using System;
using System.IO;
using System.Diagnostics;

namespace RegistrationHandler {
    using static System.Environment;
    class Program {
        static string AnotherArchitecture(string arch) {
            if (arch == "x64")
                return "x86";
            else if (arch == "x86")
                return "x64";
            throw new ArgumentException("Wrong architecture name");
        }
        static void Main(string[] mainArgs) {
            if (mainArgs.Length < 4)
                return;
            string action = mainArgs[0], dllFile = mainArgs[1], hostArch = mainArgs[2], targetArch = mainArgs[3];
            string VSCommandPromptDirectory = Path.Combine(GetFolderPath(SpecialFolder.CommonPrograms), @"Visual Studio 2019\Visual Studio Tools\VC");
            string[] links = Directory.GetFiles(VSCommandPromptDirectory);
            string target = null;
            foreach (string link in links) {
                string linkName = Path.GetFileName(link);
                if (linkName.Substring(0, 3) == hostArch) {
                    if (targetArch == hostArch && linkName.IndexOf(AnotherArchitecture(targetArch)) == -1) {
                        target = link;
                        break;
                    }
                    else if (targetArch == AnotherArchitecture(hostArch) && linkName.IndexOf(targetArch) != -1) {
                        target = link;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(target))
                return;
            using (var process = Process.Start(new ProcessStartInfo {
                FileName = "cmd.exe",
                Arguments = $"/c \"{target}\"",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardInput = true,
            })) {
                process.StandardInput.WriteLine($"Regasm.exe {(action == "Register" ? "/codebase" : "/u")} \"{dllFile}\"");
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }
        }
    }
}
