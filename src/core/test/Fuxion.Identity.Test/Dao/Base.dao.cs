using System;
using System.Diagnostics;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dao
{
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	[TypeDiscriminated(Base, AdditionalInclusions = new[] { Media })]
	public abstract class BaseDao
	{
		public BaseDao(string id, string name)
		{
			Id = id;
			Name = name;
		}
		public string Id { get; set; } = string.Empty.RandomString(10);
		public virtual string Name { get; set; }

		public override string ToString() => $"{Name} - {Id}";
	}
}
