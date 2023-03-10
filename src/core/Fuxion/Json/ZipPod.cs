namespace Fuxion.Json;

public class ZipPod<TDiscriminator> : IPod<TDiscriminator, byte[], ZipPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	public ZipPod(TDiscriminator discriminator, byte[] payload)
	{
		Discriminator = discriminator;
		Payload = payload;
		// PayloadValue = CreateValue(payload);
	}
	public ZipPodCollection<TDiscriminator> Headers { get; } = new();
	public TDiscriminator Discriminator { get; }
	public byte[]? Payload { get; }
	public T? As<T>() => throw new NotImplementedException();
	public bool TryAs<T>(out T? payload) => throw new NotImplementedException();
	public bool Is<T>() => throw new NotImplementedException();
}
public class ZipPodCollection<TDiscriminator>: IPodCollection<TDiscriminator, IPod<TDiscriminator,object, ZipPodCollection<TDiscriminator>>>
	where TDiscriminator : notnull
{

}