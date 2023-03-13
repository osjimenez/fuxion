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
		var json1 = first.Json();
		var utf = json1.Utf8();
		var zip = utf.Zip("zip");
		var json2 = zip.Json();
		var pod = json2.Pod;
		var oo = pod.Outside();

	}
}

public static class Extensions
{
	public static IPodBuilder<TDiscriminator, object, TObject> ToPod<TDiscriminator, TObject>(this TObject me, TDiscriminator discriminator) 
		where TDiscriminator : notnull
		where TObject : notnull
	{
		var pod = new Pod<TDiscriminator, object, TObject>(discriminator, me);
		var res = new PodBuilder<TDiscriminator, object, TObject>(pod);
		return res;
	}
	public static IPodBuilder<TDiscriminator, object, JsonValue> Json<TDiscriminator, TObject>(this IPodBuilder<TDiscriminator, object, TObject> me)
		where TDiscriminator : notnull
		where TObject : notnull =>
		// new PodBuilder<TDiscriminator, JsonValue, object>(new Pod<TDiscriminator, JsonValue, object>(me.Pod.Discriminator, new JsonPod<TDiscriminator, object>(me.Pod.Discriminator, me.Pod.Outside())));
		new PodBuilder<TDiscriminator, object, JsonValue>(new JsonPod<TDiscriminator, object>(me.Pod.Discriminator, me.Pod.Outside()));
	public static IPodBuilder<TDiscriminator, object, byte[]> Utf8<TDiscriminator>(this IPodBuilder<TDiscriminator, object, JsonValue> me)
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[]>(new Pod<TDiscriminator, object, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	public static IPodBuilder<TDiscriminator, object, byte[]> Zip<TDiscriminator>(this IPodBuilder<TDiscriminator, object, byte[]> me, TDiscriminator discriminator) where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[]>(new ZipPod<TDiscriminator>(discriminator, me.Pod.Inside()));
}
public interface IPodBuilder<TDiscriminator, TOutside, TInside>
	where TDiscriminator : notnull
	where TOutside : notnull
	where TInside : notnull
{
	IPod<TDiscriminator, TOutside, TInside> Pod { get; set; }
}
public class PodBuilder<TDiscriminator, TOutside, TInside>:IPodBuilder<TDiscriminator, TOutside, TInside>
	where TDiscriminator : notnull
	where TOutside : notnull
	where TInside : notnull
{
	public PodBuilder(IPod<TDiscriminator, TOutside,TInside> pod)
	{
		Pod = pod;
	}
	public IPod<TDiscriminator, TOutside, TInside> Pod { get; set; }
}