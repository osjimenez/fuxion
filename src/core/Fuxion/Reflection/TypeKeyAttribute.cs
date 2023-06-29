namespace Fuxion.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeKeyAttribute(params string[] keyChain) : Attribute
{
	public TypeKey KeyChain { get; } = keyChain;
}