using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Fuxion.Test.Helpers
{
	public class XunitLogger : ILogger
	{
		public XunitLogger(ITestOutputHelper output)
		{
			this.output = output;
		}
		ITestOutputHelper output;

		public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
		public bool IsEnabled(LogLevel logLevel) => true;
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) 
			=> output.WriteLine(state?.ToString());
	}
}
