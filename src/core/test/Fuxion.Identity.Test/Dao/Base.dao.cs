using System.Diagnostics;

namespace Fuxion.Identity.Test.Dao;

using static Helpers.TypeDiscriminatorIds;

[DebuggerDisplay("{" + nameof(Name) + "}")]
[TypeDiscriminated(Base, AdditionalInclusions = new[]
{
	Media
})]
public abstract class BaseDao
{
	public BaseDao(string id, string name)
	{
		Id   = id;
		Name = name;
	}
	public          string Id         { get; set; } = string.Empty.RandomString(10);
	public virtual  string Name       { get; set; }
	public override string ToString() => $"{Name} - {Id}";
}