using System.Text;
using System.Text.Json.Nodes;
using Fuxion.Json;
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
		var pod = json.FromJson<JsonPod<string, object>>();
		Assert.NotNull(pod);
		if (pod.Discriminator == "zip")
		{
			var zipPayload = pod.CastWithPayload<byte[]>();
			Assert.NotNull(zipPayload?.Payload);
			var zipPod = ZipPod<string>.CreateToDecompress("zip", zipPayload.Payload);
			var pod2 = Encoding.UTF8.GetString(zipPod.Outside()).FromJson<JsonPod<string, object>>();
			Assert.NotNull(pod2);
			if (pod2.Discriminator == "pay")
			{
				var payPod = pod2.CastWithPayload<PayloadBase>();
				Assert.NotNull(payPod?.Payload);
				Assert.StartsWith("name-",payPod.Payload.Name);
				var pay2Pod = pod2.CastWithPayload<PayloadDerived>();
				Assert.NotNull(pay2Pod?.Payload);
				Assert.StartsWith("nick-",pay2Pod.Payload.Nick);
				Assert.Equal(50, payPod.Payload.Age);
			}else throw new TestCanceledException($"Not PayloadBase inside JsonPod: {pod2.Discriminator}");
		} else throw new TestCanceledException($"Not ZipPod inside JsonPod");


		// PayloadBase pay = new()
		// {
		// 	Name = "name", Age = 50
		// };
		// var first = pay.ToPod("pay");
		// var firstPod = first.Pod;
		// var json1 = first.Json();
		// var json1Pod = json1.Pod;
		// Output.WriteLine($"utfPod:\r\n{json1Pod.ToJson()}");
		// var utf = json1.Utf8();
		// var utfPod = utf.Pod;
		// Output.WriteLine($"utfPod:\r\n{utfPod.ToJson()}");
		// var zip = utf.Zip("zip");
		// var zipPod = zip.Pod;
		// Output.WriteLine($"zipPod:\r\n{zipPod.ToJson()}");
		// var json2 = zip.Json();
		// var json2Pod = json2.Pod;
		// Output.WriteLine($"json2Pod:\r\n{json2Pod.ToJson()}");
	}
}

public static class Extensions
{
	public static IPodBuilder<IPod<TDiscriminator, TObject, TObject>> ToPod<TDiscriminator, TObject>(this TObject me, TDiscriminator discriminator) 
		where TDiscriminator : notnull
		where TObject : notnull
	{
		var pod = new Pod<TDiscriminator, TObject, TObject>(discriminator, me);
		var res = new PodBuilder<TDiscriminator, TObject, TObject, IPod<TDiscriminator, TObject, TObject>>(pod);
		return res;
	}
	public static IPodBuilder<JsonPod<TDiscriminator, TObject>> Json<TDiscriminator, TObject>(this IPodBuilder<IPod<TDiscriminator, TObject, TObject>> me)
		where TDiscriminator : notnull
		where TObject : notnull =>
		new PodBuilder<TDiscriminator, TObject, JsonValue, JsonPod<TDiscriminator, TObject>>(new (me.Pod.Discriminator, me.Pod.Inside()));
	
	public static IPodBuilder<IPod<TDiscriminator, object, byte[]>> Utf8o<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, JsonValue>> me)
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[], IPod<TDiscriminator, object, byte[]>>(new Pod<TDiscriminator, object, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	public static IPodBuilder<IPod<TDiscriminator, TObject, byte[]>> Utf8<TDiscriminator, TObject>(this IPodBuilder<IPod<TDiscriminator, TObject, JsonValue>> me)
		where TDiscriminator : notnull 
		where TObject : notnull =>
		new PodBuilder<TDiscriminator, TObject, byte[], IPod<TDiscriminator, TObject, byte[]>>(new Pod<TDiscriminator, TObject, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	
	public static IPodBuilder<ZipPod<TDiscriminator>> Zip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(ZipPod<TDiscriminator>.CreateToCompress(discriminator, me.Pod.Inside()));
}
public interface IPodBuilder<out TPod>
{
	TPod Pod { get; }
}
class PodBuilder<TDiscriminator, TOutside, TInside, TPod>:IPodBuilder<TPod>
	where TDiscriminator : notnull
	where TOutside : notnull
	where TInside : notnull
	where TPod : IPod<TDiscriminator, TOutside,TInside>
{
	public PodBuilder(TPod pod)
	{
		Pod = pod;
	}
	public TPod Pod { get; }
}