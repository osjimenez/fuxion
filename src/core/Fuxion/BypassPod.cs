using System.Collections;

namespace Fuxion;

public class BypassPod<TDiscriminator> : ICrossPod<TDiscriminator, object, object, BypassPodCollection<TDiscriminator>> 
	where TDiscriminator : notnull
{
	readonly object _object;
	public BypassPod(TDiscriminator discriminator, object @object)
	{
		Discriminator = discriminator;
		_object = @object;
	}
	public TDiscriminator Discriminator { get; }
	public object Outside() => _object;
	public object Inside() => _object;
	public BypassPodCollection<TDiscriminator> Headers { get; } = new();
}

public class BypassPod<TDiscriminator, TPayload> : ICrossPod<TDiscriminator, TPayload, TPayload, BypassPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	readonly TPayload _object;
	public BypassPod(TDiscriminator discriminator, TPayload @object)
	{
		Discriminator = discriminator;
		_object = @object;
	}
	public TDiscriminator Discriminator { get; }
	
	public TPayload Outside() => _object;
	public TPayload Inside() => _object;
	public BypassPodCollection<TDiscriminator> Headers { get; } = new();
}

public class BypassPod<TDiscriminator, TOutside, TInside> : BypassPod<TDiscriminator>, ICrossPod<TDiscriminator, TOutside, TInside, BypassPodCollection<TDiscriminator>>
	where TDiscriminator : notnull 
	where TInside : notnull
{
	public BypassPod(TDiscriminator discriminator, TInside @object) : base(discriminator, @object) { }
	public new TOutside Outside() => (TOutside)base.Outside();
	public new TInside Inside() => (TInside)base.Inside();
}
public class BypassPodCollection<TDiscriminator> : IPodCollection<TDiscriminator, BypassPod<TDiscriminator>>
	where TDiscriminator : notnull
{
	readonly Dictionary<TDiscriminator, ICrossPod<TDiscriminator, object, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	
	public BypassPod<TDiscriminator> this[TDiscriminator discriminator] => new(discriminator, dic[discriminator].Outside());
	public void Add(ICrossPod<TDiscriminator, object, object> pod) => dic.Add(pod.Discriminator, pod);
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	
	public IEnumerator<BypassPod<TDiscriminator>> GetEnumerator() => dic.Values.Select(_ => new BypassPod<TDiscriminator>(_.Discriminator, _.Outside())).GetEnumerator();
}