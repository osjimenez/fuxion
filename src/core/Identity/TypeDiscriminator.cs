using System.Diagnostics;

namespace Fuxion.Identity;

[Discriminator(TypeDiscriminatorId)]
[DebuggerDisplay("{" + nameof(Name) + "}")]
public sealed class TypeDiscriminator : IDiscriminator<string, string>
{
	internal TypeDiscriminator(string typeKey, string typeName)
	{
		TypeKey  = typeKey;
		TypeName = typeName;
	}
	internal const string                  TypeDiscriminatorId = "TYPE";
	public         List<TypeDiscriminator> Inclusions { get; internal set; } = new();
	public         List<TypeDiscriminator> Exclusions { get; internal set; } = new();
	public static  TypeDiscriminator       Empty      => new(TypeDiscriminatorId, TypeDiscriminatorId);
#nullable disable
	public string Id { get; internal set; }
#nullable enable
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object? IDiscriminator.Id => Id;
	public string? Name    { get; internal set; }
	public string  TypeKey { get; internal set; }
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object IDiscriminator.TypeKey => TypeKey;
	public string TypeName { get; internal set; }
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => Inclusions;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => Exclusions;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => Inclusions;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => Exclusions;
	public override string ToString()                                            => this.ToOneLineString();
	public override bool   Equals(object? obj)                                   => obj is TypeDiscriminator td ? td.Id == Id && td.TypeKey == TypeKey : false;
	public override int    GetHashCode()                                         => TypeKey.GetHashCode() ^ Id.GetHashCode();
	public static   bool operator ==(TypeDiscriminator? a, TypeDiscriminator? b) => EqualityComparer<TypeDiscriminator?>.Default.Equals(a, b);
	public static   bool operator !=(TypeDiscriminator? a, TypeDiscriminator? b) => !EqualityComparer<TypeDiscriminator?>.Default.Equals(a, b);
}