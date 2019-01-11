using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dvo
{
    [TypeDiscriminated(TypeDiscriminatorIds.Person)]
    public class PersonDvo : BaseDvo<PersonDvo>
    {
        public IList<ISkillDvo> Skills { get; set; }
    }
    [TypeDiscriminated(TypeDiscriminatorIds.Person)]
    public class PersonSummaryDvo : BaseDvo<PersonSummaryDvo>
    {
        public IList<ISkillDvo> Skills { get; set; }
    }
}
