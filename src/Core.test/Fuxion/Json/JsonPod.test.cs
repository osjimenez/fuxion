﻿using System.Text.Json.Serialization;
using Fuxion.Json;

namespace Fuxion.Test.Json;

 public class JsonPodTest : BaseTest<JsonPodTest>
 {
	public JsonPodTest(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
	[Fact(DisplayName = "JsonPod - CastWithPayload")]
	public void CastWithPayload()
	{
		var payload = new PayloadDerived {
			Name = "payloadName", Age = 23, Nick = "payloadNick"
		};
		var basePod = new JsonPod<string, PayloadBase>("podKey", payload);
		var derived = basePod.CastWithPayload<PayloadDerived>();
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
		var pod = json.DeserializeFromJson<JsonPod<string, PayloadBase>>();
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
		Assert.Equal("podKey", pod.Discriminator);
		AssertBase(pod);
		Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived?)pod));
		var derived = pod.CastWithPayload<PayloadDerived>();
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
		pod = jsonWithHeaders.DeserializeFromJson<JsonPod<string, PayloadBase>>();
		Output.WriteLine($"JSON POD:\r\n{pod.SerializeToJson()}");
		Assert.NotNull(pod);
		Assert.True(pod.Headers.Has("header1"));
		var pod3 = pod.Headers["header3"];
		Assert.NotNull(pod3);
		Assert.True(pod3.Headers.Has("header3.3"));
	}
	[Fact(DisplayName = "JsonPod - ToJson")]
	public void ToJson()
	{
		PayloadBase payload = new PayloadDerived {
			Name = "payloadName", Age = 23, Nick = "payloadNick"
		};
		var pod = payload.BuildPod<string, PayloadBase>("podKey").ToJson().Pod;
		var json = pod.SerializeToJson();
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains(@"""Discriminator"": ""podKey""", json);
		Assert.Contains(@"""Name"": ""payloadName""", json);
		Assert.Contains(@"""Age"": 23", json);
		Assert.Contains(@"""Nick"": ""payloadNick""", json);
		Assert.DoesNotContain(@"""Headers"": ", json);
		
		pod.Headers.Add("header1Payload".BuildPod("header1").ToJson().Pod);
		pod.Headers.Add("header2Payload".BuildPod("header2").ToJson().Pod);
		PodBase pod3 = new("header3", payload);
		pod3.Headers.Add("header3.3Payload".BuildPod("header3.3").ToJson().Pod);
		pod.Headers.Add(pod3);
		Assert.Throws<ArgumentException>(() => pod.Headers.Add("".BuildPod("header1").ToJson().Pod));
		json = pod.SerializeToJson();
		Output.WriteLine("Serialized json (with headers):");
		Output.WriteLine(json);
		Assert.Contains(@"""Headers"": ", json);
		
	}
	[Fact(DisplayName = "Implicit operator")]
	public void ImplicitOperator()
	{
		JsonPod<string, string> pod = new("discriminator", "payload");
		Assert.Equal("payload", pod);
	}
	[Fact(DisplayName = "Header edition")]
	public void HeaderEdition()
	{
		JsonPod<string, string> pod = new("discriminator", "payload");
		pod.Headers.Add(new PayloadBase
		{
			Name = "value1"
		}.BuildPod("h").ToJson().Pod);
		var val = pod.Headers["h"].As<PayloadBase>();
		Assert.NotNull(val);
		Output.WriteLine($"Original value: {val.Name}");
		val.Name = "value2";
		var val2 = pod.Headers["h"].As<PayloadBase>();
		Assert.NotNull(val2);
		Output.WriteLine($"Edited value: {val2.Name}");
		Assert.NotEqual("value2", val2.Name);
		Assert.Throws<ArgumentException>(() => pod.Headers.Add(val2.BuildPod("h").Pod)); // Fails because I can't add it if already exist
		Assert.False(pod.Headers.Remove("h1"));
		pod.Headers.Remove("h");
		pod.Headers.Add(val.BuildPod("h").ToJson().Pod);
		var val3 = pod.Headers["h"].As<PayloadBase>();
		Assert.NotNull(val3);
		Output.WriteLine($"Replaced value: {val3.Name}");
		Assert.Equal("value2", val3.Name);
	}
 }

public class PodBaseJsonConverter : JsonPodConverter<PodBase, string, PayloadBase> { }
[JsonConverter(typeof(PodBaseJsonConverter))]
public class PodBase : JsonPod<string, PayloadBase>
{
	[JsonConstructor]
	protected PodBase() { }
	internal PodBase(string discriminator, PayloadBase payload) : base(discriminator, payload)
	{
		Class = discriminator;
	}
	public string? Class { get; set; }
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