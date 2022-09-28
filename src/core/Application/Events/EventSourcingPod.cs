namespace Fuxion.Application.Events;

using Fuxion.Domain;
using Fuxion.Domain.Events;
using Fuxion.Json;
using Fuxion.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(EventSourcingPodConverter))]
public class EventSourcingPod : JsonPod<Event, string>
{
	[JsonConstructor]
	protected EventSourcingPod() { }
	internal EventSourcingPod(Event @event) : base(@event, @event.GetType().GetTypeKey())
	{
		if (!@event.HasEventSourcing()) throw new EventFeatureNotFoundException($"'{nameof(EventSourcingPod)}' require '{nameof(EventSourcingEventFeature)}'");
		var es = @event.EventSourcing();
		TargetVersion = es.TargetVersion;
		CorrelationId = es.CorrelationId;
		EventCommittedTimestamp = es.EventCommittedTimestamp;
		ClassVersion = es.ClassVersion;
	}
	[JsonInclude]
	public int TargetVersion { get; private set; }
	[JsonInclude]
	public Guid CorrelationId { get; private set; }
	[JsonInclude]
	public DateTime EventCommittedTimestamp { get; internal set; }
	[JsonInclude]
	public int ClassVersion { get; private set; }

	public T? AsEvent<T>() where T : Event => base.As<T>().Transform(evt => evt?.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
	public Event? AsEvent(Type type) => ((Event?)base.As(type)).Transform(evt => evt?.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
	public Event? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsEvent(typeKeyDirectory[PayloadKey]);
}
