using System.Text.Json.Serialization;

/* Unmerged change from project 'Fuxion.Pods.Test (net8.0)'
Before:
using Fuxion.Text.Json;
After:
using Fuxion;
using Fuxion.Pods;
using Fuxion.Pods.Test;
using Fuxion.Pods.Test;
using Fuxion.Pods.Test.Text;
using Fuxion.Pods.Test.Text.UriKey;
using Fuxion.Text.Json;
*/
using Fuxion.Pods;
using Fuxion.Text.Json;
using static Fuxion.Pods.Test.UriKeyTest;

namespace Fuxion.Pods.Test;

public class UriKeyPodTest : BaseTest<UriKeyPodTest>
{
	public UriKeyPodTest(ITestOutputHelper output) : base(output)
	{
		UriKeyDirectory dir = new();
		dir.SystemRegister.All();
		dir.Register<ITestPayload>();
		dir.Register<TestPayload>();
		dir.Register<TestPayloadDerived>();
		dir.Register<TestPayloadReset>();
		dir.Register<TestMessage>();
		dir.Register<TestDestination>();
		resolver = dir;
	}
	readonly IUriKeyResolver resolver;
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
	{
		var oo = typeof(TestPayloadReset).GetUriKey();
		var aa = oo.SerializeToJson();
		var bb = aa.DeserializeFromJson<UriKey>();

		var inputBuilder = "payload".BuildUriKeyPod(resolver)
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
				"item1","item2"
			});
		string base64 = inputBuilder.ToJsonNode()
			.ToUtf8Bytes()
			.Pod.Payload.ToBase64String();

		PrintVariable(base64);

		const string base642 =
			"eyJfX2Rpc2NyaW1pbmF0b3IiOiJodHRwczovL21ldGEuZnV4aW9uLmRldi9tZXRhZGF0YS90ZXN0L1Rlc3RQYXlsb2FkL1Rlc3RQYXlsb2FkRGVyaXZlZC8xLjAuMCIsIl9fcGF5bG9hZCI6eyJOaWNrIjoicGF5bG9hZE5pY2siLCJCaXJ0aGRhdGUtY3VzdG9tIjoiMjAxMi0xMi0xMiIsIk5hbWUiOiJwYXlsb2FkTmFtZSIsIkFnZS1jdXN0b20iOjIzfSwiX19pdGVtcyI6W3siX19kaXNjcmltaW5hdG9yIjoiaHR0cHM6Ly9tZXRhLmZ1eGlvbi5kZXYvc3lzdGVtL2ludC8xLjAuMCIsIl9fcGF5bG9hZCI6MTIzNH0seyJfX2Rpc2NyaW1pbmF0b3IiOiJodHRwczovL21ldGEuZnV4aW9uLmRldi9tZXRhZGF0YS90ZXN0L1Rlc3RQYXlsb2FkUmVzZXQvMS4wLjAiLCJfX3BheWxvYWQiOnsiQWRkcmVzcyI6ImhlYWRlci5hZGRyZXNzIiwiTmljayI6ImhlYWRlci5uaWNrIiwiQmlydGhkYXRlLWN1c3RvbSI6IjIwMTItMTItMTIiLCJOYW1lIjoiaGVhZGVyLm5hbWUiLCJBZ2UtY3VzdG9tIjoxMn19LHsiX19kaXNjcmltaW5hdG9yIjoiaHR0cHM6Ly9tZXRhLmZ1eGlvbi5kZXYvc3lzdGVtL3N0cmluZ1tdLzEuMC4wIiwiX19wYXlsb2FkIjpbIml0ZW0xLGl0ZW0yIl19XX0=";
		var builder = base64.FromBase64String()
			.BuildUriKeyPod(resolver)
			.FromUtf8Bytes()
			.FromJsonNode();
		var pod = builder.Pod;

		AnalyseUriKey(pod.Discriminator);
		AnalyseUriKey(pod[typeof(TestPayload)].Discriminator);

		PrintVariable(pod.GetType().GetSignature());

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
		Output.WriteLine($"--- {nameof(ITestPayload)}");
		//IsTrue(pod[typeof(ITestPayload).GetUriKey()].Payload is ITestPayload);
		//IsTrue(pod[typeof(ITestPayload)].Payload is ITestPayload);
		//IsTrue(pod.TryGetHeader<ITestPayload>(out var _));
		Output.WriteLine("--- int");
		IsTrue(pod[SystemUriKeys.Int].Payload is int);
		IsTrue(pod[resolver[typeof(int)]].Payload is int);
		IsTrue(pod[typeof(int)].Payload is int);
		IsTrue(pod.TryGetHeader<int>(out var _));
		Output.WriteLine("--- string[]");
		IsTrue(pod[SystemUriKeys.StringArray].Payload is string[]);
		IsTrue(pod[resolver[typeof(string[])]].Payload is string[]);
		IsTrue(pod[typeof(string[])].Payload is string[]);
		IsTrue(pod.TryGetHeader<string[]>(out var _));
		//Output.WriteLine($"json:\r\n{pod.BuildPod().ToJsonNode(resolver).Pod.Payload.ToJsonString(true)}");
	}
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
		}), $"'{nameof(TestPayload)}' is based on '{nameof(TestPayload)}'");
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
		}), $"'{nameof(TestPayloadDerived)}' is based on '{nameof(TestPayload)}'");
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
		}), $"'{nameof(TestPayloadReset)}' is based on '{nameof(TestPayload)}'");
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
		}), $"'{nameof(TestPayload)}' is base of '{nameof(TestPayloadReset)}'");
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
		}), $"'{nameof(TestPayloadDerived)}' is base of '{nameof(TestPayloadReset)}'");
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
		}), $"'{nameof(TestPayloadReset)}' is base of '{nameof(TestPayloadReset)}'");
	}
	[Fact]
	public void Rebuild()
	{
		TestMessage msg = new(1, "test");
		var pod = msg.BuildUriKeyPod(resolver)
			.ToUriKeyPod()
			.AddUriKeyHeader(new TestDestination("fuxion-lab-CL1-MS1"))
			.Pod;
		var bytesPod = pod.RebuildUriKeyPod<TestMessage, IUriKeyPod<TestMessage>>()
			.ToJsonNode()
			.ToUtf8Bytes()
			.Pod;
	}
	[Fact(DisplayName = "ToJson")]
	public void ToJson()
	{
		TestPayload payload = new TestPayloadDerived
		{
			Name = "payload.name",
			Age = 23,
			Nick = "payload.nick",
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
				"item1","item2"
			});
		// var pod = new TypeKeyPod<TestPayload>(resolver[typeof(TestPayload)], payload);
		Output.WriteLine($"json:\r\n{builder.ToJsonNode().Pod.Payload.ToJsonString(true)}");
		Output.WriteLine($"utf:\r\n{builder.ToJsonNode().ToUtf8Bytes().Pod.Payload.ToBase64String()}");
	}
}
[UriKey($"{UriKey.FuxionBaseUri}metadata/test/{nameof(ITestPayload)}/1.0.0")]
file interface ITestPayload
{
	string? Name { get; }
	int Age { get; }
}

