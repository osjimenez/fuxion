using System.Text.Json.Serialization;
using Fuxion.Json;

namespace Fuxion.Test.Json;

 public class JsonPod2Test : BaseTest<JsonPod2Test>
 {
	public JsonPod2Test(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
	[Fact(DisplayName = "JsonPod - CastWithPayload")]
	public void CastWithPayload()
	{
		var payload = new PayloadDerived2 {
			Name = "payloadName", Age = 23, Nick = "payloadNick"
		};
		var basePod = new JsonPod2<string, PayloadBase2>("podKey", payload);
		var derived = basePod.CastWithPayload<PayloadDerived2>();
		Assert.NotNull(derived);
		Assert.NotNull(derived.Payload);
		Assert.Equal("payloadName", derived.Payload.Name);
	}
	[Fact(DisplayName = "JsonPod - FromJson")]
	public void FromJson()
	{
		var json = """
			{
				"Discriminator": "podKey",
				"Payload": {
					"Name": "payloadName",
					"Age": 23,
					"Nick": "payloadNick"
				}
			}
			""";
		Output.WriteLine("Initial json: ");
		Output.WriteLine(json);
		var pod = json.FromJson<JsonPod2<string, PayloadBase2>>();
		Assert.NotNull(pod);
		Output.WriteLine("pod.PayloadValue: ");
		Output.WriteLine(pod.PayloadValue.ToString());
		void AssertBase(PayloadBase2? payload)
		{
			Assert.NotNull(payload);
			Assert.Equal("payloadName", payload.Name);
			Assert.Equal(23, payload.Age);
		}
		void AssertDerived(PayloadDerived2? payload)
		{
			Assert.NotNull(payload);
			AssertBase(payload);
			Assert.Equal("payloadNick", payload.Nick);
		}
		Assert.Equal("podKey", pod.Discriminator);
		AssertBase(pod);
		Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived2?)pod));
		var derived = pod.CastWithPayload<PayloadDerived2>();
		Assert.NotNull(derived);
		AssertDerived(derived);
		
		var jsonWithHeaders = """
			{
				"Class": "podKey",
				"Discriminator": "podKey",
				"Headers": [
					{
						"Discriminator": "header1",
						"Payload": "header1Payload"
					},
					{
						"Discriminator": "header2",
						"Payload": "header2Payload"
					},
					{
						"Class": "header3",
						"Discriminator": "header3",
						"Headers": [
							{
								"Discriminator": "header3.3",
								"Payload": "header3.3Payload"
							}
						],
						"Payload": {
							"Nick": "payloadNick",
							"Name": "payloadName",
							"Age": 23
						}
					}
				],
				"Payload": {
					"Nick": "payloadNick",
					"Name": "payloadName",
					"Age": 23
				}
			}
			""";
		Output.WriteLine("Initial json (with headers): ");
		Output.WriteLine(jsonWithHeaders);
		pod = jsonWithHeaders.FromJson<JsonPod2<string, PayloadBase2>>();
		Output.WriteLine($"JSON POD:\r\n{pod.ToJson()}");
		Assert.NotNull(pod);
		Assert.True(pod.Headers.Has("header1"));
		var pod3 = pod.Headers["header3"];
		Assert.NotNull(pod3);
		if (pod3 is JsonPod2<string, object> pod33)
		{
			Assert.True(pod33.Headers.Has("header3.3"));
		} else
			Assert.True(false);
	}
	[Fact(DisplayName = "JsonPod - ToJson")]
	public void ToJson()
	{
		PayloadBase2 payload = new PayloadDerived2 {
			Name = "payloadName", Age = 23, Nick = "payloadNick"
		};
		var pod = payload.BuildPod2<string, PayloadBase2>("podKey").ToJson().Pod;
		var json = pod.ToJson();
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains(@"""Discriminator"": ""podKey""", json);
		Assert.Contains(@"""Name"": ""payloadName""", json);
		Assert.Contains(@"""Age"": 23", json);
		Assert.Contains(@"""Nick"": ""payloadNick""", json);
		Assert.DoesNotContain(@"""Headers"": ", json);
		
		pod.Headers.Add("header1Payload".BuildPod2("header1").ToJson().Pod);
		pod.Headers.Add("header2Payload".BuildPod2("header2").ToJson().Pod);
		PodBase2 pod3 = new("header3", payload);
		pod3.Headers.Add("header3.3Payload".BuildPod2("header3.3").ToJson().Pod);
		pod.Headers.Add(pod3);
		Assert.Throws<ArgumentException>(() => pod.Headers.Add("".BuildPod2("header1").ToJson().Pod));
		json = pod.ToJson();
		Output.WriteLine("Serialized json (with headers):");
		Output.WriteLine(json);
		Assert.Contains(@"""Headers"": ", json);
		
	}
	[Fact(DisplayName = "Implicit operator")]
	public void ImplicitOperator()
	{
		JsonPod2<string, string> pod = new("discriminator", "payload");
		Assert.Equal("payload", pod);
	}
	[Fact(DisplayName = "Header edition")]
	public void HeaderEdition()
	{
		JsonPod2<string, string> pod = new("discriminator", "payload");
		pod.Headers.Add(new PayloadBase2
		{
			Name = "value1"
		}.BuildPod2("h").ToJson().Pod);
		var valH = pod.Headers["h"];
		if (valH is JsonPod2<string, object> valJ)
		{
			var val = valJ.As<PayloadBase2>();
			Assert.NotNull(val);
			Output.WriteLine($"Original value: {val.Name}");
			val.Name = "value2";
			var val2H = pod.Headers["h"];
			if (val2H is JsonPod2<string, object> val2J)
			{
				var val2 = val2J.As<PayloadBase2>();
				Assert.NotNull(val2);
				Output.WriteLine($"Edited value: {val2.Name}");
				Assert.NotEqual("value2", val2.Name);
				Assert.Throws<ArgumentException>(() => pod.Headers.Add(val2.BuildPod2("h")
					.Pod)); // Fails because I can't add it if already exist
			} else
				Assert.Fail("");
			Assert.False(pod.Headers.Remove("h1"));
			Assert.True(pod.Headers.Remove("h"));
			pod.Headers.Add(val.BuildPod2("h")
				.ToJson()
				.Pod);
			var val3H = pod.Headers["h"];
			if (val3H is JsonPod2<string, object> val3J)
			{
				var val3 = val3J
					.As<PayloadBase2>();
				Assert.NotNull(val3);
				Output.WriteLine($"Replaced value: {val3.Name}");
				Assert.Equal("value2", val3.Name);
			}else
				Assert.Fail("");
		}else
			Assert.Fail("");
	}
 }

public class PodBase2JsonConverter : JsonPod2Converter<PodBase2, string, PayloadBase2> { }
[JsonConverter(typeof(PodBase2JsonConverter))]
public class PodBase2 : JsonPod2<string, PayloadBase2>
{
	[JsonConstructor]
	protected PodBase2() { }
	internal PodBase2(string discriminator, PayloadBase2 payload) : base(discriminator, payload)
	{
		Class = discriminator;
	}
	public string? Class { get; set; }
}

public class PayloadBase2
{
	public string? Name { get; set; }
	public int Age { get; set; }
}

public class PayloadDerived2 : PayloadBase2
{
	public string? Nick { get; set; }
}