using System.Collections;
using System.Net.Sockets;

namespace Fuxion;

public interface IPod2<out TDiscriminator, out TPayload>
{
	TDiscriminator Discriminator { get; }
	TPayload Payload { get; }
	
}

public interface ICollectionPod2<TDiscriminator, out TPayload> : IPod2<TDiscriminator, TPayload>, IEnumerable<IPod2<TDiscriminator, object>>
{
	bool Has(TDiscriminator discriminator);
	IPod2<TDiscriminator, object> this[TDiscriminator discriminator] { get; }
	void Add(IPod2<TDiscriminator, object> pod);
	bool Remove(TDiscriminator discriminator);
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public class Pod2<TDiscriminator, TPayload>(TDiscriminator discriminator, TPayload payload) : ICollectionPod2<TDiscriminator, TPayload>
	where TDiscriminator : notnull
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected Pod2() : this(default!, default!) { }
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TDiscriminator Discriminator { get; init; } = discriminator;
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TPayload Payload { get; init; } = payload;
	
	readonly Dictionary<TDiscriminator, IPod2<TDiscriminator, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] => dic[discriminator];
	public void Add(IPod2<TDiscriminator, object> pod) => dic.Add(pod.Discriminator, pod);
	public void Add<THeaderPayload>(TDiscriminator discriminator, THeaderPayload payload) 
		where THeaderPayload : notnull
		=> dic.Add(discriminator, new Pod2<TDiscriminator, object>(discriminator, payload));
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() => dic.Values.GetEnumerator();
	public static implicit operator TPayload?(Pod2<TDiscriminator, TPayload> pod) => pod.Payload;
}
