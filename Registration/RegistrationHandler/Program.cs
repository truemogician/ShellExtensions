using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RegistrationHandler;

internal static class Program {
	private static void Main(string[] args) {
		string errMessage = null;
		if (args.Length is < 2 or > 3)
			errMessage = $"参数过{(args.Length < 2 ? "少" : "多")}";
		else if (!new Regex(@"^r(egister)?|u(nregister)?$", RegexOptions.IgnoreCase).IsMatch(args[0]))
			errMessage = "行为未定义";
		else if (!File.Exists(args[1]))
			errMessage = "目标DLL不存在";
		else if (args.Length == 3 && !new Regex(@"^[xX]?(86|64)$").IsMatch(args[2]))
			errMessage = "架构无法识别";
		if (!string.IsNullOrEmpty(errMessage)) {
			MessageBox.Show(errMessage, @"参数错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		bool? is64Bit = args.Length == 3 ? args[2].EndsWith("64") : null;
		if (args[0][0] == 'r' || args[0][0] == 'R')
			RegAsm.Register(args[1], is64Bit: is64Bit);
		else
			RegAsm.Unregister(args[1], is64Bit);
	}
}