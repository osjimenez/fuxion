using System.Text.Json.Nodes;
using Fuxion.Reflection;
using Fuxion.Test.Text.Json;
using Fuxion.Text.Json;

namespace Fuxion.Test;

public class UriKeyPodTest : BaseTest<UriKeyPodTest>
{
	public UriKeyPodTest(ITestOutputHelper output) : base(output)
	{
		UriKeyDirectory dir = new();
		dir.Register<JsonNode>(new(baseUri + "json" + version));
		dir.Register<byte[]>(new(baseUri + "byte[]" + version));
		dir.Register<int>(new(baseUri + "integer" + version));
		dir.Register<string>(new(baseUri + "string" + version));
		dir.Register<TestPayload>();
		dir.Register<TestPayloadDerived>();
		// dir.Register<TestPayload>(new(baseUri + "testPayload" + version));
		// dir.Register<TestPayloadDerived>(new(baseUri + "testPayload/Derived" + version));
		resolver = dir;
	}
	const string baseUri = "https://fuxion.dev/metadata/lab/";
	const string version = "/1.0.0";
	readonly IUriKeyResolver resolver;
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
		var builder = payload.BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddHeader(new UriKey(baseUri + "integer" + version), 1234)
			.AddUriKeyHeader(new TestPayloadDerived()
			{
				Age = 12,
				Name = "header.name",
				Nick = "header.nick",
				Birthdate = DateOnly.Parse("12/12/2012")
			})
			.AddUriKeyHeader("header.payload");
		// var pod = new TypeKeyPod<TestPayload>(resolver[typeof(TestPayload)], payload);
		Output.WriteLine($"json:\r\n{builder.ToJsonNode().Pod.Payload.ToJsonString(true)}");
		Output.WriteLine($"utf:\r\n{builder.ToJsonNode().ToUtf8Bytes().Pod.Payload.ToBase64String()}");
	}
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
	{
		const string base64 = "eyJfX2Rpc2NyaW1pbmF0b3IiOiJodHRwczovL2Z1eGlvbi5kZXYvbWV0YWRhdGEvdGVzdC9UZXN0UGF5bG9hZC9UZXN0UGF5bG9hZERlcml2ZWQvMS4wLjAiLCJfX3BheWxvYWQiOnsiTmljayI6InBheWxvYWROaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoicGF5bG9hZE5hbWUiLCJBZ2UtY3VzdG9tIjoyM30sIl9faXRlbXMiOlt7Il9fZGlzY3JpbWluYXRvciI6Imh0dHBzOi8vZnV4aW9uLmRldi9tZXRhZGF0YS9sYWIvaW50ZWdlci8xLjAuMCIsIl9fcGF5bG9hZCI6MTIzNH0seyJfX2Rpc2NyaW1pbmF0b3IiOiJodHRwczovL2Z1eGlvbi5kZXYvbWV0YWRhdGEvdGVzdC9UZXN0UGF5bG9hZC9UZXN0UGF5bG9hZERlcml2ZWQvMS4wLjAiLCJfX3BheWxvYWQiOnsiTmljayI6ImhlYWRlci5uaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoiaGVhZGVyLm5hbWUiLCJBZ2UtY3VzdG9tIjoxMn19LHsiX19kaXNjcmltaW5hdG9yIjoiaHR0cHM6Ly9mdXhpb24uZGV2L21ldGFkYXRhL2xhYi9zdHJpbmcvMS4wLjAiLCJfX3BheWxvYWQiOiJoZWFkZXIucGF5bG9hZCJ9XX0=";
		var builder = base64.FromBase64String()
			.BuildUriKeyPod(resolver)
			.FromUtf8Bytes()
			.FromJsonNode();
		var pod = builder.Pod;
		
		Output.WriteLine($"--- {nameof(TestPayload)}");
		IsTrue(pod[typeof(TestPayload).GetUriKey()].Payload is TestPayload);
		IsTrue(pod[typeof(TestPayload)].Payload is TestPayload);
		IsTrue(pod.TryGetHeader<TestPayload>(out var _));
		Output.WriteLine($"--- {nameof(TestPayloadDerived)}");
		IsTrue(pod[typeof(TestPayloadDerived).GetUriKey()].Payload is TestPayloadDerived);
		IsTrue(pod[typeof(TestPayloadDerived)].Payload is TestPayloadDerived);
		IsTrue(pod.TryGetHeader<TestPayloadDerived>(out var _));
		Output.WriteLine($"--- {nameof(Int32)}");
		IsTrue(pod[new UriKey(baseUri + "integer" + version)].Payload is int);
		IsTrue(pod[resolver[typeof(int)]].Payload is int);
		IsTrue(pod[typeof(int)].Payload is int);
		IsTrue(pod.TryGetHeader<int>(out var _));
		Output.WriteLine($"--- {nameof(String)}");
		IsTrue(pod[new UriKey(baseUri + "string" + version)].Payload is string);
		IsTrue(pod[resolver[typeof(string)]].Payload is string);
		IsTrue(pod[typeof(string)].Payload is string);
		IsTrue(pod.TryGetHeader<string>(out var _));
		//Output.WriteLine($"json:\r\n{pod.BuildPod().ToJsonNode(resolver).Pod.Payload.ToJsonString(true)}");
	}
}