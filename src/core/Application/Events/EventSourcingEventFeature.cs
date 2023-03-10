using System.Text.Json.Serialization;
using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Events;

[TypeKey(nameof(Fuxion), nameof(Application), nameof(Events), nameof(EventSourcingEventFeature))]
public class EventSourcingEventFeature : IFeature<Event>
{
	[JsonInclude]
	public int TargetVersion { get; internal set; }
	public Guid CorrelationId { get; set; }
	public DateTime EventCommittedTimestamp { get; set; }
	public int ClassVersion { get; set; }
	[JsonInclude]
	public bool IsReplay { get; internal set; }
}