using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Helpers
{
    public class Printer
    {
        public static void Print(int ident, string message) {
            Debug.WriteLine("".PadRight(ident) + message);
        }
    }
}
