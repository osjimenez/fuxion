using System.Collections;

namespace Fuxion;

public class BypassPod2<TDiscriminator> : IPod2<TDiscriminator, object, BypassPodCollection2<TDiscriminator>> 
	where TDiscriminator : notnull
{
	public BypassPod2(TDiscriminator discriminator, object @object)
	{
		Discriminator = discriminator;
		Payload = @object;
	}
	public TDiscriminator Discriminator { get; }
	public object Payload { get; }
	public BypassPodCollection2<TDiscriminator> Headers { get; } = new();
}

public class BypassPod2<TDiscriminator, TPayload> : IPod2<TDiscriminator, TPayload, BypassPodCollection2<TDiscriminator>>
	where TDiscriminator : notnull
{
	public BypassPod2(TDiscriminator discriminator, TPayload @object)
	{
		Discriminator = discriminator;
		Payload = @object;
	}
	public TDiscriminator Discriminator { get; }
	public TPayload Payload { get; }
	public BypassPodCollection2<TDiscriminator> Headers { get; } = new();
}

// public class BypassPod2<TDiscriminator, TOutside, TInside> : BypassPod2<TDiscriminator>, ICrossPod2<TDiscriminator, TOutside, TInside, BypassPodCollection2<TDiscriminator>>
// 	where TDiscriminator : notnull 
// 	where TInside : notnull
// {
// 	public BypassPod2(TDiscriminator discriminator, TInside @object) : base(discriminator, @object) { }
// 	public new TOutside Outside() => (TOutside)base.Outside();
// 	public new TInside Inside() => (TInside)base.Inside();
// }
public class BypassPodCollection2<TDiscriminator> : IPodCollection2<TDiscriminator>
	where TDiscriminator : notnull
{
	readonly Dictionary<TDiscriminator, IPod2<TDiscriminator, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	// TODO remove nullable Payload!
	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] => new BypassPod2<TDiscriminator, object>(discriminator, dic[discriminator].Payload!);
	public void Add(IPod2<TDiscriminator, object> pod) => dic.Add(pod.Discriminator, pod);
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	// TODO remove nullable Payload!
	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() => dic.Values.Select(_ => new BypassPod2<TDiscriminator>(_.Discriminator, _.Payload!)).GetEnumerator();
}