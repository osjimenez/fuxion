using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks;

[DebuggerDisplay("{" + nameof(Name) + "}")]
class StringDiscriminator : IDiscriminator<string, string>
{
	public StringDiscriminator(string id, string typeId, IEnumerable<StringDiscriminator>? inclusions, IEnumerable<StringDiscriminator>? exclusions)
	{
		Id         = id;
		Inclusions = inclusions ?? new List<StringDiscriminator>();
		Exclusions = exclusions ?? new List<StringDiscriminator>();
		TypeKey    = typeId;
	}
	public IEnumerable<StringDiscriminator>                                                Inclusions { get; set; } = new List<StringDiscriminator>();
	public IEnumerable<StringDiscriminator>                                                Exclusions { get; set; } = new List<StringDiscriminator>();
	public string                                                                          Id         { get; }
	object? IDiscriminator.                                                                Id         => Id;
	public string?                                                                         Name       => Id;
	public string                                                                          TypeKey    { get; }
	object IDiscriminator.                                                                 TypeKey    => TypeKey;
	public string                                                                          TypeName   => TypeKey;
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.                                Inclusions => Inclusions;
	IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => Inclusions;
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.                                Exclusions => Exclusions;
	IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => Exclusions;
}