namespace Fuxion.Identity.Test.Dao;

using static Helpers.TypeDiscriminatorIds;

[TypeDiscriminated(Person)]
public class PersonDao : BaseDao
{
	public PersonDao(string id, string name) : base(id, name) { }
	CityDao? _City;
	public IList<SkillDao> Skills { get; set; } = new List<SkillDao>();
	public CityDao? City
	{
		get => _City;
		set
		{
			_City = value;
			CityId = value?.Id;
		}
	}
	[DiscriminatedBy(typeof(CityDao))]
	public string? CityId { get; set; }
}