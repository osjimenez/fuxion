using System.Reflection;

namespace Fuxion.Reflection;

public class TypeKeyDirectory
{
	readonly Dictionary<string, Type> dic = new();
	public Type this[string key]
	{
		get
		{
			if (!dic.ContainsKey(key)) throw new TypeKeyNotFoundInDirectoryException($"Key '{key}' not found in '{nameof(TypeKeyDirectory)}'");
			return dic[key];
		}
	}
	public bool ContainsKey(string key) => dic.ContainsKey(key);
	public void RegisterAssemblyOf(Type type, Func<(Type Type, TypeKeyAttribute? Attribute), bool>? predicate = null, bool registerByFullNameIfNotFound = false) => RegisterAssembly(type.Assembly, predicate, registerByFullNameIfNotFound);
	public void RegisterAssemblyOf<T>(Func<(Type Type, TypeKeyAttribute? Attribute), bool>? predicate = null, bool registerByFullNameIfNotFound = false) => RegisterAssembly(typeof(T).Assembly, predicate, registerByFullNameIfNotFound);
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
		var key = type.GetTypeKey(returnFullNameIfNotFound: registerByFullNameIfNotFound);
		if (key == null) throw new ArgumentException($"The type '{type.Name}' isn't decorated with '{nameof(TypeKeyAttribute)}' attribute");
		dic.Add(key, type);
	}
}