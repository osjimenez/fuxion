namespace Fuxion.Identity;

using System.Reflection;

/// <summary>
/// Mark a property to indicate that is a key for the type of discriminator associated to it
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DiscriminatedByAttribute : Attribute
{
	public DiscriminatedByAttribute(Type type)
	{
		if (!typeof(IDiscriminator).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
			throw new Exception($"The Type '{type.Name}' must implements '{nameof(IDiscriminator)}' interface");
		Type = type;
	}
	public Type Type { get; set; }
}