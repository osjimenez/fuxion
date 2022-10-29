using Fuxion.Domain.Events;

namespace Fuxion.Application.Events;

public class PublicationEventFeature : IEventFeature
{
	public DateTime Timestamp { get; internal set; }
}