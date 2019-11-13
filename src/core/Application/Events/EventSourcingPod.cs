using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Domain.Events;
using Fuxion.Domain;
using Newtonsoft.Json;
using System;

namespace Fuxion.Application.Events
{
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
		[JsonProperty]
		public int TargetVersion { get; private set; }
		[JsonProperty]
		public Guid CorrelationId { get; private set; }
		[JsonProperty]
		public DateTime EventCommittedTimestamp { get; internal set; }
		[JsonProperty]
		public int ClassVersion { get; private set; }

		public T AsEvent<T>() where T : Event => base.As<T>().Transform(evt => evt.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
		public Event? AsEvent(Type type) => ((Event?)base.As(type)).Transform(evt => evt?.AddEventSourcing(TargetVersion, CorrelationId, EventCommittedTimestamp, ClassVersion));
		public Event? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsEvent(typeKeyDirectory[PayloadKey]);
	}
}
