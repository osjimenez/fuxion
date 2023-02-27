namespace Fuxion.Identity.Test.Dao;

[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
public abstract class SkillDao : BaseDao
{
	public SkillDao(string id, string name) : base(id, name) { }
}

//[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
public class ActorSkillDao : SkillDao
{
	public ActorSkillDao(string id, string name) : base(id, name) { }
}

//[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
public class SingerSkillDao : SkillDao
{
	public SingerSkillDao(string id, string name) : base(id, name) { }
}

//[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
public class WriterSkillDao : SkillDao
{
	public WriterSkillDao(string id, string name) : base(id, name) { }
}

//[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
public class DirectorSkillDao : SkillDao
{
	public DirectorSkillDao(string id, string name) : base(id, name) { }
}