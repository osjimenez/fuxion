using System.Diagnostics;

namespace Fuxion.Identity.Test.Dvo;

using static Helpers.TypeDiscriminatorIds;

public interface IBaseDvo<TNotifier> where TNotifier : IBaseDvo<TNotifier> { }

[DebuggerDisplay("{" + nameof(Name) + "}")]
[TypeDiscriminated(Base, AdditionalInclusions = new[]
{
	Media
})]
public abstract class BaseDvo<TNotifier> : IBaseDvo<TNotifier> where TNotifier : BaseDvo<TNotifier>
{
	public BaseDvo(string id, string name)
	{
		Id   = id;
		Name = name;
	}
	public string Id   { get; set; }
	public string Name { get; set; }
}