using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Domain.Events;
using Fuxion.Domain;
using Newtonsoft.Json;
using System;

namespace Fuxion.Application.Events
{
	public class PublicationPod : JsonPod<Event, string>
	{
		[JsonConstructor]
		protected PublicationPod() { }
		internal PublicationPod(Event @event) : base(@event, @event.GetType().GetTypeKey())
		{
			if (!@event.HasPublication()) throw new EventFeatureNotFoundException($"'{nameof(PublicationPod)}' require '{nameof(PublicationEventFeature)}'");
			var es = @event.Publication();
			Timestamp = es.Timestamp;
		}
		[JsonProperty]
		public DateTime Timestamp { get; internal set; }

		public T? AsEvent<T>() where T : Event => base.As<T>().Transform(evt => evt?.AddPublication(Timestamp));
		public Event? AsEvent(Type type) => ((Event?)base.As(type)).Transform(evt => evt?.AddPublication(Timestamp));
		public Event? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsEvent(typeKeyDirectory[PayloadKey]);
	}
}
