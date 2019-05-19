using Fuxion.ComponentModel;
using System.Diagnostics;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dvo
{
	public interface IBaseDvo<TNotifier> : INotifier<TNotifier>
		where TNotifier : IBaseDvo<TNotifier>
	{ }
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	[TypeDiscriminated(Base, AdditionalInclusions = new[] { Media })]
	public abstract class BaseDvo<TNotifier> : Notifier<TNotifier>, IBaseDvo<TNotifier>
		where TNotifier : BaseDvo<TNotifier>
	{
		public BaseDvo(string id, string name)
		{
			Id = id;
			Name = name;
		}
		public string Id { get; set; }
		public string Name { get; set; }
	}
}