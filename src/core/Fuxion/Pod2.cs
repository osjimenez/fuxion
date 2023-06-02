using System.Collections;

namespace Fuxion;

public interface IPod2<out TDiscriminator, out TPayload>
{
	TDiscriminator Discriminator { get; }
	TPayload? Payload { get; }
}
public interface IPod2<out TDiscriminator, out TPayload, out THeaders> : IPod2<TDiscriminator, TPayload>
	where THeaders : IPodCollection2<TDiscriminator>
{
	THeaders Headers { get; }
}
public interface IPodCollection2<TDiscriminator> : IEnumerable<IPod2<TDiscriminator, object>>
{
	bool Has(TDiscriminator discriminator);
	IPod2<TDiscriminator, object> this[TDiscriminator discriminator] { get; }
	void Add(IPod2<TDiscriminator, object> pod);
	bool Remove(TDiscriminator discriminator);
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public class Pod2<TDiscriminator, TPayload> : IPod2<TDiscriminator, TPayload, PodCollection2<TDiscriminator>>
	where TDiscriminator : notnull
{
	public Pod2(TDiscriminator discriminator, TPayload @object)
	{
		Discriminator = discriminator;
		Payload = @object;
	}
	public TDiscriminator Discriminator { get; }
	public TPayload Payload { get; }
	public PodCollection2<TDiscriminator> Headers { get; } = new();
}
public class PodCollection2<TDiscriminator> : IPodCollection2<TDiscriminator>
	where TDiscriminator : notnull
{
	readonly Dictionary<TDiscriminator, IPod2<TDiscriminator, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] => dic[discriminator];
	public void Add(IPod2<TDiscriminator, object> pod) => dic.Add(pod.Discriminator, pod);
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() => dic.Values.GetEnumerator();
}
