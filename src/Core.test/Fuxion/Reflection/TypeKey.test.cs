using Fuxion.Reflection;

namespace Fuxion.Test.Reflection;

public class TypeKeyTest(ITestOutputHelper output) : BaseTest<TypeKeyTest>(output)
{
	[Fact(DisplayName = "Serialize TypeKey")]
	public void Serialize()
	{
		TypeKey tk = new("http://domain", "folder1", "folder2");
		Output.WriteLine($"To string: {tk}");
		Output.WriteLine($"To json: {tk.SerializeToJson()}");
		
		tk = new("https://domain", "folder1", "folder2");
		Output.WriteLine($"To string: {tk}");
		Output.WriteLine($"To json: {tk.SerializeToJson()}");

		Assert.Throws<TypeKeyException>(() => new TypeKey("/"));
		Assert.Throws<TypeKeyException>(() => new TypeKey("\\"));
	}
	[Fact(DisplayName = "Deserialize TypeKey")]
	public void Deserialize()
	{
		var tk = "\"folder1/folder2\"".DeserializeFromJson<TypeKey>();
		Assert.NotNull(tk);
		Output.WriteLine($"Chain:\n\t{tk.KeyChain.Aggregate((a,c)=>$"{a}\n\t{c}")}");
		Assert.Equal("folder1",tk.KeyChain[0]);
		Assert.Equal("folder2",tk.KeyChain[1]);

		tk = "\"http://domain.com/folder1/folder2\"".DeserializeFromJson<TypeKey>();
		Assert.NotNull(tk);
		Output.WriteLine($"Chain:\n\t{tk.KeyChain.Aggregate((a,c)=>$"{a}\n\t{c}")}");
		Assert.Equal("http://domain.com",tk.KeyChain[0]);
		Assert.Equal("folder1",tk.KeyChain[1]);
		Assert.Equal("folder2",tk.KeyChain[2]);
		
		tk = "\"https://domain/folder1/folder2\"".DeserializeFromJson<TypeKey>();
		Assert.NotNull(tk);
		Output.WriteLine($"Chain:\n\t{tk.KeyChain.Aggregate((a,c)=>$"{a}\n\t{c}")}");
		
		Output.WriteLine($"Throws => {Assert.Throws<TypeKeyException>(() => "\"scheme://domain/folder1/folder2\"".DeserializeFromJson<TypeKey>()).Message}");
		Output.WriteLine($"Throws => {Assert.Throws<TypeKeyException>(() => "\"https://domain/folder1/folder2?one=123&two=456\"".DeserializeFromJson<TypeKey>()).Message}");
	}
	[Fact(DisplayName = "Process full name")]
	public void ProcessFullName()
	{
		Output.WriteLine($"Throws = {Assert.Throws<AttributeNotFoundException>(() => typeof(string).GetTypeKey()).Message}");
		Output.WriteLine($"Throws = {Assert.Throws<TypeKeyException>(() => typeof(string).GetTypeKey(false)).Message}");
		Assert.Equal("System.Object/System.String", typeof(string).GetTypeKey(false, true)!);
	}
	[Fact(DisplayName = "Derived types")]
	public void DerivedTypes()
	{
		Output.WriteLine($"One TypeKey = {typeof(One).GetTypeKey()}");
		Assert.Equal($"http://fuxion.dev/{nameof(One)}",typeof(One).GetTypeKey());
		Assert.Equal($"http://fuxion.dev/{nameof(One)}",typeof(One).GetTypeKey(processInheritance:false));
		Output.WriteLine($"One Throws = {Assert.Throws<TypeKeyException>(() => typeof(One).GetTypeKey(false,true)).Message}");
		Assert.Equal($"http://fuxion.dev/{nameof(One)}",typeof(One).GetTypeKey(false,true, false));
		
		Output.WriteLine($"Two TypeKey = {typeof(Two).GetTypeKey()}");
		Assert.Equal($"http://fuxion.dev/{nameof(One)}/{nameof(Two)}",typeof(Two).GetTypeKey());
		Assert.Equal($"{nameof(Two)}",typeof(Two).GetTypeKey(processInheritance:false));
		Output.WriteLine($"Two Throws = {Assert.Throws<TypeKeyException>(() => typeof(Two).GetTypeKey(false,true)).Message}");
		Assert.Equal($"{nameof(Two)}",typeof(Two).GetTypeKey(false,true, false));
		
		Output.WriteLine($"Three Throws = {Assert.Throws<AttributeNotFoundException>(() => typeof(Three).GetTypeKey()).Message}");
		Output.WriteLine($"Three Throws = {Assert.Throws<TypeKeyException>(() => typeof(Three).GetTypeKey(false)).Message}");
		Output.WriteLine($"Three Throws = {Assert.Throws<TypeKeyException>(() => typeof(Three).GetTypeKey(false, true)).Message}");
		Assert.Equal($"{typeof(Three).GetSignature(true)}",typeof(Three).GetTypeKey(false,true, false));
		
		Output.WriteLine($"Four Throws = {Assert.Throws<TypeKeyException>(() => typeof(Four).GetTypeKey(false)).Message}");
		Assert.Equal($"{nameof(Four)}",typeof(Four).GetTypeKey(processInheritance:false));
		Output.WriteLine($"Four Throws = {Assert.Throws<TypeKeyException>(() => typeof(Four).GetTypeKey(false, true)).Message}");
		Assert.Equal($"{nameof(Four)}",typeof(Four).GetTypeKey(false, true, false));
	}
}
[TypeKey("http://fuxion.dev",nameof(One))]
public class One { }
[TypeKey(nameof(Two))] 
public class Two : One{}
public class Three : Two{}
[TypeKey(nameof(Four))]
public class Four : Three{}