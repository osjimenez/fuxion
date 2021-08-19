namespace Fuxion.Application.Events;

using Fuxion.Domain.Events;

public class PublicationEventFeature : IEventFeature
{
	public DateTime Timestamp { get; internal set; }
}