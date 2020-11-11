using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using RegisterShellExtension;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace TestShellExtension {
    using static Environment;
    class Program {
        public static IEnumerable<string> SelectedItemPaths;
        public static string Platform = "x64";
        public static string VSCommandPromptDirectory = Path.Combine(GetFolderPath(SpecialFolder.CommonPrograms), @"Visual Studio 2019\Visual Studio Tools\VC");
        public static string AnotherArchitecture(string arch) {
            if (arch == "x64")
                return "x86";
            else if (arch == "x86")
                return "x64";
            throw new ArgumentException("Wrong architecture name");
        }
        public static bool Register(string action, string architecture, out string errorMessage) {
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
                process.StandardInput.WriteLine($"Regasm.exe {(action == "Register" ? "/codebase" : "/u")} {SelectedItemPaths.First()}");
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }
            errorMessage = message;
            return message == null;
        }
        static void Main(string[] args) {
            string path = Console.ReadLine();
            SelectedItemPaths = (new string[] { path }).AsEnumerable();
            Register("Register", "x64",out string errorMessage);
            Console.WriteLine(errorMessage);
            Console.ReadKey();
        }
    }
}
