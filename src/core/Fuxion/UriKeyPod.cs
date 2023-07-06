using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Fuxion;

public interface IUriKeyPod<out TPayload> : ICollectionPod<UriKey, TPayload>
{
	IPod<UriKey, object> this[Type type] { get; }
	IUriKeyResolver? Resolver { get; set; }
	bool TryGetHeader<T>([MaybeNullWhen(returnValue: false)] out T result, IUriKeyResolver? resolver = null);
}

public class UriKeyPod<TPayload>(UriKey discriminator, TPayload payload) : Pod<UriKey, TPayload>(discriminator, payload), IUriKeyPod<TPayload>
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected UriKeyPod() : this(default!, default!) { }
	public override IPod<UriKey, object> this[UriKey key]
	{
		get
		{
			// Search the key in the dictionary
			if (HeadersDictionary.TryGetValue(key, out var res)) return res;
			// Search all keys which are based on given key
			var derivedKey = HeadersDictionary.Keys.Where(k => key.Uri.IsBaseOf(k.Uri))
				// Get the key with longest path
				.MaxBy(k => k.Uri.AbsolutePath.Length);
			if (derivedKey is not null) return HeadersDictionary[derivedKey];
			// Search keys which chains are based on given key
			derivedKey ??= HeadersDictionary.FirstOrDefault(h => h.Key.Bases.Any(b => key.Uri.IsBaseOf(b))).Key;
			return derivedKey is not null ? HeadersDictionary[derivedKey] : throw new UriKeyNotFoundException($"Key '{key}' not found in directory");
		}
	}
	public override void Add(IPod<UriKey, object> pod)
	{
		// Check if new pod is derived from an existing one
		var header = this.FirstOrDefault(h => h.Discriminator.Uri.IsBaseOf(pod.Discriminator.Uri) || 
			pod.Discriminator.Bases.Any(b => h.Discriminator.Uri.IsBaseOf(b)));
		if (header is not null)
		{
			throw new UriKeyInheritanceException($"The header of type '{pod.Discriminator}' cannot be equals or based on an existing header '{header.Discriminator}'.");
		}
		// Check if new pod is base of an existing one
		header = this.FirstOrDefault(h => pod.Discriminator.Uri.IsBaseOf(h.Discriminator.Uri) || 
			h.Discriminator.Bases.Any(b => pod.Discriminator.Uri.IsBaseOf(b)));
		if (header is not null)
		{
			throw new UriKeyInheritanceException($"The header of type '{pod.Discriminator}' cannot be equals or base of an existing header '{header.Discriminator}'.");
		}
		base.Add(pod);
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