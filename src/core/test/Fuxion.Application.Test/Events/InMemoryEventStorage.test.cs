using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Events;

public class InMemoryEventStorageTest : BaseTest<InMemoryEventStorageTest>
{
	public InMemoryEventStorageTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "InMemoryEventStorage - Pending", Skip = "Not implemented yet")]
	public void Pending() => throw new NotImplementedException();
}