using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Testing;

public abstract class BaseTest<TBaseTest> where TBaseTest : BaseTest<TBaseTest>
{
	public BaseTest(ITestOutputHelper output)
	{
		Output = output;
		Printer.WriteLineAction = m => {
			try
			{
				output.WriteLine(m);
				Debug.WriteLine(m);
			} catch { }
		};
		var serviceCollection = new ServiceCollection().AddLogging(o => {
			o.AddProvider(new XUnitLoggerProvider(output));
			OnLoggingBuild(o);
		});
		OnConfigureServices(serviceCollection);
		ServiceProvider = serviceCollection.BuildServiceProvider();
		Logger = ServiceProvider.GetRequiredService<ILogger<TBaseTest>>();
	}
	protected void PrintVariable(object? value, [CallerArgumentExpression(nameof(value))] string? name = null) 
		=> Output.WriteLine($"{name} = {value}");
	protected void IsTrue(bool? value, string? userMessage = null, [CallerArgumentExpression(nameof(value))] string? name = null)
	{
		Assert.True(value, userMessage);
		PrintVariable(value, name);
	}
	protected void IsFalse(bool value, string? userMessage = null, [CallerArgumentExpression(nameof(value))] string? name = null)
	{
		Assert.False(value, userMessage);
		PrintVariable(value, name);
	}
	protected void Throws<TException>(Action testCode, [CallerArgumentExpression(nameof(testCode))] string? name = null)
		where TException: Exception
	{
		var ex = Assert.Throws<TException>(testCode);
		PrintVariable($"Throws '{ex.GetType().Name}' => {ex.Message}", name);
	}
	protected async Task ThrowsAsync<TException>(Func<Task> testCode, [CallerArgumentExpression(nameof(testCode))] string? name = null)
		where TException: Exception
	{
		var ex = await Assert.ThrowsAsync<TException>(testCode);
		PrintVariable($"Throws '{ex.GetType().Name}' => {ex.Message}", name);
	}
	protected void MatchFormattedString(string pattern, string input)
	{
		var regexPattern = pattern;
		for (var i = 0;; i++)
		{
			if (regexPattern.Contains($"{i}"))
				regexPattern = regexPattern.Replace($"{{{i}}}", ".*");
			else
				break;
		}
		regexPattern += "$";
		Assert.True(new Regex(regexPattern).Match(input).Success,$"String '{input}' doesn't match pattern '{pattern}'");
		Output.WriteLine($"String '{input}' match pattern '{pattern}'");
	}
	protected internal ITestOutputHelper Output { get; }
	protected internal IServiceProvider ServiceProvider { get; }
	protected internal ILogger<TBaseTest> Logger { get; }
	protected virtual void OnLoggingBuild(ILoggingBuilder loggingBuilder) { }
	protected virtual void OnConfigureServices(IServiceCollection serviceCollection) { }
	protected async Task Scoped<T>(Func<T, Task> action) where T : notnull
	{
		await using var scope = ServiceProvider.CreateAsyncScope();
		await action(scope.ServiceProvider.GetRequiredService<T>());
	}
}