using System.Text.Json.Serialization;

namespace Fuxion.Reflection; 

[JsonConverter(typeof(TypeKeyJsonConverter))]
public class TypeKey
{
	public string[] KeyChain { get; }
	public TypeKey(params string[] keyChain)
	{
		KeyChain = keyChain;
		if (keyChain.Length == 0) throw new TypeKeyException($"At least one key must be specified in the key chain");
		if (keyChain.Any(string.IsNullOrWhiteSpace)) throw new TypeKeyException($"Neither key in the key chain can be null or empty string");
		if (keyChain.Skip(1).Any(_ => _.Contains('/') || _.Contains('\\')))
			throw new TypeKeyException($"Neither key in the key chain (excepts the first) can contains slash'/' or back slash '\\'. The current key chain is '{ToString()}'");
		if(!keyChain.First().StartsWith("http://") && !keyChain.First().StartsWith("https://"))
			if(keyChain.First().Contains('/') || keyChain.First().Contains('\\'))
				throw new TypeKeyException($"The first element of the keyChain only can contains slash '/' if start with 'http://' or 'https://'");
	}
	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach(var key in KeyChain)
			hash.Add(key.GetHashCode());
		return hash.ToHashCode();
	}
	public override bool Equals(object? obj) => obj is TypeKey tk && tk.GetHashCode() == GetHashCode();
	public override string ToString() => KeyChain.Aggregate((c, a) => $"{c}/{a}".Trim('/'));
	public static implicit operator TypeKey(string key) => new(key);
	public static implicit operator TypeKey(string[] keyChain) => new(keyChain);
	public static implicit operator string(TypeKey key) => key.ToString();
}