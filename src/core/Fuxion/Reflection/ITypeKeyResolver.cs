namespace Fuxion.Reflection;

public interface ITypeKeyResolver
{
	Type this[TypeKey key] { get; }
	bool ContainsKey(TypeKey key);
}