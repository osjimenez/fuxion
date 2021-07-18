using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Fuxion.Testing
{
	public abstract class BaseTest
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

		protected T AssertNotNull<T>([NotNull] T? @object)
		{
			if (@object is null) throw new Xunit.Sdk.NotNullException();
			return @object;
		}
	}
}
