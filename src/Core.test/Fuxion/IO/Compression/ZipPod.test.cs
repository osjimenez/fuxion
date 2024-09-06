using System.Text.Json.Serialization;
using Fuxion.IO.Compression;
using Fuxion.Test.Text.Json;
using Fuxion.Text.Json;

namespace Fuxion.Test.IO.Compression;

public class ZipPodTest(ITestOutputHelper output) : BaseTest<ZipPodTest>(output)
{
	[Fact(DisplayName = "Zip")]
	public void Zip()
	{
		TestPayload payload = new TestPayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick",
			#if NET462
			Birthdate = DateTime.Parse("12/12/2012")
			#else
			Birthdate = DateOnly.Parse("12/12/2012")
			#endif
		};
		var builder = payload.BuildPod()
			.ToPod("testPayload")
			.AddHeader("header.string", "payload.string")
			.AddHeader("header.testPod", new TestPod
			{
				Class = "class"
			}.Transform(p =>
			{
				p.Add("header.string", "payload.string");
			}));
		var json = builder.ToJsonNode("json");
		Output.WriteLine($"json:\r\n{json.Pod.Payload.ToJsonString(true)}");
		var utf = json.ToUtf8Bytes("utf");
		Output.WriteLine($"utf:\r\n{utf.Pod.Payload.ToBase64String()}");
		var zip = utf.ToZip("zip");
		Output.WriteLine($"zip:\r\n{zip.Pod.Payload.ToBase64String()}");
		// Same as fluent
		var bytes = payload.BuildPod().ToJsonNode("json").ToUtf8Bytes("utf").ToZip("zip").Pod.Payload;
	}
	[Fact(DisplayName = "Unzip")]
	public void Unzip()
	{
		var base64 =
			"H4sIAAAAAAAACq2PQQ+CMAyF/0vPwyjeuKl3490YsmwLTDdm1nIwhP/utkAQJJ5Menl9X9vXDspSahReW91wch4KuKNrgAXjyV/GcQlFt0KRQroMwAI+a/EIxNBJisFRe6olJ5WJFsnZAOTbXZ6lCv6ZW/UxFBWDQzXh+b6PdzQpi1Bc1zLVikvlN0heN9U81bh4NHv2Y0H6zX39dTIccYovokzMYk3TGjMbHRv/yn7rY70Bn8IuG7wBAAA=";
		Output.WriteLine($"base64:\r\n{base64}");
		var unzip = base64.FromBase64String()
			.BuildPod().FromZip("unzip");
		Output.WriteLine($"unzip:\r\n{unzip.Pod.Payload.ToBase64String()}");
		var utf = unzip.FromUtf8Bytes("utf");
		Output.WriteLine($"utf:\r\n{utf.Pod.Payload}");
		var json = utf.FromJsonNode();
		Output.WriteLine($"json:\r\n{json.Pod.Payload.ToJsonString(true)}");
		var payPod = json.Pod.As<Pod<string,TestPayload>>()?.Payload;
		Assert.NotNull(payPod);
		Assert.StartsWith("payloadName", payPod.Name);
		var pay2Pod = json.Pod.As<Pod<string,TestPayloadDerived>>()?.Payload;
		Assert.NotNull(pay2Pod);
		Assert.StartsWith("payloadNick", pay2Pod.Nick);
		Assert.Equal(23, payPod.Age);
		// Same as fluent
		var payload = base64.FromBase64String()
			.BuildPod()
			.FromZip("unzip")
			.FromUtf8Bytes("utf")
			.FromJsonNode()
			.Pod.As<Pod<string, TestPayloadDerived>>()?
			.Payload;
	}
}
file class TestPod(string discriminator, TestPayload payload) : Pod<string, TestPayload>(discriminator, payload)
{
	public TestPod() : this(null!, null!) { }
	[JsonPropertyName("Class-custom")]
	public string? Class { get; set; }
}
file class TestPayload
{
	public string? Name { get; set; }
	[JsonPropertyName("Age-custom")]
#if NET462
	[JsonInclude]
#endif
	public required int Age { get; init; }
}
file class TestPayloadDerived : TestPayload
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
file record TestRecordPayload(string Name);