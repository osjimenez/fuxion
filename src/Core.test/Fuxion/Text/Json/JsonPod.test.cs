using System.Text;
using System.Text.Json.Serialization;
using Fuxion.Reflection;
using Fuxion.Text.Json;
using static Fuxion.Text.Json.IPodConverter<Fuxion.Text.Json.JsonNodePod<string>, string, System.Text.Json.Nodes.JsonNode>;

namespace Fuxion.Test.Text.Json;

public class JsonPodTest : BaseTest<JsonPodTest>
{
	public JsonPodTest(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
	[Fact(DisplayName = "ToJson")]
	public void ToJson()
	{
		TestPayload payload = new TestPayloadDerived
		{
			Name = "payload.Name",
			Age = 23,
			Nick = "payload.Nick",
			Birthdate = DateOnly.Parse("12/12/2012")
		};
		Output.WriteLine("=================== MANUAL");
		{
			TestPod testPod = new("testPayload", payload)
			{
				new Pod<string, object>("string.header", "stringPayload"),
				new TestPod("testPod.header", new TestPayloadDerived
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
			JsonNodePod<string> jsonPod = new("testPod", testPod);
			// Create Utf8 pod
			Pod<string, byte[]> utf8Pod = new("utf8", Encoding.UTF8.GetBytes(jsonPod.Payload.ToJsonString()));
			// Create formatted json string
			var json = jsonPod.Payload.ToJsonString(true);
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
			TestPod testPod = new("testPayload", payload)
			{
				Class = "class"
			};
			var builder = testPod.RebuildPod<string, TestPayload, TestPod>()
				.AddHeader("string.header", "stringPayload")
				.AddHeader(new TestPod("testPod.header", new TestPayloadDerived
				{
					Name = "testPod.header.payload.Name",
					Age = 15,
					Nick = "testPod.header.payload.Nick",
					Birthdate = DateOnly.Parse("12/12/2012")
				}))
				.AddHeader("record.header", new TestRecordPayload("record.header.Name"));
			Output.WriteLine($"json:\r\n{builder.ToJsonNode("json").Pod.Payload.ToJsonString(true)}");
			Output.WriteLine($"utf8:\r\n{builder.ToJsonNode("json").ToUtf8Bytes("utf8").Pod.Payload.ToBase64String()}");
		}
	}
	[Fact(DisplayName = "FromJson")]
	public void FromJson()
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
		var builder = bytes.BuildPod()
			.FromUtf8Bytes("json")
			.FromJsonNode();
		var pod = builder.Pod;
		Assert.NotNull(pod);
		Assert.Equal("testPod", pod.Discriminator);
		if (pod.TryAs<Pod<string, TestPayloadDerived>>(out var pod2))
			Assert.Equal("payload.Nick", pod2.Payload.Nick);
		else
			Assert.Fail("Fail on TryAs");
		var testPod = pod.As<TestPod>();
		Assert.NotNull(testPod);
		if (testPod["record.header"] is JsonNodePod<string> pod3)
		{
			var pay = pod3.As<TestRecordPayload>();
			Assert.NotNull(pay);
		} else
			Assert.Fail("'record.header' isn't JsonNodePod<string>");
	}
	[Fact(DisplayName = "Header edition")]
	public void HeaderEdition()
	{
		const string headerDiscriminator = "testPayloadPod";
		Pod<string, string> pod = new("discriminator", "payload");
		pod.Add(new TestPayloadDerived
			{
				Name = "value1",
				Nick = "Nick",
				Age = 12,
				Birthdate = DateOnly.Parse("12/12/2012")
			}.BuildPod()
			.ToJsonNode(headerDiscriminator)
			.Pod);
		if (pod[headerDiscriminator] is JsonNodePod<string> valJ)
		{
			var val = valJ.As<TestPayload>();
			Assert.NotNull(val);
			Output.WriteLine($"Original value: {val.Name}");
			val.Name = "value2";
			if (pod[headerDiscriminator] is JsonNodePod<string> val2J)
			{
				var val2 = val2J.As<TestPayload>();
				Assert.NotNull(val2);
				Output.WriteLine($"Edited value: {val2.Name}");
				Assert.NotEqual("value2", val2.Name);
				Assert.Throws<ArgumentException>(() => pod.Add(val2.BuildPod()
					.ToPod(headerDiscriminator)
					.Pod)); // Fails because I can't add it if already exist
			} else
				Assert.Fail("");
			Assert.False(pod.Remove($"{headerDiscriminator}1"));
			Assert.True(pod.Remove(headerDiscriminator));
			pod.Add(val.BuildPod()
				.ToJsonNode(headerDiscriminator)
				.Pod);
			if (pod[headerDiscriminator] is JsonNodePod<string> val3J)
			{
				var val3 = val3J.As<TestPayload>();
				Assert.NotNull(val3);
				Output.WriteLine($"Replaced value: {val3.Name}");
				Assert.Equal("value2", val3.Name);
			} else
				Assert.Fail("");
		} else
			Assert.Fail("");
	}
	[Fact(DisplayName = "Implicit operator")]
	public void ImplicitOperator()
	{
		Pod<string, string> pod = new("discriminator", "payload");
		Assert.Equal("payload", pod);
	}
}

file class TestPod(string discriminator, TestPayload payload) : Pod<string, TestPayload>(discriminator, payload)
{
	public TestPod() : this(null!, null!) { }
	[JsonPropertyName("Class-custom")]
	public string? Class { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(TestPayloadDerived), "Derived")]
file class TestPayload
{
	public string? Name { get; set; }
	[JsonPropertyName("Age-custom")]
	public required int Age { get; init; }
}
file class TestPayloadDerived : TestPayload
{
	public required string Nick { get; set; }
	[JsonPropertyName("Birthdate-custom")]
	public DateOnly Birthdate { get; set; }
}
file record TestRecordPayload(string Name);