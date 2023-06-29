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
		dir.Register<string>("string");
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
			.AddTypeKeyHeader("header.payload");
		// var pod = new TypeKeyPod<TestPayload>(resolver[typeof(TestPayload)], payload);
		Output.WriteLine($"json:\r\n{builder.ToJsonNode().Pod.Payload.ToJsonString(true)}");
		Output.WriteLine($"utf:\r\n{builder.ToJsonNode().ToUtf8Bytes((TypeKey)"utf").Pod.Payload.ToBase64String()}");
	}
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
	{
		var base64 = "eyJfX2Rpc2NyaW1pbmF0b3IiOiJqc29uIiwiX19wYXlsb2FkIjp7Il9fZGlzY3JpbWluYXRvciI6InRlc3RQYXlsb2FkL2Rlcml2ZWQiLCJfX3BheWxvYWQiOnsiTmljayI6InBheWxvYWROaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoicGF5bG9hZE5hbWUiLCJBZ2UtY3VzdG9tIjoyM30sIl9faXRlbXMiOlt7Il9fZGlzY3JpbWluYXRvciI6ImludGVnZXIiLCJfX3BheWxvYWQiOjEyMzR9LHsiX19kaXNjcmltaW5hdG9yIjoic3RyaW5nIiwiX19wYXlsb2FkIjoiaGVhZGVyLnBheWxvYWQifV19fQ==";
		ITypeKeyPodPreBuilder<byte[]> builder = base64.FromBase64String()
			.BuildTypeKeyPod(resolver);
		Assert.NotNull(builder);
			var b2 = builder
			.FromUtf8Bytes()
			.FromJsonNode();
		var pod2 = b2.Pod.Ass<Pod<TypeKey, TestPayload>>();
		Output.WriteLine($"json:\r\n{b2.Pod.Payload.ToJsonString(true)}");
	}
}