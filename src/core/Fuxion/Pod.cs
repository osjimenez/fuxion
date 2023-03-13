namespace Fuxion;

/*
Casos de uso:
- Serialización
- Crear un Pod, luego pasarlo JsonPod, de ahí a BsonPod, ZipPod, EncryptedPod, etc.
- Recibir algo como IPod y determinar de que tipo de Pod se trata
- Recibir un Pod y poder leer cualquier Header del tipo que sea
- Herencia en los Payloads y como afecta eso a los discriminadores
*/
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
public class Pod<TDiscriminator> : IPod<TDiscriminator, object, object,PodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	public Pod(TDiscriminator discriminator, object @object)
	{
		Discriminator = discriminator;
		_object = @object;
	}
	object _object;
	public TDiscriminator Discriminator { get; }
	public object Outside() => _object;
	public object Inside() => _object;
	public PodCollection<TDiscriminator> Headers { get; } = new();
}
public class Pod<TDiscriminator, TOutside, TInside> : Pod<TDiscriminator>,IPod<TDiscriminator, TOutside, TInside,PodCollection<TDiscriminator>>
	where TDiscriminator : notnull
	where TInside : notnull
{
	public Pod(TDiscriminator discriminator, TInside @object) : base(discriminator, @object) { }
	public new TOutside Outside() => (TOutside)base.Outside();
	public new TInside Inside() => (TInside)base.Inside();
}