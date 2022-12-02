using Xunit.Abstractions;

namespace Fuxion.Testing;

public class XUnitLoggerProvider : ILoggerProvider
{
	public XUnitLoggerProvider(ITestOutputHelper output) => Output = output;
	public ITestOutputHelper Output { get; }
	public void Dispose() { }
	public ILogger CreateLogger(string categoryName) => new XUnitLogger(Output);
}