namespace Fuxion.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeKeyAttribute : Attribute
{
	public TypeKeyAttribute(params string[] keyChain)
	{
		KeyChain = keyChain;
	}
	public TypeKey KeyChain { get; }
}