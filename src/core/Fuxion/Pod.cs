namespace Fuxion;

public interface IPod<out TDiscriminator, out TOutside, out TInside>
{
	TDiscriminator Discriminator { get; }
	TOutside Outside();
	TInside Inside();
}
public interface IPod<out TDiscriminator, out TOutside, out TInside, out TCollection> : IPod<TDiscriminator, TOutside, TInside>
	where TCollection : IPodCollection<TDiscriminator, IPod<TDiscriminator, object, object, TCollection>>
{
	TCollection Headers { get; }
}