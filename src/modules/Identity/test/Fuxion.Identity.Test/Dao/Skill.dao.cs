using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public abstract class SkillDao : BaseDao
    {
    }
    //[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class ActorSkillDao : SkillDao
    {

    }
    //[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class SingerSkillDao : SkillDao
    {

    }
    //[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class WriterSkillDao : SkillDao
    {

    }
    //[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class DirectorSkillDao : SkillDao
    {

    }
}
