namespace Fuxion.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeKeyAttribute : Attribute
{
	public TypeKeyAttribute(string typeKey)
	{
		if (string.IsNullOrWhiteSpace(typeKey)) throw new ArgumentException($"The key for attribute '{nameof(TypeKeyAttribute)}' cannot be null, empty or white spaces string", nameof(typeKey));
		TypeKey = typeKey;
	}
	public string TypeKey { get; }
}