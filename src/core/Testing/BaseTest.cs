using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Fuxion.Testing;

public abstract class BaseTest<TBaseTest> where TBaseTest : BaseTest<TBaseTest>
{
	public BaseTest(ITestOutputHelper output)
	{
		Output = output;
		Printer.WriteLineAction = m =>
		{
			try
			{
				output.WriteLine(m);
				Debug.WriteLine(m);
			} catch { }
		};
		var serviceCollection = new ServiceCollection().AddLogging(o =>
		{
			o.AddProvider(new XUnitLoggerProvider(output));
			OnLoggingBuild(o);
		});
		OnConfigureServices(serviceCollection);
		ServiceProvider = serviceCollection.BuildServiceProvider();
		Logger = ServiceProvider.GetRequiredService<ILogger<TBaseTest>>();
	}
	protected internal ITestOutputHelper Output { get; }
	protected internal IServiceProvider ServiceProvider { get; }
	protected internal ILogger<TBaseTest> Logger { get; }
	protected virtual void OnLoggingBuild(ILoggingBuilder loggingBuilder) { }
	protected virtual void OnConfigureServices(IServiceCollection serviceCollection) { }
}