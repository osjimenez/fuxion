using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Fuxion.Reflection;

public interface IUriKeyPod<out TPayload> : ICollectionPod<UriKey, TPayload>
{
	
}

public class UriKeyPod<TPayload>(UriKey discriminator, TPayload payload) : Pod<UriKey, TPayload>(discriminator, payload), IUriKeyPod<TPayload>
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected UriKeyPod():this(default!,default!){}
	public override IPod<UriKey, object> this[UriKey key]
	{
		get
		{
			if (HeadersDictionary.TryGetValue(key, out var res)) return res;
			// TODO Usar un metodo mas exacto/complejo, el primero que pillo no vale, hay que ordenarlos y devolver es mas derivado posible
			var derivedKey = HeadersDictionary.Keys.FirstOrDefault(k => key.Uri.IsBaseOf(k.Uri));
			if (derivedKey is not null) return HeadersDictionary[derivedKey];
			return HeadersDictionary[key];
		}
	}
	public IPod<UriKey, object> this[Type type] => this[Resolver?[type] ?? type.GetUriKey()];
	[JsonIgnore]
	public IUriKeyResolver? Resolver { get; set; } 
	public bool TryGetHeader<T>([MaybeNullWhen(returnValue: false)]out T result, IUriKeyResolver? resolver = null)
	{
		var uk = Resolver is null 
			? resolver is null 
				? typeof(T).GetUriKey()
				: resolver[typeof(T)]
			: Resolver[typeof(T)];
		
		if (this[uk] is { Payload: T payload })
		{
			result = payload;
			return true;
		}
		result = default;
		return false;
	}
}