namespace Fuxion.Reflection;

public interface ITypeKeyResolver
{
	Type this[TypeKey key] { get; }
	TypeKey this[Type type] { get; }
	bool ContainsKey(TypeKey key);
	bool ContainsKey(Type type);
}