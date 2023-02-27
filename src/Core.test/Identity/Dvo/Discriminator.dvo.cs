namespace Fuxion.Identity.Test.Dvo;

public abstract class DiscriminatorDvo<TDiscriminator> : BaseDvo<TDiscriminator>, IDiscriminator<string, string> where TDiscriminator : DiscriminatorDvo<TDiscriminator>
{
	public DiscriminatorDvo(string id, string name) : base(id, name) { }
	object? IDiscriminator.Id => Id;
	string? IDiscriminator.Name => Name;
	object IDiscriminator.TypeKey => GetTypeId();
	string IDiscriminator<string, string>.TypeKey => GetTypeId();
	string IDiscriminator.TypeName => GetTypeName();
	IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => GetInclusions();
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => GetInclusions();
	IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => GetExclusions();
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => GetExclusions();
	public override string ToString() => this.ToOneLineString();
	protected abstract string GetTypeId();
	protected abstract string GetTypeName();
	protected virtual IEnumerable<IDiscriminator<string, string>> GetInclusions() => Enumerable.Empty<DiscriminatorDvo<TDiscriminator>>();
	protected virtual IEnumerable<IDiscriminator<string, string>> GetExclusions() => Enumerable.Empty<DiscriminatorDvo<TDiscriminator>>();
}

[Discriminator("LOC")]
[TypeDiscriminated(TypeDiscriminatorIds.Location)]
public abstract class LocationDvo<TLocation> : DiscriminatorDvo<TLocation> where TLocation : LocationDvo<TLocation>
{
	public LocationDvo(string id, string name) : base(id, name) { }
	protected sealed override string GetTypeId() => "LOC";
	protected sealed override string GetTypeName() => "Location";
}

[TypeDiscriminated(TypeDiscriminatorIds.Country)]
public class CountryDvo : LocationDvo<CountryDvo>
{
	public CountryDvo(string id, string name) : base(id, name) { }
	public IList<StateDvo> States { get; set; } = new List<StateDvo>();
	protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => States;
}

[TypeDiscriminated(TypeDiscriminatorIds.State)]
public class StateDvo : LocationDvo<StateDvo>
{
	public StateDvo(string id, string name, CountryDvo country) : base(id, name) => Country = country;
	public IList<CityDvo> Cities { get; set; } = new List<CityDvo>();
	public CountryDvo Country { get; set; }
	protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() =>
		new[] {
			Country
		};
	protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => Cities;
}

[TypeDiscriminated(TypeDiscriminatorIds.City)]
public class CityDvo : LocationDvo<CityDvo>
{
	public CityDvo(string id, string name, StateDvo state) : base(id, name) => State = state;
	public StateDvo State { get; set; }
	protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() =>
		new[] {
			State
		};
}

[Discriminator("CAT")]
public class CategoryDvo : DiscriminatorDvo<CategoryDvo>
{
	public CategoryDvo(string id, string name) : base(id, name) { }
	public CategoryDvo? Parent { get; set; }
	public IEnumerable<CategoryDvo> Children { get; set; } = new List<CategoryDvo>();
	protected sealed override string GetTypeId() => "CAT";
	protected sealed override string GetTypeName() => nameof(CategoryDvo);
	protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() =>
		new[] {
			Parent
		}.RemoveNulls();
	protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => Children;
}

public class TagDvo : DiscriminatorDvo<TagDvo>
{
	public TagDvo(string id, string name) : base(id, name) { }
	protected override string GetTypeId() => "TAG";
	protected override string GetTypeName() => nameof(TagDvo);
}