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