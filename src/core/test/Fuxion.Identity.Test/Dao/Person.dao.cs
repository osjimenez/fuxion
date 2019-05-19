using System.Collections.Generic;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dao
{
	[TypeDiscriminated(Person)]
	public class PersonDao : BaseDao
	{
		public PersonDao(string id, string name) : base(id, name)
		{
		}

		public IList<SkillDao> Skills { get; set; } = new List<SkillDao>();

		private CityDao? _City;
		public CityDao? City { get => _City; set { _City = value; CityId = value?.Id; } }
		[DiscriminatedBy(typeof(CityDao))]
		public string? CityId { get; set; }
	}
}
