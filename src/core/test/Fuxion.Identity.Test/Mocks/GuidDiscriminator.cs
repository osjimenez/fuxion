using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks;

[DebuggerDisplay("{" + nameof(Name) + "}")]
class GuidDiscriminator : IDiscriminator<Guid, Guid>
{
	public GuidDiscriminator(Guid id, string name, Guid typeId, string typeName)
	{
		Id       = id;
		Name     = name;
		TypeKey  = typeId;
		TypeName = typeName;
	}
	public IEnumerable<GuidDiscriminator>                                          Inclusions { get; set; } = new List<GuidDiscriminator>();
	public IEnumerable<GuidDiscriminator>                                          Exclusions { get; set; } = new List<GuidDiscriminator>();
	public Guid                                                                    Id         { get; }
	object? IDiscriminator.                                                        Id         => Id;
	public string?                                                                 Name       { get; private set; }
	public Guid                                                                    TypeKey    { get; }
	object IDiscriminator.                                                         TypeKey    => TypeKey;
	public string                                                                  TypeName   { get; }
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.                        Inclusions => Inclusions;
	IEnumerable<IDiscriminator<Guid, Guid>> IInclusive<IDiscriminator<Guid, Guid>>.Inclusions => Inclusions;
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.                        Exclusions => Exclusions;
	IEnumerable<IDiscriminator<Guid, Guid>> IExclusive<IDiscriminator<Guid, Guid>>.Exclusions => Exclusions;
}