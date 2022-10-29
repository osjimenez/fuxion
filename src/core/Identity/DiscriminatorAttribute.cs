namespace Fuxion.Identity;

/// <summary>
///    Mark a class to be a discriminator for other classes
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatorAttribute : Attribute
{
	public DiscriminatorAttribute(object typeKey) => TypeKey = typeKey;
	public object TypeKey { get; set; }
}