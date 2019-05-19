using System.Collections.Generic;

namespace Fuxion.Identity.Test.Dvo
{
	[TypeDiscriminated(TypeDiscriminatorIds.Person)]
	public class PersonDvo : BaseDvo<PersonDvo>
	{
		public PersonDvo(string id, string name) : base(id, name) { }
		public IList<ISkillDvo> Skills { get; set; } = new List<ISkillDvo>();
	}
	[TypeDiscriminated(TypeDiscriminatorIds.Person)]
	public class PersonSummaryDvo : BaseDvo<PersonSummaryDvo>
	{
		public PersonSummaryDvo(string id, string name) : base(id, name) { }
		public IList<ISkillDvo> Skills { get; set; } = new List<ISkillDvo>();
	}
}
