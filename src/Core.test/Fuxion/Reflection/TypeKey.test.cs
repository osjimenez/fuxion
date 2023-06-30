using System.Diagnostics;
using System.Runtime.CompilerServices;
using Fuxion.Reflection;

namespace Fuxion.Test.Reflection;

public class TypeKeyTest(ITestOutputHelper output) : BaseTest<TypeKeyTest>(output)
{
	void PrintChain(TypeKey tk, [CallerArgumentExpression(nameof(tk))] string? name = null) 
		=> Output.WriteLine($"{name}:\n\t{tk.KeyChain.Aggregate((a, c) => $"{a}\n\t{c}")}");
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
		PrintChain(tk, "Chain");
		Assert.Equal("folder1",tk.KeyChain[0]);
		Assert.Equal("folder2",tk.KeyChain[1]);

		tk = "\"http://domain.com/folder1/folder2\"".DeserializeFromJson<TypeKey>();
		Assert.NotNull(tk);
		PrintChain(tk, "Chain");
		Assert.Equal("http://domain.com",tk.KeyChain[0]);
		Assert.Equal("folder1",tk.KeyChain[1]);
		Assert.Equal("folder2",tk.KeyChain[2]);
		
		tk = "\"https://domain/folder1/folder2\"".DeserializeFromJson<TypeKey>();
		Assert.NotNull(tk);
		PrintChain(tk, "Chain");
		
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
		Assert.Equal($"{TestTypeKeyAttribute.TestUrl}/{nameof(One)}",typeof(One).GetTypeKey());
		Assert.Equal($"{TestTypeKeyAttribute.TestUrl}/{nameof(One)}",typeof(One).GetTypeKey(processInheritance:false));
		Output.WriteLine($"One Throws = {Assert.Throws<TypeKeyException>(() => typeof(One).GetTypeKey(false,true)).Message}");
		Assert.Equal($"{TestTypeKeyAttribute.TestUrl}/{nameof(One)}",typeof(One).GetTypeKey(false,true, false));
		
		Output.WriteLine($"Two TypeKey = {typeof(Two).GetTypeKey()}");
		Assert.Equal($"{TestTypeKeyAttribute.TestUrl}/{nameof(One)}/{nameof(Two)}",typeof(Two).GetTypeKey());
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
	[Fact(DisplayName = "Implicit conversion")]
	public void ImplicitConversion()
	{
		PrintChain(new("one","two"));
		PrintChain(new("http://fuxion.dev","one","two"));
		PrintChain(new("https://fuxion.dev/one/two/three"));
		Assert.Throws<TypeKeyException>(() => PrintChain("ftp://one/two"));
	}
}
[TestTypeKey(TestTypeKeyClass.Test, nameof(One))]
public class One { }
[TypeKey(nameof(Two))]
public class Two : One{}
public class Three : Two{}
[TypeKey(nameof(Four))]
public class Four : Three{}

public enum TestTypeKeyClass
{
	Test
}
// This is a sample of TypeKeyAttribute extension with custom way to create keyChain
public class TestTypeKeyAttribute(TestTypeKeyClass @class, params string[] keyChain) : TypeKeyAttribute(GetChain(@class, keyChain))
{
	public const string TestUrl = "http://fuxion.dev";
	static string[] GetChain(TestTypeKeyClass @class, IEnumerable<string> keyChain)
	{
		var strClass = @class switch
		{
			TestTypeKeyClass.Test => TestUrl,
			var _ => throw new InvalidOperationException($"{@class} is not valid value")
		};
		return new[]
			{
				strClass
			}.Concat(keyChain)
			.ToArray();
	}
}