[UriKey($"{UriKey.FuxionBaseUri}metadata/test/{nameof(TestPayload)}/1.0.0")]
file class TestPayload : ITestPayload
{
	public string? Name { get; set; }
	[JsonPropertyName("Age-custom")]
	public required int Age { get; init; }
}

[UriKey($"{nameof(TestPayloadDerived)}/1.0.0")]
file class TestPayloadDerived : TestPayload, ITestPayload
{
	public required string Nick { get; set; }
	[JsonPropertyName("Birthdate-custom")]
#if NET462
	// PEND https://www.nuget.org/packages/Portable.System.DateTimeOnly#readme-body-tab
	public DateTime Birthdate { get; set; }
#else
	 public DateOnly Birthdate { get; set; }
#endif
}

[UriKey($"{UriKey.FuxionBaseUri}metadata/test/{nameof(TestPayloadReset)}/1.0.0", isReset: true)]
file class TestPayloadReset : TestPayloadDerived//, ITestPayload
{
	public required string Address { get; set; }
}

[UriKey(UriKey.FuxionBaseUri + "lab/test-message/1.0.0")]
file class TestMessage(int id, string name)
{
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
}

[UriKey(UriKey.FuxionBaseUri + "lab/test-destination/1.0.0")]
file class TestDestination(string destination)
{
	public string Destination { get; } = destination;
}