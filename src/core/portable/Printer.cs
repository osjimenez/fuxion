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
        public static bool Enabled { get; set; } = true;
        public static int IndentationLevel { get; set; }
        public static int IndentationStep { get; set; } = 3;
        public static Action<string> WriteLineAction { get; set; } = m => Debug.WriteLine(m);
        static List<string> lineMessages = new List<string>();
        public static bool IsLineWritePending { get { lock (lineMessages) { return lineMessages.Any(); } } }
        public static void Write(string message)
        {
            if (!Enabled) return;
            lock (lineMessages)
            {
                lineMessages.Add(message);
            }
        }
        public static void WriteLine(string message)
        {
            if (!Enabled) return;
            lock (lineMessages)
            {
                WriteLineAction("".PadRight(IndentationLevel * IndentationStep) + string.Concat(lineMessages) + message);
                lineMessages.Clear();
            }
        }
        #region Indent
        public static void Indent(Action action)
        {
            IndentationLevel++;
            action();
            if(IndentationLevel > 0) IndentationLevel--;
        }
        public static void Indent(string message, Action action)
        {
            WriteLine(message);
            Indent(action);
        }
        public static T Indent<T>(Func<T> func)
        {
            IndentationLevel++;
            var res = func();
            if (IndentationLevel > 0) IndentationLevel--;
            return res;
        }
        public static T Indent<T>(string message, Func<T> func)
        {
            WriteLine(message);
            return Indent(func);
        }

        public static async Task IndentAsync(Func<Task> func)
        {
            IndentationLevel++;
            await func();
            if (IndentationLevel > 0) IndentationLevel--;
        }
        public static Task IndentAsync(string message, Func<Task> func)
        {
            WriteLine(message);
            return IndentAsync(func);
        }
        public static async Task<T> IndentAsync<T>(Func<Task<T>> func)
        {
            IndentationLevel++;
            var res = await func();
            if (IndentationLevel > 0) IndentationLevel--;
            return res;
        }
        public static Task<T> IndentAsync<T>(string message, Func<Task<T>> func)
        {
            WriteLine(message);
            return IndentAsync(func);
        }
        #endregion
        #region Foreach
        public static void Foreach<T>(string message, IEnumerable<T> items, Action<T> action)
        {
            WriteLine(message);
            Indent(() =>
            {
                foreach (var item in items)
                    action(item);
            });
        }
        public static Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action)
        {
            WriteLine(message);
            return Indent(async () =>
            {
                foreach (var item in items)
                    await action(item);
            });
        }
        #endregion
    }
}
