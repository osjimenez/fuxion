namespace Fuxion.Test.Collections.Generic;
public class IListExtensionsTest : BaseTest
{
	public IListExtensionsTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "IListExtensions - RemoveIf")]
	public void RemoveIf()
	{
		var col = new[] { new Mock(1, "One"), new Mock(1, "Two") }.ToList();
		// Remove do not remove the propper element because both are the same id and Remove use Equals overrided for detemine the element to remove
		col.Remove(col.First(m => m.Name == "Two"));
		// Only element with name "Two" is in collection
		Assert.Single(col);
		Assert.Contains(col, m => m.Name == "Two");

		col = new[] { new Mock(1, "One"), new Mock(1, "Two") }.ToList();
		col.RemoveIf(m => m.Name == "Two");
		// Only element with name "One" is in collection
		Assert.Single(col);
		Assert.Contains(col, m => m.Name == "One");
	}
}
public class Mock
{
	public Mock(int id, string name)
	{
		Id = id;
		Name = name;
	}
	public int Id { get; set; }
	public string Name { get; set; }

	public override bool Equals(object? obj)
	{
		if (obj is Mock mock) return Id == mock.Id;
		return false;
	}
	public override int GetHashCode() => Id.GetHashCode();
}