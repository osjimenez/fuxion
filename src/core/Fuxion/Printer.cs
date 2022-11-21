using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Fuxion.Threading;
using Extensions = System.Extensions;

namespace Fuxion;

using static Extensions;

public class Printer
{
	Printer() { }
	static readonly Dictionary<object, PrinterInstance> factories = new();
	public static   IPrinter                            Default { get; } = new PrinterInstance();
	public static IPrinter WithKey(object key)
	{
		if (key == null) return Default;
		if (factories.ContainsKey(key)) return factories[key];
		return factories[key] = new()
		{
			Key = key
		};
	}
	public static string GetPrinted(Action action)
	{
		var res = new StringBuilder();
		var def = WriteLineAction;
		WriteLineAction = m =>
		{
			res.AppendLine(m);
			def(m);
		};
		action?.Invoke();
		return res.ToString();
	}

	#region IPrinter methods
	public static bool Enabled
	{
		get => Default.Enabled;
		set => Default.Enabled = value;
	}
	public static int IndentationLevel
	{
		get => Default.IndentationLevel;
		set => Default.IndentationLevel = value;
	}
	public static int IndentationStep
	{
		get => Default.IndentationStep;
		set => Default.IndentationStep = value;
	}
	public static char IndentationChar
	{
		get => Default.IndentationChar;
		set => Default.IndentationChar = value;
	}
	[DebuggerHidden]
	public static Action<string> WriteLineAction
	{
		get => Default.WriteLineAction;
		set => Default.WriteLineAction = value;
	}
	public static bool IsLineWritePending => Default.IsLineWritePending;
	[DebuggerHidden]
	public static void Write(string message) => Default.Write(message);
	[DebuggerHidden]
	public static void WriteLine(string message) => Default.WriteLine(message);
	public static CallResultArg<T> CallResult<T>(string callMessage = "CALL {0}:", string resultMessage = "RESULT {0}: {1}", char verticalConnectorChar = '│', char resultConnectorChar = '●',
																[CallerMemberName] string? caller = null) where T : notnull =>
		Default.CallResult<T>(callMessage, resultMessage, verticalConnectorChar, resultConnectorChar, caller);
	[DebuggerHidden]
	public static IDisposable Indent(char? verticalConnectorChar = null) => Default.Indent(verticalConnectorChar);
	[DebuggerHidden]
	public static IDisposable Indent(string message, char? verticalConnectorChar = null) => Default.Indent(message, verticalConnectorChar);
	[DebuggerHidden]
	public static void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true) => Default.Foreach(message, items, action, printMessageIfNoItems);
	[DebuggerHidden]
	public static Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true) =>
		Default.ForeachAsync(message, items, action, printMessageIfNoItems);
	#endregion
}

public class CallResultArg<T> : DisposableEnvelope<T> where T : notnull
{
	public CallResultArg(T obj, Action<T> actionOnDispose, string resultFormat, char resultConnectorChar, string? caller) : base(obj, actionOnDispose)
	{
		this.resultFormat        = resultFormat;
		this.resultConnectorChar = resultConnectorChar;
		this.caller              = caller;
	}
	readonly string?    caller;
	readonly char       resultConnectorChar;
	readonly string     resultFormat;
	public   Action<T>? OnPrintResult { get; set; }
	protected override void OnDispose()
	{
		base.OnDispose();
		var dis = Printer.Indent(resultConnectorChar);
		Printer.WriteLine(string.Format(resultFormat, caller, Value));
		OnPrintResult?.Invoke(Value);
		dis.Dispose();
	}
}

