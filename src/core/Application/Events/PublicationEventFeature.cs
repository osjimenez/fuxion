using Fuxion.Domain;

namespace Fuxion.Application.Events;

public class PublicationEventFeature : IFeature<Event>
{
	public DateTime Timestamp { get; internal set; }
}