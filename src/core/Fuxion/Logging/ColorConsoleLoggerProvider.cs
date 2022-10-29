using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Fuxion.Logging;

public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
	public ColorConsoleLoggerProvider(IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
	{
		_currentConfig = config.CurrentValue;
		_onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
	}
	readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new();
	readonly IDisposable?                                     _onChangeToken;
	ColorConsoleLoggerConfiguration                           _currentConfig;
	public ILogger                                            CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new(name, _currentConfig));
	public void Dispose()
	{
		_loggers.Clear();
		_onChangeToken?.Dispose();
	}
}