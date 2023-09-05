using Xunit.Abstractions;

namespace Fuxion.Xunit;

public class XunitLoggerProvider : ILoggerProvider
{
	public XunitLoggerProvider(ITestOutputHelper output) => Output = output;
	public ITestOutputHelper Output { get; }
	public void Dispose() { }
	public ILogger CreateLogger(string categoryName) => new XunitLogger(Output);
}