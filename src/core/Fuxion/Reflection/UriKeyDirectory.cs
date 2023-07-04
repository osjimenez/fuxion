using System.Reflection;

namespace Fuxion.Reflection;

public interface IUriKeyResolver
{
	Type this[UriKey key] { get; }
	UriKey this[Type type] { get; }
	bool ContainsKey(UriKey key);
	bool ContainsKey(Type type);
}
public class UriKeyDirectory : IUriKeyResolver
{
	readonly Dictionary<UriKey, Type> _keyToTypeDictionary = new();
	readonly Dictionary<Type, UriKey> _typeToKeyDictionary = new(ReferenceEqualityComparer.Instance);
	public Type this[UriKey key]
	{
		get
		{
			if (_keyToTypeDictionary.TryGetValue(key, out var value)) return value;
			throw new TypeKeyNotFoundException($"Key '{key}' not found in '{nameof(UriKeyDirectory)}'");
		}
	}
	public UriKey this[Type type]
	{
		get
		{
			if (_typeToKeyDictionary.TryGetValue(type, out var value)) return value;
			throw new TypeKeyNotFoundException($"Type '{type}' not found in '{nameof(UriKeyDirectory)}'");
		}
	}
	public bool ContainsKey(UriKey key) => _keyToTypeDictionary.ContainsKey(key);
	public bool ContainsKey(Type type) => _typeToKeyDictionary.ContainsKey(type);
	public void RegisterAssemblyOf(Type type, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(type.Assembly, predicate);
	public void RegisterAssemblyOf<T>(Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(typeof(T).Assembly, predicate);
	public void RegisterAssembly(Assembly assembly, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null)
	{
		var query = assembly.GetTypes().Where(t => t.HasCustomAttribute<UriKeyAttribute>(false))
			.Select(t => (Type: t, Attribute: t.GetCustomAttribute<UriKeyAttribute>()));
		if (predicate != null) query = query.Where(predicate);
		foreach (var tup in query) Register(tup.Type);
	}
	public void Register<T>() => Register(typeof(T));
	public void Register(Type type)
	{
		var key = type.GetUriKey()
			?? throw new ArgumentException($"The type '{type.Name}' isn't decorated with '{nameof(UriKeyAttribute)}' attribute");
		Register(type, key);
	}
	public void Register<T>(UriKey key) => Register(typeof(T), key);
	public void Register(Type type, UriKey key)
	{
		_keyToTypeDictionary.Add(key, type);
		_typeToKeyDictionary.Add(type, key);
	}
}