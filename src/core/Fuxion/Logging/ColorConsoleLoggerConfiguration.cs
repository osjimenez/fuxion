namespace Fuxion.Logging;

using static ConsoleTools;

// https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
public class ColorConsoleLoggerConfiguration
{
	public int EventId { get; set; }
	public string Title
	{
		get => Console.Title;
		set => Console.Title = value;
	}
	public Func<string> SingleLinePrefix
	{
		get => ConsoleTools.SingleLinePrefix;
		set => ConsoleTools.SingleLinePrefix = value;
	}
	public Func<string> FirstLinePrefix
	{
		get => ConsoleTools.FirstLinePrefix;
		set => ConsoleTools.FirstLinePrefix = value;
	}
	public Func<string> MidLinePrefix
	{
		get => ConsoleTools.MidLinePrefix;
		set => ConsoleTools.MidLinePrefix = value;
	}
	public Func<string> LastLinePrefix
	{
		get => ConsoleTools.LastLinePrefix;
		set => ConsoleTools.LastLinePrefix = value;
	}
	public Func<string> MessagePrefix
	{
		get => ConsoleTools.MessagePrefix;
		set => ConsoleTools.MessagePrefix = value;
	}
	public Func<string> FirstLineSufix
	{
		get => ConsoleTools.FirstLineSufix;
		set => ConsoleTools.FirstLineSufix = value;
	}
	public Func<string> MidLineSufix
	{
		get => ConsoleTools.MidLineSufix;
		set => ConsoleTools.MidLineSufix = value;
	}
	public Func<string> LastLineSufix
	{
		get => ConsoleTools.LastLineSufix;
		set => ConsoleTools.LastLineSufix = value;
	}
	public Func<string> MessageSufix
	{
		get => ConsoleTools.MessageSufix;
		set => ConsoleTools.MessageSufix = value;
	}
	public bool UseLock { get; set; }
	public Dictionary<LogLevel, Func<ConsoleColor>> LogLevels { get; set; } = new() {
		[LogLevel.Critical] = () => CriticalColor,
		[LogLevel.Error] = () => ErrorColor,
		[LogLevel.Warning] = () => WarnColor,
		[LogLevel.Information] = () => InfoColor,
		[LogLevel.Debug] = () => DebugColor,
		[LogLevel.Trace] = () => TraceColor,
		[LogLevel.None] = () => ConsoleColor.Black
	};
}