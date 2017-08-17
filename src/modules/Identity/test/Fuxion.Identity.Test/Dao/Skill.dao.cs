using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [TypeDiscriminated(TypeDiscriminationMode.DisableBranch)]
    public abstract class SkillDao : BaseDao
    {
    }
    public class ActorSkillDao : SkillDao
    {

    }
    public class SingerSkillDao : SkillDao
    {

    }
    public class WriterSkillDao : SkillDao
    {

    }
    public class DirectorSkillDao : SkillDao
    {

    }
}
