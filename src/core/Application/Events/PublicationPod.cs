using System.Text.Json.Serialization;
using Fuxion.Domain;
using Fuxion.Domain.Events;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Application.Events;

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
	[JsonInclude]
	public DateTime Timestamp { get; internal set; }
	public T?     AsEvent<T>() where T : Event                            => As<T>().Transform(evt => evt?.AddPublication(Timestamp));
	public Event? AsEvent(Type                          type)             => ((Event?)As(type)).Transform(evt => evt?.AddPublication(Timestamp));
	public Event? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsEvent(typeKeyDirectory[PayloadKey]);
}