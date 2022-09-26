namespace Fuxion.Test.Json;

using Fuxion.Json;

public class JsonPodTest : BaseTest
{
	public JsonPodTest(ITestOutputHelper output) : base(output) { }

	[Fact(DisplayName = "JsonPod - ToJson")]
	public void ToJson()
	{
		var payload = new PayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick"
		};
		var pod = payload.ToJsonPod("podKey");
		var json = pod.ToJson();

		Output.WriteLine("Serialized json: ");
		Output.WriteLine(json);

		Assert.Contains(@"""PayloadKey"":""podKey""", json);
		Assert.Contains(@"""Name"": ""payloadName""", json);
		Assert.Contains(@"""Age"": 23", json);
		Assert.Contains(@"""Nick"": ""payloadNick""", json);
	}
	[Fact(DisplayName = "JsonPod - FromJson")]
	public void FromJson()
	{
		var json = @"
			{
				""PayloadKey"": ""podKey"",
				""Payload"": {
					""Name"": ""payloadName"",
					""Age"": 23,
					""Nick"": ""payloadNick""
				}
			}";

		Output.WriteLine("Initial json: ");
		Output.WriteLine(json);

		var pod = json.FromJsonPod<PayloadBase, string>();
		// NULLABLE - Prefer Assert.NotNull() but the nullable constraint attribute is missing
		if (pod is null) throw new NullReferenceException($"'pod' deserialization is null");

		Output.WriteLine("pod.PayloadJRaw.Value: ");
		Output.WriteLine(pod.PayloadJRaw);

		void AssertBase(PayloadBase payload)
		{
			Assert.Equal("payloadName", payload.Name);
			Assert.Equal(23, payload.Age);
		}
		void AssertDerived(PayloadDerived payload)
		{
			AssertBase(payload);
			Assert.Equal("payloadNick", payload.Nick);
		}

		Assert.Equal("podKey", pod.PayloadKey);
		AssertBase(pod);
		Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived)pod));
		var derived = pod.CastWithPayload<PayloadDerived>();
		// NULLABLE - Prefer Assert.NotNull() but the nullable constraint attribute is missing
		if (derived is null) throw new NullReferenceException($"'derived' deserialization is null");
		AssertDerived(derived);
	}
	[Fact(DisplayName = "JsonPod - CastWithPayload")]
	public void CastWithPayload()
	{
		var payload = new PayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick"
		};
		var basePod = new JsonPod<PayloadBase, string>(payload, "podKey");
		var derived = basePod.CastWithPayload<PayloadDerived>();
		// NULLABLE - Prefer Assert.NotNull() but the nullable constraint attribute is missing
		if (derived is null) throw new NullReferenceException($"'derived' deserialization is null");
		Assert.Equal("payloadName", derived.Payload.Name);
	}
}
public class PayloadBase
{
	public string? Name { get; set; }
	public int Age { get; set; }
}
public class PayloadDerived : PayloadBase
{
	public string? Nick { get; set; }
}