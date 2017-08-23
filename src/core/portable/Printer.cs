using Fuxion.Resources;
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
        private Printer() { }

        static Dictionary<object, PrinterInstance> factories = new Dictionary<object, PrinterInstance>();

        public static IPrinter Default { get; } = new PrinterInstance();
        public static IPrinter WithKey(object key)
        {
            if (key == null) return Default;
            if (factories.ContainsKey(key))
                return factories[key];
            else
                return factories[key] = new PrinterInstance { Key = key };
        }
        public static bool Enabled { get => Default.Enabled; set => Default.Enabled = value; }
        public static int IndentationLevel { get => Default.IndentationLevel; set => Default.IndentationLevel = value; }
        public static int IndentationStep { get => Default.IndentationStep; set => Default.IndentationStep = value; }
        public static char IndentationChar { get => Default.IndentationChar; set => Default.IndentationChar = value; }
        public static string GetPrinted(Action action)
        {
            StringBuilder res = new StringBuilder();
            //var def = Printer.Default;
            var def = WriteLineAction;
            WriteLineAction = m =>
            {
                res.AppendLine(m);
                def(m);
            };
            action?.Invoke();
            return res.ToString();
        }
        [DebuggerHidden]
        public static Action<string> WriteLineAction { get => Default.WriteLineAction; set => Default.WriteLineAction = value; }
        public static bool IsLineWritePending { get => Default.IsLineWritePending; }
        [DebuggerHidden]
        public static void Write(string message) => Default.Write(message);
        [DebuggerHidden]
        public static void WriteLine(string message) => Default.WriteLine(message);
        [DebuggerHidden]
        public static void Indent(Action action) => Default.Indent(action);
        [DebuggerHidden]
        public static IDisposable Indent2(char? verticalConnectorChar = null) => Default.Indent2(verticalConnectorChar);
        [DebuggerHidden]
        public static void Indent(string message, Action action) => Default.Indent(message, action);
        [DebuggerHidden]
        public static IDisposable Indent2(string message, char? verticalConnectorChar = null) => Default.Indent2(message, verticalConnectorChar);
        [DebuggerHidden]
        public static T Indent<T>(Func<T> func) => Default.Indent(func);
        [DebuggerHidden]
        public static T Indent<T>(string message, Func<T> func) => Default.Indent(message, func);
        [DebuggerHidden]
        public static Task IndentAsync(Func<Task> func) => Default.IndentAsync(func);
        [DebuggerHidden]
        public static Task IndentAsync(string message, Func<Task> func) => Default.IndentAsync(message, func);
        [DebuggerHidden]
        public static Task<T> IndentAsync<T>(Func<Task<T>> func) => Default.IndentAsync(func);
        [DebuggerHidden]
        public static Task<T> IndentAsync<T>(string message, Func<Task<T>> func) => Default.IndentAsync(message, func);
        [DebuggerHidden]
        public static void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true)
            => Default.Foreach(message, items, action, printMessageIfNoItems);
        [DebuggerHidden]
        public static Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true)
            => Default.ForeachAsync(message, items, action, printMessageIfNoItems);
    }
    class PrinterInstance : IPrinter
    {
        public object Key { get; set; }
        public bool Enabled { get; set; } = true;
        public int IndentationLevel { get; set; }
        public int IndentationStep { get; set; } = 3;
        public char IndentationChar { get; set; } = ' ';
        //public char VerticalConectorChar { get; set; } = '│';
        //List<int> verticalConnectorLevels = new List<int>();
        Dictionary<int, char> verticalConnectorLevels = new Dictionary<int, char>();
        [DebuggerHidden]
        public Action<string> WriteLineAction { get; set; } = m => Debug.WriteLine(m);
        List<string> lineMessages = new List<string>();
        public bool IsLineWritePending { get { lock (lineMessages) { return lineMessages.Any(); } } }
        [DebuggerHidden]
        public void Write(string message)
        {
            if (!Enabled) return;
            lock (lineMessages)
            {
                lineMessages.Add(message);
            }
        }
        [DebuggerHidden]
        public void WriteLine(string message)
        {
            if (!Enabled) return;
            lock (lineMessages)
            {
                var indent = "";
                for (int i = 0; i < IndentationLevel; i++)
                {
                    var step = "";
                    if (verticalConnectorLevels.ContainsKey(i))
                    {
                        step += verticalConnectorLevels[i];
                    }
                    else
                    {
                        step += IndentationChar;
                    }
                    indent += step.PadRight(IndentationStep, IndentationChar);
                }
                //WriteLineAction("".PadRight(IndentationLevel * IndentationStep, IndentationChar) + string.Concat(lineMessages) + message);
                WriteLineAction(indent + string.Concat(lineMessages) + message);
                lineMessages.Clear();
            }
        }
        #region Indent
        [DebuggerHidden]
        public void Indent(Action action)
        {
            IndentationLevel++;
            action();
            if (IndentationLevel > 0) IndentationLevel--;
        }
        [DebuggerHidden]
        public IDisposable Indent2(char? verticalConnectorChar = null)
        {
            var o = new object();
            var currentIndentationLevel = IndentationLevel;
            if (verticalConnectorChar != null)
                verticalConnectorLevels.Add(currentIndentationLevel, verticalConnectorChar.Value);
            IndentationLevel++;
            return o.AsDisposable(_ =>
            {
                if (IndentationLevel > 0)
                    IndentationLevel--;
                verticalConnectorLevels.Remove(currentIndentationLevel);
            });
        }
        [DebuggerHidden]
        public void Indent(string message, Action action)
        {
            WriteLine(message);
            Indent(action);
        }
        [DebuggerHidden]
        public IDisposable Indent2(string message, char? verticalConnectorChar = null)
        {
            WriteLine(message);
            return Indent2(verticalConnectorChar);
        }
        [DebuggerHidden]
        public T Indent<T>(Func<T> func)
        {
            IndentationLevel++;
            var res = func();
            if (IndentationLevel > 0) IndentationLevel--;
            return res;
        }
        [DebuggerHidden]
        public T Indent<T>(string message, Func<T> func)
        {
            WriteLine(message);
            return Indent(func);
        }
        [DebuggerHidden]
        public async Task IndentAsync(Func<Task> func)
        {
            IndentationLevel++;
            await func();
            if (IndentationLevel > 0) IndentationLevel--;
        }
        [DebuggerHidden]
        public Task IndentAsync(string message, Func<Task> func)
        {
            WriteLine(message);
            return IndentAsync(func);
        }
        [DebuggerHidden]
        public async Task<T> IndentAsync<T>(Func<Task<T>> func)
        {
            IndentationLevel++;
            var res = await func();
            if (IndentationLevel > 0) IndentationLevel--;
            return res;
        }
        [DebuggerHidden]
        public Task<T> IndentAsync<T>(string message, Func<Task<T>> func)
        {
            WriteLine(message);
            return IndentAsync(func);
        }
        #endregion
        #region Foreach
        [DebuggerHidden]
        public void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true)
        {
            if (!printMessageIfNoItems && !items.Any()) return;
            WriteLine(message);
            Indent(() =>
            {
                foreach (var item in items)
                    action(item);
            });
        }
        [DebuggerHidden]
        public Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true)
        {
            if (!printMessageIfNoItems && !items.Any()) return Task.FromResult(0);
            WriteLine(message);
            return IndentAsync(async () =>
            {
                foreach (var item in items)
                    await action(item);
            });
        }
        #endregion
    }
    public interface IPrinter
    {
        bool Enabled { get; set; }
        int IndentationLevel { get; set; }
        int IndentationStep { get; set; }
        char IndentationChar { get; set; }
        [DebuggerHidden]
        Action<string> WriteLineAction { get; set; }
        bool IsLineWritePending { get; }
        [DebuggerHidden]
        void Write(string message);
        [DebuggerHidden]
        void WriteLine(string message);
        [DebuggerHidden]
        void Indent(Action action);
        [DebuggerHidden]
        IDisposable Indent2(char? verticalConnectorChar = null);
        [DebuggerHidden]
        void Indent(string message, Action action);
        [DebuggerHidden]
        IDisposable Indent2(string message, char? verticalConnectorChar = null);
        [DebuggerHidden]
        T Indent<T>(Func<T> func);
        [DebuggerHidden]
        T Indent<T>(string message, Func<T> func);
        [DebuggerHidden]
        Task IndentAsync(Func<Task> func);
        [DebuggerHidden]
        Task IndentAsync(string message, Func<Task> func);
        [DebuggerHidden]
        Task<T> IndentAsync<T>(Func<Task<T>> func);
        [DebuggerHidden]
        Task<T> IndentAsync<T>(string message, Func<Task<T>> func);
        [DebuggerHidden]
        void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true);
        [DebuggerHidden]
        Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true);
    }
}
