using System.Collections;

namespace Fuxion;

public interface ICrossPod<out TDiscriminator, out TOutside, out TInside>
{
	TDiscriminator Discriminator { get; }
	TOutside Outside();
	TInside Inside();
}
public interface ICrossPod<out TDiscriminator, out TOutside, out TInside, out TCollection> : ICrossPod<TDiscriminator, TOutside, TInside>
	where TCollection : IPodCollection<TDiscriminator, ICrossPod<TDiscriminator, object, object, TCollection>>
{
	TCollection Headers { get; }
}
public interface IPodCollection<TDiscriminator, out TPod> : IEnumerable<TPod>
	where TPod : ICrossPod<TDiscriminator, object, object>
{
	bool Has(TDiscriminator discriminator);
	TPod this[TDiscriminator discriminator] { get; }
	void Add(ICrossPod<TDiscriminator, object, object> pod);
	bool Remove(TDiscriminator discriminator);
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}