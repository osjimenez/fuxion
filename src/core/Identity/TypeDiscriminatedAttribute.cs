namespace Fuxion.Identity;

/// <summary>
///    Mark a class to define how will be discrimined by its type
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeDiscriminatedAttribute : Attribute
{
	public TypeDiscriminatedAttribute(string id)
	{
		Id = id;
		Name = id;
	}
	public TypeDiscriminatedAttribute(TypeDiscriminationDisableMode disableMode) => DisableMode = disableMode;
	public string? Id { get; set; }
	public string? Name { get; set; }
	public TypeDiscriminationDisableMode? DisableMode { get; }
	public string[]? ExplicitInclusions { get; set; }
	public string[]? ExplicitExclusions { get; set; }
	public string[]? AdditionalInclusions { get; set; }
	public string[]? AdditionalExclusions { get; set; }
	public string[]? AvoidedInclusions { get; set; }
	public string[]? AvoidedExclusions { get; set; }
}

public enum TypeDiscriminationDisableMode
{
	DisableType,
	DisableHierarchy
}