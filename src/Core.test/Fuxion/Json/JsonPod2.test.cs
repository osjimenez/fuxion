using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fuxion.Json;
using static Fuxion.Json.IPod2Converter<Fuxion.Json.JsonPod2<string,string>,string,string>;

namespace Fuxion.Test.Json;

 public class JsonPod2Test : BaseTest<JsonPod2Test>
 {
	public JsonPod2Test(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
	[Fact(DisplayName = "JsonPod - CastWithPayload")]
	public void CastWithPayload()
	{
		var payload = new TestPayloadDerived2 {
			Name = "payloadName", Age = 23, Nick = "payloadNick", Birthdate = DateOnly.Parse("12/12/2012")
		};
		var basePod = new JsonPod2<string, TestPayload2>("podKey", payload);
		var derived = basePod.CastWithPayload<TestPayloadDerived2>();
		Assert.NotNull(derived);
		Assert.NotNull(derived.Payload);
		Assert.Equal("payloadName", derived.Payload.Name);
	}
	[Fact(DisplayName = "JsonPod - FromJson")]
	public void FromJson()
	{
		var json = $$"""
			{
				"{{DISCRIMINATOR_LABEL}}": "podKey",
				"{{PAYLOAD_LABEL}}": {
					"Name": "payloadName",
					"Age": 23,
					"Nick": "payloadNick"
				}
			}
			""";
		Output.WriteLine("Initial json: ");
		Output.WriteLine(json);
		var pod = json.DeserializeFromJson<JsonPod2<string, TestPayload2>>();
		Assert.NotNull(pod);
		Output.WriteLine("pod.PayloadValue: ");
		Output.WriteLine(pod.PayloadValue!.ToString());
		void AssertBase(TestPayload2? payload)
		{
			Assert.NotNull(payload);
			Assert.Equal("payloadName", payload.Name);
			Assert.Equal(23, payload.Age);
		}
		void AssertDerived(TestPayloadDerived2? payload)
		{
			Assert.NotNull(payload);
			AssertBase(payload);
			Assert.Equal("payloadNick", payload.Nick);
		}
		Assert.Equal("podKey", pod.Discriminator);
		AssertBase(pod);
		Assert.Throws<InvalidCastException>(() => AssertDerived((TestPayloadDerived2?)pod));
		var derived = pod.CastWithPayload<TestPayloadDerived2>();
		Assert.NotNull(derived);
		AssertDerived(derived);
		
		var jsonWithHeaders = $$"""
			{
				"Klass": "podKey",
				"{{DISCRIMINATOR_LABEL}}": "podKey",
				"{{ITEMS_LABEL}}": [
					{
						"{{DISCRIMINATOR_LABEL}}": "header1",
						"{{PAYLOAD_LABEL}}": "header1Payload"
					},
					{
						"{{DISCRIMINATOR_LABEL}}": "header2",
						"{{PAYLOAD_LABEL}}": "header2Payload"
					},
					{
						"Klass": "header3",
						"{{DISCRIMINATOR_LABEL}}": "header3",
						"{{ITEMS_LABEL}}": [
							{
								"{{DISCRIMINATOR_LABEL}}": "header3.3",
								"{{PAYLOAD_LABEL}}": "header3.3Payload"
							}
						],
						"{{PAYLOAD_LABEL}}": {
							"Nick": "payloadNick",
							"Name": "payloadName",
							"Age": 23
						}
					}
				],
				"{{PAYLOAD_LABEL}}": {
					"Nick": "payloadNick",
					"Name": "payloadName",
					"Age": 23
				}
			}
			""";
		Output.WriteLine("Initial json (with headers): ");
		Output.WriteLine(jsonWithHeaders);
		// pod = jsonWithHeaders.FromJson<TestPod2>();
		// Output.WriteLine($"JSON POD:\r\n{pod.ToJson()}");
		// Assert.NotNull(pod);
		// Assert.True(pod.Has("header1"));
		// var pod3 = pod["header3"];
		// Assert.NotNull(pod3);
		// if (pod3 is JsonPod2<string, object> pod33)
		// {
		// 	var base2 = pod.ItemAs<TestPod2>("header3");
		// 	Assert.Equal("header3",base2?.Class);
		// 	Assert.True(pod33.Has("header3.3"));
		// } else
		// 	Assert.True(false);
	}
	[Fact(DisplayName = "JsonPod - FromJson new")]
	public void FromJsonNew()
	{
		const string base64 =
			@"eyJfX2Rpc2NyaW1pbmF0b3IiOiJ0ZXN0UG9kIiwiX19wYXlsb2FkIjp7IkNsYXNzLWN1c3RvbSI6ImNsYXNzIiwiX19kaXNjcmltaW5hdG9yIjoidGVzdFBheWxvYWQiLCJfX3BheWxvYWQiOnsiTmljayI6InBheWxvYWQuTmljayIsIkJpcnRoZGF0ZS1jdXN0b20iOiIyMDEyLTEyLTEyIiwiTmFtZSI6InBheWxvYWQuTmFtZSIsIkFnZS1jdXN0b20iOjIzfSwiX19pdGVtcyI6W3siX19kaXNjcmltaW5hdG9yIjoic3RyaW5nLmhlYWRlciIsIl9fcGF5bG9hZCI6InN0cmluZ1BheWxvYWQifSx7IkNsYXNzLWN1c3RvbSI6bnVsbCwiX19kaXNjcmltaW5hdG9yIjoidGVzdFBvZC5oZWFkZXIiLCJfX3BheWxvYWQiOnsiTmljayI6InRlc3RQb2QuaGVhZGVyLnBheWxvYWQuTmljayIsIkJpcnRoZGF0ZS1jdXN0b20iOiIyMDEyLTEyLTEyIiwiTmFtZSI6InRlc3RQb2QuaGVhZGVyLnBheWxvYWQuTmFtZSIsIkFnZS1jdXN0b20iOjE1fX0seyJfX2Rpc2NyaW1pbmF0b3IiOiJyZWNvcmQuaGVhZGVyIiwiX19wYXlsb2FkIjp7Ik5hbWUiOiJyZWNvcmQuaGVhZGVyLk5hbWUifX1dfX0=";
		const string json = $$"""
									{
									  "{{DISCRIMINATOR_LABEL}}": "testPod",
									  "{{PAYLOAD_LABEL}}": {
									    "Class-custom": "class",
									    "{{DISCRIMINATOR_LABEL}}": "testPayload",
									    "{{PAYLOAD_LABEL}}": {
									      "Nick": "payload.Nick",
									      "Birthdate-custom": "2012-12-12",
									      "Name": "payload.Name",
									      "Age-custom": 23
									    },
									    "{{ITEMS_LABEL}}": [
									      {
									        "{{DISCRIMINATOR_LABEL}}": "string.header",
									        "{{PAYLOAD_LABEL}}": "stringPayload"
									      },
									      {
									        "Class-custom": null,
									        "{{DISCRIMINATOR_LABEL}}": "testPod.header",
									        "{{PAYLOAD_LABEL}}": {
									          "Nick": "testPod.header.payload.Nick",
									          "Birthdate-custom": "2012-12-12",
									          "Name": "testPod.header.payload.Name",
									          "Age-custom": 15
									        }
									      },
									      {
									        "{{DISCRIMINATOR_LABEL}}": "record.header",
									        "{{PAYLOAD_LABEL}}": {
									          "Name": "record.header.Name"
									        }
									      }
									    ]
									  }
									}
									""";
		var bytes = base64.FromBase64String();
		var builder = bytes.BuildPod2()
			.FromUtf8Bytes("json")
			.FromJsonNode();
		// var builder = json.BuildPod2("")
		// 	.FromJsonNode();
		var pod = builder.Pod;
		Assert.NotNull(pod);
		Assert.Equal("testPod", pod.Discriminator);
		if (pod.TryAs<Pod2<string,TestPayloadDerived2>>(out var pod2))
		{
			Assert.Equal("payload.Nick",pod2.Payload.Nick);
		} else Assert.Fail("Fail on TryAs");
		var testPod = pod.As<TestPod2>();
		Assert.NotNull(testPod);
		if (testPod["record.header"] is JsonNodePod2<string> pod3)
		{
			var pay = pod3.As<TestRecordPayload>();
			Assert.NotNull(pay);
		} else Assert.Fail("h1 isn't JsonNodePod");
	}

	[Fact(DisplayName = "JsonPod - ToJson")]
	public void ToJson()
	{
		TestPayload2 payload = new TestPayloadDerived2
		{
			Name = "payload.Name",
			Age = 23,
			Nick = "payload.Nick",
			Birthdate = DateOnly.Parse("12/12/2012")
		};
		var rr = JsonSerializer.Serialize(payload);
		Output.WriteLine(rr);
		Output.WriteLine("=================== MANUAL");
		{
			TestPod2 testPod = new("testPayload", payload)
			{
				new Pod2<string, object>("string.header", "stringPayload"),
				new TestPod2("testPod.header", new TestPayloadDerived2
				{
					Name = "testPod.header.payload.Name",
					Age = 15,
					Nick = "testPod.header.payload.Nick",
					Birthdate = DateOnly.Parse("12/12/2012")
				})
			};
			testPod.Class = "class";
			testPod.Add("record.header", new TestRecordPayload("record.header.Name"));
			// Create JsonNode pod
			JsonNodePod2<string> jsonPod = new("testPod", testPod);
			// Create Utf8 pod
			Pod2<string, byte[]> utf8Pod = new("utf8", Encoding.UTF8.GetBytes(jsonPod));
			// Create formatted json string
			var json = jsonPod.SerializeToJson(true);
			Output.WriteLine($"json:\r\n{json}");
			Output.WriteLine($"utf8:\r\n{utf8Pod.Payload.ToBase64String()}");
			// Assertion
			Assert.Contains($@"""{DISCRIMINATOR_LABEL}"": ""testPayload""", json);
			Assert.Contains($@"""{DISCRIMINATOR_LABEL}"": ""string.header""", json);
			Assert.Contains($@"""{DISCRIMINATOR_LABEL}"": ""testPod.header""", json);
			Assert.Contains($@"""{DISCRIMINATOR_LABEL}"": ""record.header""", json);
		}
		Output.WriteLine("=================== BUILDER");
		{
			TestPod2 testPod = new("testPayload", payload)
			{
				Class = "class"
			};
			var builder = testPod.RebuildPod2()
				.AddHeader("string.header","stringPayload")
				.AddHeader(new TestPod2("testPod.header",new TestPayloadDerived2()
				{
					Name = "testPod.header.payload.Name",
					Age = 15,
					Nick = "testPod.header.payload.Nick",
					Birthdate = DateOnly.Parse("12/12/2012")
				}))
				.AddHeader("record.header",new TestRecordPayload("record.header.Name"));
			Output.WriteLine($"json:\r\n{builder.ToJsonNode("json").Pod.SerializeToJson(true)}");
			Output.WriteLine($"utf8:\r\n{builder.ToJsonNode("json").ToUtf8Bytes("utf8").Pod.Payload.ToBase64String()}");
		}
		// var pod = payload.BuildPod2<string, PayloadBase2>("podKey").Pod;
		// var jsonPod = pod.BuildPod2().ToJson().Pod;
		// var json = jsonPod.ToJson();
		// Output.WriteLine("Serialized json:");
		// Output.WriteLine(json);
		// Assert.Contains($@"""{AnyConverter.DISCRIMINATOR_LABEL}"": ""podKey""", json);
		// Assert.Contains(@"""Name"": ""payloadName""", json);
		// Assert.Contains(@"""Age"": 23", json);
		// Assert.Contains(@"""Nick"": ""payloadNick""", json);
		// Assert.DoesNotContain($@"""{AnyConverter.ITEMS_LABEL}"": ", json);
		//
		// pod.Add("header1Payload".BuildPod2("header1").ToJson().Pod);
		// pod.Add("header2Payload".BuildPod2("header2").ToJson().Pod);
		// PodBase2 pod3 = new("header3", payload);
		// pod3.Add("header3.3Payload".BuildPod2("header3.3").ToJson().Pod);
		// pod.Add(pod3);
		// Assert.Throws<ArgumentException>(() => pod.Add("".BuildPod2("header1").ToJson().Pod));
		// json = pod.BuildPod2().ToJson().Pod.ToJson();
		// Output.WriteLine("Serialized json (with headers):");
		// Output.WriteLine(json);
		// Assert.Contains($@"""{AnyConverter.ITEMS_LABEL}"": ", json);
		
	}
	[Fact(DisplayName = "Implicit operator")]
	public void ImplicitOperator()
	{
		// JsonPod2<string, string> pod = new("discriminator", "payload");
		Pod2<string, string> pod = new("discriminator", "payload");
		Assert.Equal("payload", pod);
	}
	[Fact(DisplayName = "Header edition")]
	public void HeaderEdition()
	{
		const string headerDiscriminator = "testPayloadPod";
		Pod2<string, string> pod = new("discriminator", "payload");
		pod.Add(new TestPayloadDerived2
		{
			Name = "value1",
			Nick = "Nick",
			Age = 12,
			Birthdate = DateOnly.Parse("12/12/2012")
		}.BuildPod2().ToJsonNode(headerDiscriminator).Pod);
		if (pod[headerDiscriminator] is JsonNodePod2<string> valJ)
		{
			var val = valJ.As<TestPayload2>();
			Assert.NotNull(val);
			Output.WriteLine($"Original value: {val.Name}");
			val.Name = "value2";
			if (pod[headerDiscriminator] is JsonNodePod2<string> val2J)
			{
				var val2 = val2J.As<TestPayload2>();
				Assert.NotNull(val2);
				Output.WriteLine($"Edited value: {val2.Name}");
				Assert.NotEqual("value2", val2.Name);
				Assert.Throws<ArgumentException>(() => pod.Add(val2.BuildPod2().ToPod(headerDiscriminator)
					.Pod)); // Fails because I can't add it if already exist
			} else
				Assert.Fail("");
			Assert.False(pod.Remove($"{headerDiscriminator}1"));
			Assert.True(pod.Remove(headerDiscriminator));
			pod.Add(val.BuildPod2().ToJsonNode(headerDiscriminator)
				.Pod);
			if (pod[headerDiscriminator] is JsonNodePod2<string> val3J)
			{
				var val3 = val3J
					.As<TestPayload2>();
				Assert.NotNull(val3);
				Output.WriteLine($"Replaced value: {val3.Name}");
				Assert.Equal("value2", val3.Name);
			}else
				Assert.Fail("");
		}else
			Assert.Fail("");
	}
 }
public class TestPod2(string discriminator, TestPayload2 payload) : Pod2<string, TestPayload2>(discriminator, payload)
{
	public TestPod2():this(null!,null!){}
	[JsonPropertyName("Class-custom")]
	public string? Class { get; set; }
}
[JsonPolymorphic]
[JsonDerivedType(typeof(TestPayloadDerived2),typeDiscriminator: "Derived")]
public class TestPayload2
{
	public string? Name { get; set; }
	[JsonPropertyName("Age-custom")]
	public required int Age { get; init; }
}

public class TestPayloadDerived2 : TestPayload2
{
	public required string Nick { get; set; }
	[JsonPropertyName("Birthdate-custom")]
	public DateOnly Birthdate { get; set; }
}
public record TestRecordPayload(string Name);