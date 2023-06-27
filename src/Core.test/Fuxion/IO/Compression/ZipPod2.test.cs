using Fuxion.IO.Compression;
using Fuxion.Json;
using Fuxion.Test.Json;

namespace Fuxion.Test.IO.Compression;

public class ZipPod2Test : BaseTest<ZipPod2Test>
{
	public ZipPod2Test(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Zip")]
	public void Zip()
	{
		TestPayload2 pay = new TestPayloadDerived2 {
			Name = "payloadName", Age = 23, Nick = "payloadNick", Birthdate = DateOnly.Parse("12/12/2012")
		};
		var first = pay.BuildPod2().ToPod("").Transform(_ =>
		{
			// _.Pod.Add("header1Payload".BuildPod2("header1").Pod);
			// _.Pod.Add("header2Payload".BuildPod2("header2").Pod);
			// _.Pod.Add(new TestPod2("header3",pay).Transform(h3 =>
			// {
			// 	h3.Class = "header3";
			// 	//h3.Add("header3.3Payload".BuildPod2("header3.3").Pod);
			// }));
		});
		// first.Pod.Add("header1Payload".BuildPod2("header1").Pod);
		// first.Pod.Add("header2Payload".BuildPod2("header2").Pod);
		Output.WriteLine($"first:\r\n{first.Pod.SerializeToJson()}");
		var json1 = first.ToJson();
		// json1.Pod.Add("header1Payload".BuildPod2("header1").ToJson().Pod);
		// json1.Pod.Add("header2Payload".BuildPod2("header2").ToJson().Pod);
		// PodBase2 pod3 = new("header3", pay);
		// pod3.Add("header3.3Payload".BuildPod2("header3.3").ToJson().Pod);
		// json1.Pod.Add(pod3);
		Output.WriteLine($"json1:\r\n{json1.Pod.SerializeToJson()}");
		var utf = json1.ToUtf8();
		var utfPod = utf.Pod;
		Output.WriteLine($"utf:\r\n{utf.Pod.SerializeToJson()}");
		var zip = utf.ToZip("zip");
		Output.WriteLine($"zip:\r\n{zip.Pod.SerializeToJson()}");
		var json2 = zip.ToJson();
		Output.WriteLine($"json2:\r\n{json2.Pod.SerializeToJson()}");

		// Same as fluent
		var pod = pay.BuildPod("pay").ToJson().ToUtf8().ToZip("zip").ToJson().Pod;
	}
	[Fact(DisplayName = "Unzip")]
	public void Unzip()
	{
		var json = """
			{
			  "__discriminator": "zip",
			  "__payload": "H4sIAAAAAAAACqvm5VJQUHLJLE4uyszNzEssyS9SslJQKkisVNIBS3mkJqakFhUDBaNB/GoQgU1HBlidIUQXUEFAYmVOfmIKkhRMBKSgVocIw4xwG2aE2zDnnMTiYoRKY7ghOGxBKCDNq8Z6CJ0Y7gNKoriQlysWUy3UAqCYX2ZyNjTYQXJgLlQ9SDYxNxVZFsRFyDqmgySNjCECQJuQrMO0DJ9V+CxCsQZofi0APVVrFDkCAAA="
			}
			""";
		var json1 = json.FromJsonPod2<string>().BuildPod();
		Output.WriteLine($"json1:\r\n{json1.Pod.SerializeToJson()}");
		var bytes = json1.FromBytes();
		Output.WriteLine($"bytes:\r\n{bytes.Pod.SerializeToJson()}");
		var unzip = bytes.FromZip();
		Output.WriteLine($"unzip:\r\n{unzip.Pod.SerializeToJson()}");
		var utf = unzip.FromUtf8();
		Output.WriteLine($"utf:\r\n{utf.Pod.SerializeToJson()}");
		var json2 = utf.FromJson();
		Output.WriteLine($"json2:\r\n{json2.Pod.SerializeToJson()}");
		var pod = json2.Pod;

		var payPod = pod.CastWithPayload<PayloadBase>();
		Assert.NotNull(payPod?.Payload);
		Assert.StartsWith("payloadName", payPod.Payload.Name);
		var pay2Pod = pod.CastWithPayload<PayloadDerived>();
		Assert.NotNull(pay2Pod?.Payload);
		Assert.StartsWith("payloadNick", pay2Pod.Payload.Nick);
		Assert.Equal(23, payPod.Payload.Age);
		Assert.True(pod.Has("header1"));
		var pod3 = pod["header3"];
		Assert.NotNull(pod3);
		if (pod3 is JsonPod2<string, object> pod33)
		{
			Assert.True(pod33.Has("header3.3"));
		} else
			Assert.True(false);
		
		// Same as fluent
		pod = json.FromJsonPod2<string>().BuildPod().FromBytes().FromZip().FromUtf8().FromJson().Pod;
	}
}