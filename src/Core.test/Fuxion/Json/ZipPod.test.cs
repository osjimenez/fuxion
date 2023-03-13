using System.Text;
using System.Text.Json.Nodes;
using Fuxion.Json;

namespace Fuxion.Test.Json;

public class ZipPodTest : BaseTest<ZipPodTest>
{
	public ZipPodTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Zip")]
	public void Zip()
	{
		PayloadBase pay = new()
		{
			Name = "name", Age = 50
		};
		var first = pay.ToPod("pay");
		var firstPod = first.Pod;
		var json1 = first.Json();
		var json1Pod = json1.Pod;
		var utf = json1.Utf8();
		var utfPod = utf.Pod;
		var zip = utf.Zip("zip");
		var zipPod = zip.Pod;
		var json2 = zip.Json();
		var json2Pod = json2.Pod;
		var oo = json2Pod.Payload;

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
		new PodBuilder<TDiscriminator, TObject, JsonValue, JsonPod<TDiscriminator, TObject>>(new (me.Pod.Discriminator, me.Pod.Outside()));
	
	public static IPodBuilder<IPod<TDiscriminator, object, byte[]>> Utf8o<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, JsonValue>> me)
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[], IPod<TDiscriminator, object, byte[]>>(new Pod<TDiscriminator, object, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	public static IPodBuilder<IPod<TDiscriminator, TObject, byte[]>> Utf8<TDiscriminator, TObject>(this IPodBuilder<IPod<TDiscriminator, TObject, JsonValue>> me)
		where TDiscriminator : notnull 
		where TObject : notnull =>
		new PodBuilder<TDiscriminator, TObject, byte[], IPod<TDiscriminator, TObject, byte[]>>(new Pod<TDiscriminator, TObject, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	
	public static IPodBuilder<ZipPod<TDiscriminator>> Zip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(new (discriminator, me.Pod.Inside()));
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