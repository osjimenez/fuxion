using System.Diagnostics.CodeAnalysis;

namespace Fuxion.Json;

public class ZipPod<TDiscriminator> : IPod<TDiscriminator, byte[], byte[]>//, ZipPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	public ZipPod(TDiscriminator discriminator, byte[] payload)
	{
		Discriminator = discriminator;
		_payload = payload;
		// PayloadValue = CreateValue(payload);
	}
	byte[] _payload;
	// public ZipPodCollection<TDiscriminator> Headers { get; } = new();
	public TDiscriminator Discriminator { get; }
	public byte[] Outside() => _payload;
	public byte[] Inside() => _payload;
	public T? As<T>() => throw new NotImplementedException();
	public bool TryAs<T>([NotNullWhen(true)] out T? payload) => throw new NotImplementedException();
	public bool Is<T>() => throw new NotImplementedException();
	public static implicit operator byte[](ZipPod<TDiscriminator> pod) => pod.Outside();
}
// public class ZipPodCollection<TDiscriminator>: IPodCollection<TDiscriminator, IPod<TDiscriminator,object, byte[], ZipPodCollection<TDiscriminator>>>
// 	where TDiscriminator : notnull
// {
// 	IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>> podCollectionImplementation;
// 	void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.AddByOutside<TPayload>(TDiscriminator discriminator, TPayload payload) => podCollectionImplementation.AddByOutside(discriminator, payload);
// 	void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.AddByOutside<TPayload>(IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>> pod) => podCollectionImplementation.AddByOutside<TPayload>(pod);
// 	void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.Replace<TPayload>(TDiscriminator discriminator, TPayload payload) => podCollectionImplementation.Replace(discriminator, payload);
// 	bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.Has(TDiscriminator discriminator) => podCollectionImplementation.Has(discriminator);
// 	IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>? IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.GetPod<TPayload>(TDiscriminator discriminator) => podCollectionImplementation.GetPod<TPayload>(discriminator);
// 	bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.TryGetPod<TPayload>(TDiscriminator discriminator, out IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>? pod) => podCollectionImplementation.TryGetPod<TPayload>(discriminator, out pod);
// 	TPayload? IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.GetPayload<TPayload>(TDiscriminator discriminator) where TPayload : default => podCollectionImplementation.GetPayload<TPayload>(discriminator);
// 	bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.TryGetPayload<TPayload>(TDiscriminator discriminator, out TPayload? payload) where TPayload : default => podCollectionImplementation.TryGetPayload(discriminator, out payload);
// 	IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>> IPodCollection<TDiscriminator, IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.this[TDiscriminator discriminator] => podCollectionImplementation[discriminator];
// 	IEnumerator<IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>> IEnumerable<IPod<TDiscriminator, object, byte[], ZipPodCollection<TDiscriminator>>>.GetEnumerator() => podCollectionImplementation.GetEnumerator();
// }