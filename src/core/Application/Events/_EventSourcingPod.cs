// using System.Text.Json.Serialization;
// using Fuxion.Domain;
// using Fuxion.Domain.Events;
// using Fuxion.Json;
// using Fuxion.Reflection;
//
// namespace Fuxion.Application.Events;
//
// [JsonConverter(typeof(EventSourcingPodConverter))]
// public class EventSourcingPod : JsonPod<TypeKey, Event>
// {
// 	[JsonConstructor]
// 	protected EventSourcingPod() { }
// 	internal EventSourcingPod(Event @event) : base(@event.GetType().GetTypeKey(), @event)
// 	{
// 		if (!@event.HasEventSourcing()) throw new EventFeatureNotFoundException($"'{nameof(EventSourcingPod)}' require '{nameof(EventSourcingEventFeature)}'");
// 		var es = @event.EventSourcing();
// 		TargetVersion = es.TargetVersion;
// 		CorrelationId = es.CorrelationId;
// 		EventCommittedTimestamp = es.EventCommittedTimestamp;
// 		ClassVersion = es.ClassVersion;
// 	}
// 	[JsonInclude]
// 	public int TargetVersion { get; private set; }
// 	[JsonInclude]
// 	public Guid CorrelationId { get; private set; }
// 	[JsonInclude]
// 	public DateTime EventCommittedTimestamp { get; internal set; }
// 	[JsonInclude]
// 	public int ClassVersion { get; private set; }
// 	public T? AsEvent<T>() where T : Event => As<T>().Transform(evt => evt?.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
// 	public Event? AsEvent(Type type) => ((Event?)As(type)).Transform(evt => evt?.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
// 	public Event? WithTypeKeyResolver(ITypeKeyResolver typeKeyResolver) => AsEvent(typeKeyResolver[Discriminator]);
// }