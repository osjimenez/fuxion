using System.Collections;

namespace Fuxion;

// public interface IBasePod2<out TDiscriminator>
// {
// 	TDiscriminator Discriminator { get; }
// }
public interface IPod2<out TDiscriminator, out TPayload>
{
	TDiscriminator Discriminator { get; }
	TPayload? Payload { get; }
}
public interface IRawPod2<out TDiscriminator, out TPayload, out TRaw> : IPod2<TDiscriminator, TPayload>
{
	// TOutside Outside();
	// TInside Inside();
	TRaw? Raw { get; }
}
// public interface IPod2<out TDiscriminator, out TPayload> : ICrossPod2<TDiscriminator, TPayload, TPayload>
// {
// 	TPayload Payload { get; }
// 	TPayload ICrossPod2<TDiscriminator, TPayload, TPayload>.Outside() => Payload;
// 	TPayload ICrossPod2<TDiscriminator, TPayload, TPayload>.Inside() => Payload;
// }
public interface IPod2<out TDiscriminator, out TPayload, out TCollection> : IPod2<TDiscriminator, TPayload>
	where TCollection : IPodCollection2<TDiscriminator>
{
	TCollection Headers { get; }
}
public interface IRawPod2<out TDiscriminator, out TPayload, out TRaw, out TCollection> : IRawPod2<TDiscriminator, TPayload, TRaw>, IPod2<TDiscriminator, TPayload, TCollection>
	where TCollection : IPodCollection2<TDiscriminator>
{
}
public interface IPodCollection2<TDiscriminator> : IEnumerable<IPod2<TDiscriminator, object>>
	// where TPod : IPod2<TDiscriminator, object>
{
	bool Has(TDiscriminator discriminator);
	IPod2<TDiscriminator, object> this[TDiscriminator discriminator] { get; }
	void Add(IPod2<TDiscriminator, object> pod);
	bool Remove(TDiscriminator discriminator);
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
// public interface ICrossPodCollection2<in TDiscriminator, out TPod> : IEnumerable<TPod>
// 	where TPod : ICrossPod2<TDiscriminator, object, object>
// {
// 	bool Has(TDiscriminator discriminator);
// 	TPod this[TDiscriminator discriminator] { get; }
// 	void Add(ICrossPod2<TDiscriminator, object, object> pod);
// 	bool Remove(TDiscriminator discriminator);
// 	// IMPLEMENTED
// 	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
// }
