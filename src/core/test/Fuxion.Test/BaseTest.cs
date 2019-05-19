using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Fuxion.Test
{
	public class BaseTest
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
				}
				catch { }
			};
		}
		protected ITestOutputHelper Output { get; private set; }
	}
	public interface IO
	{
		void Vamos() => Console.WriteLine("");
	}
}
