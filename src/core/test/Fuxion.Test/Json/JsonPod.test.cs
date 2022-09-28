namespace Fuxion.Test.Json;

using Fuxion.Json;

public class JsonPodTest : BaseTest
{
	public JsonPodTest(ITestOutputHelper output) : base(output) {
		Printer.WriteLineAction = m => output.WriteLine(m);
	}

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

		Assert.Contains(@"""PayloadKey"": ""podKey""", json);
		Assert.Contains(@"""Name"": ""payloadName""", json);
		Assert.Contains(@"""Age"": 23", json);
		Assert.Contains(@"""Nick"": ""payloadNick""", json);
	}
	[Fact(DisplayName = "JsonPod - FromJson")]
	public void FromJson()
	{
		var json = """
			{
				"PayloadKey": "podKey",
				"Payload": {
					"Name": "payloadName",
					"Age": 23,
					"Nick": "payloadNick"
				}
			}
			""";

		Output.WriteLine("Initial json: ");
		Output.WriteLine(json);

		var pod = json.FromJsonPod<PayloadBase, string>();
		Assert.NotNull(pod);

		Output.WriteLine("pod.PayloadValue: ");
		Output.WriteLine(pod.PayloadValue.ToString());

		void AssertBase(PayloadBase? payload)
		{
			Assert.NotNull(payload);
			Assert.Equal("payloadName", payload.Name);
			Assert.Equal(23, payload.Age);
		}
		void AssertDerived(PayloadDerived? payload)
		{
			Assert.NotNull(payload);
			AssertBase(payload);
			Assert.Equal("payloadNick", payload.Nick);
		}

		Assert.Equal("podKey", pod.PayloadKey);
		AssertBase(pod);
		Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived?)pod));
		var derived = pod.CastWithPayload<PayloadDerived>();
		Assert.NotNull(derived);
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
		Assert.NotNull(derived);
		Assert.Equal("payloadName", derived.Payload!.Name);
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