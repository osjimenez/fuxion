using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Events;

public class InMemoryEventStorageTest : BaseTest
{
	public InMemoryEventStorageTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "InMemoryEventStorage - Pending")]
	public void Pending() => throw new NotImplementedException();
}
