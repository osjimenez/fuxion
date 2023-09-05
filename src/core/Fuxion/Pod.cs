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
	bool Has(TDiscriminator discriminator);
	void Add(IPod<TDiscriminator, object> pod);
	bool Remove(TDiscriminator discriminator);
}

public class Pod<TDiscriminator, TPayload>(TDiscriminator discriminator, TPayload payload) : ICollectionPod<TDiscriminator, TPayload>
	where TDiscriminator : notnull
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected Pod() : this(default!, default!) { }
	protected Dictionary<TDiscriminator, IPod<TDiscriminator, object>> HeadersDictionary { get; } = new();
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TDiscriminator Discriminator { get; init; } = discriminator;
	// ATTENTION: The private setter cannot be removed, it is needed for deserialization
	public TPayload Payload { get; init; } = payload;
	public bool Has(TDiscriminator discriminator) => HeadersDictionary.ContainsKey(discriminator);
	public virtual IPod<TDiscriminator, object> this[TDiscriminator discriminator] => HeadersDictionary[discriminator];
	public bool Remove(TDiscriminator discriminator) => HeadersDictionary.Remove(discriminator);
	public IEnumerator<IPod<TDiscriminator, object>> GetEnumerator() => HeadersDictionary.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public virtual void Add(IPod<TDiscriminator, object> pod) => HeadersDictionary.Add(pod.Discriminator, pod);
	public virtual void Add<THeaderPayload>(TDiscriminator discriminator, THeaderPayload payload)
		where THeaderPayload : notnull
		=> Add(new Pod<TDiscriminator, object>(discriminator, payload));
	public static implicit operator TPayload?(Pod<TDiscriminator, TPayload> pod) => pod.Payload;
}