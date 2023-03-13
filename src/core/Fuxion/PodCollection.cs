using System.Collections;

namespace Fuxion;

public interface IPodCollection<TDiscriminator, out TPod> : IEnumerable<TPod>
	where TPod : IPod<TDiscriminator, object, object>
{
	bool Has(TDiscriminator discriminator);
	TPod this[TDiscriminator discriminator] { get; }
	void Add(IPod<TDiscriminator, object, object> pod);
	bool Remove(TDiscriminator discriminator);
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class PodCollection<TDiscriminator>:IPodCollection<TDiscriminator,Pod<TDiscriminator>>
	where TDiscriminator : notnull
{
	readonly Dictionary<TDiscriminator, IPod<TDiscriminator, object, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	public Pod<TDiscriminator> this[TDiscriminator discriminator] => new(discriminator, dic[discriminator].Outside());
	public void Add(IPod<TDiscriminator, object, object> pod) => dic.Add(pod.Discriminator, pod);
	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	public IEnumerator<Pod<TDiscriminator>> GetEnumerator() => dic.Values.Select(_ => new Pod<TDiscriminator>(_.Discriminator, _.Outside())).GetEnumerator();
}