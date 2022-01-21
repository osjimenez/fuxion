namespace Fuxion.Logging;

using Microsoft.Extensions.Logging;
using System;
using static Fuxion.ConsoleTools;

public class ColorConsoleLogger : ILogger
{
	private readonly string _name;
	private readonly ColorConsoleLoggerConfiguration _config;

	public ColorConsoleLogger(
		string name,
		ColorConsoleLoggerConfiguration config) =>
		(_name, _config) = (name, config);

	public IDisposable BeginScope<TState>(TState state) => default!;

	public bool IsEnabled(LogLevel logLevel) =>
		_config.LogLevels.ContainsKey(logLevel);

	private static readonly object lockObject = new();
	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		if (_config.EventId == 0 || _config.EventId == eventId.Id)
		{
			var message = formatter(state, exception);
			var foreground = state switch
			{
				ColorConsoleState s => s.ForegroundColor,
				_ => _config.LogLevels[logLevel]()
			};
			if (_config.UseLock)
				lock (lockObject)
				{
					WriteLine(message, foreground);
				}
			else
				WriteLine(message, foreground);
		}
	}
}