class PrinterInstance : IPrinter
{
	readonly List<string>                  lineMessages            = new();
	readonly Locker<Dictionary<int, char>> verticalConnectorLevels = new(new());
	public   object?                       Key              { get; set; }
	public   bool                          Enabled          { get; set; } = true;
	public   int                           IndentationLevel { get; set; }
	public   int                           IndentationStep  { get; set; } = 3;
	public   char                          IndentationChar  { get; set; } = ' ';
	[DebuggerHidden]
	public Action<string> WriteLineAction { get; set; } = m => Debug.WriteLine(m);
	public bool IsLineWritePending
	{
		get
		{
			lock (lineMessages) return lineMessages.Any();
		}
	}
	[DebuggerHidden]
	public void Write(string message)
	{
		if (!Enabled) return;
		lock (lineMessages) lineMessages.Add(message);
	}
	[DebuggerHidden]
	public void WriteLine(string message)
	{
		if (!Enabled) return;
		lock (lineMessages)
		{
			var indent = "";
			for (var i = 0; i < IndentationLevel; i++)
			{
				var step = "";
				var t    = verticalConnectorLevels.Read(dic => dic.ContainsKey(i) ? dic[i] : (char?)null);
				if (t is not null && t.HasValue)
					step += t.Value;
				else
					step += IndentationChar;
				indent += step.PadRight(IndentationStep, IndentationChar);
			}
			WriteLineAction(indent + string.Concat(lineMessages) + message);
			lineMessages.Clear();
		}
	}
	[DebuggerHidden]
	public CallResultArg<T> CallResult<T>(string callMessage = "CALL {0}:", string resultMessage = "RESULT {0}: {1}", char verticalConnectorChar = '│', char resultConnectorChar = '●',
													  [CallerMemberName] string? caller = null) where T : notnull
	{
		var dis = Printer.Indent(string.Format(callMessage, caller), verticalConnectorChar);
		var res = new CallResultArg<T>(default!, _ => dis.Dispose(), resultMessage, resultConnectorChar, caller);
		return res;
	}

	#region Indent
	[DebuggerHidden]
	public IDisposable Indent(char? verticalConnectorChar = null)
	{
		var o                       = new object();
		var currentIndentationLevel = IndentationLevel;
		if (verticalConnectorChar is not null)
			verticalConnectorLevels.Write(dic =>
			{
				if (!dic.ContainsKey(currentIndentationLevel)) dic.Add(currentIndentationLevel, verticalConnectorChar!.Value);
			});
		IndentationLevel++;
		return o.AsDisposable(_ =>
		{
			if (IndentationLevel > 0) IndentationLevel--;
			verticalConnectorLevels.Write(dic => dic.Remove(currentIndentationLevel));
		});
	}
	[DebuggerHidden]
	public IDisposable Indent(string message, char? verticalConnectorChar = null)
	{
		WriteLine(message);
		return Indent(verticalConnectorChar);
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
	#endregion

	#region Foreach
	[DebuggerHidden]
	public void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true)
	{
		if (!printMessageIfNoItems && !items.Any()) return;
		WriteLine(message);
		using (Indent())
			foreach (var item in items)
				action(item);
	}
	[DebuggerHidden]
	public async Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true)
	{
		if (!printMessageIfNoItems && !items.Any()) return;
		WriteLine(message);
		using (Indent())
			foreach (var item in items)
				await action(item);
	}
	#endregion
}

public interface IPrinter
{
	bool Enabled            { get; set; }
	int  IndentationLevel   { get; set; }
	int  IndentationStep    { get; set; }
	char IndentationChar    { get; set; }
	bool IsLineWritePending { get; }
	[DebuggerHidden]
	Action<string> WriteLineAction { get; set; }
	[DebuggerHidden]
	void Write(string message);
	[DebuggerHidden]
	void WriteLine(string message);
	[DebuggerHidden]
	IDisposable Indent(char? verticalConnectorChar = null);
	[DebuggerHidden]
	CallResultArg<T> CallResult<T>(string callMessage, string resultMessage, char verticalConnectorChar, char resultConnectorChar, [CallerMemberName] string? caller = null) where T : notnull;
	[DebuggerHidden]
	IDisposable Indent(string message, char? verticalConnectorChar = null);
	[DebuggerHidden]
	void Foreach<T>(string message, IEnumerable<T> items, Action<T> action, bool printMessageIfNoItems = true);
	[DebuggerHidden]
	Task ForeachAsync<T>(string message, IEnumerable<T> items, Func<T, Task> action, bool printMessageIfNoItems = true);
}