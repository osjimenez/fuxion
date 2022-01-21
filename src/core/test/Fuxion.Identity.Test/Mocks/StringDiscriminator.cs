namespace Fuxion.Identity.Test.Mocks;

using System.Diagnostics;

[DebuggerDisplay("{" + nameof(Name) + "}")]
internal class StringDiscriminator : IDiscriminator<string, string>
{
	public StringDiscriminator(string id, string typeId, IEnumerable<StringDiscriminator>? inclusions, IEnumerable<StringDiscriminator>? exclusions)
	{
		Id = id;
		Inclusions = inclusions ?? new List<StringDiscriminator>();
		Exclusions = exclusions ?? new List<StringDiscriminator>();
		TypeKey = typeId;
	}

	public string Id { get; private set; }
	object? IDiscriminator.Id => Id;

	public string? Name => Id;

	public string TypeKey { get; private set; }
	object IDiscriminator.TypeKey => TypeKey;

	public string TypeName => TypeKey;

	public IEnumerable<StringDiscriminator> Inclusions { get; set; } = new List<StringDiscriminator>();
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => Inclusions;
	IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => Inclusions;

	public IEnumerable<StringDiscriminator> Exclusions { get; set; } = new List<StringDiscriminator>();
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => Exclusions;
	IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => Exclusions;
}