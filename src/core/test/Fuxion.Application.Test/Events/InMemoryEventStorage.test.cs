namespace Fuxion.Application.Test.Events;

using Fuxion.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class InMemoryEventStorageTest : BaseTest
{
	public InMemoryEventStorageTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "InMemoryEventStorage - Pending")]
	public void Pending()
	{
		throw new NotImplementedException();
	}
}
