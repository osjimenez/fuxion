using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    public class PersonDao : BaseDao
    {
        public IList<SkillDao> Skills { get; set; }
    }
}
