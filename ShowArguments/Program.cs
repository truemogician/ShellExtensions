using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowArguments {
    class Program {
        static void Main(string[] args) {
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
                strBuilder.AppendLine(args[i]);
            Console.WriteLine(strBuilder.ToString());
            Console.ReadKey();
        }
    }
}
