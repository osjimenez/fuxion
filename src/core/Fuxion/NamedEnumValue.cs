using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Fuxion;

public struct NamedEnumValue(Enum value) : IEquatable<Enum>
{
	public string Name { get; } = value.GetType()
		.GetField(value.ToString())
		?.GetCustomAttribute<DisplayAttribute>(false)
		?.GetName() ?? value.ToString();
	public Enum Value { get; } = value;
	public override string ToString() => Name;
	public static implicit operator NamedEnumValue(Enum @enum) => new(@enum);
	public static implicit operator Enum(NamedEnumValue value) => value.Value;
	public static bool operator ==(Enum? @enum, NamedEnumValue value) => value.Equals(@enum);
	public static bool operator !=(Enum? @enum, NamedEnumValue value) => !value.Equals(@enum);
	public static bool operator ==(NamedEnumValue value, Enum? @enum) => value.Equals(@enum);
	public static bool operator !=(NamedEnumValue value, Enum? @enum) => !value.Equals(@enum);
	public override bool Equals(object? obj) => obj is Enum @enum ? Equals(@enum) : obj is NamedEnumValue value && Equals(value.Value);
	public override int GetHashCode() => Value.GetHashCode();
	public bool Equals(Enum? @enum) => @enum != null && @enum.Equals(Value) || @enum == null && Value == null;
}