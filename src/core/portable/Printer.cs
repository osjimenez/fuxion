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
        public static Action<string> WriteLineAction { get; set; } = m => Debug.WriteLine(m);
        static List<string> lineMessages = new List<string>();
        public static bool IsLineWritePending { get { return lineMessages.Any(); } }
        public static void Write(string message)
        {
            if (!Enabled) return;
            lineMessages.Add(message);
        }
        public static void WriteLine(string message)
        {
            if (!Enabled) return;
            WriteLineAction("".PadRight(IdentationLevel * IdentationStep) + string.Concat(lineMessages) + message);
            lineMessages.Clear();
        }
        public static void Ident(Action action)
        {
            IdentationLevel++;
            action();
            if(IdentationLevel > 0) IdentationLevel--;
        }
        public static void Ident(string message, Action action)
        {
            WriteLine(message);
            Ident(action);
        }
        public static T Ident<T>(Func<T> func)
        {
            IdentationLevel++;
            var res = func();
            if (IdentationLevel > 0) IdentationLevel--;
            return res;
        }
        public static T Ident<T>(string message, Func<T> func)
        {
            WriteLine(message);
            return Ident(func);
        }
        public static void Foreach<T>(string message, IEnumerable<T> items, Action<T> action)
        {
            WriteLine(message);
            Ident(() =>
            {
                foreach (var item in items)
                    action(item);
            });
        }
        public static bool Enabled { get; set; } = true;
    }
}
