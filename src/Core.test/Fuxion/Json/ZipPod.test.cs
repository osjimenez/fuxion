using System.Text;
using System.Text.Json.Nodes;
using Fuxion.IO.Compression;
using Fuxion.Json;
using Fuxion.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fuxion.Test.Json;

public class ZipPodTest : BaseTest<ZipPodTest>
{
	public ZipPodTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Zip")]
	public void Zip()
	{
		PayloadDerived pay = new()
		{
			Name = $"name-{"ABC".RandomString(500)}", 
			Nick = $"nick-{"ABC".RandomString(500)}",
			Age = 50
		};
		var first = pay.ToPod("pay");
		var firstPod = first.Pod;
		var json1 = first.Json();
		var json1Pod = json1.Pod;
		Output.WriteLine($"utfPod:\r\n{json1Pod.ToJson()}");
		var utf = json1.Utf8();
		var utfPod = utf.Pod;
		Output.WriteLine($"utfPod:\r\n{utfPod.ToJson()}");
		var zip = utf.Zip("zip");
		var zipPod = zip.Pod;
		Output.WriteLine($"zipPod:\r\n{zipPod.ToJson()}");
		var json2 = zip.Json();
		var json2Pod = json2.Pod;
		Output.WriteLine($"json2Pod:\r\n{json2Pod.ToJson()}");

		// Same as fluent
		var pod = pay.ToPod("pay").Json().Utf8().Zip("zip").Json().Pod;
	}
	[Fact(DisplayName = "Unzip")]
	public void Unzip()
	{
		var json = """
			{
			  "Discriminator": "zip",
			  "Payload": "H4sIAAAAAAAACkWTzU4CQRCE7yS8w2bPknjx4m1mPBtfYYLGbBQw6IUQ3t2u+noWAuxM/1ZX1163m2maX5bf/Xk5LMf+dzrPz9P80y/zg11v/fJ96u9hvOoeltdl/6WYYzx3tdTayvpprbRawlaLTuHRQR9HhRtX1UnWsNUmW1j1V7ERzsV5TUGKjLJRwyYVdYiDVMD2kSGXEuzNlhEEPltULK6RkVCNzil81ZMGdqsEN7VibvqFx9hcE5AA9Y+8nNLdBMzosNvsikKkqpGi9m5C7fgaqylLNHLCXEIxSYNzd6ajBzThgz3mgzKMRimmTZdLuKehabFZt6EMKaEfPqyEeO5YEhRDpGrAmLPHvK5qygwF0ait83OryYTz/GBzHkkBWQB+dBpqSIKtKYThVNU3tJQQKvN0UOYn+xvo4M4cphZT2+CVA39iQsRsm4q5qAyEwzzc1YCuU9ar6oYkkLXOvAO5hASFBqjvSPaF4H3nRQEWGwSYCiAQJnE0er8v0grJtyKlTnlmLqsSyqeE8PSo6227uf0Dgg0uYVcEAAA="
			}
			""";
		var json1 = json.FromJsonPod<string>();
		var first = json1.FromPod("zip");
		var utf = first.Utf8();
		var base64 = first.Base64();
		var unzip = utf.Unzip("pay");
		var json2 = unzip.Json();
		var pod = json2.Pod;
		// if (pod.Discriminator == "zip")
		// {
		// 	var zipPayload = pod.CastWithPayload<byte[]>();
		// 	Assert.NotNull(zipPayload?.Payload);
		// 	var zipPod = ZipPod<string>.CreateToDecompress("zip", zipPayload.Payload);
		// 	var pod2 = Encoding.UTF8.GetString(zipPod.Outside()).FromJson<JsonPod<string, object>>();
		// 	Assert.NotNull(pod2);
		// 	if (pod2.Discriminator == "pay")
		// 	{
		// 		var payPod = pod2.CastWithPayload<PayloadBase>();
		// 		Assert.NotNull(payPod?.Payload);
		// 		Assert.StartsWith("name-",payPod.Payload.Name);
		// 		var pay2Pod = pod2.CastWithPayload<PayloadDerived>();
		// 		Assert.NotNull(pay2Pod?.Payload);
		// 		Assert.StartsWith("nick-",pay2Pod.Payload.Nick);
		// 		Assert.Equal(50, payPod.Payload.Age);
		// 	}else throw new TestCanceledException($"Not PayloadBase inside JsonPod: {pod2.Discriminator}");
		// } else throw new TestCanceledException($"Not ZipPod inside JsonPod");
	}
}