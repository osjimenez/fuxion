using Xunit.Abstractions;

namespace Fuxion.Testing;

// https://stackoverflow.com/questions/43680174/entity-framework-core-log-queries-for-a-single-db-context-instance
public class XUnitLogger : ILogger
{
	public XUnitLogger(ITestOutputHelper output)
	{
		Output = output;
	}
	
	public ITestOutputHelper Output { get; }

	public void Log<TState>(LogLevel                         logLevel, EventId eventId, TState state, Exception? exception,
									Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		if (formatter == null)
			throw new ArgumentNullException(nameof(formatter));

		var message = formatter(state, exception);
		if (string.IsNullOrEmpty(message) && exception == null)
			return;

		var line = $"{logLevel}: {message.Replace("\n",$"\n{logLevel}: ")}";

		Output.WriteLine(line);

		if (exception != null)
			Output.WriteLine(exception.ToString());
	}
	public bool        IsEnabled(LogLevel        logLevel)                     => true;
	public IDisposable BeginScope<TState>(TState state) where TState : notnull => new XUnitScope();
}