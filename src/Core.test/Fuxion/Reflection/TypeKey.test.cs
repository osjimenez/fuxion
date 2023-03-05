using Fuxion.Reflection;

namespace Fuxion.Test.Reflection;

public class TypeKeyTest : BaseTest<TypeKeyTest>
{
	public TypeKeyTest(ITestOutputHelper output):base(output){}
	[Fact(DisplayName = "Serialize TypeKey")]
	public void Serialize()
	{
		TypeKey tk = new("http://domain", "folder1", "folder2");
		Output.WriteLine($"To string: {tk}");
		Output.WriteLine($"To json: {tk.ToJson()}");
		
		tk = new("https://domain", "folder1", "folder2");
		Output.WriteLine($"To string: {tk}");
		Output.WriteLine($"To json: {tk.ToJson()}");

		Assert.Throws<ArgumentException>(() => new TypeKey("/"));
		Assert.Throws<ArgumentException>(() => new TypeKey("\\"));
	}
}