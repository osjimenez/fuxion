using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace Fuxion
{
	public struct NamedEnumValue : IEquatable<Enum>
	{
		public NamedEnumValue(Enum value)
		{
			Value = value;
			Name = value.GetType()
				.GetField(value.ToString())
				.GetCustomAttributes(false)
				.OfType<DisplayAttribute>()
				.FirstOrDefault()?.GetName() ?? value.ToString();
		}
		public string Name { get; }
		public Enum Value { get; }
		public override string ToString() => Name;
		public static implicit operator NamedEnumValue(Enum @enum) => new NamedEnumValue(@enum);
		public static implicit operator Enum(NamedEnumValue value) => value.Value;
		public static bool operator ==(Enum @enum, NamedEnumValue value) => @enum.Equals(value.Value);
		public static bool operator !=(Enum @enum, NamedEnumValue value) => !@enum.Equals(value.Value);
		public static bool operator ==(NamedEnumValue value, Enum @enum) => @enum.Equals(value.Value);
		public static bool operator !=(NamedEnumValue value, Enum @enum) => !@enum.Equals(value.Value);
		public override bool Equals(object obj)
		{
			if (obj is Enum @enum)
				return @enum.Equals(Value);
			if (obj is NamedEnumValue value)
				return value.Value.Equals(Value);
			return false;
		}
		public override int GetHashCode() => Value.GetHashCode();
		public bool Equals(Enum @enum) => @enum.Equals(Value);
	}
}
