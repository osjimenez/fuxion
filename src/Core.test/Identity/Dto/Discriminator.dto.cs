namespace Fuxion.Identity.Test.Dto;

public abstract class DiscriminatorDto : BaseDto, IDiscriminator<string, string>
{
	public DiscriminatorDto(string id, string name) : base(id, name) { }
	object? IDiscriminator.Id => Id;
	string? IDiscriminator.Name => Name;
	object IDiscriminator.TypeKey => GetTypeId();
	string IDiscriminator.TypeName => GetTypeName();
	IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => GetExclusions();
	IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => GetInclusions();
	string IDiscriminator<string, string>.TypeKey => GetTypeId();
	IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => GetInclusions();
	IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => GetExclusions();
	public override string ToString() => this.ToOneLineString();
	protected abstract string GetTypeId();
	protected abstract string GetTypeName();
	protected virtual IEnumerable<DiscriminatorDto> GetExclusions() => Enumerable.Empty<DiscriminatorDto>();
	protected virtual IEnumerable<DiscriminatorDto> GetInclusions() => Enumerable.Empty<DiscriminatorDto>();
}

[Discriminator("LOC")]
[TypeDiscriminated(TypeDiscriminatorIds.Location)]
public abstract class LocationDto : DiscriminatorDto
{
	public LocationDto(string id, string name) : base(id, name) { }
	protected sealed override string GetTypeId() => "LOC";
	protected sealed override string GetTypeName() => "Location";
	protected sealed override IEnumerable<DiscriminatorDto> GetExclusions() => GetLocationExclusions();
	protected abstract IEnumerable<LocationDto> GetLocationExclusions();
	protected sealed override IEnumerable<DiscriminatorDto> GetInclusions() => GetLocationInclusions();
	protected abstract IEnumerable<LocationDto> GetLocationInclusions();
}

[TypeDiscriminated(TypeDiscriminatorIds.State)]
public class StateDto : LocationDto
{
	public StateDto(string id, string name, CountryDto country) : base(id, name) => Country = country;
	public IList<CityDto> Cities { get; set; } = new List<CityDto>();
	public CountryDto Country { get; set; }
	protected override IEnumerable<LocationDto> GetLocationExclusions() =>
		new[] {
			Country
		};
	protected override IEnumerable<LocationDto> GetLocationInclusions() => Cities.Cast<LocationDto>().ToList();
}

[TypeDiscriminated(TypeDiscriminatorIds.Country)]
public class CountryDto : LocationDto
{
	public CountryDto(string id, string name) : base(id, name) { }
	public IList<StateDto> States { get; set; } = new List<StateDto>();
	protected override IEnumerable<LocationDto> GetLocationExclusions() =>
		new LocationDto[]
			{ };
	protected override IEnumerable<LocationDto> GetLocationInclusions() => States.Cast<LocationDto>().ToList();
}

[TypeDiscriminated(TypeDiscriminatorIds.City)]
public class CityDto : LocationDto
{
	public CityDto(string id, string name, StateDto state) : base(id, name) => State = state;
	public StateDto State { get; set; }
	protected override IEnumerable<LocationDto> GetLocationExclusions() =>
		new[] {
			State
		};
	protected override IEnumerable<LocationDto> GetLocationInclusions() =>
		new LocationDto[]
			{ };
}

[Discriminator("CAT")]
public class CategoryDto : DiscriminatorDto
{
	public CategoryDto(string id, string name) : base(id, name) { }
	public CategoryDto? Parent { get; set; }
	public IEnumerable<CategoryDto> Children { get; set; } = new List<CategoryDto>();
	protected sealed override string GetTypeId() => "CAT";
	protected sealed override string GetTypeName() => nameof(CategoryDto);
	protected override IEnumerable<DiscriminatorDto> GetInclusions() => Children;
	protected override IEnumerable<DiscriminatorDto> GetExclusions() =>
		new[] {
			Parent
		}.RemoveNulls();
}

public class TagDto : DiscriminatorDto
{
	public TagDto(string id, string name) : base(id, name) { }
	protected override string GetTypeId() => "TAG";
	protected override string GetTypeName() => nameof(TagDto);
}