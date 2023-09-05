namespace Fuxion;

public class SemanticVersionIdentifier : IComparable, IComparable<SemanticVersionIdentifier>, IEquatable<SemanticVersionIdentifier>
{
	public SemanticVersionIdentifier(string identifier)
	{
		Value = identifier;
		IsNumber = uint.TryParse(identifier, out var numberValue);
		if (IsNumber) NumberValue = numberValue;
	}
	public string Value { get; }
	public bool IsNumber { get; }
	public uint? NumberValue { get; }
	
	public override string ToString() => Value;
	
	public static bool operator ==(SemanticVersionIdentifier? identifier1, SemanticVersionIdentifier? identifier2)
	{
		if (identifier1 is null) return identifier2 is null;
		return identifier1.Equals(identifier2);
	}
	public static bool operator !=(SemanticVersionIdentifier identifier1, SemanticVersionIdentifier identifier2) => !(identifier1 == identifier2);
	public static bool operator < (SemanticVersionIdentifier identifier1, SemanticVersionIdentifier identifier2)
	{
#if NETSTANDARD2_0 || NET462
		if(identifier1 is null) throw new ArgumentException(nameof(identifier1));
#else
		ArgumentNullException.ThrowIfNull(identifier1);
#endif
		return identifier1.CompareTo(identifier2) < 0;
	}
	public static bool operator <=(SemanticVersionIdentifier identifier1, SemanticVersionIdentifier identifier2) => identifier1 == identifier2 || identifier1 < identifier2;
	public static bool operator > (SemanticVersionIdentifier identifier1, SemanticVersionIdentifier identifier2)
	{
#if NETSTANDARD2_0 || NET462
		if(identifier1 is null) throw new ArgumentException(nameof(identifier1));
#else
		ArgumentNullException.ThrowIfNull(identifier1);
#endif
		return identifier2 < identifier1;
	}
	public static bool operator >=(SemanticVersionIdentifier identifier1, SemanticVersionIdentifier identifier2) => identifier1 == identifier2 || identifier1 > identifier2;
	public int CompareTo(SemanticVersionIdentifier? other)
	{
		if (ReferenceEquals(this, other)) return 0;
		if (other is null) return 1;
		return (IsNumber, other.IsNumber) switch
		{
			(true, true) => Nullable.Compare(NumberValue, other.NumberValue),
			(true, false) => -1,
			(false, true) => 1,
			var _ => string.Compare(Value, other.Value, StringComparison.Ordinal)
		};
	}
	public int CompareTo(object? obj)
	{
		if (obj is null)
			return 1;
		var other = obj as SemanticVersionIdentifier
			?? throw new ArgumentException($"Type must be '{nameof(SemanticVersionIdentifier)}'", "obj");
		return CompareTo(other);
	}
	public bool Equals(SemanticVersionIdentifier? other) => other is not null && string.Equals(Value, other.Value);
	public override bool Equals(object? obj)
	{
		var identifier = obj as SemanticVersionIdentifier;
		return identifier is not null && Equals(identifier);
	}
	public override int GetHashCode() => Value.GetHashCode();
}