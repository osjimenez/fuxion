using System.Text;

namespace Fuxion;

public static class ConsoleTools
{
	public static Func<string> SingleLinePrefix { get; set; } = () => "";
	public static Func<string> FirstLinePrefix { get; set; } = () => "";
	public static Func<string> MidLinePrefix { get; set; } = () => "";
	public static Func<string> LastLinePrefix { get; set; } = () => "";
	public static Func<string> MessagePrefix { get; set; } = () => "";
	public static Func<string> FirstLineSufix { get; set; } = () => "";
	public static Func<string> MidLineSufix { get; set; } = () => "";
	public static Func<string> LastLineSufix { get; set; } = () => "";
	public static Func<string> MessageSufix { get; set; } = () => "";
	public static ConsoleColor OkColor { get; set; } = ConsoleColor.DarkGreen;
	public static ConsoleColor InfoColor { get; set; } = ConsoleColor.DarkCyan;
	public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.DarkRed;
	public static ConsoleColor WarnColor { get; set; } = ConsoleColor.DarkYellow;
	public static ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;
	public static ConsoleColor TraceColor { get; set; } = ConsoleColor.DarkGray;
	public static ConsoleColor NoticeColor { get; set; } = ConsoleColor.Magenta;
	public static ConsoleColor CriticalColor { get; set; } = ConsoleColor.Red;
	public static ConsoleColor HighlightColor { get; set; } = ConsoleColor.White;
	static void SetOriginal()
	{
		//Console.BackgroundColor = ConsoleColor.Black;
		//Console.BufferHeight = 9001;
		//Console.BufferWidth = 120;
		//Console.ForegroundColor = ConsoleColor.Gray;
		//Console.Title = "TITLE";
		//Console.WindowHeight = 30;
		//Console.WindowWidth = 120;
		//Console.WindowLeft = 0;
		//Console.WindowTop = 0;
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Gray;
	}
	public static void WriteLine(string message, ConsoleColor foregorund = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
	{
		var originalForeground = Console.ForegroundColor;
		var originalBackground = Console.BackgroundColor;
		Console.ForegroundColor = foregorund;
		Console.BackgroundColor = background;
		var lines = message.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
		StringBuilder sb = new();
		for (var i = 0; i < lines.Count; i++)
			if (i == 0 && lines.Count == 1)
				sb.Append(SingleLinePrefix() + lines[i].Trim('\r', '\n') + FirstLineSufix());
			else if (i == 0 && lines.Count > 1)
				sb.AppendLine(FirstLinePrefix() + lines[i].Trim('\r', '\n') + FirstLineSufix());
			else if (i == 0)
				sb.Append(FirstLinePrefix() + lines[i].Trim('\r', '\n') + FirstLineSufix());
			else if (i == lines.Count - 1)
				sb.Append(LastLinePrefix() + lines[i].Trim('\r', '\n') + LastLineSufix());
			else
				sb.AppendLine(MidLinePrefix() + lines[i].Trim('\r', '\n') + MidLineSufix());
		Console.WriteLine($"{MessagePrefix()}{sb}{MessageSufix()}");
		Console.ForegroundColor = originalForeground;
		Console.BackgroundColor = originalBackground;
	}
	public static void Ok(string message) => WriteLine(message, OkColor);
	public static void Info(string message) => WriteLine(message, InfoColor);
	public static void Error(string message) => WriteLine(message, ErrorColor);
	public static void Warn(string message) => WriteLine(message, WarnColor);
	public static void Debug(string message) => WriteLine(message, DebugColor);
	public static void Trace(string message) => WriteLine(message, TraceColor);
	public static void Notice(string message) => WriteLine(message, NoticeColor);
	public static void Critical(string message) => WriteLine(message, CriticalColor);
	public static void Highlight(string message) => WriteLine(message, HighlightColor);
}