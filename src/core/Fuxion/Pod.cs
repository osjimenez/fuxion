using System.Collections;

namespace Fuxion;

public interface IPod<out TDiscriminator, out TPayload>
{
	TDiscriminator Discriminator { get; }
	TPayload Payload { get; }
}

public interface ICollectionPod<TDiscriminator, out TPayload> : IPod<TDiscriminator, TPayload>, IEnumerable<IPod<TDiscriminator, object>>
{
	IPod<TDiscriminator, object> this[TDiscriminator discriminator] { get; }
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	bool Has(TDiscriminator discriminator);
	void Add(IPod<TDiscriminator, object> pod);
	bool Remove(TDiscriminator discriminator);
}

public class Pod<TDiscriminator, TPayload>(TDiscriminator discriminator, TPayload payload) : ICollectionPod<TDiscriminator, TPayload>
	where TDiscriminator : notnull
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected Pod() : this(default!, default!) { }
	readonly Dictionary<TDiscriminator, IPod<TDiscriminator, object>> dic = new();
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TDiscriminator Discriminator { get; init; } = discriminator;
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TPayload Payload { get; init; } = payload;
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	public IPod<TDiscriminator, object> this[TDiscriminator discriminator] => dic[discriminator];
	public void Add(IPod<TDiscriminator, object> pod) => dic.Add(pod.Discriminator, pod);
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	public IEnumerator<IPod<TDiscriminator, object>> GetEnumerator() => dic.Values.GetEnumerator();
	public void Add<THeaderPayload>(TDiscriminator discriminator, THeaderPayload payload)
		where THeaderPayload : notnull
		=> dic.Add(discriminator, new Pod<TDiscriminator, object>(discriminator, payload));
	public static implicit operator TPayload?(Pod<TDiscriminator, TPayload> pod) => pod.Payload;
}