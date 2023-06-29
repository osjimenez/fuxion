using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fuxion.Reflection;
using Fuxion.Test.Text.Json;
using Fuxion.Text.Json;

namespace Fuxion.Test.Reflection;

public class TypeKeyPodTest : BaseTest<TypeKeyPodTest>
{
	public TypeKeyPodTest(ITestOutputHelper output) : base(output)
	{
		TypeKeyDirectory dir = new();
		dir.Register<JsonNode>("json");
		dir.Register<byte[]>("bytes");
		dir.Register<int>("integer");
		dir.Register<string>("string");
		dir.Register<TestPayload>("testPayload");
		dir.Register<TestPayloadDerived>(new[]{
			"testPayload","derived"
		});
		resolver = dir;
	}
	readonly ITypeKeyResolver resolver;
	[Fact(DisplayName = "ToJson")]
	public void ToJson()
	{
		TestPayload payload = new TestPayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick",
			Birthdate = DateOnly.Parse("12/12/2012")
		};
		var builder = payload.BuildTypeKeyPod(resolver)
			.ToTypeKeyPod()
			.AddHeader((TypeKey)"integer", 1234)
			.AddTypeKeyHeader(new TestPayloadDerived()
			{
				Age = 12,
				Name = "header.name",
				Nick = "header.nick",
				Birthdate = DateOnly.Parse("12/12/2012")
			})
			.AddTypeKeyHeader("header.payload");
		// var pod = new TypeKeyPod<TestPayload>(resolver[typeof(TestPayload)], payload);
		Output.WriteLine($"json:\r\n{builder.ToJsonNode().Pod.Payload.ToJsonString(true)}");
		Output.WriteLine($"utf:\r\n{builder.ToJsonNode().ToUtf8Bytes().Pod.Payload.ToBase64String()}");
	}
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
	{
		var base64 = "eyJfX2Rpc2NyaW1pbmF0b3IiOiJ0ZXN0UGF5bG9hZC9kZXJpdmVkIiwiX19wYXlsb2FkIjp7Ik5pY2siOiJwYXlsb2FkTmljayIsIkJpcnRoZGF0ZS1jdXN0b20iOiIyMDEyLTEyLTEyIiwiTmFtZSI6InBheWxvYWROYW1lIiwiQWdlLWN1c3RvbSI6MjN9LCJfX2l0ZW1zIjpbeyJfX2Rpc2NyaW1pbmF0b3IiOiJpbnRlZ2VyIiwiX19wYXlsb2FkIjoxMjM0fSx7Il9fZGlzY3JpbWluYXRvciI6InRlc3RQYXlsb2FkL2Rlcml2ZWQiLCJfX3BheWxvYWQiOnsiTmljayI6ImhlYWRlci5uaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoiaGVhZGVyLm5hbWUiLCJBZ2UtY3VzdG9tIjoxMn19LHsiX19kaXNjcmltaW5hdG9yIjoic3RyaW5nIiwiX19wYXlsb2FkIjoiaGVhZGVyLnBheWxvYWQifV19";
		ITypeKeyPodPreBuilder<byte[]> builder = base64.FromBase64String()
			.BuildTypeKeyPod(resolver);
		Assert.NotNull(builder);
			var b2 = builder
			.FromUtf8Bytes()
			.FromJsonNode();
		// var pod2 = b2.Pod.Ass<Pod<TypeKey, TestPayload>>(resolver);
		var pod2 = b2.Pod;
		Assert.NotNull(pod2);
		Assert.True(pod2[new[]
		{
			"testPayload", "derived"
		}].Payload is TestPayload);
		Assert.True(pod2[new[]
		{
			"testPayload", "derived"
		}].Payload is TestPayloadDerived);
		Assert.True(pod2["integer"].Payload is int);
		Assert.True(pod2["string"].Payload is string);
		Output.WriteLine($"json:\r\n{b2.Pod.BuildPod().ToJsonNode(resolver).Pod.Payload.ToJsonString(true)}");
	}
}