using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks;

[DebuggerDisplay("{" + nameof(Name) + "}")]
class GuidStringDiscriminator : IDiscriminator<Guid, string>
{
	public GuidStringDiscriminator(Guid id, string name, string typeId, IEnumerable<GuidStringDiscriminator>? inclusions, IEnumerable<GuidStringDiscriminator>? exclusions)
	{
		Id         = id;
		Name       = name;
		Inclusions = inclusions ?? new List<GuidStringDiscriminator>();
		Exclusions = exclusions ?? new List<GuidStringDiscriminator>();
		TypeKey    = typeId;
	}
	public IEnumerable<GuidStringDiscriminator>                                        Inclusions { get; set; } = new List<GuidStringDiscriminator>();
	public IEnumerable<GuidStringDiscriminator>                                        Exclusions { get; set; } = new List<GuidStringDiscriminator>();
	public Guid                                                                        Id         { get; }
	object? IDiscriminator.                                                            Id         => Id;
	public string?                                                                     Name       { get; private set; }
	public string                                                                      TypeKey    { get; }
	object IDiscriminator.                                                             TypeKey    => TypeKey;
	public string                                                                      TypeName   => TypeKey;
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.                            Inclusions => Inclusions;
	IEnumerable<IDiscriminator<Guid, string>> IInclusive<IDiscriminator<Guid, string>>.Inclusions => Inclusions;
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.                            Exclusions => Exclusions;
	IEnumerable<IDiscriminator<Guid, string>> IExclusive<IDiscriminator<Guid, string>>.Exclusions => Exclusions;
}