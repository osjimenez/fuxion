using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fuxion.Reflection;
using Fuxion.Test.Text.Json;
using Fuxion.Text.Json;

namespace Fuxion.Test;

public class UriKeyPodTest : BaseTest<UriKeyPodTest>
{
	public UriKeyPodTest(ITestOutputHelper output) : base(output)
	{
		UriKeyDirectory dir = new();
		dir.SystemRegister.All();
		dir.Register<TestPayload>();
		dir.Register<TestPayloadDerived>();
		dir.Register<TestPayloadReset>();
		dir.Register<TestMessage>();
		dir.Register<TestDestination>();
		resolver = dir;
	}
	readonly IUriKeyResolver resolver;
	[Fact]
	public void Headers()
	{
		var builder = "".BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddUriKeyHeader(new TestPayload
			{
				Age = 12,
				Name = "header.name"
			});
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayload
		{
			Age = 12,
			Name = "header.name"
		}),$"'{nameof(TestPayload)}' is based on '{nameof(TestPayload)}'");
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayloadDerived
		{
			Age = 12,
			Name = "header.name",
			Nick = "header.nick",
#if NET462
			Birthdate = DateTime.Parse("12/12/2012"),
#else
			Birthdate = DateOnly.Parse("12/12/2012"),
#endif
		}),$"'{nameof(TestPayloadDerived)}' is based on '{nameof(TestPayload)}'");
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayloadReset
		{
			Age = 12,
			Name = "header.name",
			Nick = "header.nick",
#if NET462
			Birthdate = DateTime.Parse("12/12/2012"),
#else
			Birthdate = DateOnly.Parse("12/12/2012"),
#endif
			Address = "header.address"
		}),$"'{nameof(TestPayloadReset)}' is based on '{nameof(TestPayload)}'");
		builder = "".BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddUriKeyHeader(new TestPayloadReset
			{
				Age = 12,
				Name = "header.name",
				Nick = "header.nick",
#if NET462
				Birthdate = DateTime.Parse("12/12/2012"),
#else
				Birthdate = DateOnly.Parse("12/12/2012"),
#endif
				Address = "header.address"
			});
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayload
		{
			Age = 12,
			Name = "header.name"
		}),$"'{nameof(TestPayload)}' is base of '{nameof(TestPayloadReset)}'");
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayloadDerived
		{
			Age = 12,
			Name = "header.name",
			Nick = "header.nick",
#if NET462
			Birthdate = DateTime.Parse("12/12/2012"),
#else
			Birthdate = DateOnly.Parse("12/12/2012"),
#endif
		}),$"'{nameof(TestPayloadDerived)}' is base of '{nameof(TestPayloadReset)}'");
		Throws<UriKeyInheritanceException>(() => builder.AddUriKeyHeader(new TestPayloadReset
		{
			Age = 12,
			Name = "header.name",
			Nick = "header.nick",
#if NET462
			Birthdate = DateTime.Parse("12/12/2012"),
#else
			Birthdate = DateOnly.Parse("12/12/2012"),
#endif
			Address = "header.address"
		}),$"'{nameof(TestPayloadReset)}' is base of '{nameof(TestPayloadReset)}'");
	}
	[Fact]
	public void Rebuild()
	{
		TestMessage msg = new(1, "test");
		var pod = msg.BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddUriKeyHeader(new TestDestination("fuxion-lab-CL1-MS1"))
			.Pod;
		var bytesPod = pod.RebuildUriKeyPod<TestMessage,IUriKeyPod<TestMessage>>()
			.ToJsonNode()
			.ToUtf8Bytes()
			.Pod;
	}
	[Fact(DisplayName = "ToJson")]
	public void ToJson()
	{
		TestPayload payload = new TestPayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick",
#if NET462
			Birthdate = DateTime.Parse("12/12/2012"),
#else
			Birthdate = DateOnly.Parse("12/12/2012"),
#endif
		};
		var builder = payload.BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddHeader(SystemUriKeys.Int, 1234)
			.AddUriKeyHeader(new TestPayloadReset
			{
				Age = 12,
				Name = "header.name",
				Nick = "header.nick",
#if NET462
				Birthdate = DateTime.Parse("12/12/2012"),
#else
				Birthdate = DateOnly.Parse("12/12/2012"),
#endif
				Address = "header.address"
			})
			.AddUriKeyHeader(new[]
			{
				"item1,item2"
			});
		// var pod = new TypeKeyPod<TestPayload>(resolver[typeof(TestPayload)], payload);
		Output.WriteLine($"json:\r\n{builder.ToJsonNode().Pod.Payload.ToJsonString(true)}");
		Output.WriteLine($"utf:\r\n{builder.ToJsonNode().ToUtf8Bytes().Pod.Payload.ToBase64String()}");
	}
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
	{
		const string base64 = "eyJfX2Rpc2NyaW1pbmF0b3IiOiJodHRwczovL21ldGEuZnV4aW9uLmRldjo0NDMvbWV0YWRhdGEvdGVzdC9UZXN0UGF5bG9hZC9UZXN0UGF5bG9hZERlcml2ZWQvMS4wLjAiLCJfX3BheWxvYWQiOnsiTmljayI6InBheWxvYWROaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoicGF5bG9hZE5hbWUiLCJBZ2UtY3VzdG9tIjoyM30sIl9faXRlbXMiOlt7Il9fZGlzY3JpbWluYXRvciI6Imh0dHBzOi8vbWV0YS5mdXhpb24uZGV2OjQ0My9zeXN0ZW0vaW50LzEuMC4wIiwiX19wYXlsb2FkIjoxMjM0fSx7Il9fZGlzY3JpbWluYXRvciI6Imh0dHBzOi8vbWV0YS5mdXhpb24uZGV2OjQ0My9tZXRhZGF0YS90ZXN0L1Rlc3RQYXlsb2FkUmVzZXQvMS4wLjAiLCJfX3BheWxvYWQiOnsiQWRkcmVzcyI6ImhlYWRlci5hZGRyZXNzIiwiTmljayI6ImhlYWRlci5uaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoiaGVhZGVyLm5hbWUiLCJBZ2UtY3VzdG9tIjoxMn19LHsiX19kaXNjcmltaW5hdG9yIjoiaHR0cHM6Ly9tZXRhLmZ1eGlvbi5kZXY6NDQzL3N5c3RlbS9zdHJpbmdbXS8xLjAuMCIsIl9fcGF5bG9hZCI6WyJpdGVtMSxpdGVtMiJdfV19";
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
		Output.WriteLine($"--- {nameof(TestPayloadReset)}");
		IsTrue(pod[typeof(TestPayloadReset).GetUriKey()].Payload is TestPayloadReset);
		IsTrue(pod[typeof(TestPayloadReset)].Payload is TestPayloadReset);
		IsTrue(pod.TryGetHeader<TestPayloadReset>(out var _));
		Output.WriteLine($"--- int");
		IsTrue(pod[SystemUriKeys.Int].Payload is int);
		IsTrue(pod[resolver[typeof(int)]].Payload is int);
		IsTrue(pod[typeof(int)].Payload is int);
		IsTrue(pod.TryGetHeader<int>(out var _));
		Output.WriteLine($"--- string[]");
		IsTrue(pod[SystemUriKeys.StringArray].Payload is string[]);
		IsTrue(pod[resolver[typeof(string[])]].Payload is string[]);
		IsTrue(pod[typeof(string[])].Payload is string[]);
		IsTrue(pod.TryGetHeader<string[]>(out var _));
		//Output.WriteLine($"json:\r\n{pod.BuildPod().ToJsonNode(resolver).Pod.Payload.ToJsonString(true)}");
	}
}

