using System.Reflection;

namespace Fuxion.Reflection;

public class TypeKeyDirectory
{
	readonly Dictionary<TypeKey, Type> _keyToTypeDictionary = new();
	readonly Dictionary<Type, TypeKey> _typeToKeyDictionary = new();
	public Type this[TypeKey key]
	{
		get
		{
			if (!_keyToTypeDictionary.ContainsKey(key)) throw new TypeKeyNotFoundInDirectoryException($"Key '{key}' not found in '{nameof(TypeKeyDirectory)}'");
			return _keyToTypeDictionary[key];
		}
	}
	public TypeKey this[Type type]
	{
		get
		{
			if (!_typeToKeyDictionary.ContainsKey(type)) throw new TypeKeyNotFoundInDirectoryException($"Type '{type}' not found in '{nameof(TypeKeyDirectory)}'");
			return _typeToKeyDictionary[type];
		}
	}
	public bool ContainsKey(TypeKey key) => _keyToTypeDictionary.ContainsKey(key);
	public bool ContainsKey(Type type) => _typeToKeyDictionary.ContainsKey(type);
	public void RegisterAssemblyOf(Type type, Func<(Type Type, TypeKeyAttribute? Attribute), bool>? predicate = null, bool registerByFullNameIfNotFound = false) =>
		RegisterAssembly(type.Assembly, predicate, registerByFullNameIfNotFound);
	public void RegisterAssemblyOf<T>(Func<(Type Type, TypeKeyAttribute? Attribute), bool>? predicate = null, bool registerByFullNameIfNotFound = false) =>
		RegisterAssembly(typeof(T).Assembly, predicate, registerByFullNameIfNotFound);
	public void RegisterAssembly(Assembly assembly, Func<(Type Type, TypeKeyAttribute? Attribute), bool>? predicate = null, bool registerByFullNameIfNotFound = false)
	{
		var query = assembly.GetTypes().Where(t => t.HasCustomAttribute<TypeKeyAttribute>(false) || registerByFullNameIfNotFound)
			// NULLABLE - I check before that custom attribute exist
			.Select(t => (Type: t, Attribute: t.GetCustomAttribute<TypeKeyAttribute>()));
		if (predicate != null) query = query.Where(predicate);
		foreach (var tup in query) Register(tup.Type, registerByFullNameIfNotFound);
	}
	public void Register<T>(bool registerByFullNameIfNotFound = false) => Register(typeof(T), registerByFullNameIfNotFound);
	public void Register(Type type, bool registerByFullNameIfNotFound = false)
	{
		var key = type.GetTypeKey(processFullNameIfNotFound: registerByFullNameIfNotFound)
			?? throw new ArgumentException($"The type '{type.Name}' isn't decorated with '{nameof(TypeKeyAttribute)}' attribute");
		_keyToTypeDictionary.Add(key, type);
		_typeToKeyDictionary.Add(type, key);
	}
}