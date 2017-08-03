using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [TypeDiscriminated(false)]
    public abstract class SkillDao : BaseDao
    {
    }
    [TypeDiscriminated(false)]
    public class ActorSkillDao : SkillDao
    {

    }
    [TypeDiscriminated(false)]
    public class SingerSkillDao : SkillDao
    {

    }
    [TypeDiscriminated(false)]
    public class WriterSkillDao : SkillDao
    {

    }
    [TypeDiscriminated(false)]
    public class DirectorSkillDao : SkillDao
    {

    }
}
