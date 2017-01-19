using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class Printer
    {
        public static int IdentationLevel { get; set; }
        public static int IdentationStep { get; set; } = 3;
        public static Action<string> PrintAction { get; set; } = m => Debug.WriteLine(m);
        public static void Print(string message)
        {
            if (!Enabled) return;
            PrintAction("".PadRight(IdentationLevel * IdentationStep) + message);
        }
        public static void Ident(Action action)
        {
            IdentationLevel++;
            action();
            if(IdentationLevel > 0) IdentationLevel--;
        }
        public static void Ident(string message, Action action)
        {
            Print(message);
            IdentationLevel++;
            action();
            if (IdentationLevel > 0) IdentationLevel--;
        }
        public static bool Enabled { get; set; } = true;
    }
}
