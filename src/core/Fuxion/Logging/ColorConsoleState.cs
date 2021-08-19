namespace Fuxion.Logging;
public sealed class ColorConsoleState
{
	public ConsoleColor ForegroundColor { get; set; }
	public ConsoleColor BackgroundColor { get; set; }
	public string Message { get; set; } = string.Empty;
	public object[] Arguments { get; set; } = Array.Empty<object>();
}