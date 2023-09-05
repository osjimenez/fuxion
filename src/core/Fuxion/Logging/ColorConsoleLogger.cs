namespace Fuxion.Logging;

using static ConsoleTools;

public class ColorConsoleLogger : ILogger
{
	public ColorConsoleLogger(string name, ColorConsoleLoggerConfiguration config) => (_name, _config) = (name, config);
	static readonly object lockObject = new();
	readonly ColorConsoleLoggerConfiguration _config;
	readonly string _name;
	public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
	public bool IsEnabled(LogLevel logLevel) => _config.LogLevels.ContainsKey(logLevel);
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel)) return;
		if (_config.EventId == 0 || _config.EventId == eventId.Id)
		{
			var message = formatter(state, exception);
			var foreground = state switch {
				ColorConsoleState s => s.ForegroundColor,
				_                   => _config.LogLevels[logLevel]()
			};
			if (_config.UseLock)
				lock (lockObject)
					WriteLine(message, foreground);
			else
				WriteLine(message, foreground);
		}
	}
}