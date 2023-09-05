namespace Fuxion;

public class SemanticVersionIdentifierCollection(SemanticVersionIdentifier[] identifiers) : IComparable, IComparable<SemanticVersionIdentifierCollection>, IEquatable<SemanticVersionIdentifierCollection>
{
	readonly SemanticVersionIdentifier[] identifiers = identifiers;
	public int Count => identifiers.Length;
	public override string ToString() => identifiers.Aggregate("", (a, c) => a + "." + c, a => a.Trim('.'));
	public static bool operator ==(SemanticVersionIdentifierCollection? collection1, SemanticVersionIdentifierCollection? collection2)
	{
		if (collection1 is null) return collection2 is null;
		return collection1.Equals(collection2);
	}
	public static bool operator !=(SemanticVersionIdentifierCollection collection1, SemanticVersionIdentifierCollection collection2) => !(collection1 == collection2);
	public static bool operator < (SemanticVersionIdentifierCollection collection1, SemanticVersionIdentifierCollection collection2)
	{
#if NETSTANDARD2_0 || NET462
		if (collection1 is null) throw new ArgumentException(nameof(collection1));
#else
		ArgumentNullException.ThrowIfNull(collection1);
#endif
		return collection1.CompareTo(collection2) < 0;
	}
	public static bool operator <=(SemanticVersionIdentifierCollection collection1, SemanticVersionIdentifierCollection collection2) => collection1 == collection2 || collection1 < collection2;
	public static bool operator > (SemanticVersionIdentifierCollection collection1, SemanticVersionIdentifierCollection collection2)
	{
#if NETSTANDARD2_0 || NET462
		if (collection1 is null) throw new ArgumentException(nameof(collection1));
#else
		ArgumentNullException.ThrowIfNull(collection1);
#endif
		return collection2 < collection1;
	}
	public static bool operator >=(SemanticVersionIdentifierCollection collection1, SemanticVersionIdentifierCollection collection2) => collection1 == collection2 || collection1 > collection2;
	public int CompareTo(SemanticVersionIdentifierCollection? other)
	{
		if (ReferenceEquals(this, other)) return 0;
		if (other is null) return -1;
		for (var i = 0; i < System.Math.Max(identifiers.Length, other.identifiers.Length); i++)
		{
			if (identifiers.Length < i + 1) return -1;
			if (other.identifiers.Length < i + 1) return 1;
			var res = identifiers[i]
				.CompareTo(other.identifiers[i]);
			if (res != 0) return res;
		}
		return 0;
	}
	public int CompareTo(object? obj)
	{
		if (obj is null)
			return 1;
		var other = obj as SemanticVersionIdentifierCollection
			?? throw new ArgumentException($"Type must be '{nameof(SemanticVersionIdentifierCollection)}'", "obj");
		return CompareTo(other);
	}
	public bool Equals(SemanticVersionIdentifierCollection? other) => other is not null && identifiers.SequenceEqual(other.identifiers);
	public override bool Equals(object? obj)
	{
		var collection = obj as SemanticVersionIdentifierCollection;
		return collection is not null && Equals(collection);
	}
	public override int GetHashCode() => identifiers.GetHashCode();
}