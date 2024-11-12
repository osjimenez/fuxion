using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Fuxion.Pods;

public interface IUriKeyPod<out TPayload> : ICollectionPod<UriKey, TPayload>
{
	IPod<UriKey, object> this[Type type] { get; }
	IUriKeyResolver? Resolver { get; set; }
	bool TryGetHeader<T>([MaybeNullWhen(false)] out T result, IUriKeyResolver? resolver = null);
}

public class UriKeyPod<TPayload>(UriKey discriminator, TPayload payload) : Pod<UriKey, TPayload>(discriminator, payload), IUriKeyPod<TPayload>
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected UriKeyPod() : this(default!, default!) { }
	public override IPod<UriKey, object> this[UriKey key] => TryGetHeaderPod(key, out var pod) ? pod : throw new UriKeyNotFoundException($"Key '{key}' not found in headers");
	public override void Add(IPod<UriKey, object> pod)
	{
		// Check if new pod is derived from an existing one
		var header = this.FirstOrDefault(h => h.Discriminator.Key.IsBaseOf(pod.Discriminator.Key) || pod.Discriminator.Bases.Any(b => h.Discriminator.Key.IsBaseOf(b)));
		if (header is not null) throw new UriKeyInheritanceException($"The header of type '{pod.Discriminator}' cannot be equals or based on an existing header '{header.Discriminator}'.");
		// Check if new pod is base of an existing one
		header = this.FirstOrDefault(h => pod.Discriminator.Key.IsBaseOf(h.Discriminator.Key) || h.Discriminator.Bases.Any(b => pod.Discriminator.Key.IsBaseOf(b)));
		if (header is not null) throw new UriKeyInheritanceException($"The header of type '{pod.Discriminator}' cannot be equals or base of an existing header '{header.Discriminator}'.");
		base.Add(pod);
	}
	public IPod<UriKey, object> this[Type type] => this[Resolver?[type] ?? type.GetUriKey()];
	[JsonIgnore]
	public IUriKeyResolver? Resolver { get; set; }
	public bool TryGetHeader<T>([MaybeNullWhen(false)] out T result, IUriKeyResolver? resolver = null)
	{
		var uk = (resolver, Resolver) switch
		{
			(not null, _) => resolver[typeof(T)],
			(null, not null) => Resolver[typeof(T)],
			_ => typeof(T).GetUriKey()
		};
		if (TryGetHeaderPod(uk, out var pod) && pod.Payload is T payload)
		{
			result = payload;
			return true;
		}
		result = default;
		return false;
	}
	bool TryGetHeaderPod(UriKey key, [MaybeNullWhen(false)] out IPod<UriKey, object> result)
	{
		// Search the key in the dictionary
		if (HeadersDictionary.TryGetValue(key, out var res))
		{
			result = res;
			return true;
		}
		// Search all keys which are based on given key
		var derivedKey = HeadersDictionary.Keys.Where(k => key.Key.IsBaseOf(k.Key))
			// Get the key with longest path
			.MaxBy(k => k.Key.AbsolutePath.Length);
		if (derivedKey is not null)
		{
			result = HeadersDictionary[derivedKey];
			return true;
		}
		// Search keys which chains are based on given key
		derivedKey ??= HeadersDictionary.FirstOrDefault(h => h.Key.Bases.Any(b => key.Key.IsBaseOf(b)))
			.Key;
		if (derivedKey is not null)
		{
			result = HeadersDictionary[derivedKey];
			return true;
		}
		result = null;
		return false;
		// return derivedKey is not null ? HeadersDictionary[derivedKey] : throw new UriKeyNotFoundException($"Key '{key}' not found in directory");
	}
}