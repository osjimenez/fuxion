using Xunit.Abstractions;

namespace Fuxion.Testing;

public class XUnitLoggerProvider : ILoggerProvider
{
	public XUnitLoggerProvider(ITestOutputHelper output)
	{
		Output = output;
	}
	public void              Dispose() { }
	public ITestOutputHelper Output    { get; private set; }

	public ILogger CreateLogger(string categoryName)
	{
		return new XUnitLogger(Output);
	}
}