[UriKey($"{UriKey.FuxionBaseUri}metadata/test/{nameof(TestPayload)}/1.0.0")]
file class TestPayload
{
	public string? Name { get; set; }
	[JsonPropertyName("Age-custom")]
#if NET462
	[JsonInclude]
	public int Age { get; internal set; }
#elif NET6_0
	public int Age { get; init; }
#else
	public required int Age { get; init; }
#endif
}
[UriKey($"{nameof(TestPayloadDerived)}/1.0.0")]
file class TestPayloadDerived : TestPayload
{
#if NET462
	public string Nick { get; set; } = "";
#elif NET6_0
	public string Nick { get; set; } = "";
#else
	public required string Nick { get; set; }
#endif
	[JsonPropertyName("Birthdate-custom")]
#if NET462
	public DateTime Birthdate { get; set; }
#else
	public DateOnly Birthdate { get; set; }
#endif
}

[UriKey($"{UriKey.FuxionBaseUri}metadata/test/{nameof(TestPayloadReset)}/1.0.0", isReset: true)]
file class TestPayloadReset : TestPayloadDerived
{
#if NET462
	public string Address { get; set; } = "";
#elif NET6_0
	public string Address { get; set; } = "";
#else
	public required string Address { get; set; }
#endif
}
[UriKey(UriKey.FuxionBaseUri+"lab/test-message/1.0.0")]
public class TestMessage(int id, string name)
{
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
}
[UriKey(UriKey.FuxionBaseUri+"lab/test-destination/1.0.0")]
public class TestDestination(string destination)
{
	public string Destination { get; } = destination;
}