using System.Text.Json.Serialization;
using Fuxion.Domain;
using Fuxion.Domain.Events;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Application.Events;

public class PublicationPod : JsonPod<TypeKey, Event>
{
	[JsonConstructor]
	protected PublicationPod() { }
	internal PublicationPod(Event @event) : base(@event.GetType().GetTypeKey(), @event)
	{
		if (!@event.HasPublication()) throw new EventFeatureNotFoundException($"'{nameof(PublicationPod)}' require '{nameof(PublicationEventFeature)}'");
		var es = @event.Publication();
		Timestamp = es.Timestamp;
	}
	[JsonInclude]
	public DateTime Timestamp { get; internal set; }
	public T? AsEvent<T>() where T : Event => As<T>().Transform(evt => evt?.AddPublication(Timestamp));
	public Event? AsEvent(Type type) => ((Event?)As(type)).Transform(evt => evt?.AddPublication(Timestamp));
	public Event? WithTypeKeyResolver(ITypeKeyResolver typeKeyResolver) => AsEvent(typeKeyResolver[Discriminator]);
}