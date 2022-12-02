using System.ComponentModel.DataAnnotations.Schema;

namespace Fuxion.Identity.Test.Dao;

[Table(nameof(DiscriminatorDao))]
public abstract class DiscriminatorDao : BaseDao, IDiscriminator<string, string>
{
	public DiscriminatorDao(string id, string name) : base(id, name) { }
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
	protected virtual IEnumerable<DiscriminatorDao> GetExclusions() => Enumerable.Empty<DiscriminatorDao>();
	protected virtual IEnumerable<DiscriminatorDao> GetInclusions() => Enumerable.Empty<DiscriminatorDao>();
}

[Discriminator("LOC")]
[TypeDiscriminated(TypeDiscriminatorIds.Location)]
public abstract class LocationDao : DiscriminatorDao
{
	public LocationDao(string id, string name) : base(id, name) { }
	protected sealed override string GetTypeId() => "LOC";
	protected sealed override string GetTypeName() => nameof(LocationDao);
	protected sealed override IEnumerable<DiscriminatorDao> GetExclusions() => GetLocationExclusions();
	protected abstract IEnumerable<LocationDao> GetLocationExclusions();
	protected sealed override IEnumerable<DiscriminatorDao> GetInclusions() => GetLocationInclusions();
	protected abstract IEnumerable<LocationDao> GetLocationInclusions();
}

[Table(nameof(CountryDao))]
[TypeDiscriminated(TypeDiscriminatorIds.Country)]
public class CountryDao : LocationDao
{
	public CountryDao(string id, string name) : base(id, name) { }
	[DiscriminatedBy(typeof(CountryDao))]
	public new string Id
	{
		get => base.Id;
		set => base.Id = value;
	}
	public IList<StateDao> States { get; set; } = new List<StateDao>();
	protected override IEnumerable<LocationDao> GetLocationExclusions() =>
		new LocationDao[]
			{ };
	protected override IEnumerable<LocationDao> GetLocationInclusions() => States.Cast<LocationDao>().ToList();
}

[Table(nameof(StateDao))]
[TypeDiscriminated(TypeDiscriminatorIds.State)]
public class StateDao : LocationDao
{
	public StateDao(string id, string name, CountryDao country) : base(id, name)
	{
		_Country = country;
		CountryId = country.Id;
	}
	CountryDao _Country;
	[DiscriminatedBy(typeof(StateDao))]
	public new string Id
	{
		get => base.Id;
		set => base.Id = value;
	}
	public IList<CityDao> Cities { get; set; } = new List<CityDao>();
	public CountryDao Country
	{
		get => _Country;
		set
		{
			_Country = value;
			CountryId = value.Id;
		}
	}
	[DiscriminatedBy(typeof(CountryDao))]
	public string CountryId { get; set; }
	protected override IEnumerable<LocationDao> GetLocationExclusions() =>
		new[]
		{
			Country
		}.RemoveNulls();
	protected override IEnumerable<LocationDao> GetLocationInclusions() => Cities.Cast<LocationDao>().ToList();
}

[Table(nameof(CityDao))]
[TypeDiscriminated(TypeDiscriminatorIds.City)]
public class CityDao : LocationDao
{
	public CityDao(string id, string name, StateDao state) : base(id, name)
	{
		_State = state;
		StateId = state.Id;
	}
	StateDao _State;
	[DiscriminatedBy(typeof(CityDao))]
	public new string Id
	{
		get => base.Id;
		set => base.Id = value;
	}
	public StateDao State
	{
		get => _State;
		set
		{
			_State = value;
			StateId = value.Id;
		}
	}
	[DiscriminatedBy(typeof(StateDao))]
	public string StateId { get; set; }
	protected override IEnumerable<LocationDao> GetLocationExclusions() =>
		new[]
		{
			State
		}.RemoveNulls();
	protected override IEnumerable<LocationDao> GetLocationInclusions() =>
		new LocationDao[]
			{ };
}

[Discriminator("CAT")]
[Table(nameof(CategoryDao))]
[TypeDiscriminated(Helpers.TypeDiscriminatorIds.Category)]
public class CategoryDao : DiscriminatorDao
{
	public CategoryDao(string id, string name) : base(id, name) { }
	[DiscriminatedBy(typeof(CategoryDao))]
	public new string Id
	{
		get => base.Id;
		set => base.Id = value;
	}
	public CategoryDao? Parent { get; set; }
	//[DiscriminatedBy(typeof(CategoryDao))]
	public string? ParentId { get; set; }
	public IEnumerable<CategoryDao> Children { get; set; } = new List<CategoryDao>();
	protected sealed override string GetTypeId() => "CAT";
	protected sealed override string GetTypeName() => nameof(CategoryDao);
	protected override IEnumerable<DiscriminatorDao> GetInclusions() => Children;
	protected override IEnumerable<DiscriminatorDao> GetExclusions() =>
		new[]
		{
			Parent
		}.RemoveNulls();
}

[Table("TAG")]
public class TagDao : DiscriminatorDao
{
	public TagDao(string id, string name) : base(id, name) { }
	protected override string GetTypeId() => "TAG";
	protected override string GetTypeName() => nameof(TagDao);
}