using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dao
{
    [TypeDiscriminated(Person)]
    public class PersonDao : BaseDao
    {
        public IList<SkillDao> Skills { get; set; }

        CityDao _City;
        public CityDao City { get { return _City; } set { _City = value; CityId = value.Id; } }
        [DiscriminatedBy(typeof(CityDao))]
        public string CityId { get; set; }
    }
}
